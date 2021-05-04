COMMIT WORK;
SET AUTODDL OFF;
SET TERM ^ ;

/* Stored procedures */

CREATE PROCEDURE "SPW_Cash_SaveProductMix" 
(
  "BUSINESSDATE" DATE
)
AS
BEGIN EXIT; END ^


ALTER PROCEDURE "SPW_Cash_SaveProductMix" 
(
  "BUSINESSDATE" DATE
)
AS
DECLARE VARIABLE ProductCode         INTEGER;
DECLARE VARIABLE QuarterTime         TIMESTAMP;
DECLARE VARIABLE SoldEatInQy         SMALLINT;
DECLARE VARIABLE SoldTakeOutQy       SMALLINT;
DECLARE VARIABLE PromoEatInQy        SMALLINT;
DECLARE VARIABLE PromoTakeOutQy      SMALLINT;
DECLARE VARIABLE DiscountEatInQy     SMALLINT;
DECLARE VARIABLE DiscountTakeOutQy   SMALLINT;
DECLARE VARIABLE EatInNetAm          NUMERIC(8, 2);
DECLARE VARIABLE TakeOutNetAm        NUMERIC(8, 2);
DECLARE VARIABLE TotalTaxAm          NUMERIC(8, 2);
BEGIN
  /*************************************
   * Mise à jour du PMX à la journée   *
   *************************************/
  /* Suppression des données existantes */
  DELETE FROM CSHPMXJOUR WHERE CAL_DTACTIVITE = :BusinessDate;
  /* Récupération des Ventes, Pomos, Repas Employés/Managers et Pertes à partir du POS */
  INSERT INTO CSHPMXJOUR
  (
      CAL_DTACTIVITE
    , PRV_COD
    , PVJ_PRIXBRUTSP
    , PVJ_PRIXBRUTEMP
    , PVJ_PRIXNETSP
    , PVJ_PRIXNETEMP
    , PVJ_QTESP
    , PVJ_QTEEMP
    , PVJ_QTEPROMOS
    , PVJ_QTEPERTES
    , PVJ_QTEREPASEQP
    , PVJ_QTEREPASMGR
    , PVJ_MTREPASEMP
    , PVJ_QTEDISCOUNTSP
    , PVJ_QTEDISCOUNTEMP
    , PVJ_QTEDISCOUNT
    , PVJ_EAT_IN_NET_AM
    , PVJ_TAKE_OUT_NET_AM
    , PVJ_TOTAL_GROSS_AM
    , PVJ_FLAGDFU
  )
    SELECT
      :BusinessDate
    , PMX.PRODUCT_CODE
    , CAST(F_IFTHENELSE('=', PRO.EAT_IN_PRICE, 0, 0, PRI.EAT_IN_PRICE) AS NUMERIC(6,2))
    , CAST(F_IFTHENELSE('=', PRO.TAKE_OUT_PRICE, 0, 0, PRI.TAKE_OUT_PRICE) AS NUMERIC(6,2))
    , CAST(F_IFTHENELSE('=', PRO.EAT_IN_PRICE, 0, 0, PRI.NET_EAT_IN_PRICE) AS NUMERIC(6,2))
    , CAST(F_IFTHENELSE('=', PRO.TAKE_OUT_PRICE, 0, 0, PRI.NET_TAKE_OUT_PRICE) AS NUMERIC(6,2))
    , PMX.SOLD_EAT_IN_QY
    , PMX.SOLD_TAKE_OUT_QY
    , PMX.PROMO_EAT_IN_QY + PMX.PROMO_TAKE_OUT_QY
    , PMX.WASTE_EAT_IN_QY + PMX.WASTE_TAKE_OUT_QY + Z(PER.PRP_TOTQTEPERTES)
    , PMX.CREW_MEAL_EAT_IN_QY + PMX.CREW_MEAL_TAKE_OUT_QY + Z(PER.PRP_TOTQTEREPASEQP)
    , PMX.MANAGER_MEAL_EAT_IN_QY + PMX.MANAGER_MEAL_TAKE_OUT_QY
    , 0     /* PVJ_MTREPASEMP */
    , PMX.DISCOUNT_EAT_IN_QY
    , PMX.DISCOUNT_TAKE_OUT_QY
    , PMX.DISCOUNT_EAT_IN_QY + PMX.DISCOUNT_TAKE_OUT_QY
    , PMX.EAT_IN_NET_AM
    , PMX.TAKE_OUT_NET_AM
    , PMX.EAT_IN_NET_AM + PMX.TAKE_OUT_NET_AM + PMX.TOTAL_TAX_AM
    , '0'   /* PVJ_FLAGDFU */
    FROM
      VW_CMU_DAILY_PMX PMX
      LEFT OUTER JOIN CSHPERTEREPAS PER ON PMX.PRODUCT_CODE = PER.PRV_COD
      LEFT OUTER JOIN VW_CMU_PMIX_PRODUCT PRO ON PRO.PRODUCT_ID = PMX.PRODUCT_CODE
      LEFT OUTER JOIN VW_PRODUCT_PRICE PRI ON PRI.PRODUCT_CODE = PMX.PRODUCT_CODE;
  /* Récupération des Repas Employés/Managers et Pertes à partir de SIR pour les produits non vendus */
  INSERT INTO CSHPMXJOUR
  (
      CAL_DTACTIVITE
    , PRV_COD
    , PVJ_PRIXBRUTSP
    , PVJ_PRIXBRUTEMP
    , PVJ_PRIXNETSP
    , PVJ_PRIXNETEMP
    , PVJ_QTESP
    , PVJ_QTEEMP
    , PVJ_QTEPROMOS
    , PVJ_QTEPERTES
    , PVJ_QTEREPASEQP
    , PVJ_QTEREPASMGR
    , PVJ_MTREPASEMP
    , PVJ_QTEDISCOUNTSP
    , PVJ_QTEDISCOUNTEMP
    , PVJ_QTEDISCOUNT
    , PVJ_EAT_IN_NET_AM
    , PVJ_TAKE_OUT_NET_AM
    , PVJ_TOTAL_GROSS_AM
    , PVJ_FLAGDFU
  )
    SELECT
      :BusinessDate
    , PER.PRV_COD
    , PRI.EAT_IN_PRICE
    , PRI.TAKE_OUT_PRICE
    , PRI.NET_EAT_IN_PRICE
    , PRI.NET_TAKE_OUT_PRICE
    , 0     /* PVJ_QTESP */
    , 0     /* PVJ_QTEEMP */
    , 0     /* PVJ_QTEPROMOS */
    , Z(PER.PRP_TOTQTEPERTES)
    , Z(PER.PRP_TOTQTEREPASEQP)
    , Z(PER.PRP_TOTQTEREPASMGR)
    , 0     /* PVJ_MTREPASEMP */
    , 0     /* PVJ_QTEDISCOUNTSP */
    , 0     /* PVJ_QTEDISCOUNTEMP */
    , 0     /* PVJ_QTEDISCOUNT */
    , 0     /* PVJ_EAT_IN_NET_AM */
    , 0     /* PVJ_TAKE_OUT_NET_AM */
    , 0     /* PVJ_TOTAL_GROSS_AM */
    , '0'   /* PVJ_FLAGDFU */
    FROM
      CSHPERTEREPAS PER
      LEFT OUTER JOIN VW_PRODUCT_PRICE PRI ON PRI.PRODUCT_CODE = PER.PRV_COD
      LEFT OUTER JOIN VW_CMU_DAILY_PMX PMX ON PMX.PRODUCT_CODE = PER.PRV_COD
    WHERE
      PMX.PRODUCT_CODE IS NULL;
  /*************************************
   * Mise à jour du PMX au 1/4 d'heure *
   *************************************/
  DELETE FROM CSHPMXHOR WHERE CAL_DTACTIVITE = :BusinessDate;
  /* Récupération des Ventes totales */
  INSERT INTO CSHPMXHOR
  (
      CAL_DTACTIVITE, PRV_COD, PHR_HR
    , PHR_VENDUSP, PHR_VENDUEMP
    , PHR_VENDUDRV
    , PHR_PROMOSP, PHR_PROMOEMP
    , PHR_DISCSP, PHR_DISCEMP
    , PMX_TOT_NET_AM, PMX_TOT_GROSS_AM
    , PHR_FLAGDFU
  )
    SELECT
      :BusinessDate, PRODUCT_CODE, QUARTER_TIME
    , SOLD_EAT_IN_QY, SOLD_TAKE_OUT_QY
    , 0        /* PHR_VENDUDRV */
    , PROMO_EAT_IN_QY, PROMO_TAKE_OUT_QY
    , DISCOUNT_EAT_IN_QY, DISCOUNT_TAKE_OUT_QY
    , EAT_IN_NET_AM + TAKE_OUT_NET_AM
    , EAT_IN_NET_AM + TAKE_OUT_NET_AM + TOTAL_TAX_AM
    , '0'
    FROM VW_CMU_QUARTER_HOUR_PMX
    WHERE
      (POD_TYPE = 'ALL')
      AND (SOLD_EAT_IN_QY <> 0 OR SOLD_TAKE_OUT_QY <> 0 OR
           PROMO_EAT_IN_QY <> 0 OR PROMO_TAKE_OUT_QY <> 0 OR
           DISCOUNT_EAT_IN_QY <> 0 OR DISCOUNT_TAKE_OUT_QY <> 0);
  /* Mise à jour des Ventes Comptoir (FC) */
  FOR SELECT
        PRODUCT_CODE, QUARTER_TIME
      , SOLD_EAT_IN_QY, SOLD_TAKE_OUT_QY
      , PROMO_EAT_IN_QY, PROMO_TAKE_OUT_QY
      , DISCOUNT_EAT_IN_QY, DISCOUNT_TAKE_OUT_QY
      , EAT_IN_NET_AM, TAKE_OUT_NET_AM
      , TOTAL_TAX_AM
      FROM VW_CMU_QUARTER_HOUR_PMX
      WHERE
        (POD_TYPE = 'FC')
        AND (SOLD_EAT_IN_QY <> 0 OR SOLD_TAKE_OUT_QY <> 0 OR
             PROMO_EAT_IN_QY <> 0 OR PROMO_TAKE_OUT_QY <> 0 OR
             DISCOUNT_EAT_IN_QY <> 0 OR DISCOUNT_TAKE_OUT_QY <> 0)
      INTO
        :ProductCode, :QuarterTime
      , :SoldEatInQy, :SoldTakeOutQy
      , :PromoEatInQy, :PromoTakeOutQy
      , :DiscountEatInQy, :DiscountTakeOutQy
      , :EatInNetAm, :TakeOutNetAm
      , :TotalTaxAm
  DO
  BEGIN
    UPDATE CSHPMXHOR
    SET
      PMX_FRC_EAT_IN_SOLD_QY = :SoldEatInQy
    , PMX_FRC_TAKE_OUT_SOLD_QY = :SoldTakeOutQy
    , PMX_FRC_EAT_IN_PROMO_QY = :PromoEatInQy
    , PMX_FRC_TAKE_OUT_PROMO_QY = :PromoTakeOutQy
    , PMX_FRC_EAT_IN_DISCOUNT_QY = :DiscountEatInQy
    , PMX_FRC_TAKE_OUT_DISCOUNT_QY = :DiscountTakeOutQy
    , PMX_FRC_NET_AM = :EatInNetAm + :TakeOutNetAm
    , PMX_FRC_GROSS_AM = :EatInNetAm + :TakeOutNetAm + :TotalTaxAm
    WHERE
      (CAL_DTACTIVITE = :BusinessDate) AND (PHR_HR = :QuarterTime) AND (PRV_COD = :ProductCode);
  END
  /* Mise à jour des Ventes Drive (DT) */
  FOR SELECT
        PRODUCT_CODE, QUARTER_TIME
      , SOLD_EAT_IN_QY, SOLD_TAKE_OUT_QY
      , PROMO_EAT_IN_QY, PROMO_TAKE_OUT_QY
      , DISCOUNT_EAT_IN_QY, DISCOUNT_TAKE_OUT_QY
      , EAT_IN_NET_AM, TAKE_OUT_NET_AM
      , TOTAL_TAX_AM
      FROM VW_CMU_QUARTER_HOUR_PMX
      WHERE
        (POD_TYPE = 'DT')
        AND (SOLD_EAT_IN_QY <> 0 OR SOLD_TAKE_OUT_QY <> 0 OR
             PROMO_EAT_IN_QY <> 0 OR PROMO_TAKE_OUT_QY <> 0 OR
             DISCOUNT_EAT_IN_QY <> 0 OR DISCOUNT_TAKE_OUT_QY <> 0)
      INTO
        :ProductCode, :QuarterTime
      , :SoldEatInQy, :SoldTakeOutQy
      , :PromoEatInQy, :PromoTakeOutQy
      , :DiscountEatInQy, :DiscountTakeOutQy
      , :EatInNetAm, :TakeOutNetAm
      , :TotalTaxAm
  DO
  BEGIN
    UPDATE CSHPMXHOR
    SET
      PHR_VENDUDRV = :SoldTakeOutQy
    , PMX_DRV_PROMO_QY = :PromoTakeOutQy
    , PMX_DRV_DISCOUNT_QY = :DiscountTakeOutQy
    , PMX_DRV_NET_AM = :TakeOutNetAm
    , PMX_DRV_GROSS_AM = :TakeOutNetAm + :TotalTaxAm
    WHERE
      (CAL_DTACTIVITE = :BusinessDate) AND (PHR_HR = :QuarterTime) AND (PRV_COD = :ProductCode);
  END
  /* Mise à jour des Ventes Kiosk (CSO) */
  FOR SELECT
        PRODUCT_CODE, QUARTER_TIME
      , SOLD_EAT_IN_QY, SOLD_TAKE_OUT_QY
      , PROMO_EAT_IN_QY, PROMO_TAKE_OUT_QY
      , DISCOUNT_EAT_IN_QY, DISCOUNT_TAKE_OUT_QY
      , EAT_IN_NET_AM, TAKE_OUT_NET_AM
      , TOTAL_TAX_AM
      FROM VW_CMU_QUARTER_HOUR_PMX
      WHERE
        (POD_TYPE = 'CSO')
        AND (SOLD_EAT_IN_QY <> 0 OR SOLD_TAKE_OUT_QY <> 0 OR
             PROMO_EAT_IN_QY <> 0 OR PROMO_TAKE_OUT_QY <> 0 OR
             DISCOUNT_EAT_IN_QY <> 0 OR DISCOUNT_TAKE_OUT_QY <> 0)
      INTO
        :ProductCode, :QuarterTime
      , :SoldEatInQy, :SoldTakeOutQy
      , :PromoEatInQy, :PromoTakeOutQy
      , :DiscountEatInQy, :DiscountTakeOutQy
      , :EatInNetAm, :TakeOutNetAm
      , :TotalTaxAm
  DO
  BEGIN
    UPDATE CSHPMXHOR
    SET
      PMX_KSK_SOLD_QY_IN = :SoldEatInQy
    , PMX_KSK_SOLD_QY_OUT= :SoldTakeOutQy
    , PMX_KSK_PROMO_QY_IN = :PromoEatInQy
    , PMX_KSK_PROMO_QY_OUT = :PromoTakeOutQy
    , PMX_KSK_DISCOUNT_QY_IN = :DiscountEatInQy
    , PMX_KSK_DISCOUNT_QY_OUT = :DiscountTakeOutQy
    , PMX_KSK_NET_AM = :EatInNetAm + :TakeOutNetAm
    , PMX_KSK_GROSS_AM = :EatInNetAm + :TakeOutNetAm + :TotalTaxAm
    WHERE
      (CAL_DTACTIVITE = :BusinessDate) AND (PHR_HR = :QuarterTime) AND (PRV_COD = :ProductCode);
  END
  /* Mise à jour des Ventes McCafé (MCC) */
  FOR SELECT
        PRODUCT_CODE, QUARTER_TIME
      , SOLD_EAT_IN_QY, SOLD_TAKE_OUT_QY
      , PROMO_EAT_IN_QY, PROMO_TAKE_OUT_QY
      , DISCOUNT_EAT_IN_QY, DISCOUNT_TAKE_OUT_QY
      , EAT_IN_NET_AM, TAKE_OUT_NET_AM
      , TOTAL_TAX_AM
      FROM VW_CMU_QUARTER_HOUR_PMX
      WHERE
        (POD_TYPE = 'MCC')
        AND (SOLD_EAT_IN_QY <> 0 OR SOLD_TAKE_OUT_QY <> 0 OR
             PROMO_EAT_IN_QY <> 0 OR PROMO_TAKE_OUT_QY <> 0 OR
             DISCOUNT_EAT_IN_QY <> 0 OR DISCOUNT_TAKE_OUT_QY <> 0)
      INTO
        :ProductCode, :QuarterTime
      , :SoldEatInQy, :SoldTakeOutQy
      , :PromoEatInQy, :PromoTakeOutQy
      , :DiscountEatInQy, :DiscountTakeOutQy
      , :EatInNetAm, :TakeOutNetAm
      , :TotalTaxAm
  DO
  BEGIN
    UPDATE CSHPMXHOR
    SET
      PMX_MCF_SOLD_QY_IN = :SoldEatInQy
    , PMX_MCF_SOLD_QY_OUT = :SoldTakeOutQy
    , PMX_MCF_PROMO_QY_IN = :PromoEatInQy
    , PMX_MCF_PROMO_QY_OUT = :PromoTakeOutQy
    , PMX_MCF_DISCOUNT_QY_IN = :DiscountEatInQy
    , PMX_MCF_DISCOUNT_QY_OUT = :DiscountTakeOutQy
    , PMX_MCF_NET_AM = :EatInNetAm + :TakeOutNetAm
    , PMX_MCF_GROSS_AM = :EatInNetAm + :TakeOutNetAm + :TotalTaxAm
    WHERE
      (CAL_DTACTIVITE = :BusinessDate) AND (PHR_HR = :QuarterTime) AND (PRV_COD = :ProductCode);
  END
