using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebModels;

namespace WEB.Models
{
    public class DataJsonManageOil
    {
        public int VehicleID { get; set; }
        public int DriverID { get; set; }
        public Double TotalOil { get; set; }
        public string NumberPlate { get; set; }
        public string CarOwerName { get; set; }

    }
}