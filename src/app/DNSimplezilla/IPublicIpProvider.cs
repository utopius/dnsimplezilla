using System.Net;
using System.Threading.Tasks;

namespace DNSimplezilla
{
    public interface IPublicIpProvider
    {
        Task<IPAddress> GetPublicIpAsync();
        Task<IPAddress> GetPublicIPv4Async();
        Task<IPAddress> GetPublicIPv6Async();
    }
}