using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebModels;

namespace WEB.Models
{
    public class TransportActualExport : TransportActual
    {

        public String SourcePartnerName { get; set; }
        public String DestinationPartnerName { get; set; }
        public String CarOwerName { get; set; }
        public String NumberPlate { get; set; }
        public String VehicleWeightName { get; set; }
        public String StartLocationName { get; set; }
        public String EndLocationName { get; set; }
        public String ActualWeightName { get; set; }
        public String SourcePartnerAddress { get; set; }
        public String SourcePartnerMobile { get; set; }
        public String DestinationPartnerAddress { get; set; }
        public String DestinationPartnerMobile { get; set; }
       

    }
}