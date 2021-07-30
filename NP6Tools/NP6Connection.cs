using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Reflection;
using System.Xml;
using System.Runtime.InteropServices;

using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml.Linq;
using System.Configuration;

namespace Mcd.App.GetXmlRpc
{
    public class NP6Connection
    {
        public string Url { get; set; }

        private static readonly int RequestTimeout = int.Parse(ConfigurationManager.AppSettings["RequestTimeout"]);
        public async Task<XmlDocument> SendRequestAsync(string dataRequest)
        {
            XmlDocument xmlDocument = new XmlDocument();

            xmlDocument.LoadXml(await ExecuteData2Async(dataRequest));
            return xmlDocument;

        }
        public async Task<XmlDocument> SendRequestAsync(XmlDocument dataRequest)
        {
            return await SendRequestAsync(dataRequest.OuterXml);
        }
        public async Task<string> ExecuteData2Async(string datarequest)
        {
            string responseMsg;
            using (HttpClient client = new HttpClient())
            {
                var content = new StringContent(datarequest, Encoding.ASCII, "text/xml");
                content.Headers.ContentType = MediaTypeHeaderValue.Parse("text/xml");


                // client.DefaultRequestHeaders.Add("Content-Type", "text/xml");
                client.DefaultRequestHeaders.Add("Connection", "keep-alive");
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/3.0 (compatible; Indy Library)");
                client.DefaultRequestHeaders.Add("Accept", "*/*");
                client.Timeout = TimeSpan.FromSeconds(RequestTimeout);
                
                var response = await client.PostAsync(this.Url, content);
                responseMsg = await response.Content.ReadAsStringAsync();
                XDocument xdoc = XDocument.Parse(responseMsg);
                

                return xdoc.ToString();

            }
            
        }

    }
}
