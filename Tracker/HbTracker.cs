#region

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Tracker.Properties;
using Color = SharpDX.Color;
using Rectangle = SharpDX.Rectangle;

#endregion

namespace Tracker
{
    /// <summary>
    ///     Health bar tracker tracks allies and enemies spells and summoners cooldowns.
    /// </summary>
    public static class HbTracker
    {
        public static Render.Sprite CdFrame;

        private static readonly Dictionary<string, Render.Sprite> SummonerTextures =
            new Dictionary<string, Render.Sprite>(StringComparer.InvariantCultureIgnoreCase);

        public static Render.Line ReadyLine;
        public static Render.Text Text;
        public static SpellSlot[] SummonerSpellSlots = { SpellSlot.Summoner1, SpellSlot.Summoner2 };
        public static SpellSlot[] SpellSlots = { SpellSlot.Q, SpellSlot.W, SpellSlot.E, SpellSlot.R };
        public static Menu Config;
        public static Bitmap CurrentHud = Resources.hud;

        public static string[] SummonersNames =
        {
            "SummonerBarrier", "SummonerBoost", "SummonerDot", "SummonerExhaust",
            "SummonerFlash", "SummonerHaste", "SummonerHeal", "SummonerOdinGarrison", "SummonerSmite",
            "SummonerTeleport", "s5_summonersmiteduel", "s5_summonersmiteplayerganker", "s5_summonersmitequick",
            "itemsmiteaoe"
        };

        static HbTracker()
        {
            try
            {
                foreach (var sName in
                    SummonersNames)
                {
                    SummonerTextures.Add(sName, GetSummonerTexture(sName));
                }

                CdFrame = new Render.Sprite(CurrentHud, Vector2.Zero);
                ReadyLine = new Render.Line(Vector2.Zero, Vector2.Zero, 2, Color.Black);
                Text = new Render.Text("", Vector2.Zero, 13, Color.Black);
            }
            catch (Exception e)
            {
                Console.WriteLine(@"/ff can't load the textures: " + e);
            }

            Drawing.OnDraw += Drawing_OnEndScene;
        }

        public static void AttachToMenu(Menu menu)
        {
            Config = menu.AddSubMenu(new Menu("CD Tracker", "CD Tracker"));
            Config.AddItem(new MenuItem("TrackAllies", "Track allies").SetValue(true));
            Config.AddItem(new MenuItem("TrackEnemies", "Track enemies").SetValue(true));
            Config.AddItem(new MenuItem("XmasHud", "Use Christmas Theme").SetValue(true));
            CurrentHud = Config.Item("XmasHud").IsActive() ? Resources.xmas_hud : Resources.hud;

            if (Config.Item("XmasHud").IsActive())
            {
                CdFrame = new Render.Sprite(CurrentHud, Vector2.Zero);
            }

            Config.Item("XmasHud").ValueChanged += (sender, e) =>
            {
                CurrentHud = e.GetNewValue<bool>() ? Resources.xmas_hud : Resources.hud;
                CdFrame = new Render.Sprite(CurrentHud, Vector2.Zero);
            };
        }

        private static Render.Sprite GetSummonerTexture(string name)
        {
            Bitmap bitmap;
            switch (name)
            {
                case "SummonerOdinGarrison":
                    bitmap = Resources.SummonerOdinGarrison;
                    break;
                case "SummonerBoost":
                    bitmap = Resources.SummonerBoost;
                    break;
                case "SummonerTeleport":
                    bitmap = Resources.SummonerTeleport;
                    break;
                case "SummonerHeal":
                    bitmap = Resources.SummonerHeal;
                    break;
                case "SummonerExhaust":
                    bitmap = Resources.SummonerExhaust;
                    break;
                case "SummonerSmite":
                    bitmap = Resources.SummonerSmite;
                    break;
                case "SummonerDot":
                    bitmap = Resources.SummonerDot;
                    break;
                case "SummonerHaste":
                    bitmap = Resources.SummonerHaste;
                    break;
                case "SummonerFlash":
                    bitmap = Resources.SummonerFlash;
                    break;
                case "s5_summonersmiteduel":
                    bitmap = Resources.s5_summonersmiteduel;
                    break;
                case "s5_summonersmiteplayerganker":
                    bitmap = Resources.s5_summonersmiteplayerganker;
                    break;
                case "s5_summonersmitequick":
                    bitmap = Resources.s5_summonersmitequick;
                    break;
                case "itemsmiteaoe":
                    bitmap = Resources.itemsmiteaoe;
                    break;
                default:
                    bitmap = Resources.SummonerBarrier;
                    break;
            }

            return new Render.Sprite(bitmap, Vector2.Zero);
        }

