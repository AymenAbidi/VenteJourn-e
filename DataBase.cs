using Mcd.App.GetXmlRpc.DAL;
using Mcd.App.GetXmlRpc.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Mcd.App.GetXmlRpc
{
    public class DataBase : IDisposable
    {
        private readonly McDashboard_Entities _ctx;

        private readonly logger _logger;

        private List<APP_DDAY_HOURLY_SALES> HsFinalList;

        private List<APP_DDAY_HOURLY_PMX> HpFinalList;

        private static List<RFA_POD_SIR_ID> Pod_SIR_ID;

        private static List<VW_From_InfoSite4u_Resto_APP_SITE> VW_From_InfoSite4u_APP_SITE;


        private readonly bool StoredProcedure = bool.Parse(ConfigurationManager.AppSettings["StoredProcedure"]);



        private static readonly bool logXmlToData = bool.Parse(ConfigurationManager.AppSettings["LogXML-To-DATA"]);

        private static readonly bool logDataToDb = bool.Parse(ConfigurationManager.AppSettings["LogDATA-To-DB"]);

        private string connectionString = ConfigurationManager.ConnectionStrings[1].ConnectionString.Split('"')[1];
        private string dateFormat = ConfigurationManager.AppSettings["DateFormat"];

        public DataBase(logger _logger)
        {

            _ctx = new McDashboard_Entities();
            this._logger = _logger;
            HsFinalList = new List<APP_DDAY_HOURLY_SALES>();
            HpFinalList = new List<APP_DDAY_HOURLY_PMX>();
            Pod_SIR_ID = _ctx.RFA_POD_SIR_ID.ToList();
            VW_From_InfoSite4u_APP_SITE = _ctx.VW_From_InfoSite4u_Resto_APP_SITE.ToList();
        }


        // Sauvegarde des données récuperées depuis les fichiers xml dans la base de données
        public void SaveHourlySales(HourlySales hourlySalesObjet, SqlConnection con, DateTime dateActivity, int? numResto = null)
        {
            //Stopwatch retDataHs = new Stopwatch();
            //retDataHs.Start();
            List<APP_DDAY_HOURLY_SALES> app_dday_hourly_sales = new List<APP_DDAY_HOURLY_SALES>();
            List<APP_DDAY_HOURLY_PMX> app_dday_hourly_pmx = new List<APP_DDAY_HOURLY_PMX>();
            MapToSales(hourlySalesObjet, app_dday_hourly_sales, app_dday_hourly_pmx, numResto);
            //retDataHs.Stop();
            if (logXmlToData)
            {
                //_logger.Debug("Durée : Recuperation des données db depuis objet Hs", numResto, retDataHs.Elapsed);
            }




            DataTable dtHs = GenHsDataTable(app_dday_hourly_sales);
            DataTable dtHp = GenHpDataTable(app_dday_hourly_pmx);
            using (SqlCommand cmdHp = new SqlCommand("exec SPW_INSERT_DDAY_HOURLY_PMX @list , @business_date='" + dateActivity.ToString(dateFormat) + "'", con))
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
            using (SqlCommand cmdHs = new SqlCommand("exec SPW_INSERT_DDAY_HOURLY_SALES @list , @business_date='" + dateActivity.ToString(dateFormat) + "'", con))
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
        }

        public static decimal GetDouble(string value, decimal defaultValue)
        {
            
            decimal result;
            NumberStyles style;
            CultureInfo provider;
            style = NumberStyles.Number;

            provider = new CultureInfo("en-US");
            result = Decimal.Parse(value, style, provider);


            
            return result*10;
        }
        // Récupération des données dépuis les fichiers xml HourlySales
        private void MapToSales(HourlySales hourlySales, List<APP_DDAY_HOURLY_SALES> app_dday_hourly_sales, List<APP_DDAY_HOURLY_PMX> app_dday_hourly_pmx, int? numResto = null)
        {
            if (logXmlToData)
            {
                _logger.Debug("Parcours de l'objet HourlySales pour récuperation de données", numResto);
            }

            CultureInfo usCulture = new CultureInfo("en-US");
            NumberFormatInfo dbNumberFormat = usCulture.NumberFormat;
            



            hourlySales.POD.ForEach((POD pod) =>
            {
                if (pod.StoreTotal != null)
                {
                    pod.StoreTotal.Sales.ForEach((Sales sales) =>
                    {
                        short DDHS_DISCOUNT_IN_TAC_QY = 0;
                        short DDHS_DISCOUNT_OUT_TAC_QY = 0;
                        decimal DDHS_DISCOUNT_IN_SALES_AM = 0;
                        decimal DDHS_DISCOUNT_OUT_SALES_AM = 0;



                        sales.Product.ForEach((Product product) =>
                        {
                            if (product.Id != "11111")
                            {
                                int DDHP_PROD_ID = int.Parse(product.Id);
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
                                DDHP_SALES_TM = short.Parse(hourlySales.DayPartitioning.Segment.FirstOrDefault(seg => seg.Id == sales.Id.ToString()).BegTime);

                                product.OperationType.ForEach((OperationType opT) =>
                                {


                                    if (opT.operationType == "SALE")
                                    {
                                        DDHP_EAT_IN_QY = short.Parse(opT.PMix.QtyEatIn);
                                        DDHP_TAKE_OUT_QY = short.Parse(opT.PMix.QtyTakeOut);
                                        DDHP_CA_IN_AM = GetDouble(opT.PMix.EatInNetAmount,decimal.Parse(opT.PMix.EatInNetAmount, dbNumberFormat)) ;
                                        DDHP_CA_OUT_AM = GetDouble(opT.PMix.TakeOutNetAmount, decimal.Parse(opT.PMix.TakeOutNetAmount, dbNumberFormat)) ;
                                    }
                                    if (opT.operationType == "PROMO")
                                    {
                                        DDHP_PROMO_IN_QY = short.Parse(opT.PMix.QtyEatIn);
                                        DDHP_PROMO_OUT_QY = short.Parse(opT.PMix.QtyTakeOut);
                                    }
                                    if (opT.operationType == "DISCOUNT")
                                    {
                                        DDHP_DISCOUNT_IN_QY = short.Parse(opT.PMix.QtyEatIn);
                                        DDHP_DISCOUNT_OUT_QY = short.Parse(opT.PMix.QtyTakeOut);
                                        DDHP_EAT_IN_QY += short.Parse(opT.PMix.QtyEatIn);
                                        DDHP_TAKE_OUT_QY += short.Parse(opT.PMix.QtyTakeOut);
                                        DDHP_CA_IN_AM += GetDouble(opT.PMix.EatInNetAmount, decimal.Parse(opT.PMix.EatInNetAmount, dbNumberFormat)) ;
                                        DDHP_CA_OUT_AM += GetDouble(opT.PMix.EatInNetAmount, decimal.Parse(opT.PMix.TakeOutNetAmount, dbNumberFormat)) ;

                                        DDHS_DISCOUNT_IN_TAC_QY += short.Parse(opT.PMix.QtyEatIn);
                                        DDHS_DISCOUNT_OUT_TAC_QY += short.Parse(opT.PMix.QtyTakeOut);
                                        DDHS_DISCOUNT_IN_SALES_AM += GetDouble(opT.PMix.EatInNetAmount, decimal.Parse(opT.PMix.EatInNetAmount, dbNumberFormat)) ;
                                        DDHS_DISCOUNT_OUT_SALES_AM += GetDouble(opT.PMix.TakeOutNetAmount,decimal.Parse(opT.PMix.TakeOutNetAmount, dbNumberFormat)) ;


                                    }
                                    if (opT.operationType == "CREW")
                                    {
                                        DDHP_EMPLOYEE_MEAL_QY = (short)(int.Parse(opT.PMix.QtyEatIn) + int.Parse(opT.PMix.QtyTakeOut));



                                        DDHP_EMPLOYEE_MEAL_AM = GetDouble(opT.PMix.EatInNetAmount, decimal.Parse(opT.PMix.EatInNetAmount, dbNumberFormat))  + GetDouble(opT.PMix.TakeOutNetAmount, decimal.Parse(opT.PMix.TakeOutNetAmount, dbNumberFormat)) ;
                                    }
                                    if (opT.operationType == "MANAGER")
                                    {
                                        DDHP_MGR_MEAL_QY = (short)(short.Parse(opT.PMix.QtyEatIn) + short.Parse(opT.PMix.QtyTakeOut));
                                    }
                                    if (opT.operationType == "REFUND")
                                    {
                                        DDHP_EAT_IN_QY -= short.Parse(opT.PMix.QtyEatIn);
                                        DDHP_TAKE_OUT_QY -= short.Parse(opT.PMix.QtyTakeOut);
                                        DDHP_CA_IN_AM -= GetDouble(opT.PMix.EatInNetAmount, decimal.Parse(opT.PMix.EatInNetAmount, dbNumberFormat)) ;
                                        DDHP_CA_OUT_AM -=GetDouble(opT.PMix.TakeOutNetAmount, decimal.Parse(opT.PMix.TakeOutNetAmount, dbNumberFormat)) ;
                                    }


                                });
                                APP_DDAY_HOURLY_PMX app_hourly_pmx = new APP_DDAY_HOURLY_PMX
                                {
                                    DDHP_SITE_ID = (short)numResto,
                                    DDHP_MCDE_SIR_ID = (byte)(Pod_SIR_ID.Any(pod_sir => pod_sir.PDID_PODSHORT == pod.PodShort) ? Pod_SIR_ID.FirstOrDefault(pod_sir => pod_sir.PDID_PODSHORT == pod.PodShort).PDID_MCDE_SIR_ID : 0),
                                    DDHP_MVAL_SIR_ID = (byte)(Pod_SIR_ID.Any(pod_sir => pod_sir.PDID_PODSHORT == pod.PodShort) ? Pod_SIR_ID.FirstOrDefault(pod_sir => pod_sir.PDID_PODSHORT == pod.PodShort).PDID_MVAL_SIR_ID : 0),
                                    DDHP_LLVR_SIR_ID = (byte)(Pod_SIR_ID.Any(pod_sir => pod_sir.PDID_PODSHORT == pod.PodShort) ? Pod_SIR_ID.FirstOrDefault(pod_sir => pod_sir.PDID_PODSHORT == pod.PodShort).PDID_LLVR_SIR_ID : 0),
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
                                    DDHP_BUSINESS_DT = DateTime.Now,
                                    DDHP_PROCESS_DT = DateTime.Now
                                };
                                app_dday_hourly_pmx.Add(app_hourly_pmx);
                            }
                        });
                        APP_DDAY_HOURLY_SALES app_hourly_sale = new APP_DDAY_HOURLY_SALES
                        {

                            DDHS_SITE_ID = (short)numResto,
                            DDHS_MCDE_SIR_ID = (short)(Pod_SIR_ID.Any(pod_sir => pod_sir.PDID_PODSHORT == pod.PodShort) ? Pod_SIR_ID.FirstOrDefault(pod_sir => pod_sir.PDID_PODSHORT == pod.PodShort).PDID_MCDE_SIR_ID : 0),
                            DDHS_MVAL_SIR_ID = (short)(Pod_SIR_ID.Any(pod_sir => pod_sir.PDID_PODSHORT == pod.PodShort) ? Pod_SIR_ID.FirstOrDefault(pod_sir => pod_sir.PDID_PODSHORT == pod.PodShort).PDID_MVAL_SIR_ID : 0),
                            DDHS_LLVR_SIR_ID = (short)(Pod_SIR_ID.Any(pod_sir => pod_sir.PDID_PODSHORT == pod.PodShort) ? Pod_SIR_ID.FirstOrDefault(pod_sir => pod_sir.PDID_PODSHORT == pod.PodShort).PDID_LLVR_SIR_ID : 0),
                            DDHS_SALES_TM = short.Parse(hourlySales.DayPartitioning.Segment.FirstOrDefault(seg => seg.Id == sales.Id.ToString()).BegTime),
                            DDHS_SALES_PROD_AM = GetDouble(sales.ProductNetAmount, decimal.Parse(sales.ProductNetAmount, dbNumberFormat)) ,
                            DDHS_SALES_NON_PROD_AM = GetDouble(sales.NetAmount,decimal.Parse(sales.NetAmount, dbNumberFormat))  -GetDouble(sales.ProductNetAmount, decimal.Parse(sales.ProductNetAmount, dbNumberFormat)) ,
                            DDHS_EAT_IN_SALES_AM = GetDouble(sales.EatInNetAmount, decimal.Parse(sales.EatInNetAmount, dbNumberFormat) ),
                            DDHS_TAKE_OUT_SALES_AM = GetDouble(sales.TakeOutNetAmount, decimal.Parse(sales.TakeOutNetAmount, dbNumberFormat)) ,
                            DDHS_EAT_IN_TAC_QY = Eat_In_Qy(sales.ExtTC, sales.EatInTC, sales.TakeOutTC),
                            DDHS_TAKE_OUT_TAC_QY = Take_Out_Qy(sales.ExtTC, sales.EatInTC, sales.TakeOutTC),
                            DDHS_DISCOUNT_IN_TAC_QY = DDHS_DISCOUNT_IN_TAC_QY,
                            DDHS_DISCOUNT_OUT_TAC_QY = DDHS_DISCOUNT_OUT_TAC_QY,
                            DDHS_DISCOUNT_IN_SALES_AM = DDHS_DISCOUNT_IN_SALES_AM,
                            DDHS_DISCOUNT_OUT_SALES_AM = DDHS_DISCOUNT_OUT_SALES_AM,
                            DDHS_CREW_HOURS_WORKED = 0,
                            DDHS_PROCESS_DT = DateTime.Now,
                            DDHS_BUSINESS_DT = DateTime.Now,

                        };

                        app_dday_hourly_sales.Add(app_hourly_sale);
                    });
                }
            });
            if (logXmlToData)
            {
                _logger.Debug("Fin du parcours de l'objet HourlySales", numResto);
            }
        }


        //Conversion des données récupérées en table de données pour l'intégration dans la base de données
        private DataTable GenHsDataTable(List<APP_DDAY_HOURLY_SALES> hsList)
        {
            DataTable dtHs = new DataTable();

            DataColumn DDHS_SITE_ID = new DataColumn("DDHS_SITE_ID");

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


            dtHs.Columns.Add(DDHS_SITE_ID);

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


            hsList.ForEach((APP_DDAY_HOURLY_SALES hs) =>
            {
                DataRow d = dtHs.NewRow();
                d["DDHS_SITE_ID"] = hs.DDHS_SITE_ID;

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


                dtHs.Rows.Add(d);

            });
            return dtHs;
        }

        private DataTable GenHpDataTable(List<APP_DDAY_HOURLY_PMX> hpList)
        {
            DataTable dtHp = new DataTable();

            DataColumn DDHP_SITE_ID = new DataColumn("DDHP_SITE_ID");

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




            dtHp.Columns.Add(DDHP_SITE_ID);

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




            hpList.ForEach((APP_DDAY_HOURLY_PMX hp) =>
            {
                DataRow d = dtHp.NewRow();
                d["DDHP_SITE_ID"] = hp.DDHP_SITE_ID;

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


                dtHp.Rows.Add(d);
            });

            return dtHp;
        }

        public short Eat_In_Qy(string extTC_str, string eatInTC_str, string takeOutTC_str)
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

        public List<string> RestoList()
        {
            List<string> result = new List<string>();
            VW_From_InfoSite4u_APP_SITE.ForEach((VW_From_InfoSite4u_Resto_APP_SITE entry) =>
            {
                result.Add(entry.SITE_ID.ToString());
                Console.WriteLine(entry.SITE_ID.ToString());
                Console.WriteLine(result.Count);
            });
            return result;
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
