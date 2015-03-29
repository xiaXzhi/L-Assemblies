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

        //TODO: Check if HealthTarget is in W range
        private static Obj_AI_Hero GetHealTarget()
        {
            switch (CConfig.ConfigMenu.Item("priority").GetValue<StringList>().SelectedIndex)
            {
                case 0:
                    return HeroManager.Allies.MaxOrDefault(hero => hero.TotalAttackDamage); //return MostAD
                case 1:
                    return HeroManager.Allies.MaxOrDefault(hero => hero.TotalMagicalDamage); //return MostAP
                case 2:
                    return HeroManager.Allies.MinOrDefault(hero => hero.Health); //return MostAP
            }
            return null;
        }

        internal static void AutoHeal()
        {
            if (!CConfig.ConfigMenu.Item("useHeal").GetValue<bool>() || !W.IsReady())
                return;

            if (GetHealTarget().HealthPercentage() <= CConfig.ConfigMenu.Item("percentage").GetValue<Slider>().Value &&
                !GetHealTarget().IsBlocked())
            {
                W.Cast(GetHealTarget());
            }
        }
    }
}
