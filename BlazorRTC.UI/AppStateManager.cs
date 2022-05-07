using Blazored.LocalStorage;

namespace BlazorRTC.UI
{
    public class AppStateManager
    {
        private string? _currentMeetingId;
        private string? _role;
        private bool _meetingStarted;
        private bool _videoOn;
        private bool _micOn;
        private bool _speakerOn;
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

        public bool VideoOff
        {
            get => _videoOn;
            set
            {
                _videoOn=value;
                NotifyStateChange();
            }
        }
        public bool MicOff
        {
            get => _micOn;
            set
            {
                _micOn=value;
                NotifyStateChange();
            }
        }

        public bool SpeakerOn
        {
            get => _speakerOn;
            set
            {
                _speakerOn=value;
                NotifyStateChange();
            }
        }

        public event Action? OnChange;

        private void NotifyStateChange() => OnChange?.Invoke();
    }
}
