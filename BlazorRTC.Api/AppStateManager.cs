namespace BlazorRTC.Api
{
    public class AppStateManager
    {
        private List<User> _users = new();
        private List<CallGroup> _groups = new();

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
            if (_users.All(u => u.id != id))
                return;
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

        public object? GetOffer(string id) => _groups.FirstOrDefault(g => g.id==id)?.offer;

        public void AddCandidate(string id, object candidate)
        {
            var group = _groups.FirstOrDefault(u => u.id == id);
            if (group==null)
                return;
            group.Candidates.Add(candidate);
        }

    }

    public record User(string id, string? username);

    public record CallGroup(string id, object offer, object? answer = null)
    {
        public List<object> Candidates { get; set; } = new();
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
