using Blazored.LocalStorage;

namespace BlazorRTC.UI
{
    public class AppStateManager
    {
        private string? _currentMeetingId;
        private string? _role;
        private bool _meetingStarted;
        public string? CurrentMeetingId
        {
            get => _currentMeetingId;
            set
            {
                _currentMeetingId = value;
                NotifyStateChange();
            }
        }
        public string? Role
        {
            get => _role;
            set
            {
                _role = value;
                NotifyStateChange();
            }
        }

        public bool MeetingStarted
        {
            get => _meetingStarted;
            set
            {
                _meetingStarted=value;
                NotifyStateChange();
            }
        }

        public event Action? OnChange;

        private void NotifyStateChange() => OnChange?.Invoke();
    }
}
