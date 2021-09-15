using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;

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
        }

        private void btnViewException_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SomeMethod();
            }
            catch (Exception ex)
            {
                ExceptionViewer ev = new ExceptionViewer("An unexpected error occurred in the application.", ex, this);
                ev.ShowDialog();
            }
        }

        void SomeMethod()
        {
            AnotherMethod();
        }

        void AnotherMethod()
        {
            int x = 1;
            int y = 0;

            try
            {
                int z = x / y;
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
    }
}
