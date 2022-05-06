using Microsoft.AspNetCore.SignalR;

namespace BlazorRTC.Api
{
    public class ChatHub : Hub
    {
        private readonly AppStateManager _appStateManager;

        public ChatHub(AppStateManager appStateManager)
        {
            _appStateManager=appStateManager;
        }

        public async Task OnConnect(string id)
        {
            _appStateManager.SaveUser(id);
            await Clients.All.SendAsync("ConnectionAdded", id);
        }

        public async Task Disconnect(string id)
        {
            _appStateManager.RemoveUser(id);
            await Clients.All.SendAsync("ConnectionRemoved", id);
        }

        public async Task AddCandidate(string id, object candidate)
        {
            _appStateManager.AddCandidate(id, candidate);
        }
        public async Task SendCandidate(string sender, object candidate)
        {
            await Clients.All.SendAsync("Candidate", sender, candidate);
        }
        public async Task SaveOffer(string id, object offer)
        {
            _appStateManager.StartCall(id, offer);
        }
        public async Task SendAnswer(string id, object answer)
        {
            await Clients.All.SendAsync("Answer", id, answer);
        }

        public async Task JoinCall(string id)
        {
            _appStateManager.GetCandidates(id).ForEach(async c => await Clients.All.SendAsync("SendCandidate", c));
        }

    }
}
