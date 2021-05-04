
/* View: VW_CMU_PMIX_PRODUCT, Owner: SIRXP */

CREATE VIEW "VW_CMU_PMIX_PRODUCT" (
  "PRODUCT_ID", 
  "EAT_IN_PRICE", 
  "EAT_IN_TAX", 
  "TAKE_OUT_PRICE", 
  "TAKE_OUT_TAX"
) AS

SELECT
  PRODUCT_ID
, CAST(0 AS NUMERIC(6,2))
, CAST(0 AS NUMERIC(6,2))
, CAST(0 AS NUMERIC(6,2))
, CAST(0 AS NUMERIC(6,2))
FROM
  CMU_PMIX_PRODUCT
WHERE
  EAT_IN_PRICE = 0 OR TAKE_OUT_PRICE = 0
UNION ALL
SELECT
  PRODUCT_ID
, EAT_IN_PRICE
, EAT_IN_TAX
, TAKE_OUT_PRICE
, TAKE_OUT_TAX
FROM
  CMU_PMIX_PRODUCT
WHERE
  (EAT_IN_PRICE <> 0 OR EAT_IN_PRICE IS NULL)
  AND (TAKE_OUT_PRICE <> 0 OR TAKE_OUT_PRICE IS NULL)
;
