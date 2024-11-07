namespace W2.Users
{
    public class ListUsersInput
    {
        public int MaxResultCount { get; set; }
        public int SkipCount { get; set; }
        public string Filter { get; set; }
        public string Role { get; set; }
        public string Sorting { get; set; }
    }
}
