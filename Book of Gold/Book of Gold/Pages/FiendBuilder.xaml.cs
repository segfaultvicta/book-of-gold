using FirstFloor.ModernUI.Presentation;
using FirstFloor.ModernUI.Windows.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
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
using Xceed.Wpf.Toolkit;

namespace Book_of_Gold.Pages
{
    /// <summary>
    /// Interaction logic for Fiend.xaml
    /// </summary>
    public partial class FiendBuilder : UserControl
    {
        private Fiend fiend = new Fiend();
        private AbilityList abilList = new AbilityList();
        private AbilityList knownAbilities = new AbilityList();
        private bool lightWeaponSelected = false;
        private bool mediumWeaponSelected = false;
        private bool heavyWeaponSelected = false;

        private Weapon lightWeapon = new Weapon("d8", 9, 40);
        private Weapon mediumWeapon = new Weapon("d10", 12, 50);
        private Weapon heavyWeapon = new Weapon("d12", 15, 60);

        public FiendBuilder()
        {
            InitializeComponent();

            availAbilitiesGrid.DataContext = abilList;
            knownAbilView.ItemsSource = knownAbilities;

            fiendName.AddHandler(PreviewMouseLeftButtonDownEvent, new MouseButtonEventHandler(SelectivelyIgnoreMouseButton), true);
            fiendName.AddHandler(GotKeyboardFocusEvent, new RoutedEventHandler(SelectAllText), true);
            fiendName.AddHandler(MouseDoubleClickEvent, new RoutedEventHandler(SelectAllText), true);
        }

        private void SelectAllText(object sender, RoutedEventArgs e)
        {
            fiendName.SelectAll();
        }

        private void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
        {
            if (!fiendName.IsKeyboardFocusWithin)
            {
                fiendName.Focus();
                e.Handled = true;
            }
        }

        private void update(bool fromLoading = false, bool rebuildAbilities = false, bool reprimeAbilities = false)
        {
            if (!IsLoaded && !fromLoading)
            {
                return;
            }

            hpText.Text = fiend.HP.ToString();
            mpText.Text = fiend.MP.ToString();
            lpText.Text = fiend.LP.ToString();
            tryUpdateSpinner(atkSpinner, fiend.ATK);
            tryUpdateSpinner(magSpinner, fiend.MAG);
            tryUpdateSpinner(wilSpinner, fiend.WIL);
            tryUpdateSpinner(stmSpinner, fiend.STM);
            tryUpdateSpinner(vitSpinner, fiend.VIT);
            tryUpdateSpinner(lukSpinner, fiend.LUK);
            apRemainingText.Text = fiend.AttributesRemaining.ToString();
            skillsRemainingText.Text = fiend.RemainingAbilities.ToString();
            if(fiend.RemainingAbilities < 0)
            {
                skillsRemainingText.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
                skillsRemainingText.Foreground = new SolidColorBrush(AppearanceManager.Current.AccentColor);
            }
            if(fiend.AttributesRemaining < 0)
            {
                apRemainingText.Foreground = new SolidColorBrush(Colors.Red);
            }
            else
            {
                apRemainingText.Foreground = new SolidColorBrush(AppearanceManager.Current.AccentColor);
            }

            this.updateAbilityList(rebuildAbilities);

            if(reprimeAbilities)
            {
                foreach(Ability a in knownAbilities)
                {
                    a.Prime(fiend);
                }
            }

            if (!fiend.WeaponFree)
            {
                lightWeaponChoice.IsEnabled = lightWeaponSelected;
                mediumWeaponChoice.IsEnabled = mediumWeaponSelected;
                heavyWeaponChoice.IsEnabled = heavyWeaponSelected;
            }
            else
            {
                lightWeaponChoice.IsEnabled = true;
                mediumWeaponChoice.IsEnabled = true;
                heavyWeaponChoice.IsEnabled = true;
            }

            lightWeaponChoice.Foreground =
                lightWeaponSelected ? new SolidColorBrush(AppearanceManager.Current.AccentColor) :
                lightWeaponChoice.IsEnabled ? new SolidColorBrush(Colors.White) :
                new SolidColorBrush(Colors.Gray);

            mediumWeaponChoice.Foreground =
                mediumWeaponSelected ? new SolidColorBrush(AppearanceManager.Current.AccentColor) :
                mediumWeaponChoice.IsEnabled ? new SolidColorBrush(Colors.White) :
                new SolidColorBrush(Colors.Gray);

            heavyWeaponChoice.Foreground =
                heavyWeaponSelected ? new SolidColorBrush(AppearanceManager.Current.AccentColor) :
                heavyWeaponChoice.IsEnabled ? new SolidColorBrush(Colors.White) :
                new SolidColorBrush(Colors.Gray);
        }

