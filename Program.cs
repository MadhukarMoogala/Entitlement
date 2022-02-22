
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace Entitlement
{
    class Program
    {
        public const string _baseApiUrl = @"apps.exchange.autodesk.com";
        public const string _resourcePath = @"webservices/checkentitlement";       
        
        static async Task Main(string[] args)
        {
            if(args.Length != 2)
            {
                Console.WriteLine(@"Usage:
                Entitlement.exe <userId> <appId>
                ex: Entitlement.exe GS5ZNVMPUZKX 8510204064326138084");
                return;
            }
            string userId = args[0];
            string appId =  args[1];
            var queryParams = new NameValueCollection()
            {
                { "userid",userId },
                { "appid", appId }
            };

            var uriBuilder = new UriBuilder
            {
                Scheme = "https",
                Host = _baseApiUrl,
                Path = _resourcePath,
                Query = ToQueryString(queryParams)
            };
            Console.WriteLine(await new HttpClient().GetStringAsync(uriBuilder.Uri));
        }
        public static string ToQueryString(NameValueCollection nvc)
        {
            StringBuilder sb = new StringBuilder();

            bool first = true;

            foreach (string key in nvc.AllKeys)
            {
                foreach (string value in nvc.GetValues(key))
                {
                    if (!first)
                    {
                        sb.Append("&");
                    }

                    sb.AppendFormat("{0}={1}", Uri.EscapeDataString(key), Uri.EscapeDataString(value));

                    first = false;
                }
            }

            return sb.ToString();
        }        
    }
}
