using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebModels;

namespace WEB.Models
{
    public class DataJsonOtherCost
    {
      
        public int DriverID { get; set; }
        public Double? TotalOtherCost { get; set; }
        public Double? TotalAdvanceCost { get; set; }
        public Double? TotalMortgageCost { get; set; }

    }
}