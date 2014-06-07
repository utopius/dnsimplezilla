using System;

namespace DNSimple.UpdateService
{
    public class Configuration
    {
        public String Domain { get; set; }
        public String DomainToken { get; set; }
        public int RecordId { get; set; }
        public int UpdateIntervalInMinutes { get; set; }
    }
}