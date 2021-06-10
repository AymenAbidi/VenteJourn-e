using Mcd.App.GetXmlRpc.DAL;
using Mcd.App.GetXmlRpc.Helpers;
using Mcd.App.GetXmlRpc.PMX;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;


namespace Mcd.App.GetXmlRpc
{
    public class DataBase : IDisposable
    {

        private readonly McDashboard_DevlEntities _ctx;
        private readonly logger _logger;
        private List<APP_DDAY_HOURLY_SALES> HsFinalList;
        private List<APP_DDAY_HOURLY_PMX> HpFinalList;
        private static List<RFU_POD_SIR_ID> Pod_SIR_ID;
    
        private readonly bool StoredProcedure = bool.Parse(ConfigurationManager.AppSettings["StoredProcedure"]);
        private string connectionString = ConfigurationManager.AppSettings["StoredProcConnectionString"];
        public DataBase(logger _logger)
        {

            _ctx = new McDashboard_DevlEntities();
            this._logger = _logger;
            HsFinalList = new List<APP_DDAY_HOURLY_SALES>();
            HpFinalList = new List<APP_DDAY_HOURLY_PMX>();
            Pod_SIR_ID = _ctx.RFU_POD_SIR_ID.ToList();
            
        }

        public void SaveHourlySales(HourlySales hourlySalesObjet, int? numResto = null, DateTime? dateActivity = null)
        {
            Stopwatch retDataHs = new Stopwatch();
            retDataHs.Start();
            List<APP_DDAY_HOURLY_SALES> app_dday_hourly_sales = new List<APP_DDAY_HOURLY_SALES>();
            List<APP_DDAY_HOURLY_PMX> app_dday_hourly_pmx = new List<APP_DDAY_HOURLY_PMX>();
            MapToSales(hourlySalesObjet,app_dday_hourly_sales,app_dday_hourly_pmx, numResto);
            retDataHs.Stop();
            Console.WriteLine("--------Diagnostics--------- Recuperation des données db depuis objet Hs :" + retDataHs.Elapsed);
            HsFinalList.AddRange(app_dday_hourly_sales);
            HpFinalList.AddRange(app_dday_hourly_pmx);
        }

        public void SaveChanges()
        {
            Console.WriteLine($"Début du commit en BDD de # {HsFinalList.Count} enregistrements pour HourlySales");
            Console.WriteLine($"Début du commit en BDD de # {HpFinalList.Count} enregistrements pour PMix");
            _logger.Info($"Début du commit en BDD de # {HsFinalList.Count} enregistrements pour HourlySales");
            _logger.Info($"Début du commit en BDD de # {HpFinalList.Count} enregistrements pour PMix");
            Stopwatch savedb = new Stopwatch();
            savedb.Start();
            if (StoredProcedure)
            {
                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();
                    Console.WriteLine("connexion version : " + con.ServerVersion);
                    DataTable dtHs = GenHsDataTable();
                    DataTable dtHp = GenHpDataTable();
                    using (SqlCommand cmdHp = new SqlCommand("exec SPW_INSERT_DDAY_HOURLY_PMX @list", con))
                    {
                        var pList = new SqlParameter("@list", SqlDbType.Structured);
                        pList.TypeName = "dbo.APP_DDAY_HOURLY_PMX_TYPE";
                        pList.Value = dtHp;
                        cmdHp.Parameters.Add(pList);
                        try { var drHp = cmdHp.ExecuteReader(); }
                        catch (Exception e)
                        {
                            _logger.Error("Erreur lors de l'execution de la procedure sp_insert_ddhp_hourly_pmx :", e);
                            Console.WriteLine(e);
                        }
                    }
                    using (SqlCommand cmdHs = new SqlCommand("exec SPW_INSERT_DDAY_HOURLY_SALES @list", con))
                    {
                        var pList = new SqlParameter("@list", SqlDbType.Structured);
                        pList.TypeName = "dbo.APP_DDAY_HOURLY_SALES_TYPE";
                        pList.Value = dtHs;
                        cmdHs.Parameters.Add(pList);
                        try { var drHs = cmdHs.ExecuteReader(); }
                        catch (Exception e)
                        {
                            _logger.Error("Erreur lors de l'execution de la procedure sp_insert_ddhs_hourly_sales :", e);
                            Console.WriteLine(e);
                        }
                    }
                    savedb.Stop();
                    Console.WriteLine("integration base de données : " + savedb.Elapsed);
                }
            }
            else
            {
                try
                {
                    Stopwatch addDbhs = new Stopwatch();
                    addDbhs.Start();
                    _ctx.APP_DDAY_HOURLY_SALES.AddRange(HsFinalList);
                    addDbhs.Stop();
                    Console.WriteLine("--------Diagnostics--------- Ajout hs dans la table db :" + addDbhs.Elapsed);
                    Stopwatch addDbhp = new Stopwatch();
                    addDbhp.Start();
                    _ctx.APP_DDAY_HOURLY_PMX.AddRange(HpFinalList);
                    addDbhp.Stop();
                    Console.WriteLine("--------Diagnostics--------- Ajout hp dans la table db :" + addDbhp.Elapsed);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    _logger.Error("Erreur lors de l'integration des données avec Entity Framework :", e);
                }
                try
                {
                    Stopwatch comdb = new Stopwatch();
                    comdb.Start();
                    _ctx.SaveChanges();
                    comdb.Stop();
                    Console.WriteLine("--------Diagnostics--------- Commit db :" + comdb.Elapsed);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    _logger.Error("Erreur lors du commit DB Entity Framework :", e);
                }
            }
            _logger.Info($"Fin du commit en BDD de # {HsFinalList.Count} enregistrements ");
            HsFinalList = new List<APP_DDAY_HOURLY_SALES>();
        }

