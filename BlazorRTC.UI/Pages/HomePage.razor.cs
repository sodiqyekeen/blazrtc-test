using BlazorRTC.Shared.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using MudBlazor;
using System.Net.Http.Json;

namespace BlazorRTC.UI.Pages
{
    public partial class HomePage
    {
        bool meetingStarted;
        string? meetingId;
        private IJSObjectReference? module;
        private DotNetObjectReference<HomePage>? dotNetHelper;
        private HubConnection? hubConnection;
        private CancellationTokenSource cts = new();
        bool joinMeeting = true;
        string thumbIcon = Icons.Material.Filled.AddIcCall;
        protected override async Task OnInitializedAsync()
        {
            dotNetHelper = DotNetObjectReference.Create(this);

            hubConnection = new HubConnectionBuilder()
                .WithUrl(_configuration["ApiUrl"] + "blazorrtc")
                .Build();
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
#pragma warning disable IDE0058 // Expression value is never used
            ConnectWithRetryAsync(hubConnection, cts.Token);


            hubConnection?.On<string, object>("Candidate", async (sender, candidate) =>
            {
                if (_appStateManager.CurrentMeetingId ==sender&& _appStateManager.Role is "caller")
                    await js.InvokeVoidAsync("handleCandidate", candidate);
            });

            hubConnection?.On<object>("SendCandidate", async (candidate) =>
                 await js.InvokeVoidAsync("handleCandidate", candidate));

            hubConnection?.On<string, object>("Answer", async (id, answer) =>
            {
                if (_appStateManager.CurrentMeetingId==id && _appStateManager.Role is "caller")
                    await js.InvokeVoidAsync("handleAnser", answer);
            });


            hubConnection.Closed += error => ConnectWithRetryAsync(hubConnection, cts.Token);
        }
        #region JS 

        [JSInvokable("addcandidate")]
        public async Task AddCandidate(object candidate)
        {
            Console.WriteLine("Adding candidate...");
            await hubConnection.InvokeAsync("AddCandidate", _appStateManager.CurrentMeetingId, candidate);
        }
        [JSInvokable("saveoffer")]
        public async Task SaveOffer(object offer)
        {
            Console.WriteLine("Adding offer...");
            await hubConnection.InvokeAsync("SaveOffer", _appStateManager.CurrentMeetingId, offer);
        }

        [JSInvokable("sendcandidate")]
        public async Task SendCandidate(object candidate)
        {
            Console.WriteLine("Sending candidate...");
            await hubConnection.InvokeAsync("SendCandidate", _appStateManager.CurrentMeetingId, candidate);
        }

        [JSInvokable("sendanswer")]
        public async Task SendAnswer(string caller, object answer)
        {
            Console.WriteLine("Sending answer...");
            await hubConnection.InvokeAsync("SendAnswer", caller, answer);
        }

        #endregion

        public async Task CreateMeeting(CreateMeetingRequest request)
        {
            meetingId=Guid.NewGuid().ToString();
            _appStateManager.CurrentMeetingId=meetingId;
            _appStateManager.Role = "caller";
            _appStateManager.MeetingStarted=true;
            await js.InvokeVoidAsync("createPeerOffer", dotNetHelper);
        }

        public async Task JoinMeeting(JoinMeetingRequest request)
        {
            _appStateManager.CurrentMeetingId = request.Meetingid;
            _appStateManager.Role = "receiver";
            var offer = await _httpClient.GetFromJsonAsync<object>($"offers/{_appStateManager.CurrentMeetingId}");
            Console.WriteLine($"Offer: " + offer);
            _appStateManager.MeetingStarted=true;
            await js.InvokeVoidAsync("joinCall", dotNetHelper, offer, _appStateManager.CurrentMeetingId);
        }

        async Task HangUp()
        {
            await js.InvokeVoidAsync("hangup");
            _appStateManager.MeetingStarted=false;
            _appStateManager.CurrentMeetingId=null;
            _appStateManager.Role= null;
        }

        async Task ToggleStatus(bool status)
        {
            joinMeeting=!joinMeeting;
            thumbIcon =joinMeeting ? Icons.Material.Filled.AddIcCall : Icons.Material.Filled.Call;
            StateHasChanged();
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
