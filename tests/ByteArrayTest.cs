﻿using System.Linq;
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
    public class ByteArrayTest
    {
        [SetUp]
        public void SetUp()
        {
            _sqlite3Platform = new SQLitePlatformTest();
        }

        private SQLitePlatformTest _sqlite3Platform;

        public class ByteArrayClass
        {
            [PrimaryKey, AutoIncrement]
            public int ID { get; set; }

            public byte[] bytes { get; set; }

            public void AssertEquals(ByteArrayClass other)
            {
                Assert.AreEqual(other.ID, ID);
                if (other.bytes == null || bytes == null)
                {
                    Assert.IsNull(other.bytes);
                    Assert.IsNull(bytes);
                }
                else
                {
                    Assert.AreEqual(other.bytes.Length, bytes.Length);
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        Assert.AreEqual(other.bytes[i], bytes[i]);
                    }
                }
            }
        }

        [Test]
        [Description("Create objects with various byte arrays and check they can be stored and retrieved correctly")]
        public void ByteArrays()
        {
            //Byte Arrays for comparisson
            ByteArrayClass[] byteArrays =
            {
                new ByteArrayClass
                {
                    bytes = new byte[] {1, 2, 3, 4, 250, 252, 253, 254, 255}
                }, //Range check
                new ByteArrayClass
                {
                    bytes = new byte[] {0}
                }, //null bytes need to be handled correctly
                new ByteArrayClass
                {
                    bytes = new byte[] {0, 0}
                },
                new ByteArrayClass
                {
                    bytes = new byte[] {0, 1, 0}
                },
                new ByteArrayClass
                {
                    bytes = new byte[] {1, 0, 1}
                },
                new ByteArrayClass
                {
                    bytes = new byte[] {}
                }, //Empty byte array should stay empty (and not become null)
                new ByteArrayClass
                {
                    bytes = null
                } //Null should be supported
            };

            var database = new SQLiteConnection(_sqlite3Platform, TestPath.GetTempFileName());
            database.CreateTable<ByteArrayClass>();

            //Insert all of the ByteArrayClass
            foreach (ByteArrayClass b in byteArrays)
            {
                database.Insert(b);
            }

            //Get them back out
            ByteArrayClass[] fetchedByteArrays = database.Table<ByteArrayClass>().OrderBy(x => x.ID).ToArray();

            Assert.AreEqual(fetchedByteArrays.Length, byteArrays.Length);
            //Check they are the same
            for (int i = 0; i < byteArrays.Length; i++)
            {
                byteArrays[i].AssertEquals(fetchedByteArrays[i]);
            }
        }

        [Test]
        [Description("Create A large byte array and check it can be stored and retrieved correctly")]
        public void LargeByteArray()
        {
            const int byteArraySize = 1024*1024;
            var bytes = new byte[byteArraySize];
            for (int i = 0; i < byteArraySize; i++)
            {
                bytes[i] = (byte) (i%256);
            }

            var byteArray = new ByteArrayClass
            {
                bytes = bytes
            };

            var database = new SQLiteConnection(_sqlite3Platform, TestPath.GetTempFileName());
            database.CreateTable<ByteArrayClass>();

            //Insert the ByteArrayClass
            database.Insert(byteArray);

            //Get it back out
            ByteArrayClass[] fetchedByteArrays = database.Table<ByteArrayClass>().ToArray();

            Assert.AreEqual(fetchedByteArrays.Length, 1);

            //Check they are the same
            byteArray.AssertEquals(fetchedByteArrays[0]);
        }
    }
}