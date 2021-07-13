using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WEB.Models
{
    public class PercentPriceViewModel
    {
        [Required(ErrorMessage = "Vui lòng chọn phần trăm")]
        [Display(Name = "Phần trăm")]
        [RegularExpression(@"^[+-]?([0-9]+([.][0-9]*)?|[.][0-9]+)$", ErrorMessage = "Yêu cầu nhập đúng định dạng")]
        public string Percent { get; set; }
    }
}                                                                         