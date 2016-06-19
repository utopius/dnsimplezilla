using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DNSimple;

namespace DNSimplezilla
{
    public interface IDnSimple
    {
        Task<IEnumerable<DomainRecord>> GetDnsRecordsAsync(Domain domain);
        Task UpdateRecordAsync(Domain domain, DomainRecord hostRecord, IPAddress publicIp);
    }

    class DnSimple : IDnSimple
    {
        private readonly DNSimpleRestClient _client;

        public DnSimple(DNSimpleRestClient client)
        {
            _client = client;
        }

        public async Task<IEnumerable<DomainRecord>> GetDnsRecordsAsync(Domain domain)
        {
            return await Task.Run(() =>
            {
                dynamic records = _client.ListRecords(domain.Name);
                var enumerable = (IEnumerable<dynamic>) records;
                return enumerable.Select(x => x.record)
                                 .Select(record => DomainRecord.FromDynamic(record))
                                 .Cast<DomainRecord>();
            });
        }

        public async Task UpdateRecordAsync(Domain domain, DomainRecord hostRecord, IPAddress publicIp)
        {
            await Task.Run(() => _client.UpdateRecord(domain.Name, (int) hostRecord.Id, hostRecord.Name, publicIp.ToString()));
        }
    }
}