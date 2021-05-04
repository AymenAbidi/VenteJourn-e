create or alter procedure "SPW_CMU_SaveHourlySalesPMX" (
    DODELETE smallint,
    BUSINESSDATE date,
    PODTYPE varchar(5),
    SALEQUARTERTIME timestamp,
    PRODUCTCODE integer,
    OPERATIONTYPE varchar(30),
    EATINQTY integer,
    TAKEOUTQTY integer,
    EATINNETAMOUNT numeric(10,2),
    TAKEOUTNETAMOUNT numeric(10,2),
    NETAMOUNT numeric(10,2),
    TAXAMOUNT numeric(10,2))
as
BEGIN
  IF (DoDelete = 1) THEN
  BEGIN
    DELETE FROM CMU_HOURLY_SALES_PMX;
  END

  INSERT INTO CMU_HOURLY_SALES_PMX(BUSINESS_DATE, PRODUCT_CODE, QUARTER_TIME,     POD_TYPE, OPERATION_TYPE, EAT_IN_QY, TAKE_OUT_QY, EAT_IN_NET_AMOUNT, TAKE_OUT_NET_AMOUNT, NET_AMOUNT, TAX_AMOUNT)
                            VALUES(:BusinessDate, :ProductCode, :SaleQuarterTime, :PODType, :OperationType, :EatInQty, :TakeOutQty, :EatInNetAmount,   :TakeOutNetAmount,   :NetAmount, :TaxAmount);
END