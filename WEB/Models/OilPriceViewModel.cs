using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WebModels;

namespace WEB.Models
{
    public class OilPriceViewModel: OilPrice
    {
        [Required(ErrorMessage = "Vui lòng nhập giá dầu")]
        [Display(Name = "Giá dầu")]
        [RegularExpression(@"^[+]?([0-9]+([.][0-9]*)?|[.][0-9]+)$", ErrorMessage = "Yêu cầu nhập đúng định dạng")]
        public string OilPrice { get; set; }
    }
}                                                                         