        /*public void SavePmix(HourlySales hourlySales, DayPartitioning dayPartitioning, int numResto)
        {
            Stopwatch retDataHp = new Stopwatch();
            retDataHp.Start();
            List<APP_DDAY_HOURLY_PMX> list = MapToPMX(hourlySales, dayPartitioning, numResto);
            retDataHp.Stop();
            Console.WriteLine("--------Diagnostics--------- Récuperation des données db depuis objet Hp :" + retDataHp.Elapsed);
            HpFinalList.AddRange(list);
        }*/

        private void MapToSales(HourlySales hourlySales, List<APP_DDAY_HOURLY_SALES> app_dday_hourly_sales, List<APP_DDAY_HOURLY_PMX> app_dday_hourly_pmx, int? numResto = null)
        {
            //List<APP_DDAY_HOURLY_SALES> app_dday_hourly_sales = new List<APP_DDAY_HOURLY_SALES>();
           
            Random random = new Random();
            hourlySales.POD.ForEach((POD pod) =>
            {
                if (pod.StoreTotal != null)
                {
                    pod.StoreTotal.Sales.ForEach((Sales sales) =>
                    {
                        
                        APP_DDAY_HOURLY_SALES app_hourly_sale = new APP_DDAY_HOURLY_SALES
                        {
                            DDES_SITE_ID = 1,
                            DDES_MCDE_SIR_ID = (short)(Pod_SIR_ID.Any(pod_sir => pod_sir.PDID_PODSHORT == pod.PodShort) ? Pod_SIR_ID.FirstOrDefault(pod_sir => pod_sir.PDID_PODSHORT == pod.PodShort).PDID_MCDE_SIR_ID : 0),
                            DDES_MVAL_SIR_ID = (short)(Pod_SIR_ID.Any(pod_sir => pod_sir.PDID_PODSHORT == pod.PodShort) ? Pod_SIR_ID.FirstOrDefault(pod_sir => pod_sir.PDID_PODSHORT == pod.PodShort).PDID_MVAL_SIR_ID : 0),
                            DDES_LLVR_SIR_ID = (short)(Pod_SIR_ID.Any(pod_sir => pod_sir.PDID_PODSHORT == pod.PodShort) ? Pod_SIR_ID.FirstOrDefault(pod_sir => pod_sir.PDID_PODSHORT == pod.PodShort).PDID_LLVR_SIR_ID : 0),
                            DDES_SALES_TM = short.Parse(hourlySales.DayPartitioning.Segment.FirstOrDefault(seg => seg.Id == sales.Id.ToString()).BegTime),
                            DDES_SALES_PROD_AM = decimal.Parse(sales.ProductNetAmount),
                            DDES_SALES_NON_PROD_AM = decimal.Parse(sales.NetAmount) - decimal.Parse(sales.ProductNetAmount),
                            DDES_EAT_IN_SALES_AM = decimal.Parse(sales.EatInNetAmount),
                            DDES_TAKE_OUT_SALES_AM = decimal.Parse(sales.TakeOutNetAmount),
                            DDES_EAT_IN_TAC_QY = Eat_In_Qy(sales.ExtTC, sales.EatInTC, sales.TakeOutTC),
                            DDES_TAKE_OUT_TAC_QY = Take_Out_Qy(sales.ExtTC, sales.EatInTC, sales.TakeOutTC),
                            DDES_DISCOUNT_IN_TAC_QY = 0,
                            DDES_DISCOUNT_OUT_TAC_QY = 0,
                            DDES_DISCOUNT_IN_SALES_AM = 0,
                            DDES_DISCOUNT_OUT_SALES_AM = 0,
                            DDES_CREW_HOURS_WORKED = 0,
                            DDES_PROCESS_DT = DateTime.Now,
                            DDES_BUSINESS_DT = DateTime.ParseExact(hourlySales.RequestDate, "yyyyMMdd",
                                                           CultureInfo.InvariantCulture),
                        };
                        app_dday_hourly_sales.Add(app_hourly_sale);

                        sales.Product.ForEach((Product product) =>
                        {
                            if (product.Id != "11111")
                            {
                                int DDMX_PROD_ID = int.Parse(product.Id);
                                short DDMX_SALES_TM = 0;
                                short DDMX_EAT_IN_QY = 0;
                                short DDMX_TAKE_OUT_QY = 0;
                                short DDMX_PROMO_IN_QY = 0;
                                short DDMX_PROMO_OUT_QY = 0;
                                short DDMX_DISCOUNT_IN_QY = 0;
                                short DDMX_DISCOUNT_OUT_QY = 0;
                                short DDMX_EMPLOYEE_MEAL_QY = 0;
                                short DDMX_MGR_MEAL_QY = 0;
                                decimal DDMX_EMPLOYEE_MEAL_AM = 0;
                                decimal DDMX_CA_IN_AM = 0;
                                decimal DDMX_CA_OUT_AM = 0;
                                DDMX_SALES_TM = short.Parse(hourlySales.DayPartitioning.Segment.FirstOrDefault(seg => seg.Id == sales.Id.ToString()).BegTime);

                                product.OperationType.ForEach((OperationType opT) =>
                                {


                                    if (opT.operationType == "SALE")
                                    {
                                        DDMX_EAT_IN_QY = short.Parse(opT.PMix.QtyEatIn);
                                        DDMX_TAKE_OUT_QY = short.Parse(opT.PMix.QtyTakeOut);
                                        DDMX_CA_IN_AM = decimal.Parse(opT.PMix.EatInNetAmount);
                                        DDMX_CA_OUT_AM = decimal.Parse(opT.PMix.TakeOutNetAmount);
                                    }
                                    if (opT.operationType == "PROMO")
                                    {
                                        DDMX_PROMO_IN_QY = short.Parse(opT.PMix.QtyEatIn);
                                        DDMX_PROMO_OUT_QY = short.Parse(opT.PMix.QtyTakeOut);
                                    }
                                    if (opT.operationType == "DISCOUNT")
                                    {
                                        DDMX_DISCOUNT_IN_QY = short.Parse(opT.PMix.QtyEatIn);
                                        DDMX_DISCOUNT_OUT_QY = short.Parse(opT.PMix.QtyTakeOut);
                                    }
                                    if (opT.operationType == "CREW")
                                    {
                                        DDMX_EMPLOYEE_MEAL_QY = (short)(int.Parse(opT.PMix.QtyEatIn) + int.Parse(opT.PMix.QtyTakeOut));
                                        DDMX_EMPLOYEE_MEAL_AM = decimal.Parse(opT.PMix.EatInNetAmount) + decimal.Parse(opT.PMix.TakeOutNetAmount);
                                    }
                                    if (opT.operationType == "MANAGER")
                                    {
                                        DDMX_MGR_MEAL_QY = (short)(short.Parse(opT.PMix.QtyEatIn) + short.Parse(opT.PMix.QtyTakeOut));
                                    }
                                    if (opT.operationType == "REFUND")
                                    {
                                        DDMX_EAT_IN_QY -= short.Parse(opT.PMix.QtyEatIn);
                                        DDMX_TAKE_OUT_QY -= short.Parse(opT.PMix.QtyTakeOut);
                                        DDMX_CA_IN_AM -= decimal.Parse(opT.PMix.EatInNetAmount);
                                        DDMX_CA_OUT_AM -= decimal.Parse(opT.PMix.TakeOutNetAmount);
                                    }


                                });
                                APP_DDAY_HOURLY_PMX app_hourly_pmx = new APP_DDAY_HOURLY_PMX
                                {
                                    DDMX_SITE_ID = (short)numResto,
                                    DDMX_MCDE_SIR_ID = (byte)(Pod_SIR_ID.Any(pod_sir => pod_sir.PDID_PODSHORT == pod.PodShort) ? Pod_SIR_ID.FirstOrDefault(pod_sir => pod_sir.PDID_PODSHORT == pod.PodShort).PDID_MCDE_SIR_ID : 0),
                                    DDMX_MVAL_SIR_ID = (byte)(Pod_SIR_ID.Any(pod_sir => pod_sir.PDID_PODSHORT == pod.PodShort) ? Pod_SIR_ID.FirstOrDefault(pod_sir => pod_sir.PDID_PODSHORT == pod.PodShort).PDID_MVAL_SIR_ID : 0),
                                    DDMX_LLVR_SIR_ID = (byte)(Pod_SIR_ID.Any(pod_sir => pod_sir.PDID_PODSHORT == pod.PodShort) ? Pod_SIR_ID.FirstOrDefault(pod_sir => pod_sir.PDID_PODSHORT == pod.PodShort).PDID_LLVR_SIR_ID : 0),
                                    DDMX_SALES_TM = DDMX_SALES_TM,
                                    DDMX_PROD_ID = DDMX_PROD_ID,
                                    DDMX_EAT_IN_QY = DDMX_EAT_IN_QY,
                                    DDMX_TAKE_OUT_QY = DDMX_TAKE_OUT_QY,
                                    DDMX_PROMO_IN_QY = DDMX_PROMO_IN_QY,
                                    DDMX_PROMO_OUT_QY = DDMX_PROMO_OUT_QY,
                                    DDMX_DISCOUNT_IN_QY = DDMX_DISCOUNT_IN_QY,
                                    DDMX_DISCOUNT_OUT_QY = DDMX_DISCOUNT_OUT_QY,
                                    DDMX_EMPLOYEE_MEAL_QY = DDMX_EMPLOYEE_MEAL_QY,
                                    DDMX_MGR_MEAL_QY = DDMX_MGR_MEAL_QY,
                                    DDMX_EMPLOYEE_MEAL_AM = DDMX_EMPLOYEE_MEAL_AM,
                                    DDMX_CA_IN_AM = DDMX_CA_IN_AM,
                                    DDMX_CA_OUT_AM = DDMX_CA_OUT_AM,
                                    DDMX_BUSINESS_DT = DateTime.ParseExact(hourlySales.RequestDate, "yyyyMMdd",
                                                           CultureInfo.InvariantCulture),
                                    DDMX_PROCESS_DT = DateTime.Now
                                };
                                app_dday_hourly_pmx.Add(app_hourly_pmx);
                            }
                        });
                    });
                }
            });
            //return app_dday_hourly_sales;
        }
/*
        private List<APP_DDAY_HOURLY_PMX> MapToPMX(HourlyPMX hourlyPMX, DayPartitioning dayPartitioning, int? numResto = null)
        {
            List<APP_DDAY_HOURLY_PMX> app_dday_hourly_pmx = new List<APP_DDAY_HOURLY_PMX>();
            int DDMX_PROD_ID = 0;
            hourlyPMX.POS.ForEach((PMX.POS pos) =>
            {
                pos.FamilyGroup.ForEach((FamilyGroup familyGroup) =>
                {
                    familyGroup.Product.ForEach((PMX.Product product) =>
                    {
                        DDMX_PROD_ID = product.Id;
                        product.OperationType.ForEach((PMX.OperationType opT) =>
                        {
                            opT.Price.ForEach((Price price) =>
                            {
                                short DDMX_SALES_TM = 0;
                                short DDMX_EAT_IN_QY = 0;
                                short DDMX_TAKE_OUT_QY = 0;
                                short DDMX_PROMO_IN_QY = 0;
                                short DDMX_PROMO_OUT_QY = 0;
                                short DDMX_DISCOUNT_IN_QY = 0;
                                short DDMX_DISCOUNT_OUT_QY = 0;
                                short DDMX_EMPLOYEE_MEAL_QY = 0;
                                short DDMX_MGR_MEAL_QY = 0;
                                decimal DDMX_EMPLOYEE_MEAL_AM = 0;
                                decimal DDMX_CA_IN_AM = 0;
                                decimal DDMX_CA_OUT_AM = 0;
                                DDMX_SALES_TM = short.Parse(dayPartitioning.Segment.FirstOrDefault(seg => seg.Id == price.SaleTime.ToString()).BegTime);
                                DDMX_CA_IN_AM = (decimal)price.PMix.NetAmtEatIn;
                                DDMX_CA_OUT_AM = (decimal)price.PMix.NetAmtTakeOut;
                                if (opT.operationType == "SALE")
                                {
                                    DDMX_EAT_IN_QY = (short)price.PMix.QtyEatIn;
                                    DDMX_TAKE_OUT_QY = (short)price.PMix.QtyTakeOut;
                                }
                                if (opT.operationType == "PROMO")
                                {
                                    DDMX_PROMO_IN_QY = (short)price.PMix.QtyEatIn;
                                    DDMX_PROMO_OUT_QY = (short)price.PMix.QtyTakeOut;
                                }
                                if (opT.operationType == "DISCOUNT")
                                {
                                    DDMX_DISCOUNT_IN_QY = (short)price.PMix.QtyEatIn;
                                    DDMX_DISCOUNT_OUT_QY = (short)price.PMix.QtyTakeOut;
                                }
                                if (opT.operationType == "CREW")
                                {
                                    DDMX_EMPLOYEE_MEAL_QY = (short)price.PMix.QtyEatIn;
                                    DDMX_EMPLOYEE_MEAL_AM = (short)price.PMix.NetAmtEatIn + (short)opT.PMix.NetAmtTakeOut;
                                }
                                if (opT.operationType == "MANAGER")
                                {
                                    DDMX_MGR_MEAL_QY = (short)price.PMix.QtyEatIn;
                                }
                                APP_DDAY_HOURLY_PMX app_hourly_pmx = new APP_DDAY_HOURLY_PMX
                                {
                                    DDMX_SITE_ID = (short)numResto,
                                    DDMX_MCDE_SIR_ID = (byte)(Pod_SIR_ID.Any(pod_sir => pod_sir.PDID_PODSHORT == pos.PodShort) ? Pod_SIR_ID.FirstOrDefault(pod_sir => pod_sir.PDID_PODSHORT == pos.PodShort).PDID_MCDE_SIR_ID : 0),
                                    DDMX_MVAL_SIR_ID = (byte)(Pod_SIR_ID.Any(pod_sir => pod_sir.PDID_PODSHORT == pos.PodShort) ? Pod_SIR_ID.FirstOrDefault(pod_sir => pod_sir.PDID_PODSHORT == pos.PodShort).PDID_MVAL_SIR_ID : 0),
                                    DDMX_LLVR_SIR_ID = (byte)(Pod_SIR_ID.Any(pod_sir => pod_sir.PDID_PODSHORT == pos.PodShort) ? Pod_SIR_ID.FirstOrDefault(pod_sir => pod_sir.PDID_PODSHORT == pos.PodShort).PDID_LLVR_SIR_ID : 0),
                                    DDMX_SALES_TM = DDMX_SALES_TM,
                                    DDMX_PROD_ID = DDMX_PROD_ID,
                                    DDMX_EAT_IN_QY = DDMX_EAT_IN_QY,
                                    DDMX_TAKE_OUT_QY = DDMX_TAKE_OUT_QY,
                                    DDMX_PROMO_IN_QY = DDMX_PROMO_IN_QY,
                                    DDMX_PROMO_OUT_QY = DDMX_PROMO_OUT_QY,
                                    DDMX_DISCOUNT_IN_QY = DDMX_DISCOUNT_IN_QY,
                                    DDMX_DISCOUNT_OUT_QY = DDMX_DISCOUNT_OUT_QY,
                                    DDMX_EMPLOYEE_MEAL_QY = DDMX_EMPLOYEE_MEAL_QY,
                                    DDMX_MGR_MEAL_QY = DDMX_MGR_MEAL_QY,
                                    DDMX_EMPLOYEE_MEAL_AM = DDMX_EMPLOYEE_MEAL_AM,
                                    DDMX_CA_IN_AM = DDMX_CA_IN_AM,
                                    DDMX_CA_OUT_AM = DDMX_CA_OUT_AM,
                                    DDMX_BUSINESS_DT = DateTime.ParseExact("20181003", "yyyyMMdd",
                                                    CultureInfo.InvariantCulture),
                                    DDMX_PROCESS_DT = DateTime.Now
                                };
                                app_dday_hourly_pmx.Add(app_hourly_pmx);
                            });
                        });
                    });
                });
            });
            return app_dday_hourly_pmx;
        }*/

