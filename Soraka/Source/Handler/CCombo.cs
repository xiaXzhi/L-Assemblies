using LeagueSharp.Common;
using SorakaSharp.Source.Helper;

namespace SorakaSharp.Source.Handler
{
    internal static class CCombo
    {
        //Load Spells from the SpellHandler
        private static Spell Q
        {
            get { return CSpell.Q; }
        }

        private static Spell E
        {
            get { return CSpell.E; }
        }

        private static bool ComboValid
        {
            get
            {
                return CConfig.ConfigMenu.Item("comboUseQ").GetValue<bool>() ||
                       CConfig.ConfigMenu.Item("comboUseE").GetValue<bool>();
            }
        }

        internal static void Combo()
        {
            // Validate spell usage
            if (!ComboValid)
                return;

            var target = TargetSelector.GetTarget(Q.IsEnabledReady() ? Q.Range : E.Range,
                TargetSelector.DamageType.Magical);

            if (target != null)
            {
                //Cast Q
                if (Q.IsEnabledReady() && Q.CanCast(target))
                {
                    //Check for AOE to prevent casting on the edge..
                    if (target.CountEnemiesInRange(Q.Width) == 1)
                    {
                        Q.Cast(target);
                    }
                    else
                    {
                        Q.Cast(target, false, true);
                    }
                }

                //Cast E
                if (E.IsEnabledReady() && E.CanCast(target))
                {
                    //Check for AOE to prevent casting on the edge..
                    if (target.CountEnemiesInRange(E.Width) == 1)
                    {
                        E.Cast(target);
                    }
                    else
                    {
                        E.Cast(target, false, true);
                    }
                }
            }
        }
    }
}
