using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.AccessControl;
using System.Web;
using WebModels;

namespace WEB.Models
{
    public class ManageOilViewModel: ManageOil
    {
        [Display(ResourceType = typeof(WebResources), Name = "CarOwerName")]
        public string CarOwerName { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "NumberPlate")]
        public string NumberPlate { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số Km")]
        [Display(ResourceType = typeof(WebResources), Name = "Km")]
        [RegularExpression(@"^[+]?([0-9]+([.][0-9]*)?|[.][0-9]+)$", ErrorMessage = "Yêu cầu nhập đúng định dạng")]
        public string DistanceString { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập định mức dầu")]
        [Display(ResourceType = typeof(WebResources), Name = "OilLevel")]
        [RegularExpression(@"^[+]?([0-9]+([.][0-9]*)?|[.][0-9]+)$", ErrorMessage = "Yêu cầu nhập đúng định dạng")]
        public string OilLevelString { get; set; }


        [Required(ErrorMessage = "Vui lòng nhập số lít đã cấp")]
        [Display(ResourceType = typeof(WebResources), Name = "OilLevel")]
        [RegularExpression(@"^[+]?([0-9]+([.][0-9]*)?|[.][0-9]+)$", ErrorMessage = "Yêu cầu nhập đúng định dạng")]
        public string SuppliedOilString { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "OtherRun")]
        [RegularExpression(@"^[+]?([0-9]+([.][0-9]*)?|[.][0-9]+)$", ErrorMessage = "Yêu cầu nhập đúng định dạng")]
        public string OtherRunString { get; set; }

    }
}