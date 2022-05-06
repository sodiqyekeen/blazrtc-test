namespace BlazorRTC.Shared
{
    public class User
    {
        public User(string id)
        {
            Id=id;
        }
        public string Id { get; set; }
    }
}