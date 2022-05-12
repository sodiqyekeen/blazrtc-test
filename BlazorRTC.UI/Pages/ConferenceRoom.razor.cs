using BlazorRTC.Shared.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using MudBlazor;
using System.Net.Http.Json;

namespace BlazorRTC.UI.Pages
{
    public partial class ConferenceRoom
    {
        //string? meetingId;
        private IJSObjectReference? module;
        private DotNetObjectReference<ConferenceRoom>? dotNetHelper;
        private HubConnection? hubConnection;
        private CancellationTokenSource cts = new();
        bool joinMeeting = true;
        string thumbIcon = Icons.Material.Filled.AddIcCall;
        protected override async Task OnInitializedAsync()
        {
            await SetConnectionId();
            dotNetHelper = DotNetObjectReference.Create(this);

            hubConnection = new HubConnectionBuilder()
                .WithUrl(_configuration["ApiUrl"] + "blazorrtc")
                .Build();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
#pragma warning disable IDE0058 // Expression value is never used
            ConnectWithRetryAsync(hubConnection, cts.Token);


            hubConnection?.On<string, object, string>("Candidate", async (meetingId, candidate, clientId) =>
            {
                if (_appStateManager.CurrentMeetingId ==meetingId&& _appStateManager.Role is "caller")
                    await js.InvokeVoidAsync("handleCandidate", candidate, clientId);
            });

            hubConnection?.On<object>("SendCandidate", async (candidate) =>
                 await js.InvokeVoidAsync("handleCandidate", candidate));

            hubConnection?.On<string, object, string>("Answer", async (id, answer, clientId) =>
            {
                if (_appStateManager.CurrentMeetingId==id && _appStateManager.Role is "caller")
                    await js.InvokeVoidAsync("handleAnser", answer, clientId);
            });

            hubConnection?.On<string, object, string>("Offer", async (meetingId, offer, clientId) =>
            {
                if (_appStateManager.CurrentMeetingId==meetingId && _appStateManager.ClientId == clientId)
                {
                    Console.WriteLine($"received offer for ({meetingId})");
                    _appStateManager.MeetingStarted=true;
                    StateHasChanged();
                    await js.InvokeVoidAsync("joinCall", dotNetHelper, offer, clientId);
                }
            });

            hubConnection?.On<string, string>("JoinRequest", async (meetingId, clientId) =>
            {
                Console.WriteLine($"{clientId} is waiting to join ({meetingId})");
                Console.WriteLine(_appStateManager.CurrentMeetingId==meetingId);
                if (_appStateManager.CurrentMeetingId==meetingId)
                    await js.InvokeVoidAsync("createPeerOffer", dotNetHelper, clientId);
            });
            hubConnection.Closed += error => ConnectWithRetryAsync(hubConnection, cts.Token);
        }
        #region JS 

        [JSInvokable("addcandidate")]
        public async Task AddCandidate(object candidate, string clientId)
        {
            Console.WriteLine($"Adding candidate... {clientId}");
            await hubConnection!.InvokeAsync("AddMeetingCandidate", _appStateManager.CurrentMeetingId, candidate, clientId);
        }
        [JSInvokable("saveoffer")]
        public async Task SaveOffer(object offer, string clientId)
        {
            Console.WriteLine("Adding offer...");
            await hubConnection.InvokeAsync("SendOffer", _appStateManager.CurrentMeetingId, offer, clientId);
        }

        [JSInvokable("sendcandidate")]
        public async Task SendCandidate(object candidate, string clientId)
        {
            Console.WriteLine("Sending candidate...");
            await hubConnection.InvokeAsync("SendMeetingCandidate", _appStateManager.CurrentMeetingId, candidate, clientId);
        }

        [JSInvokable("sendanswer")]
        public async Task SendAnswer(string clientId, object answer)
        {
            Console.WriteLine("Sending answer...");
            await hubConnection!.InvokeAsync("SendMeetingAnswer", clientId, answer, _appStateManager.CurrentMeetingId);
        }

        #endregion

        public async Task CreateMeeting(CreateMeetingRequest request)
        {
            _appStateManager.CurrentMeetingId=Guid.NewGuid().ToString()[..6];
            _appStateManager.Role = "caller";
            _appStateManager.MeetingStarted=true;

            await js.InvokeVoidAsync("startMeeting");
            await hubConnection!.InvokeAsync("NewMeeting", _appStateManager.CurrentMeetingId, _appStateManager.ClientId);
        }

        public async Task JoinMeeting(JoinMeetingRequest request)
        {
            Console.WriteLine($"Requesting to join meeting: {request.Meetingid}");
            _appStateManager.CurrentMeetingId = request.Meetingid;
            _appStateManager.Role = "receiver";
            await hubConnection!.InvokeAsync("JoinMeeting", _appStateManager.CurrentMeetingId, _appStateManager.ClientId);
            //var offer = await _httpClient.GetFromJsonAsync<object>($"offers/{_appStateManager.CurrentMeetingId}");
            //Console.WriteLine($"Offer: " + offer);
            //_appStateManager.MeetingStarted=true;
            //await js.InvokeVoidAsync("joinCall", dotNetHelper, offer, _appStateManager.CurrentMeetingId);
        }

        async Task HangUp()
        {
            await js.InvokeVoidAsync("hangup");
            await hubConnection!.InvokeAsync("EndMeeting", _appStateManager.CurrentMeetingId, _appStateManager.ClientId);
            _appStateManager.MeetingStarted=false;
            _appStateManager.CurrentMeetingId=null;
            _appStateManager.Role= null;
        }

        async Task ToggleVideo()
        {
            await js.InvokeVoidAsync("toggleVideo", _appStateManager.VideoOff);
            _appStateManager.VideoOff = !_appStateManager.VideoOff;
        }
        async Task ToggleMic()
        {
            await js.InvokeVoidAsync("toggleMic", _appStateManager.MicOff);
            _appStateManager.MicOff = !_appStateManager.MicOff;
        }

        void ToggleStatus(bool status)
        {
            joinMeeting=!joinMeeting;
            thumbIcon =joinMeeting ? Icons.Material.Filled.AddIcCall : Icons.Material.Filled.Call;
            StateHasChanged();
        }
        async Task SetConnectionId()
        {
            var id = await _localStorage.GetItemAsStringAsync("connId");
            if (string.IsNullOrEmpty(id))
            {
                id=Guid.NewGuid().ToString();
                await _localStorage.SetItemAsStringAsync("connId", id);
            }
            _appStateManager.ClientId=id;
        }
        public async Task<bool> ConnectWithRetryAsync(HubConnection hubConnection, CancellationToken cancellationToken)
        {
            while (true)
            {
                try
                {
                    await hubConnection.StartAsync(cancellationToken);
                    return true;
                }
                catch when (cancellationToken.IsCancellationRequested)
                {
                    return false;
                }
                catch (Exception)
                {
                    await Task.Delay(5000, cancellationToken);
                }
            }
        }

        async ValueTask IAsyncDisposable.DisposeAsync()
        {
            if (module is not null)
            {
                await module.DisposeAsync();
            }
            if (hubConnection is not null)
            {
                await hubConnection.DisposeAsync();
            }
        }
    }
}
