//using CookComputing.XmlRpc;
using System;
using System.Xml;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Reflection;

namespace Mcd.App.GetXmlRpc
{
    class NP6Client
    {
        private readonly string url;
        private readonly bool saveXML;
        private readonly XmlRpcClient client;

        public NP6Client(string AdressIP, bool SaveXML)
        {
            url = "http://" + AdressIP + ":8080//goform/RPC2";
            saveXML = SaveXML;
            client = new XmlRpcClient
            {
                Url = url
            };
            return;
        }
        public async Task<HourlySales> GetHourlySalesAsync()
        {
            HourlySales result=null;
            XmlRpcResponse responseLogin;
            XmlRpcRequest requestLogin = new XmlRpcRequest("datarequest");
            string appPath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            requestLogin.AddParams("HourlySales", "", "", "");
            try
            {
                responseLogin = await client.ExecuteAsync(requestLogin);
            }
            finally
            {
               //client.WriteRequest(appPath + "\\Request1.xml");
               // client.WriteResponse(appPath + "\\Response1.xml");
            }
            if (responseLogin.GetStruct().ContainsKey("id"))
            {
                XmlRpcRequest requestQuery = new XmlRpcRequest("Query");
                requestQuery.AddParams(responseLogin.GetStruct()["id"], "", "", "");

                XmlRpcResponse responseQuery = await client.ExecuteAsync(requestQuery);
                if (responseQuery.GetStruct().ContainsKey("payload"))
                {
                    string responsepayload = Encoding.UTF8.GetString((byte[])responseQuery.GetStruct()["payload"]);
                    try
                    {
                        XmlSerializer serializer = new XmlSerializer(typeof(HourlySales));

                        using (TextReader reader = new StringReader(responsepayload))
                        {
                            result = (HourlySales)serializer.Deserialize(reader);
                        }
                    }
                    catch
                    {
                        File.WriteAllText(appPath + "\\response_" + System.DateTime.Now.ToString("MM_dd_HH__mm_ss") + ".xml", responsepayload);
                    }
                }

            }
            return result;
        }
        public async Task<string> GetHourlySalesAsync(DateTime date, string outputPath)
        {
            XmlRpcResponse responseLogin;
            XmlRpcRequest requestLogin = new XmlRpcRequest("datarequest");
            string appPath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            string dateActivite = "";
            if (DateTime.Compare(date, DateTime.Now) != 0)
            {
                dateActivite = date.ToString("YYYYMMDD");
            }
            requestLogin.AddParams("HourlySales", dateActivite, "", "");
            try
            {
                responseLogin = await client.ExecuteAsync(requestLogin);
            }
            finally
            {
                //client.WriteRequest(appPath + "\\Request1.xml");
                // client.WriteResponse(appPath + "\\Response1.xml");
            }
            if (responseLogin.GetStruct().ContainsKey("id"))
            {
                XmlRpcRequest requestQuery = new XmlRpcRequest("Query");
                requestQuery.AddParams(responseLogin.GetStruct()["id"], "", "", "");

                XmlRpcResponse responseQuery = await client.ExecuteAsync(requestQuery);
                if (responseQuery.GetStruct().ContainsKey("payload"))
                {
                    string responsepayload = Encoding.UTF8.GetString((byte[])responseQuery.GetStruct()["payload"]);
                    string OutputFile = "HourlySales_" + System.DateTime.Now.ToString("MM_dd_HH__mm_ss") + ".xml";
                    File.WriteAllText(outputPath + "\\" + OutputFile, responsepayload);
                    return OutputFile;
                }
            }
            return "";
        }
        public async Task<string> GetPMXAsync(DateTime date, string outputPath)
        {
            XmlRpcResponse responseLogin;
            XmlRpcRequest requestLogin = new XmlRpcRequest("datarequest");
            string appPath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            string dateActivite = "";
            if (DateTime.Compare(date, DateTime.Now) != 0)
            {
                dateActivite = date.ToString("YYYYMMDD");
            }
            requestLogin.AddParams("PMix", dateActivite, "", "");
            try
            {
                responseLogin = await client.ExecuteAsync(requestLogin);
            }
            finally
            {
                //client.WriteRequest(appPath + "\\Request1.xml");
                // client.WriteResponse(appPath + "\\Response1.xml");
            }
            if (responseLogin.GetStruct().ContainsKey("id"))
            {
                XmlRpcRequest requestQuery = new XmlRpcRequest("Query");
                requestQuery.AddParams(responseLogin.GetStruct()["id"], "", "", "");

                XmlRpcResponse responseQuery = await client.ExecuteAsync(requestQuery);
                if (responseQuery.GetStruct().ContainsKey("payload"))
                {
                    string responsepayload = Encoding.UTF8.GetString((byte[])responseQuery.GetStruct()["payload"]);
                    string OutputFile = "PMX_" + System.DateTime.Now.ToString("MM_dd_HH__mm_ss") + ".xml";
                    File.WriteAllText(outputPath + "\\" + OutputFile, responsepayload);
                    return OutputFile;
                }
            }
            return "";
        }
    }
}
