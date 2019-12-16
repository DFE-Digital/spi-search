using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.WindowsAzure.Storage.Core;
using Newtonsoft.Json;

namespace Dfe.Spi.Search.IntegrationTests.TestHelpers
{
    public class RequestBuilder
    {
        private string _method = "GET";
        private byte[] _body;
        private Dictionary<string, string> _headers = new Dictionary<string, string>();

        private RequestBuilder()
        {
        }

        public static RequestBuilder CreateRequest()
        {
            return new RequestBuilder();
        }


        public RequestBuilder WithMethod(string method)
        {
            _method = method;
            return this;
        }


        public RequestBuilder WithJsonBody(object body)
        {
            var json = JsonConvert.SerializeObject(body);
            return WithBody(json);
        }

        public RequestBuilder WithBody(string body)
        {
            var buffer = Encoding.UTF8.GetBytes(body);
            return WithBody(buffer);
        }

        public RequestBuilder WithBody(byte[] body)
        {
            _body = body;
            return this;
        }


        public RequestBuilder WithHeader(string name, string value)
        {
            if (_headers.ContainsKey(name))
            {
                _headers[name] = value;
            }
            else
            {
                _headers.Add(name, value);
            }

            return this;
        }


        public HttpRequest AsHttpRequest()
        {
            var request = new DefaultHttpRequest(new DefaultHttpContext())
            {
                Method = _method
            };

            foreach (var header in _headers)
            {
                request.Headers.Add(header.Key, header.Value);
            }
            
            if (_body != null)
            {
                request.Body = new MemoryStream(_body);
            }

            return request;
        }
    }
}