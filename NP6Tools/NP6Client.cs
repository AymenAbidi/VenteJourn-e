using Mcd.App.GetXmlRpc.Helpers;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Mcd.App.GetXmlRpc
{
    class NP6Client
    {
        private readonly string url;

        private readonly bool saveXML;

        private readonly XmlRpcClient client;

        private static readonly string xmlFilesPath = ConfigurationManager.AppSettings["xmlFilesPath"];

        private static readonly bool logXmlRpcReq = bool.Parse(ConfigurationManager.AppSettings["LogXML-RPC-Req"]);

        public NP6Client(string AdressIP, string Port, bool SaveXML)
        {
            url = $"http://{AdressIP}:{Port}//goform/RPC2";
            saveXML = SaveXML;
            client = new XmlRpcClient
            {
                Url = url
            };
            return;
        }

        public async Task<string> SauvegarderHourlySalesAsync(DateTime date, logger _logger, int numResto)
        {
            if (logXmlRpcReq)
            {
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
                    _logger.Debug("Durée : Requete login xmlrpc HourlySales " + hsReqLogin.Elapsed);
                    Console.Write("requestLogin executé \n");
                  
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
                        _logger.Debug("Durée : Requete Data xmlrpc HourlySales " + hsReqq.Elapsed);
                        _logger.Debug($"SauvegarderHourlySalesAsync.responseQuery = {responseQuery.GetStruct()}", numResto);

                        if (responseQuery.GetStruct().ContainsKey("payload"))
                        {
                            Console.Write("SauvegarderHourlySalesAsync.responseQuery contains payload key \n");
                            _logger.Debug($"SauvegarderHourlySalesAsync.responseQuery contains payload key", numResto);
                            Stopwatch doc = new Stopwatch();
                            doc.Start();
                            string responsepayload = Encoding.UTF8.GetString((byte[])responseQuery.GetStruct()["payload"]);
                            /*string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());

                            if (responsepayload.StartsWith(_byteOrderMarkUtf8))
                            {
                                Console.WriteLine(_byteOrderMarkUtf8);
                                responsepayload = responsepayload.Remove(0, _byteOrderMarkUtf8.Length);
                            }*/
                            Console.Write(responsepayload[0]);
                            responsepayload = responsepayload.Substring(responsepayload.IndexOf("<Response"));
                            Console.Write(responsepayload[0]);
                            var xmlDoc = XDocument.Load(new StringReader(responsepayload));
                            string path = $"{xmlFilesPath}\\{numResto}_{logger.dateExecution}_{Guid.NewGuid()}.xml";
                            Console.Write($"Chemin où on va déplacer le contenu xml : {path} \n");
                            xmlDoc.Save(path);
                            doc.Stop();
                            _logger.Debug("Durée : Chargement du fichier xml dans le disque :" + doc.Elapsed);
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
            }
            else
            {
                try
                {
                    XmlRpcRequest requestLogin = new XmlRpcRequest("datarequest");
                    string dateActivite = (date.Date != DateTime.Now.Date) ? date.ToString("yyyyMMdd") : "";
                    requestLogin.AddParams("HourlySales", dateActivite, "", "");
                    XmlRpcResponse responseLogin = Task.Run(async () => await client.ExecuteAsync(requestLogin)).Result;
                    if (responseLogin.GetStruct().ContainsKey("id"))
                    {

                        XmlRpcRequest requestQuery = new XmlRpcRequest("Query");
                        requestQuery.AddParams(responseLogin.GetStruct()["id"], "", "", "");
                        XmlRpcResponse responseQuery = await client.ExecuteAsync(requestQuery);


                        if (responseQuery.GetStruct().ContainsKey("payload"))
                        {


                            string responsepayload = Encoding.UTF8.GetString((byte[])responseQuery.GetStruct()["payload"]);
                            /*string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());

                            if (responsepayload.StartsWith(_byteOrderMarkUtf8))
                            {
                                Console.WriteLine(_byteOrderMarkUtf8);
                                responsepayload = responsepayload.Remove(0, _byteOrderMarkUtf8.Length);
                            }*/
                            Console.Write(responsepayload[0]);
                            responsepayload = responsepayload.Substring(responsepayload.IndexOf("<Response"));
                            Console.Write(responsepayload[0]);
                            var xmlDoc = XDocument.Load(new StringReader(responsepayload));
                            string path = $"{xmlFilesPath}\\{numResto}_{logger.dateExecution}_{Guid.NewGuid()}.xml";

                            xmlDoc.Save(path);


                            return path;
                        }

                    }
                }
                catch (Exception ex)
                {
                    Console.Write($"Erreur lors la génération du chemin {ex.Message} \n");
                    _logger.Error($"Erreur lors la génération du chemin", ex, numResto);
                    return null;
                }


                return null;
            }
        }

        public async Task<XDocument> SauvegarderHourlySalesAsyncDoc(DateTime date, logger _logger, int numResto)
        {
            if (logXmlRpcReq)
            {
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
                    _logger.Debug("Durée : Requete login xmlrpc HourlySales " + hsReqLogin.Elapsed);
                    Console.Write("requestLogin executé \n");
                   
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
                        _logger.Debug("Dureé : Requete query xmlrpc HourlySales " + hsReqq.Elapsed);
                        _logger.Debug($"SauvegarderHourlySalesAsync.responseQuery = {responseQuery.GetStruct()}", numResto);

                        if (responseQuery.GetStruct().ContainsKey("payload"))
                        {
                            Console.Write("SauvegarderHourlySalesAsync.responseQuery contains payload key \n");
                            _logger.Debug($"SauvegarderHourlySalesAsync.responseQuery contains payload key", numResto);
                            Stopwatch doc = new Stopwatch();
                            doc.Start();
                            string responsepayload = Encoding.UTF8.GetString((byte[])responseQuery.GetStruct()["payload"]);
                            /*string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
                            
                            if (responsepayload.StartsWith(_byteOrderMarkUtf8)) {
                                Console.WriteLine(_byteOrderMarkUtf8);   
                             responsepayload = responsepayload.Remove(0, _byteOrderMarkUtf8.Length); }*/
                            Console.Write(responsepayload[0]);
                            responsepayload = responsepayload.Substring(responsepayload.IndexOf("<Response"));
                            Console.Write(responsepayload[0]);

                            TextReader tr = new StringReader(responsepayload);
                            return XDocument.Load(tr);
                        }
                        Console.Write("SauvegarderHourlySalesAsync.responseQuery DOES NOT contain payload key \n");
                        _logger.Debug($"SauvegarderHourlySalesAsync.responseQuery DOES NOT contain payload key", numResto);
                    }
                }
                catch (Exception ex)
                {
                    Console.Write($"Erreur lors la récuperation du XML { ex.Message} \n");
                    _logger.Error($"Erreur lors la récuperation du XML", ex, numResto);
                    return null;
                }
                Console.Write($"SauvegarderHourlySalesAsync returning null \n");
                _logger.Debug($"SauvegarderHourlySalesAsync returning null", numResto);

                return null;
            }
            else
            {
                try
                {

                    XmlRpcRequest requestLogin = new XmlRpcRequest("datarequest");
                    string dateActivite = (date.Date != DateTime.Now.Date) ? date.ToString("yyyyMMdd") : "";
                    requestLogin.AddParams("HourlySales", dateActivite, "", "");


                    Stopwatch hsReqLogin = new Stopwatch();
                    hsReqLogin.Start();
                    XmlRpcResponse responseLogin = Task.Run(async () => await client.ExecuteAsync(requestLogin)).Result;
                    hsReqLogin.Stop();


                    if (responseLogin.GetStruct().ContainsKey("id"))
                    {

                        XmlRpcRequest requestQuery = new XmlRpcRequest("Query");
                        requestQuery.AddParams(responseLogin.GetStruct()["id"], "", "", "");

                        XmlRpcResponse responseQuery = await client.ExecuteAsync(requestQuery);


                        if (responseQuery.GetStruct().ContainsKey("payload"))
                        {


                            string responsepayload = Encoding.UTF8.GetString((byte[])responseQuery.GetStruct()["payload"]);
                            /* string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());

                             if (responsepayload.StartsWith(_byteOrderMarkUtf8))
                             {
                                 Console.WriteLine(_byteOrderMarkUtf8);
                                 responsepayload = responsepayload.Remove(0, _byteOrderMarkUtf8.Length);
                             }*/
                            Console.Write(responsepayload[0]);
                            responsepayload = responsepayload.Substring(responsepayload.IndexOf("<Response"));
                            Console.Write(responsepayload[0]);
                            TextReader tr = new StringReader(responsepayload);
                            return XDocument.Load(tr);
                        }

                    }
                }
                catch (Exception ex)
                {
                    Console.Write($"Erreur lors la récuperation du XML { ex.Message} \n");
                    _logger.Error($"Erreur lors la récuperation du XML", ex, numResto);
                    return null;
                }


                return null;
            }
        }
    }
}
