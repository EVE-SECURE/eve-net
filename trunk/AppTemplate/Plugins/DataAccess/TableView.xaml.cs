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


// #define WRITE_XSHD

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using AppTemplate.Interfaces;
using AppTemplate.SharedControls;


namespace DataAccess
{
   public partial class TableView : UserControl
   {
      public static readonly DependencyProperty SqlQueryTabControlProperty =
          DependencyProperty.RegisterAttached("SqlQueryTabControl",
          typeof(TabControl),
          typeof(TableView),
          new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.None));

      public TabControl SqlQueryTabControl
      {
         get
         {
            return (TabControl)GetValue(SqlQueryTabControlProperty);
         }
         set
         {
            SetValue(SqlQueryTabControlProperty, value);
         }
      }

      public List<TableVM> Tables { get; set; }

      public List<String> TableNames
      {
         get
         {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
               return new List<string>();

            DataTable data = ((Application.Current as IPluginContainer).GetPlugin<DataAccessPlugin>() as DataAccessPlugin).GetDataTable(@"SELECT * FROM sqlite_master WHERE type='table'");

            List<String> tables = new List<string>();

            foreach (DataRow row in data.Rows)
               tables.Add(row["tbl_name"].ToString());

            return tables;
         }
         set { }
      }

      public TableView()
      {
         Tables = new List<TableVM>();

         foreach (string table in TableNames)
         {
            TableVM tvm = new TableVM();
            tvm.TableName = table;
            tvm.ColumnNames = new List<TableColumn>();

            DataTable data = ((Application.Current as IPluginContainer).GetPlugin<DataAccessPlugin>() as DataAccessPlugin).GetDataTable(@"PRAGMA table_info(" + table + ")");

            foreach (DataRow row in data.Rows)
            {
               tvm.ColumnNames.Add(new TableColumn() { Name = row["name"].ToString() });
            }

            Tables.Add(tvm);
         }

         WriteOutTables();

         InitializeComponent();
      }

      [Conditional("WRITE_XSHD")]
      private void WriteOutTables()
      {
         List<string> allColumns = new List<string>();

         using (StreamWriter tableFile = new StreamWriter(@"d:\ccp_tables.xml"))
         {
            tableFile.WriteLine("<KeyWords name = \"CCPTables\" bold=\"false\" italic=\"false\" color=\"Teal\">");

            using (StreamWriter columnFile = new StreamWriter(@"d:\ccp_columns.xml"))
            {
               foreach (TableVM table in Tables)
               {
                  tableFile.WriteLine("<Key word = \"{0}\" />", table.TableName);
                  foreach (TableColumn column in table.ColumnNames)
                  {
                     if (!allColumns.Contains(column.Name))
                        allColumns.Add(column.Name);
                  }
               }

               columnFile.WriteLine("<KeyWords name = \"CCPTableColumns\" bold=\"false\" italic=\"false\" color=\"RoyalBlue\">");

               foreach (string column in allColumns)
               {
                  columnFile.WriteLine("<Key word = \"{0}\" />", column);
               }

               tableFile.WriteLine(@"</KeyWords>");
               columnFile.WriteLine(@"</KeyWords>");
            }
         }
      }

      private void onListViewSizeChanged(object sender, SizeChangedEventArgs e)
      {
          ListView listView = sender as ListView;
          GridView gView = listView.View as GridView;

          gView.Columns[0].Width = listView.ActualWidth;
      }

      private void DisplaySelectedTable(string tableName, int upperLimit)
      {
         QueryView query = new QueryView();
         query._editor.Text = "SELECT * FROM " + tableName;

         if (upperLimit > 0)
            query._editor.Text = query._editor.Text + " LIMIT 0, " + upperLimit.ToString();

         CloseableTabItem tab = new CloseableTabItem();
         tab.Content = query;
         tab.Header = "Query";
         tab.CloseTab += delegate
         {
            SqlQueryTabControl.Items.Remove(tab);
         };

         DataTable data = ((Application.Current as IPluginContainer).GetPlugin<DataAccessPlugin>() as DataAccessPlugin).GetDataTable(query._editor.Text);
         query._dataGrid.DataContext = data.DefaultView;

         SqlQueryTabControl.Items.Add(tab);
         tab.Focus();
      }

      private void onDisplayFirstOneThousandRows(object sender, RoutedEventArgs e)
      {
         if (!(sender is MenuItem)
         || !(((sender as MenuItem).Parent as ContextMenu).PlacementTarget is TextBlock))
         {
            return;
         }

         string tableName = (((sender as MenuItem).Parent as ContextMenu).PlacementTarget as TextBlock).Text;

         DisplaySelectedTable(tableName, 1000);
      }

      private void onDisplayAllRows(object sender, RoutedEventArgs e)
      {
         if (!(sender is MenuItem)
         || !(((sender as MenuItem).Parent as ContextMenu).PlacementTarget is TextBlock))
         {
            return;
         }

         string tableName = (((sender as MenuItem).Parent as ContextMenu).PlacementTarget as TextBlock).Text;

         DisplaySelectedTable(tableName, 0);
      }
   }

   public class TableColumn
   {
      public string Name { get; set; }
   }

   public class TableVM
   {
      public string TableName { get; set; }
      public List<TableColumn> ColumnNames { get; set; }
   }

}
