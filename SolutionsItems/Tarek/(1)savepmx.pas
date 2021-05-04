procedure TNewPOS6.SavePMX(ABusinessDate: TDateTime;
  HS: IXMLHourlySales6; PMX: IXMLPMix6; SaveDetail: Boolean = False);
var
  FamilyIdx, SalesIdx, ProductIdx, OperationIdx, PODIdx: Integer;
  Product: IXMLPMix6Product;
  StoreTotal: IXMLStoreTotalSales;
  DayPartitioning: IXMLHourlySales6DayPartitioningSegment;
begin
  with TIBStoredProc.Create(nil) do
  try
    Database := IBDatabase;
    if not Transaction.InTransaction then
      Transaction.StartTransaction;
    try
      { Suppression des données dans les tables CMU }
      StoredProcName := 'SPW_CMU_DeleteData';
      ParamByName('DelCash').AsInteger := 0;
      ParamByName('DelHourlySales').AsInteger := 0;
      ParamByName('DelPMX').AsInteger := 1;
      ParamByName('DelSTLD').AsInteger := 0;      
      ExecProc;

      { Récupération des prix des produits }
      StoredProcName := 'SPW_CMU_SavePMixProduct';
      for FamilyIdx := 0 to PMX.FamilyGroup.Count -1 do
      begin
        for ProductIdx := 0 to PMX.FamilyGroup[FamilyIdx].Count -1 do
        begin
          Product := PMX.FamilyGroup[FamilyIdx].Product[ProductIdx];
          ParamByName('DoDelete').AsInteger := Ord((FamilyIdx = 0) and (ProductIdx = 0));
          ParamByName('BusinessDate').AsDate := ABusinessDate;
          ParamByName('ProductId').AsInteger := Product.Id;
          ParamByName('EatInPrice').Value := Product.EatinPrice;
          ParamByName('EatInTax').Value := Product.EatinTax;
          ParamByName('TakeOutPrice').Value := Product.TakeoutPrice;
          ParamByName('TakeOutTax').Value := Product.TakeoutTax;
          Application.ProcessMessages;
          ExecProc;
        end;
      end;

      { Récupération du PMX par 1/4 d'heure et par type d'opération }
      StoredProcName := 'SPW_CMU_SaveHourlySalesPMX';
      for SalesIdx := 0 to HS.StoreTotal.Count -1 do
      begin
        StoreTotal := HS.StoreTotal[SalesIdx];
        DayPartitioning := HS.DayPartitioning.SegmentOfId[StoreTotal.Id];
        for ProductIdx := 0 to StoreTotal.Count -1 do
        begin
          ParamByName('DoDelete').AsInteger := Ord((SalesIdx = 0) and (ProductIdx = 0));
          ParamByName('BusinessDate').AsDate := ABusinessDate;
          ParamByName('PODType').AsString := 'ALL';
          ParamByName('SaleQuarterTime').AsDateTime := GetSaleDateTime(ABusinessDate, HS.OpeningDate, DayPartitioning.BegTime, DayPartitioning.DayOffset);
          ParamByName('ProductCode').AsInteger := StoreTotal.Product[ProductIdx].Id;
          ParamByName('OperationType').AsString := 'ALL';
          ParamByName('EatInQty').Value := 0;
          ParamByName('TakeOutQty').Value := 0;
          ParamByName('EatInNetAmount').Value := 0;
          ParamByName('TakeOutNetAmount').Value := 0;
          ParamByName('NetAmount').Value := 0;
          ParamByName('TaxAmount').Value := 0;
          ExecProc;
          Application.ProcessMessages;

          for OperationIdx := 0 to StoreTotal.Product[ProductIdx].Count -1 do
          begin
            ParamByName('DoDelete').AsInteger := 0;
            ParamByName('BusinessDate').AsDate := ABusinessDate;
            ParamByName('PODType').AsString := 'ALL';
            ParamByName('SaleQuarterTime').AsDateTime := GetSaleDateTime(ABusinessDate, HS.OpeningDate, DayPartitioning.BegTime, DayPartitioning.DayOffset);
            ParamByName('ProductCode').AsInteger := StoreTotal.Product[ProductIdx].Id;
            ParamByName('OperationType').AsString := StoreTotal.Product[ProductIdx].OperationType[OperationIdx].OperationType;
            ParamByName('EatInQty').AsInteger := StoreTotal.Product[ProductIdx].OperationType[OperationIdx].PMix.QtyEatIn;
            ParamByName('TakeOutQty').AsInteger := StoreTotal.Product[ProductIdx].OperationType[OperationIdx].PMix.QtyTakeOut;
            ParamByName('EatInNetAmount').Value := StoreTotal.Product[ProductIdx].OperationType[OperationIdx].PMix.EatInNetAmount;
            ParamByName('TakeOutNetAmount').Value := StoreTotal.Product[ProductIdx].OperationType[OperationIdx].PMix.TakeOutNetAmount;
            ParamByName('NetAmount').Value := StoreTotal.Product[ProductIdx].OperationType[OperationIdx].PMix.NetAmount;
            ParamByName('TaxAmount').Value := StoreTotal.Product[ProductIdx].OperationType[OperationIdx].PMix.TaxAmount;
            ExecProc;
            Application.ProcessMessages;
          end;
        end;
      end;

      { Récupération du PMX par POD type, par 1/4 d'heure et par type d'opération }
      for PODIdx := 0 to HS.POD.Count -1 do
      begin
        for SalesIdx := 0 to HS.POD[PodIdx].StoreTotal.Count -1 do
        begin
          StoreTotal := HS.POD[PodIdx].StoreTotal[SalesIdx];
          DayPartitioning := HS.DayPartitioning.SegmentOfId[StoreTotal.Id];
          for ProductIdx := 0 to StoreTotal.Count -1 do
          begin
            for OperationIdx := 0 to StoreTotal.Product[ProductIdx].Count -1 do
            begin
              ParamByName('DoDelete').AsInteger := 0;
              ParamByName('BusinessDate').AsDate := ABusinessDate;
              ParamByName('PODType').AsString := HS.POD[PODIdx].PodShort;
              ParamByName('SaleQuarterTime').AsDateTime := GetSaleDateTime(ABusinessDate, HS.OpeningDate, DayPartitioning.BegTime, DayPartitioning.DayOffset);
              ParamByName('ProductCode').AsInteger := StoreTotal.Product[ProductIdx].Id;
              ParamByName('OperationType').AsString := StoreTotal.Product[ProductIdx].OperationType[OperationIdx].OperationType;
              ParamByName('EatInQty').AsInteger := StoreTotal.Product[ProductIdx].OperationType[OperationIdx].PMix.QtyEatIn;
              ParamByName('TakeOutQty').AsInteger := StoreTotal.Product[ProductIdx].OperationType[OperationIdx].PMix.QtyTakeOut;
              ParamByName('EatInNetAmount').Value := StoreTotal.Product[ProductIdx].OperationType[OperationIdx].PMix.EatInNetAmount;
              ParamByName('TakeOutNetAmount').Value := StoreTotal.Product[ProductIdx].OperationType[OperationIdx].PMix.TakeOutNetAmount;
              ParamByName('NetAmount').Value := StoreTotal.Product[ProductIdx].OperationType[OperationIdx].PMix.NetAmount;
              ParamByName('TaxAmount').Value := StoreTotal.Product[ProductIdx].OperationType[OperationIdx].PMix.TaxAmount;
              ExecProc;
              Application.ProcessMessages;
            end;
          end;
        end;
      end;

      { Sauvegarde du PMX au 1/4 h et à la journée }
      StoredProcName := 'SPW_Cash_SaveProductMix';
      ParamByName('BusinessDate').AsDateTime := ABusinessDate;
      ExecProc;

      { Sauvegarde du PMX au 1/4 et par point de vente }
      if SaveDetail then
      begin
        StoredProcName := 'SPW_Cash_SaveProductMixLV';
        ParamByName('BusinessDate').AsDateTime := ABusinessDate;
        ExecProc;
      end;

      Transaction.Commit;
    except
      Transaction.Rollback;
      raise;
    end;
  finally
    Free;
  end;
end;