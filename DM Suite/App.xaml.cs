﻿using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;
using Microsoft.Data.Sqlite;
using DM_Suite.Services.LoggingServices;
using DM_Suite.Menu_Features;
using DM_Suite.Initiative_Features;
using MetroLog;
using Windows.Storage;
using Windows.ApplicationModel.Resources;

namespace DM_Suite
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : Application
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            Suspending += OnSuspending;
            string databaseName = "DMSuiteDatabase";
            bool menuTablesCreated = MenuDBHelper.CreateDatabaseTables(databaseName);
            if (menuTablesCreated)
            {
                LoggingServices.Instance.WriteLine<App>("Sqlite Database Tables for MENU and MENU_ITEMS were created.", LogLevel.Info);
            }

            bool initiativeTableCreated = InitiativeDBHelper.CreateDatabaseTable(databaseName);
            if (initiativeTableCreated)
            {
                LoggingServices.Instance.WriteLine<App>("Sqlite Database Tables for INITIATIVE was created.", LogLevel.Info);
            }
        }

        ResourceLoader resourceLoader = ResourceLoader.GetForViewIndependentUse();

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            // Log application start
            LoggingServices.Instance.WriteLine<App>("Application starting...", LogLevel.Info);

            if (!(Window.Current.Content is RootControl rootControl))
            {
                // Create a Frame to act as the navigation context and navigate to the first page
                rootControl = new RootControl();

                rootControl.RootFrame.NavigationFailed += OnNavigationFailed;

                // Place the frame in the current Window
                Window.Current.Content = rootControl;
            }

            if (rootControl.RootFrame.Content == null)
            {
                // When the navigation stack isn't restored navigate to the first page,
                // configuring the new page by passing required information as a navigation
                // parameter
                rootControl.RootFrame.Navigate(typeof(HomeScreen), e.Arguments);
            }
            // Ensure the current window is active
            Window.Current.Activate();
        }
        

        /// <summary>
        /// Invoked when Navigation to a certain page fails
        /// </summary>
        /// <param name="sender">The Frame which failed navigation</param>
        /// <param name="e">Details about the navigation failure</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// Invoked when application execution is being suspended.  Application state is saved
        /// without knowing whether the application will be terminated or resumed with the contents
        /// of memory still intact.
        /// </summary>
        /// <param name="sender">The source of the suspend request.</param>
        /// <param name="e">Details about the suspend request.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            deferral.Complete();
        }
    }
}
