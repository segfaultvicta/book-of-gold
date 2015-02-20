using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Book_of_Gold
{
    public class Weapon
    {
        public Weapon(string dice, int power, int delay)
        {
            Delay = delay;
            Dice = dice;
            Power = power;
        }

        public int Delay { get; set; }

        public string Dice { get; set; }

        public int Power { get; set; }
    }
}
