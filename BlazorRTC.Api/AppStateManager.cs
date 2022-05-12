using System.Diagnostics.CodeAnalysis;

namespace BlazorRTC.Api
{
    public class AppStateManager
    {
        private List<User> _users = new();
        private List<CallGroup> _groups = new();
        private List<Meeting> _meetings = new();
        #region Old Method

        public void SaveUser(string id, string? username = null)
        {
            var user = _users.FirstOrDefault(u => u.id==id);
            if (user!=null)
            {
                RemoveUser(id);
            }
            _users.Add(new User(id, username));
        }

        public void RemoveUser(string id)
        {
            _users.RemoveAll(u => u.id==id);
        }

        public IEnumerable<string> GetUsers() => _users.Select(u => u.id);

        public void StartCall(string id, object offer)
        {
            //if (_users.All(u => u.id != id))
            //    return;
            _groups.Add(new CallGroup(id, offer));
        }

        public void JoinCall(string id, object candidate)
        {
            var group = _groups.FirstOrDefault(u => u.id != id);
            if (group==null)
                return;
            _groups.Remove(group);
            var newGroup = group with { answer=candidate };
            _groups.Add(newGroup);
        }

        public List<object> GetCandidates(string id)
        {
            return _groups.First(g => g.id==id).Candidates;
        }

        public List<Meeting> GetMeetings() => _meetings;

        public object? GetOffer(string id) => _groups.FirstOrDefault(g => g.id==id)?.offer;

        public void AddCandidate(string id, object candidate)
        {
            var group = _groups.FirstOrDefault(u => u.id == id);
            if (group==null)
                return;
            group.Candidates.Add(candidate);
        }

        public void AddCandidate(string meetingId, object candidate, string clientId)
        {
            if (!TryGetMeeting(meetingId, out var meeting)) return;
            if (!meeting.Participants.ContainsKey(clientId))
            {
                meeting.Participants.Add(clientId, new List<object> { candidate });
                return;
            }
            meeting.Participants[clientId].Add(candidate);
        }

        public void JoinMeeting(string meetingId, string clientId)
        {
            if (!TryGetMeeting(meetingId, out var meeting)) return;
            if (meeting.Participants.ContainsKey(clientId)) return;
            meeting.Participants.Add(clientId, new());
        }
        #endregion

        public void CreateMeeting(string meetingId, string clientId)
        {
            var meeting = new Meeting(meetingId, clientId);
            _meetings.Add(meeting);
        }

        public void LeaveMeeting(string meetingId, string clientId)
        {
            // var meeting = _meetings.FirstOrDefault(m => m.Id==meetingId);
            if (!TryGetMeeting(meetingId, out var meeting)) return;
            //meeting.Participants.RemoveAll(p => p==clientId);
            if (meeting.Participants.ContainsKey(clientId))
                meeting.Participants.Remove(clientId);
        }

        private bool TryGetMeeting(string id, [NotNullWhen(true)] out Meeting? meeting)
        {
            meeting = _meetings.FirstOrDefault(m => m.Id==id);
            return meeting !=null;
        }

    }

    public record User(string id, string? username);

    public record CallGroup(string id, object offer, object? answer = null)
    {
        public List<object> Candidates { get; set; } = new();
    }



    public class Meeting
    {
        public Meeting(string id, string createdBy)
        {
            Id=id;
            CreatedBy=createdBy;
            Participants.Add(createdBy, new());
        }
        public string Id { get; set; }
        public string CreatedBy { get; set; }
        public Dictionary<string, List<object>> Participants { get; set; } = new();
    }

    //public class Call
    //{
    //    public Call(string id, object offer)
    //    {
    //        Id=id;
    //        Offer = 
    //        Candidates= new();
    //    }
    //    public string Id { get; set; }

    //    public object Offer { get; set; }
    //    public object Answer { get; set; }
    //    public List<object> Candidates { get; set; }
    //}
}