       /* private List<APP_DDAY_HOURLY_PMX> MapToPMX(HourlySales hourlySales, DayPartitioning dayPartitioning, int? numResto = null)
        {
            List<APP_DDAY_HOURLY_PMX> app_dday_hourly_pmx = new List<APP_DDAY_HOURLY_PMX>();
            int DDMX_PROD_ID = 0;
            hourlySales.POD.ForEach((POD pod) =>
            {
                pod.StoreTotal.Sales.ForEach((Sales sales) =>
                {
                    sales.Product.ForEach((Product product) =>
                    {
                        if(product.Id!="11111")
                        { 
                        DDMX_PROD_ID = int.Parse(product.Id);
                        short DDMX_SALES_TM = 0;
                        short DDMX_EAT_IN_QY = 0;
                        short DDMX_TAKE_OUT_QY = 0;
                        short DDMX_PROMO_IN_QY = 0;
                        short DDMX_PROMO_OUT_QY = 0;
                        short DDMX_DISCOUNT_IN_QY = 0;
                        short DDMX_DISCOUNT_OUT_QY = 0;
                        short DDMX_EMPLOYEE_MEAL_QY = 0;
                        short DDMX_MGR_MEAL_QY = 0;
                        decimal DDMX_EMPLOYEE_MEAL_AM = 0;
                        decimal DDMX_CA_IN_AM = 0;
                        decimal DDMX_CA_OUT_AM = 0;
                        DDMX_SALES_TM = short.Parse(dayPartitioning.Segment.FirstOrDefault(seg => seg.Id == sales.Id.ToString()).BegTime);

                        product.OperationType.ForEach((OperationType opT) =>
                        {


                            if (opT.operationType == "SALE")
                            {
                                DDMX_EAT_IN_QY = short.Parse(opT.PMix.QtyEatIn);
                                DDMX_TAKE_OUT_QY = short.Parse(opT.PMix.QtyTakeOut);
                                DDMX_CA_IN_AM = decimal.Parse(opT.PMix.EatInNetAmount);
                                DDMX_CA_OUT_AM = decimal.Parse(opT.PMix.TakeOutNetAmount);
                            }
                            if (opT.operationType == "PROMO")
                            {
                                DDMX_PROMO_IN_QY = short.Parse(opT.PMix.QtyEatIn);
                                DDMX_PROMO_OUT_QY = short.Parse(opT.PMix.QtyTakeOut);
                            }
                            if (opT.operationType == "DISCOUNT")
                            {
                                DDMX_DISCOUNT_IN_QY = short.Parse(opT.PMix.QtyEatIn);
                                DDMX_DISCOUNT_OUT_QY = short.Parse(opT.PMix.QtyTakeOut);
                            }
                            if (opT.operationType == "CREW")
                            {
                                DDMX_EMPLOYEE_MEAL_QY = (short)(int.Parse(opT.PMix.QtyEatIn) + int.Parse(opT.PMix.QtyTakeOut));
                                DDMX_EMPLOYEE_MEAL_AM = decimal.Parse(opT.PMix.EatInNetAmount) + decimal.Parse(opT.PMix.TakeOutNetAmount);
                            }
                            if (opT.operationType == "MANAGER")
                            {
                                DDMX_MGR_MEAL_QY = (short)(short.Parse(opT.PMix.QtyEatIn) + short.Parse(opT.PMix.QtyTakeOut));
                            }
                            if (opT.operationType == "REFUND")
                            {
                                DDMX_EAT_IN_QY -= short.Parse(opT.PMix.QtyEatIn);
                                DDMX_TAKE_OUT_QY -= short.Parse(opT.PMix.QtyTakeOut);
                                DDMX_CA_IN_AM -= decimal.Parse(opT.PMix.EatInNetAmount);
                                DDMX_CA_OUT_AM -= decimal.Parse(opT.PMix.TakeOutNetAmount);
                            }


                        });
                        APP_DDAY_HOURLY_PMX app_hourly_pmx = new APP_DDAY_HOURLY_PMX
                        {
                            DDMX_SITE_ID = (short)numResto,
                            DDMX_MCDE_SIR_ID = (byte)(Pod_SIR_ID.Any(pod_sir => pod_sir.PDID_PODSHORT == pod.PodShort) ? Pod_SIR_ID.FirstOrDefault(pod_sir => pod_sir.PDID_PODSHORT == pod.PodShort).PDID_MCDE_SIR_ID : 0),
                            DDMX_MVAL_SIR_ID = (byte)(Pod_SIR_ID.Any(pod_sir => pod_sir.PDID_PODSHORT == pod.PodShort) ? Pod_SIR_ID.FirstOrDefault(pod_sir => pod_sir.PDID_PODSHORT == pod.PodShort).PDID_MVAL_SIR_ID : 0),
                            DDMX_LLVR_SIR_ID = (byte)(Pod_SIR_ID.Any(pod_sir => pod_sir.PDID_PODSHORT == pod.PodShort) ? Pod_SIR_ID.FirstOrDefault(pod_sir => pod_sir.PDID_PODSHORT == pod.PodShort).PDID_LLVR_SIR_ID : 0),
                            DDMX_SALES_TM = DDMX_SALES_TM,
                            DDMX_PROD_ID = DDMX_PROD_ID,
                            DDMX_EAT_IN_QY = DDMX_EAT_IN_QY,
                            DDMX_TAKE_OUT_QY = DDMX_TAKE_OUT_QY,
                            DDMX_PROMO_IN_QY = DDMX_PROMO_IN_QY,
                            DDMX_PROMO_OUT_QY = DDMX_PROMO_OUT_QY,
                            DDMX_DISCOUNT_IN_QY = DDMX_DISCOUNT_IN_QY,
                            DDMX_DISCOUNT_OUT_QY = DDMX_DISCOUNT_OUT_QY,
                            DDMX_EMPLOYEE_MEAL_QY = DDMX_EMPLOYEE_MEAL_QY,
                            DDMX_MGR_MEAL_QY = DDMX_MGR_MEAL_QY,
                            DDMX_EMPLOYEE_MEAL_AM = DDMX_EMPLOYEE_MEAL_AM,
                            DDMX_CA_IN_AM = DDMX_CA_IN_AM,
                            DDMX_CA_OUT_AM = DDMX_CA_OUT_AM,
                            DDMX_BUSINESS_DT = DateTime.ParseExact("20181003", "yyyyMMdd",
                                                   CultureInfo.InvariantCulture),
                            DDMX_PROCESS_DT = DateTime.Now
                        };
                        app_dday_hourly_pmx.Add(app_hourly_pmx);
                    }
                    });
                });
            });
            return app_dday_hourly_pmx;
        }*/
        private DataTable GenHsDataTable()
        {
            DataTable dtHs = new DataTable();

            DataColumn DDES_SITE_ID = new DataColumn("DDES_SITE_ID");
            DataColumn DDES_BUSINESS_DT = new DataColumn("DDES_BUSINESS_DT");
            DataColumn DDES_SALES_TM = new DataColumn("DDES_SALES_TM");
            DataColumn DDES_MCDE_SIR_ID = new DataColumn("DDES_MCDE_SIR_ID");
            DataColumn DDES_MVAL_SIR_ID = new DataColumn("DDES_MVAL_SIR_ID");
            DataColumn DDES_LLVR_SIR_ID = new DataColumn("DDES_LLVR_SIR_ID");
            DataColumn DDES_SALES_PROD_AM = new DataColumn("DDES_SALES_PROD_AM");
            DataColumn DDES_SALES_NON_PROD_AM = new DataColumn("DDES_SALES_NON_PROD_AM");
            DataColumn DDES_EAT_IN_TAC_QY = new DataColumn("DDES_EAT_IN_TAC_QY");
            DataColumn DDES_TAKE_OUT_TAC_QY = new DataColumn("DDES_TAKE_OUT_TAC_QY");
            DataColumn DDES_EAT_IN_SALES_AM = new DataColumn("DDES_EAT_IN_SALES_AM");
            DataColumn DDES_TAKE_OUT_SALES_AM = new DataColumn("DDES_TAKE_OUT_SALES_AM");
            DataColumn DDES_DISCOUNT_IN_TAC_QY = new DataColumn("DDES_DISCOUNT_IN_TAC_QY");
            DataColumn DDES_DISCOUNT_OUT_TAC_QY = new DataColumn("DDES_DISCOUNT_OUT_TAC_QY");
            DataColumn DDES_DISCOUNT_IN_SALES_AM = new DataColumn("DDES_DISCOUNT_IN_SALES_AM");
            DataColumn DDES_DISCOUNT_OUT_SALES_AM = new DataColumn("DDES_DISCOUNT_OUT_SALES_AM");
            DataColumn DDES_CREW_HOURS_WORKED = new DataColumn("DDES_CREW_HOURS_WORKED");
            DataColumn DDES_PROCESS_DT = new DataColumn("DDES_PROCESS_DT");

            dtHs.Columns.Add(DDES_SITE_ID);
            dtHs.Columns.Add(DDES_BUSINESS_DT);
            dtHs.Columns.Add(DDES_SALES_TM);
            dtHs.Columns.Add(DDES_MCDE_SIR_ID);
            dtHs.Columns.Add(DDES_MVAL_SIR_ID);
            dtHs.Columns.Add(DDES_LLVR_SIR_ID);
            dtHs.Columns.Add(DDES_SALES_PROD_AM);
            dtHs.Columns.Add(DDES_SALES_NON_PROD_AM);
            dtHs.Columns.Add(DDES_EAT_IN_TAC_QY);
            dtHs.Columns.Add(DDES_TAKE_OUT_TAC_QY);
            dtHs.Columns.Add(DDES_EAT_IN_SALES_AM);
            dtHs.Columns.Add(DDES_TAKE_OUT_SALES_AM);
            dtHs.Columns.Add(DDES_DISCOUNT_IN_TAC_QY);
            dtHs.Columns.Add(DDES_DISCOUNT_OUT_TAC_QY);
            dtHs.Columns.Add(DDES_DISCOUNT_IN_SALES_AM);
            dtHs.Columns.Add(DDES_DISCOUNT_OUT_SALES_AM);
            dtHs.Columns.Add(DDES_CREW_HOURS_WORKED);
            dtHs.Columns.Add(DDES_PROCESS_DT);

            HsFinalList.ForEach((APP_DDAY_HOURLY_SALES hs) =>
            {
                DataRow d = dtHs.NewRow();
                d["DDES_SITE_ID"] = hs.DDES_SITE_ID;
                d["DDES_BUSINESS_DT"] = hs.DDES_BUSINESS_DT;
                d["DDES_SALES_TM"] = hs.DDES_SALES_TM;
                d["DDES_MCDE_SIR_ID"] = hs.DDES_MCDE_SIR_ID;
                d["DDES_MVAL_SIR_ID"] = hs.DDES_MVAL_SIR_ID;
                d["DDES_LLVR_SIR_ID"] = hs.DDES_LLVR_SIR_ID;
                d["DDES_SALES_PROD_AM"] = hs.DDES_SALES_PROD_AM;
                d["DDES_SALES_NON_PROD_AM"] = hs.DDES_SALES_NON_PROD_AM;
                d["DDES_EAT_IN_TAC_QY"] = hs.DDES_EAT_IN_TAC_QY;
                d["DDES_TAKE_OUT_TAC_QY"] = hs.DDES_TAKE_OUT_TAC_QY;
                d["DDES_EAT_IN_SALES_AM"] = hs.DDES_EAT_IN_SALES_AM;
                d["DDES_TAKE_OUT_SALES_AM"] = hs.DDES_TAKE_OUT_SALES_AM;
                d["DDES_DISCOUNT_IN_TAC_QY"] = hs.DDES_DISCOUNT_IN_TAC_QY;
                d["DDES_DISCOUNT_OUT_TAC_QY"] = hs.DDES_DISCOUNT_OUT_TAC_QY;
                d["DDES_DISCOUNT_IN_SALES_AM"] = hs.DDES_DISCOUNT_IN_SALES_AM;
                d["DDES_DISCOUNT_OUT_SALES_AM"] = hs.DDES_DISCOUNT_OUT_SALES_AM;
                d["DDES_CREW_HOURS_WORKED"] = hs.DDES_CREW_HOURS_WORKED;
                d["DDES_PROCESS_DT"] = hs.DDES_PROCESS_DT;

                dtHs.Rows.Add(d);

            });
            return dtHs;

        }

