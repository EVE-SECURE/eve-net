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
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AppTemplate.Interfaces;
using AppTemplate.Utilities;
using EVE.Net;
using ICSharpCode.AvalonEdit.Folding;
using Plugins.AccountManager;

namespace Plugins.APITest
{
   public partial class APITestDialog : System.Windows.Controls.UserControl
   {
      public List<apiKeyViewModel> apiKeys
      {
         get 
         {
            AccountManager.AccountManagerPlugin manager = ((IPluginContainer)Application.Current).GetPlugin<AccountManager.AccountManagerPlugin>() as AccountManager.AccountManagerPlugin;

            return manager.apiKeys; 
         }

         set 
         { 
         }
      }

      public apiKeyViewModel SelectedKey { get; set; }


      private List<string> apiCalls_ = new List<string>();

      public List<string> APICalls
      {
         get { return apiCalls_; }
         set { apiCalls_ = value; }
      }

      public string SelectedImagePath { get; set; }

      public string SelectedCall { get; set; }

      private FoldingManager foldingManager = null;
      private XmlFoldingStrategy foldingStrategy = null;


      public APITestDialog()
      {
         InitializeComponent();

         foldingManager = FoldingManager.Install(xmlViewer.TextArea);
         foldingStrategy = new XmlFoldingStrategy();

         LoadAPICalls();
      }

      private void LoadAPICalls()
      {
         Assembly[] loadedAssemblies = AppDomain.CurrentDomain.GetAssemblies();

         foreach (Assembly asm in loadedAssemblies)
         {
            Type[] types = asm.GetTypes();

            AppTemplate.Utilities.ObjectFactory<APIObject> factory = AppTemplate.Utilities.ObjectFactory<APIObject>.Instance;
            bool loaded = false;

            foreach (Type t in types)
            {
               if (!t.IsAbstract && typeof(APIObject).IsAssignableFrom(t))
               {
                  if (!loaded)
                  {
                     loaded = true;
                     factory.LoadAdditionalAssembly(t);
                  }

                  APICalls.Add(t.FullName);
               }
            }
         }

         APICalls.Sort();
      }

      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
      private void doProcess(object sender, RoutedEventArgs e)
      {
         try
         {
            APIObject api = null;

            if (SelectedKey == null)
            {
               MessageBox.Show("Error: SelectedKey is NULL.");
               return;
            }

            if (SelectedKey.Characters.Count > 0)
            {
               if (SelectedKey.Characters.Count == 1)
               {
                  SelectedKey.ActorID = SelectedKey.Characters[0].characterID.ToString(CultureInfo.InvariantCulture);
               }
               else
               {
                  Plugins.AccountManager.SelectCharacterDlg dlg
                     = new Plugins.AccountManager.SelectCharacterDlg(SelectedKey.Characters);

                  dlg.ShowDialog();
                  if (dlg.DialogResult == true)
                  {
                     SelectedKey.ActorID = dlg.SelectedCharacter.characterID.ToString(CultureInfo.InvariantCulture);
                  }
               }
            }

            if (APICalls[apiBox.SelectedIndex].Contains("ImageServer"))
            {
               apiKeyViewModel keyInfo = new apiKeyViewModel(SelectedKey.ID, SelectedKey.vCode);
               keyInfo.Refresh();

               api = new ImageServer(keyInfo.KeyType == "Corporation" ? ImageServer.ImageType.Corporation : ImageServer.ImageType.Character, 256, SelectedKey.ActorID);
               api.Query();
            }
            else
            {
               api = AppTemplate.Utilities.ObjectFactory<APIObject>.Instance.Create(APICalls[apiBox.SelectedIndex]);

               api.actorID = SelectedKey.ActorID;
               api.vCode = SelectedKey.vCode;
               api.keyID = SelectedKey.ID;

               api.Query();
            }

            propertyGrid.SelectedObject = api;

            if (api is ImageServer)
            {
               ImagePathConverter ipc = new ImagePathConverter();

               SelectedImagePath = System.IO.Path.GetFullPath((api as ImageServer).FileName);
               imageView.Source = ipc.Convert(SelectedImagePath, null, null, null) as ImageSource;
               imageView.Height = imageView.Source.Height;
               imageView.Width = imageView.Source.Width;
            }
            else
            {
               xmlViewer.Load((api as IAPIReader).CacheFile);
               foldingStrategy.UpdateFoldings(foldingManager, xmlViewer.Document);
            }
         }
         catch (ArgumentException)
         {
            MessageBox.Show("This API call requires additional arguments which this test utility is not prepared to provide.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
         }
         catch (Exception ex)
         {
            MessageBox.Show("Unexpected error. Details: " + ex.Format(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
         }
      }
   }

   public class ImagePathConverter : IValueConverter
   {
      public object Convert(object value, Type targetType,
          object parameter, System.Globalization.CultureInfo culture)
      {
         if (value == null)
            return null;
         // value contains the full path to the image
         string path = (string)value;

         // load the image, specify CacheOption so the file is not locked
         BitmapImage image = new BitmapImage();
         image.BeginInit();
         image.CacheOption = BitmapCacheOption.OnLoad;
         image.UriSource = new Uri(path);
         image.EndInit();

         return image;
      }

      public object ConvertBack(object value, Type targetType,
          object parameter, System.Globalization.CultureInfo culture)
      {
         throw new NotImplementedException("The method or operation is not implemented.");
      }
   }
}
