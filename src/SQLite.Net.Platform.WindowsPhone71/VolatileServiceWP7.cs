using SQLite.Net.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

namespace SQLite.Net.Platform.WindowsPhone71
{
    class VolatileServiceWP7: IVolatileService
    {
        public void Write(ref int transactionDepth, int depth)
        {
            //TODO ?
            Thread.MemoryBarrier();
            transactionDepth = depth;
            Debug.WriteLine("Volatile.Write({0},{1}", transactionDepth, depth);
        }
    }
}
