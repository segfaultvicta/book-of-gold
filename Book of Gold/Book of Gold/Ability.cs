using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Book_of_Gold
{
    public enum AbilityType
    {
        Action,
        Passive,
        Quick
    }

    public enum Target
    {
        Single, Row, Group,
        Debug
    }

    public enum Basis
    {
        Weapon, Physical, Magical, Restorative, Vitality, Level,
        None,
        Temporary,
        NonWeapon
    }

    public class Ability
    {
        private bool primed = false;
        private Fiend fiend;
        private string delay = "";
        private List<DamageCode> codes = new List<DamageCode>();
        private List<Tuple<string, decimal>> damageMultipliers = new List<Tuple<string, decimal>>();
        private Regex damageCode = new Regex("(?<pow>\\d+)\\s?x\\s?(?<stat>(MAG|ATK|WIL|VIT|LVL))(\\s?\\+\\s?(?<dice>\\d*d\\d+))?");
        private string description = "";
        private string cleandescription = "";

        public Ability() { }

        public string Name { get; set; }

        public AbilityType Type { get; set; }

        public string Target { get; set; }

        public string CT { get; set; }

        public string MP { get; set; }

        public string LP { get; set; }

        public string COS { get; set; }

        /// <summary>
        /// Provides a string of the form (Category) Subcategory, for display
        /// </summary>
        public string Path
        {
            get
            {
                return "(" + Category + ") " + Subcategory;
            }
        }

        public string Subcategory { get; set; }

        public string TrueSubcategory { get; set; }

        public string Category { get; set; }

        public string Delay
        {
            get
            {
                return primed ? this.RenderDelayFor(this.fiend) : this.RenderUnprimedDelay();
                //return !primed ? this.delay + "D" : this.RenderDelayFor(this.fiend);
            }
            set
            {
                this.delay = value;
            }
        }

        public int DelayModifier { get; set; }

        public Basis Basis { get; set; }

        public List<Tuple<string, decimal>> DamageMultipliers
        {
            get
            {
                return damageMultipliers;
            }
        }

        [JsonIgnore]
        public List<DamageCode> Codes 
        {
            get
            {
                if(primed && !this.ProvenNoDamageCodes)
                {
                    MatchCollection mc = damageCode.Matches(CleanDescription);
                    if (mc.Count > 0)
                    {
                        foreach (Match m in mc)
                        {
                            int power = int.Parse(m.Groups["pow"].Value);
                            string stat = m.Groups["stat"].Value;
                            string dice = m.Groups["dice"].Value;
                            this.AddDamageCode(m.ToString(), power, stat, dice);
                        }
                    }
                    else
                    {
                        ProvenNoDamageCodes = true;
                    }
                }
                return this.codes;
            }
        }

        [JsonIgnore]
        public bool ProvenNoDamageCodes { get; set; }

        public void AddDamageCode(string code, int power, string stat, string dice)
        {
            this.codes.Add(new DamageCode(code, power, stat, dice));
        }

        public void AddDamageMultiplier(string code, decimal multiplier)
        {
            this.DamageMultipliers.Add(new Tuple<string, decimal>(code, multiplier));
        }

        public string Description {
            get
            {
                return description;
            }
            set
            {
                description = value.Replace("&lt;","<").Replace("&gt;",">");
            }
        }

        public bool TakesActionSlot { get; set; }

        public bool AllowsMultipleWeapons { get; set; }

        public void Prime(Fiend f)
        {
            fiend = f;
            primed = true;
        }

        [JsonIgnore]
        public string RenderDescription
        {
            get
            {
                if (!primed)
                {
                    return "Nope";
                }
                else
                {
                    return AbilityHandler.RenderAbility(this, fiend);
                }
            }
        }

        [JsonIgnore]
        public string Costs
        {
            get
            {
                string r = "";
                r += CT != null && CT != "" ? CT + ", " : "";
                r += MP != null && MP != "" ? MP + ", " : "";
                r += LP != null && LP != "" ? LP + ", " : "";
                return r.TrimEnd(',', ' ');
            }
        }

        public Tuple<string, List<string>> RenderDamageFor(Fiend f, DamageCode d = null, Tuple<string, decimal> m = null)
        {
            return d == null ? RenderDamageFor(fiend, m)
                 : d.Basis == Basis.Physical ? d.Render(f.ATK)
                 : d.Basis == Basis.Magical ? d.Render(f.MAG)
                 : d.Basis == Basis.Restorative ? d.Render(f.WIL)
                 : d.Basis == Basis.Vitality ? d.Render(f.VIT)
                 : d.Basis == Basis.Level ? d.Render(f.Level)
                 : RenderDamageFor(fiend, m);
        }

        private Tuple<string, List<string>> RenderDamageFor(Fiend f, Tuple<string, decimal> m = null)
        {
            return this.Basis == Basis.Weapon ? this.RenderWeaponDamage(f, m)
                : new Tuple<string, List<string>>("", new List<string>());
        }

        private Tuple<string, List<string>> RenderWeaponDamage(Fiend f, Tuple<string, decimal> m = null)
        {
            if(f.WeaponCount == 0)
            {
                return new Tuple<string,List<string>>("",new List<string>());
            }
            List<string> weaponStringsList = new List<string>();
            foreach (Weapon w in fiend.Weapons)
            {
                weaponStringsList.Add(string.Format("{0} + {1}", w.Dice, (int)(w.Power * fiend.ATK * m.Item2)));
            }
            return new Tuple<string, List<string>>(m.Item1, weaponStringsList);
        }

        private string RenderDelayFor(Fiend f)
        {
            return this.Basis == Basis.Weapon ? this.RenderWeaponDelay(f)
                : this.Type == AbilityType.Quick || this.Type == AbilityType.Passive ? "" 
                : this.delay.TrimEnd('D') + "D";
        }

        private string RenderWeaponDelay(Fiend f)
        {
            if (f.WeaponCount == 0)
            {
                return "-D";
            }
            else if (f.WeaponCount < 2)
            {
                return (f.Weapons[0].Delay + this.DelayModifier).ToString() + "D";
            }
            else
            {
                string weapondelay = "[";
                foreach(Weapon p in f.Weapons)
                {
                    weapondelay += (p.Delay + this.DelayModifier).ToString() + "D, ";
                }
                weapondelay = weapondelay.TrimEnd(',', ' ');
                weapondelay += "]";
                return weapondelay;
            }
        }

        private string RenderUnprimedDelay()
        {
            if (this.Basis == Basis.Weapon)
            {
                string sign = DelayModifier > 0 ? "+" : "-";
                string mod = Math.Abs(DelayModifier).ToString();
                return DelayModifier == 0 ? "[W]D" : "[W]" + sign + mod + "D";
            }
            else
            {
                return this.Type == AbilityType.Action ? this.delay : "";
            }
        }

        public string Keywords { get; set; }

        public string CleanDescription
        {
            get
            {
                return cleandescription;
            }
            set
            {
                cleandescription = value.Replace("&lt;", "<").Replace("&gt;", ">");
            }
        }

        [JsonIgnore]
        public string RenderOutput 
        {
            get
            {
                return Type == AbilityType.Action ? String.Format("{0}\tT: {1}\t{5}\r\n{3}\t{4}\r\n{2}, {6}\r\n{7}\r\n", Name, Target, Path, Costs, Delay, COS, Keywords, RenderDescription) :
                    Type == AbilityType.Quick ? String.Format("{0}\t{2}, {6}\t{3}\r\n{7}\r\n", Name, Target, Path, Costs, Delay, COS, Keywords, RenderDescription) :
                    String.Format("{0}\t{2}, {6}\r\n{7}\r\n", Name, Target, Path, Costs, Delay, COS, Keywords, RenderDescription); 
            }
        }

        public int BoostMaxMP { get; set; }

        public int BoostMaxLP { get; set; }
    }
}
