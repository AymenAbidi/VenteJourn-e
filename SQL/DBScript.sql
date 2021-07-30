create database McDashboard_Rect
use McDashboard_Rect

delete from APP_DDAY_HOURLY_SALES
delete from APP_DDAY_HOURLY_PMX
delete from TRA_LOG

select * from TRA_LOG where LOG_MESSAGE like '%JOB%'
 
update APP_DDAY_HOURLY_SALES set DDHS_SITE_ID=2

select sum(DDHS_SALES_PROD_AM) from APP_DDAY_HOURLY_SALES order by DDHS_SALES_PROD_AM
select DDHS_SALES_PROD_AM from APP_DDAY_HOURLY_SALES where DDHS_SALES_TM=1515
select sum(DDHS_SALES_PROD_AM) from APP_DDAY_HOURLY_SALES where DDHS_SALES_TM <1515
select * from APP_DDAY_HOURLY_PMX where DDHP_SALES_TM <1515 and DDHP_MCDE_SIR_ID=1 and DDHP_MVAL_SIR_ID=2
select * from APP_DDAY_HOURLY_SALES where DDHS_TAKE_OUT_SALES_AM=11.36
select * from APP_DDAY_HOURLY_SALES order by DDHS_SALES_TM
select * from APP_DDAY_HOURLY_PMX where  DDHP_DISCOUNT_OUT_QY <> 0
select sum(DDHP_CA_OUT_AM) from APP_DDAY_HOURLY_PMX where DDHP_SALES_TM <1515
select sum(DDHP_MGR_MEAL_QY) from APP_DDAY_HOURLY_PMX where DDHP_SALES_TM <1515

CREATE TABLE [dbo].[APP_DDAY_HOURLY_SALES](	
    [DDHS_ID] [int] IDENTITY(1,1) NOT NULL,
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
	[DDHS_PROCESS_DT] [datetime] NULL,
	PRIMARY KEY(DDHS_ID)
) ON [PRIMARY]	

CREATE TABLE [dbo].[APP_DDAY_HOURLY_PMX](	
    [DDHP_ID] [int] IDENTITY(1,1) NOT NULL,
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
	[DDHP_PROCESS_DT] [datetime] NOT NULL,
	PRIMARY KEY(DDHP_ID)
) ON [PRIMARY]	

CREATE TABLE[dbo].[RFU_POD_SIR_ID](
[PDID_PODSHORT][varchar](255) NOT NULL,
[PDID_MCDE_SIR_ID][int] NOT NULL,
[PDID_MVAL_SIR_ID][int] NOT NULL,
[PDID_LLVR_SIR_ID][int] NOT NULL,
PRIMARY KEY(PDID_PODSHORT)
) ON [PRIMARY]


INSERT [dbo].[RFU_POD_SIR_ID](PDID_PODSHORT,PDID_MCDE_SIR_ID,PDID_MVAL_SIR_ID,PDID_LLVR_SIR_ID) VALUES ( 'FC',	1,	1,	1);
INSERT [dbo].[RFU_POD_SIR_ID](PDID_PODSHORT,PDID_MCDE_SIR_ID,PDID_MVAL_SIR_ID,PDID_LLVR_SIR_ID) VALUES ( 'DT',	1,	2,	2);
INSERT [dbo].[RFU_POD_SIR_ID](PDID_PODSHORT,PDID_MCDE_SIR_ID,PDID_MVAL_SIR_ID,PDID_LLVR_SIR_ID) VALUES ( 'CSO',	3,	9,	3);
INSERT [dbo].[RFU_POD_SIR_ID](PDID_PODSHORT,PDID_MCDE_SIR_ID,PDID_MVAL_SIR_ID,PDID_LLVR_SIR_ID) VALUES ( 'MCC',	4,	3,	4);
INSERT [dbo].[RFU_POD_SIR_ID](PDID_PODSHORT,PDID_MCDE_SIR_ID,PDID_MVAL_SIR_ID,PDID_LLVR_SIR_ID) VALUES ( 'DLV',	6,	10,	3);
INSERT [dbo].[RFU_POD_SIR_ID](PDID_PODSHORT,PDID_MCDE_SIR_ID,PDID_MVAL_SIR_ID,PDID_LLVR_SIR_ID) VALUES ( 'CK',	6,	10,	3);
INSERT [dbo].[RFU_POD_SIR_ID](PDID_PODSHORT,PDID_MCDE_SIR_ID,PDID_MVAL_SIR_ID,PDID_LLVR_SIR_ID) VALUES ( 'MCE',	1,	5,	5);
INSERT [dbo].[RFU_POD_SIR_ID](PDID_PODSHORT,PDID_MCDE_SIR_ID,PDID_MVAL_SIR_ID,PDID_LLVR_SIR_ID) VALUES ( 'CKD',	6,	8,	8);

CREATE TABLE [dbo].[TRA_LOG](
	[LOG_ID] [int] IDENTITY(1,1) NOT NULL,
	[LOG_DATE] [datetime] NOT NULL,
	[LOG_RESTAURANTID] [varchar](255) NOT NULL,
	[LOG_DUREE] [varchar](255) NOT NULL,
	[LOG_THREAD] [varchar](255) NOT NULL,
	[LOG_LEVEL] [varchar](50) NOT NULL,
	[LOG_LOGGER] [varchar](255) NOT NULL,
	[LOG_MESSAGE] [varchar](4000) NOT NULL,
	[LOG_PROCESSLOGID] [varchar](50) NOT NULL,
	[LOG_EXCEPTION] [varchar](2000) NULL
) ON [PRIMARY]

use McDashboard_Rect
select * from TRA_LOG where LOG_DUREE like '___.%' and LOG_MESSAGE like '%Requete Data%' order by LOG_DUREE desc
select * from APP_DDAY_HOURLY_SALES
delete from TRA_LOG 

CREATE TYPE [dbo].[APP_DDAY_HOURLY_PMX_TYPE] AS TABLE(	
	[DDHP_SITE_ID] [smallint] NOT NULL,
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
	[DDHP_CA_OUT_AM] [smallmoney] NULL
	
);
drop type APP_DDAY_HOURLY_SALES_TYPE
drop procedure SPW_INSERT_DDAY_HOURLY_SALES_2

CREATE TYPE [dbo].[APP_DDAY_HOURLY_SALES_TYPE] AS TABLE(	
	[DDHS_SITE_ID] [smallint] NOT NULL,
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
	[DDHS_CREW_HOURS_WORKED] [numeric](12, 3) NULL

);

