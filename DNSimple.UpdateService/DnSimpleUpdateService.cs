using System.Threading;
using System.Threading.Tasks;

namespace DNSimple.UpdateService
{
    public class DnSimpleUpdateService
    {
        private readonly Configuration _configuration;
        private readonly Timer _timer;
        private readonly int _intervalMilliSeconds;
        private readonly DnSimpleRestClient _dnSimple;
        private readonly JsonIpRestClient _jsonip;

        public DnSimpleUpdateService(Configuration configuration)
        {
            _configuration = configuration;

            _timer = new Timer(OnTimerTick);
            _intervalMilliSeconds = configuration.UpdateIntervalInMinutes * 1000 * 60;
            _dnSimple = new DnSimpleRestClient(configuration.Domain, configuration.DomainToken);
            _jsonip = new JsonIpRestClient();
        }

        public void Start()
        {
            _timer.Change(1, _intervalMilliSeconds);
        }

        private void OnTimerTick(object state)
        {
            Task.Factory.StartNew(UpdateDns)
                .ContinueWith(LogError, TaskContinuationOptions.OnlyOnFaulted);
        }

        private static void LogError(Task task)
        {
            var ex = task.Exception != null ? task.Exception.Flatten() : null;
            EventLog.Error(string.Format("An Exception occurred:\n {0}", ex), ex);
        }

        private void UpdateDns()
        {
            string ip = _jsonip.FetchIp();
            if (string.IsNullOrEmpty(ip))
            {
                EventLog.Warn(string.Format("Whoops, ip response is strange: {0}", ip));
                return;
            }

            var record = _dnSimple.FetchRecord(_configuration.RecordId);
            if (record == null)
            {
                EventLog.Warn(string.Format("Whoops, record is null."));
                return;
            }

            if (ip == record.content)
            {
                EventLog.Info(string.Format("Record '{0}' still points to IP address '{1}', no need to update.", record, ip));
                return;
            }

            EventLog.Info(string.Format("Current public IP Address is '{0}' (Obtained from http://jsonip.com)", ip));
            _dnSimple.UpdateRecord(_configuration.RecordId, ip);
            EventLog.Info(string.Format("Successfully updated record '{0}' to point to IP address '{1}'.", record, ip));
        }

        public void Stop()
        {
            _timer.Dispose();
        }
    }
}
