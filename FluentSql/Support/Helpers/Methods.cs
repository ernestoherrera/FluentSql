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

        public static string ADDYEARS = "AddYears";
        public static string ADDQUARTERS = "AddQuarters";
        public static string ADDMONTHS = "AddMonths";
        public static string ADDDAYOFYEAR = "AddDayOfYear";
        public static string ADDDAYS = "AddDays";
        public static string ADDWEEKS = "AddWeeks";
        public static string ADDDAYOFWEEK = "AddDayOfWeek";
        public static string ADDHOURS = "AddHours";
        public static string ADDMINUTES = "AddMinutes";
        public static string ADDSECONDS = "AddSeconds";
        public static string ADDMILLISECONDS = "AddMilliseconds";
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
        public static string NOW = "Now";
        #endregion
    }
}
