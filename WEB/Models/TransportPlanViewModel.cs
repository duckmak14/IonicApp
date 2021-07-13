using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebModels;

namespace WEB.Models
{
    public class TransportPlanViewModel : TransportPlan
    {
        public DateTime? StartDateTime
        {
            get
            {
                if (!StartTime.HasValue)
                {
                    return null;
                }

                var date = new DateTime(StartTime.Value);
                return date;
            }
        }

        public DateTime? EndDateTime
        {
            get
            {
                if (!EndTime.HasValue)
                {
                    return null;
                }

                var date = new DateTime(EndTime.Value);
                return date;
            }
        }

        public string StatusString
        {
            get
            {
                string status = "Chưa chạy";
                if (Status == true)
                {
                    status = "Hoàn thành";
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