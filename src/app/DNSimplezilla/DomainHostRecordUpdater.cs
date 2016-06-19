using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DNSimplezilla
{
    public class DomainHostRecordUpdater
    {
        private readonly IPublicIpProvider _publicIpProvider;
        private readonly IDnSimple _dnSimple;
        private readonly IEventLog _eventLog;

        public DomainHostRecordUpdater(IPublicIpProvider publicIpProvider, IDnSimple dnSimple, IEventLog eventLog)
        {
            if (publicIpProvider == null) throw new ArgumentNullException("publicIpProvider");
            if (dnSimple == null) throw new ArgumentNullException("dnSimple");
            if (eventLog == null) throw new ArgumentNullException("eventLog");

            _publicIpProvider = publicIpProvider;
            _dnSimple = dnSimple;
            _eventLog = eventLog;
        }

        public async Task UpdateAsync(IEnumerable<Domain> domains)
        {
            foreach (var domain in domains)
            {
                _eventLog.Info(string.Format("Checking domain [{0}]...", domain.Name));
                try
                {
                    await UpdateDomainAsync(domain);
                }
                catch (Exception e)
                {
                    _eventLog.Error(string.Format("Failed to update domain [{0}]: {1}", domain.Name, e.Message), e);
                    throw;
                }
            }
        }

        private async Task UpdateDomainAsync(Domain domain)
        {
            var publicIp = await _publicIpProvider.GetPublicIpAsync().ConfigureAwait(false);
            var domainRecords = await _dnSimple.GetDnsRecordsAsync(domain);
            var configuredRecords = domainRecords.Where(record => domain.HostRecords.Contains(record.Name))
                                                 .ToArray();
            try
            {
                var ipAddress = await _publicIpProvider.GetPublicIPv4Async();
                await UpdateDnsHostRecordsAsync(domain, configuredRecords.Where(record => "A".Equals(record.RecordType))
                                                                         .ToArray(), ipAddress);
            }
            catch (Exception e)
            {
                _eventLog.Error(
                    string.Format("Failed to update DNSv4 records of domain {0}: {1}", domain.Name,
                        string.Join(Environment.NewLine, configuredRecords.Where(record => "A".Equals(record.RecordType))
                                                                          .ToArray()
                                                                          .Select(r => r.ToString()))), e);
            }

            if (publicIp.AddressFamily != AddressFamily.InterNetworkV6)
            {
                _eventLog.Warn(
                    string.Format(
                        "The automatically determined public ip '{0}' is not an IPv6 Address, IPv6 DNS records won't get updated. Is IPv6 disabled?",
                        publicIp));
                return;
            }

            try
            {
                var ipAddress = await _publicIpProvider.GetPublicIPv6Async();
                await UpdateDnsHostRecordsAsync(domain, configuredRecords.Where(record => "AAAA".Equals(record.RecordType))
                                                                         .ToArray(), ipAddress);
            }
            catch (Exception e)
            {
                _eventLog.Error(
                    string.Format("Failed to update DNSv6 records of domain {0}: {1}", domain.Name,
                        string.Join(Environment.NewLine, configuredRecords.Where(record => "A".Equals(record.RecordType))
                                                                          .ToArray()
                                                                          .Select(r => r.ToString()))), e);
            }
        }

        private async Task UpdateDnsHostRecordsAsync(Domain domain, DomainRecord[] records, IPAddress publicIp)
        {
            if (!records.Any())
            {
                return;
            }
            var hostRecordsToUpdate = records.Where(r => !Equals(r.Content, publicIp.ToString()))
                                             .ToList();
            if (!hostRecordsToUpdate.Any())
            {
                _eventLog.Info(string.Format("No update necessary for domain [{0}], all host records up-to-date...", domain.Name));
                return;
            }
            _eventLog.Info(string.Format("Updating {0} record(s) to ip {1}:{2}{3}", hostRecordsToUpdate.Count, publicIp, Environment.NewLine,
                string.Join(Environment.NewLine, hostRecordsToUpdate)));

            foreach (var hostRecord in hostRecordsToUpdate)
            {
                try
                {
                    await _dnSimple.UpdateRecordAsync(domain, hostRecord, publicIp);
                }
                catch (Exception e)
                {
                    _eventLog.Error(string.Format("Failed to update record [{0}].", hostRecord), e);
                }
            }
        }
    }
}