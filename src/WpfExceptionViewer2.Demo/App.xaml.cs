using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WpfExceptionViewer2;

namespace ExceptionViewerDemo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Debug.WriteLine($"Entered {nameof(App_DispatcherUnhandledException)}.");
            if (e != null)
            {
                new ExceptionViewer("Application.Current.DispatcherUnhandledException\nApplication stops working and needs to be restarted.", e.Exception).Show();
                e.Handled = true;
            }
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                new ExceptionViewer("AppDomain.CurrentDomain.UnhandledException\nApplication stops working and needs to be restarted.", ex).Show();
            }
            else
            {
                MessageBox.Show("AppDomain.CurrentDomain.UnhandledException\nApplication stops working and needs to be restarted.");
            }
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            if (e.Exception is Exception ex)
            {
                new ExceptionViewer("TaskScheduler.UnobservedTaskException\nApplication stops working and needs to be restarted.", ex).Show();
            }
            else
            {
                MessageBox.Show("AppDomain.CurrentDomain.UnhandledException\nApplication stops working and needs to be restarted.");
            }
        }
    }
}