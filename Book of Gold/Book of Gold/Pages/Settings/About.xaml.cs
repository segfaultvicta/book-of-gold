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

namespace Book_of_Gold.Pages.Settings
{
    /// <summary>
    /// Interaction logic for About.xaml
    /// </summary>
    public partial class About : UserControl
    {
        private Dictionary<int, string> fiendstories = new Dictionary<int, string>();

        public About()
        {
            InitializeComponent();
            fiendstories[0] = "Long ago, in the time of dreams, there was a Sorceror without equal. \n\n The world was pale, and without interest to him! \n\n What fruit was there in wine? What tang in meat? \n\n There was nothing for it but to build a \"FIEND ENGINE\". \n\n Fiend after fiend marched out, which the Sorceror easily dispatched. \n\n But their meat was delicious!";
            fiendstories[1] = "Long ago, in the time of dreams, there was a Sorceror without companion. \n\n There is no ache in the world like loneliness! \n\n Who could she take to balls? Whose blood could she drink? \n\n There was nothing for it but to build a \"FIEND ENGINE.\" \n\n In a tremendous forge, it melted the stars into a handsome giant! \n\n Love lived in starlight is the best!";
            fiendstories[2] = "Long ago, in the time of dreams, there was a Sorceror heavy with regret. \n\n O, the scars of wars past!\n\n What hand or craft can make things as they weren't to be? \n\n There was nothing for it but to build a \"FIEND ENGINE\". \n\n In perfect simulacrum, they acted out a golden yesteryear. \n\nAnd as commanded, made the same mistakes!";
        }

        private void aboutText_Loaded(object sender, RoutedEventArgs e)
        {
            var random = new Random();
            this.aboutText.Text = fiendstories[random.Next(0,3)];
        }

        
    }
}
