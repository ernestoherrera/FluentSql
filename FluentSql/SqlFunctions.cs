using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FluentSql
{
    public class SqlFunctions
    {
        private static string FUNCTION_DIRECT_CALL = "This function can not be called directly.";

        #region DateAdd Function
        public static DateTime AddYears(DateTime fieldName, int numberOfYears)
        {
            throw new NotSupportedException(FUNCTION_DIRECT_CALL);
        }

        public static DateTime AddYears(DateTime? fieldName, int numberOfYears)
        {
            throw new NotSupportedException(FUNCTION_DIRECT_CALL);
        }

        public static DateTime AddQuarters(DateTime? fieldName, int numberOfQuarters)
        {
            throw new NotSupportedException(FUNCTION_DIRECT_CALL);
        }

        public static DateTime AddMonths(DateTime? fieldName, int numberOfMonths)
        {
            throw new NotSupportedException(FUNCTION_DIRECT_CALL);
        }

        public static DateTime AddDayOfYear(DateTime? fieldName, int numberOfYears)
        {
            throw new NotSupportedException(FUNCTION_DIRECT_CALL);
        }

        public static DateTime AddDays(DateTime? fieldName, int numberOfDays)
        {
            throw new NotSupportedException(FUNCTION_DIRECT_CALL);
        }

        public static DateTime AddWeeks(DateTime? fieldName, int numberOfWeeks)
        {
            throw new NotSupportedException(FUNCTION_DIRECT_CALL);
        }

        public static DateTime AddDayOfWeek(DateTime? fieldName, int dayOfWeek)
        {
            throw new NotSupportedException(FUNCTION_DIRECT_CALL);
        }

        public static DateTime AddHours(DateTime? fieldName, int numberOfHours)
        {
            throw new NotSupportedException(FUNCTION_DIRECT_CALL);
        }

        public static DateTime AddMinutes(DateTime? fieldName, int numberOfMinutes)
        {
            throw new NotSupportedException(FUNCTION_DIRECT_CALL);
        }

        public static DateTime AddSeconds(DateTime? fieldName, int numberOfSeconds)
        {
            throw new NotSupportedException(FUNCTION_DIRECT_CALL);
        }

        public static DateTime AddMilliseconds(DateTime? fieldName, int numberOfMilliseconds)
        {
            throw new NotSupportedException(FUNCTION_DIRECT_CALL);
        }
        #endregion

        #region DatePart Function
        public static int GetYear(DateTime? fieldName)
        {
            throw new NotSupportedException(FUNCTION_DIRECT_CALL);
        }

        public static int GetQuarter(DateTime? fieldName)
        {
            throw new NotSupportedException(FUNCTION_DIRECT_CALL);
        }

        public static int GetMonth(DateTime? fieldName)
        {
            throw new NotSupportedException(FUNCTION_DIRECT_CALL);
        }

        public static int GetDayOfYear(DateTime? fieldName)
        {
            throw new NotSupportedException(FUNCTION_DIRECT_CALL);
        }

        public static int GetDay(DateTime? fieldName)
        {
            throw new NotSupportedException(FUNCTION_DIRECT_CALL);
        }

        public static int GetWeek(DateTime? fieldName)
        {
            throw new NotSupportedException(FUNCTION_DIRECT_CALL);
        }

        public static int GetWeekday(DateTime? fieldName)
        {
            throw new NotSupportedException(FUNCTION_DIRECT_CALL);
        }

        public static int GetHour(DateTime? fieldName)
        {
            throw new NotSupportedException(FUNCTION_DIRECT_CALL);
        }

        public static int GetMinute(DateTime? fieldName)
        {
            throw new NotSupportedException(FUNCTION_DIRECT_CALL);
        }

        public static int GetSecond(DateTime? fieldName)
        {
            throw new NotSupportedException(FUNCTION_DIRECT_CALL);
        }

        public static int GetMillisecond(DateTime? fieldName)
        {
            throw new NotSupportedException(FUNCTION_DIRECT_CALL);
        }
        #endregion
    }
}
