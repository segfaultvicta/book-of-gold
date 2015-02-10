using FirstFloor.ModernUI.Presentation;
using System;
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

namespace Book_of_Gold.Pages
{
    /// <summary>
    /// Interaction logic for Fiend.xaml
    /// </summary>
    public partial class FiendBuilder : UserControl
    {
        public FiendBuilder()
        {
            InitializeComponent();
            lightWeaponChoice.Background = new SolidColorBrush(AppearanceManager.Current.AccentColor);
        }

        private void lightWeaponChoice_Click(object sender, RoutedEventArgs e)
        {

        }

        private void mediumWeaponChoice_Click(object sender, RoutedEventArgs e)
        {
            
        }

        private void heavyWeaponChoice_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
