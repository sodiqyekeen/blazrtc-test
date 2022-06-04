using Blazored.LocalStorage;

namespace BlazorRTC.UI
{
    public class AppStateManager
    {
        private string? _currentMeetingId;
        private string? _role;
        private bool _meetingStarted;
        private bool _videoOff;
        private bool _micOff;
        private bool _speakerOn;
        private List<string>? _remoteIds;
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
            get => _videoOff;
            set
            {
                _videoOff=value;
                NotifyStateChange();
            }
        }
        public bool MicOff
        {
            get => _micOff;
            set
            {
                _micOff=value;
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

        public List<string> RemoteIds
        {
            get => _remoteIds ??= new List<string>();
            set
            {
                _remoteIds=value;

                NotifyStateChange();
            }
        }

        public string? ClientId { get; set; }

        public event Action? OnChange;

        private void NotifyStateChange() => OnChange?.Invoke();

        public void Reset()
        {
            _micOff=false;
            _videoOff=false;
            _meetingStarted=false;
            _role =null;
            _currentMeetingId=null;
            _remoteIds=null;
            NotifyStateChange();
        }
    }
}
