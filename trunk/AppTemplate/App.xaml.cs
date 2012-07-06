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
using System.Windows.Input;
using AppTemplate.Interfaces;
using AppTemplate.Utilities;
using AvalonDock;
using Microsoft.Win32;
using System.Threading;
using System.Globalization;
using System.Collections;

namespace AppTemplate
{
   public partial class App : Application, IAppTemplate, IPluginContainer, ISingleInstanceApp
   {
      public static string ApplicationName { get { return "EVE.Net"; } }

      private List<string> commandLineArguments = new List<string>();
      public List<string> CommandLineArguments
      {
         get { return commandLineArguments; }
         private set { commandLineArguments = value; }
      }

       public IDictionary Settings { get { return Properties; } }

       public FrameworkElement RequestDockingPoint<T>(IAppPlugin plugin) where T : FrameworkElement
       {
          if (MainWindow != null)
          {
             if (typeof(T) == typeof(Menu))
             {
                return (MainWindow as MainWindow).mainMenu;
             }
             if (typeof(T) == typeof(MenuItem))
             {
                return (MainWindow as MainWindow).pluginsMenu;
             }
             else if (typeof(T) == typeof(ToolBarTray))
             {
                return (MainWindow as MainWindow).pluginsToolbarTray;
             }
             else if (typeof(T) == typeof(ToolBarTray))
             {
                return (MainWindow as MainWindow).mainMenu;
             }
             else if (typeof(T) == typeof(DockablePane))
             {
                return (MainWindow as MainWindow).workPane;
             }
             else if (typeof(T) == typeof(DocumentPane))
             {
                return (MainWindow as MainWindow).docPane;
             }
          }

          throw new ArgumentException("Unable to locate a docking point of the requested type.");
       }

       private static void SelectivelyHandleMouseButton(object sender, MouseButtonEventArgs e)
       {
          var textbox = (sender as TextBox);
          if (textbox != null && !textbox.IsKeyboardFocusWithin)
          {
             if (e.OriginalSource.GetType().Name == "TextBoxView")
             {
                e.Handled = true;
                textbox.Focus();
             }
          }
       }

       private static void SelectAllText(object sender, RoutedEventArgs e)
       {
          var textBox = e.OriginalSource as TextBox;
          if (textBox != null)
             textBox.SelectAll();
       }

       protected override void OnStartup(StartupEventArgs e)
       {
          InitializeSettings();

          AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(AssemblyHelper.ResolveAssemblies);
          AppDomain.CurrentDomain.AssemblyLoad += new AssemblyLoadEventHandler(AssemblyHelper.AssemblyLoaded);

          AssemblyHelper.AddAssemblySearchPath(Path.Combine(Directory.GetCurrentDirectory(), "Plugins"), true);

          EventManager.RegisterClassHandler(typeof(TextBox), UIElement.PreviewMouseLeftButtonDownEvent,
             new MouseButtonEventHandler(SelectivelyHandleMouseButton), true);

          EventManager.RegisterClassHandler(typeof(TextBox), UIElement.GotKeyboardFocusEvent,
            new RoutedEventHandler(SelectAllText), true);

           base.OnStartup(e);

           // CULTURE TEST
           //Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("nl-BE");
           //Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo("nl-BE");
           //FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement),
           //   new FrameworkPropertyMetadata(System.Windows.Markup.XmlLanguage.GetLanguage(CultureInfo.CurrentCulture.IetfLanguageTag)));
       }

       private void InitializeSettings()
       {
          Properties["ApplicationName"] = ApplicationName;

          string installPath = Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), Properties["ApplicationName"] as string);
          string settingsPath = Path.Combine(Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments), Properties["ApplicationName"] as string);

          Properties["installPath"] = installPath;
          Properties["settingsPath"] = settingsPath;

          if (!Directory.Exists(installPath))
             Directory.CreateDirectory(installPath);

          if (!Directory.Exists(settingsPath))
             Directory.CreateDirectory(settingsPath);

          Properties["cachePath"] = Path.Combine(settingsPath, "Cache");
          Properties["imagesPath"] = Path.Combine(settingsPath, "Images");
          Properties["pluginPath"] = Path.Combine(installPath, "Plugins");
          Properties["configFile"] = Path.Combine(settingsPath, "settings.xml");

          EVE.Net.Settings.CacheFolder = Properties["cachePath"] as string;
          EVE.Net.Settings.ImagesFolder = Properties["imagesPath"] as string;

          AssemblyHelper.AddAssemblySearchPath(Properties["installPath"] as string, false);
          AssemblyHelper.AddAssemblySearchPath(Properties["pluginPath"] as string, true);
       }

       [System.STAThreadAttribute()]
       [System.Diagnostics.DebuggerNonUserCodeAttribute()]
       public static void Main()
       {
          if (SingleInstance<App>.InitializeAsFirstInstance(ApplicationName as string))
          {
             AppTemplate.App app = new AppTemplate.App();
             app.InitializeComponent();
             app.Run();

             SingleInstance<App>.Cleanup();
          }
       }

       public bool SignalExternalCommandLineArgs(IList<string> args)
       {
          List<string> arguments = new List<string>(args);
          return true;
       }

       public List<IAppPlugin> Plugins
       {
          get 
          {
             return (this.MainWindow as MainWindow).plugins;
          }
       }

       public IAppPlugin GetPlugin<T>() where T : IAppPlugin
       {
          foreach (IAppPlugin p in Plugins)
          {
             if (p.GetType() == typeof(T))
                return p;
          }
          return null;
       }
   }
}
