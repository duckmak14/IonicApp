using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.AccessControl;
using System.Web;
using WebModels;

namespace WEB.Models
{
    public class StatisticDriverViewModel
    {
        public DateTime StatisticDriverDate { get; set; }

        public int ID { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "CarOwerName")]
        public string CarOwerName { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "NumberPlate")]
        public string NumberPlate { get; set; }


        [Display(ResourceType = typeof(WebResources), Name = "TotalDriverPay")]

        public Double? TotalDriverPay { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "TotalOil")]

        public Double? TotalOil { get; set; }

        [Display(ResourceType = typeof(WebResources), Name = "TotalSalary")]

        public Double? TotalSalary { get; set; }


        public Double? TotalMortgageCost { get; set; }


        public Double? TotalAdvanceCost { get; set; }


        public Double? TotalOtherCost { get; set; }

    
        public Double Total
        {
            get
            {
                Double total = 0; Double driverPay = 0; Double oil = 0; Double salary = 0; Double mortgage = 0; Double advance = 0; Double other = 0;
                if(TotalDriverPay != null)
                {
                    driverPay = (Double)TotalDriverPay;
                }
                if (TotalOil != null)
                {
                    oil = (Double)TotalOil;
                }
                if (TotalAdvanceCost != null)
                {
                    advance = (Double)TotalAdvanceCost;
                }
                if (TotalSalary != null)
                {
                    salary = (Double)TotalSalary;
                }
                if (TotalMortgageCost != null)
                {
                    mortgage = (Double)TotalMortgageCost;
                }
                if (TotalOtherCost != null)
                {
                    other = (Double)TotalOtherCost;
                }

                total = driverPay + oil + salary + mortgage + advance + other;

                return total;
            }
        }

        public string dataString;

    }
}