using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Treorisoft.Net
{
    internal static class ExceptionExtensions
    {
        public static ErrorReceivedEventArgs AsEventArgs(this Exception ex, [CallerMemberName]string memberName = "")
        {
            return ErrorReceivedEventArgs.Create(ex, memberName);
        }
    }
}