        private DataTable GenHpDataTable()
        {
            DataTable dtHp = new DataTable();

            DataColumn DDMX_SITE_ID = new DataColumn("DDMX_SITE_ID");
            DataColumn DDMX_BUSINESS_DT = new DataColumn("DDMX_BUSINESS_DT");
            DataColumn DDMX_PROD_ID = new DataColumn("DDMX_PROD_ID");
            DataColumn DDMX_SALES_TM = new DataColumn("DDMX_SALES_TM");
            DataColumn DDMX_MCDE_SIR_ID = new DataColumn("DDMX_MCDE_SIR_ID");
            DataColumn DDMX_MVAL_SIR_ID = new DataColumn("DDMX_MVAL_SIR_ID");
            DataColumn DDMX_LLVR_SIR_ID = new DataColumn("DDMX_LLVR_SIR_ID");
            DataColumn DDMX_EAT_IN_QY = new DataColumn("DDMX_EAT_IN_QY");
            DataColumn DDMX_TAKE_OUT_QY = new DataColumn("DDMX_TAKE_OUT_QY");
            DataColumn DDMX_PROMO_IN_QY = new DataColumn("DDMX_PROMO_IN_QY");
            DataColumn DDMX_PROMO_OUT_QY = new DataColumn("DDMX_PROMO_OUT_QY");
            DataColumn DDMX_DISCOUNT_IN_QY = new DataColumn("DDMX_DISCOUNT_IN_QY");
            DataColumn DDMX_DISCOUNT_OUT_QY = new DataColumn("DDMX_DISCOUNT_OUT_QY");
            DataColumn DDMX_EMPLOYEE_MEAL_QY = new DataColumn("DDMX_EMPLOYEE_MEAL_QY");
            DataColumn DDMX_MGR_MEAL_QY = new DataColumn("DDMX_MGR_MEAL_QY");
            DataColumn DDMX_EMPLOYEE_MEAL_AM = new DataColumn("DDMX_EMPLOYEE_MEAL_AM");
            DataColumn DDMX_CA_IN_AM = new DataColumn("DDMX_CA_IN_AM");
            DataColumn DDMX_CA_OUT_AM = new DataColumn("DDMX_CA_OUT_AM");
            DataColumn DDMX_PROCESS_DT = new DataColumn("DDMX_PROCESS_DT");



            dtHp.Columns.Add(DDMX_SITE_ID);
            dtHp.Columns.Add(DDMX_BUSINESS_DT);
            dtHp.Columns.Add(DDMX_PROD_ID);
            dtHp.Columns.Add(DDMX_SALES_TM);
            dtHp.Columns.Add(DDMX_MCDE_SIR_ID);
            dtHp.Columns.Add(DDMX_MVAL_SIR_ID);
            dtHp.Columns.Add(DDMX_LLVR_SIR_ID);
            dtHp.Columns.Add(DDMX_EAT_IN_QY);
            dtHp.Columns.Add(DDMX_TAKE_OUT_QY);
            dtHp.Columns.Add(DDMX_PROMO_IN_QY);
            dtHp.Columns.Add(DDMX_PROMO_OUT_QY);
            dtHp.Columns.Add(DDMX_DISCOUNT_IN_QY);
            dtHp.Columns.Add(DDMX_DISCOUNT_OUT_QY);
            dtHp.Columns.Add(DDMX_EMPLOYEE_MEAL_QY);
            dtHp.Columns.Add(DDMX_MGR_MEAL_QY);
            dtHp.Columns.Add(DDMX_EMPLOYEE_MEAL_AM);
            dtHp.Columns.Add(DDMX_CA_IN_AM);
            dtHp.Columns.Add(DDMX_CA_OUT_AM);
            dtHp.Columns.Add(DDMX_PROCESS_DT);



            HpFinalList.ForEach((APP_DDAY_HOURLY_PMX hp) =>
            {
                DataRow d = dtHp.NewRow();
                d["DDMX_SITE_ID"] = hp.DDMX_SITE_ID;
                d["DDMX_BUSINESS_DT"] = hp.DDMX_BUSINESS_DT;
                d["DDMX_PROD_ID"] = hp.DDMX_PROD_ID;
                d["DDMX_SALES_TM"] = hp.DDMX_SALES_TM;
                d["DDMX_MCDE_SIR_ID"] = hp.DDMX_MCDE_SIR_ID;
                d["DDMX_MVAL_SIR_ID"] = hp.DDMX_MVAL_SIR_ID;
                d["DDMX_LLVR_SIR_ID"] = hp.DDMX_LLVR_SIR_ID;
                d["DDMX_EAT_IN_QY"] = hp.DDMX_EAT_IN_QY;
                d["DDMX_TAKE_OUT_QY"] = hp.DDMX_TAKE_OUT_QY;
                d["DDMX_PROMO_IN_QY"] = hp.DDMX_PROMO_IN_QY;
                d["DDMX_PROMO_OUT_QY"] = hp.DDMX_PROMO_OUT_QY;
                d["DDMX_DISCOUNT_IN_QY"] = hp.DDMX_DISCOUNT_IN_QY;
                d["DDMX_DISCOUNT_OUT_QY"] = hp.DDMX_DISCOUNT_OUT_QY;
                d["DDMX_EMPLOYEE_MEAL_QY"] = hp.DDMX_EMPLOYEE_MEAL_QY;
                d["DDMX_MGR_MEAL_QY"] = hp.DDMX_MGR_MEAL_QY;
                d["DDMX_EMPLOYEE_MEAL_AM"] = hp.DDMX_EMPLOYEE_MEAL_AM;
                d["DDMX_CA_IN_AM"] = hp.DDMX_CA_IN_AM;
                d["DDMX_CA_OUT_AM"] = hp.DDMX_CA_OUT_AM;
                d["DDMX_PROCESS_DT"] = hp.DDMX_PROCESS_DT;

                dtHp.Rows.Add(d);
            });

            return dtHp;
        }

