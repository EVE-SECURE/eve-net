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
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using AppTemplate.Interfaces;
using AppTemplate.SharedControls;

namespace DataAccess
{
   public partial class QueryWindow : UserControl
   {
      private FileSystemWatcher watcher_ = null;

      public string QueryFolder
      {
         get
         {
            return System.IO.Path.Combine((Application.Current as IAppTemplate).Settings["settingsPath"] as string, @"DataAccess\Qeries");
         }
      }

      private ObservableCollection<string> savedQueries_ = null;

      public ObservableCollection<string> SavedQueries
      {
        get { return savedQueries_; }
        set { savedQueries_ = value; }
      }

      public string SelectedSqlQuery { get; set; }

      public QueryWindow()
      {
         if (Directory.Exists(QueryFolder))
         {
            SavedQueries = new ObservableCollection<string>(System.IO.Directory.GetFiles(QueryFolder, "*.sql", SearchOption.TopDirectoryOnly));
         }

         InitializeComponent();
      }

      private void SetupDirectoryWatcher()
      {
         if (!Directory.Exists(QueryFolder))
            Directory.CreateDirectory(QueryFolder);

         watcher_ = new FileSystemWatcher();
         watcher_.Filter = "*.sql";
         watcher_.Created += onFoundNewSqlQueryFileInFolder;
         watcher_.Deleted += onDeleteSqlFileInFolder;
         watcher_.Path = QueryFolder;
         watcher_.EnableRaisingEvents = true;
      }

      private void onFoundNewSqlQueryFileInFolder(object sender, FileSystemEventArgs e)
      {
         Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
         {
            if (!SavedQueries.Contains(e.FullPath))
               SavedQueries.Add(e.FullPath);
         });
      }

      private void onDeleteSqlFileInFolder(object sender, FileSystemEventArgs e)
      {
         Dispatcher.Invoke(DispatcherPriority.Normal, (Action)delegate()
         {
            SavedQueries.Remove(e.FullPath);
         });
      }

      private void doNewQuery(object sender, RoutedEventArgs e)
      {
         QueryView query = new QueryView();

         CloseableTabItem tab = new CloseableTabItem();
         tab.Content = query;
         tab.Header = "Query";
         tab.CloseTab += doCloseTab;

         _tabControl.Items.Add(tab);
         tab.Focus();
      }

      private void doCloseTab(object sender, RoutedEventArgs e)
      {
         _tabControl.Items.Remove((sender as CloseableTabItem));
      }

      private void doSave(object sender, RoutedEventArgs e)
      {
         QueryView query = _tabControl.SelectedContent as QueryView;
         
         if (query == null)
            return;

         InputPrompt promptDlg = new InputPrompt("Save", "Enter the filename to save as.", "FileName:");

         promptDlg.ShowDialog();

         if (promptDlg.DialogResult == true)
         {
            string outputFile = System.IO.Path.Combine(QueryFolder, promptDlg.InputString);

            try
            {
               if (String.IsNullOrEmpty(System.IO.Path.GetExtension(outputFile)))
                  outputFile = outputFile + ".sql";
            }
            catch (ArgumentException)
            {
               MessageBox.Show("Path contains invalid characters.");
               return;
            }

            (_tabControl.SelectedItem as TabItem).Header = System.IO.Path.GetFileNameWithoutExtension(outputFile);
            using (StreamWriter writer = new StreamWriter(outputFile))
            {
               writer.Write(query._editor.Text);
            }
         }
      }

      private void doSelectSqlScript(object sender, SelectionChangedEventArgs e)
      {
         if (String.IsNullOrEmpty(SelectedSqlQuery))
            return;

         QueryView query = (_tabControl.SelectedContent as QueryView);

         if (query == null)
            return;

         if (!String.IsNullOrEmpty(query._editor.Text))
         {
            if (MessageBox.Show("Replace the contents of the current query with the saved query?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
               return;
         }


         (_tabControl.SelectedItem as TabItem).Header = System.IO.Path.GetFileNameWithoutExtension(SelectedSqlQuery);
         query._editor.Load(SelectedSqlQuery);
      }

      private void doProcess(object sender, RoutedEventArgs e)
      {
         QueryView query = (_tabControl.SelectedContent as QueryView);
         if (query == null)
            return;

         string sql = query._editor.Text;

         DataTable data = ((Application.Current as IPluginContainer).GetPlugin<DataAccessPlugin>() as DataAccessPlugin).GetDataTable(sql);

         query._dataGrid.DataContext = data.DefaultView;
      }

      private void onLoaded(object sender, RoutedEventArgs e)
      {
         SetupDirectoryWatcher();
      }

      private void onUnloaded(object sender, RoutedEventArgs e)
      {
         if (watcher_ != null)
         {
            watcher_.Dispose();
            watcher_ = null;
         }
      }
   }
}
