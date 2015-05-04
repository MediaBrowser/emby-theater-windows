// This file is a part of MPDN Extensions.
// https://github.com/zachsaw/MPDN_Extensions
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library.
// 
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using Mpdn.Config;

namespace Mpdn.PlayerExtensions
{
    public abstract class PlayerExtension : PlayerExtension<NoSettings> { }

    public abstract class PlayerExtension<TSettings> : PlayerExtension<TSettings, ScriptConfigDialog<TSettings>>
        where TSettings : class, new()
    { }

    public abstract class PlayerExtension<TSettings, TDialog> : ExtensionUi<Config.Internal.PlayerExtensions, TSettings, TDialog>, IPlayerExtension
        where TSettings : class, new()
        where TDialog : ScriptConfigDialog<TSettings>, new()
    {
        public abstract IList<Verb> Verbs { get; }

        #region Implementation

        public override void Initialize()
        {
            base.Initialize();

            PlayerControl.KeyDown += PlayerKeyDown;
            LoadVerbs();
        }

        public override void Destroy()
        {
            PlayerControl.KeyDown -= PlayerKeyDown;

            base.Destroy();
        }

        private readonly IDictionary<Keys, Action> m_Actions = new Dictionary<Keys, Action>();

        protected void LoadVerbs()
        {
            foreach (var verb in Verbs)
            {
                var shortcut = DecodeKeyString(verb.ShortcutDisplayStr);
                m_Actions.Remove(shortcut); //Prevent duplicates FIFO.
                m_Actions.Add(shortcut, verb.Action);
            }
        }

        private void PlayerKeyDown(object sender, PlayerControlEventArgs<KeyEventArgs> e)
        {
            Action action;
            if (m_Actions.TryGetValue(e.InputArgs.KeyData, out action))
            {
                action();
            }
        }

        protected static bool TryDecodeKeyString(String keyString, out Keys keys)
        {
            var keyWords = Regex.Split(keyString, @"\W+");
            keyString = String.Join(", ", keyWords.Select(DecodeKeyWord).ToArray());

            return (Enum.TryParse(keyString, true, out keys));
        }

        private static Keys DecodeKeyString(String keyString)
        {
            Keys keys;
            if (TryDecodeKeyString(keyString, out keys))
                return keys;
            else
                throw new ArgumentException("Can't convert string to keys.");
        }

        private static String DecodeKeyWord(String keyWord)
        {
            switch (keyWord.ToLower())
            {
                case "ctrl":
                    return "Control";
                case "0":
                    return "D0";
                case "1":
                    return "D1";
                case "2":
                    return "D2";
                case "3":
                    return "D3";
                case "4":
                    return "D4";
                case "5":
                    return "D5";
                case "6":
                    return "D6";
                case "7":
                    return "D7";
                case "8":
                    return "D8";
                case "9":
                    return "D9";
                default:
                    return keyWord;
            }
        }

        #endregion
    }
}
