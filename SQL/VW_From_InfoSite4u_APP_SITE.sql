
CREATE  VIEW [dbo].[VW_From_InfoSite4u_Resto_APP_SITE]
AS	
SELECT	SITE_ID
,		SITE_TX
from    VW_For_DatalakeExploit_APP_SITE
GO

select * from VW_From_InfoSite4u_APP_SITE