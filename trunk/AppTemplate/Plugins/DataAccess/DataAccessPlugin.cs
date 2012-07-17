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
using System.Windows;
using System.Windows.Controls;
using AppTemplate.Interfaces;
using AvalonDock;
using System.Collections.Generic;
using System.Windows.Threading;
using System.ComponentModel;

namespace DataAccess
{
   public class DataAccessPlugin : IAppPlugin, IDBProvider
   {
      private Settings settings = new Settings();

      public System.Collections.Generic.List<EVE.Net.RefTypes.ReferenceType> ReferenceTypes
      {
         get { return settings.ReferenceTypes; }
      }

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
            return System.IO.Path.Combine((Application.Current as IAppTemplate).Settings["pluginPath"] as string, @"DataAccess\Data\" + currentDB);
         }
      }

      private SQLiteDataBase db = null;

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

      public void Initialize()
      {
         Settings.Load(SettingsFile, ref settings);

         db = new SQLiteDataBase(dbFilePath);

         MenuItem menu = (Application.Current as IAppTemplate).RequestDockingPoint<MenuItem>(this) as MenuItem;

         MenuItem pluginMenu = new MenuItem();
         pluginMenu.Header = "Data";
         menu.Items.Add(pluginMenu);

         MenuItem sqlMenu = new MenuItem();
         sqlMenu.Header = "Query SQL";
         sqlMenu.Click += onQuerySql;
         pluginMenu.Items.Add(sqlMenu);

         InitializeUpdaters();
      }

      public void Unload()
      {
         Settings.Save(SettingsFile, ref settings);
      }

      private void InitializeUpdaters()
      {
         DispatcherTimer refTypesTimer = new DispatcherTimer();

         refTypesTimer.Interval = TimeSpan.FromSeconds(5);
         refTypesTimer.Tick += delegate
         {
            refTypesTimer.Interval = TimeSpan.FromMinutes(10);

            using (BackgroundWorker worker = new BackgroundWorker())
            {
               worker.DoWork += delegate
               {
                  UpdateReferenceTypes();
               };

               worker.RunWorkerAsync(Application.Current.Dispatcher);
            }
         };

         refTypesTimer.Start();
      }

      private void UpdateReferenceTypes()
      {
         DateTime tmNow = DateTime.Now;

         if ((tmNow - settings.TimeOfLastRefTypesUpdate).TotalDays > 0)
         {
            EVE.Net.RefTypes refTypes = new EVE.Net.RefTypes();

            refTypes.Query();

            settings.ReferenceTypes = new List<EVE.Net.RefTypes.ReferenceType>(refTypes.refTypes);

            settings.TimeOfLastRefTypesUpdate = tmNow;
         }
      }

      private void onQuerySql(object sender, RoutedEventArgs e)
      {
         DocumentPane documentPane = (Application.Current as IAppTemplate).RequestDockingPoint<DocumentPane>(this) as DocumentPane;

         DockableContent dc = new DockableContent();
         dc.Title = "SQL Query";
         dc.Content = new QueryWindow();

         documentPane.Items.Add(dc);
         dc.Focus();
      }
   }
}
