using Topshelf;

namespace DNSimple.UpdateService
{
    class Program
    {
        static int Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            var exitCode = HostFactory.Run(x =>
                {
                    x.Service<DnSimpleUpdateService>(s =>
                    {
                        s.ConstructUsing(hostSettings =>
                        {
                            EventLog.Info("DNSimplezilla preparing for start...");

                            var configProvider = ConfigurationProvider.Load();
                            return new DnSimpleUpdateService(configProvider.Configuration);
                        });
                        s.WhenStarted((service, host) =>
                            {
                                EventLog.Info("DNSimplezilla starting...");

                                service.Start();

                                EventLog.Info("DNSimplezilla started.");
                                return true;
                            });
                        s.WhenStopped((service, host) =>
                            {
                                EventLog.Info("DNSimplezilla stopping...");

                                service.Stop();

                                EventLog.Info("DNSimplezilla stopped.");
                                return true;
                            });
                    });
                    x.SetServiceName("DNSimplezilla");
                    x.SetDisplayName("DNSimplezilla");
                    x.SetDescription("Updates DNSimple Records");
                    x.DependsOnEventLog();
                    x.RunAsLocalSystem();
                });

            return (int)exitCode;
        }
    }
}
