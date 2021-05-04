CREATE OR ALTER VIEW VW_CMU_HOURLY_SALES_PMX(
    PRODUCT_CODE,
    QUARTER_TIME,
    POD_TYPE,
    OPERATION_TYPE,
    SOLD_EAT_IN_QY,
    SOLD_TAKE_OUT_QY,
    EAT_IN_NET_AM,
    TAKE_OUT_NET_AM,
    TOTAL_TAX_AM,
    SALE_EAT_IN_QY,
    SALE_TAKE_OUT_QY,
    REFUND_EAT_IN_QY,
    REFUND_TAKE_OUT_QY,
    PROMO_EAT_IN_QY,
    PROMO_TAKE_OUT_QY,
    DISCOUNT_EAT_IN_QY,
    DISCOUNT_TAKE_OUT_QY,
    WASTE_EAT_IN_QY,
    WASTE_TAKE_OUT_QY,
    CREW_EAT_IN_QY,
    CREW_TAKE_OUT_QY,
    MANAGER_EAT_IN_QY,
    MANAGER_TAKE_OUT_QY)
AS
/* R?cup?ration des Ventes */
SELECT PRODUCT_CODE,
       QUARTER_TIME,
       POD_TYPE,
       OPERATION_TYPE,
       EAT_IN_QY,              /* SOLD_EAT_IN_QY */
       TAKE_OUT_QY,            /* SOLD_TAKE_OUT_QY */
       EAT_IN_NET_AMOUNT,      /* EAT_IN_NET_AM */
       TAKE_OUT_NET_AMOUNT,    /* TAKE_OUT_NET_AM */
       TAX_AMOUNT,             /* TOTAL_TAX_AM */
       EAT_IN_QY,              /* SALE_EAT_IN_QY */
       TAKE_OUT_QY,            /* SALE_TAKE_OUT_QY */
       0,                      /* REFUND_EAT_IN_QY */
       0,                      /* REFUND_TAKE_OUT_QY */
       0,                      /* PROMO_EAT_IN_QY */
       0,                      /* PROMO_TAKE_OUT_QY */
       0,                      /* DISCOUNT_EAT_IN_QY */
       0,                      /* DISCOUNT_TAKE_OUT_QY */
       0,                      /* WASTE_EAT_IN_QY */
       0,                      /* WASTE_TAKE_OUT_QY */
       0,                      /* CREW_EAT_IN_QY */
       0,                      /* CREW_TAKE_OUT_QY */
       0,                      /* MANAGER_EAT_IN_QY */
       0                       /* MANAGER_TAKE_OUT_QY */
  FROM CMU_HOURLY_SALES_PMX
 WHERE OPERATION_TYPE = 'SALE'
 UNION ALL
/* R?cup?ration des Remboursements */
SELECT PRODUCT_CODE,
       QUARTER_TIME,
       POD_TYPE,
       OPERATION_TYPE,
       - EAT_IN_QY,            /* SOLD_EAT_IN_QY */
       - TAKE_OUT_QY,          /* SOLD_TAKE_OUT_QY */
       - EAT_IN_NET_AMOUNT,    /* EAT_IN_NET_AM */
       - TAKE_OUT_NET_AMOUNT,  /* TAKE_OUT_NET_AM */
       - TAX_AMOUNT,           /* TOTAL_TAX_AM */
       0,                      /* SALE_EAT_IN_QY */
       0,                      /* SALE_TAKE_OUT_QY */
       EAT_IN_QY,              /* REFUND_EAT_IN_QY */
       TAKE_OUT_QY,            /* REFUND_TAKE_OUT_QY */
       0,                      /* PROMO_EAT_IN_QY */
       0,                      /* PROMO_TAKE_OUT_QY */
       0,                      /* DISCOUNT_EAT_IN_QY */
       0,                      /* DISCOUNT_TAKE_OUT_QY */
       0,                      /* WASTE_EAT_IN_QY */
       0,                      /* WASTE_TAKE_OUT_QY */
       0,                      /* CREW_EAT_IN_QY */
       0,                      /* CREW_TAKE_OUT_QY */
       0,                      /* MANAGER_EAT_IN_QY */
       0                       /* MANAGER_TAKE_OUT_QY */
  FROM CMU_HOURLY_SALES_PMX
 WHERE OPERATION_TYPE = 'REFUND'
 UNION ALL
