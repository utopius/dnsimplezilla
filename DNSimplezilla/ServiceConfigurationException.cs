using System;

namespace DNSimplezilla
{
    public class ServiceConfigurationException : Exception
    {
        public ServiceConfigurationException(string message)
            : base(message)
        {
        }
    }
}