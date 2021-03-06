﻿using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using DM_Suite.Menu_Features;
using DM_Suite.Initiative_Features;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace DM_Suite
{
    public sealed partial class RootControl : UserControl
    {
        private Type currentPage;

        // List of ValueTuple holding the Navigation Tag and the relative Navigation Page 
        private readonly IList<(string Tag, Type Page)> _pages = new List<(string Tag, Type Page)>
        {
            ("home", typeof(HomeScreen)),
            ("menu", typeof(MenuPage)),
            ("initiative", typeof(InitiativePage))
        };

        public RootControl()
        {
            InitializeComponent();
            RootFrame.Navigated += OnNavigated;
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            currentPage = e.SourcePageType;
        }

        public Frame RootFrame
        {
            get
            {
                return rootFrame;
            }
        }

        private void NavigationView_Loaded(object sender, RoutedEventArgs e)
        {
            NavView.IsPaneOpen = false;
            NavigationView_Navigate("home");

            rootFrame.Navigated += On_Navigated;
        }

        private void NavigationView_Navigate(string navItemTag)
        {
            var item = _pages.First(p => p.Tag.Equals(navItemTag));
            if (currentPage == item.Page)
                return;
            rootFrame.Navigate(item.Page);

            currentPage = item.Page;
        }

        private void On_Navigated(object sender, NavigationEventArgs e)
        {
            var item = _pages.First(p => p.Page == e.SourcePageType);

            NavView.SelectedItem = NavView.MenuItems
                .OfType<NavigationViewItem>()
                .First(n => n.Tag.Equals(item.Tag));
        }

        private void NavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.InvokedItem == null)
                return;
            else
            {
                if (args.IsSettingsInvoked)
                    // TODO: Create a Settings Page.
                    //ContentFrame.Navigate(typeof(SettingsPage));
                    return;
                else
                {
                    // Getting the Tag from Content (args.InvokedItem is the content of NavigationViewItem)
                    var navItemTag = NavView.MenuItems
                        .OfType<NavigationViewItem>()
                        .First(i => args.InvokedItem.Equals(i.Content))
                        .Tag.ToString();

                    NavigationView_Navigate(navItemTag);
                }
            }
        }
    }
}

