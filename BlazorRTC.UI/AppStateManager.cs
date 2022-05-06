using Blazored.LocalStorage;

namespace BlazorRTC.UI
{
    public class AppStateManager
    {
        private string? _currentMeetingId;

        public string? CurrentMeetingId
        {
            get => _currentMeetingId;
            set
            {
                _currentMeetingId = value;
                NotifyStateChange();
            }
        }

        public event Action? OnChange;

        private void NotifyStateChange() => OnChange?.Invoke();
    }
}
