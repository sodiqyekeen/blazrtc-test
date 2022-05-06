namespace BlazorRTC.Api.Services
{
    public class MeetingService
    {
        private readonly AppStateManager _appStateManager;

        public MeetingService(AppStateManager appStateManager)
        {
            _appStateManager=appStateManager;
        }
    }
}
