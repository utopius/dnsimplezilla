namespace DNSimplezilla
{
    public class Configuration
    {
        public string Username { get; set; }
        public string ApiToken { get; set; }
        public int UpdateInterval { get; set; }
        public Domain[] Domains { get; set; }
    }

    public class Domain
    {
        public string Name { get; set; }
        public string[] HostRecords { get; set; }
    }
}