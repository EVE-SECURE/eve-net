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

using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace Plugins.AccountManager
{
   /// <summary>
   /// Interaction logic for AddAPIKeyDlg.xaml
   /// </summary>
   public partial class AddAPIKeyDlg : Window
   {
      private apiKeyViewModel kvm = new apiKeyViewModel();

      public apiKeyViewModel Kvm
      {
         get { return kvm; }
         set { kvm = value; }
      }

      public AddAPIKeyDlg()
         : this(null)
      {
      }

      public AddAPIKeyDlg(apiKeyViewModel vm)
      {
         if (vm != null)
            kvm = vm;

         InitializeComponent();
      }

      private void onGoToEveAPI(object sender, RequestNavigateEventArgs e)
      {
         Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
         e.Handled = true;
      }

      private void onLoadKey(object sender, RoutedEventArgs e)
      {
         kvm.Refresh();

         _charList.ItemsSource = null;
         _charList.ItemsSource = kvm.Characters;
      }

      private void onAddAccount(object sender, RoutedEventArgs e)
      {
         DialogResult = true;

         Close(); 
      }
   }
}
