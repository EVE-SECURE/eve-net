﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AppTemplate.SharedControls
{
   public class CloseableTabItem : TabItem
   {
      static CloseableTabItem()
      {
         DefaultStyleKeyProperty.OverrideMetadata(typeof(CloseableTabItem),
             new FrameworkPropertyMetadata(typeof(CloseableTabItem)));
      }

      public static readonly RoutedEvent CloseTabEvent =
          EventManager.RegisterRoutedEvent("CloseTab", RoutingStrategy.Bubble,
              typeof(RoutedEventHandler), typeof(CloseableTabItem));

      public event RoutedEventHandler CloseTab
      {
         add { AddHandler(CloseTabEvent, value); }
         remove { RemoveHandler(CloseTabEvent, value); }
      }

      public override void OnApplyTemplate()
      {
         base.OnApplyTemplate();

         Button closeButton = base.GetTemplateChild("PART_Close") as Button;

         if (closeButton != null)
            closeButton.Click += new System.Windows.RoutedEventHandler(closeButton_Click);
      }

      void closeButton_Click(object sender, System.Windows.RoutedEventArgs e)
      {
         this.RaiseEvent(new RoutedEventArgs(CloseTabEvent, this));
      }
   }
}
