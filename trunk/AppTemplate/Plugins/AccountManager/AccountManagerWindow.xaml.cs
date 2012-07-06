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
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Data;
using AppTemplate.Interfaces;

namespace Plugins.AccountManager
{
   public partial class AccountManagerWindow : Window
   {
      private ObservableCollection<apiKeyViewModel> keys_ = new ObservableCollection<apiKeyViewModel>();
      public ObservableCollection<apiKeyViewModel> Keys
      {
         get { return keys_; }
         set { keys_ = value; }
      }

      public AccountManagerWindow()
      {
         if (Owner == null)
            Owner = Application.Current.MainWindow;

         AccountManagerPlugin manager = ((IPluginContainer)Application.Current).GetPlugin<AccountManagerPlugin>() as AccountManagerPlugin;

         keys_ = new ObservableCollection<apiKeyViewModel>(manager.apiKeys);

         InitializeComponent();
      }

      private void SaveKeys()
      {
         AccountManagerPlugin manager = ((IPluginContainer)Application.Current).GetPlugin<AccountManagerPlugin>() as AccountManagerPlugin;
         manager.apiKeys = new List<apiKeyViewModel>(keys_);
         AppTemplate.Utilities.Serializer.Serialize(manager.SettingsFile, new List<apiKeyViewModel>(Keys));
      }

      private void onAddCharacter(object sender, RoutedEventArgs e)
      {
         AddAPIKeyDlg dlg = new AddAPIKeyDlg();

         dlg.ShowDialog();

         if (dlg.DialogResult == true)
         {
            apiKeyViewModel newKey = dlg.Kvm;

            foreach (apiKeyViewModel vm in Keys)
            {
               if (newKey.ID == vm.ID && newKey.vCode == vm.vCode)
               {
                  MessageBox.Show("Duplicate API Key.");
                  return;
               }
            }

            Keys.Add(newKey);
            SaveKeys();
         }
      }

      private void onEditCharacter(object sender, RoutedEventArgs e)
      {
         if (_keyList.SelectedItem == null)
            return;

         apiKeyViewModel copy = AppTemplate.Utilities.Serializer.DeepCopy<apiKeyViewModel>(_keyList.SelectedItem);
         AddAPIKeyDlg dlg = new AddAPIKeyDlg(copy);

         dlg.ShowDialog();
         if (dlg.DialogResult == true)
         {
            copy.Refresh();

            keys_[_keyList.SelectedIndex] = dlg.Kvm;
            SaveKeys();
         }
      }

      private void onDeleteCharacter(object sender, RoutedEventArgs e)
      {
      }
    
      private void onClose(object sender, RoutedEventArgs e)
      {
         Close();
      }
   }

   [ValueConversion(typeof(List<EVE.Net.APIKeyInfo.Character>), typeof(string))]
   public class ListOfCharactersConverter : IValueConverter
   {
      public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         List<EVE.Net.APIKeyInfo.Character> chars = (value as List<EVE.Net.APIKeyInfo.Character>);

         StringBuilder sb = new StringBuilder();
         for (int x = 0; x < chars.Count; x++)
         {
            sb.AppendFormat("{0}{1}", chars[x].characterName, x < chars.Count - 1 ? "," : "");
         }

         return sb.ToString();
      }

      public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
      {
         throw new System.NotImplementedException();
      }
   }

}
