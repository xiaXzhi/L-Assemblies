using System;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using LeagueSharp.Common.Data;

namespace Twitch
{
    public class Program
    {
        private static Menu _config;
        private static Orbwalking.Orbwalker _orbwalker;

        private static Spell _w;
        private static Spell _e;

        private static Obj_AI_Hero _player { get { return ObjectManager.Player; } }

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        private static float GetDamage(Obj_AI_Hero target)
        {
            return _e.GetDamage(target);
        }

        private static void Game_OnGameLoad(EventArgs args)
        {
            //Verify Champion
            if (_player.ChampionName != "Twitch")
                return;

            //Spells
            _w = new Spell(SpellSlot.W, 950);
            _w.SetSkillshot(0.25f, 120f, 1400f, false, SkillshotType.SkillshotCircle);
            _e = new Spell(SpellSlot.E, 1200);

            //Menu instance
            _config = new Menu("Twitch", "Twitch", true);

            //Orbwalker
            _orbwalker = new Orbwalking.Orbwalker(_config.SubMenu("Orbwalking"));

            //Targetsleector
            var targetSelectorMenu = new Menu("Target Selector", "Target Selector");
            TargetSelector.AddToMenu(targetSelectorMenu);
            _config.AddSubMenu(targetSelectorMenu);

            //Combo
            _config.SubMenu("Combo").AddItem(new MenuItem("UseWCombo", "Use W").SetValue(true));

            //Misc
            _config.SubMenu("Misc").AddItem(new MenuItem("EKillsteal", "Killsteal with E").SetValue(true));
            _config.SubMenu("Misc").AddItem(new MenuItem("EDamage", "E damage on healthbar").SetValue(new Circle(true, Color.Green)));
            _config.SubMenu("Misc").AddItem(new MenuItem("Emobs", "Kill big mobs with E").SetValue(true));

            //Attach to root
            _config.AddToMainMenu();

            // Enable E damage indicators
            CustomDamageIndicator.Initialize(GetDamage);

            //Listen to events
            Game.OnUpdate += Game_OnUpdate;
            Drawing.OnDraw += Drawing_OnDraw;
        }

        private static void Drawing_OnDraw(EventArgs args)
        {
            // E damage on healthbar
            CustomDamageIndicator.DrawingColor = _config.Item("EDamage").GetValue<Circle>().Color;
            CustomDamageIndicator.Enabled = _config.Item("EDamage").GetValue<Circle>().Active;

        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (_e.IsReady())
            {
                //Killsteal with E
                if (_config.Item("EKillsteal").GetValue<bool>())
                {

                    foreach (
                        var enemy in
                            ObjectManager.Get<Obj_AI_Hero>()
                                .Where(enemy => enemy.IsValidTarget(_e.Range) && _e.IsKillable(enemy))
                        )
                    {
                        _e.Cast();
                    }
                }

                //Kill large monsters
                if (_config.Item("Emobs").GetValue<bool>() && _orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo )
                {
                    var minions = MinionManager.GetMinions(_e.Range, MinionTypes.All, MinionTeam.NotAlly);
                    foreach (
                        var m in
                            minions.Where(
                                m => (m.BaseSkinName.Contains("MinionSiege") || m.BaseSkinName.Contains("Dragon") ||
                                      m.BaseSkinName.Contains("Baron")) && _e.IsKillable(m)))
                    {
                        _e.Cast();
                    }
                }
            }

            //Combo/Items
            if (_orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
            {
                var target = TargetSelector.GetTarget(_w.Range, TargetSelector.DamageType.Physical);

                //Use W
                if (_config.Item("UseWCombo").GetValue<bool>())
                {
                    if (target.IsValidTarget(_w.Range) && _w.CanCast(target))
                    {
                        _w.Cast(target);
                    }
                }

                //Use Botrk
                if (target != null && target.Type == _player.Type &&
                    target.ServerPosition.Distance(_player.ServerPosition) < 450)
                {
                    var hasCutGlass = Items.HasItem(3144);
                    var hasBotrk = Items.HasItem(3153);

                    if (hasBotrk || hasCutGlass)
                    {
                        var itemId = hasCutGlass ? 3144 : 3153;
                        var damage = _player.GetItemDamage(target, Damage.DamageItems.Botrk);
                        if (hasCutGlass || _player.Health + damage < _player.MaxHealth)
                            Items.UseItem(itemId, target);
                    }
                }

                //Use Youmus
                if (target != null && target.Type == _player.Type && Orbwalking.InAutoAttackRange(target))
                {
                    Items.UseItem(3142);
                }
            }

            //Auto buy blue trinket
            if (_player.Level >= 6 && _player.InShop() && !(Items.HasItem(3342) || Items.HasItem(3363)))
            {
                _player.BuyItem(ItemId.Scrying_Orb_Trinket);
            }
        }
    }
}
