using System.Collections.Generic;
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

        private static bool IsBlocked(this Obj_AI_Hero unit)
        {
            return CConfig.ConfigMenu.SubMenu("Heal").SubMenu("DontHeal").Items.Any(entry => entry.DisplayName == unit.BaseSkinName && entry.IsActive());
        }

        private static Obj_AI_Hero GetHealTarget()
        {
            switch (CConfig.ConfigMenu.Item("priority").GetValue<StringList>().SelectedIndex)
            {
                case 0: // MostAD
                    return
                        HeroManager.Allies.Where(ally => ally.IsValidTarget(W.Range + 50, false) && !ally.IsMe)
                            .OrderByDescending(dmg => dmg.TotalAttackDamage())
                            .First();
                case 1: // MostAP
                    return
                        HeroManager.Allies.Where(ally => ally.IsValidTarget(W.Range + 50, false) && !ally.IsMe)
                            .OrderByDescending(ap => ap.TotalMagicalDamage())
                            .First();

                case 2: //LowestHP
                    return
                        HeroManager.Allies.Where(ally => ally.IsValidTarget(W.Range + 50, false) && !ally.IsMe)
                            .OrderBy(health => health.HealthPercent)
                            .First();
            }
            return null;
        }

        internal static void AutoHeal()
        {
            if (!CConfig.ConfigMenu.Item("useHeal").GetValue<bool>() || !W.IsReady())
                return;

            if (GetHealTarget().HealthPercent <= CConfig.ConfigMenu.Item("percentage").GetValue<Slider>().Value && !GetHealTarget().IsBlocked())
            {

                W.Cast(GetHealTarget());
            }
        }
    }
}
