using System;
using LeagueSharp;
using LeagueSharp.Common;
using SorakaSharp.Source.Handler;
using SorakaSharp.Source.Manager.Heal;
using SorakaSharp.Source.Manager.Heal.Ultimate;

namespace SorakaSharp.Source
{
    internal static class Program
    {
        // Initialize Champion
        internal const string ChampionName = "Soraka";
        private static Obj_AI_Hero Player { get { return ObjectManager.Player; } }


        private static void Main(string[] args)
        {
            // Add OnGameLoad
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            // Validate Champion
            if (Player.ChampionName != ChampionName)
                return;

            //Load Handlers
            CConfig.Initialize();
            CSpell.Initialize();

            //Listen to Additional Events
            AntiGapcloser.OnEnemyGapcloser += AntiGapcloser_OnEnemyGapcloser;
            Interrupter2.OnInterruptableTarget += Interrupter2_OnInterruptableTarget;
            Game.OnUpdate += Game_OnGameUpdate;
        }

        private static void AntiGapcloser_OnEnemyGapcloser(ActiveGapcloser gapcloser)
        {
            if (!CConfig.ConfigMenu.Item("AntiGapCloser").GetValue<bool>() || !CSpell.E.CanCast(gapcloser.Sender))
                return;

            CSpell.E.Cast(gapcloser.Sender);
        }

        private static void Interrupter2_OnInterruptableTarget(Obj_AI_Hero sender, Interrupter2.InterruptableTargetEventArgs args)
        {
            if (!CConfig.ConfigMenu.Item("InterruptSpells").GetValue<bool>() || !CSpell.E.CanCast(sender) || args.DangerLevel != Interrupter2.DangerLevel.High)
                return;

            CSpell.E.Cast(sender);
        }

        private static void Game_OnGameUpdate(EventArgs args)
        {
            CHeal.AutoHeal();
            CUltimate.SmartSave();
            CUltimate.TeamfightUltimate();

            if (CConfig.ConfigMenu.Item("ComboActive").GetValue<KeyBind>().Active)
            {
                CCombo.Combo();
            }
        }
    }
}
