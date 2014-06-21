using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using SQLite.Net.Attributes;

#if __WIN32__
using SQLitePlatformTest = SQLite.Net.Platform.Win32.SQLitePlatformWin32;
#elif WINDOWS_PHONE
using SQLitePlatformTest = SQLite.Net.Platform.WindowsPhone8.SQLitePlatformWP8;
#elif __WINRT__
using SQLitePlatformTest = SQLite.Net.Platform.WinRT.SQLitePlatformWinRT;
#elif __IOS__
using SQLitePlatformTest = SQLite.Net.Platform.XamarinIOS.SQLitePlatformIOS;
#elif __ANDROID__
using SQLitePlatformTest = SQLite.Net.Platform.XamarinAndroid.SQLitePlatformAndroid;
#else
using SQLitePlatformTest = SQLite.Net.Platform.Generic.SQLitePlatformGeneric;
#endif


namespace SQLite.Net.Tests
{
    [TestFixture]
    public class InsertTest
    {
        [SetUp]
        public void Setup()
        {
            _db = new TestDb(TestPath.GetTempFileName());
        }

        [TearDown]
        public void TearDown()
        {
            if (_db != null)
            {
                _db.Close();
            }
        }

        private TestDb _db;

        public class TestObj
        {
            [AutoIncrement, PrimaryKey]
            public int Id { get; set; }

            public String Text { get; set; }

            public override string ToString()
            {
                return string.Format("[TestObj: Id={0}, Text={1}]", Id, Text);
            }
        }

        public class TestObj2
        {
            [PrimaryKey]
            public int Id { get; set; }

            public String Text { get; set; }

            public override string ToString()
            {
                return string.Format("[TestObj: Id={0}, Text={1}]", Id, Text);
            }
        }

        public class OneColumnObj
        {
            [AutoIncrement, PrimaryKey]
            public int Id { get; set; }
        }

        public class UniqueObj
        {
            [PrimaryKey]
            public int Id { get; set; }
        }

        public class TestDb : SQLiteConnection
        {
            public TestDb(String path)
                : base(new SQLitePlatformTest(), path)
            {
                CreateTable<TestObj>();
                CreateTable<TestObj2>();
                CreateTable<OneColumnObj>();
                CreateTable<UniqueObj>();
            }
        }

        [Test]
        public void InsertALot()
        {
            int n = 10000;
            IEnumerable<TestObj> q = from i in Enumerable.Range(1, n)
                select new TestObj
                {
                    Text = "I am"
                };
            TestObj[] objs = q.ToArray();
            _db.TraceListener = DebugTraceListener.Instance;

            var sw = new Stopwatch();
            sw.Start();

            int numIn = _db.InsertAll(objs);

            sw.Stop();

            Assert.AreEqual(numIn, n, "Num inserted must = num objects");

            TestObj[] inObjs = _db.CreateCommand("select * from TestObj").ExecuteQuery<TestObj>().ToArray();

            for (int i = 0; i < inObjs.Length; i++)
            {
                Assert.AreEqual(i + 1, objs[i].Id);
                Assert.AreEqual(i + 1, inObjs[i].Id);
                Assert.AreEqual("I am", inObjs[i].Text);
            }

            var numCount = _db.CreateCommand("select count(*) from TestObj").ExecuteScalar<int>();

            Assert.AreEqual(numCount, n, "Num counted must = num objects");
        }

        [Test]
        public void InsertAllFailureInsideTransaction()
        {
            List<UniqueObj> testObjects = Enumerable.Range(1, 20).Select(i => new UniqueObj
            {
                Id = i
            }).ToList();
            testObjects[testObjects.Count - 1].Id = 1; // causes the insert to fail because of duplicate key

            ExceptionAssert.Throws<SQLiteException>(() => _db.RunInTransaction(() => { _db.InsertAll(testObjects); }));

            Assert.AreEqual(0, _db.Table<UniqueObj>().Count());
        }

        [Test]
        public void InsertAllFailureOutsideTransaction()
        {
            List<UniqueObj> testObjects = Enumerable.Range(1, 20).Select(i => new UniqueObj
            {
                Id = i
            }).ToList();
            testObjects[testObjects.Count - 1].Id = 1; // causes the insert to fail because of duplicate key

            ExceptionAssert.Throws<SQLiteException>(() => _db.InsertAll(testObjects));

            Assert.AreEqual(0, _db.Table<UniqueObj>().Count());
        }

