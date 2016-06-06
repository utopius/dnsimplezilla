using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DNSimplezilla
{
    class MyExternalIpDto
    {
        public string Ip { get; set; }
    }

    public class MyExternalIpClient : IPublicIpProvider
    {
        public async Task<IPAddress> GetPublicIPv4Async()
        {
            using (var client = new HttpClient())
            {
                var ipv4Response = await client.GetAsync("http://ipv4.myexternalip.com/json");
                ipv4Response.EnsureSuccessStatusCode();
                var ipv4 = await ipv4Response.Content.ReadAsStringAsync();
                var dto = JsonConvert.DeserializeObject<MyExternalIpDto>(ipv4);
                return IPAddress.Parse(dto.Ip);
            }
        }

        public async Task<IPAddress> GetPublicIPv6Async()
        {
            using (var client = new HttpClient())
            {
                var ipv6Response = await client.GetAsync("http://ipv6.myexternalip.com/json");
                ipv6Response.EnsureSuccessStatusCode();
                var ipv6 = await ipv6Response.Content.ReadAsStringAsync();
                var dto = JsonConvert.DeserializeObject<MyExternalIpDto>(ipv6);
                return IPAddress.Parse(dto.Ip);
            }
        }
    }
}