/* Mise à jour des Ventes Web (DLV + CK) */
  FOR SELECT
        PRODUCT_CODE, QUARTER_TIME
      , SUM(SOLD_EAT_IN_QY), SUM(SOLD_TAKE_OUT_QY)
      , SUM(PROMO_EAT_IN_QY), SUM(PROMO_TAKE_OUT_QY)
      , SUM(DISCOUNT_EAT_IN_QY), SUM(DISCOUNT_TAKE_OUT_QY)
      , SUM(EAT_IN_NET_AM), SUM(TAKE_OUT_NET_AM)
      , SUM(TOTAL_TAX_AM)
      FROM VW_CMU_QUARTER_HOUR_PMX
      WHERE
        (POD_TYPE = 'DLV' OR POD_TYPE = 'CK')
        AND (SOLD_EAT_IN_QY <> 0 OR SOLD_TAKE_OUT_QY <> 0 OR
             PROMO_EAT_IN_QY <> 0 OR PROMO_TAKE_OUT_QY <> 0 OR
             DISCOUNT_EAT_IN_QY <> 0 OR DISCOUNT_TAKE_OUT_QY <> 0)
      GROUP BY PRODUCT_CODE, QUARTER_TIME
      INTO
        :ProductCode, :QuarterTime
      , :SoldEatInQy, :SoldTakeOutQy
      , :PromoEatInQy, :PromoTakeOutQy
      , :DiscountEatInQy, :DiscountTakeOutQy
      , :EatInNetAm, :TakeOutNetAm
      , :TotalTaxAm
  DO
  BEGIN
    UPDATE CSHPMXHOR
    SET
      PMX_WEB_EAT_IN_SOLD_QY = :SoldEatInQy
    , PMX_WEB_TAKE_OUT_SOLD_QY = :SoldTakeOutQy
    , PMX_WEB_EAT_IN_PROMO_QY = :PromoEatInQy
    , PMX_WEB_TAKE_OUT_PROMO_QY = :PromoTakeOutQy
    , PMX_WEB_EAT_IN_DISCOUNT_QY = :DiscountEatInQy
    , PMX_WEB_TAKE_OUT_DISCOUNT_QY = :DiscountTakeOutQy
    , PMX_WEB_NET_AM = :EatInNetAm + :TakeOutNetAm
    , PMX_WEB_GROSS_AM = :EatInNetAm + :TakeOutNetAm + :TotalTaxAm
    WHERE
      (CAL_DTACTIVITE = :BusinessDate) AND (PHR_HR = :QuarterTime) AND (PRV_COD = :ProductCode);
  END
END
 ^

SET TERM ; ^
COMMIT WORK;
SET AUTODDL ON;
