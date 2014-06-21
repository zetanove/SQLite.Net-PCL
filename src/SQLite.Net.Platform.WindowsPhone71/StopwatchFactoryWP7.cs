using SQLite.Net.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace SQLite.Net.Platform.WindowsPhone71
{
    public class StopwatchFactoryWP7 : IStopwatchFactory
    {
        public IStopwatch Create()
        {
            return new StopwatchWP7();
        }

        private class StopwatchWP7 : IStopwatch
        {
            DispatcherTimer timer;
            long startTicks;

            public StopwatchWP7()
            {
                timer=new DispatcherTimer();
            }

            public void Stop()
            {
                timer.Stop();
            }

            public void Reset()
            {
                timer.Stop();
                Start();
            }

            public void Start()
            {
                startTicks = DateTime.Now.Ticks;
                timer.Start();
            }

            public long ElapsedMilliseconds
            {
                get { 
                    return DateTime.Now.Ticks - startTicks; 
                }
            }
        }
    }
}
