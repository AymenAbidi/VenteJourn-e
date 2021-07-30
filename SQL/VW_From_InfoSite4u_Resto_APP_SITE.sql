CREATE  VIEW [dbo].[VW_From_InfoSite4u_Resto_APP_SITE]
AS	
SELECT	SITE_ID
,	SITE_TX
from	[Infosite4U_Rect].dbo.VW_For_McDashboard_APP_SITE
GO