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
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using AppTemplate.Interfaces;
using AppTemplate.Utilities;
using AvalonDock;

namespace AppTemplate
{
   public partial class MainWindow : Window
   {
      internal List<IAppPlugin> plugins = new List<IAppPlugin>();

      [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303:DoNotPassLiteralsAsLocalizedParameters")]
      public MainWindow()
      {
         InitializeComponent();

         Title = (Application.Current as IAppTemplate).Settings["ApplicationName"] as string;

         Application.Current.Resources["ThemeDictionary"] = new ResourceDictionary();

         try
         {
            LoadPlugins();
         }
         catch (Exception ex)
         {
            MessageBox.Show("Error: Unable to load plugins.  Details: " + ex.Format(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
         }
      }

      private void LoadPlugins()
      {
         if (plugins.Count == 0)
         {
            string pluginPath = (Application.Current as IAppTemplate).Settings["installPath"] as string;
            pluginPath = Path.Combine(pluginPath, "Plugins");

            PluginLoader loader = new PluginLoader(pluginPath);

            List<IAppPlugin> found_plugins = loader.FindPlugins<IAppPlugin>(false);

            if (found_plugins.Count > 0)
               plugins.AddRange(found_plugins);

            foreach (IAppPlugin plugin in found_plugins)
            {
               plugins.Add(plugin);

               plugin.Initialize();
            }
         }
      }


      private void onDockManagerLoaded(object sender, RoutedEventArgs e)
      {
      }

      private void SetDefaultTheme(object sender, RoutedEventArgs e)
      {
         ThemeFactory.ResetTheme();
      }

      private void ChangeCustomTheme(object sender, RoutedEventArgs e)
      {
         string uri = (string)((MenuItem)sender).Tag;
         ThemeFactory.ChangeTheme(new Uri(uri, UriKind.RelativeOrAbsolute));
      }

      private void ChangeStandardTheme(object sender, RoutedEventArgs e)
      {
         string name = (string)((MenuItem)sender).Tag;
         ThemeFactory.ChangeTheme(name);
      }

      private void ChangeColor(object sender, RoutedEventArgs e)
      {
         ThemeFactory.ChangeColors((Color)ColorConverter.ConvertFromString(((MenuItem)sender).Header.ToString()));
      }

   }
}
