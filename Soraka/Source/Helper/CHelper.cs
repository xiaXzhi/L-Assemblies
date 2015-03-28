using LeagueSharp.Common;
using SorakaSharp.Source.Handler;

namespace SorakaSharp.Source.Helper
{
    internal static class CHelper
    {
        internal static bool IsEnabled(this Spell spell)
        {
            return CConfig.ConfigMenu.Item(string.Concat("Use", spell.Slot.ToString())).GetValue<bool>();
        }

        internal static bool IsEnabledReady(this Spell spell)
        {
            return spell.IsEnabled() && spell.IsReady();
        }
    }
}
