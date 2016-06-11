﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace DiscordCthulhu {
    public class CRemoveGame : Command {
        public CRemoveGame () {
            command = "removegame";
            name = "Remove Game";
            help = "\"!removegame <gamename>\" - Removes you from the list of people with this game.";
        }

        public override async void ExecuteCommand ( MessageEventArgs e, List<string> arguments ) {
            base.ExecuteCommand (e, arguments);
            if (AllowExecution (arguments)) {

                if (CSetGame.games.Contains (arguments[0].ToUpper ())) {
                    Role[] roles = e.Server.FindRoles (arguments[0].ToUpper (), true).ToArray ();

                    if (roles.Length == 1) {
                        Role role = roles[0];
                        await e.User.RemoveRoles (role);
                    }
                }
            }
        }
    }
}