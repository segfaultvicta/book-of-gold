using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Book_of_Gold
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message + 
                Environment.NewLine + Environment.NewLine + 
                e.Exception.InnerException.Message + Environment.NewLine + "====================================" + Environment.NewLine +
                e.Exception.InnerException.StackTrace, "Something has gone horribly awry.", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
