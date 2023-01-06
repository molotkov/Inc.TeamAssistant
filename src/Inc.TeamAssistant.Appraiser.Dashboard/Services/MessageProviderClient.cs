using System.Net.Http.Json;
using Inc.TeamAssistant.Appraiser.Model;
using Inc.TeamAssistant.Appraiser.Model.Common;

namespace Inc.TeamAssistant.Appraiser.Dashboard.Services;

internal sealed class MessageProviderClient : IMessageProvider
{
    private readonly HttpClient _client;

    public MessageProviderClient(HttpClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    public async Task<ServiceResult<Dictionary<string, Dictionary<string, string>>>> Get()
    {
        try
        {
            var result = await _client
                .GetFromJsonAsync<ServiceResult<Dictionary<string, Dictionary<string, string>>>>("resources");

            if (result is null)
                throw new ApplicationException("Parse response with error.");

            return result;
        }
        catch (Exception ex)
        {
            return ServiceResult.Failed<Dictionary<string, Dictionary<string, string>>>(ex.Message);
        }
    }
}