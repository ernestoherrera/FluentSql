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

        #region DateAdd Function stubs
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
        #endregion

        #region DatePart Function stubs
        public static string GETYEAR = "GetYear";
        public static string GETQUARTER = "GetQuarter";
        public static string GETMONTH = "GetMonth";
        public static string GETDAYOFYEAR = "GetDayOfYear";
        public static string GETDAY = "GetDay";
        public static string GETWEEK = "GetWeek";
        public static string GETWEEKDAY = "GetWeekday";
        public static string GETHOUR = "GetHour";
        public static string GETMINUTE = "GetMinute";
        public static string GETSECOND = "GetSecond";
        public static string GETMILLISECOND = "GetMillisecond";
        #endregion

        public static string COMPARETO = "CompareTo";

        public static string YEAR = "Year";
        public static string NOW = "Now";
        #endregion
    }
}
