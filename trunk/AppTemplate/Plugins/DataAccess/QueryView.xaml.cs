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
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using AppTemplate.Utilities;
using ICSharpCode.AvalonEdit.Highlighting;
using Microsoft.Windows.Controls;

namespace DataAccess
{
   public partial class QueryView : UserControl
   {

      public class SqlRowsVM : ViewModel
      {
         private DataGrid _dataGrid = null;

         public DataGrid DataGrid
         {
            get { return _dataGrid; }
            set 
            { 
               _dataGrid = value;
               OnPropertyChanged(() => RowCount);
            }
         }

         public SqlRowsVM() { }

         public int RowCount
         {
            get
            {
               if (DataGrid != null && DataGrid.DataContext != null)
               {
                  return (DataGrid.DataContext as DataView).Table.Rows.Count;
               }

               return 0;
            }
            set 
            {
               OnPropertyChanged(() => RowCount);
            }
         }
      }

      SqlRowsVM _vm = new SqlRowsVM();

      public SqlRowsVM VM
      {
         get { return _vm; }
         set { _vm = value; }
      }

      public QueryView()
      {
         if (HighlightingManager.Instance.GetDefinition("Sql") == null)
         {
            IHighlightingDefinition customHighlighting;

            using (Stream s = GetType().Assembly.GetManifestResourceStream("DataAccess.Resources.SQL-Mode.xshd"))
            {
               if (s == null)
                  throw new InvalidOperationException("Could not find embedded resource");

               using (XmlReader reader = new XmlTextReader(s))
               {
                  customHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.
                     HighlightingLoader.Load(reader, HighlightingManager.Instance);
               }
            }

            HighlightingManager.Instance.RegisterHighlighting("Sql", new string[] { ".sql", ".*", "" }, customHighlighting);
         }

         InitializeComponent();

         _editor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("Sql");
         _dataGrid.DataContextChanged += onDataContextChanged;
      }

      private void onDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
      {
         if (e.NewValue != null)
         {
            VM.DataGrid = (sender as DataGrid);
         }
      }
   }
}
