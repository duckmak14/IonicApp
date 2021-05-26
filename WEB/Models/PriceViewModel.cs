using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WEB.Models
{
    public class PriceViewModel
    {
        public int ID { get; set; }
        [Required(ErrorMessage = "Vui lòng điền giá")]
        [Display(Name = "Giá")]
        public String Price { get; set;}
        [Required(ErrorMessage = "Vui lòng chọn mã lộ trình")]
        [Display(Name = "Mã lộ trình")]
        public int RouteID { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn nhà thầu")]
        [Display(Name = "Nhà thầu")]
        public Nullable<int> SourcePartnerID { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn đơn vị thuê")]
        [Display(Name = "Đơn vị thuê")]
        public Nullable<int> DestinationPartnerID { get; set; }
        [Required(ErrorMessage = "Vui lòng chọn tải trọng")]
        [Display(Name = "Tải trọng")]
        public Nullable<int> WeightID { get; set; }
        public String Note { get; set; }

    }
}