        private static void Drawing_OnEndScene(EventArgs args)
        {
            try
            {
                foreach (var hero in
                    HeroManager.AllHeroes.Where(
                        hero =>
                            hero.IsValid && !hero.IsMe && hero.IsHPBarRendered &&
                            (hero.IsEnemy && Config.Item("TrackEnemies").GetValue<bool>() ||
                             hero.IsAlly && Config.Item("TrackAllies").GetValue<bool>())))
                {
                    var pos = GetHPBarPositionWithOffset(hero);
                    var X = (int) pos.X;
                    var Y = (int) pos.Y;

                    var k = 0;

                    foreach (var sSlot in SummonerSpellSlots)
                    {
                        var spell = hero.Spellbook.GetSpell(sSlot);
                        var texture = SummonerTextures.ContainsKey(spell.Name) ? SummonerTextures[spell.Name] : SummonerTextures["SummonerBarrier"];
                        var t = spell.CooldownExpires - Game.Time;
                        var percent = Math.Abs(spell.Cooldown) > float.Epsilon ? t / spell.Cooldown : 1f;
                        var n = t > 0 ? (int) (19 * (1f - percent)) : 19;
                        var ts = TimeSpan.FromSeconds((int) t);
                        var s = t > 60 ? string.Format("{0}:{1:D2}", ts.Minutes, ts.Seconds) : string.Format("{0:0}", t);

                        if (t > 0)
                        {
                            Text.text = s;
                            Text.X = X - 5 - s.Length * 5;
                            Text.Y = Y + 1 + 13 * k;
                            Text.Color = Color.White;
                            Text.OnEndScene();
                        }

                        texture.X = X + 3 + (Config.Item("XmasHud").IsActive() ? 1 : 0);
                        texture.Y = Y + 1 + 13 * k;
                        var crop = Config.Item("XmasHud").IsActive() ? 14 : 12;
                        texture.Crop(new Rectangle(0, 12 * n, crop, 12));
                        texture.OnEndScene();
                        k++;
                    }

                    var hudOffset = GetHudOffset();
                    CdFrame.X = X + (int) hudOffset.X;
                    CdFrame.Y = Y + (int) hudOffset.Y;
                    CdFrame.OnEndScene();

                    var miscOffset = GetMiscOffset();
                    var startX = X + 19 + (int) miscOffset.X;
                    var startY = Y + 20 + (int) miscOffset.Y;

                    foreach (var slot in SpellSlots)
                    {
                        var spell = hero.Spellbook.GetSpell(slot);
                        var t = spell.CooldownExpires - Game.Time;
                        var percent = t > 0 && Math.Abs(spell.Cooldown) > float.Epsilon ? 1f - t / spell.Cooldown : 1f;

                        if (t > 0 && t < 100)
                        {
                            var s = string.Format(t < 1f ? "{0:0.0}" : "{0:0}", t);
                            Text.text = s;
                            Text.X = startX + (24 - s.Length * 4) / 2;
                            Text.Y = startY + 6;
                            Text.Color = Color.White;
                            Text.OnEndScene();
                        }

                        var darkColor = t > 0 ? new ColorBGRA(168, 98, 0, 255) : new ColorBGRA(0, 130, 15, 255);
                        var lightColor = t > 0 ? new ColorBGRA(235, 137, 0, 255) : new ColorBGRA(0, 168, 25, 255);

                        if (hero.Spellbook.CanUseSpell(slot) != SpellState.NotLearned)
                        {
                            for (var i = 0; i < 2 + (Config.Item("XmasHud").IsActive() ? 1 : 0); i++)
                            {
                                ReadyLine.Start = new Vector2(startX, startY + i * 2);
                                ReadyLine.End = new Vector2(startX + percent * 23, startY + i * 2);
                                ReadyLine.Color = i == 0 ? lightColor : darkColor;
                                ReadyLine.OnEndScene();
                            }
                        }

                        startX = startX + 27;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(@"/ff can't draw sprites: " + e);
            }
        }

        private static Vector2 GetHudOffset()
        {
            return Config.Item("XmasHud").IsActive() ? new Vector2(-10, -11) : Vector2.Zero;
        }

        public static Vector2 GetMiscOffset()
        {
            return Config.Item("XmasHud").IsActive() ? new Vector2(-2, -1) : Vector2.Zero;
        }

        private static Vector2 GetHPBarPositionWithOffset(Obj_AI_Base unit)
        {
            if (unit == null || !unit.IsValid)
            {
                return Vector2.Zero;
            }

            var offset = unit.IsAlly ? new Vector2(-9, 14) : new Vector2(-9, 17);
            var hpPos = unit.HPBarPosition;
            return new Vector2(hpPos.X + offset.X, hpPos.Y + offset.Y);
        }
    }
}