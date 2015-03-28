using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SorakaSharp.Source.Handler;

namespace SorakaSharp.Source.Manager.Heal
{
    internal static class CHeal
    {
        private static Spell W
        {
            get { return CSpell.W; }
        }

        private static int GetPriority()
        {
            switch (CConfig.ConfigMenu.Item("priority").GetValue<StringList>().SelectedIndex)
            {
                case 0: // Most AD
                    return 1;
                case 1: // Most AP
                    return 2;
                case 2: // Lowest HP
                    return 3;
            }
            return 0;
        }

        private static Obj_AI_Hero GetHealTarget() //TODO: Take care of "Don't Heal XXX", 
        {
            foreach (var ally in HeroManager.Allies.Where(ally => ally.IsValidTarget(W.Range)))
            {
                switch (GetPriority())
                {
                    case 1: //return MostAD
                    case 2: //return MostAP
                    case 3: //return LowestHP
                        break;
                }
            }
            return null;
        }

        internal static void AutoHeal()
        {
            if (!CConfig.ConfigMenu.Item("useHeal").GetValue<bool>() || !W.IsReady())
                return;

            if (GetHealTarget().HealthPercent <= CConfig.ConfigMenu.Item("percentage").GetValue<Slider>().Value)
            {
                W.Cast(GetHealTarget());
            }
        }
    }
}
