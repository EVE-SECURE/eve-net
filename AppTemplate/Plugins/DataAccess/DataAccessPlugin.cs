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
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using AppTemplate.Interfaces;
using AvalonDock;

namespace DataAccess
{
   public class DataAccessPlugin : IAppPlugin, IDBProvider
   {
      public string Name { get { return this.GetType().Name; } }

      public string SettingsFile
      {
         get
         {
            return System.IO.Path.Combine((Application.Current as IAppTemplate).Settings["settingsPath"] as string, "DataAccess.xml");
         }
      }

      private string dbFilePath
      {
         get
         {
            string currentDB = "cru16-sqlite3-v1.db";
            return System.IO.Path.Combine((Application.Current as IAppTemplate).Settings["pluginPath"] as string, @"DataAccess\" + currentDB);
         }
      }

      private SQLiteDataBase db = null;

      public DataTable dbLookupShipsByRaceAndClass()
      {
         return db.GetDataTable(SQLQueries.SHIPS_BY_RACE_AND_CLASS);
      }

      public DataTable dbLookupTypeNamesByID(params object[] typeIDs)
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("SELECT typeName FROM invtypes WHERE ");

         for (int x = 0; x < typeIDs.Length; x++)
         {
            sb.AppendFormat("typeID={0}", typeIDs[x].ToString());
            if (x < typeIDs.Length - 1)
               sb.Append(" OR ");
            else
               sb.Append(";");
         }

         return db.GetDataTable(sb.ToString());
      }

      public DataTable dbLookupTypeDetailsByID(params object[] typeIDs)
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("SELECT * FROM invtypes WHERE ");

         for (int x = 0; x < typeIDs.Length; x++)
         {
            sb.AppendFormat("typeID={0}", typeIDs[x].ToString());
            if (x < typeIDs.Length - 1)
               sb.Append(" OR ");
            else
               sb.Append(";");
         }

         return db.GetDataTable(sb.ToString());
      }

      public DataTable dbLookupAllPublishedModules()
      {
         return db.GetDataTable(SQLQueries.ALL_PUBLISHED_MODULES);
      }

      public DataTable dbLookupBasicShipFittingDetails(params object[] typeIDs)
      {
         StringBuilder sb = new StringBuilder();
         sb.Append("WHERE (");

         for (int x = 0; x < typeIDs.Length; x++)
         {
            sb.AppendFormat("invtypes.typeID={0}", typeIDs[x].ToString());

            if (x < typeIDs.Length - 1)
               sb.Append(" OR ");
            else
               sb.Append(") ");
         }

         return db.GetDataTable(SQLQueries.BASIC_SHIP_FITTING, sb.ToString());
      }

      public DataTable GetDataTable(string sql_query, params object[] args)
      {
         return db.GetDataTable(sql_query, args);
      }

      public int ExecuteNonQuery(string sql_query, params object[] args)
      {
         return db.ExecuteNonQuery(sql_query, args);
      }

      public string ExecuteScalar(string sql_query, params object[] args)
      {
         return db.ExecuteScalar(sql_query, args);
      }

      private void Report(DataTable dt)
      {
         foreach (DataColumn col in dt.Columns)
         {
            Debug.Write(string.Format("{0}\t", col));
         }
         Debug.WriteLine("");

         foreach (DataRow row in dt.Rows)
         {
            foreach (DataColumn col in dt.Columns)
            {
               Debug.Write(string.Format("{0}\t", row[col]));
            }

            Debug.WriteLine("");
         }

         Console.WriteLine("");
      }

      public void Initialize()
      {
         db = new SQLiteDataBase(dbFilePath);

         MenuItem menu = (Application.Current as IAppTemplate).RequestDockingPoint<MenuItem>(this) as MenuItem;

         MenuItem pluginMenu = new MenuItem();
         pluginMenu.Header = "Data";
         menu.Items.Add(pluginMenu);

         MenuItem sqlMenu = new MenuItem();
         sqlMenu.Header = "Query SQL";
         sqlMenu.Click += onQuerySql;
         pluginMenu.Items.Add(sqlMenu);
      }

      private void onQuerySql(object sender, RoutedEventArgs e)
      {
         //DockablePane dockPane = (Application.Current as IAppTemplate).RequestDockingPoint<DockablePane>(this) as DockablePane;

         //DockableContent dpc = new DockableContent();
         //dpc.Title = "Tables";
         //dpc.Content = new TableView();
         //dockPane.Items.Add(dpc);

         DocumentPane documentPane = (Application.Current as IAppTemplate).RequestDockingPoint<DocumentPane>(this) as DocumentPane;

         DockableContent dc = new DockableContent();
         dc.Title = "SQL Query";
         dc.Content = new QueryWindow();

         documentPane.Items.Add(dc);
         dc.Focus();
      }
   }
}
