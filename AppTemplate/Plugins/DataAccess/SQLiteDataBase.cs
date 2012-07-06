//
// The MIT License (MIT)
//
// Copyright (C) 2012 Gary McNickle
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal 
// in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell 
// copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, 
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Data;
using System.Data.SQLite;
using AppTemplate.Utilities;

namespace DataAccess
{
   public class SQLiteDataBase
   {
      private string dbFilePath { get; set; }
      private string dbConnection { get; set; }

      public SQLiteDataBase(string db_file)
      {
         dbFilePath = db_file;
         dbConnection = String.Format("Data Source={0}", db_file);
      }

      internal DataTable GetDataTable(string sql_query, params object[] args)
      {
         if (args != null && args.Length > 0)
            sql_query = string.Format(sql_query, args);
         
         DataTable dt = new DataTable();

         try
         {
            using (SQLiteConnection connection = new SQLiteConnection(dbConnection))
            {
               connection.Open();
               SQLiteCommand cmd = new SQLiteCommand(connection);

               cmd.CommandText = sql_query;

               using (SQLiteDataReader reader = cmd.ExecuteReader())
               {
                  dt.Load(reader);
               }
            }
         }
         catch (Exception Ex)
         {
            System.Windows.MessageBox.Show(Ex.Format(), "SQL Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
         }

         return dt;
      }

      internal int ExecuteNonQuery(string sql_query, params object[] args)
      {
         if (args != null && args.Length > 0)
            sql_query = string.Format(sql_query, args);

         try
         {
            using (SQLiteConnection connection = new SQLiteConnection(dbConnection))
            {
               connection.Open();

               using (SQLiteCommand cmd = new SQLiteCommand(connection))
               {
                  cmd.CommandText = sql_query;

                  return cmd.ExecuteNonQuery();
               }
            }
         }
         catch (Exception)
         {
            throw;
         }
      }

      internal string ExecuteScalar(string sql_query, params object[] args)
      {
         if (args != null && args.Length > 0)
            sql_query = string.Format(sql_query, args);

         using (SQLiteConnection connection = new SQLiteConnection(dbConnection))
         {
            connection.Open();

            using (SQLiteCommand cmd = new SQLiteCommand(connection))
            {
               cmd.CommandText = sql_query;
               object value = cmd.ExecuteScalar();

               if (value != null)
               {
                  return value.ToString();
               }
            }
         }

         return "";
      }
   }

   /// <summary>
   /// Sample regular expression function.  Example Usage:
   /// SELECT * FROM foo WHERE name REGEXP '$bar'
   /// SELECT * FROM foo WHERE REGEXP('$bar', name)
   /// 
   /// </summary>
   [SQLiteFunction(Name = "REGEXP", Arguments = 2, FuncType = FunctionType.Scalar)]
   class RegexpFunction : SQLiteFunction
   {
      public override object Invoke(object[] args)
      {
         return System.Text.RegularExpressions.Regex.IsMatch(Convert.ToString(args[1]), Convert.ToString(args[0]));
      }
   }
}
