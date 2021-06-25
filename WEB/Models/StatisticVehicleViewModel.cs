using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.AccessControl;
using System.Web;
using WebModels;

namespace WEB.Models
{
    public class StatisticVehicleViewModel
    {
        public DateTime StatisticVehicleDate { get; set; }

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


        public Double? TotalParking{ get; set; }


        public Double? TotalMonthTicket { get; set; }

        public Double? TotalDateTicket { get; set; }

        public Double? TotalTransportActual{ get; set; }



        public Double Total
        {
            get
            {
                Double total = 0; Double driverPay = 0; Double oil = 0; Double salary = 0; Double month = 0; Double parking = 0; Double date = 0; Double actual = 0;
                if (TotalDriverPay != null)
                {
                    driverPay = (Double)TotalDriverPay;
                }
                if (TotalOil != null)
                {
                    oil = (Double)TotalOil;
                }
                if (TotalParking != null)
                {
                    parking = (Double)TotalParking;
                }
                if (TotalSalary != null)
                {
                    salary = (Double)TotalSalary;
                }
                if (TotalMonthTicket != null)
                {
                    month = (Double)TotalMonthTicket;
                }
                if (TotalDateTicket != null)
                {
                    date = (Double)TotalDateTicket;
                }
                if (TotalTransportActual != null)
                {
                    actual = (Double)TotalTransportActual;
                }
                total = driverPay + oil + salary + month + date + actual + parking;

                return total;
            }
        }

        public string dataString;

    }
}