using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Book_of_Gold
{
    public class DamageCode
    {
        private int power;
        private string stat;
        private string dice;
        private Basis basis;
        private string originalCode;

        public DamageCode(string code, int power, string stat, string dice)
        {
            this.power = power;
            this.stat = stat;
            this.basis =    stat == "ATK" ? Basis.Physical : 
                            stat == "MAG" ? Basis.Magical : 
                            stat == "WIL" ? Basis.Restorative : 
                            stat == "VIT" ? Basis.Vitality :
                            stat == "LVL" ? Basis.Level :
                            Basis.None;
            this.dice = dice;
            this.originalCode = code;
        }

        public string Original
        {
            get
            {
                return originalCode;
            }
        }

        public Basis Basis
        {
            get
            {
                return basis;
            }
        }

        public override string ToString()
        {
            return power.ToString() + " x " + stat + " + " + dice;
        }

        internal Tuple<string, List<string>> Render(int stat)
        {
            int dmg = power * stat;
            List<string> placeholder = new List<string>();
            placeholder.Add(dice != "" ? dice + " + " + dmg.ToString() : dmg.ToString());
            return new Tuple<string, List<string>>(originalCode, placeholder);
        }
    }
}
