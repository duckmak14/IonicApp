using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using WebMatrix.WebData;

namespace WebModels
{
    [Table("Vehicles")]
    public partial class Vehicle
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int ID { get; set; }
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredCarOwerName")]
        [Display(ResourceType = typeof(WebResources), Name = "CarOwerName")]
        public string CarOwerName { get; set; }
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredNumberPlate")]
        [Display(ResourceType = typeof(WebResources), Name = "NumberPlate")]
        public string NumberPlate { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "CarPartnerName")]
        public int? PartnerID { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Mobile")]
        public string Mobile { get; set; }
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredWeight")]
        [Display(ResourceType = typeof(WebResources), Name = "WeightID")]
        public int? WeightID { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<DateTime> CreatedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<DateTime> ModifiedDate { get; set; }
        public virtual Partner Partner { get; set; }

        [ForeignKey("WeightID")]
        public virtual VehicleWeight VehicleWeight { get; set; }
        [NotMapped]
        public string dataString { get; set; }

    }

    [Table("RepairVehicles")]
    public partial class RepairVehicle
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int ID { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredRepairCategory")]
        [Display(ResourceType = typeof(WebResources), Name = "RepairCategory")]
        public Nullable<int> CategoryID { get; set; }

        //[Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredRepairContent")]
        [Display(ResourceType = typeof(WebResources), Name = "RepairContent")]
        public string Content { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredRepairDate")]
        [Display(ResourceType = typeof(WebResources), Name = "Date")]
        public DateTime RepairDate { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredRepairPrice")]
        [Display(ResourceType = typeof(WebResources), Name = "PriceOfDriver")]
        public Double? Price { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredVehicle")]
        [Display(ResourceType = typeof(WebResources), Name = "VehicleID")]
        public Nullable<int> VehicleID { get; set; }

        //[Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredDriver")]
        [Display(ResourceType = typeof(WebResources), Name = "DriverID")]
        public Nullable<int> DriverID { get; set; }

        public Nullable<int> CreatedBy { get; set; }
        public Nullable<DateTime> CreatedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<DateTime> ModifiedDate { get; set; }
        public Boolean? IsRemove { get; set; }

        [ForeignKey("VehicleID")]
        public virtual Vehicle Vehicle { get; set; }

        [ForeignKey("CategoryID")]
        public virtual RepairCategory RepairCategory { get; set; }

        [ForeignKey("DriverID")]
        public virtual Vehicle Driver { get; set; }

        [NotMapped]
        public string dataString { get; set; }

        [NotMapped]
        public string dataStringDelete { get; set; }

        [NotMapped]
        public string dataStringDate { get; set; }

    }

    [Table("DriverPays")]
    public partial class DriverPay
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int ID { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredPayContent")]
        [Display(ResourceType = typeof(WebResources), Name = "PayContent")]
        public string Content { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredPayDate")]
        [Display(ResourceType = typeof(WebResources), Name = "Date")]
        public DateTime PayDate { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredPayPrice")]
        [Display(ResourceType = typeof(WebResources), Name = "PriceOfDriver")]
        public Double? Price { get; set; }

        //[Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredVehicle")]
        [Display(ResourceType = typeof(WebResources), Name = "VehicleID")]
        public Nullable<int> VehicleID { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredDriver")]
        [Display(ResourceType = typeof(WebResources), Name = "DriverID")]
        public Nullable<int> DriverID { get; set; }
        public Boolean? IsRemove { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<DateTime> CreatedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<DateTime> ModifiedDate { get; set; }

        [ForeignKey("VehicleID")]
        public virtual Vehicle Vehicle { get; set; }

        [ForeignKey("DriverID")]
        public virtual Vehicle Driver { get; set; }

        [NotMapped] 
        public string dataString { get; set; }

        [NotMapped] 
        public string dataStringDelete { get; set; }

        [NotMapped] 
        public string dataStringDate { get; set; }
    }

    [Table("ParkingCosts")]
    public partial class ParkingCost
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int ID { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredDate")]
        [Display(ResourceType = typeof(WebResources), Name = "Date")]
        public DateTime ParkingDate { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredParkingPrice")]
        [Display(ResourceType = typeof(WebResources), Name = "PriceOfParking")]
        public Double? Price { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredVehicle")]
        [Display(ResourceType = typeof(WebResources), Name = "VehicleID")]
        public Nullable<int> VehicleID { get; set; }

        //[Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredDriver")]
        [Display(ResourceType = typeof(WebResources), Name = "DriverID")]
        public Nullable<int> DriverID { get; set; }
        public Boolean? IsRemove { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<DateTime> CreatedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<DateTime> ModifiedDate { get; set; }

        [ForeignKey("VehicleID")]
        public virtual Vehicle Vehicle { get; set; }

        [ForeignKey("DriverID")]
        public virtual Vehicle Driver { get; set; }

        [NotMapped]
        public string dataString { get; set; }

        [NotMapped]
        public string dataStringDelete { get; set; }

        [NotMapped]
        public string dataStringDate { get; set; }
    }

    [Table("ManageOils")]
    public partial class ManageOil
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int ID { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Km")]
        public Double Distance { get; set; }


        [Display(ResourceType = typeof(WebResources), Name = "OilLevel")]
        public Double OilLevel { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "SuppliedFromLevel")]
        public Double SuppliedFromLevel
        {
            get; set;
        }

        [Display(ResourceType = typeof(WebResources), Name = "OtherRun")]
        public Double OtherRun { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "AmountOil")]
        public Double AmountOil
        {
            get; set;
        }

        [Display(ResourceType = typeof(WebResources), Name = "SuppliedOil")]
        public Double SuppliedOil { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "DifferenceOil")]
        public Double DifferenceOil
        {
            get; set;
        }

        [Display(ResourceType = typeof(WebResources), Name = "TotalMoney")]
        public Double Total
        {
            get; set;
        }

        //[Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredVehicle")]
        [Display(ResourceType = typeof(WebResources), Name = "VehicleID")]
        public Nullable<int> VehicleID { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredDriver")]
        [Display(ResourceType = typeof(WebResources), Name = "DriverID")]
        public Nullable<int> DriverID { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Note")]
        public String Note { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<DateTime> CreatedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<DateTime> ModifiedDate { get; set; }
        public Boolean? IsRemove { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredDate")]
        [Display(ResourceType = typeof(WebResources), Name = "Date")]
        public DateTime OilDate { get; set; }

        [ForeignKey("VehicleID")]
        public virtual Vehicle Vehicle { get; set; }

        [ForeignKey("DriverID")]
        public virtual Vehicle Driver { get; set; }

        [NotMapped]
        public string dataString { get; set; }

        [NotMapped]
        public Double OilPrice { get; set; }

        [NotMapped]
        public string dataStringDelete { get; set; }

        [NotMapped]
        public string dataStringDate { get; set; }
    }

    [Table("OtherCosts")]
    public partial class OtherCost
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int ID { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "AdvanceCosts")]
        public Double? AdvanceCosts { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "MortgageCosts")]
        public Double? MortgageCosts { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "OtherCosts")]
        public Double? OtherCosts { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "TotalMoney")]
        public Double Total
        {
            get; set;
        }
        //[Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredVehicle")]
        [Display(ResourceType = typeof(WebResources), Name = "VehicleID")]
        public Nullable<int> VehicleID { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredDriver")]
        [Display(ResourceType = typeof(WebResources), Name = "DriverID")]
        public Nullable<int> DriverID { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Note")]
        public String Note { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<DateTime> CreatedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<DateTime> ModifiedDate { get; set; }
        public Boolean? IsRemove { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredDate")]
        [Display(ResourceType = typeof(WebResources), Name = "Date")]
        public DateTime OtherCostDate { get; set; }

        [ForeignKey("VehicleID")]
        public virtual Vehicle Vehicle { get; set; }

        [ForeignKey("DriverID")]
        public virtual Vehicle Driver { get; set; }

        [NotMapped]
        public string dataString { get; set; }

        [NotMapped]
        public string dataStringDelete { get; set; }

        [NotMapped]
        public string dataStringDate { get; set; }

    }


    [Table("ManageSalarys")]
    public partial class ManageSalary
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int ID { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredDate")]
        [Display(ResourceType = typeof(WebResources), Name = "Date")]
        public DateTime SalaryDate { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Km")]
        public Double Distance { get; set; }


        [Display(ResourceType = typeof(WebResources), Name = "WorkDay")]
        public Double WorkDay { get; set; }


        [Display(ResourceType = typeof(WebResources), Name = "WorkDayPrice")]
        public Double WorkDayPrice { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "DistancePrice")]
        public Double DistancePrice { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "WorkDayTotal")]
        public Double WorkDayTotal
        {
            get; set;
        }

        [Display(ResourceType = typeof(WebResources), Name = "DistanceTotal")]
        public Double DistanceTotal
        {
            get; set;
        }

        [Display(ResourceType = typeof(WebResources), Name = "PhoneCosts")]
        public Double? PhoneCosts { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "SupportCosts")]
        public Double? SupportCosts { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "BonusCosts")]
        public Double? BonusCosts { get; set; }

        //Salary Total is total excluding InsuranceCosts
        [Display(ResourceType = typeof(WebResources), Name = "SalaryTotal")]
        public Double SalaryTotal {
            get; set;
        }

        [Display(ResourceType = typeof(WebResources), Name = "InsuranceCosts")]
        public Double? InsuranceCosts { get; set; }

        // After included InsuranceCosts
        [Display(ResourceType = typeof(WebResources), Name = "TotalMoney")]
        public Double Total
        {
            get; set;
        }


        //[Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredVehicle")]
        [Display(ResourceType = typeof(WebResources), Name = "VehicleID")]
        public Nullable<int> VehicleID { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredDriver")]
        [Display(ResourceType = typeof(WebResources), Name = "DriverID")]
        public Nullable<int> DriverID { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Note")]
        public String Note { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<DateTime> CreatedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<DateTime> ModifiedDate { get; set; }
        public Boolean? IsRemove { get; set; }

       

        [ForeignKey("VehicleID")]
        public virtual Vehicle Vehicle { get; set; }

        [ForeignKey("DriverID")]
        public virtual Vehicle Driver { get; set; }

        [NotMapped]
        public string dataString { get; set; }

        [NotMapped]
        public string dataStringDelete { get; set; }

        [NotMapped]
        public string dataStringDate { get; set; }


    }



    [Table("ManageTickets")]
    public partial class ManageTicket
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int ID { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredCategory")]
        [Display(ResourceType = typeof(WebResources), Name = "Category")]
        public String Category { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredDate")]
        [Display(ResourceType = typeof(WebResources), Name = "Date")]
        public DateTime TicketDate { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredPrice")]
        [Display(ResourceType = typeof(WebResources), Name = "PriceOfDriver")]
        public Double Price { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredVehicle")]
        [Display(ResourceType = typeof(WebResources), Name = "VehicleID")]
        public Nullable<int> VehicleID { get; set; }

        //[Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredDriver")]
        [Display(ResourceType = typeof(WebResources), Name = "DriverID")]
        public Nullable<int> DriverID { get; set; }
        public Nullable<int> TicketType { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Note")]
        public String Note { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<DateTime> CreatedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<DateTime> ModifiedDate { get; set; }
        public Boolean? IsRemove { get; set; }

        [ForeignKey("VehicleID")]
        public virtual Vehicle Vehicle { get; set; }

        [ForeignKey("DriverID")]
        public virtual Vehicle Driver { get; set; }

        [NotMapped]
        public string dataString { get; set; }

        [NotMapped]
        public string dataStringDelete { get; set; }

        [NotMapped]
        public string dataStringDate { get; set; }
    }


    [Table("OilPrices")]
    public partial class OilPrice
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int ID { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredOilPrice")]
        [Display(ResourceType = typeof(WebResources), Name = "OilPrice")]
        public Double Price { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredStartDate")]
        [Display(ResourceType = typeof(WebResources), Name = "StartDate")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredEndDate")]
        [Display(ResourceType = typeof(WebResources), Name = "EndDate")]
        public DateTime EndDate { get; set; }

        public Nullable<int> CreatedBy { get; set; }
        public Nullable<DateTime> CreatedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<DateTime> ModifiedDate { get; set; }

        [NotMapped]
        public string dataString { get; set; }
        [NotMapped]
        public string dataStringEnd { get; set; }
    }


    [Table("RepairCategorys")]
    public partial class RepairCategory
    {
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredRepairCategory")]
        [Display(ResourceType = typeof(WebResources), Name = "RepairCategory")]
        public string Category { get; set; }

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int ID { get; set; }

        public Nullable<int> CreatedBy { get; set; }
        public Nullable<DateTime> CreatedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<DateTime> ModifiedDate { get; set; }
        public Boolean IsRemove { get; set; }

        [NotMapped]
        public string dataString { get; set; }

    }

    [Table("VehicleWeights")]
    public partial class VehicleWeight
    {
        [Display(ResourceType = typeof(WebResources), Name = "WeightName")]
        public string WeightName { get; set; }

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int ID { get; set; }

    }

    [Table("PricingTables")]
    public partial class PricingTable
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int ID { get; set; }
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredRouteCode")]
        [Display(ResourceType = typeof(WebResources), Name = "RouteCode")]
        public int? RouteID { get; set; }
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredPriceCode")]
        [Display(ResourceType = typeof(WebResources), Name = "Price")]
        [DisplayFormat(DataFormatString = "{0:n2}", ApplyFormatInEditMode = true)]
        public Nullable<double> Price { get; set; }
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredSourcePartner")]
        [Display(ResourceType = typeof(WebResources), Name = "SourcePartnerID")]
        public Nullable<int> SourcePartnerID { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "DestinationPartnerID")]
        public Nullable<int> DestinationPartnerID { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredWeight")]
        [Display(ResourceType = typeof(WebResources), Name = "WeightID")]
        public Nullable<int> WeightID { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Note")]
        public String Note { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<DateTime> CreatedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<DateTime> ModifiedDate { get; set; }
        public virtual Partner SourcePartner { get; set; }
        public virtual Partner DestinationPartner { get; set; }
        public virtual VehicleWeight Weight { get; set; }
        public virtual Route Route { get; set; }
        [NotMapped]
        public string dataString { get; set; }

    }

    [Table("Partners")]
    public partial class Partner
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int ID { get; set; }
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredPartnerName")]
        [Display(ResourceType = typeof(WebResources), Name = "PartnerName")]
        public String PartnerName { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Address")]
        public String Address { get; set; }

        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,4}", ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "EmailNotValid")]
        [Display(ResourceType = typeof(WebResources), Name = "Email")]
        public String Email { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "PartnerCode")]
        public String PartnerCode { get; set; }

        //[Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredMobile")]
        [Display(ResourceType = typeof(WebResources), Name = "Mobile")]
        public String Mobile { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<DateTime> CreatedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<DateTime> ModifiedDate { get; set; }
        [NotMapped]
        public string dataString { get; set; }
    }
    [Table("PriceChangeLogs")]
    public partial class PriceChangeLog
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int ID { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Price")]

        public Nullable<Double> Price { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "ChangePrice")]
        public Nullable<Double> ChangePrice { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "RouteID")]
        public Nullable<int> RouteID { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "ChangeRouteID")]
        public Nullable<int> ChangeRouteID { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "SourcePartnerID")]
        public Nullable<int> SourcePartnerID { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "ChangeSourceID")]
        public Nullable<int> ChangeSourceID { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "DestinationPartnerID")]
        public Nullable<int> DestinationPartnerID { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "ChangeDestinationID")]
        public Nullable<int> ChangeDestinationID { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "WeightID")]
        public Nullable<int> WeightID { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "ChangeWeightID")]
        public Nullable<int> ChangeWeightID { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<DateTime> CreatedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<DateTime> ModifiedDate { get; set; }
        public int PricingTableID { get; set; }
    }
    [Table("PriceChangePeriod")]
    public partial class PriceChangePeriod
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int ID { get; set; }
        public Double Percent { get; set; }
        public int CreatedBy { get; set; }
        public Nullable<DateTime> CreatedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<DateTime> ModifiedDate { get; set; }
    }
    [Table("Locations")]
    public partial class Location
    {

        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int ID { get; set; }
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredLocationName")]
        [Display(ResourceType = typeof(WebResources), Name = "LocationName")]
        public String LocationName { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "AddressName")]
        public String AddressName { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "LocationParentID")]
        public int? ParentID { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "DistrictID")]
        public string DistrictID { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "ProvinceID")]
        public string ProvinceID { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<DateTime> CreatedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<DateTime> ModifiedDate { get; set; }
        public virtual District District { get; set; }
        public virtual Province Province { get; set; }

        [NotMapped]
        public string ProvinceName { get; set; }

        [NotMapped]
        public string dataString { get; set; }
    }


    [Table("Routes")]
    public partial class Route
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int ID { get; set; }
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredEndLocation")]
        [Display(ResourceType = typeof(WebResources), Name = "EndLocationID")]
        public int? EndLocationID { get; set; }
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredStartLocation")]
        [Display(ResourceType = typeof(WebResources), Name = "StartLocationID")]
        public int? StartLocationID { get; set; }
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredRouteCode")]
        [Display(ResourceType = typeof(WebResources), Name = "RouteCode")]
        public string RouteCode { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<DateTime> CreatedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<DateTime> ModifiedDate { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "StartLocation")]
        public virtual Location StartLocation { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Distance")]
        public string Distance { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "EndLocation")]
        public virtual Location EndLocation { get; set; }
        [NotMapped]
        public string dataString { get; set; }
    }

    [Table("Provinces")]
    public partial class Province
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public string ID { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "ProvinceName")]
        public string ProvinceName { get; set; }

        public int CountryId { get; set; }
    }


    [Table("Districts")]
    public partial class District
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public string ID { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "DistrictName")]
        public string DistrictName { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "ProvinceID")]
        public string ProvinceID { get; set; }
    }


    [Table("TransportActuals")]
    public partial class TransportActual
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int ID { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Note")]
        public string Note { get; set; }
        //[Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredUnitPrice")]

        [Display(ResourceType = typeof(WebResources), Name = "UnitPrice")]
        public Nullable<double> UnitPrice { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "TotalMoney")]
        public Nullable<double> TotalMoney
        {
            get
            {
                if (!UnitPrice.HasValue || !TripCount.HasValue)
                {
                    return null;
                }

                var total = UnitPrice * TripCount;
                total = Math.Round((double)(total) / 1000) * 1000;

                return total;
            }
        }

        [Display(ResourceType = typeof(WebResources), Name = "TotalMoney")]
        public Nullable<double> TotalMoneyAT
        {
            get
            {
                if (!UnitPriceAT.HasValue || !TripCount.HasValue)
                {
                    return null;
                }

                var total = UnitPriceAT * TripCount;
                total = Math.Round((double)(total) / 1000) * 1000;

                return total;
            }
        }

        [Display(ResourceType = typeof(WebResources), Name = "TotalMoney")]
        public Nullable<double> TotalMoneyHPC
        {
            get
            {
                if (!UnitPriceHPC.HasValue || !TripCount.HasValue)
                {
                    return null;
                }

                var total = UnitPriceHPC * TripCount;
                total = Math.Round((double)(total) / 1000) * 1000;

                return total;
            }
        }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredPlanDate")]
        [Display(ResourceType = typeof(WebResources), Name = "PlanDate")]
        public DateTime ActualDate { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "TrackingCode")]
        public string TrackingCode { get; set; }
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredVehicle")]
        [Display(ResourceType = typeof(WebResources), Name = "VehicleID")]
        public Nullable<int> VehicleID { get; set; }
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredRouteCode")]
        [Display(ResourceType = typeof(WebResources), Name = "RouteCode")]
        public Nullable<int> RouteID { get; set; }
        //[Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredTripCount")]
        [Display(ResourceType = typeof(WebResources), Name = "TripCount")]
        public Nullable<Double> TripCount { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "ActualWeight")]
        public Nullable<int> ActualWeightID { get; set; }
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredSourcePartner")]
        [Display(ResourceType = typeof(WebResources), Name = "SourcePartnerID")]
        public Nullable<int> SourcePartnerID { get; set; }
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredDestinationPartner")]
        [Display(ResourceType = typeof(WebResources), Name = "DestinationPartnerID")]
        public Nullable<int> DestinationPartnerID { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Status")]
        public Boolean Status { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "StartTime")]
        public long? StartTime { get; set; }
        //[Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredEndTime")]
        [Display(ResourceType = typeof(WebResources), Name = "EndTime")]
        public long? EndTime { get; set; }
        public Nullable<double> UnitPriceAT { get; set; }
        public Nullable<double> UnitPriceHPC { get; set; }
        public Boolean? IsRemove { get; set; }

        public Nullable<int> CreatedBy { get; set; }
        public Nullable<DateTime> CreatedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<DateTime> ModifiedDate { get; set; }
        public virtual Route Route { get; set; }
        public virtual Vehicle Vehicle { get; set; }
        public virtual VehicleWeight ActualWeight { get; set; }
        public virtual Partner SourcePartner { get; set; }
        public virtual Partner DestinationPartner { get; set; }
        [NotMapped]
        public string dataString { get; set; }
        [NotMapped]
        [Display(ResourceType = typeof(WebResources), Name = "TripCount")]
        [RegularExpression(@"^([0-9]+([.][0-9]*)?|[.][0-9]+)$", ErrorMessage = "Yêu cầu nhập đúng định dạng")]
        public string TripCountViewModel { get; set; }
    }


    [Table("TransportPlans")]
    public partial class TransportPlan
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int ID { get; set; }
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredPlanDate")]
        [Display(ResourceType = typeof(WebResources), Name = "PlanDate")]
        public DateTime PlanDate { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "TrackingCode")]
        public string TrackingCode { get; set; }
        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredVehicle")]
        [Display(ResourceType = typeof(WebResources), Name = "VehicleID")]
        public Nullable<int> VehicleID { get; set; }
        //[Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredRouteCode")]
        [Display(ResourceType = typeof(WebResources), Name = "RouteCode")]
        public Nullable<int> RouteID { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "DetailCode")]
        public string DetailCode { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Amount")]
        public string Amount { get; set; }
        //[Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredEndLocation")]
        [Display(ResourceType = typeof(WebResources), Name = "EndLocation")]
        public int? EndLocationID { get; set; }
        //[Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredStartLocation")]
        [Display(ResourceType = typeof(WebResources), Name = "StartLocation")]
        public int? StartLocationID { get; set; }
        //[Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredTripCount")]
        //[Display(ResourceType = typeof(WebResources), Name = "TripCount")]
        //public Nullable<int> TripCount { get; set; }
        //[Display(ResourceType = typeof(WebResources), Name = "ActualWeight")]
        //public Nullable<int> ActualWeightID { get; set; }
        //[Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredStartTime")]
        [Display(ResourceType = typeof(WebResources), Name = "StartTime")]
        public long? StartTime { get; set; }
        //[Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredEndTime")]
        [Display(ResourceType = typeof(WebResources), Name = "EndTime")]
        public long? EndTime { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Note")]
        public String Note { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "TripBack")]
        public String TripBack { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "Status")]
        public Boolean? Status { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredSourcePartner")]
        [Display(ResourceType = typeof(WebResources), Name = "SourcePartnerID")]
        public Nullable<int> SourcePartnerID { get; set; }

        [Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredDestinationPartner")]
        [Display(ResourceType = typeof(WebResources), Name = "DestinationPartnerID")]
        public Nullable<int> DestinationPartnerID { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "ActualWeight")]
        public Nullable<int> ActualWeightID { get; set; }
        public Boolean? IsRemove { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<DateTime> CreatedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<DateTime> ModifiedDate { get; set; }
        public virtual Route Route { get; set; }
        public virtual Vehicle Vehicle { get; set; }
        public virtual Partner SourcePartner { get; set; }
        public virtual Partner DestinationPartner { get; set; }
        public virtual VehicleWeight ActualWeight { get; set; }
        public virtual Location StartLocation { get; set; }
        public virtual Location EndLocation { get; set; }

        [NotMapped]
        public string dataString { get; set; }

        [NotMapped]
        public string dataStringDelete { get; set; }

        [NotMapped]
        public string dataStringDate { get; set; }


    }

    [Table("EnumerationChangeLogs")]
    public partial class EnumerationChangeLog
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        [Display(ResourceType = typeof(WebResources), Name = "ID")]
        public int ID { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "Time")]
        public Nullable<DateTime> Time { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "ChangeTime")]
        public Nullable<DateTime> ChangeTime { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "StartTime")]
        public long? StartTime { get; set; }
        public long? ChangeStartTime { get; set; }

        //[Required(ErrorMessageResourceType = typeof(AccountResources), ErrorMessageResourceName = "RequiredEndTime")]
        [Display(ResourceType = typeof(WebResources), Name = "EndTime")]
        public long? EndTime { get; set; }
        public long? ChangeEndTime { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "RouteID")]
        public Nullable<int> RouteID { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "ChangeRouteID")]
        public Nullable<int> ChangeRouteID { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "SourcePartner")]
        public Nullable<int> SourcePartner { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "ChangeSourcePartner")]
        public Nullable<int> ChangeSourcePartner { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "DestinationPartner")]
        public Nullable<int> DestinationPartner { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "ChangeDestinationPartner")]
        public Nullable<int> ChangeDestinationPartner { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "ActualWeightID")]
        public Nullable<int> ActualWeightID { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "ChangeActualWeight")]
        public Nullable<int> ChangeActualWeight { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "TripCount")]
        public Nullable<Double> TripCount { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "ChangeTripCount")]
        public Nullable<Double> ChangeTripCount { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "UnitPrice")]
        public Nullable<Double> UnitPrice { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "ChangeUnitPrice")]
        public Nullable<Double> ChangeUnitPrice { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "IsRemove")]
        public bool IsRemove { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "VehicleID")]
        public Nullable<int> VehicleID { get; set; }
        [Display(ResourceType = typeof(WebResources), Name = "ChangeVehicleID")]
        public Nullable<int> ChangeVehicleID { get; set; }
        public Nullable<double> UnitPriceAT { get; set; }
        public Nullable<double> UnitPriceHPC { get; set; }
        public Nullable<double> ChangeUnitPriceAT { get; set; }
        public Nullable<double> ChangeUnitPriceHPC { get; set; }
        public Nullable<int> CreatedBy { get; set; }
        public Nullable<DateTime> CreatedDate { get; set; }
        public Nullable<int> ModifiedBy { get; set; }
        public Nullable<DateTime> ModifiedDate { get; set; }
    }
}
