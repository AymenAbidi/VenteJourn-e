using System;
using System.Xml;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace Mcd.App.GetXmlRpc
{

    /// <summary>
    /// XML-RPC client
    /// </summary>
    public class XmlRpcClient : NP6Connection
    {

        protected XmlDocument xmlRequest;
        protected XmlDocument xmlResponse;
        protected XmlRpcRequest request;
        protected XmlRpcResponse response;
        /// <summary>
        /// Gets the xml request
        /// </summary>
        /// <returns>The xml request</returns>
        public XmlDocument GetXmlRequest()
        {
            return xmlRequest;
        }

        /// <summary>
        /// Gets the xml response
        /// </summary>
        /// <returns>The xml response</returns>
        public XmlDocument GetXmlResponse()
        {
            return xmlResponse;
        }

        /// <summary>
        /// Writes the request
        /// </summary>
        /// <param name="fileName">File name</param>
        public void WriteRequest(String fileName)
        {
            TextWriter streamWriter = new StreamWriter(fileName, false, Encoding.UTF8);
            WriteRequest(streamWriter);
        }

        /// <summary>
        /// Writes the request
        /// </summary>
        /// <param name="outStream">Out stream</param>
        public void WriteRequest(TextWriter outStream)
        {
            xmlRequest.Save(outStream);
        }

        /// <summary>
        /// Writes the response
        /// </summary>
        /// <param name="fileName">File name</param>
        public void WriteResponse(String fileName)
        {
            TextWriter streamWriter = new StreamWriter(fileName, false, Encoding.UTF8);
            WriteResponse(streamWriter);
        }

        /// <summary>
        /// Writes the response
        /// </summary>
        /// <param name="outStream">Out stream</param>
        public void WriteResponse(TextWriter outStream)
        {
            if (xmlResponse != null)
              xmlResponse.Save(outStream);
        }

        /// <summary>
        /// Execute the specified methodName and parameters
        /// </summary>
        /// <param name="methodName">Method name</param>
        /// <param name="parameters">Parameters</param>
        public async Task<XmlRpcResponse> ExecuteAsync(string methodName, List<object> parameters)
        {
            return await ExecuteAsync(new XmlRpcRequest(methodName, parameters));
        }

        /// <summary>
        /// Execute the specified methodName and parameters
        /// </summary>
        /// <param name="methodName">Method name</param>
        /// <param name="parameters">Parameters</param>
        public async Task<XmlRpcResponse> ExecuteAsync(string methodName, object[] parameters)
        {
            return await ExecuteAsync(new XmlRpcRequest(methodName, parameters));
        }

        /// <summary>
        /// Execute the specified methodName
        /// </summary>
        /// <param name="methodName">Method name</param>
        public async Task<XmlRpcResponse> ExecuteAsync(string methodName)
        {
            return await ExecuteAsync(new XmlRpcRequest(methodName));
        }

        /// <summary>
        /// Execute the specified request
        /// </summary>
        /// <param name="request">Request</param>
        public virtual async Task<XmlRpcResponse> ExecuteAsync(XmlRpcRequest request)
        {
            this.request = request;

            XmlDocument xmlRequest = RequestFactory.BuildRequest(request);
            this.xmlRequest = xmlRequest;

            XmlDocument xmlResponse = await SendRequestAsync(xmlRequest);

            this.xmlResponse = xmlResponse;

            this.response = ResponseFactory.BuildResponse(xmlResponse);

            return response;
        }
        public async Task<XmlDocument> ExecuteFreeAsync(XmlRpcRequest request)
        {
            this.request = request;

            XmlDocument xmlRequest = RequestFactory.BuildRequest(request);
            this.xmlRequest = xmlRequest;

            XmlDocument xmlResponse = await SendRequestAsync(xmlRequest);

            this.xmlResponse = xmlResponse;

            return xmlResponse;
        }
        public async Task<HourlySales> ExecuteHourlySalesAsync(XmlRpcRequest request)
        {
            this.request = request;

            XmlDocument xmlRequest = RequestFactory.BuildRequest(request);
            this.xmlRequest = xmlRequest;
            XmlDocument xmlResponse = await SendRequestAsync(xmlRequest);
            this.xmlResponse = xmlResponse;

            XmlSerializer serializer = new XmlSerializer(typeof(HourlySales));
            HourlySales result;
            using (TextReader reader = new StringReader(xmlResponse.OuterXml))
            {
                result = (HourlySales)serializer.Deserialize(reader);
            }
            return result;
        }
    }
}

