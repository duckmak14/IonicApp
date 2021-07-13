using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.AccessControl;
using System.Web;
using WebModels;

namespace WEB.Models
{
    public class ManageSalaryViewModel: ManageSalary
    {
        [Display(ResourceType = typeof(WebResources), Name = "CarOwerName")]
        public string CarOwerName { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "NumberPlate")]
        public string NumberPlate { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số ngày công")]
        [Display(ResourceType = typeof(WebResources), Name = "WorkDay")]
        [RegularExpression(@"^[+]?([0-9]+([.][0-9]*)?|[.][0-9]+)$", ErrorMessage = "Yêu cầu nhập đúng định dạng")]
        public string WorkDayString { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập đơn giá ngày công")]
        [Display(ResourceType = typeof(WebResources), Name = "WorkDayPrice")]
        [RegularExpression(@"^[+]?([0-9]+([.][0-9]*)?|[.][0-9]+)$", ErrorMessage = "Yêu cầu nhập đúng định dạng")]
        public string WorkDayPriceString { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập đơn giá Km")]
        [Display(ResourceType = typeof(WebResources), Name = "DistancePrice")]
        [RegularExpression(@"^[+]?([0-9]+([.][0-9]*)?|[.][0-9]+)$", ErrorMessage = "Yêu cầu nhập đúng định dạng")]
        public string DistancePriceString { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "PhoneCosts")]
        [RegularExpression(@"^[+]?([0-9]+([.][0-9]*)?|[.][0-9]+)$", ErrorMessage = "Yêu cầu nhập đúng định dạng")]
        public string PhoneCostsString { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "SupportCosts")]
        [RegularExpression(@"^[+]?([0-9]+([.][0-9]*)?|[.][0-9]+)$", ErrorMessage = "Yêu cầu nhập đúng định dạng")]
        public string SupportCostsString { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "BonusCosts")]
        [RegularExpression(@"^[+]?([0-9]+([.][0-9]*)?|[.][0-9]+)$", ErrorMessage = "Yêu cầu nhập đúng định dạng")]
        public string BonusCostsString { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "InsuranceCosts")]
        [RegularExpression(@"^[+-]?([0-9]+([.][0-9]*)?|[.][0-9]+)$", ErrorMessage = "Yêu cầu nhập đúng định dạng")]
        public string InsuranceCostsString { get; set; }


    }
}