using System;
using System.Windows;
using WpfExceptionViewer;

namespace ExceptionViewerDemo
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();

            ExceptionViewer.DefaultTitle = "Sorry";
        }

        private void AnotherMethod()
        {
            var x = 1;
            var y = 0;

            try
            {
                var z = x / y;
            }
            catch (Exception ex)
            {
                var appEx = new ApplicationException("The calculation could not be completed.", ex);

                // Populate the Data member for demo purposes.

                appEx.Data.Add("x", x);
                appEx.Data.Add("y", y);

                throw appEx;
            }
        }

        private void btnViewException_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SomeMethod();
            }
            catch (Exception ex)
            {
                var ev = new ExceptionViewer("An unexpected error occurred in the application.", ex, this);
                ev.ShowDialog();
            }
        }

        private void killMe(object sender, RoutedEventArgs e)
        {
            SomeMethod();
        }

        private void SomeMethod()
        {
            AnotherMethod();
        }
    }
}