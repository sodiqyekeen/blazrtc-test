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
        public async Task AddMeetingCandidate(string id, object candidate, string clientId)
        {
            _appStateManager.AddCandidate(id, candidate, clientId);
        }
        public async Task SendCandidate(string sender, object candidate)
        {
            await Clients.All.SendAsync("Candidate", sender, candidate);
        }
        public async Task SendMeetingCandidate(string meetingId, object candidate, string clientId)
        {
            await Clients.Group(meetingId).SendAsync("Candidate", meetingId, candidate, clientId);
        }
        public async Task SaveOffer(string id, object offer)
        {
            _appStateManager.StartCall(id, offer);
        }

        public async Task SendOffer(string meetingId, object offer, string clientId)
        {
            await Clients.Groups(meetingId).SendAsync("Offer", meetingId, offer, clientId);
        }
        public async Task SendMeetingAnswer(string clientId, object answer, string meetingId)
        {
            await Clients.Group(meetingId).SendAsync("Answer", meetingId, answer, clientId);
        }
        public async Task SendAnswer(string clientId, object answer)
        {
            await Clients.All.SendAsync("Answer", clientId, answer);
        }

        public async Task JoinCall(string id)
        {
            _appStateManager.GetCandidates(id).ForEach(async c => await Clients.All.SendAsync("SendCandidate", c));
        }

        public async Task JoinMeeting(string meetingId, string clientId)
        {
            _appStateManager.JoinMeeting(meetingId, clientId);
            await Clients.Groups(meetingId).SendAsync("JoinRequest", meetingId, clientId);
            await Groups.AddToGroupAsync(Context.ConnectionId, meetingId);

        }

        public async Task NewMeeting(string meetingId, string clientId)
        {
            _appStateManager.CreateMeeting(meetingId, clientId);
            await Groups.AddToGroupAsync(Context.ConnectionId, meetingId);
        }
        public async Task EndMeeting(string meetingId, string clientId)
        {
            _appStateManager.LeaveMeeting(meetingId, clientId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, meetingId);
            await Clients.Group(meetingId).SendAsync("NotifyMeeting", $"{clientId} left the call.");
        }

    }
}
