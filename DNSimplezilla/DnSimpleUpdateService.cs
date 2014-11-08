using System;
using System.Threading;
using System.Threading.Tasks;
using DNSimple;

namespace DNSimplezilla
{
    public class DnSimpleUpdateService
    {
        private readonly ConfigurationProvider _configProvider;
        private readonly IEventLog _eventLog;
        private readonly Timer _timer;

        public DnSimpleUpdateService(ConfigurationProvider configProvider, IEventLog eventLog)
        {
            if (configProvider == null) throw new ArgumentNullException("configProvider");
            if (eventLog == null) throw new ArgumentNullException("eventLog");
            _configProvider = configProvider;
            _eventLog = eventLog;

            _timer = new Timer(OnTimerTick);
        }

        public void Start()
        {
            var configuration = _configProvider.Load();
            _timer.Change(TimeSpan.FromSeconds(1), TimeSpan.FromMinutes(configuration.UpdateInterval));
        }

        private void OnTimerTick(object state)
        {
            var jsonIpRestClient = new JsonIpRestClient();
            var configuration = _configProvider.Load();
            var dnSimpleRestClient = new DNSimpleRestClient(configuration.Username, token: configuration.ApiToken);
            var recordUpdater = new DomainHostRecordUpdater(jsonIpRestClient, dnSimpleRestClient, configuration.Domains, _eventLog);

            Task.Factory.StartNew(recordUpdater.Update)
                .ContinueWith(LogError, TaskContinuationOptions.OnlyOnFaulted);
        }

        private void LogError(Task task)
        {
            if (task == null) throw new ArgumentNullException("task");
            var ex = task.Exception != null ? task.Exception.Flatten() : null;
            _eventLog.Error(string.Format("An Exception occurred:\n {0}", ex), ex);
        }

        public void Stop()
        {
            _timer.Dispose();
        }
    }
}
