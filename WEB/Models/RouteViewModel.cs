using System.ComponentModel.DataAnnotations;

namespace WEB.Models
{
    public class RouteViewModel
    {
        public int ID { get; set; }
        [Required(ErrorMessage = "Yêu cầu chọn nơi trả")]
        [Display(Name = "Nơi trả")]
        public int? EndLocationID { get; set; }
        [Required(ErrorMessage = "Yêu cầu chọn nơi nhận")]
        [Display(Name = "Nơi nhận")]
        public int? StartLocationID { get; set; }
        [Required(ErrorMessage = "Yêu cầu nhập mã lộ trình")]
        [Display(Name = "Mã lộ trình")]
        public string RouteCode { get; set; }

        [RegularExpression(@"^[+]?([0-9]+([.][0-9]*)?|[.][0-9]+)$", ErrorMessage = "Yêu cầu nhập đúng định dạng")]
        [Display(Name = "Khoảng cách")]
        public string Distance { get; set; }
    }

}