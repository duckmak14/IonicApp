using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebModels;

namespace WEB.Models
{
    public class LocationExport : Location
    {

        public String DistrictName { get; set; }
        public String ProvinceName { get; set; }

    }
}