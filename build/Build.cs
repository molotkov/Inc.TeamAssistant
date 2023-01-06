using System;
using System.Collections.Generic;
using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.Coverlet;
using Nuke.Common.Tools.Docker;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using Renci.SshNet;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.Docker.DockerTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[ShutdownDotNetAfterServerBuild]
public sealed class Build : NukeBuild
{
    [Parameter("Docker hub username", Name = "hubusername")]
    private readonly string HubUsername;

    [Parameter("Docker hub password", Name = "hubpassword")]
    private readonly string HubPassword;

    [Parameter("Server name", Name = "servername")]
    private readonly string ServerName;

    [Parameter("Server username", Name = "serverusername")]
    private readonly string ServerUsername;

    [Parameter("Server password", Name = "serverpassword")]
    private readonly string ServerPassword;


    public static int Main () => Execute<Build>(x => x.Compile);

    private AbsolutePath SourceDirectory => RootDirectory / "src";
    private AbsolutePath OutputDirectory => RootDirectory / "output";
    private AbsolutePath TestsDirectory => RootDirectory / "tests";
    private AbsolutePath TestReportsDirectory => OutputDirectory / "test-reports";

    private readonly string _migrationRunnerProject = "Inc.TeamAssistant.Appraiser.MigrationsRunner";
    private readonly IEnumerable<string> _appProjects = new[] { "Inc.TeamAssistant.Appraiser.Backend" };

    private IEnumerable<string> ProjectsForPublish => _appProjects.Concat(_migrationRunnerProject);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    private readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Solution]
    private readonly Solution Solution;

    [GitVersion(Framework = "net6.0")]
    private readonly GitVersion GitVersion;

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            TestsDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(OutputDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .EnableNoRestore());
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetConfiguration(Configuration)
                .ResetVerbosity()
                .SetResultsDirectory(TestReportsDirectory)
                .When(IsServerBuild, c => c.EnableUseSourceLink())
                .EnableNoRestore()
                .EnableNoBuild()
                .CombineWith(Solution.GetProjects("*Tests"), (_, p) => _
                    .SetProjectFile(p)
                    .SetLoggers($"junit;LogFileName={p.Name}-results.xml;MethodFormat=Class;FailureBodyFormat=Verbose")));
        });

    Target Publish => _ => _
        .DependsOn(Test)
        .Executes(() =>
        {
            DotNetPublish(s => s
                .SetConfiguration(Configuration)
                .EnableNoBuild()
                .EnableNoRestore()
                .CombineWith(ProjectsForPublish, (ss, p) => ss
                    .SetProject(Solution.GetProject(p))
                    .SetOutput(OutputDirectory / p)));
        });

    Target BuildImages => _ => _
        .DependsOn(Publish)
        .Executes(() =>
        {
            DockerBuild(x => x
                .DisableProcessLogOutput()
                .SetProcessWorkingDirectory(RootDirectory)
                .SetPath(".")
                .SetFile("cicd/dockerfile.app_component")
                .SetTag(GetImageName("inc.teamassistant.appraiser")));

            DockerBuild(x => x
                .DisableProcessLogOutput()
                .SetProcessWorkingDirectory(RootDirectory)
                .SetPath(".")
                .SetFile("cicd/dockerfile.migrations_runner")
                .SetTag(GetImageName("inc.teamassistant.appraiser.migrationsrunner")));
        });

    Target PushImages => _ => _
        .DependsOn(BuildImages)
        .Executes(() =>
        {
            DockerLogin(s => s
                .SetUsername(HubUsername)
                .SetPassword(HubPassword)
                .DisableProcessLogOutput());

            DockerPush(s => s
                .SetName(GetImageName("inc.teamassistant.appraiser"))
                .DisableProcessLogOutput());
            DockerPush(s => s
                .SetName(GetImageName("inc.teamassistant.appraiser.migrationsrunner"))
                .DisableProcessLogOutput());
        });

    Target Deploy => _ => _
        .DependsOn(PushImages)
        .Executes(() =>
        {
            var appDirectory = "/home/inc_teamassistant_appraiser/prod";
            var appraiserImage = GetImageName("inc.teamassistant.appraiser");
            var migrationsRunnerImage = GetImageName("inc.teamassistant.appraiser.migrationsrunner");

            using var client = new SshClient(ServerName, ServerUsername, ServerPassword);

            client.Connect();

            client.RunCommand($"docker pull {appraiserImage}");
            Console.WriteLine($"Image {appraiserImage} pulled");

            client.RunCommand($"docker pull {migrationsRunnerImage}");
            Console.WriteLine($"Image {migrationsRunnerImage} pulled");

            client.RunCommand($"cd {appDirectory} && docker-compose down");
            Console.WriteLine("App stopped");

            client.RunCommand($"cd {appDirectory} && docker-compose up -d");
            Console.WriteLine("App started");

            client.Disconnect();
        });

    private string GetImageName(string projectName)
    {
        return $"dyatlovhome/{projectName}:latest";
    }
}
