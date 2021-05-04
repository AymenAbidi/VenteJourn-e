using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Collections;

namespace Mcd.App.GetXmlRpc
{ 
    [XmlRoot(ElementName = "ProductInfo")]
    public class ProductInfo
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "name")]
        public string Name { get; set; }
        [XmlAttribute(AttributeName = "familyGroup")]
        public string FamilyGroup { get; set; }
        [XmlAttribute(AttributeName = "class")]
        public string Class { get; set; }
        [XmlAttribute(AttributeName = "department")]
        public string Department { get; set; }
        [XmlAttribute(AttributeName = "classDepartment")]
        public string ClassDepartment { get; set; }
        [XmlAttribute(AttributeName = "subClassDepartment")]
        public string SubClassDepartment { get; set; }
    }

    [XmlRoot(ElementName = "ProductTable")]
    public class ProductTable
    {
        [XmlElement(ElementName = "ProductInfo")]
        public List<ProductInfo> ProductInfo { get; set; }
    }

    [XmlRoot(ElementName = "Segment")]
    public class Segment
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "begTime")]
        public string BegTime { get; set; }
        [XmlAttribute(AttributeName = "endTime")]
        public string EndTime { get; set; }
        [XmlAttribute(AttributeName = "dayOffset")]
        public string DayOffset { get; set; }
    }

    [XmlRoot(ElementName = "DayPartitioning")]
    public class DayPartitioning
    {
        [XmlElement(ElementName = "Segment")]
        public List<Segment> Segment { get; set; }
    }

    [XmlRoot(ElementName = "Tab")]
    public class Tab
    {
        [XmlAttribute(AttributeName = "qtyEatIn")]
        public string QtyEatIn { get; set; }
        [XmlAttribute(AttributeName = "qtyTakeOut")]
        public string QtyTakeOut { get; set; }
        [XmlAttribute(AttributeName = "netAmount")]
        public string NetAmount { get; set; }
        [XmlAttribute(AttributeName = "taxAmount")]
        public string TaxAmount { get; set; }
    }

    [XmlRoot(ElementName = "PMix")]
    public class PMix
    {
        [XmlElement(ElementName = "Tab")]
        public Tab Tab { get; set; }

        [XmlAttribute(AttributeName = "qtyEatIn")]
        public string QtyEatIn { get; set; }

        [XmlAttribute(AttributeName = "qtyTakeOut")]
        public string QtyTakeOut { get; set; }

        [XmlAttribute(AttributeName = "eatInNetAmount")]
        public string EatInNetAmount { get; set; }

        [XmlAttribute(AttributeName = "takeOutNetAmount")]
        public string TakeOutNetAmount { get; set; }

        [XmlAttribute(AttributeName = "netAmount")]
        public string NetAmount { get; set; }

        [XmlAttribute(AttributeName = "taxAmount")]
        public string TaxAmount { get; set; }

        [XmlAttribute(AttributeName = "netBeforeDiscount")]
        public string NetBeforeDiscount { get; set; }

        [XmlAttribute(AttributeName = "taxBeforeDiscount")]
        public string TaxBeforeDiscount { get; set; }
    }

    [XmlRoot(ElementName = "WavePMix")]
    public class WavePMix
    {
        [XmlAttribute(AttributeName = "qtyEatIn")]
        public string QtyEatIn { get; set; }

        [XmlAttribute(AttributeName = "qtyTakeOut")]
        public string QtyTakeOut { get; set; }

        [XmlAttribute(AttributeName = "wavesAmount")]
        public string WavesAmount { get; set; }

        [XmlAttribute(AttributeName = "receivedQtyEatIn")]
        public string ReceivedQtyEatIn { get; set; }

        [XmlAttribute(AttributeName = "receivedQtyTakeOut")]
        public string ReceivedQtyTakeOut { get; set; }

        [XmlAttribute(AttributeName = "receivedAmount")]
        public string ReceivedAmount { get; set; }
    }

    [XmlRoot(ElementName = "OperationType")]
    public class OperationType
    {
        [XmlElement(ElementName = "PMix")]
        public PMix PMix { get; set; }

        [XmlElement(ElementName = "WavePMix")]
        public WavePMix WavePMix { get; set; }

        [XmlAttribute(AttributeName = "operationType")]
        public string operationType { get; set; }
    }

    [XmlRoot(ElementName = "Product")]
    public class Product
    {
        [XmlElement(ElementName = "OperationType")]
        public List<OperationType> OperationType { get; set; }

        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }

        [XmlAttribute(AttributeName = "tc")]
        public string Tc { get; set; }

        [XmlAttribute(AttributeName = "tabTC")]
        public string TabTC { get; set; }

        [XmlAttribute(AttributeName = "tabsCount")]
        public string TabsCount { get; set; }

        [XmlAttribute(AttributeName = "wavesCount")]
        public string WavesCount { get; set; }

        [XmlAttribute(AttributeName = "wavesReceivedCount")]
        public string WavesReceivedCount { get; set; }
    }

    [XmlRoot(ElementName = "Sales")]
    public class Sales
    {
        [XmlElement(ElementName = "Product")]
        public List<Product> Product { get; set; }

        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }

        [XmlAttribute(AttributeName = "tc")]
        public string Tc { get; set; }

        [XmlAttribute(AttributeName = "prodTC")]
        public string ProdTC { get; set; }

        [XmlAttribute(AttributeName = "nonProdTC")]
        public string NonProdTC { get; set; }

        [XmlAttribute(AttributeName = "extTC")]
        public string ExtTC { get; set; }

        [XmlAttribute(AttributeName = "tabTC")]
        public string TabTC { get; set; }

        [XmlAttribute(AttributeName = "tabsCount")]
        public string TabsCount { get; set; }

        [XmlAttribute(AttributeName = "wavesCount")]
        public string WavesCount { get; set; }

        [XmlAttribute(AttributeName = "wavesReceivedCount")]
        public string WavesReceivedCount { get; set; }

        [XmlAttribute(AttributeName = "eatInTC")]
        public string EatInTC { get; set; }

        [XmlAttribute(AttributeName = "takeOutTC")]
        public string TakeOutTC { get; set; }

        [XmlAttribute(AttributeName = "eatInNetAmount")]
        public string EatInNetAmount { get; set; }

        [XmlAttribute(AttributeName = "takeOutNetAmount")]
        public string TakeOutNetAmount { get; set; }

        [XmlAttribute(AttributeName = "netAmount")]
        public string NetAmount { get; set; }

        [XmlAttribute(AttributeName = "taxAmount")]
        public string TaxAmount { get; set; }

        [XmlAttribute(AttributeName = "productNetAmount")]
        public string ProductNetAmount { get; set; }

        [XmlAttribute(AttributeName = "productTaxAmount")]
        public string ProductTaxAmount { get; set; }

        [XmlAttribute(AttributeName = "tabNetAmount")]
        public string TabNetAmount { get; set; }

        [XmlAttribute(AttributeName = "tabTaxAmount")]
        public string TabTaxAmount { get; set; }

        [XmlAttribute(AttributeName = "wavesAmount")]
        public string WavesAmount { get; set; }

        [XmlAttribute(AttributeName = "wavesReceivedAmount")]
        public string WavesReceivedAmount { get; set; }
    }

    [XmlRoot(ElementName = "StoreTotal")]
    public class StoreTotal
    {
        [XmlElement(ElementName = "Sales")]
        public List<Sales> Sales { get; set; }
    }

    [XmlRoot(ElementName = "POD")]
    public class POD
    {
        [XmlElement(ElementName = "StoreTotal")]
        public StoreTotal StoreTotal { get; set; }

        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }

        [XmlAttribute(AttributeName = "podShort")]
        public string PodShort { get; set; }
    }

    [XmlRoot(ElementName = "POS")]
    public class POS
    {
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }

        [XmlAttribute(AttributeName = "serviceType")]
        public string ServiceType { get; set; }

        [XmlAttribute(AttributeName = "podShort")]
        public string PodShort { get; set; }

        [XmlAttribute(AttributeName = "businessDay")]
        public string BusinessDay { get; set; }

        [XmlAttribute(AttributeName = "openedDate")]
        public string OpenedDate { get; set; }

        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; }

        [XmlElement(ElementName = "StoreTotal")]
        public StoreTotal StoreTotal { get; set; }
    }

    [XmlRoot(ElementName = "OrderTakers")]
    public class OrderTakers
    {
        [XmlElement(ElementName = "POS")]
        public List<POS> POS { get; set; }
    }

    [XmlRoot(ElementName = "Response")]
    public class HourlySales
    {
        [XmlElement(ElementName = "ProductTable")]
        public ProductTable ProductTable { get; set; }

        [XmlElement(ElementName = "DayPartitioning")]
        public DayPartitioning DayPartitioning { get; set; }

        [XmlElement(ElementName = "StoreTotal")]
        public StoreTotal StoreTotal { get; set; }

        [XmlElement(ElementName = "POD")]
        public List<POD> POD { get; set; }

        [XmlElement(ElementName = "POS")]
        public POS POS { get; set; }

        [XmlElement(ElementName = "OrderTakers")]
        public OrderTakers OrderTakers { get; set; }

        [XmlAttribute(AttributeName = "npVersion")]
        public string NpVersion { get; set; }

        [XmlAttribute(AttributeName = "creationDate")]
        public string CreationDate { get; set; }

        [XmlAttribute(AttributeName = "storeId")]
        public string StoreId { get; set; }

        [XmlAttribute(AttributeName = "storeName")]
        public string StoreName { get; set; }

        [XmlAttribute(AttributeName = "requestDataType")]
        public string RequestDataType { get; set; }

        [XmlAttribute(AttributeName = "requestReport")]
        public string RequestReport { get; set; }

        [XmlAttribute(AttributeName = "requestPOD")]
        public string RequestPOD { get; set; }

        [XmlAttribute(AttributeName = "requestPOSList")]
        public string RequestPOSList { get; set; }

        [XmlAttribute(AttributeName = "requestDate")]
        public string RequestDate { get; set; }

        [XmlAttribute(AttributeName = "requestTime")]
        public string RequestTime { get; set; }

        [XmlAttribute(AttributeName = "openingDate")]
        public string OpeningDate { get; set; }

        [XmlAttribute(AttributeName = "openingTime")]
        public string OpeningTime { get; set; }

        [XmlAttribute(AttributeName = "closingDate")]
        public string ClosingDate { get; set; }

        [XmlAttribute(AttributeName = "closingTime")]
        public string ClosingTime { get; set; }

        [XmlAttribute(AttributeName = "UTCOffset")]
        public string UTCOffset { get; set; }

    }

}
