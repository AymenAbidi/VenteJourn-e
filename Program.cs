using log4net;
using Mcd.App.GetXmlRpc.Helpers;
using Mcd.App.GetXmlRpc.PMX;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Mcd.App.GetXmlRpc
{
    class Program
    {
        private static DataBase database;

        private static readonly logger _logger = new logger();

        private static readonly string NP6Port = ConfigurationManager.AppSettings["NP6Port"];

        private static readonly bool SaveXML = bool.Parse(ConfigurationManager.AppSettings["SaveXML"]);

        private static readonly int ThreadSplit = int.Parse(ConfigurationManager.AppSettings["ThreadSplit"]);

        private static readonly bool logXmlToData = bool.Parse(ConfigurationManager.AppSettings["LogXML-To-DATA"]);

        private static int processRepeat = int.Parse(ConfigurationManager.AppSettings["processRepeat"]);

        static void Main(string[] args)
        {
            GlobalContext.Properties["LogFileName"] = ConfigurationManager.AppSettings["logPath"]; //log file path
            log4net.Config.XmlConfigurator.Configure();
            database = new DataBase(_logger);

            if (args.Count() == 0)
            {

                List<string> Restaurants = File.ReadAllLines("Restaurants.txt")
                                               .Where(r => !string.IsNullOrEmpty(r))
                                               .ToList();


                if (processRepeat > 0)
                    Restaurants = Enumerable.Repeat(Restaurants.FirstOrDefault(), processRepeat).ToList();

                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                _logger.Info($"Début process global de {Restaurants.Count} restaurants");

                var splitRestaurants = SplitList(Restaurants, ThreadSplit);
                var tasks = new List<Task>();
                int i = 0;

                foreach (var splitResto in splitRestaurants)
                {

                    Console.WriteLine($"splitResto count : {splitResto.Count}");
                    Parallel.ForEach(splitResto, resto =>
                    {
                        int j = i;
                        Console.WriteLine($"Processing resto index {j}");
                        tasks.Add(Task.Run(async () =>
                        {
                            int? mockIndex = null;
                            if (processRepeat > 0) mockIndex = int.Parse(resto.Trim()) + j;
                            callResto(Convert.ToInt32(resto.Trim()), DateTime.Now, mockIndex).Wait();
                            Console.WriteLine($"Processing resto index {j} Finished");

                        }));
                        i++;
                    });
                    Task.WaitAll(tasks.ToArray());

                    tasks = new List<Task>();
                }

                try
                {
                    _logger.Info($"Ajout des nouveaux enregistrements dans la BDD");

                    database.SaveChanges();
                }
                catch (Exception ex)
                {
                    while (ex.InnerException != null) ex = ex.InnerException;
                    _logger.Error(string.Format($"Erreur lors de sauvegarde des enregistrements : {ex.Message}"), ex);
                }
                stopWatch.Stop();

                _logger.Info($"Fin process global", null, stopWatch.Elapsed);
            }
            else
                callResto(Convert.ToInt32(args[0].Trim()), DateTime.Parse(args[1])).Wait();

            Console.WriteLine("Tapez Entrer pour quitter...");
            Console.ReadLine();
        }

        async private static Task callResto(int numResto, DateTime dateActivity, int? mockIndex = null)
        {
            int mocknumResto = mockIndex != null ? mockIndex.Value : numResto;
            string Ip = String.Empty;

            Console.WriteLine($"callResto execution");

            Stopwatch stopWatch = new Stopwatch();

            stopWatch.Start();
            try
            {
                _logger.Info($"Début Appel XML pour le resto # {mocknumResto}", mocknumResto);


                Ip = getAdresseWayStation(numResto);

                _logger.Info($"addresse IP : {Ip}", mocknumResto);


                NP6Client NP6 = new NP6Client(Ip, NP6Port, true);

                Console.WriteLine($"NP6 initialisé, addresse Ip : {Ip} \n");

                // Sauvegarde des fichiers XML sur le disque
                if (SaveXML)
                {
                    string path = await NP6.SauvegarderHourlySalesAsync(dateActivity, _logger, mocknumResto);


                    Console.WriteLine($"Chemin HourlySales généré : {path} \n");


                    _logger.Info($"Chemin fichier HourlySales sauvegardé : {path}", mocknumResto);


                    processXML(path, Ip, mocknumResto);

                }
                //Traitement direct des fichiers XML
                else
                {
                    XDocument hourlySalesDoc = await NP6.SauvegarderHourlySalesAsyncDoc(dateActivity, _logger, mocknumResto);
                    Console.WriteLine("Document XML HourlySales récupéré");
                    _logger.Info("Document XML HourlySales récupéré");


                    processXMLDoc(hourlySalesDoc, Ip, mocknumResto);
                }


            }
            catch (Exception ex)
            {
                while (ex.InnerException != null) ex = ex.InnerException;
                _logger.Error(string.Format($"Erreur lors de récupération des ventes horaires: {ex.Message}"),
                              ex, numResto);
            }
            finally
            {
                stopWatch.Stop();
                _logger.Info($"Fin du traitement pour le resto # {mockIndex}, durée du traitement : {stopWatch.Elapsed}",
                                mockIndex, stopWatch.Elapsed);
            }
        }

        private static void processXMLDoc(XDocument hourlySales, string Ip, int numResto)
        {
            Stopwatch stopWatch = new Stopwatch();

            try
            {
                stopWatch.Start();

                _logger.Info($"Début du process XML-RPC pour le resto # {numResto}", numResto);

                XmlSerializer serializer = new XmlSerializer(typeof(HourlySales));
                XmlSerializer PMXserializer = new XmlSerializer(typeof(HourlyPMX));
                HourlySales hourlySalesObjet = new HourlySales();


                Stopwatch desHs = new Stopwatch();
                desHs.Start();

                try
                {
                    hourlySalesObjet = (HourlySales)serializer.Deserialize(hourlySales.Root.CreateReader());
                    desHs.Stop();
                    if (logXmlToData)
                    {
                        _logger.Debug("Durée : Deserialisation du fichier hourlysales en objet :" + desHs.Elapsed);
                    }

                }
                catch (Exception ex)
                {
                    _logger.Error("Erreur lors de la Deserialisation du fichier XML en objet", ex);
                }

                if (hourlySalesObjet == null)
                {
                    _logger.Warn(string.Format($"Erreur lors de récupération Ventes Horaires ({0})", Ip), numResto);
                }
                else if (hourlySalesObjet.RequestReport != "HOURLYSALES")
                {
                    _logger.Warn(string.Format($"RequestReport = {hourlySalesObjet.RequestReport} " + $"quand ca doit être HOURLYSALES"), numResto);
                }
                else
                {
                    database.SaveHourlySales(hourlySalesObjet, numResto);
                }

                return;
            }
            catch (Exception ex)
            {
                _logger.Error($"Erreur lors du process du fichier XML, chemin Ip : {Ip}", ex, numResto);
                Console.WriteLine(ex);

                throw ex;
            }
            finally
            {
                stopWatch.Stop();
                _logger.Info($"Fin du process XML-RPC pour le resto # {numResto}, durée du traitement : {stopWatch.Elapsed}",
                                numResto, stopWatch.Elapsed);
            }
        }

        private static void processXML(string path, string Ip, int numResto)
        {
            Stopwatch stopWatch = new Stopwatch();

            try
            {
                stopWatch.Start();

                _logger.Info($"Début du process XML-RPC pour le resto # {numResto}", numResto);

                XmlSerializer serializer = new XmlSerializer(typeof(HourlySales));

                HourlySales hourlySalesObjet = new HourlySales();



                Stopwatch desHs = new Stopwatch();
                desHs.Start();
                using (FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (BufferedStream bufferedStream = new BufferedStream(fileStream))
                    {
                        using (StreamReader streamReader = new StreamReader(bufferedStream))
                        {
                            StringReader rdr = new StringReader(streamReader.ReadToEnd());
                            try
                            {
                                hourlySalesObjet = (HourlySales)serializer.Deserialize(rdr);

                                desHs.Stop();
                                if (logXmlToData)
                                {
                                    _logger.Debug("Durée : Deserialisation du fichier hourlysales en objet :" + desHs.Elapsed);
                                }

                            }
                            catch (Exception ex)
                            {
                                _logger.Error("Erreur lors de la Deserialisation du fichier XML en objet", ex);
                            }



                            if (hourlySalesObjet == null)
                            {
                                _logger.Warn(string.Format($"Erreur lors de récupération Ventes Horaires ({0})", Ip), numResto);
                            }
                            else if (hourlySalesObjet.RequestReport != "HOURLYSALES")
                            {
                                _logger.Warn(string.Format($"RequestReport = {hourlySalesObjet.RequestReport} " + $"quand ca doit être HOURLYSALES"), numResto);
                            }
                            else
                            {
                                database.SaveHourlySales(hourlySalesObjet, numResto);

                            }
                        }
                    }
                }


                return;
            }
            catch (Exception ex)
            {
                _logger.Error($"Erreur lors du process du fichier XML, chemin : {path}, Ip : {Ip}", ex, numResto);
                Console.WriteLine(ex);
                throw ex;
            }
            finally
            {
                stopWatch.Stop();
                _logger.Info($"Fin du process XML-RPC pour le resto # {numResto}, durée du traitement : {stopWatch.Elapsed}",
                                numResto, stopWatch.Elapsed);
            }
        }

        private static string getAdresseWayStation(int iNumResto)
        {
            if (iNumResto < 1536)
            {
                return "10." + (19 + Math.Truncate((double)iNumResto / 256)) + "." + (iNumResto - (Math.Truncate((double)iNumResto / 256) * 256)) + ".71";
            }
            else
            {
                return "10." + (-2 + Math.Truncate((double)iNumResto / 256)) + "." + (iNumResto - (Math.Truncate((double)iNumResto / 256) * 256)) + ".71";
            }
        }

        private static IEnumerable<List<string>> SplitList(List<string> restaurants, int nSize = 30)
        {
            for (int i = 0; i < restaurants.Count; i += nSize)
            {
                yield return restaurants.GetRange(i, Math.Min(nSize, restaurants.Count - i));
            }
        }
    }
}