/* R?cup?ration des Promotions */
SELECT PRODUCT_CODE,
       QUARTER_TIME,
       POD_TYPE,
       OPERATION_TYPE,
       0,                      /* SOLD_EAT_IN_QY */
       0,                      /* SOLD_TAKE_OUT_QY */
       EAT_IN_NET_AMOUNT,      /* EAT_IN_NET_AM */
       TAKE_OUT_NET_AMOUNT,    /* TAKE_OUT_NET_AM */
       TAX_AMOUNT,             /* TOTAL_TAX_AM */
       0,                      /* SALE_EAT_IN_QY */
       0,                      /* SALE_TAKE_OUT_QY */
       0,                      /* REFUND_EAT_IN_QY */
       0,                      /* REFUND_TAKE_OUT_QY */
       EAT_IN_QY,              /* PROMO_EAT_IN_QY */
       TAKE_OUT_QY,            /* PROMO_TAKE_OUT_QY */
       0,                      /* DISCOUNT_EAT_IN_QY */
       0,                      /* DISCOUNT_TAKE_OUT_QY */
       0,                      /* WASTE_EAT_IN_QY */
       0,                      /* WASTE_TAKE_OUT_QY */
       0,                      /* CREW_EAT_IN_QY */
       0,                      /* CREW_TAKE_OUT_QY */
       0,                      /* MANAGER_EAT_IN_QY */
       0                       /* MANAGER_TAKE_OUT_QY */
  FROM CMU_HOURLY_SALES_PMX
 WHERE OPERATION_TYPE = 'PROMO'
 UNION ALL
/* R?cup?ration des Remises */
SELECT PRODUCT_CODE,
       QUARTER_TIME,
       POD_TYPE,
       OPERATION_TYPE,
       EAT_IN_QY,              /* SOLD_EAT_IN_QY */
       TAKE_OUT_QY,            /* SOLD_TAKE_OUT_QY */
       EAT_IN_NET_AMOUNT,      /* EAT_IN_NET_AM */
       TAKE_OUT_NET_AMOUNT,    /* TAKE_OUT_NET_AM */
       TAX_AMOUNT,             /* TOTAL_TAX_AM */
       0,                      /* SALE_EAT_IN_QY */
       0,                      /* SALE_TAKE_OUT_QY */
       0,                      /* REFUND_EAT_IN_QY */
       0,                      /* REFUND_TAKE_OUT_QY */
       0,                      /* PROMO_EAT_IN_QY */
       0,                      /* PROMO_TAKE_OUT_QY */
       EAT_IN_QY,              /* DISCOUNT_EAT_IN_QY */
       TAKE_OUT_QY,            /* DISCOUNT_TAKE_OUT_QY */
       0,                      /* WASTE_EAT_IN_QY */
       0,                      /* WASTE_TAKE_OUT_QY */
       0,                      /* CREW_EAT_IN_QY */
       0,                      /* CREW_TAKE_OUT_QY */
       0,                      /* MANAGER_EAT_IN_QY */
       0                       /* MANAGER_TAKE_OUT_QY */
  FROM CMU_HOURLY_SALES_PMX
 WHERE OPERATION_TYPE = 'DISCOUNT'
 UNION ALL
