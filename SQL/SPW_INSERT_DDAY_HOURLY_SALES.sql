CREATE PROCEDURE [dbo].[SPW_INSERT_DDAY_HOURLY_SALES]
    @list APP_DDAY_HOURLY_SALES_TYPE READONLY
AS
BEGIN
  
    INSERT INTO APP_DDAY_HOURLY_SALES (DDHS_SITE_ID,
DDHS_BUSINESS_DT,
DDHS_SALES_TM,
DDHS_MCDE_SIR_ID,
DDHS_MVAL_SIR_ID,
DDHS_LLVR_SIR_ID,
DDHS_SALES_PROD_AM,
DDHS_SALES_NON_PROD_AM,
DDHS_EAT_IN_TAC_QY,
DDHS_TAKE_OUT_TAC_QY,
DDHS_EAT_IN_SALES_AM,
DDHS_TAKE_OUT_SALES_AM,
DDHS_DISCOUNT_IN_TAC_QY,
DDHS_DISCOUNT_OUT_TAC_QY,
DDHS_DISCOUNT_IN_SALES_AM,
DDHS_DISCOUNT_OUT_SALES_AM,
DDHS_CREW_HOURS_WORKED,
DDHS_PROCESS_DT)
    SELECT *
    FROM @list;
    
END