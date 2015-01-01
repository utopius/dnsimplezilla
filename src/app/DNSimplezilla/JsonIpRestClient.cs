using System;
using System.Net;
using Newtonsoft.Json;
using RestSharp;

namespace DNSimplezilla
{
    public class JsonIpRestClient : RestClient
    {
        public JsonIpRestClient()
            : base("http://jsonip.com")
        {
            
        }

        public string FetchIp()
        {
            var request = new RestRequest(BaseUrl, Method.GET);

            var response = Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                dynamic ipObject = JsonConvert.DeserializeObject(response.Content);
                return ipObject.ip;
            }

            throw new JsonIpException(string.Format(
                "Error fetching ip address. StatusCode: {0}, StatusDescription: {1}", response.StatusCode,
                response.StatusDescription));
        }
    }

    public class JsonIpException : Exception
    {
        public JsonIpException(string message) : base(message)
        {
        }
    }
}