        [Test]
        public void InsertAllSuccessInsideTransaction()
        {
            List<UniqueObj> testObjects = Enumerable.Range(1, 20).Select(i => new UniqueObj
            {
                Id = i
            }).ToList();

            _db.RunInTransaction(() => { _db.InsertAll(testObjects); });

            Assert.AreEqual(testObjects.Count, _db.Table<UniqueObj>().Count());
        }

        [Test]
        public void InsertAllSuccessOutsideTransaction()
        {
            List<UniqueObj> testObjects = Enumerable.Range(1, 20).Select(i => new UniqueObj
            {
                Id = i
            }).ToList();

            _db.InsertAll(testObjects);

            Assert.AreEqual(testObjects.Count, _db.Table<UniqueObj>().Count());
        }

        [Test]
        public void InsertIntoOneColumnAutoIncrementTable()
        {
            var obj = new OneColumnObj();
            _db.Insert(obj);

            var result = _db.Get<OneColumnObj>(1);
            Assert.AreEqual(1, result.Id);
        }

        [Test]
        public void InsertIntoTwoTables()
        {
            var obj1 = new TestObj
            {
                Text = "GLaDOS loves testing!"
            };
            var obj2 = new TestObj2
            {
                Text = "Keep testing, just keep testing"
            };

            int numIn1 = _db.Insert(obj1);
            Assert.AreEqual(1, numIn1);
            int numIn2 = _db.Insert(obj2);
            Assert.AreEqual(1, numIn2);

            List<TestObj> result1 = _db.Query<TestObj>("select * from TestObj").ToList();
            Assert.AreEqual(numIn1, result1.Count);
            Assert.AreEqual(obj1.Text, result1.First().Text);

            List<TestObj> result2 = _db.Query<TestObj>("select * from TestObj2").ToList();
            Assert.AreEqual(numIn2, result2.Count);
        }

        [Test]
        public void InsertOrReplace()
        {
            _db.TraceListener = DebugTraceListener.Instance;
            _db.InsertAll(from i in Enumerable.Range(1, 20)
                select new TestObj
                {
                    Text = "#" + i
                });

            Assert.AreEqual(20, _db.Table<TestObj>().Count());

            var t = new TestObj
            {
                Id = 5,
                Text = "Foo",
            };
            _db.InsertOrReplace(t);

            List<TestObj> r = (from x in _db.Table<TestObj>() orderby x.Id select x).ToList();
            Assert.AreEqual(20, r.Count);
            Assert.AreEqual("Foo", r[4].Text);
        }

        [Test]
        public void InsertTwoTimes()
        {
            var obj1 = new TestObj
            {
                Text = "GLaDOS loves testing!"
            };
            var obj2 = new TestObj
            {
                Text = "Keep testing, just keep testing"
            };


            int numIn1 = _db.Insert(obj1);
            int numIn2 = _db.Insert(obj2);
            Assert.AreEqual(1, numIn1);
            Assert.AreEqual(1, numIn2);

            List<TestObj> result = _db.Query<TestObj>("select * from TestObj").ToList();
            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(obj1.Text, result[0].Text);
            Assert.AreEqual(obj2.Text, result[1].Text);
        }

        [Test]
        public void InsertWithExtra()
        {
            var obj1 = new TestObj2
            {
                Id = 1,
                Text = "GLaDOS loves testing!"
            };
            var obj2 = new TestObj2
            {
                Id = 1,
                Text = "Keep testing, just keep testing"
            };
            var obj3 = new TestObj2
            {
                Id = 1,
                Text = "Done testing"
            };

            _db.Insert(obj1);


            try
            {
                _db.Insert(obj2);
                Assert.Fail("Expected unique constraint violation");
            }
            catch (SQLiteException)
            {
            }
            _db.Insert(obj2, "OR REPLACE");


            try
            {
                _db.Insert(obj3);
                Assert.Fail("Expected unique constraint violation");
            }
            catch (SQLiteException)
            {
            }
            _db.Insert(obj3, "OR IGNORE");

            List<TestObj> result = _db.Query<TestObj>("select * from TestObj2").ToList();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(obj2.Text, result.First().Text);
        }
    }
}