
using System;
using System.Collections.Generic;
using System.Linq;

namespace HuntingDog.Core
{
    public static class Extensions
    {
        public static Boolean IsEmpty<T>(this IEnumerable<T> collection)
        {
            return !collection.Any();
        }

        public static Boolean SafeRun(this Object o, Action action, String context)
        {
            var success = true;

            try
            {
                if (action != null)
                {
                    action();
                }
            }
            catch (Exception ex)
            {
                var errorMessage = (context != null)
                    ? (context + ": ")
                    : String.Empty;

                LogFactory.GetLog(o.GetType()).Error((errorMessage + ex.Message), ex);

                success = false;
            }

            return success;
        }
    }
}
