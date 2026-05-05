using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoAssistantAppDataLibrary.Helpers
{
    public static class AutoAssistantStringFormatter
    {
        public static string FormatMpg(decimal mpg)
        {
            if (mpg == Constants.NO_MPG)
            {
                return "--";
            }

            return Math.Round(mpg, 1).ToString("N1");
        }

        public static string FormatMiles(decimal miles)
        {
            if (miles == Constants.NO_MILES)
            {
                return "--";
            }

            return Math.Round(miles).ToString("N0");
        }

        /// <summary>
        /// Returns "x miles"
        /// </summary>
        /// <param name="miles"></param>
        /// <returns></returns>
        public static string FormatMilesWithText(decimal miles)
        {
            return FormatMiles(miles) + " miles";
        }

        public static string FormatCost(decimal cost)
        {
            if (cost == Constants.NO_COST)
            {
                return "$--.--";
            }

            return cost.ToString("C");
        }

        public static string FormatGallons(decimal gallons)
        {
            if (gallons == Constants.NO_GALLONS)
            {
                return "--";
            }

            return Math.Round(gallons, 3).ToString("N3");
        }

        public static string FormatGallonsWithText(decimal gallons)
        {
            return FormatGallons(gallons) + " gallons";
        }

        public static string FormatPricePerGallonWithText(decimal costPerGallon)
        {
            return FormatCost(costPerGallon) + " per gallon";
        }
    }
}
