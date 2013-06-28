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
using System.Linq;
using System.Text;
using AppTemplate.Interfaces;
using System.Windows;
using System.Windows.Controls;
using AvalonDock;

namespace WalletReports
{
    class WalletReportsPlugin : IAppPlugin
    {
        public string Name { get { return this.GetType().Name; } }

        public string SettingsFile
        {
            get
            {
                return System.IO.Path.Combine((Application.Current as IAppTemplate).Settings["settingsPath"] as string, "WalletReports.xml");
            }
        }

        public void Initialize()
        {
           MenuItem menu = (Application.Current as IAppTemplate).RequestDockingPoint<MenuItem>(this) as MenuItem;

           MenuItem pluginMenu = new MenuItem();
           pluginMenu.Header = "Corp Wallet";
           pluginMenu.Click += doTest;

           menu.Items.Add(pluginMenu);
        }

        private void doTest(object sender, RoutedEventArgs e)
        {
           DocumentPane pane = (Application.Current as IAppTemplate).RequestDockingPoint<DocumentPane>(this) as DocumentPane;

           DockableContent dc = new DockableContent();
           dc.Title = "Corp Wallet";

           ReportsPage page = new ReportsPage();
           dc.Content = page;

           pane.Items.Add(dc);
           dc.Focus();
        }


        public void Unload()
        {
        }
    }
}
