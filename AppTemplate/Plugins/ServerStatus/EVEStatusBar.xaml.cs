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
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ServerStatus
{
   public partial class EVEStatusBar : UserControl, INotifyPropertyChanged
   {
      public event PropertyChangedEventHandler PropertyChanged;

      protected void OnPropertyChanged(params string[] propertyNames)
      {
         foreach (string propertyName in propertyNames)
         {
            if (PropertyChanged != null)
               PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
         }
      }

      private DispatcherTimer _tqStatusTimer = null;
      private DispatcherTimer _sgStatusTimer = null;

      private ResourceDictionary ImageLibrary
      {
         get
         {
            string uriAddress = @"pack://application:,,,/ServerStatus;component/Resources/Images.xaml";

            ResourceDictionary imageLibrary = null;
            foreach (ResourceDictionary rd in System.Windows.Application.Current.Resources.MergedDictionaries)
            {
               if (rd.Source.ToString() == uriAddress)
               {
                  imageLibrary = rd;
                  break;
               }
            }

            if (imageLibrary == null)
            {
               imageLibrary = new ResourceDictionary();
               imageLibrary.Source = new Uri(uriAddress);

               System.Windows.Application.Current.Resources.MergedDictionaries.Add(imageLibrary);
            }

            return imageLibrary;
         }
      }

      private ImageSource _tqStatusImage = null;
      public ImageSource TranquilityStatusImage
      {
         get
         {
            return _tqStatusImage;
         }

         set 
         {
            _tqStatusImage = value;
            OnPropertyChanged("TranquilityStatusImage");
         }
      }

      private ImageSource _sgStatusImage = null;
      public ImageSource SingularityStatusImage
      {
         get
         {
            return _sgStatusImage;
         }

         set
         {
            _sgStatusImage = value;
            OnPropertyChanged("SingularityStatusImage");
         }
      }

      public string _tranquilityStatus = "Waiting to contact server";
      public string TranquilityStatus
      {
         get { return _tranquilityStatus; }
         set { _tranquilityStatus = value; OnPropertyChanged("TranquilityStatus"); }
      }

      public string _singularityStatus = "Waiting to contact server";
      public string SingularityStatus
      {
         get { return _singularityStatus; }
         set { _singularityStatus = value; OnPropertyChanged("SingularityStatus"); }
      }

      private static object lock_ = new object();

      public EVEStatusBar()
      {
         _tqStatusImage = ImageLibrary["green_image"] as ImageSource;
         _sgStatusImage = ImageLibrary["green_image"] as ImageSource;

         InitializeComponent();
      }

      private void Test()
      {
      }

      private void CreateServerWatchDog(DispatcherTimer timer, string uri, string serverName, PropertyInfo imageProperty, PropertyInfo statusProperty)
      {
         timer.Interval = TimeSpan.FromSeconds(5);
         timer.Tick += delegate
         {
             using (BackgroundWorker worker = new BackgroundWorker())
             {
                 worker.DoWork += delegate
                 {
                     EVE.Net.ServerStatus status = null;

                     lock (lock_)
                     {
                         status = CheckServerStatus(uri);
                     }

                     ImageSource currentImage = imageProperty.GetGetMethod().Invoke(this, null) as ImageSource;

                     if (status.serverOpen)
                     {
                         Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                             (DispatcherOperationCallback)delegate(object arg)
                             {
                                 if (currentImage != ImageLibrary["blue_image"])
                                     imageProperty.GetSetMethod().Invoke(this, new object[] { ImageLibrary["blue_image"] });

                                 statusProperty.GetSetMethod().Invoke(this, new object[]{string.Format("{0} is online{1}{2} players connected", 
                                                              serverName, 
                                                              Environment.NewLine,
                                                              status.onlinePlayers)});

                                 return null;
                             }, null);
                     }
                     else
                     {
                         Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                             (DispatcherOperationCallback)delegate(object arg)
                             {
                                 if (currentImage != ImageLibrary["red_image"])
                                     imageProperty.GetSetMethod().Invoke(this, new object[] { ImageLibrary["red_image"] });

                                 statusProperty.GetSetMethod().Invoke(this, new object[] { string.Format("{0} is offline", serverName) });

                                 return null;
                             }, null);
                     }
                 };

                 worker.RunWorkerAsync(Dispatcher);
             }

            if (timer.Interval == TimeSpan.FromSeconds(5))
               timer.Interval = TimeSpan.FromMinutes(3);
         };

         timer.Start();
      }

      private EVE.Net.ServerStatus CheckServerStatus(string server)
      {
         string oldUri = EVE.Net.Settings.APIUri;
         EVE.Net.ServerStatus status = null;
        
         try
         {
            EVE.Net.Settings.APIUri = server;
            status = new EVE.Net.ServerStatus();

            status.Query();
         }
         finally
         {
            EVE.Net.Settings.APIUri = oldUri;
         }

         return status;
      }

      private void onLoaded(object sender, RoutedEventArgs e)
      {
         _tqStatusTimer = new DispatcherTimer();
         _sgStatusTimer = new DispatcherTimer();

         CreateServerWatchDog(_tqStatusTimer, EVE.Net.EVEConstants.tranquilityAPIUri, "Tranquility",
                              this.GetType().GetProperty("TranquilityStatusImage"),
                              this.GetType().GetProperty("TranquilityStatus"));

         CreateServerWatchDog(_sgStatusTimer, EVE.Net.EVEConstants.singularityAPIUri, "Singularity",
                              this.GetType().GetProperty("SingularityStatusImage"),
                              this.GetType().GetProperty("SingularityStatus"));

      }
   
      private void onUnloaded(object sender, RoutedEventArgs e)
      {
         if (_tqStatusTimer != null && _tqStatusTimer.IsEnabled)
            _tqStatusTimer.Stop();

         if (_sgStatusTimer != null && _sgStatusTimer.IsEnabled)
            _sgStatusTimer.Stop();
      }
   }
}
