﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DesktopCS.Controls
{
    public class CSTabItem : TabItem
    {
        static CSTabItem()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CSTabItem), new FrameworkPropertyMetadata(typeof(CSTabItem)));
        }

        public static readonly DependencyProperty UnreadProperty = DependencyProperty.Register("Unread", typeof(bool), typeof(CSTabItem), new PropertyMetadata(false));

        public static readonly RoutedEvent CloseTabEvent =
            EventManager.RegisterRoutedEvent("CloseTab", RoutingStrategy.Bubble,
                typeof(RoutedEventHandler), typeof(CSTabItem));

        public event RoutedEventHandler CloseTab
        {
            add { AddHandler(CloseTabEvent, value); }
            remove { RemoveHandler(CloseTabEvent, value); }
        }

        [System.ComponentModel.Description("Shows or hides the unread glow.")]
        [System.ComponentModel.Category("Common")]
        public bool Unread
        {
            get { return (bool)GetValue(UnreadProperty); }
            set { SetValue(UnreadProperty, value); }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            var closeButton = GetTemplateChild("PART_Close") as Button;
            if (closeButton != null)
                closeButton.Click += closeButton_Click;
        }

        void closeButton_Click(object sender, RoutedEventArgs e)
        {
            var args = new RoutedEventArgs(CloseTabEvent, this);
            RaiseEvent(args);

            if (!args.Handled)
            {
                var tabItem = args.Source as TabItem;
                if (tabItem != null)
                {
                    var tabControl = tabItem.Parent as TabControl;
                    if (tabControl != null)
                        tabControl.Items.Remove(tabItem);
                }
            }
        }
    }
}
