using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DNSimplezilla
{
    public class ICanHazIpClient : IPublicIpProvider
    {
        public async Task<IPAddress> GetPublicIpAsync()
        {
            return await GetPublicIpAsync("https://icanhazip.com").ConfigureAwait(false);
        }

        public async Task<IPAddress> GetPublicIPv4Async()
        {
            return await GetPublicIpAsync("https://ipv4.icanhazip.com").ConfigureAwait(false);
        }

        public async Task<IPAddress> GetPublicIPv6Async()
        {
            return await GetPublicIpAsync("https://ipv6.icanhazip.com").ConfigureAwait(false);
        }

        private static async Task<IPAddress> GetPublicIpAsync(string requestUri)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync(requestUri).ConfigureAwait(false);
                    response.EnsureSuccessStatusCode();
                    var value = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    return IPAddress.Parse(value.Trim());
                }
                catch (HttpRequestException e)
                {
                    throw new FetchExternalIpException(string.Format("Failed to get the public ip from '{0}': {1}, HTTP request failed", requestUri, e.Message), e);
                }
                catch (Exception e)
                {
                    throw new FetchExternalIpException(string.Format("Failed to get the public ip from '{0}': {1}", requestUri, e.Message), e);
                }
            }
        }
    }
}