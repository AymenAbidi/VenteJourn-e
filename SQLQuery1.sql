use McDashboard

Delete from APP_DDAY_HOURLY_PMX where DDHP_SITE_ID=1

select * from APP_DDAY_HOURLY_PMX

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
declare @list as APP_DDAY_HOURLY_PMX_TYPE
insert into @list select * from APP_DDAY_HOURLY_PMX
exec sp_insert_ddhp_hourly_pmx @list
commit

