using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql.Support.Helpers
{
    public class Methods
    {
        #region Supported .NET Where Clause Methods
        public static string STARTSWITH = "StartsWith";
        public static string ENDSWITH = "EndsWith";
        public static string CONTAINS = "Contains";
        public static string EQUALS = "Equals";
        public static string DATETIMENOW = "DateTime.Now";
        public static string DATE = "Date";
        public static string DAY = "Day";
        public static string DAYOFWEEK = "DayOfWeek"; // Not implemented
        public static string DAYOFYEAR = "DayOfYear"; // Not implemented
        public static string HOUR = "Hour";
        public static string KIND = "Kind";
        public static string ADDYEARS = "AddYears";
        public static string ADDDAYS = "AddDays";
        public static string ADDHOURS = "AddHours";
        public static string ADDMILLISECONDS = "AddMilliseconds";
        public static string ADDMINUTES = "AddMinutes";
        public static string ADDMONTHS = "AddMonths";
        public static string ADDSECONDS = "AddSeconds";
        public static string ADDTICKS = "AddTicks";
        public static string COMPARETO = "CompareTo";
        public static string MILLISECOND = "Millisecond";
        public static string MINUTE = "Minute";
        public static string MONTH = "Month";
        public static string SECOND = "Second";
        public static string TICKS = "Ticks";
        public static string TIMEOFDAY = "TimeOfDay";
        public static string TOSHORTDATESTRING = "ToShortDateString";
        public static string YEAR = "Year";
        #endregion
    }
}
