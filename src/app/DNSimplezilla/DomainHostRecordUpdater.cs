using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DNSimple;

namespace DNSimplezilla
{
    public class DomainHostRecordUpdater
    {
        private readonly IPublicIpProvider _publicIpProvider;
        private readonly DNSimpleRestClient _dnSimpleClient;
        private readonly Domain[] _domains;
        private readonly IEventLog _eventLog;

        public DomainHostRecordUpdater(IPublicIpProvider publicIpProvider, DNSimpleRestClient dnSimpleClient, Domain[] domains, IEventLog eventLog)
        {
            if (publicIpProvider == null) throw new ArgumentNullException("publicIpProvider");
            if (dnSimpleClient == null) throw new ArgumentNullException("dnSimpleClient");
            if (domains == null) throw new ArgumentNullException("domains");
            if (eventLog == null) throw new ArgumentNullException("eventLog");

            _publicIpProvider = publicIpProvider;
            _dnSimpleClient = dnSimpleClient;
            _domains = domains;
            _eventLog = eventLog;
        }

        public async Task UpdateAsync()
        {
            foreach (var domain in _domains)
            {
                _eventLog.Info(string.Format("Checking domain [{0}]...", domain.Name));
                try
                {
                    await UpdateDomainAsync(domain);
                }
                catch (Exception e)
                {
                    _eventLog.Error(string.Format("Failed to update domain [{0}].", domain.Name), e);
                }
            }
        }

        private async Task UpdateDomainAsync(Domain domain)
        {
            var domainRecords = _dnSimpleClient.GetDnsRecords(domain)
                                               .ToArray();
            var configuredRecords = domainRecords.Where(record => domain.HostRecords.Contains(record.Name))
                                                 .ToArray();
            var dnsV4Records = configuredRecords.Where(record => "A".Equals(record.RecordType))
                                                .ToArray();
            if (dnsV4Records.Any())
            {
                try
                {
                    await UpdateDnsV4Records(domain, dnsV4Records);
                }
                catch (Exception e)
                {
                    _eventLog.Error(string.Format("Failed to update DNSv4 records of domain {0}: {1}", domain.Name, string.Join(Environment.NewLine, dnsV4Records.Select(r => r.ToString()))), e);
                }
            }

            var dnsV6Records = configuredRecords.Where(record => "AAAA".Equals(record.RecordType))
                                                .ToArray();
            if (dnsV6Records.Any())
            {
                try
                {
                    await UpdateDnsV6Records(domain, dnsV4Records);
                }
                catch (Exception e)
                {
                    _eventLog.Error(string.Format("Failed to update DNSv4 records of domain {0}: {1}", domain.Name, string.Join(Environment.NewLine, dnsV4Records.Select(r => r.ToString()))), e);
                }
            }
        }

        private async Task UpdateDnsV4Records(Domain domain, IEnumerable<DomainRecord> records)
        {
            var publicIp = await _publicIpProvider.GetPublicIPv4Async();
            var hostRecordsToUpdate = records.Where(r => !Equals(r.Content, publicIp.ToString()))
                                             .ToList();
            if (!hostRecordsToUpdate.Any())
            {
                _eventLog.Info(string.Format("No update necessary for domain [{0}], all host records up-to-date...", domain.Name));
                return;
            }
            _eventLog.Info(string.Format("Updating {0} record(s) to ip {1}:{2}{3}", hostRecordsToUpdate.Count, publicIp, Environment.NewLine, string.Join(Environment.NewLine, hostRecordsToUpdate)));

            foreach (var hostRecord in hostRecordsToUpdate)
            {
                try
                {
                    _dnSimpleClient.UpdateRecord(domain.Name, (int)hostRecord.Id, hostRecord.Name, publicIp.ToString());
                }
                catch (Exception e)
                {
                    _eventLog.Error(string.Format("Failed to update record [{0}].", hostRecord), e);
                }
            }
        }

        private async Task UpdateDnsV6Records(Domain domain, IEnumerable<DomainRecord> records)
        {
            var publicIp = await _publicIpProvider.GetPublicIPv6Async();
            var hostRecordsToUpdate = records.Where(r => !Equals(r.Content, publicIp.ToString()))
                                             .ToList();
            if (!hostRecordsToUpdate.Any())
            {
                _eventLog.Info(string.Format("No update necessary for domain [{0}], all host records up-to-date...", domain.Name));
                return;
            }
            _eventLog.Info(string.Format("Updating {0} record(s) to ip {1}:{2}{3}", hostRecordsToUpdate.Count, publicIp, Environment.NewLine, string.Join(Environment.NewLine, hostRecordsToUpdate)));

            foreach (var hostRecord in hostRecordsToUpdate)
            {
                try
                {
                    _dnSimpleClient.UpdateRecord(domain.Name, (int)hostRecord.Id, hostRecord.Name, publicIp.ToString());
                }
                catch (Exception e)
                {
                    _eventLog.Error(string.Format("Failed to update record [{0}].", hostRecord), e);
                }
            }
        }
    }
}