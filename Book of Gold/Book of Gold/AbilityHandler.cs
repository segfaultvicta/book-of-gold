using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Book_of_Gold
{
    public enum NodeTextResult
    {
        Accept, Ignore, Reject, Striptags
    }

    public sealed class AbilityHandler
    {
        private static readonly AbilityHandler instance = new AbilityHandler();

        private static List<Ability> cache;

        private static Regex striptags = new Regex("<.*>");
        private static Regex stripNewlines = new Regex("\n");
        private static Regex findCategoryName = new Regex(".?-(?<name>[\\w\\s]+)-.?");

        private static Regex actionMatcher = new Regex("Name: (?<name>[\\w\\s':,!\\-]+)Category: (?<category>[\\w\\s]+)\\s?Type: (?<type>\\w+)\\s?(?<keywords>.+)Target: (?<target>.+?)\\s?(CT(?<ct>\\d+))? (?<delay>[?\\[\\]W\\+\\-0-9]+)D (MP: (?<mp>[0-9?]+))?\\s+(?<description>.*)CoS: (?<cos>[0-9]+)");
        private static Regex quickMatcher = new Regex("Name: (?<name>[\\w\\s':,!\\-]+)Category: (?<category>.+)?Type: Quick (?<keywords>.*?) [L|M]P: (?<lp>\\d+)\\s+(?<description>.*)");
        private static Regex passiveMatcher = new Regex("Name: (?<name>[\\w\\s':,\\-!]+)Category: (?<category>.+)?Type: Passive\\s+(?<description>.*)");

        private static Regex damageCode = new Regex("(?<pow>\\d+)\\s?x\\s?(?<stat>(MAG|ATK|WIL|VIT|LVL))(\\s?\\+\\s?(?<dice>\\d*d\\d+))?");

        private static Regex dcReplacer = new Regex("((\\d+ x )?(MAG|ATK|WIL|VIT|LVL)( \\+ \\d*d\\d*)?)");
        private static Regex weapReplacer = new Regex("(?<whole>(x)?(?<mult>\\d?\\.?\\d?)?\\s?(ranged )?weapon damage( with an x(?<mult2>.*)power multiplier)?)");

        private static List<string> elementWhitelist;
        private static List<string> elementStripList;

        private static List<string> sharedLists;

        private AbilityHandler() 
        {
            elementWhitelist = new List<string>();
            elementStripList = new List<string>();
            cache = new List<Ability>();
            sharedLists = new List<string>();

            elementWhitelist.Add("br");

            elementStripList.Add("strong");
            elementStripList.Add("em");
            elementStripList.Add("span");

            sharedLists.Add("General");
            sharedLists.Add("Threat");
            sharedLists.Add("Boss");
            sharedLists.Add("Super");

            // look for the cache file in system and load it if we find it
            if (File.Exists("monster_abilities.json"))
            {
                cache = JsonConvert.DeserializeObject<List<Ability>>(File.ReadAllText(@"monster_abilities.json"));
            }
        }

        public void DestroyCache()
        {
            File.Delete("monster_abilities.json");
            cache = new List<Ability>();
        }

        public static AbilityHandler Instance
        {
            get
            {
                return instance;
            }
        }

        internal AbilityList Fetch(string p)
        {
            // check the cache for that string. if cache hits, hurrah!
            AbilityList r = new AbilityList();

            string uri;
            bool fullPage = false;

            if(sharedLists.Contains(p))
            {
                uri = "http://storagebin.wikispaces.com/S2-MonsterAbilities";
            }
            else
            {
                fullPage = true;
                uri = "http://storagebin.wikispaces.com/S2-" + p;
            }

            var q = from ability in cache
                    where ability.Category == p
                    select ability;
            if (q.Count() > 0)
            {
                // cache hit!
                foreach (Ability a in q)
                {
                    r.Add(a);
                }
            }
            else
            {
                // if cache misses, need to fetch and parse ability information from wikispaces
                // and then rebuild the cache for that string.
                HtmlWeb web = new HtmlWeb();
                HtmlDocument rulesdoc = web.Load(uri);
                HtmlNode contentView = rulesdoc.GetElementbyId("content_view");
                HtmlNodeCollection category_headers = contentView.SelectNodes(".//h3");
                foreach (HtmlNode h3 in category_headers)
                {
                    if (fullPage)
                    {
                        r.Add(Cache(ParseAbility(GetAbilityText(h3), p)));
                    }
                    else
                    {
                        Ability a = ParseAbility(GetAbilityText(h3), p);
                        a.Category = a.TrueSubcategory;
                        Cache(a);
                        if (a.Category == p)
                        {
                            r.Add(a);
                        }
                    }
                }
                File.WriteAllText(@"monster_abilities.json", JsonConvert.SerializeObject(cache, Formatting.Indented));
            }

            return r;
        }

        private Ability Cache(Ability ability)
        {
            cache.Add(ability);
            return ability;
        }

        // this is almost definitely suboptimal, deal with it or submit a pull request B)
        private Ability ParseAbility(string text, string category)
        {
            Ability a = new Ability();
            a.TakesActionSlot = true;
            a.Category = category;
            // actionMatcher    /Name: (?<name>[\w\s':,!\-]+)Category: (?<category>[\w\s]+)\s?Type: (?<type>\w+)\s?(?<keywords>.+)Target: (?<target>.+?)\s?(CT(?<ct>\d+))? (?<delay>[?\[\]W\+\-0-9]+)D (MP: (?<mp>[0-9?]+))?\s+(?<description>.*)CoS: (?<cos>[0-9]+)/
            // quickMatcher     /Name: (?<name>[\w\s':,!\-]+)Category: (?<category>.+)?Type: Quick (?<keywords>.*?) LP: (?<lp>\d+)\s+(?<description>.*)/
            // passiveMatcher   /Name: (?<name>[\w\s':,!\-]+)Category: (?<category>.+)?Type: Passive\s+(?<description>.*)/
            Match m = actionMatcher.Match(text);
            if (m.Success)
            {
                a.Type = AbilityType.Action;
                a.Name = m.Groups["name"].Value.Trim();
                a.Subcategory = m.Groups["category"].Value.Trim();
                a.COS = "CoS: " + m.Groups["cos"].Value;
                a.Keywords = m.Groups["keywords"].Value.Trim();
                a.Description = "(" + m.Groups["keywords"].Value.Trim() + ")" + Environment.NewLine + m.Groups["description"].Value + a.COS;
                a.CleanDescription = m.Groups["description"].Value.Trim();
                a.Target = m.Groups["target"].Value;
                a.MP = m.Groups["mp"].Value != "" ? m.Groups["mp"].Value + " MP" : "";
                a.CT = m.Groups["ct"].Value != "" ? m.Groups["ct"].Value + " CT" : "";
                string delay = m.Groups["delay"].Value;
                if (delay.Contains('W'))
                {
                    a.Basis = Basis.Weapon;
                    // get delay modifier
                    if (delay == "[W]")
                    {
                        a.DelayModifier = 0;
                    }
                    else
                    {
                        string delaymodnum = delay.Substring(4);
                        string delaymodsign = delay.Substring(3, 1);
                        int sign = delaymodsign == "+" ? 1 : -1;
                        a.DelayModifier = int.Parse(delaymodnum) * sign;
                    }
                }
                else
                {
                    a.Delay = delay;
                    a.Basis = Basis.NonWeapon;
                }
                if(a.Basis == Basis.Weapon)
                {
                    // ability will have at least one damage multiplier, if anything
                    foreach(Match wd_match in weapReplacer.Matches(a.CleanDescription))
                    {
                        string mult = "";
                        if (wd_match.Groups["mult"].Length > 0)
                        {
                            mult = wd_match.Groups["mult"].Value;
                        }
                        else if (wd_match.Groups["mult2"].Length > 0)
                        {
                            mult = wd_match.Groups["mult2"].Value;
                        }
                        decimal temp;

                        if(a.Name == "Crackshot")
                        {
                            int four = 2 + 2;
                        }

                        if (Decimal.TryParse(mult, out temp))
                        {
                            a.AddDamageMultiplier(wd_match.ToString(), temp);
                        }
                        else
                        {
                            a.AddDamageMultiplier(wd_match.ToString(), 1.0M);
                        }
                    }
                }

                // ability might have damage codes that need to be parsed out
                // (?<pow>\d+)\s?x\s?(?<stat>\w{3})(\s?\+\s?(?<dice>\d*d\d+))?

                foreach (Match dc_match in damageCode.Matches(a.CleanDescription))
                {
                    int power = int.Parse(dc_match.Groups["pow"].Value);
                    string stat = dc_match.Groups["stat"].Value;
                    string dice = dc_match.Groups["dice"].Value;
                    a.AddDamageCode(dc_match.ToString(), power, stat, dice);
                }
  
                // SPECIAL CASES
                if (a.Subcategory == "General Abilities" || a.Subcategory == "Boss" || a.Subcategory == "Threat" || a.Subcategory == "Super")
                {
                    a.TrueSubcategory = a.Subcategory;
                    a.Subcategory = "Ability";
                }

                if (a.Subcategory == "Ambush Skills")
                {
                    a.CleanDescription += " Remember to take the Hidden passive!";
                }
            } 
            else
            {
                m = quickMatcher.Match(text);
                if (m.Success)
                {
                    a.Type = AbilityType.Quick;
                    a.Name = m.Groups["name"].Value.Trim();
                    //a.Subcategory =  "Quick (" + a.Category + ")";
                    a.TrueSubcategory = m.Groups["category"].Value.Trim();
                    a.Subcategory = "Quick";
                    a.LP = m.Groups["lp"].Value != "" ? m.Groups["lp"].Value + " LP" : "";
                    a.Keywords = m.Groups["keywords"].Value.Trim();
                    a.Description = "(" + m.Groups["keywords"].Value + ")" + Environment.NewLine + m.Groups["description"].Value;
                    a.CleanDescription = m.Groups["description"].Value.Trim();
                    a.Basis = Basis.None;

                    // SPECIAL CASES
                    if (a.Name == "Void Flare")
                    {
                        a.LP = "";
                        a.MP = "2 MP";
                    }
                }
                else
                {
                    m = passiveMatcher.Match(text);
                    if(m.Success)
                    {
                        a.Type = AbilityType.Passive;
                        a.Name = m.Groups["name"].Value.Trim();
                        a.TrueSubcategory = m.Groups["category"].Value.Trim();
                        a.Subcategory = "Passive";
                        a.Description = m.Groups["description"].Value;
                        a.CleanDescription = m.Groups["description"].Value.Trim();
                        a.Basis = Basis.None;

                        // SPECIAL CASES
                        if (a.Description.Contains("Snarler Special"))
                        {
                            a.TakesActionSlot = false;
                        }

                        if (a.Name == "Second Weapon")
                        {
                            a.AllowsMultipleWeapons = true;
                        }

                        if (a.Subcategory == "General Abilities")
                        {
                            a.Subcategory = "Passives";
                        }

                        if (a.Name == "Hidden")
                        {
                            a.TakesActionSlot = false;
                        }

                        if (a.Name == "Deep Mana")
                        {
                            a.BoostMaxMP = 5;
                        }

                        if (a.Name == "Deep Luck")
                        {
                            a.BoostMaxLP = 5;
                        }
                    }
                    else
                    {
                        // regexes failed to parse the ability, somehow indicate this abject failure
                        a.Name = "!!! PARSE FAILURE !!!";
                        a.Description = text;
                    }
                }
            }
            if(a.TrueSubcategory == "General Abilities")
            {
                a.TrueSubcategory = "General";
            }
            return a;
        }

        private string GetAbilityText(HtmlNode h3)
        {
            string r = "Name: " + striptags.Replace(h3.InnerText, "") + "\n";
            Match m = findCategoryName.Match(h3.InnerHtml);
            r += "Category: " + m.Groups["name"].Value + "\n";

            HtmlNode node = h3.NextSibling;
            bool valid = true;
            while (valid)
            {
                switch (NodeTextAccept(node))
                {
                    case NodeTextResult.Accept:
                        r += node.InnerText;
                        break;
                    case NodeTextResult.Reject:
                        valid = false;
                        break;
                    case NodeTextResult.Striptags:
                        r += striptags.Replace(node.InnerText, "");
                        break;
                    default:
                        break;
                }
                if (valid)
                {
                    node = node.NextSibling;
                }
            }
            return r.Replace('\n', ' ');
        }

        private NodeTextResult NodeTextAccept(HtmlNode node)
        {
            if(node == null)
            {
                return NodeTextResult.Reject;
            }
            if (node.NodeType == HtmlNodeType.Text)
            {
                return NodeTextResult.Accept;
            }
            else if (elementWhitelist.Contains(node.Name))
            {
                return NodeTextResult.Ignore;
            }
            else if (elementStripList.Contains(node.Name))
            {
                return NodeTextResult.Striptags;
            }
            else
            {
                return NodeTextResult.Reject;
            }
        }

        public Ability Copy(Ability a)
        {
            Ability b;
            MemoryStream ms = new MemoryStream();
            using (BsonWriter w = new BsonWriter(ms))
            {
                JsonSerializer s = new JsonSerializer();
                s.Serialize(w, a);
            }
            string sData = Convert.ToBase64String(ms.ToArray());
            byte[] bData = Convert.FromBase64String(sData);
            ms = new MemoryStream(bData);
            using (BsonReader r = new BsonReader(ms))
            {
                JsonSerializer s = new JsonSerializer();
                b = s.Deserialize<Ability>(r);
            }
            ms.Close();
            return b;
        }

        internal static string RenderAbility(Ability ability, Fiend fiend)
        {
            
            string render = ability.CleanDescription;

            if (ability.Basis == Basis.Weapon)
            {
                if (fiend.WeaponCount > 0)
                {
                    List<Tuple<string, decimal>> reversedMultipliers = new List<Tuple<string, decimal>>(ability.DamageMultipliers);
                    reversedMultipliers.Reverse();
   
                    foreach (Tuple<string, decimal> m in reversedMultipliers)
                    {
                        Tuple<string, List<string>> d = ability.RenderDamageFor(fiend, null, m);
                        string weapondamage = "";
                        foreach (string s in d.Item2)
                        {
                            weapondamage += s + ", ";
                        }
                        weapondamage = " " + (d.Item2.Count == 1 ? weapondamage.TrimEnd(' ', ',') : " [" + weapondamage.TrimEnd(' ', ',') + "]");
                        render = render.Replace(d.Item1, weapondamage);
                    }
                }
            }

            List<DamageCode> codes = ability.Codes;
            foreach (DamageCode code in codes)
            {
                Tuple<string, List<string>> d = ability.RenderDamageFor(fiend, code);
                // shit is a lot more straightforward here:
                string original = d.Item1;
                string replacement = d.Item2[0];
                render = render.Replace(original, replacement);
            }

            return render; 
        }
    }
}
