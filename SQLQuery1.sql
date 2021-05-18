use McDashboard

Delete from APP_DDAY_HOURLY_SALES where DDHS_BUSINESS_DT='2018-10-03 00:00:00'

select * from log

CREATE TYPE [dbo].[APP_DDAY_HOURLY_PMX_TYPE] AS TABLE(	
	[DDHP_SITE_ID] [smallint] NOT NULL,
	[DDHP_BUSINESS_DT] [smalldatetime] NOT NULL,
	[DDHP_PROD_ID] [int] NOT NULL,
	[DDHP_SALES_TM] [smallint] NOT NULL,
	[DDHP_MCDE_SIR_ID] [tinyint] NOT NULL,
	[DDHP_MVAL_SIR_ID] [tinyint] NOT NULL,
	[DDHP_LLVR_SIR_ID] [tinyint] NOT NULL,
	[DDHP_EAT_IN_QY] [smallint] NOT NULL,
	[DDHP_TAKE_OUT_QY] [smallint] NOT NULL,
	[DDHP_PROMO_IN_QY] [smallint] NULL,
	[DDHP_PROMO_OUT_QY] [smallint] NULL,
	[DDHP_DISCOUNT_IN_QY] [smallint] NULL,
	[DDHP_DISCOUNT_OUT_QY] [smallint] NULL,
	[DDHP_EMPLOYEE_MEAL_QY] [smallint] NULL,
	[DDHP_MGR_MEAL_QY] [smallint] NULL,
	[DDHP_EMPLOYEE_MEAL_AM] [money] NULL,
	[DDHP_CA_IN_AM] [smallmoney] NULL,
	[DDHP_CA_OUT_AM] [smallmoney] NULL,
	[DDHP_PROCESS_DT] [datetime] NOT NULL
);

CREATE TYPE [dbo].[APP_DDAY_HOURLY_SALES_TYPE] AS TABLE(	
	[DDHS_SITE_ID] [smallint] NOT NULL,
	[DDHS_BUSINESS_DT] [smalldatetime] NOT NULL,
	[DDHS_SALES_TM] [smallint] NOT NULL,
	[DDHS_MCDE_SIR_ID] [smallint] NOT NULL,
	[DDHS_MVAL_SIR_ID] [smallint] NOT NULL,
	[DDHS_LLVR_SIR_ID] [smallint] NOT NULL,
	[DDHS_SALES_PROD_AM] [money] NULL,
	[DDHS_SALES_NON_PROD_AM] [money] NULL,
	[DDHS_EAT_IN_TAC_QY] [smallint] NULL,
	[DDHS_TAKE_OUT_TAC_QY] [smallint] NULL,
	[DDHS_EAT_IN_SALES_AM] [money] NULL,
	[DDHS_TAKE_OUT_SALES_AM] [money] NULL,
	[DDHS_DISCOUNT_IN_TAC_QY] [smallint] NULL,
	[DDHS_DISCOUNT_OUT_TAC_QY] [smallint] NULL,
	[DDHS_DISCOUNT_IN_SALES_AM] [money] NULL,
	[DDHS_DISCOUNT_OUT_SALES_AM] [money] NULL,
	[DDHS_CREW_HOURS_WORKED] [numeric](12, 3) NULL,
	[DDHS_PROCESS_DT] [datetime] NULL

);


declare @list as APP_DDAY_HOURLY_SALES_TYPE
insert into @list select * from APP_DDAY_HOURLY_SALES
exec sp_insert_ddhp_hourly_sales @list
commit

