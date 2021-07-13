using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.AccessControl;
using System.Web;
using WebModels;

namespace WEB.Models
{
    public class OtherCostViewModel: OtherCost
    {
        [Display(ResourceType = typeof(WebResources), Name = "CarOwerName")]
        public string CarOwerName { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "NumberPlate")]
        public string NumberPlate { get; set; }

       
        [Display(ResourceType = typeof(WebResources), Name = "AdvanceCosts")]
        [RegularExpression(@"^[+-]?([0-9]+([.][0-9]*)?|[.][0-9]+)$", ErrorMessage = "Yêu cầu nhập đúng định dạng")]
        public string AdvanceCostsString { get; set; }

       
        [Display(ResourceType = typeof(WebResources), Name = "MortgageCosts")]
        [RegularExpression(@"^[+-]?([0-9]+([.][0-9]*)?|[.][0-9]+)$", ErrorMessage = "Yêu cầu nhập đúng định dạng")]
        public string MortgageCostsString { get; set; }

     
        [Display(ResourceType = typeof(WebResources), Name = "OtherCosts")]
        [RegularExpression(@"^[+-]?([0-9]+([.][0-9]*)?|[.][0-9]+)$", ErrorMessage = "Yêu cầu nhập đúng định dạng")]
        public string OtherCostsString { get; set; }


    }
}