        private void updateAbilityList(bool rebuild)
        {
            knownAbilities.Clear();
            Ability attack = new Ability();
            attack.Name = "Attack";
            attack.Type = AbilityType.Action;
            attack.Basis = Basis.Weapon;
            attack.Target = "Single";
            attack.TakesActionSlot = false;
            attack.CleanDescription = "Deals weapon damage physical damage.";
            attack.COS = "CoS: 80";
            attack.Prime(fiend);
            attack.DamageMultiplier = 1.0M;
            attack.Category = "Basic";
            attack.Subcategory = "Attack";
            knownAbilities.Add(attack);
            this.addAbilitiesFrom(fiend.KnownAbilities, knownAbilities);

            if (rebuild)
            {
                this.buildAbilityList();
            }
        }

        private void buildAbilityList()
        {
            abilList.Clear();
            this.addAbilitiesFrom("General", abilList);
            
            if(fiend.Rank == Rank.Threat)
            {
                this.addAbilitiesFrom("Threat", abilList);
            }
            if (fiend.Rank == Rank.Boss)
            {
                this.addAbilitiesFrom("Threat", abilList);
                this.addAbilitiesFrom("Boss", abilList);
            } 
            if (fiend.Rank == Rank.Super)
            {
                this.addAbilitiesFrom("Threat", abilList);
                this.addAbilitiesFrom("Boss", abilList);
                this.addAbilitiesFrom("Super", abilList);
            }
            this.addAbilitiesFrom(fiend.Type.ToString(), abilList);
        }

        private void addAbilitiesFrom(string category, AbilityList to)
        {
            AbilityList temp = AbilityHandler.Instance.Fetch(category);
            foreach(Ability a in temp)
            {
                to.Add(a);
            }
        }

        private void addAbilitiesFrom(AbilityList from, AbilityList to)
        {
            foreach(Ability a in from)
            {
                to.Add(a);
            }
        }

        private void tryUpdateSpinner(Xceed.Wpf.Toolkit.IntegerUpDown spinner, int value)
        {
            if (value <= fiend.AttributeCap)
            {
                spinner.Value = value;
            }
            else
            {
                spinner.Value = fiend.AttributeCap;
            }
            spinner.Minimum = 4 + fiend.RankAttributeBonus;
            spinner.Maximum = fiend.AttributeCap;
            if (fiend.AttributesRemaining <= 0)
            {
                spinner.Maximum = spinner.Value;
            }
        }

        private void lightWeaponChoice_Click(object sender, RoutedEventArgs e)
        {
            if(lightWeaponSelected)
            {
                lightWeaponSelected = false;
                fiend.Weapons = fiend.Weapons.Where<Weapon>(w => w.Dice != "d8").ToList<Weapon>();
            }
            else
            {
                if(fiend.WeaponFree)
                {
                    lightWeaponSelected = true;
                    fiend.Weapons.Add(lightWeapon);
                }
            }
            update();
        }

        private void mediumWeaponChoice_Click(object sender, RoutedEventArgs e)
        {
            if(mediumWeaponSelected)
            {
                mediumWeaponSelected = false;
                fiend.Weapons = fiend.Weapons.Where<Weapon>(w => w.Dice != "d10").ToList<Weapon>();
            }
            else
            {
                if (fiend.WeaponFree)
                {
                    mediumWeaponSelected = true;
                    fiend.Weapons.Add(mediumWeapon);
                }
            }
            update();
        }

        private void heavyWeaponChoice_Click(object sender, RoutedEventArgs e)
        {
            if(heavyWeaponSelected)
            {
                heavyWeaponSelected = false;
                fiend.Weapons = fiend.Weapons.Where<Weapon>(w => w.Dice != "d12").ToList<Weapon>();
            }
            else
            {
                if (fiend.WeaponFree)
                {
                    heavyWeaponSelected = true;
                    fiend.Weapons.Add(heavyWeapon);
                }
            }
            update();
        }

        private void rankComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            fiend.Rank = (Rank)Enum.Parse(typeof(Rank), ((ComboBoxItem)rankComboBox.SelectedItem).Content.ToString());
            this.update(false, true);
        }

