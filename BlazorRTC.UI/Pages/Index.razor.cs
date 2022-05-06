using BlazorRTC.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.JSInterop;
using MudBlazor;
using System.Net.Http.Json;

namespace BlazorRTC.UI.Pages
{
    public partial class Index
    {
        string? text = "";
        private HubConnection? hubConnection;
        private List<string> messages = new List<string>();

        private DotNetObjectReference<Index>? dotNetHelper;
        List<string> users = new();
        string? id;
        private CancellationTokenSource cts = new();
        bool online = false;
        string thumbIcon = Icons.Material.Filled.Close;
        Color thumbColour = Color.Error;

        protected override async Task OnInitializedAsync()
        {
            await SetConnectionId();
            dotNetHelper = DotNetObjectReference.Create(this);


            hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7282/blazorrtc")
                .Build();
            ConnectWithRetryAsync(hubConnection, cts.Token);



            hubConnection?.On<string>("ConnectionAdded", async (id) =>
            {
                await GetUsers();
                StateHasChanged();
            });

            hubConnection?.On<string>("ConnectionRemoved", async (id) =>
            {
                await GetUsers();
                StateHasChanged();
            });

            hubConnection?.On<string, object>("Candidate", async (sender, candidate) =>
            {
                if (id!=sender)
                    await jsRuntime.InvokeVoidAsync("handleCandidate", candidate);
            });

            hubConnection?.On<object>("SendCandidate", async (candidate) =>
                await jsRuntime.InvokeVoidAsync("handleCandidate", candidate));

            hubConnection?.On<string, object>("Answer", async (id, answer) =>
            {
                if (this.id==id)
                    await jsRuntime.InvokeVoidAsync("handleAnser", answer);
            });

            //hubConnection?.On<string, object, string>("joinedcall", async (id, answer, type) =>
            //{
            //    if (this.id==id)
            //        await jsRuntime.InvokeVoidAsync("handleSignallingData", answer);
            //});



            hubConnection.Closed += error => ConnectWithRetryAsync(hubConnection, cts.Token);
            await GetUsers();
        }

        async Task SendMessage()
        {
            await jsRuntime.InvokeAsync<string>("sendMessage", $"Dummy message from {id} @ {DateTime.Now}");
        }
        async Task CreateOffer()
        {
            await jsRuntime.InvokeVoidAsync("createPeerOffer", dotNetHelper);
        }

        async Task JoinCall(string id)
        {
            var offer = await _httpClient.GetFromJsonAsync<object>($"offers/{id}");
            Console.WriteLine($"Offer: " + offer);
            await jsRuntime.InvokeVoidAsync("joinCall", dotNetHelper, offer, id);
        }

        [JSInvokable("addcandidate")]
        public async Task AddCandidate(object candidate)
        {
            Console.WriteLine("Adding candidate...");
            await hubConnection.InvokeAsync("AddCandidate", id, candidate);
        }
        [JSInvokable("saveoffer")]
        public async Task SaveOffer(object offer)
        {
            Console.WriteLine("Adding offer...");
            await hubConnection.InvokeAsync("SaveOffer", id, offer);
        }

        [JSInvokable("sendcandidate")]
        public async Task SendCandidate(object candidate)
        {
            Console.WriteLine("Sending candidate...");
            await hubConnection.InvokeAsync("SendCandidate", id, candidate);
        }

        [JSInvokable("sendanswer")]
        public async Task SendAnswer(string caller, object answer)
        {
            Console.WriteLine("Sending answer...");
            await hubConnection.InvokeAsync("SendAnswer", caller, answer);
        }


        #region Basic

        async Task ToggleStatus(bool status)
        {
            online=status;
            if (status)
            {
                thumbIcon = Icons.Material.Filled.Done;
                thumbColour = Color.Success;
                await hubConnection.InvokeAsync("OnConnect", id);
            }
            else
            {
                thumbIcon =  Icons.Material.Filled.Close;
                thumbColour = Color.Error;
                await hubConnection.InvokeAsync("Disconnect", id);
            }
            //StateHasChanged();
        }
        async Task GetUsers() => users = (await _httpClient.GetFromJsonAsync<List<string>>("users"))??new List<string>();
        async Task SetConnectionId()
        {
            id = await _localStorage.GetItemAsStringAsync("connId");
            if (string.IsNullOrEmpty(id))
            {
                id=Guid.NewGuid().ToString();
                await _localStorage.SetItemAsStringAsync("connId", id);
            }
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

        public async ValueTask DisposeAsync()
        {

            if (hubConnection is not null)
            {
                await hubConnection.InvokeAsync("Disconnect", id);
                await hubConnection.DisposeAsync();
            }
        }

        #endregion
    }
}
