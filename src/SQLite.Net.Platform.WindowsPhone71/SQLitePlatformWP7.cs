using SQLite.Net.Interop;
using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace SQLite.Net.Platform.WindowsPhone71
{
    public class SQLitePlatformWP7 : ISQLitePlatform
    {
        public SQLitePlatformWP7()
        {
            var api = new SQLiteApiWP7();

            //            api.SetDirectory(/*temp directory type*/2, Windows.Storage.ApplicationData.Current.TemporaryFolder.Path);

            SQLiteApi = api;
            VolatileService = new VolatileServiceWP7();
            ReflectionService = new ReflectionServiceWP7();
            StopwatchFactory = new StopwatchFactoryWP7();
        }

        public ISQLiteApi SQLiteApi { get; private set; }
        public IStopwatchFactory StopwatchFactory { get; private set; }
        public IReflectionService ReflectionService { get; private set; }
        public IVolatileService VolatileService { get; private set; }
    }
}
