using System.ComponentModel.DataAnnotations;

namespace BlazorRTC.Shared.Models
{
    public class CreateMeetingRequest
    {
        [Required]
        public string? Username { get; set; }
    }

    public class JoinMeetingRequest : CreateMeetingRequest
    {
        [Required]
        public string? Meetingid { get; set; }
    }
}
