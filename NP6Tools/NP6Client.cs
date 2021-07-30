using Mcd.App.GetXmlRpc.Helpers;
using System;
using System.Collections.Generic;
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
        
        private static readonly int WaitTime = int.Parse(ConfigurationManager.AppSettings["WaitTime"]);


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
                string message = "Erreur : requête datarequest HourlySales non abouti";
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
                    _logger.Debug("Durée : Requete login xmlrpc HourlySales" ,numResto , hsReqLogin.Elapsed);
                    Console.Write("requestLogin executé \n");
                  
                    if (responseLogin.GetStruct().ContainsKey("id"))
                    {
                        Console.WriteLine("JOB ID :"+responseLogin.GetStruct()["id"]);
                        _logger.Debug("LOGIN - JOD ID :" + responseLogin.GetStruct()["id"] + " - Fault Code :" + responseLogin.GetFaultCode() + " , Message erreur :" + responseLogin.GetFaultString(),numResto);
                        Console.Write("SauvegarderHourlySalesAsync.responseLogin contains id key \n");
                        _logger.Debug($"SauvegarderHourlySalesAsync.responseLogin contains id key", numResto);

                        XmlRpcRequest requestQuery = new XmlRpcRequest("Query");
                        requestQuery.AddParams(responseLogin.GetStruct()["id"], "", "", "");
                        Stopwatch hsReqq = new Stopwatch();
                        hsReqq.Start();
                        XmlRpcResponse responseQuery = await client.ExecuteAsync(requestQuery);
                       
                        if (responseQuery.GetFaultCode() != -1) {
                            _logger.Error("Erreur dans la requête XML-RPC , Code erreur :"+responseQuery.GetFaultCode()+" , Message erreur :"+responseQuery.GetFaultString(), null, numResto);
                        }
                        
                        hsReqq.Stop();
                        _logger.Debug("Durée : Requete Data xmlrpc HourlySales " ,numResto, hsReqq.Elapsed);
                        _logger.Debug($"SauvegarderHourlySalesAsync.responseQuery = {responseQuery.GetStruct()}", numResto);

                        if (responseQuery.GetStruct().ContainsKey("faultCode"))
                        {
                            _logger.Debug($"Job id : {responseLogin.GetStruct()["id"]} fault code : {responseLogin.GetStruct()["faultCode"]}",numResto);
                            while (responseQuery.GetStruct()["faultCode"].ToString() == "99")
                            {
                                Stopwatch sw = new Stopwatch();
                                sw.Start();

                                while (sw.ElapsedMilliseconds < WaitTime*1000)
                                {
                                   // Console.WriteLine(sw.ElapsedMilliseconds);

                                }
                                responseQuery = await client.ExecuteAsync(requestQuery);
                                sw.Stop();
                                if (!responseQuery.GetStruct().ContainsKey("faultCode")) break;
                            }
                        }
                        if (responseQuery.GetStruct().ContainsKey("payload"))
                        {
                            message = $"Requête datarequest HourlySales réussi JOB ID : {responseLogin.GetStruct()["id"]} et requête Query réussi";
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
                           
                            responsepayload = responsepayload.Substring(responsepayload.IndexOf("<Response"));
                            
                            var xmlDoc = XDocument.Load(new StringReader(responsepayload));
                            string path = $"{xmlFilesPath}\\{numResto}_{logger.dateExecution}_{Guid.NewGuid()}.xml";
                            Console.Write($"Chemin où on va déplacer le contenu xml : {path} \n");
                            xmlDoc.Save(path);
                            doc.Stop();
                            _logger.Debug("Durée : Chargement du fichier xml dans le disque",numResto, doc.Elapsed);
                            return path;
                        }
                        
                       
                        Console.Write("SauvegarderHourlySalesAsync.responseQuery DOES NOT contain payload key \n");
                        _logger.Debug($"SauvegarderHourlySalesAsync.responseQuery DOES NOT contain payload key", numResto);
                    }
                    
                }
                catch (Exception ex)
                {
                    Console.Write($"Erreur lors de la requête xmlrpc : {message} {ex.Message} \n");
                    _logger.Error($"Erreur lors de la requête xmlrpc : {message}", ex, numResto);
                    return null;
                }
                Console.Write($"SauvegarderHourlySalesAsync returning null \n");
                _logger.Debug($"SauvegarderHourlySalesAsync returning null", numResto);

                return null;
            }
            else
            {
                string message = "Erreur : requête datarequest HourlySales non abouti";
                try
                {
                    

                    XmlRpcRequest requestLogin = new XmlRpcRequest("datarequest");
                    string dateActivite = (date.Date != DateTime.Now.Date) ? date.ToString("yyyyMMdd") : "";
                    requestLogin.AddParams("HourlySales", dateActivite, "", "");

                    Console.Write("execution requestLogin \n");
                    Stopwatch hsReqLogin = new Stopwatch();
                    hsReqLogin.Start();

                    XmlRpcResponse responseLogin = Task.Run(async () => await client.ExecuteAsync(requestLogin)).Result;

                    hsReqLogin.Stop();
                   
                    Console.Write("requestLogin executé \n");

                    if (responseLogin.GetStruct().ContainsKey("id"))
                    {
                        Console.WriteLine("JOB ID :" + responseLogin.GetStruct()["id"]);
                        Console.Write("SauvegarderHourlySalesAsync.responseLogin contains id key \n");

                        XmlRpcRequest requestQuery = new XmlRpcRequest("Query");
                        requestQuery.AddParams(responseLogin.GetStruct()["id"], "", "", "");
                        Stopwatch hsReqq = new Stopwatch();
                        hsReqq.Start();
                        XmlRpcResponse responseQuery = await client.ExecuteAsync(requestQuery);

                        if (responseQuery.GetFaultCode() != -1)
                        {
                            _logger.Error("Erreur dans la requête XML-RPC , Code erreur :" + responseQuery.GetFaultCode() + " , Message erreur :" + responseQuery.GetFaultString(), null, numResto);
                        }

                        hsReqq.Stop();
                       
                        if (responseQuery.GetStruct().ContainsKey("faultCode"))
                        {
                           while (responseQuery.GetStruct()["faultCode"].ToString() == "99")
                            {
                                Stopwatch sw = new Stopwatch();
                                sw.Start();

                                while (sw.ElapsedMilliseconds < WaitTime * 1000)
                                {
                                    // Console.WriteLine(sw.ElapsedMilliseconds);

                                }
                                responseQuery = await client.ExecuteAsync(requestQuery);
                                sw.Stop();
                                if (!responseQuery.GetStruct().ContainsKey("faultCode")) break;
                            }
                        }
                        if (responseQuery.GetStruct().ContainsKey("payload"))
                        {
                            message = $"Requête datarequest HourlySales réussi JOB ID : {responseLogin.GetStruct()["id"]} et requête Query réussi";
                            Console.Write("SauvegarderHourlySalesAsync.responseQuery contains payload key \n");
                            Stopwatch doc = new Stopwatch();
                            doc.Start();
                            string responsepayload = Encoding.UTF8.GetString((byte[])responseQuery.GetStruct()["payload"]);
                            /*string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());

                            if (responsepayload.StartsWith(_byteOrderMarkUtf8))
                            {
                                Console.WriteLine(_byteOrderMarkUtf8);
                                responsepayload = responsepayload.Remove(0, _byteOrderMarkUtf8.Length);
                            }*/

                            responsepayload = responsepayload.Substring(responsepayload.IndexOf("<Response"));

                            var xmlDoc = XDocument.Load(new StringReader(responsepayload));
                            string path = $"{xmlFilesPath}\\{numResto}_{logger.dateExecution}_{Guid.NewGuid()}.xml";
                            Console.Write($"Chemin où on va déplacer le contenu xml : {path} \n");
                            xmlDoc.Save(path);
                            doc.Stop();
                            return path;
                        }


                        Console.Write("SauvegarderHourlySalesAsync.responseQuery DOES NOT contain payload key \n");
                    }

                }
                catch (Exception ex)
                {
                    Console.Write($"Erreur lors de la requête xmlrpc : {message} {ex.Message} \n");
                    _logger.Error($"Erreur lors de la requête xmlrpc : {message}", ex, numResto);
                    return null;
                }
                Console.Write($"SauvegarderHourlySalesAsync returning null \n");

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
                    _logger.Debug("Durée : Requete login xmlrpc HourlySales ",numResto, hsReqLogin.Elapsed);
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
                        if (responseQuery.GetFaultCode() != -1)
                        {
                            _logger.Error("Erreur dans la requête XML-RPC , Code erreur :" + responseQuery.GetFaultCode() + " , Message erreur :" + responseQuery.GetFaultString(), null, numResto);
                        }
                        hsReqq.Stop();
                        _logger.Debug("Dureé : Requete query xmlrpc HourlySales " ,numResto, hsReqq.Elapsed);
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
                            
                            responsepayload = responsepayload.Substring(responsepayload.IndexOf("<Response"));
                            

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
                        if (responseQuery.GetFaultCode() != -1)
                        {
                            _logger.Error("Erreur dans la requête XML-RPC , Code erreur :" + responseQuery.GetFaultCode() + " , Message erreur :" + responseQuery.GetFaultString(), null, numResto);
                        }

                        if (responseQuery.GetStruct().ContainsKey("payload"))
                        {


                            string responsepayload = Encoding.UTF8.GetString((byte[])responseQuery.GetStruct()["payload"]);
                            /* string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());

                             if (responsepayload.StartsWith(_byteOrderMarkUtf8))
                             {
                                 Console.WriteLine(_byteOrderMarkUtf8);
                                 responsepayload = responsepayload.Remove(0, _byteOrderMarkUtf8.Length);
                             }*/
                            
                            responsepayload = responsepayload.Substring(responsepayload.IndexOf("<Response"));
                            
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
