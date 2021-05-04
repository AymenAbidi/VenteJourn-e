using log4net;
using Mcd.App.GetXmlRpc.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Mcd.App.GetXmlRpc
{
    class Program
    {
        private static DataBase database;
        private static readonly string successPath =  ConfigurationManager.AppSettings["successPath"];
        private static readonly string errorPath =  ConfigurationManager.AppSettings["errorPath"];
        private static readonly string NP6Port =  ConfigurationManager.AppSettings["NP6Port"];
        private static readonly bool isDeplacer =  bool.Parse(ConfigurationManager.AppSettings["isDeplacer"]);
        private static readonly logger _logger = new logger();
        private static int processRepeat = int.Parse(ConfigurationManager.AppSettings["processRepeat"]);
        static void Main(string[] args)
        {
            GlobalContext.Properties["LogFileName"] = ConfigurationManager.AppSettings["logPath"]; //log file path
            log4net.Config.XmlConfigurator.Configure();
            database = new DataBase(_logger);
            //6min10dec pour 1200fichiers   =>  5min
            if (args.Count() == 0)
            {
                List<string> Restaurants = File.ReadAllLines("Restaurants.txt")
                                               .Where(r => !string.IsNullOrEmpty(r))
                                               .ToList();
                if(processRepeat > 0)
                    Restaurants = Enumerable.Repeat(Restaurants.FirstOrDefault(), processRepeat).ToList();

                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();
                _logger.Info($"Début process global de {Restaurants.Count} restaurants");

                var splitRestaurants = SplitList(Restaurants, 600);
                var tasks = new List<Task>();
                int i = 0;

                foreach (var splitResto in splitRestaurants)
                {
                    Console.WriteLine($"splitResto count : {splitResto.Count}");
                    Parallel.ForEach(splitResto, resto =>
                    {
                        int j = i;
                        Console.WriteLine($"Processing resto index {j}");
                        tasks.Add(Task.Run(() =>
                        {
                            int? mockIndex = null;
                            if (processRepeat > 0) mockIndex = int.Parse(resto.Trim()) + j ;
                            callResto(Convert.ToInt32(resto.Trim()), DateTime.Now, mockIndex);
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
                callResto(Convert.ToInt32(args[0].Trim()), DateTime.Parse(args[1]));

            Console.WriteLine("Tapez Entrer pour quitter...");
            Console.ReadLine();
        }

        async private static void callResto(int numResto, DateTime dateActivity, int? mockIndex = null)
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

                string path = await NP6.SauvegarderHourlySalesAsync(dateActivity, _logger, mocknumResto);

                Console.WriteLine($"Chemin généré : {path} \n");

                _logger.Info($"Chemin fichier sauvegardé : {path}", mocknumResto);

                processXML(path, Ip, mocknumResto);
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
#region PMX
#if !DEBUG
            try
            {
                NP6Client NP6 = new NP6Client(Ip, NP6Port, true);
                _logger.Info("Start Request: Produits PMX", mocknumResto);
                XDocument docPmix = await NP6.GetPMXAsync(dateActivity);
                database.SavePmix(numResto, dateActivity, docPmix);
                _logger.Info("Récupération: Produits PMX", mocknumResto);
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Erreur lors de récupération des Produits PMX: {0}", ex.Message), ex, numResto);
            }
#endif
#endregion
        }

        private static void processXML(string path, string Ip, int numResto)
        {
            Stopwatch stopWatch = new Stopwatch();

            try
            {
                stopWatch.Start();

                _logger.Info($"Début du process XML-RPC pour le resto # {numResto}", numResto);

                XmlSerializer serializer = new XmlSerializer(typeof(HourlySales));

                bool isProcessed = false;

                using (FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (BufferedStream bufferedStream = new BufferedStream(fileStream))
                    {
                        using (StreamReader streamReader = new StreamReader(bufferedStream))
                        {
                            StringReader rdr = new StringReader(streamReader.ReadToEnd());
                            var hourlySalesObjet = (HourlySales)serializer.Deserialize(rdr);

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
                                isProcessed = true;
                            }
                        }
                    }
                }
                    

                deplacerXML(path, isProcessed);
                return;
            }
           catch(Exception ex)
            {
                _logger.Error($"Erreur lors du process du fichier XML, chemin : {path}, Ip : {Ip}", ex, numResto);
                deplacerXML(path, false);
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

        private static void deplacerXML(string path, bool isSuccess)
        {
            string nomFichier = Path.GetFileName(path);
#if !DEBUG
            if(isSuccess)
            {
                if (isDeplacer)
                    File.Move(path, $"{successPath}\\{nomFichier}");
                else
                    File.Delete(path);
            }
            else
                File.Move(path, $"{errorPath}\\{nomFichier}");
#endif
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
