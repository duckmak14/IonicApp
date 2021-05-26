using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebModels;

namespace WEB.Models
{
    public class TransportPlanExport : TransportPlan
    {

        public String RouteCode { get; set; }
        public String SourcePartnerName { get; set; }
        public String DestinationPartnerName { get; set; }
        public String CarOwerName { get; set; }
        public String NumberPlate { get; set; }
        public String VehicleWeightName { get; set; }
        public String StartLocationnName { get; set; }
        public String EndLocationName { get; set; }
        public String ActualWeightName { get; set; }

    }
}