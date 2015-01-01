using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace DNSimplezilla
{
    public class ConfigurationProvider
    {
        public static ConfigurationProvider Create(IEventLog eventLog)
        {
            var configFile = LocateConfigurationFile(eventLog);

            var provider = new ConfigurationProvider(configFile, eventLog);
            return provider;
        }

        private readonly FileInfo _configFile;
        private readonly IEventLog _eventLog;

        private ConfigurationProvider(FileInfo configFile, IEventLog eventLog)
        {
            if (configFile == null) throw new ArgumentNullException("configFile");
            _configFile = configFile;
            _eventLog = eventLog;
        }

        private static FileInfo LocateConfigurationFile(IEventLog eventLog)
        {
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly()
                                                                         .Location);
            var programDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), fileVersionInfo.ProductName);
            if (!Directory.Exists(programDataPath))
            {
                Directory.CreateDirectory(programDataPath);
            }

            var executableDirectoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly()
                                                                        .Location);
            if (executableDirectoryPath == null) throw new InvalidOperationException("ExecutableDirectory is null. WTF?");

            var localConfigFile = new FileInfo(Path.Combine(executableDirectoryPath, "dnsimplezilla.conf"));
            var programDataConfigFile = new FileInfo(Path.Combine(programDataPath, "dnsimplezilla.conf"));

            if (programDataConfigFile.Exists && !localConfigFile.Exists)
            {
                eventLog.Info(string.Format("Configuration file found at {0}.", programDataConfigFile.FullName));
                return programDataConfigFile;
            }
            if (localConfigFile.Exists && !programDataConfigFile.Exists)
            {
                eventLog.Info(string.Format("Configuration file found at {0}.", localConfigFile.FullName));
                return localConfigFile;
            }

            var message =
                string.Format("Failed to locate configuration. Place a config file like the provided 'dnsimplezilla.conf' in either {0} or {1}",
                    localConfigFile.FullName, programDataConfigFile.FullName);
            eventLog.Warn(message);
            throw new ServiceConfigurationException(message);
        }

        public Configuration Load()
        {
            try
            {
                using (var reader = new StreamReader(_configFile.OpenRead(), Encoding.UTF8))
                {
                    var json = reader.ReadToEnd();
                    var configuration = JsonConvert.DeserializeObject<Configuration>(json);

                    _eventLog.Info(string.Format("Configuration loaded from {0}.", _configFile.FullName));
                    return configuration;
                }
            }
            catch (Exception e)
            {
                _eventLog.Error(string.Format("Configuration file {0} corrupted and cannot be restored.", _configFile.FullName), e);
                throw;
            }
        }

        public void Save(Configuration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            _eventLog.Info(string.Format("Saving Configuration to '{0}'...", _configFile.FullName));
            try
            {
                using (var writer = new StreamWriter(_configFile.OpenWrite(), Encoding.UTF8))
                {
                    string json = JsonConvert.SerializeObject(configuration);
                    writer.Write(json);
                    _eventLog.Info(string.Format("Configuration saved to {0}.", _configFile.FullName));
                }
            }
            catch (Exception e)
            {
                _eventLog.Error("Error saving Configuration! File may be corrupted!", e);
            }
        }
    }
}