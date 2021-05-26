using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.AccessControl;
using System.Web;
using WebModels;

namespace WEB.Models
{
    public class TransportActualViewModel: TransportActual
    {
        public string StatusString 
        {
            get
            {
                string status = "Chờ duyệt";
                if(Status)
                {
                    status = "Đã duyệt";
                }  
                return status;
            }
        }

        [Display(ResourceType = typeof(WebResources), Name = "TripCount")]
        [RegularExpression(@"^[+-]?([0-9]+([.][0-9]*)?|[.][0-9]+)$", ErrorMessage = "Yêu cầu nhập đúng định dạng")]
        public string TripCountViewModel { get; set; }
    }
}