        public short Eat_In_Qy(string extTC_str,string eatInTC_str,string takeOutTC_str)
        {
            short extTC = short.Parse(extTC_str);
            short eatInTC = short.Parse(eatInTC_str);
            short takeOutTC = short.Parse(takeOutTC_str);

            if (eatInTC + takeOutTC == extTC)
            {
                return eatInTC;
            }
            else if (extTC - takeOutTC < 0)
            {
                return 0;
            }
            else
            {
                return (short)(extTC - takeOutTC);
            }
        }
        public short Take_Out_Qy(string extTC_str, string eatInTC_str, string takeOutTC_str)
        {
            short extTC = short.Parse(extTC_str);
            short eatInTC = short.Parse(eatInTC_str);
            short takeOutTC = short.Parse(takeOutTC_str);
            if (eatInTC + takeOutTC == extTC)
            {
                return takeOutTC;
            }
            else if (extTC - takeOutTC >= 0)
            {
                return takeOutTC;
            }
            else
            {
                return (short)(takeOutTC + (extTC - takeOutTC));
            }
        }

       /* private NP6XML LireNP6(int numResto, DateTime dateActivity)
        {
            return _ctx.NP6XML.FirstOrDefault(n => n.NumRestaurant == numResto && n.DayActivity == dateActivity);
        }*/

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
