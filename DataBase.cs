using log4net;
using Mcd.App.GetXmlRpc.DAL;
using Mcd.App.GetXmlRpc.Helpers;
using Mcd.App.GetXmlRpc.PMX;
using System;
using System.Collections.Generic;
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

        private List<APP_DDAY_HOURLY_SALES> finalList;
        public DataBase(logger _logger)
        {
            //this.server = ConfigurationManager.AppSettings["Serveur"] ?? "(local)";
            //this.database = ConfigurationManager.AppSettings["DataBase"] ?? "test";
            //this.tableName = ConfigurationManager.AppSettings["Table"] ?? "test";
            _ctx = new McDashboardEntities();
            this._logger = _logger;
            finalList = new List<APP_DDAY_HOURLY_SALES>();
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
           
            try
            {
                // Suppression des données dans les tables CMU

                //clear table CMU_HOURLY_SALES

                //Récupération du total des ventes par 1/4 d'heure

                

                for (int salesIdx = 0; salesIdx < hourlySalesObjet.StoreTotal.Sales.Count; salesIdx++)
                {
                     var sales = hourlySalesObjet.StoreTotal.Sales[salesIdx];
                    
                     var dayPartitioning = hourlySalesObjet.DayPartitioning.Segment[int.Parse(sales.Id)-1];

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
            }

            
            List<APP_DDAY_HOURLY_SALES> app_dday_hourly_sales = MapToSales(hourlySalesObjet,numResto);

            //    _ctx.APP_HOURLY_SALES.Add(app_hourly_sales);

            //    _ctx.SaveChanges();
            //}
            Console.WriteLine("*******************");
            Console.WriteLine(app_dday_hourly_sales.Count);
            Console.WriteLine(numResto);
            finalList.AddRange(app_dday_hourly_sales);
            

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
            Console.WriteLine($"Début du commit en BDD de # {finalList.Count} enregistrements");
            _logger.Info($"Début du commit en BDD de # {finalList.Count} enregistrements");

            Console.WriteLine("11111111111111111");
            
            try { _ctx.APP_DDAY_HOURLY_SALES.AddRange(finalList); }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            
            Console.WriteLine("222222222222222222");
            try { _ctx.SaveChanges(); }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            Console.WriteLine("333333333333333");
            Console.WriteLine(_ctx.Set<APP_DDAY_HOURLY_SALES>());
            _logger.Info($"Fin du commit en BDD de # {finalList.Count} enregistrements ");
            finalList = new List<APP_DDAY_HOURLY_SALES>();
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

            List<APP_DDAY_HOURLY_PMX> list = MapToPMX(hourlyPMX, dayPartitioning, numResto);

           

            Console.WriteLine("PMX --- 11111111111111111");

            try { _ctx.APP_DDAY_HOURLY_PMX.AddRange(list); }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine("PMX --- 222222222222222222");
            try { _ctx.SaveChanges(); }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            Console.WriteLine("PMX --- 333333333333333");
        }

        private List<APP_DDAY_HOURLY_SALES> MapToSales(HourlySales hourlySales , int? numResto=null)
        {
            /*decimal HSA_EAT_IN_TAC_QY = 0;
            decimal HSA_TAKE_OUT_TAC_QY = 0;
            decimal HSA_EAT_IN_SALES_AM = 0;
            decimal HSA_TAKE_OUT_SALES_AM = 0;
            hourlySales.StoreTotal.Sales.ForEach((Sales s) =>
            {

                HSA_EAT_IN_SALES_AM += decimal.Parse(s.EatInNetAmount);
                HSA_TAKE_OUT_SALES_AM += decimal.Parse(s.TakeOutNetAmount);
                HSA_TAKE_OUT_SALES_AM += decimal.Parse(s.p);
                HSA_TAKE_OUT_SALES_AM += decimal.Parse(s.TakeOutNetAmount);

            });
            Console.WriteLine(HSA_EAT_IN_SALES_AM);
            Console.WriteLine(HSA_TAKE_OUT_SALES_AM);*/

            List<APP_DDAY_HOURLY_SALES> app_dday_hourly_sales = new List<APP_DDAY_HOURLY_SALES>();

           // Console.WriteLine("++++++++++++++++++++");
           // Console.WriteLine(hourlySales.DayPartitioning.Segment.Count);
            hourlySales.DayPartitioning.Segment.ForEach((Segment segment) =>
            {
                // Console.WriteLine(s.Id);
                // Console.WriteLine(s.BegTime);
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
                    //HSA_EAT_IN_SALES_AM = HSA_EAT_IN_SALES_AM,
                    //HSA_TAKE_OUT_SALES_AM = HSA_TAKE_OUT_SALES_AM,

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

            //Console.WriteLine("++++++++++++++++++++");
            //Console.WriteLine(hourlyPMX.ProductTable.ProductInfo.Count);
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
