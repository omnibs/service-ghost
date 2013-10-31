using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DebugConsole
{
    using ProjectUnderTest;

    class Program
    {
        static void Main(string[] args)
        {
            new ClassUnderTest().MethodUnderTest();
        }
    }
}
