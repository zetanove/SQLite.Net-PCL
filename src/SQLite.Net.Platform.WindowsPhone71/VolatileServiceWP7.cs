using SQLite.Net.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SQLite.Net.Platform.WindowsPhone71
{
    class VolatileServiceWP7: IVolatileService
    {
        public void Write(ref int transactionDepth, int depth)
        {
            //TODO ?
            transactionDepth = depth;
        }
    }
}
