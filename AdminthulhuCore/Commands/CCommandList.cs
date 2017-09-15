﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Adminthulhu {
    public class CCommandList : Command {

        public CCommandList() {
            command = "clist";
            shortHelp = "Show command list.";
            catagory = Category.Utility;

            availableInDM = true;

            AddOverload (typeof (string), "Reveals a full list of all commands.");
            AddOverload (typeof (string), "Reveals a list of commands in a given command array.");
            AddOverload (typeof (string), "Reveals a list of commands in a given command set.");
        }

        public Task<Result> Execute(SocketUserMessage e) {
            return Execute (e, Program.commands);
        }

        public Task<Result> Execute(SocketUserMessage e, Command [ ] commands) {
            var catagories = commands.Where (x => x.AllowExecution (e, new List<string> ()) == "").GroupBy (x => x.catagory);
            string result = "```";

            foreach (var catagory in catagories) {
                result += catagory.ElementAt (0).catagory.ToString () + " Commands\n";
                foreach (var item in catagory) {
                    result += Utility.FormatCommand (item) + "\n";
                }
                result += "\n";
            }
            result += "```";

            // I mean, it works, right?
            return TaskResult (result, result);
        }

        public Task<Result> Execute(SocketUserMessage e, CommandSet set) {
            return TaskResult (set.GetHelp (e), set.GetHelp (e));
        }
    }
}
