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
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using AppTemplate.Interfaces;
using AppTemplate.Utilities;

namespace Plugins.AccountManager
{
   public class AccountManagerPlugin : IAppPlugin
   {
      private List<apiKeyViewModel> apiKeys_ = new List<apiKeyViewModel>();
      public List<apiKeyViewModel> apiKeys
      {
         get { return apiKeys_; }
         set { apiKeys_ = value; }
      }

      public string Name { get { return this.GetType().Name; } }

      public string SettingsFile
      {
         get
         {
            return System.IO.Path.Combine((Application.Current as IAppTemplate).Settings["settingsPath"] as string, "AccountManager.xml");
         }
      }

      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
      public void LoadSettings()
      {
         try
         {
            if (System.IO.File.Exists(SettingsFile))
               apiKeys = AppTemplate.Utilities.Serializer.Deserialize(SettingsFile, typeof(List<apiKeyViewModel>)) as List<apiKeyViewModel>;
         }
         catch (Exception ex)
         {
            MessageBox.Show("AccountManager :: Error while attempting to read settings file.  Details: " + ex.Format(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
         }
      }

      public void SaveSettings()
      {
         //try
         //{
         //   AppTemplate.Utilities.Serializer.Serialize(SettingsFile, apiKeys);
         //}
         //catch { }
      }

      public void Initialize()
      {
         LoadSettings();

         MenuItem menu = (Application.Current as IAppTemplate).RequestDockingPoint<MenuItem>(this) as MenuItem;

         MenuItem pluginMenu = new MenuItem();
         pluginMenu.Header = "Accounts";
         menu.Items.Add(pluginMenu);

         //MenuItem selectCharMenu = new MenuItem();
         //selectCharMenu.Header = "Select Character";
         //pluginMenu.Items.Add(selectCharMenu);

         MenuItem manageAccountsMenu = new MenuItem();
         manageAccountsMenu.Header = "Manage Characters";
         manageAccountsMenu.Click += onManageCharacters;
         pluginMenu.Items.Add(manageAccountsMenu);
      }

      public void Unload()
      {
         SaveSettings();
      }

      private void onManageCharacters(object sender, RoutedEventArgs e)
      {
         AccountManagerWindow dlg = new AccountManagerWindow();

         dlg.ShowDialog();
      }
   }
}
