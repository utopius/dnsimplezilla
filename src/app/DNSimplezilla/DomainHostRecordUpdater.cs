using System;
using System.Collections.Generic;
using System.Linq;
using DNSimple;

namespace DNSimplezilla
{
    public class DomainHostRecordUpdater
    {
        private readonly JsonIpRestClient _jsonIpClient;
        private readonly DNSimpleRestClient _dnSimpleClient;
        private readonly Domain[] _domains;
        private readonly IEventLog _eventLog;

        public DomainHostRecordUpdater(JsonIpRestClient jsonIpClient, DNSimpleRestClient dnSimpleClient, Domain[] domains, IEventLog eventLog)
        {
            if (jsonIpClient == null) throw new ArgumentNullException("jsonIpClient");
            if (dnSimpleClient == null) throw new ArgumentNullException("dnSimpleClient");
            if (domains == null) throw new ArgumentNullException("domains");
            if (eventLog == null) throw new ArgumentNullException("eventLog");

            _jsonIpClient = jsonIpClient;
            _dnSimpleClient = dnSimpleClient;
            _domains = domains;
            _eventLog = eventLog;
        }

        public void Update()
        {
            var publicIp = _jsonIpClient.FetchIp();
            if (string.IsNullOrEmpty(publicIp))
            {
                _eventLog.Warn(string.Format("Whoops, ip response is strange: [{0}], skipping update...", publicIp));
                return;
            }

            foreach (var domain in _domains)
            {
                _eventLog.Info(string.Format("Checking domain [{0}]...", domain.Name));
                try
                {
                    var domainWasUpdated = UpdateDomainRecords(_dnSimpleClient, domain, publicIp);
                    if (!domainWasUpdated)
                    {
                        _eventLog.Info(string.Format("No update necessary for domain [{0}]...", domain.Name));
                    }
                }
                catch (Exception e)
                {
                    _eventLog.Error(string.Format("Failed to update domain [{0}].", domain.Name), e);
                }
            }
        }

        private bool UpdateDomainRecords(DNSimpleRestClient client, Domain domain, string publicIp)
        {
            var records = ((IEnumerable<dynamic>)client.ListRecords(domain.Name)).Select(x => x.record)
                                                                                 .Select(record => DomainRecord.FromDynamic(record))
                                                                                 .Cast<DomainRecord>()
                                                                                 .ToList();

            var hostRecords = records.Where(record => domain.HostRecords.Contains(record.Name))
                                     .Where(record => "A".Equals(record.RecordType))
                                     .ToList();

            var upToDateRecords = hostRecords.Where(r => r.Content == publicIp)
                                             .ToList();
            if (upToDateRecords.Any())
                _eventLog.Info(string.Format("Records which are up to date:{0}{1}", Environment.NewLine, string.Join(Environment.NewLine, upToDateRecords)));

            var hostRecordsToUpdate = hostRecords.Except(upToDateRecords)
                                                 .ToList();

            if (!hostRecordsToUpdate.Any())
            {
                return false;
            }

            _eventLog.Info(string.Format("Updating records to ip {0}:{1}{2}", publicIp, Environment.NewLine,
                string.Join(Environment.NewLine, hostRecordsToUpdate)));

            foreach (var hostRecord in hostRecordsToUpdate)
            {
                try
                {
                    client.UpdateRecord(domain.Name, (int)hostRecord.Id, hostRecord.Name, publicIp);
                }
                catch (Exception e)
                {
                    _eventLog.Error(string.Format("Failed to update record [{0}].", hostRecord), e);
                }
            }
            return true;
        }
    }
}