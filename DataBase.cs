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

        private readonly McDashboardEntities _ctx;
        private readonly logger _logger;
        private List<APP_DDAY_HOURLY_SALES> HsFinalList;
        private List<APP_DDAY_HOURLY_PMX> HpFinalList;
        private static List<MCDE_TAB> Mcde_Mapping;
        private static List<MVAL_TAB> Mval_Mapping;
        private static List<LLVR_TAB> Llvr_Mapping;
        private readonly bool StoredProcedure = bool.Parse(ConfigurationManager.AppSettings["StoredProcedure"]);
        private string connectionString = ConfigurationManager.AppSettings["StoredProcConnectionString"];
        public DataBase(logger _logger)
        {

            _ctx = new McDashboardEntities();
            this._logger = _logger;
            HsFinalList = new List<APP_DDAY_HOURLY_SALES>();
            HpFinalList = new List<APP_DDAY_HOURLY_PMX>();
            Mcde_Mapping = _ctx.MCDE_TAB.ToList();
            Mval_Mapping = _ctx.MVAL_TAB.ToList();
            Llvr_Mapping = _ctx.LLVR_TAB.ToList();
        }

        public void SaveHourlySales(HourlySales hourlySalesObjet, int? numResto = null, DateTime? dateActivity = null)
        {
            Stopwatch retDataHs = new Stopwatch();
            retDataHs.Start();
            List<APP_DDAY_HOURLY_SALES> app_dday_hourly_sales = MapToSales(hourlySalesObjet, numResto);
            retDataHs.Stop();
            Console.WriteLine("--------Diagnostics--------- Recuperation des données db depuis objet Hs :" + retDataHs.Elapsed);
            HsFinalList.AddRange(app_dday_hourly_sales);
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
                    using (SqlCommand cmdHp = new SqlCommand("exec sp_insert_ddhp_hourly_pmx @list", con))
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
                    using (SqlCommand cmdHs = new SqlCommand("exec sp_insert_ddhs_hourly_sales @list", con))
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

        public void SavePmix(HourlyPMX hourlyPMX, DayPartitioning dayPartitioning, int numResto)
        {
            Stopwatch retDataHp = new Stopwatch();
            retDataHp.Start();
            List<APP_DDAY_HOURLY_PMX> list = MapToPMX(hourlyPMX, dayPartitioning, numResto);
            retDataHp.Stop();
            Console.WriteLine("--------Diagnostics--------- Récuperation des données db depuis objet Hp :" + retDataHp.Elapsed);
            HpFinalList.AddRange(list);
        }

        private List<APP_DDAY_HOURLY_SALES> MapToSales(HourlySales hourlySales, int? numResto = null)
        {
            List<APP_DDAY_HOURLY_SALES> app_dday_hourly_sales = new List<APP_DDAY_HOURLY_SALES>();
            short DDHS_SALES_TM = 0;
            decimal DDHS_SALES_PROD_AM = 0;
            decimal DDHS_SALES_NON_PROD_AM = 0;
            decimal DDHS_EAT_IN_SALES_AM = 0;
            decimal DDHS_TAKE_OUT_SALES_AM = 0;
            short DDHS_EAT_IN_TAC_QY = 0;
            short DDHS_TAKE_OUT_TAC_QY = 0;
            short DDHS_DISCOUNT_IN_TAC_QY = 0;
            short DDHS_DISCOUNT_OUT_TAC_QY = 0;
            decimal DDHS_DISCOUNT_IN_SALES_AM = 0;
            decimal DDHS_DISCOUNT_OUT_SALES_AM = 0;
            Random random = new Random();
            hourlySales.POS.ForEach((POS pos) =>
            {
                if (pos.StoreTotal != null)
                {
                    pos.StoreTotal.Sales.ForEach((Sales sales) =>
                    {
                        DDHS_SALES_TM = short.Parse(hourlySales.DayPartitioning.Segment.FirstOrDefault(seg => seg.Id == sales.Id.ToString()).BegTime);
                        DDHS_SALES_PROD_AM += decimal.Parse(sales.ProductNetAmount);
                        DDHS_SALES_NON_PROD_AM += decimal.Parse(sales.NetAmount);
                        sales.Product.ForEach((Product product) =>
                            {
                                product.OperationType.ForEach((OperationType OpT) =>
                                {
                                    if (OpT.operationType == "DISCOUNT")
                                    {
                                        DDHS_DISCOUNT_IN_SALES_AM += decimal.Parse(OpT.PMix.EatInNetAmount);
                                        DDHS_DISCOUNT_OUT_SALES_AM += decimal.Parse(OpT.PMix.TakeOutNetAmount);
                                        DDHS_DISCOUNT_IN_TAC_QY += short.Parse(OpT.PMix.QtyEatIn);
                                        DDHS_DISCOUNT_OUT_TAC_QY += short.Parse(OpT.PMix.QtyTakeOut);
                                    }
                                    else if (OpT.operationType == "SALE")
                                    {
                                        DDHS_EAT_IN_SALES_AM += decimal.Parse(OpT.PMix.EatInNetAmount);
                                        DDHS_TAKE_OUT_SALES_AM += decimal.Parse(OpT.PMix.TakeOutNetAmount);
                                        DDHS_EAT_IN_TAC_QY += short.Parse(OpT.PMix.QtyEatIn);
                                        DDHS_TAKE_OUT_TAC_QY += short.Parse(OpT.PMix.QtyTakeOut);
                                    }
                                });
                            });
                        APP_DDAY_HOURLY_SALES app_hourly_sale = new APP_DDAY_HOURLY_SALES
                        {
                            DDHS_SITE_ID = short.Parse(pos.Id),
                            DDHS_MCDE_SIR_ID = (short)(Mcde_Mapping.Any(mcde => mcde.PodShort == pos.PodShort) ? Mcde_Mapping.FirstOrDefault(mcde => mcde.PodShort == pos.PodShort).MCDE_SIR_ID : 0),
                            DDHS_MVAL_SIR_ID = (short)(Mval_Mapping.Any(mval => mval.PodShort == pos.PodShort) ? Mval_Mapping.FirstOrDefault(mval => mval.PodShort == pos.PodShort).MVAL_SIR_ID : 0),
                            DDHS_LLVR_SIR_ID = (short)(Llvr_Mapping.Any(llvr => llvr.PodShort == pos.PodShort) ? Llvr_Mapping.FirstOrDefault(llvr => llvr.PodShort == pos.PodShort).LLVR_SIR_ID : 0),
                            DDHS_SALES_TM = DDHS_SALES_TM,
                            DDHS_SALES_PROD_AM = DDHS_SALES_PROD_AM,
                            DDHS_SALES_NON_PROD_AM = DDHS_SALES_NON_PROD_AM - DDHS_SALES_PROD_AM,
                            DDHS_EAT_IN_SALES_AM = DDHS_EAT_IN_SALES_AM,
                            DDHS_TAKE_OUT_SALES_AM = DDHS_TAKE_OUT_SALES_AM,
                            DDHS_EAT_IN_TAC_QY = DDHS_EAT_IN_TAC_QY,
                            DDHS_TAKE_OUT_TAC_QY = DDHS_TAKE_OUT_TAC_QY,
                            DDHS_DISCOUNT_IN_TAC_QY = DDHS_DISCOUNT_IN_TAC_QY,
                            DDHS_DISCOUNT_OUT_TAC_QY = DDHS_DISCOUNT_OUT_TAC_QY,
                            DDHS_DISCOUNT_IN_SALES_AM = DDHS_DISCOUNT_IN_SALES_AM,
                            DDHS_DISCOUNT_OUT_SALES_AM = DDHS_DISCOUNT_OUT_SALES_AM,
                            DDHS_BUSINESS_DT = DateTime.ParseExact("20181003", "yyyyMMdd",
                                                   CultureInfo.InvariantCulture),
                        };
                        app_dday_hourly_sales.Add(app_hourly_sale);
                    });
                }
            });
            return app_dday_hourly_sales;
        }

        private List<APP_DDAY_HOURLY_PMX> MapToPMX(HourlyPMX hourlyPMX, DayPartitioning dayPartitioning, int? numResto = null)
        {
            List<APP_DDAY_HOURLY_PMX> app_dday_hourly_pmx = new List<APP_DDAY_HOURLY_PMX>();
            int DDHP_PROD_ID = 0;
            hourlyPMX.POS.ForEach((PMX.POS pos) =>
            {
                pos.FamilyGroup.ForEach((FamilyGroup familyGroup) =>
                {
                    familyGroup.Product.ForEach((PMX.Product product) =>
                    {
                        DDHP_PROD_ID = product.Id;
                        product.OperationType.ForEach((PMX.OperationType opT) =>
                        {
                            opT.Price.ForEach((Price price) =>
                            {
                                short DDHP_SALES_TM = 0;
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
                                DDHP_SALES_TM = short.Parse(dayPartitioning.Segment.FirstOrDefault(seg => seg.Id == price.SaleTime.ToString()).BegTime);
                                DDHP_CA_IN_AM = (decimal)price.PMix.NetAmtEatIn;
                                DDHP_CA_OUT_AM = (decimal)price.PMix.NetAmtTakeOut;
                                if (opT.operationType == "SALE")
                                {
                                    DDHP_EAT_IN_QY = (short)price.PMix.QtyEatIn;
                                    DDHP_TAKE_OUT_QY = (short)price.PMix.QtyTakeOut;
                                }
                                if (opT.operationType == "PROMO")
                                {
                                    DDHP_PROMO_IN_QY = (short)price.PMix.QtyEatIn;
                                    DDHP_PROMO_OUT_QY = (short)price.PMix.QtyTakeOut;
                                }
                                if (opT.operationType == "DISCOUNT")
                                {
                                    DDHP_DISCOUNT_IN_QY = (short)price.PMix.QtyEatIn;
                                    DDHP_DISCOUNT_OUT_QY = (short)price.PMix.QtyTakeOut;
                                }
                                if (opT.operationType == "CREW")
                                {
                                    DDHP_EMPLOYEE_MEAL_QY = (short)price.PMix.QtyEatIn;
                                    DDHP_EMPLOYEE_MEAL_AM = (short)price.PMix.NetAmtEatIn + (short)opT.PMix.NetAmtTakeOut;
                                }
                                if (opT.operationType == "MANAGER")
                                {
                                    DDHP_MGR_MEAL_QY = (short)price.PMix.QtyEatIn;
                                }
                                APP_DDAY_HOURLY_PMX app_hourly_pmx = new APP_DDAY_HOURLY_PMX
                                {
                                    DDHP_SITE_ID = (short)numResto,
                                    DDHP_MCDE_SIR_ID = (byte)(Mcde_Mapping.Any(mcde => mcde.PodShort == pos.PodShort) ? Mcde_Mapping.FirstOrDefault(mcde => mcde.PodShort == pos.PodShort).MCDE_SIR_ID : 0),
                                    DDHP_MVAL_SIR_ID = (byte)(Mval_Mapping.Any(mval => mval.PodShort == pos.PodShort) ? Mval_Mapping.FirstOrDefault(mval => mval.PodShort == pos.PodShort).MVAL_SIR_ID : 0),
                                    DDHP_LLVR_SIR_ID = (byte)(Llvr_Mapping.Any(llvr => llvr.PodShort == pos.PodShort) ? Llvr_Mapping.FirstOrDefault(llvr => llvr.PodShort == pos.PodShort).LLVR_SIR_ID : 0),
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
                        });
                    });
                });
            });
            return app_dday_hourly_pmx;
        }
        private DataTable GenHsDataTable()
        {
            DataTable dtHs = new DataTable();

            DataColumn DDHS_SITE_ID = new DataColumn("DDHS_SITE_ID");
            DataColumn DDHS_BUSINESS_DT = new DataColumn("DDHS_BUSINESS_DT");
            DataColumn DDHS_SALES_TM = new DataColumn("DDHS_SALES_TM");
            DataColumn DDHS_MCDE_SIR_ID = new DataColumn("DDHS_MCDE_SIR_ID");
            DataColumn DDHS_MVAL_SIR_ID = new DataColumn("DDHS_MVAL_SIR_ID");
            DataColumn DDHS_LLVR_SIR_ID = new DataColumn("DDHS_LLVR_SIR_ID");
            DataColumn DDHS_SALES_PROD_AM = new DataColumn("DDHS_SALES_PROD_AM");
            DataColumn DDHS_SALES_NON_PROD_AM = new DataColumn("DDHS_SALES_NON_PROD_AM");
            DataColumn DDHS_EAT_IN_TAC_QY = new DataColumn("DDHS_EAT_IN_TAC_QY");
            DataColumn DDHS_TAKE_OUT_TAC_QY = new DataColumn("DDHS_TAKE_OUT_TAC_QY");
            DataColumn DDHS_EAT_IN_SALES_AM = new DataColumn("DDHS_EAT_IN_SALES_AM");
            DataColumn DDHS_TAKE_OUT_SALES_AM = new DataColumn("DDHS_TAKE_OUT_SALES_AM");
            DataColumn DDHS_DISCOUNT_IN_TAC_QY = new DataColumn("DDHS_DISCOUNT_IN_TAC_QY");
            DataColumn DDHS_DISCOUNT_OUT_TAC_QY = new DataColumn("DDHS_DISCOUNT_OUT_TAC_QY");
            DataColumn DDHS_DISCOUNT_IN_SALES_AM = new DataColumn("DDHS_DISCOUNT_IN_SALES_AM");
            DataColumn DDHS_DISCOUNT_OUT_SALES_AM = new DataColumn("DDHS_DISCOUNT_OUT_SALES_AM");
            DataColumn DDHS_CREW_HOURS_WORKED = new DataColumn("DDHS_CREW_HOURS_WORKED");
            DataColumn DDHS_PROCESS_DT = new DataColumn("DDHS_PROCESS_DT");

            dtHs.Columns.Add(DDHS_SITE_ID);
            dtHs.Columns.Add(DDHS_BUSINESS_DT);
            dtHs.Columns.Add(DDHS_SALES_TM);
            dtHs.Columns.Add(DDHS_MCDE_SIR_ID);
            dtHs.Columns.Add(DDHS_MVAL_SIR_ID);
            dtHs.Columns.Add(DDHS_LLVR_SIR_ID);
            dtHs.Columns.Add(DDHS_SALES_PROD_AM);
            dtHs.Columns.Add(DDHS_SALES_NON_PROD_AM);
            dtHs.Columns.Add(DDHS_EAT_IN_TAC_QY);
            dtHs.Columns.Add(DDHS_TAKE_OUT_TAC_QY);
            dtHs.Columns.Add(DDHS_EAT_IN_SALES_AM);
            dtHs.Columns.Add(DDHS_TAKE_OUT_SALES_AM);
            dtHs.Columns.Add(DDHS_DISCOUNT_IN_TAC_QY);
            dtHs.Columns.Add(DDHS_DISCOUNT_OUT_TAC_QY);
            dtHs.Columns.Add(DDHS_DISCOUNT_IN_SALES_AM);
            dtHs.Columns.Add(DDHS_DISCOUNT_OUT_SALES_AM);
            dtHs.Columns.Add(DDHS_CREW_HOURS_WORKED);
            dtHs.Columns.Add(DDHS_PROCESS_DT);

            HsFinalList.ForEach((APP_DDAY_HOURLY_SALES hs) =>
            {
                DataRow d = dtHs.NewRow();
                d["DDHS_SITE_ID"] = hs.DDHS_SITE_ID;
                d["DDHS_BUSINESS_DT"] = hs.DDHS_BUSINESS_DT;
                d["DDHS_SALES_TM"] = hs.DDHS_SALES_TM;
                d["DDHS_MCDE_SIR_ID"] = hs.DDHS_MCDE_SIR_ID;
                d["DDHS_MVAL_SIR_ID"] = hs.DDHS_MVAL_SIR_ID;
                d["DDHS_LLVR_SIR_ID"] = hs.DDHS_LLVR_SIR_ID;
                d["DDHS_SALES_PROD_AM"] = hs.DDHS_SALES_PROD_AM;
                d["DDHS_SALES_NON_PROD_AM"] = hs.DDHS_SALES_NON_PROD_AM;
                d["DDHS_EAT_IN_TAC_QY"] = hs.DDHS_EAT_IN_TAC_QY;
                d["DDHS_TAKE_OUT_TAC_QY"] = hs.DDHS_TAKE_OUT_TAC_QY;
                d["DDHS_EAT_IN_SALES_AM"] = hs.DDHS_EAT_IN_SALES_AM;
                d["DDHS_TAKE_OUT_SALES_AM"] = hs.DDHS_TAKE_OUT_SALES_AM;
                d["DDHS_DISCOUNT_IN_TAC_QY"] = hs.DDHS_DISCOUNT_IN_TAC_QY;
                d["DDHS_DISCOUNT_OUT_TAC_QY"] = hs.DDHS_DISCOUNT_OUT_TAC_QY;
                d["DDHS_DISCOUNT_IN_SALES_AM"] = hs.DDHS_DISCOUNT_IN_SALES_AM;
                d["DDHS_DISCOUNT_OUT_SALES_AM"] = hs.DDHS_DISCOUNT_OUT_SALES_AM;
                d["DDHS_CREW_HOURS_WORKED"] = hs.DDHS_CREW_HOURS_WORKED;
                d["DDHS_PROCESS_DT"] = hs.DDHS_PROCESS_DT;

                dtHs.Rows.Add(d);

            });
            return dtHs;

        }

        private DataTable GenHpDataTable()
        {
            DataTable dtHp = new DataTable();

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



            dtHp.Columns.Add(DDHP_SITE_ID);
            dtHp.Columns.Add(DDHP_BUSINESS_DT);
            dtHp.Columns.Add(DDHP_PROD_ID);
            dtHp.Columns.Add(DDHP_SALES_TM);
            dtHp.Columns.Add(DDHP_MCDE_SIR_ID);
            dtHp.Columns.Add(DDHP_MVAL_SIR_ID);
            dtHp.Columns.Add(DDHP_LLVR_SIR_ID);
            dtHp.Columns.Add(DDHP_EAT_IN_QY);
            dtHp.Columns.Add(DDHP_TAKE_OUT_QY);
            dtHp.Columns.Add(DDHP_PROMO_IN_QY);
            dtHp.Columns.Add(DDHP_PROMO_OUT_QY);
            dtHp.Columns.Add(DDHP_DISCOUNT_IN_QY);
            dtHp.Columns.Add(DDHP_DISCOUNT_OUT_QY);
            dtHp.Columns.Add(DDHP_EMPLOYEE_MEAL_QY);
            dtHp.Columns.Add(DDHP_MGR_MEAL_QY);
            dtHp.Columns.Add(DDHP_EMPLOYEE_MEAL_AM);
            dtHp.Columns.Add(DDHP_CA_IN_AM);
            dtHp.Columns.Add(DDHP_CA_OUT_AM);
            dtHp.Columns.Add(DDHP_PROCESS_DT);



            HpFinalList.ForEach((APP_DDAY_HOURLY_PMX hp) =>
            {
                DataRow d = dtHp.NewRow();
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

                dtHp.Rows.Add(d);
            });

            return dtHp;
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
