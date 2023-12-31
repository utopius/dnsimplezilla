﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DNSimplezilla
{
    public class MyExternalIpClient : IPublicIpProvider
    {
        // ReSharper disable once ClassNeverInstantiated.Local
        private class MyExternalIpDto
        {
            public string Ip { get; set; }
        }

        public async Task<IPAddress> GetPublicIpAsync()
        {
            return await GetPublicIpAsync("http://myexternalip.com/json").ConfigureAwait(false);
        }

        public async Task<IPAddress> GetPublicIPv4Async()
        {
            return await GetPublicIpAsync("http://ipv4.myexternalip.com/json").ConfigureAwait(false);
        }

        public async Task<IPAddress> GetPublicIPv6Async()
        {
            return await GetPublicIpAsync("http://ipv6.myexternalip.com/json").ConfigureAwait(false);
        }

        private static async Task<IPAddress> GetPublicIpAsync(string requestUri)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync(requestUri).ConfigureAwait(false);
                    var value = await response.EnsureSuccessStatusCode().Content.ReadAsStringAsync().ConfigureAwait(false);
                    var dto = JsonConvert.DeserializeObject<MyExternalIpDto>(value);
                    return IPAddress.Parse(dto.Ip);
                }
                catch (Exception e)
                {
                    throw new FetchExternalIpException(string.Format("Failed to get the public ip from '{0}': {1}", requestUri, e.Message), e);
                }
            }
        }
    }
}