        private void typeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            fiend.Type = (Type)Enum.Parse(typeof(Type), ((ComboBoxItem)typeComboBox.SelectedItem).Content.ToString());
            this.update(false, true);
        }

        private void level_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            fiend.Level = (int)levelSlider.Value;
            this.update();
        }

        private void atkSpinner_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            fiend.ATKBase = (int)atkSpinner.Value - fiend.RankAttributeBonus - 4;
            this.update();
        }

        private void magSpinner_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            fiend.MAGBase = (int)magSpinner.Value - fiend.RankAttributeBonus - 4;
            this.update();
        }

        private void wilSpinner_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            fiend.WILBase = (int)wilSpinner.Value - fiend.RankAttributeBonus - 4;
            this.update();
        }

        private void vitSpinner_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            fiend.VITBase = (int)vitSpinner.Value - fiend.RankAttributeBonus - 4;
            this.update();
        }

        private void stmSpinner_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            fiend.STMBase = (int)stmSpinner.Value - fiend.RankAttributeBonus - 4;
            this.update();
        }

        private void lukSpinner_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            fiend.LUKBase = (int)lukSpinner.Value - fiend.RankAttributeBonus - 4;
            this.update();
        }

        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog d = new Microsoft.Win32.SaveFileDialog();
            d.FileName = fiend.Name;
            d.DefaultExt = ".json";
            d.Filter = "JSON Fiend files (.json)|*.json";
            Nullable<bool> result = d.ShowDialog();
            if(result == true)
            {
                File.WriteAllText(@d.FileName, JsonConvert.SerializeObject(fiend, Formatting.Indented));
            }
        }

        private void loadButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "JSON Fiend files (.json)|*.json";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                try
                {
                    Fiend loadFiend = JsonConvert.DeserializeObject<Fiend>(File.ReadAllText(@dlg.FileName));
                    fiend = loadFiend;
                    fiendName.Text = fiend.Name;
                    levelSlider.Value = fiend.Level;
                    rankComboBox.SelectedIndex = (int)fiend.Rank;
                    typeComboBox.SelectedIndex = (int)fiend.Type;
                    lightWeaponSelected = fiend.Weapons.Any<Weapon>(w => w.Dice == "d8");
                    mediumWeaponSelected = fiend.Weapons.Any<Weapon>(w => w.Dice == "d10");
                    heavyWeaponSelected = fiend.Weapons.Any<Weapon>(w => w.Dice == "d12");
                }
                catch (Exception x)
                {
                    System.Windows.MessageBox.Show(x.Message +
                    Environment.NewLine + Environment.NewLine +
                    x.InnerException.Message + Environment.NewLine + 
                    "====================================" + Environment.NewLine +
                    x.InnerException.StackTrace, "There was a problem loading the fiend from savefile.", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                this.update(false, false, true);
            }
        }

        private void exportButton_Click(object sender, RoutedEventArgs e)
        {
            String output = fiend.Name + " :: ";
            output += "Lv. " + fiend.Level.ToString() + " " + fiend.Rank.ToString() + " " + fiend.Type.ToString() + "\r\n";
            output += "ATK" + fiend.ATK + "\tMAG" + fiend.MAG + "\r\n";
            output += "VIT" + fiend.VIT + "\tSTM" + fiend.STM + "\r\n";
            output += "WIL" + fiend.WIL + "\tLUK" + fiend.LUK + "\r\n";
            output += "HP: " + fiend.HP + "\tMP: " + fiend.MP + "\tLP: " + fiend.LP + "\r\n";
            output += "Rewards " + (fiend.Level * (fiend.Rank == Rank.Mook ? 1 : fiend.Rank == Rank.Threat ? 2 : fiend.Rank == Rank.Boss ? 3 : 5)).ToString() + " XP\r\n\r\n";
            
            foreach (Ability a in fiend.KnownAbilities)
            {
                output += a.RenderOutput + "\r\n";
            }

            Microsoft.Win32.SaveFileDialog d = new Microsoft.Win32.SaveFileDialog();
            d.FileName = fiend.Name;
            d.DefaultExt = ".txt";
            d.Filter = "Text files (.txt)|*.txt";
            Nullable<bool> result = d.ShowDialog();
            if (result == true)
            {
                File.WriteAllText(@d.FileName, output);
            }
        }

        private void csvButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void activateFiendEngine(object sender, RoutedEventArgs e)
        {

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.update(true, true);
        }

        private void availAbilitiesGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Ability selectedAbility = AbilityHandler.Instance.Copy((Ability)availAbilitiesGrid.SelectedItem);
            if (selectedAbility.TakesActionSlot && fiend.RemainingAbilities == 0)
            {
                System.Windows.MessageBox.Show("You can't add any more abilities! :(");
            }
            else
            {
                selectedAbility.Prime(this.fiend);
                fiend.KnownAbilities.Add(selectedAbility);
                this.update();
            }
        }

        private void availAbilitiesScroll_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void resetCachesButton_Click(object sender, RoutedEventArgs e)
        {
            AbilityHandler.Instance.DestroyCache();
        }

        private void knownAbilView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            fiend.KnownAbilities.Remove((Ability)knownAbilView.SelectedItem);
            this.update();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            fiend.Name = fiendName.Text;
        }
    }

}
