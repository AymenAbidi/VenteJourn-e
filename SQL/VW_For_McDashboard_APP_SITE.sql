CREATE VIEW [dbo].[VW_For_McDashboard_APP_SITE]
AS	
SELECT	SITE_ID
,		SITE_TX
,		SITE_OPENING_DT
,		SITE_PERMANENT_CLOSURE_DT
from	dbo.APP_SITE


GO