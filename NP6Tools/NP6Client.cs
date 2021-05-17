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
using System.Xml.Linq;
using Mcd.App.GetXmlRpc.Helpers;
using System.Configuration;
using System.Threading;
using System.Diagnostics;

namespace Mcd.App.GetXmlRpc
{
    class NP6Client
    {
        private readonly string url;
        private readonly bool saveXML;
        private readonly XmlRpcClient client;
        private static readonly string xmlFilesPath = ConfigurationManager.AppSettings["xmlFilesPath"];


        public NP6Client(string AdressIP, string Port, bool SaveXML)
        {
            url = $"http://{AdressIP}:{Port}//goform/RPC2";
            saveXML = SaveXML;
            client = new XmlRpcClient
            {
                Url = "http://127.0.0.1:5678/"
            };
            return;
        }
        public async Task<HourlySales> GetHourlySalesAsync()
        {
            HourlySales result=null;
            XmlRpcResponse responseLogin;
            XmlRpcRequest requestLogin = new XmlRpcRequest("datarequest");
            Console.WriteLine(requestLogin);
            string appPath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            
            requestLogin.AddParams("HourlySales", "", "", "");
           
            try
            {
                
                responseLogin = await client.ExecuteAsync(requestLogin);
                
            }
          
            
            finally
            {  
               client.WriteRequest(appPath + "\\Request1.xml");
               client.WriteResponse(appPath + "\\Response1.xml");
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
        public async Task<string> SauvegarderHourlySalesAsync(DateTime date, logger _logger, int numResto)
        {
//if DEBUG
            //return $"{xmlFilesPath}\\datarequest-HourlySales-Response-Payload.xml";
//#else
            try
            {
                _logger.Debug($"Début Sauvegarde des données des ventes horaires (Fn:SauvegarderHourlySalesAsync)", numResto);
                XmlRpcRequest requestLogin = new XmlRpcRequest("datarequest");
                string dateActivite = (date.Date != DateTime.Now.Date) ? date.ToString("yyyyMMdd") : "";
                requestLogin.AddParams("HourlySales", dateActivite, "", "");
                
                Console.Write("execution requestLogin \n");
                Stopwatch hsReqLogin = new Stopwatch();
                hsReqLogin.Start();
                XmlRpcResponse responseLogin = Task.Run(async () => await client.ExecuteAsync(requestLogin)).Result;
                hsReqLogin.Stop();
                Console.WriteLine("************ Requete login xmlrpc HourlySales " + hsReqLogin.Elapsed);
                Console.Write("requestLogin executé \n");
                Console.Write($"responseLogin : {responseLogin} \n \n \n \n");
                Console.WriteLine(responseLogin.GetStruct().Count);
                if (responseLogin.GetStruct().ContainsKey("id"))
                {
                    Console.Write("SauvegarderHourlySalesAsync.responseLogin contains id key \n");
                    _logger.Debug($"SauvegarderHourlySalesAsync.responseLogin contains id key", numResto);

                    XmlRpcRequest requestQuery = new XmlRpcRequest("Query");
                    requestQuery.AddParams(responseLogin.GetStruct()["id"], "", "", "");
                    Stopwatch hsReqq = new Stopwatch();
                    hsReqq.Start();
                    XmlRpcResponse responseQuery = await client.ExecuteAsync(requestQuery);
                    hsReqq.Stop();
                    Console.WriteLine("************ Requete query xmlrpc HourlySales " + hsReqq.Elapsed);
                    _logger.Debug($"SauvegarderHourlySalesAsync.responseQuery = {responseQuery.GetStruct()}", numResto);

                    if (responseQuery.GetStruct().ContainsKey("payload"))
                    {
                        Console.Write("SauvegarderHourlySalesAsync.responseQuery contains payload key \n");
                        _logger.Debug($"SauvegarderHourlySalesAsync.responseQuery contains payload key", numResto);
                        Stopwatch doc = new Stopwatch();
                        doc.Start();
                        string responsepayload = Encoding.UTF8.GetString((byte[])responseQuery.GetStruct()["payload"]);
                        var xmlDoc = XDocument.Load(new StringReader(responsepayload));
                        string path = $"{xmlFilesPath}\\{numResto}_{logger.dateExecution}_{Guid.NewGuid()}.xml";
                        Console.Write($"Chemin où on va déplacer le contenu xml : {path} \n");
                        xmlDoc.Save(path);
                        doc.Stop();
                        Console.WriteLine("**************** Chargement du fichier xml dans le disque :" + doc.Elapsed);
                        return path;
                    }
                    Console.Write("SauvegarderHourlySalesAsync.responseQuery DOES NOT contain payload key \n");
                    _logger.Debug($"SauvegarderHourlySalesAsync.responseQuery DOES NOT contain payload key", numResto);
                }
            }
            catch (Exception ex)
            {
                Console.Write($"Erreur lors la génération du chemin {ex.Message} \n");
                _logger.Error($"Erreur lors la génération du chemin", ex, numResto);
                return null;
            }
            Console.Write($"SauvegarderHourlySalesAsync returning null \n");
            _logger.Debug($"SauvegarderHourlySalesAsync returning null", numResto);

            return null;
//#endif
        }


        public async Task<XDocument> SauvegarderHourlySalesAsyncDoc(DateTime date, logger _logger, int numResto)
        {
            //if DEBUG
            //return $"{xmlFilesPath}\\datarequest-HourlySales-Response-Payload.xml";
            //#else
            try
            {
                _logger.Debug($"Début Sauvegarde des données des ventes horaires (Fn:SauvegarderHourlySalesAsync)", numResto);
                XmlRpcRequest requestLogin = new XmlRpcRequest("datarequest");
                string dateActivite = (date.Date != DateTime.Now.Date) ? date.ToString("yyyyMMdd") : "";
                requestLogin.AddParams("HourlySales", dateActivite, "", "");

                Console.Write("execution requestLogin \n");
                Stopwatch hsReqLogin = new Stopwatch();
                hsReqLogin.Start();
                XmlRpcResponse responseLogin = Task.Run(async () => await client.ExecuteAsync(requestLogin)).Result;
                hsReqLogin.Stop();
                Console.WriteLine("************ Requete login xmlrpc HourlySales " + hsReqLogin.Elapsed);
                Console.Write("requestLogin executé \n");
                Console.Write($"responseLogin : {responseLogin} \n \n \n \n");
                Console.WriteLine(responseLogin.GetStruct().Count);
                if (responseLogin.GetStruct().ContainsKey("id"))
                {
                    Console.Write("SauvegarderHourlySalesAsync.responseLogin contains id key \n");
                    _logger.Debug($"SauvegarderHourlySalesAsync.responseLogin contains id key", numResto);

                    XmlRpcRequest requestQuery = new XmlRpcRequest("Query");
                    requestQuery.AddParams(responseLogin.GetStruct()["id"], "", "", "");
                    Stopwatch hsReqq = new Stopwatch();
                    hsReqq.Start();
                    XmlRpcResponse responseQuery = await client.ExecuteAsync(requestQuery);
                    hsReqq.Stop();
                    Console.WriteLine("************ Requete query xmlrpc HourlySales " + hsReqq.Elapsed);
                    _logger.Debug($"SauvegarderHourlySalesAsync.responseQuery = {responseQuery.GetStruct()}", numResto);

                    if (responseQuery.GetStruct().ContainsKey("payload"))
                    {
                        Console.Write("SauvegarderHourlySalesAsync.responseQuery contains payload key \n");
                        _logger.Debug($"SauvegarderHourlySalesAsync.responseQuery contains payload key", numResto);
                        Stopwatch doc = new Stopwatch();
                        doc.Start();
                        string responsepayload = Encoding.UTF8.GetString((byte[])responseQuery.GetStruct()["payload"]);
                        TextReader tr = new StringReader(responsepayload);
                        return XDocument.Load(tr);
                    }
                    Console.Write("SauvegarderHourlySalesAsync.responseQuery DOES NOT contain payload key \n");
                    _logger.Debug($"SauvegarderHourlySalesAsync.responseQuery DOES NOT contain payload key", numResto);
                }
            }
            catch (Exception ex)
            {
                Console.Write($"Erreur lors la génération du chemin {ex.Message} \n");
                _logger.Error($"Erreur lors la génération du chemin", ex, numResto);
                return null;
            }
            Console.Write($"SauvegarderHourlySalesAsync returning null \n");
            _logger.Debug($"SauvegarderHourlySalesAsync returning null", numResto);

            return null;
            //#endif
        }

        public async Task<string> GetPMXAsync(DateTime date, int numResto)
        {
//#if DEBUG
            //return $"{xmlFilesPath}\\Pmix1.xml";
//#else
            XmlRpcResponse responseLogin;
            XmlRpcRequest requestLogin = new XmlRpcRequest("datarequest");
            string appPath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            string dateActivite = "";
            if (date.Date != DateTime.Now.Date)
            {
                dateActivite = date.ToString("yyyyMMdd");
            }
            requestLogin.AddParams("PMix", dateActivite, "", "");
            Stopwatch hpreql = new Stopwatch();
            hpreql.Start();
            try
            {
                responseLogin = await client.ExecuteAsync(requestLogin);
                hpreql.Stop();
                Console.WriteLine("************* requete login pmx : " + hpreql.Elapsed);
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
                Stopwatch hpreqq = new Stopwatch();
                hpreqq.Start();
                XmlRpcResponse responseQuery = await client.ExecuteAsync(requestQuery);
                hpreqq.Stop();
                Console.WriteLine("************* requete données pmx : " + hpreqq.Elapsed);
                if (responseQuery.GetStruct().ContainsKey("payload"))
                {
                    string responsepayload = Encoding.UTF8.GetString((byte[])responseQuery.GetStruct()["payload"]);
                    //TextReader tr = new StringReader(responsepayload);
                    //return XDocument.Load(tr);
                    Stopwatch hpdoc = new Stopwatch();
                    hpdoc.Start();
                    var xmlDoc = XDocument.Load(new StringReader(responsepayload));
                    string path = $"{xmlFilesPath}\\{numResto}_{logger.dateExecution}_{Guid.NewGuid()}.xml";
                    Console.Write($"Chemin où on va déplacer le contenu xml : {path} \n");
                    xmlDoc.Save(path);
                    hpdoc.Stop();
                    Console.WriteLine("************* chargement fichier pmx dans le disque: " + hpdoc.Elapsed);
                    return path;
                }
            }
            return null;
//#endif
        }

        public async Task<XDocument> GetPMXAsyncDoc(DateTime date, int numResto)
        {
            //#if DEBUG
            //return $"{xmlFilesPath}\\Pmix1.xml";
            //#else
            XmlRpcResponse responseLogin;
            XmlRpcRequest requestLogin = new XmlRpcRequest("datarequest");
            string appPath = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);
            string dateActivite = "";
            if (date.Date != DateTime.Now.Date)
            {
                dateActivite = date.ToString("yyyyMMdd");
            }
            requestLogin.AddParams("PMix", dateActivite, "", "");
            Stopwatch hpreql = new Stopwatch();
            hpreql.Start();
            try
            {
                responseLogin = await client.ExecuteAsync(requestLogin);
                hpreql.Stop();
                Console.WriteLine("************* requete login pmx : " + hpreql.Elapsed);
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
                Stopwatch hpreqq = new Stopwatch();
                hpreqq.Start();
                XmlRpcResponse responseQuery = await client.ExecuteAsync(requestQuery);
                hpreqq.Stop();
                Console.WriteLine("************* requete données pmx : " + hpreqq.Elapsed);
                if (responseQuery.GetStruct().ContainsKey("payload"))
                {
                    string responsepayload = Encoding.UTF8.GetString((byte[])responseQuery.GetStruct()["payload"]);
                    TextReader tr = new StringReader(responsepayload);
                    return XDocument.Load(tr);
                    
                }
            }
            return null;
            //#endif
        }
    }
}
