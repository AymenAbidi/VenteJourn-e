using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Collections;

namespace Mcd.App.GetXmlRpc.PMX
{
	[XmlRoot(ElementName = "ProductInfo")]
	public class ProductInfo
	{

		[XmlAttribute(AttributeName = "id")]
		public int Id { get; set; }

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

	[XmlRoot(ElementName = "PMix")]
	public class PMix
	{

		[XmlAttribute(AttributeName = "qtyTakeOut")]
		public int QtyTakeOut { get; set; }

		[XmlAttribute(AttributeName = "netAmtTakeOut")]
		public double NetAmtTakeOut { get; set; }

		[XmlAttribute(AttributeName = "taxTakeOut")]
		public double TaxTakeOut { get; set; }

		[XmlAttribute(AttributeName = "netBeforeDiscountTakeOut")]
		public double NetBeforeDiscountTakeOut { get; set; }

		[XmlAttribute(AttributeName = "taxBeforeDiscountTakeOut")]
		public double TaxBeforeDiscountTakeOut { get; set; }

		[XmlAttribute(AttributeName = "qtyEatIn")]
		public int QtyEatIn { get; set; }

		[XmlAttribute(AttributeName = "netBeforeDiscountEatIn")]
		public double NetBeforeDiscountEatIn { get; set; }

		[XmlAttribute(AttributeName = "taxBeforeDiscountEatIn")]
		public double TaxBeforeDiscountEatIn { get; set; }

		[XmlAttribute(AttributeName = "netAmtEatIn")]
		public double NetAmtEatIn { get; set; }

		[XmlAttribute(AttributeName = "taxEatIn")]
		public double TaxEatIn { get; set; }
	}

	[XmlRoot(ElementName = "Price")]
	public class Price
	{

		[XmlElement(ElementName = "PMix")]
		public PMix PMix { get; set; }

		[XmlAttribute(AttributeName = "weekDay")]
		public string WeekDay { get; set; }

		[XmlAttribute(AttributeName = "saleTime")]
		public int SaleTime { get; set; }

		[XmlAttribute(AttributeName = "dayPart")]
		public string DayPart { get; set; }

		[XmlAttribute(AttributeName = "salePrice")]
		public double SalePrice { get; set; }

		[XmlAttribute(AttributeName = "saleTax")]
		public double SaleTax { get; set; }
	}

	[XmlRoot(ElementName = "OperationType")]
	public class OperationType
	{

		[XmlElement(ElementName = "PMix")]
		public PMix PMix { get; set; }

		[XmlElement(ElementName = "Price")]
		public List<Price> Price { get; set; }

		[XmlAttribute(AttributeName = "operationType")]
		public string operationType { get; set; }
	}

	[XmlRoot(ElementName = "Product")]
	public class Product
	{

		[XmlElement(ElementName = "OperationType")]
		public List<OperationType> OperationType { get; set; }

		[XmlAttribute(AttributeName = "id")]
		public int Id { get; set; }

		[XmlAttribute(AttributeName = "priceType")]
		public string PriceType { get; set; }

		[XmlAttribute(AttributeName = "eatinPrice")]
		public double EatinPrice { get; set; }

		[XmlAttribute(AttributeName = "eatinTax")]
		public double EatinTax { get; set; }

		[XmlAttribute(AttributeName = "takeoutPrice")]
		public double TakeoutPrice { get; set; }

		[XmlAttribute(AttributeName = "takeoutTax")]
		public double TakeoutTax { get; set; }
	}

	[XmlRoot(ElementName = "FamilyGroup")]
	public class FamilyGroup
	{

		[XmlElement(ElementName = "Product")]
		public List<Product> Product { get; set; }

		[XmlAttribute(AttributeName = "groupCode")]
		public int GroupCode { get; set; }

		[XmlAttribute(AttributeName = "groupName")]
		public string GroupName { get; set; }
	}

	[XmlRoot(ElementName = "OperatorSession")]
	public class OperatorSession
	{

		[XmlAttribute(AttributeName = "id")]
		public int Id { get; set; }

		[XmlAttribute(AttributeName = "name")]
		public string Name { get; set; }

		[XmlAttribute(AttributeName = "login")]
		public string Login { get; set; }

		[XmlAttribute(AttributeName = "logout")]
		public string Logout { get; set; }

		[XmlAttribute(AttributeName = "initialGT")]
		public double InitialGT { get; set; }

		[XmlAttribute(AttributeName = "finalGT")]
		public double FinalGT { get; set; }

		[XmlAttribute(AttributeName = "initialCoupon")]
		public int InitialCoupon { get; set; }

		[XmlAttribute(AttributeName = "finalCoupon")]
		public int FinalCoupon { get; set; }

		[XmlElement(ElementName = "Product")]
		public Product Product { get; set; }
	}

	[XmlRoot(ElementName = "POS")]
	public class POS
	{

		[XmlElement(ElementName = "OperatorSession")]
		public List<OperatorSession> OperatorSession { get; set; }

		[XmlElement(ElementName = "FamilyGroup")]
		public List<FamilyGroup> FamilyGroup { get; set; }

		[XmlAttribute(AttributeName = "id")]
		public int Id { get; set; }

		[XmlAttribute(AttributeName = "podShort")]
		public string PodShort { get; set; }

		[XmlAttribute(AttributeName = "businessDay")]
		public string BusinessDay { get; set; }

		[XmlAttribute(AttributeName = "openedDate")]
		public string OpenedDate { get; set; }

		[XmlAttribute(AttributeName = "status")]
		public string Status { get; set; }
	}

	[XmlRoot(ElementName = "Response")]
	public class HourlyPMX
	{

		[XmlElement(ElementName = "ProductTable")]
		public ProductTable ProductTable { get; set; }

		[XmlElement(ElementName = "FamilyGroup")]
		public List<FamilyGroup> FamilyGroup { get; set; }

		[XmlElement(ElementName = "POS")]
		public List<POS> POS { get; set; }

		[XmlAttribute(AttributeName = "npVersion")]
		public string NpVersion { get; set; }

		[XmlAttribute(AttributeName = "creationDate")]
		public string CreationDate { get; set; }

		[XmlAttribute(AttributeName = "storeId")]
		public int StoreId { get; set; }

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
		public int RequestDate { get; set; }

		[XmlAttribute(AttributeName = "requestTime")]
		public string RequestTime { get; set; }
	}
}
