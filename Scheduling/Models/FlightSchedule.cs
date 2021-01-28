using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Scheduling.Models
{
   // [Keyless]
    public class FlightSchedule
    {

        //[DatabaseGenerated(DatabaseGeneratedOption.None)]
        //[Display(Name = "Number")]
        public int ScheduleID { get; set; }
        //Always 4 characters long. Padded with leading zeroes for flight numbers under 1000.
        public string FlightNumber { get; set; }

        //Always 14 characters long and always follows the DDMMMYY format.For flights with an open-ended period of operation, the second date is replaced with the “ XXX “ string.
        public DateTime PeriodOfOperationFrom { get; set; }
        public DateTime? PeriodOfOperationTo { get; set; }
        public bool FlyOnMondays { get; set; }
        public bool FlyOnTuesdays { get; set; }
        public bool FlyOnWednesdays { get; set; }
        public bool FlyOnThursdays { get; set; }
        public bool FlyOnFridays { get; set; }
        public bool FlyOnSaturdays { get; set; }
        public bool FlyOnSundays { get; set; }

        //Always 4 characters long. Displayed in military time (24 hour format).
        public string DepartureTime { get; set; }

        //Always 3 character long. IATA code for an airport
        public string OriginStation { get; set; }

        //Always 3 character long. IATA code for an airport
        public string DestinationStation { get; set; }

        //Always 3 character long. Represents the internal Air Canada aircraft number operating on this route.
        public string Aircraft { get; set; }

        public FlightSchedule() { }
        public FlightSchedule(string flightInfo)
        {

            string flightNumber = string.Empty;
            string periodOfOperation = string.Empty;
            string periodOfOperationFrom = string.Empty;
            string periodOfOperationTo = string.Empty;

            string daysOfOperation = string.Empty;
            string departureTime = string.Empty;
            string originStation = string.Empty;
            string destinationStation = string.Empty;
            string aircraft = string.Empty;

            flightNumber = GetText(flightInfo, 0, 6);
            periodOfOperation = GetText(flightInfo, 6, 14);

            periodOfOperationFrom = periodOfOperation.Substring(0, 7);
            periodOfOperationTo = periodOfOperation.Substring(7, 7);

            if (periodOfOperationTo.ToUpper().Contains("XXX"))
                periodOfOperationTo = null;

            daysOfOperation = GetText(flightInfo, 20, 7);
            departureTime = GetText(flightInfo, 27, 4);

            if (departureTime.Contains(".."))
                departureTime ="....";

            originStation = GetText(flightInfo, 31, 3);
            destinationStation = GetText(flightInfo, 34, 3);

            aircraft = GetText(flightInfo, 37, 3);
            if (aircraft.Contains(".."))
                aircraft = null;

            DateTime dtPeriodOfOperationFrom = GetDate(periodOfOperationFrom);

            DateTime? dtPeriodOfOperationTo = null;

            if (periodOfOperationTo != null)
                dtPeriodOfOperationTo = GetDate(periodOfOperationTo);


            FlightNumber = flightNumber;
            PeriodOfOperationFrom = dtPeriodOfOperationFrom;
            PeriodOfOperationTo = dtPeriodOfOperationTo;
            FlyOnMondays = (daysOfOperation.Contains("1")) ? true : false;
            FlyOnTuesdays = (daysOfOperation.Contains("2")) ? true : false;
            FlyOnWednesdays = (daysOfOperation.Contains("3")) ? true : false;
            FlyOnThursdays = (daysOfOperation.Contains("4")) ? true : false;
            FlyOnFridays = (daysOfOperation.Contains("5")) ? true : false;
            FlyOnSaturdays = (daysOfOperation.Contains("6")) ? true : false;
            FlyOnSundays = (daysOfOperation.Contains("7")) ? true : false;
            DepartureTime = departureTime;
            OriginStation = originStation;
            DestinationStation = destinationStation;
            Aircraft = aircraft;


        }

        string GetText(string text, int from, int to)
        {
            string subString = string.Empty;

            if (!string.IsNullOrWhiteSpace(text))
            {
                try
                {
                    subString = text.Substring(from, to);
                }
                catch (Exception exp)
                {

                }
            }

            return subString;

        }

        DateTime GetDate(string text)
        {
            DateTime dt = new DateTime();

            if (!string.IsNullOrWhiteSpace(text))
            {
                try
                {
                    dt = Convert.ToDateTime(text.Insert(5, DateTime.Now.Year.ToString().Substring(0, 2)));
                }
                catch (Exception exp)
                {

                }
            }

            return dt;

        }
    }
}
