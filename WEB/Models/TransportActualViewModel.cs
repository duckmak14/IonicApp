
using WebModels;

namespace WEB.Models
{
    public class TransportActualViewModel : TransportActual
    {
        public string StatusString
        {
            get
            {
                string status = "Chờ duyệt";
                if (Status)
                {
                    status = "Đã duyệt";
                }
                return status;
            }
        }

        public string RouteCode { get; set; }
        public string VehicleWeightName { get; set; }
        public string CarOwerName { get; set; }
        public string NumberPlate { get; set; }
        public string SourcePartnerName { get; set; }
        public string DestinationPartnerName { get; set; }
        public string ActualWeightName { get; set; }
        public string StartLocationName { get; set; }
        public string EndLocationName { get; set; }
    }
}