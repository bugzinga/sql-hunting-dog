
using System;
using System.Collections.Generic;
using System.Linq;

namespace HuntingDog.Core
{
    public static class Extensions
    {
        public static Boolean IsEmpty<T>(this IEnumerable<T> value)
        {
            return !value.Any();
        }
    }
}
