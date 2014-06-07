using System;
using System.Dynamic;
using System.Globalization;
using Newtonsoft.Json;
using RestSharp;

namespace DNSimple.UpdateService
{
    class DnSimpleRestClient : RestClient
    {
        private readonly string _domain;

        public DnSimpleRestClient(string domain, string domainToken)
            : base("https://dnsimple.com/")
        {
            if (domain == null) throw new ArgumentNullException("domain");
            if (string.IsNullOrWhiteSpace(domain)) throw new ArgumentException("argument must not be null or whitespace", "domain");
            if (domainToken == null) throw new ArgumentNullException("domainToken");
            if (string.IsNullOrWhiteSpace(domainToken)) throw new ArgumentException("argument must not be null or whitespace", "domainToken");


            this.AddDefaultHeader("X-DNSimple-Domain-Token", domainToken);
            this.AddDefaultHeader("Accept", "application/json");

            _domain = domain;
        }

        private RestRequest CreateRecordRequest(int recordId)
        {
            var request = new RestRequest("domains/{domain}/records/{id}");
            request.RequestFormat = DataFormat.Json;
            request.AddUrlSegment("domain", _domain);
            request.AddUrlSegment("id", recordId.ToString(CultureInfo.InvariantCulture)); // replaces matching token in request.Resource

            return request;
        }

        public dynamic FetchRecord(int recordId)
        {
            var request = CreateRecordRequest(recordId);

            var response = Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject(response.Content);
            }

            throw new DnSimpleException(
                string.Format("Error fetching record {0}. StatusCode: {1}, StatusDescription: {2}", recordId,
                    response.StatusCode, response.StatusDescription));
        }

        public dynamic UpdateRecord(int recordId, string ip)
        {
            var request = CreateRecordRequest(recordId);
            request.Method = Method.PUT;

            dynamic body = new ExpandoObject();
            body.record = new ExpandoObject();
            body.record.content = ip;

            request.AddBody(body);

            var response = Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return JsonConvert.DeserializeObject(response.Content);
            }

            throw new DnSimpleException(
                string.Format("Error updating record {0} to ip {1}. StatusCode: {2}, StatusDescription: {3}", recordId,
                    ip, response.StatusCode, response.StatusDescription));
        }
    }

    public class DnSimpleException : Exception
    {
        public DnSimpleException(string message)
            : base(message)
        {
        }
    }
}
