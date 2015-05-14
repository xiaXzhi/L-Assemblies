// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Combo.cs" company="LeagueSharp">
//   Copyright (C) 2015 LeagueSharp
//   
//   This program is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//   
//   This program is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//   
//   You should have received a copy of the GNU General Public License
//   along with this program.  If not, see <http://www.gnu.org/licenses/>.
// </copyright>
// <summary>
//   The combo.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Ashe.Source.Logic
{
    using System;
    using System.Linq;

    using LeagueSharp;
    using LeagueSharp.Common;

    /// <summary>
    /// The combo.
    /// </summary>
    internal class Combo
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes static members of the <see cref="Combo"/> class.
        /// </summary>
        static Combo()
        {
            Game.OnUpdate += Game_OnUpdate;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The game_ on update.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        private static void Game_OnUpdate(EventArgs args)
        {
            if (DoCombo)
            {
                var target = TargetSelector.GetTarget(W.Range, TargetSelector.DamageType.Physical);
                if (target.IsValidTarget())
                {
                    if (UseQ
                        && (Player.GetBuffCount("AsheQ") >= StackSlider
                            || Player.GetBuffCount("AsheQReady") >= StackSlider))
                    {
                        Q.Cast();
                    }

                    if (UseW)
                    {
                        W.Cast(target);
                    }

                    if (UseR)
                    {
                        var waypoints = target.GetWaypoints();
                        if ((Player.Distance(waypoints.Last().To3D()) - Player.Distance(target.Position)) > 400
                            && (!W.IsReady() && W.IsKillable(target)))
                        {
                            R.Cast(target);
                        }
                        else if ((!W.IsKillable(target) || !W.IsReady()) && R.IsKillable(target)
                                 && Player.Distance(target) < 2500)
                        {
                            R.Cast(target);
                        }
                    }
                }
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether do combo.
        /// </summary>
        private static bool DoCombo
        {
            get
            {
                return Ashe.Menu.Item("ComboActive").GetValue<KeyBind>().Active;
            }
        }

        // Player
        /// <summary>
        /// Gets the player.
        /// </summary>
        private static Obj_AI_Hero Player
        {
            get
            {
                return Ashe.Player;
            }
        }

        // Spells
        /// <summary>
        /// Gets the q.
        /// </summary>
        private static Spell Q
        {
            get
            {
                return Ashe.Q;
            }
        }

        /// <summary>
        /// Gets the r.
        /// </summary>
        private static Spell R
        {
            get
            {
                return Ashe.R;
            }
        }

        // Slider
        /// <summary>
        /// Gets the stack slider.
        /// </summary>
        private static int StackSlider
        {
            get
            {
                return Ashe.Menu.Item("QSlider").GetValue<Slider>().Value;
            }
        }

        // Booleans
        /// <summary>
        /// Gets a value indicating whether use q.
        /// </summary>
        private static bool UseQ
        {
            get
            {
                return Ashe.Menu.Item("UseQCombo").GetValue<bool>();
            }
        }

        /// <summary>
        /// Gets a value indicating whether use r.
        /// </summary>
        private static bool UseR
        {
            get
            {
                return Ashe.Menu.Item("UseRCombo").GetValue<bool>();
            }
        }

        /// <summary>
        /// Gets a value indicating whether use w.
        /// </summary>
        private static bool UseW
        {
            get
            {
                return Ashe.Menu.Item("UseWCombo").GetValue<bool>();
            }
        }

        /// <summary>
        /// Gets the w.
        /// </summary>
        private static Spell W
        {
            get
            {
                return Ashe.W;
            }
        }

        #endregion
    }
}