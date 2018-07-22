using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Threading;
using System.Diagnostics;

namespace DataGridFromHaasQuotes
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private const int MINIMUM_SPLASH_TIME = 2500; // Miliseconds 
        private const int SPLASH_FADE_TIME = 1000;     // Miliseconds 

        protected override void OnStartup(StartupEventArgs e)
        {
            // Step 1 - Load the splash screen 
            SplashScreen splash = new SplashScreen("SplashScreen01.png");
            splash.Show(false, true);

            // Step 2 - Start a stop watch 
            Stopwatch timer = new Stopwatch();
            timer.Start();


            // Step 4 - Make sure that the splash screen lasts at least two seconds 
            timer.Stop();
            int remainingTimeToShowSplash = MINIMUM_SPLASH_TIME - (int)timer.ElapsedMilliseconds;
            if (remainingTimeToShowSplash > 0)
                Thread.Sleep(remainingTimeToShowSplash);

            // Step 5 - show the page 
            splash.Close(TimeSpan.FromMilliseconds(SPLASH_FADE_TIME));

            // Step 3 - Load your windows but don't show it yet 
            base.OnStartup(e);
            MainWindow main = new MainWindow();

            try
            {
                main.Show();
            }
            catch (Exception ex)
            {
                //occurs if user did not enter valid PQ and cancelled UserLogin (or some un-handled error occurred)
                main.Close();
                //return;
            }
        }
    }
}
