using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BooksTextsSplit.Library.Helpers
{
    public static class ClassesMethodsNames
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static string GetMyMethodName()
        {
            var st = new StackTrace(new StackFrame(1));
            return st.GetFrame(0).GetMethod().Name;
        }
    }
}
