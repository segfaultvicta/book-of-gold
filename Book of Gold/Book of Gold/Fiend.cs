using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Book_of_Gold
{
    public enum Rank
    {
        Mook, Threat, Boss, Super
    }

    public enum Type
    {
        Snarler, Scrapper, Bruiser, Lurker, Shooter, Evoker, Nuker, Blighter, Defender, Healer, Leader
    }

    public class Fiend
    {
        private AbilityList knownAbilities = new AbilityList();

        public Fiend()
        {
            Weapons = new List<Weapon>();
        }

        public string Name { get; set; }

        public Rank Rank { get; set; }

        public Type Type { get; set; }

        public int Level { get; set; }

        [JsonIgnore]
        public int HP 
        { 
            get 
            {
                return (int)(50 * (decimal)VIT * (decimal)Level * RankHPMultiplier);
            } 
        }

        [JsonIgnore]
        public int MP
        {
            get
            {
                return STM - 5 + Level + (int)knownAbilities.Sum<Ability>(a => (decimal)a.BoostMaxMP);
            }
        }

        [JsonIgnore]
        public int LP
        {
            get
            {
                return LUK - 5 + Level + (int)knownAbilities.Sum<Ability>(a => (decimal)a.BoostMaxLP);
            }
        }

        [JsonIgnore]
        public int ATK
        {
            get
            {
                return 4 + ATKBase + RankAttributeBonus;
            }
        }

        [JsonIgnore]
        public int MAG
        {
            get
            {
                return 4 + MAGBase + RankAttributeBonus;
            }
        }

        [JsonIgnore]
        public int WIL
        {
            get
            {
                return 4 + WILBase + RankAttributeBonus;
            }
        }

        [JsonIgnore]
        public int VIT
        {
            get
            {
                return 4 + VITBase + RankAttributeBonus;
            }
        }

        [JsonIgnore]
        public int STM
        {
            get
            {
                return 4 + STMBase + RankAttributeBonus;
            }
        }

        [JsonIgnore]
        public int LUK
        {
            get
            {
                return 4 + LUKBase + RankAttributeBonus;
            }
        }

        public int ATKBase { get; set; }

        public int MAGBase { get; set; }

        public int WILBase { get; set; }

        public int VITBase { get; set; }

        public int STMBase { get; set; }

        public int LUKBase { get; set; }

        [JsonIgnore]
        public int AbilitySlots
        {
            get
            {
                return 2 + 2 * Level + RankAbilityBonus;
            }
        }

        [JsonIgnore]
        public int RemainingAbilities
        {
            get
            {
                int used = 0;
                foreach(Ability a in KnownAbilities)
                {
                    used += a.TakesActionSlot ? 1 : 0;
                }
                return AbilitySlots - used;
            }
        }

        [JsonIgnore]
        public int AttributeCap
        {
            get
            {
                return 9 + Level;
            }
        }

        [JsonIgnore]
        public int AttributePoints
        {
            get
            {
                return 5 + 5 * Level;
            }
        }

        [JsonIgnore]
        public int AttributesRemaining
        {
            get
            {
                return AttributePoints - (ATKBase + MAGBase + WILBase + VITBase + STMBase + LUKBase);
            }
        }

        [JsonIgnore]
        public int RankAttributeBonus
        {
            get
            {
                switch (Rank)
                {
                    case Rank.Mook:
                        return 0;
                    case Rank.Threat:
                        return 1;
                    case Rank.Boss:
                        return 2;
                    case Rank.Super:
                        return 3;
                    default:
                        return 0;
                }
            }
        }

        [JsonIgnore]
        public int RankAbilityBonus
        {
            get
            {
                switch (Rank)
                {
                    case Rank.Mook:
                        return 0;
                    case Rank.Threat:
                        return 1;
                    case Rank.Boss:
                        return 2;
                    case Rank.Super:
                        return 3;
                    default:
                        return 0;
                }
            }
        }

        [JsonIgnore]
        public decimal RankHPMultiplier
        {
            get
            {
                switch (Rank)
                {
                    case Rank.Mook:
                        return 1.0M;
                    case Rank.Threat:
                        return 1.5M;
                    case Rank.Boss:
                        return 2.0M;
                    case Rank.Super:
                        return 3.0M;
                    default:
                        return 1.0M;
                }
            }
        }

        [JsonIgnore]
        public int WeaponCount
        {
            get
            {
                return Weapons == null ? 0 : Weapons.Count;
            }
        }

        public List<Weapon> Weapons { get; set; }


        public AbilityList KnownAbilities
        {
            get
            {
                return knownAbilities;
            }
        }

        [JsonIgnore]
        public bool WeaponFree 
        {
            get
            {
                bool b = knownAbilities.Count<Ability>(a => a.AllowsMultipleWeapons) + 1 > WeaponCount;
                return b;
            }
        }
    }
}
