﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebModels;

namespace WEB.Models
{
    public class DataJsonDriverPay 
    {
        public int VehicleID { get; set; }
        public int DriverID { get; set; }
        public Double? TotalDriverPay { get; set; }

    }
}