/* R?cup?ration des Pertes */
SELECT PRODUCT_CODE,
       QUARTER_TIME,
       POD_TYPE,
       OPERATION_TYPE,
       0,                      /* SOLD_EAT_IN_QY */
       0,                      /* SOLD_TAKE_OUT_QY */
       EAT_IN_NET_AMOUNT,      /* EAT_IN_NET_AM */
       TAKE_OUT_NET_AMOUNT,    /* TAKE_OUT_NET_AM */
       TAX_AMOUNT,             /* TOTAL_TAX_AM */
       0,                      /* SALE_EAT_IN_QY */
       0,                      /* SALE_TAKE_OUT_QY */
       0,                      /* REFUND_EAT_IN_QY */
       0,                      /* REFUND_TAKE_OUT_QY */
       0,                      /* PROMO_EAT_IN_QY */
       0,                      /* PROMO_TAKE_OUT_QY */
       0,                      /* DISCOUNT_EAT_IN_QY */
       0,                      /* DISCOUNT_TAKE_OUT_QY */
       EAT_IN_QY,              /* WASTE_EAT_IN_QY */
       TAKE_OUT_QY,            /* WASTE_TAKE_OUT_QY */
       0,                      /* CREW_EAT_IN_QY */
       0,                      /* CREW_TAKE_OUT_QY */
       0,                      /* MANAGER_EAT_IN_QY */
       0                       /* MANAGER_TAKE_OUT_QY */
  FROM CMU_HOURLY_SALES_PMX
 WHERE OPERATION_TYPE = 'WASTE'
 UNION ALL
/* R?cup?ration des Repas Equipiers */
SELECT PRODUCT_CODE,
       QUARTER_TIME,
       POD_TYPE,
       OPERATION_TYPE,
       0,                      /* SOLD_EAT_IN_QY */
       0,                      /* SOLD_TAKE_OUT_QY */
       EAT_IN_NET_AMOUNT,      /* EAT_IN_NET_AM */
       TAKE_OUT_NET_AMOUNT,    /* TAKE_OUT_NET_AM */
       TAX_AMOUNT,             /* TOTAL_TAX_AM */
       0,                      /* SALE_EAT_IN_QY */
       0,                      /* SALE_TAKE_OUT_QY */
       0,                      /* REFUND_EAT_IN_QY */
       0,                      /* REFUND_TAKE_OUT_QY */
       0,                      /* PROMO_EAT_IN_QY */
       0,                      /* PROMO_TAKE_OUT_QY */
       0,                      /* DISCOUNT_EAT_IN_QY */
       0,                      /* DISCOUNT_TAKE_OUT_QY */
       0,                      /* WASTE_EAT_IN_QY */
       0,                      /* WASTE_TAKE_OUT_QY */
       EAT_IN_QY,              /* CREW_EAT_IN_QY */
       TAKE_OUT_QY,            /* CREW_TAKE_OUT_QY */
       0,                      /* MANAGER_EAT_IN_QY */
       0                       /* MANAGER_TAKE_OUT_QY */
  FROM CMU_HOURLY_SALES_PMX
 WHERE OPERATION_TYPE = 'CREW'
 UNION ALL
/* R?cup?ration des Repas Managers */
SELECT PRODUCT_CODE,
       QUARTER_TIME,
       POD_TYPE,
       OPERATION_TYPE,
       0,                      /* SOLD_EAT_IN_QY */
       0,                      /* SOLD_TAKE_OUT_QY */
       EAT_IN_NET_AMOUNT,      /* EAT_IN_NET_AM */
       TAKE_OUT_NET_AMOUNT,    /* TAKE_OUT_NET_AM */
       TAX_AMOUNT,             /* TOTAL_TAX_AM */
       0,                      /* SALE_EAT_IN_QY */
       0,                      /* SALE_TAKE_OUT_QY */
       0,                      /* REFUND_EAT_IN_QY */
       0,                      /* REFUND_TAKE_OUT_QY */
       0,                      /* PROMO_EAT_IN_QY */
       0,                      /* PROMO_TAKE_OUT_QY */
       0,                      /* DISCOUNT_EAT_IN_QY */
       0,                      /* DISCOUNT_TAKE_OUT_QY */
       0,                      /* WASTE_EAT_IN_QY */
       0,                      /* WASTE_TAKE_OUT_QY */
       0,                      /* CREW_EAT_IN_QY */
       0,                      /* CREW_TAKE_OUT_QY */
       EAT_IN_QY,              /* MANAGER_EAT_IN_QY */
       TAKE_OUT_QY             /* MANAGER_TAKE_OUT_QY */
  FROM CMU_HOURLY_SALES_PMX
 WHERE OPERATION_TYPE = 'MANAGER'
;