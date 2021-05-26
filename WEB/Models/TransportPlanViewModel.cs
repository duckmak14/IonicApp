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
    }
}