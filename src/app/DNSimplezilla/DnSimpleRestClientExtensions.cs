using System.Collections.Generic;
using System.Linq;
using DNSimple;

namespace DNSimplezilla
{
    static class DnSimpleRestClientExtensions
    {
        public static IEnumerable<DomainRecord> GetDnsRecords(this DNSimpleRestClient client, Domain domain)
        {
            dynamic records = client.ListRecords(domain.Name);
            var enumerable = (IEnumerable<dynamic>)records;
            return enumerable.Select(x => x.record)
                             .Select(record => DomainRecord.FromDynamic(record))
                             .Cast<DomainRecord>();
        }
    }
}