using Inc.TeamAssistant.Appraiser.Model;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace Inc.TeamAssistant.Appraiser.Dashboard.Services;

internal sealed class EventsProviderClient : IEventsProvider
{
	private readonly HubConnection _hubConnection;

	public EventsProviderClient(NavigationManager navigationManager)
	{
		if (navigationManager is null)
			throw new ArgumentNullException(nameof(navigationManager));

        _hubConnection = new HubConnectionBuilder()
            .WithUrl(navigationManager.ToAbsoluteUri("/messages"))
            .WithAutomaticReconnect()
            .Build();
	}

	public async Task OnStoryChanged(Guid assessmentSessionId, Func<Task> changed)
	{
		if (changed is null)
			throw new ArgumentNullException(nameof(changed));

        _hubConnection.On("StoryChanged", changed);

        await _hubConnection.StartAsync();
        await _hubConnection.InvokeAsync("JoinToGroup", assessmentSessionId);
    }

    public ValueTask DisposeAsync() => _hubConnection.DisposeAsync();
}