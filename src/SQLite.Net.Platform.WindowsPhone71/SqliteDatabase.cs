//  $Header$
using System;
using System.Collections;
using System.Collections.Generic;
using Community.CsharpSqlite;

namespace SQLite.Net.Platform.WindowsPhone71
{

    
    using sqlite = Community.CsharpSqlite.Sqlite3.sqlite3;
    using Vdbe = Community.CsharpSqlite.Sqlite3.Vdbe;
    /// <summary>
    /// C#-SQLite wrapper with functions for opening, closing and executing queries.
    /// </summary>
    public class SQLiteDatabase
    {
        // pointer to database
        private sqlite db;

        /// <summary>
        /// Creates new instance of SQLiteBase class with no database attached.
        /// </summary>
        public SQLiteDatabase()
        {
            db = null;
        }
        /// <summary>
        /// Creates new instance of SQLiteDatabase class and opens database with given name.
        /// </summary>
        /// <param name="DatabaseName">Name (and path) to SQLite database file</param>
        public SQLiteDatabase(String DatabaseName)
        {
            OpenDatabase(DatabaseName);
        }

        /// <summary>
        /// Opens database. 
        /// </summary>
        /// <param name="DatabaseName">Name of database file</param>
        public void OpenDatabase(String DatabaseName)
        {
            // opens database 
            if (
#if NET_35
 Sqlite3.Open
#else
Sqlite3.sqlite3_open
#endif
(DatabaseName, out db) != Sqlite3.SQLITE_OK)
            {
                // if there is some error, database pointer is set to 0 and exception is throws
                db = null;
                throw new Exception("Error with opening database " + DatabaseName + "!");
            }
        }

        /// <summary>
        /// Closes opened database.
        /// </summary>
        public void CloseDatabase()
        {
            // closes the database if there is one opened
            if (db != null)
            {
#if NET_35
        Sqlite3.Close
#else
                Sqlite3.sqlite3_close
#endif
(db);
            }
        }

        /// <summary>
        /// Returns connection
        /// </summary>
        public sqlite Connection()
        {
            return db;
        }       

        /// <summary>
        /// Executes query that does not return anything (e.g. UPDATE, INSERT, DELETE).
        /// </summary>
        /// <param name="query"></param>
        public void ExecuteNonQuery(String query)
        {
            // calles SQLite function that executes non-query
            Sqlite3.exec(db, query, 0, 0, 0);
            // if there is error, excetion is thrown
            if (db.errCode != Sqlite3.SQLITE_OK)
                throw new Exception("Error with executing non-query: \"" + query + "\"!\n" +
#if NET_35
 Sqlite3.Errmsg
#else
 Sqlite3.sqlite3_errmsg
#endif
(db));
        }
    }
}
