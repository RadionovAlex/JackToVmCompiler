using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackToVmCompiler.Utils
{
    internal static class ListUtils
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable) =>
            enumerable == null || enumerable.Count() == 0;
    }
}
