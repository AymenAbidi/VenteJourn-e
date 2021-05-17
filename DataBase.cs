using log4net;
using Mcd.App.GetXmlRpc.DAL;
using Mcd.App.GetXmlRpc.Helpers;
using Mcd.App.GetXmlRpc.PMX;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Mcd.App.GetXmlRpc
{
    public class DataBase : IDisposable
    {
        //private readonly string server;
        //private readonly string database;
        //private readonly string tableName;
        private readonly McDashboardEntities _ctx;
        private readonly logger _logger;

        private List<APP_DDAY_HOURLY_SALES> HsFinalList;
        private List<APP_DDAY_HOURLY_PMX> HpFinalList;
        public DataBase(logger _logger)
        {
            //this.server = ConfigurationManager.AppSettings["Serveur"] ?? "(local)";
            //this.database = ConfigurationManager.AppSettings["DataBase"] ?? "test";
            //this.tableName = ConfigurationManager.AppSettings["Table"] ?? "test";
            _ctx = new McDashboardEntities();
            this._logger = _logger;
            HsFinalList = new List<APP_DDAY_HOURLY_SALES>();
            HpFinalList = new List<APP_DDAY_HOURLY_PMX>();
        }

        public void SaveHourlySales(HourlySales hourlySalesObjet, int? numResto = null, DateTime? dateActivity = null)
        {
            
            bool donationSales = false;

            for (int productIdx = hourlySalesObjet.ProductTable.ProductInfo.Count - 1; productIdx >= 0; productIdx--)
            {
                if (int.Parse(hourlySalesObjet.ProductTable.ProductInfo[productIdx].Id) == 11111)
                {
                    donationSales = true;
                }

            }

            /*try
            {
                // Suppression des données dans les tables CMU

                //clear table CMU_HOURLY_SALES

                //Récupération du total des ventes par 1/4 d'heure

                

                for (int salesIdx = 0; salesIdx < hourlySalesObjet.StoreTotal.Sales.Count; salesIdx++)
                {
                     var sales = hourlySalesObjet.StoreTotal.Sales[salesIdx];
                    
                     //var dayPartitioning = hourlySalesObjet.DayPartitioning.Segment[int.Parse(sales.Id)-1];

                    // Call Procedure SPW_CMU_SaveHourlySales
                }

                //Récupération des ventes par POD type et par 1/4 d'heure
                
                for (int podIdx = 0; podIdx < hourlySalesObjet.POD.Count; podIdx++)
                {
                    POD pod = hourlySalesObjet.POD[podIdx];

                    
                    for (int salesIdx = 0; salesIdx < pod.StoreTotal.Sales.Count; salesIdx++)
                    {
                        // Call Procedure SPW_CMU_SaveHourlySales

                        Sales sales = pod.StoreTotal.Sales[salesIdx];
                        var dayPartitioning = hourlySalesObjet.DayPartitioning.Segment[int.Parse(sales.Id)-1];

                        //Récupération des dons par POD type, par 1/4 d'heure et par type d'opération

                        if (donationSales)
                        {
                            for (int productIdx = sales.Product.Count; productIdx >= 0; productIdx--)
                            {
                                if (int.Parse(sales.Product[productIdx].Id) == 11111)
                                {
                                    for (int operationIdx = 0; operationIdx < sales.Product[productIdx].OperationType.Count; operationIdx++)
                                    {
                                        PMix pmix = sales.Product[productIdx].OperationType[operationIdx].PMix;

                                        // Call Procedure SPW_CMU_SaveHourlySalesPMX
                                    }
                                }
                            }
                        }
                    }
                }

                //Sauvegarde des ventes au 1/4 h et à l'heure

                //Call Procedure SPW_Cash_SaveHourlySales  A supprimer


                //Sauvegarde des ventes au 1/4 et par point de vente

                //if (SaveDetail) <= On utilise pas ce parametre comme dans la fonction Delphi
                //{
               
                //}
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw e;
            }*/

            Stopwatch retDataHs = new Stopwatch();
            retDataHs.Start();
            List<APP_DDAY_HOURLY_SALES> app_dday_hourly_sales = MapToSales(hourlySalesObjet,numResto);
            retDataHs.Stop();
            Console.WriteLine("************recuperation des données db depuis objet Hs :" + retDataHs.Elapsed);
            //    _ctx.APP_HOURLY_SALES.Add(app_hourly_sales);

            //    _ctx.SaveChanges();
            //}
            
            HsFinalList.AddRange(app_dday_hourly_sales);
            

            #region XML Sauvegarde
            //var objetTrouve = LireNP6(numResto, dateActivity);

            //if (objetTrouve != null)
            //{
            //    objetTrouve.HourlySales = doc.Document.ToString();
            //    objetTrouve.HourlySalesDateMAJ = DateTime.Now;

            //    //AddOrUpdate in same table
            //}
            //else
            //{

            //}

            //    //Si on un problème de connexion, on ajoute le code erreur dans ce champ (colonne remarque à ajouter)
            //    //Sinon on rempli par 200
            #endregion
        }

        public void SaveChanges()
        {
            Console.WriteLine($"Début du commit en BDD de # {HsFinalList.Count} enregistrements");
            _logger.Info($"Début du commit en BDD de # {HsFinalList.Count} enregistrements");

            using (var con = new SqlConnection("Server=(local);Database=McDashboard;Integrated Security=true"))
            {
                con.Open();
                Console.WriteLine("connexion version : "+con.ServerVersion);

                DataTable dt = new DataTable();
                DataColumn DDHP_SITE_ID = new DataColumn("DDHP_SITE_ID");
                DataColumn DDHP_BUSINESS_DT = new DataColumn("DDHP_BUSINESS_DT");
                DataColumn DDHP_PROD_ID = new DataColumn("DDHP_PROD_ID");
                DataColumn DDHP_SALES_TM = new DataColumn("DDHP_SALES_TM");
                DataColumn DDHP_MCDE_SIR_ID = new DataColumn("DDHP_MCDE_SIR_ID");
                DataColumn DDHP_MVAL_SIR_ID = new DataColumn("DDHP_MVAL_SIR_ID");
                DataColumn DDHP_LLVR_SIR_ID = new DataColumn("DDHP_LLVR_SIR_ID");
                DataColumn DDHP_EAT_IN_QY = new DataColumn("DDHP_EAT_IN_QY");
                DataColumn DDHP_TAKE_OUT_QY = new DataColumn("DDHP_TAKE_OUT_QY");
                DataColumn DDHP_PROMO_IN_QY = new DataColumn("DDHP_PROMO_IN_QY");
                DataColumn DDHP_PROMO_OUT_QY = new DataColumn("DDHP_PROMO_OUT_QY");
                DataColumn DDHP_DISCOUNT_IN_QY = new DataColumn("DDHP_DISCOUNT_IN_QY");
                DataColumn DDHP_DISCOUNT_OUT_QY = new DataColumn("DDHP_DISCOUNT_OUT_QY");
                DataColumn DDHP_EMPLOYEE_MEAL_QY = new DataColumn("DDHP_EMPLOYEE_MEAL_QY");
                DataColumn DDHP_MGR_MEAL_QY = new DataColumn("DDHP_MGR_MEAL_QY");
                DataColumn DDHP_EMPLOYEE_MEAL_AM = new DataColumn("DDHP_EMPLOYEE_MEAL_AM");
                DataColumn DDHP_CA_IN_AM = new DataColumn("DDHP_CA_IN_AM");
                DataColumn DDHP_CA_OUT_AM = new DataColumn("DDHP_CA_OUT_AM");
                DataColumn DDHP_PROCESS_DT = new DataColumn("DDHP_PROCESS_DT");

                dt.Columns.Add(DDHP_SITE_ID);
                dt.Columns.Add(DDHP_BUSINESS_DT);
                dt.Columns.Add(DDHP_PROD_ID);
                dt.Columns.Add(DDHP_SALES_TM);
                dt.Columns.Add(DDHP_MCDE_SIR_ID);
                dt.Columns.Add(DDHP_MVAL_SIR_ID);
                dt.Columns.Add(DDHP_LLVR_SIR_ID);
                dt.Columns.Add(DDHP_EAT_IN_QY);
                dt.Columns.Add(DDHP_TAKE_OUT_QY);
                dt.Columns.Add(DDHP_PROMO_IN_QY);
                dt.Columns.Add(DDHP_PROMO_OUT_QY);
                dt.Columns.Add(DDHP_DISCOUNT_IN_QY);
                dt.Columns.Add(DDHP_DISCOUNT_OUT_QY);
                dt.Columns.Add(DDHP_EMPLOYEE_MEAL_QY);
                dt.Columns.Add(DDHP_MGR_MEAL_QY);
                dt.Columns.Add(DDHP_EMPLOYEE_MEAL_AM);
                dt.Columns.Add(DDHP_CA_IN_AM);
                dt.Columns.Add(DDHP_CA_OUT_AM);
                dt.Columns.Add(DDHP_PROCESS_DT);

                HpFinalList.ForEach((APP_DDAY_HOURLY_PMX hp) =>
                { DataRow d = dt.NewRow();
                    d["DDHP_SITE_ID"] = hp.DDHP_SITE_ID;
                    d["DDHP_BUSINESS_DT"] = hp.DDHP_BUSINESS_DT;
                    d["DDHP_PROD_ID"] = hp.DDHP_PROD_ID;
                    d["DDHP_SALES_TM"] = hp.DDHP_SALES_TM;
                    d["DDHP_MCDE_SIR_ID"] = hp.DDHP_MCDE_SIR_ID;
                    d["DDHP_MVAL_SIR_ID"] = hp.DDHP_MVAL_SIR_ID;
                    d["DDHP_LLVR_SIR_ID"] = hp.DDHP_LLVR_SIR_ID;
                    d["DDHP_EAT_IN_QY"] = hp.DDHP_EAT_IN_QY;
                    d["DDHP_TAKE_OUT_QY"] = hp.DDHP_TAKE_OUT_QY;
                    d["DDHP_PROMO_IN_QY"] = hp.DDHP_PROMO_IN_QY;
                    d["DDHP_PROMO_OUT_QY"] = hp.DDHP_PROMO_OUT_QY;
                    d["DDHP_DISCOUNT_IN_QY"] = hp.DDHP_DISCOUNT_IN_QY;
                    d["DDHP_DISCOUNT_OUT_QY"] = hp.DDHP_DISCOUNT_OUT_QY;
                    d["DDHP_EMPLOYEE_MEAL_QY"] = hp.DDHP_EMPLOYEE_MEAL_QY;
                    d["DDHP_MGR_MEAL_QY"] = hp.DDHP_MGR_MEAL_QY;
                    d["DDHP_EMPLOYEE_MEAL_AM"] = hp.DDHP_EMPLOYEE_MEAL_AM;
                    d["DDHP_CA_IN_AM"] = hp.DDHP_CA_IN_AM;
                    d["DDHP_CA_OUT_AM"] = hp.DDHP_CA_OUT_AM;
                    d["DDHP_PROCESS_DT"] = hp.DDHP_PROCESS_DT;

                    dt.Rows.Add(d);
                });

                using (SqlCommand cmd = new SqlCommand("exec sp_insert_ddhp_hourly_pmx @list", con))
                {


                    Console.WriteLine("1111111111");
                    var pList = new SqlParameter("@list", SqlDbType.Structured);
                    pList.TypeName = "dbo.APP_DDAY_HOURLY_PMX_TYPE";
                    pList.Value = dt;
                    Console.WriteLine("222222");
                    cmd.Parameters.Add(pList);
                    Console.WriteLine("333333");
                    try { var dr = cmd.ExecuteReader(); }
                    catch(Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    /*using (var dr = cmd.ExecuteReader())
                        {
                        Console.WriteLine("444444");
                        while (dr.Read())
                        {
                            Console.WriteLine("5555555");
                            Console.WriteLine(dr);
                        }
                        }*/
                    
                }
            }

            /* try {
                 Stopwatch addDbhs = new Stopwatch();
                 addDbhs.Start();
                 _ctx.APP_DDAY_HOURLY_SALES.AddRange(HsFinalList);
                 addDbhs.Stop();
                 Console.WriteLine("************ajout hs dans la table db :" + addDbhs.Elapsed);
                 Stopwatch addDbhp = new Stopwatch();
                 addDbhp.Start();
                 _ctx.APP_DDAY_HOURLY_PMX.AddRange(HpFinalList);
                 addDbhp.Stop();
                 Console.WriteLine("************ajout hp dans la table db :" + addDbhp.Elapsed);
             }
             catch (Exception e)
             {
                 Console.WriteLine(e);
             }


             try {
                 Stopwatch comdb = new Stopwatch();
                 comdb.Start();
                 _ctx.SaveChanges();
                 comdb.Stop();
                 Console.WriteLine("************commit db :" + comdb.Elapsed);
             }
             catch(Exception e)
             {
                 Console.WriteLine(e);
             }*/


            _logger.Info($"Fin du commit en BDD de # {HsFinalList.Count} enregistrements ");
            HsFinalList = new List<APP_DDAY_HOURLY_SALES>();
        }

        public void SavePmix(HourlyPMX hourlyPMX , DayPartitioning dayPartitioning ,int numResto)
        {
            /*var objetTrouve = LireNP6(numResto, dateActivity);

            if (objetTrouve != null)
            {
                objetTrouve.PMIX = doc.Document.ToString();
                objetTrouve.PmixDateMAJ = DateTime.Now;
            }
            else
            {
                //INSERT INTO dbo.T_Pages(NumRestaurant, DayActivity, HourlySales, HourlySalesDateMAJ) 
                //VALUES(@numResto, @dateActivity, @doc, GETDATE()); ";
            }

            //    //Si on un problème de connexion, on ajoute le code erreur dans ce champ (colonne remarque à ajouter)
            //    //Sinon on rempli par 200

            _ctx.SaveChanges();*/

            #region Connextion par ConnectionString
            //using (var con = new SqlConnection(string.Format("Server=(local);Database=TempDb;Integrated Security=true", server, database)))
            //{
            //    con.Open();
            //    string query = @"IF EXISTS(SELECT * FROM dbo.{0} WHERE NumRestaurant = @numResto and DayActivity = @dateActivity)
            //            UPDATE dbo.{0} 
            //            SET HourlySales = @doc, HourlySalesDateMAJ = GETDATE()
            //            WHERE NumRestaurant = @numResto and DayActivity = @dateActivity
            //        ELSE
            //            INSERT INTO dbo.T_Pages(NumRestaurant, DayActivity, HourlySales, HourlySalesDateMAJ) VALUES(@numResto, @dateActivity, @doc, GETDATE());";

            //    //Si on un problème de connexion, on ajoute le code erreur dans ce champ (colonne remarque à ajouter)


            //    var cmdInsertXml = new SqlCommand(string.Format(query, tableName), con);
            //    cmdInsertXml.Parameters.Add("@numResto", SqlDbType.Int).Value = numResto;
            //    cmdInsertXml.Parameters.Add("@dateActivity", SqlDbType.Date).Value = dateActivity;
            //    var pDoc = cmdInsertXml.Parameters.Add("@doc", System.Data.SqlDbType.Xml);

            //    pDoc.Value = doc.CreateReader();

            //    return (cmdInsertXml.ExecuteNonQuery() > 0);
            //}
            #endregion

            Stopwatch retDataHp = new Stopwatch();
            retDataHp.Start();

            List<APP_DDAY_HOURLY_PMX> list = MapToPMX(hourlyPMX, dayPartitioning, numResto);

            retDataHp.Stop();
            Console.WriteLine("************recuperation des données db depuis objet Hp :" + retDataHp.Elapsed);
            HpFinalList.AddRange(list);

            
           
            
        }

        private List<APP_DDAY_HOURLY_SALES> MapToSales(HourlySales hourlySales , int? numResto=null)
        {
            

            List<APP_DDAY_HOURLY_SALES> app_dday_hourly_sales = new List<APP_DDAY_HOURLY_SALES>();

          
            hourlySales.DayPartitioning.Segment.ForEach((Segment segment) =>
            {
                
                short  HSA_SALES_TM = short.Parse(segment.BegTime);
                decimal HSA_SALES_PROD_AM = 0;
                decimal HSA_SALES_NON_PROD_AM = 0;
                decimal HSA_EAT_IN_SALES_AM = 0;
                decimal HSA_TAKE_OUT_SALES_AM = 0;
                short HSA_EAT_IN_TAC_QY = 0;
                short HSA_TAKE_OUT_TAC_QY = 0;
                short HSA_DISCOUNT_IN_TAC_QY = 0;
                short HSA_DISCOUNT_OUT_TAC_QY = 0;
                decimal HSA_DISCOUNT_IN_SALES_AM = 0;
                decimal HSA_DISCOUNT_OUT_SALES_AM = 0;

                Random random = new Random();

                hourlySales.StoreTotal.Sales.ForEach((Sales sales) =>
                {
                    if (sales.Id == segment.Id)
                    {
                        HSA_SALES_PROD_AM += decimal.Parse(sales.ProductNetAmount);
                        HSA_SALES_NON_PROD_AM += decimal.Parse(sales.NetAmount) - HSA_SALES_PROD_AM;
                        sales.Product.ForEach((Product product) => {
                            product.OperationType.ForEach((OperationType OpT) =>
                            {
                                if (OpT.operationType == "DISCOUNT")
                                {
                                    HSA_DISCOUNT_IN_SALES_AM += decimal.Parse(OpT.PMix.EatInNetAmount);
                                    HSA_DISCOUNT_OUT_SALES_AM += decimal.Parse(OpT.PMix.TakeOutNetAmount);
                                    HSA_DISCOUNT_IN_TAC_QY += short.Parse(OpT.PMix.QtyEatIn);
                                    HSA_DISCOUNT_OUT_TAC_QY += short.Parse(OpT.PMix.QtyTakeOut);
                                }
                                else if(OpT.operationType =="SALE")
                                {
                                    HSA_EAT_IN_SALES_AM += decimal.Parse(OpT.PMix.EatInNetAmount);
                                    HSA_TAKE_OUT_SALES_AM += decimal.Parse(OpT.PMix.TakeOutNetAmount);
                                    HSA_EAT_IN_TAC_QY += short.Parse(OpT.PMix.QtyEatIn);
                                    HSA_TAKE_OUT_TAC_QY += short.Parse(OpT.PMix.QtyTakeOut);
                                }
                                
                            });
                        
                        });
                    }

                    

                });
                
                APP_DDAY_HOURLY_SALES app_hourly_sale = new APP_DDAY_HOURLY_SALES
                {
                    
                    DDHS_SITE_ID = (short)numResto,
                    DDHS_MCDE_SIR_ID = (short)random.Next(100,999),
                    DDHS_MVAL_SIR_ID = (short)random.Next(100, 999),
                    DDHS_LLVR_SIR_ID = (short)random.Next(100, 999),
                    DDHS_SALES_TM = HSA_SALES_TM,
                    DDHS_SALES_PROD_AM = HSA_SALES_PROD_AM,
                    DDHS_SALES_NON_PROD_AM = HSA_SALES_NON_PROD_AM,
                    DDHS_EAT_IN_SALES_AM = HSA_EAT_IN_SALES_AM,
                    DDHS_TAKE_OUT_SALES_AM = HSA_TAKE_OUT_SALES_AM,
                    DDHS_EAT_IN_TAC_QY = HSA_EAT_IN_TAC_QY,
                    DDHS_TAKE_OUT_TAC_QY = HSA_TAKE_OUT_TAC_QY,
                    DDHS_DISCOUNT_IN_TAC_QY = HSA_DISCOUNT_IN_TAC_QY,
                    DDHS_DISCOUNT_OUT_TAC_QY = HSA_DISCOUNT_OUT_TAC_QY,
                    DDHS_DISCOUNT_IN_SALES_AM = HSA_DISCOUNT_IN_SALES_AM,
                    DDHS_DISCOUNT_OUT_SALES_AM = HSA_DISCOUNT_OUT_SALES_AM,
                   

                    DDHS_BUSINESS_DT = DateTime.ParseExact(!string.IsNullOrEmpty(hourlySales.POS.BusinessDay) ?
                                                    hourlySales.POS.BusinessDay : "00000000", "yyyyMMdd",
                                                    CultureInfo.InvariantCulture),
                    
                };

                app_dday_hourly_sales.Add(app_hourly_sale);
            });
            return app_dday_hourly_sales;

        }

        private List<APP_DDAY_HOURLY_PMX> MapToPMX(HourlyPMX hourlyPMX, DayPartitioning dayPartitioning, int? numResto = null)
        {


            List<APP_DDAY_HOURLY_PMX> app_dday_hourly_pmx = new List<APP_DDAY_HOURLY_PMX>();

            
            dayPartitioning.Segment.ForEach((Segment segment) =>
            {
                
                short DDHP_SALES_TM = short.Parse(segment.BegTime);
                int DDHP_PROD_ID = 0;
                short DDHP_EAT_IN_QY = 0;
                short DDHP_TAKE_OUT_QY = 0;
                short DDHP_PROMO_IN_QY = 0;
                short DDHP_PROMO_OUT_QY = 0;
                short DDHP_DISCOUNT_IN_QY = 0;
                short DDHP_DISCOUNT_OUT_QY = 0;
                short DDHP_EMPLOYEE_MEAL_QY = 0;
                short DDHP_MGR_MEAL_QY = 0;
                decimal DDHP_EMPLOYEE_MEAL_AM = 0;
                decimal DDHP_CA_IN_AM = 0;
                decimal DDHP_CA_OUT_AM = 0;

                Random random = new Random();

                hourlyPMX.FamilyGroup.ForEach((FamilyGroup familyGroup) =>
                {
                    familyGroup.Product.ForEach((PMX.Product product) =>
                    {
                        DDHP_PROD_ID = product.Id;
                        product.OperationType.ForEach((PMX.OperationType opT) =>
                            {
                                opT.Price.ForEach((Price price) =>
                                {
                                    if (price.SaleTime.ToString() == segment.Id)
                                    {

                                        DDHP_CA_IN_AM += (decimal)opT.PMix.NetAmtEatIn;
                                        DDHP_CA_OUT_AM += (decimal)opT.PMix.NetAmtTakeOut;
                                        if (opT.operationType == "SALE")
                                        {
                                            DDHP_EAT_IN_QY += (short)opT.PMix.QtyEatIn;
                                            DDHP_TAKE_OUT_QY += (short)opT.PMix.QtyTakeOut;
                                        }
                                        if (opT.operationType == "PROMO")
                                        {
                                            DDHP_PROMO_IN_QY += (short)opT.PMix.QtyEatIn;
                                            DDHP_PROMO_OUT_QY += (short)opT.PMix.QtyTakeOut;
                                        }
                                        if (opT.operationType == "DISCOUNT")
                                        {
                                            DDHP_DISCOUNT_IN_QY += (short)opT.PMix.QtyEatIn;
                                            DDHP_DISCOUNT_OUT_QY += (short)opT.PMix.QtyTakeOut;
                                        }
                                        if (opT.operationType == "CREW")
                                        {
                                            DDHP_EMPLOYEE_MEAL_QY += (short)opT.PMix.QtyEatIn;
                                            DDHP_EMPLOYEE_MEAL_AM += (short)opT.PMix.NetAmtEatIn + (short)opT.PMix.NetAmtTakeOut;


                                        }
                                        if (opT.operationType == "MANAGER")
                                        {
                                            DDHP_MGR_MEAL_QY += (short)opT.PMix.QtyEatIn;

                                        }
                                    }
                                });

                            });
                        APP_DDAY_HOURLY_PMX app_hourly_pmx = new APP_DDAY_HOURLY_PMX
                        {

                            DDHP_SITE_ID = (short)numResto,
                            DDHP_MCDE_SIR_ID = (byte)random.Next(100, 999),
                            DDHP_MVAL_SIR_ID = (byte)random.Next(100, 999),
                            DDHP_LLVR_SIR_ID = (byte)random.Next(100, 999),
                            DDHP_SALES_TM = DDHP_SALES_TM,
                            DDHP_PROD_ID = DDHP_PROD_ID,
                            DDHP_EAT_IN_QY = DDHP_EAT_IN_QY,
                            DDHP_TAKE_OUT_QY = DDHP_TAKE_OUT_QY,
                            DDHP_PROMO_IN_QY = DDHP_PROMO_IN_QY,
                            DDHP_PROMO_OUT_QY = DDHP_PROMO_OUT_QY,
                            DDHP_DISCOUNT_IN_QY = DDHP_DISCOUNT_IN_QY,
                            DDHP_DISCOUNT_OUT_QY = DDHP_DISCOUNT_OUT_QY,
                            DDHP_EMPLOYEE_MEAL_QY = DDHP_EMPLOYEE_MEAL_QY,
                            DDHP_MGR_MEAL_QY = DDHP_MGR_MEAL_QY,
                            DDHP_EMPLOYEE_MEAL_AM = DDHP_EMPLOYEE_MEAL_AM,
                            DDHP_CA_IN_AM = DDHP_CA_IN_AM,
                            DDHP_CA_OUT_AM = DDHP_CA_OUT_AM,


                            DDHP_BUSINESS_DT = DateTime.ParseExact("20181003", "yyyyMMdd",
                                                        CultureInfo.InvariantCulture),
                            DDHP_PROCESS_DT = DateTime.Now
                        };

                        app_dday_hourly_pmx.Add(app_hourly_pmx);

                    });

                    /*Console.WriteLine("-------------------------------------");
                    Console.WriteLine("DDHP_SALES_TM :" + DDHP_SALES_TM);
                    Console.WriteLine("DDHP_PROD_ID :" + DDHP_PROD_ID);
                    Console.WriteLine("DDHP_EAT_IN_QY :" + DDHP_EAT_IN_QY);
                    Console.WriteLine("DDHP_TAKE_OUT_QY :" + DDHP_TAKE_OUT_QY);
                    Console.WriteLine("DDHP_PROMO_IN_QY :" + DDHP_PROMO_IN_QY);
                    Console.WriteLine("DDHP_PROMO_OUT_QY :" + DDHP_PROMO_OUT_QY);
                    Console.WriteLine("DDHP_DISCOUNT_IN_QY :" + DDHP_DISCOUNT_IN_QY);
                    Console.WriteLine("DDHP_DISCOUNT_OUT_QY :" + DDHP_DISCOUNT_OUT_QY);
                    Console.WriteLine("DDHP_EMPLOYEE_MEAL_QY :" + DDHP_EMPLOYEE_MEAL_QY);
                    Console.WriteLine("DDHP_MGR_MEAL_QY :" + DDHP_MGR_MEAL_QY);
                    Console.WriteLine("DDHP_EMPLOYEE_MEAL_AM :" + DDHP_EMPLOYEE_MEAL_AM);
                    Console.WriteLine("DDHP_CA_IN_AM :" + DDHP_CA_IN_AM);
                    Console.WriteLine("DDHP_CA_OUT_AM :" + DDHP_CA_OUT_AM);*/

                    
                });
            });
                return app_dday_hourly_pmx;

            }
        

        private NP6XML LireNP6(int numResto, DateTime dateActivity)
        {
            return _ctx.NP6XML.FirstOrDefault(n => n.NumRestaurant == numResto && n.DayActivity == dateActivity);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
                _ctx.Dispose();
        }
    }
}
