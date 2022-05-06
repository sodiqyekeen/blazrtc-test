using Blazored.LocalStorage;

namespace BlazorRTC.UI
{
    public class AppStateManager
    {
        private readonly ILocalStorageService _localStorage;

        public AppStateManager(ILocalStorageService localStorage)
        {
            _localStorage=localStorage;
        }

        public string? ConnectionId
        {
            get => _localStorage.GetItemAsStringAsync("connId").Result;
            set
            {
                var _connectionId = _localStorage.GetItemAsStringAsync("connId").Result;
                if (string.IsNullOrEmpty(_connectionId))
                    _localStorage.SetItemAsStringAsync("connId", value).GetAwaiter().GetResult();
            }
        }
    }
}
