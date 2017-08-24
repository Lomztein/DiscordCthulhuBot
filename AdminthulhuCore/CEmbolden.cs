﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace Adminthulhu {
    class CEmbolden : Command {

        private char[] available = new char[] { 'a', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't'
                                              , 'u', 'v', 'w', 'x', 'y', 'z'};

        private char[] numbers = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};
        private Dictionary<char, string> specifics = new Dictionary<char, string> ();
        private bool ignoreUnavailable = true;

        public CEmbolden () {
            command = "embolden";
            shortHelp = "Embolden.";
            longHelp = "Makes your text much more bold, and kind of spammy.";
            argumentNumber = 1;
            catagory = Catagory.Fun;
            availableInDM = true;

            specifics.Add ('b', "🅱");
        }

        public override Task ExecuteCommand ( SocketUserMessage e, List<string> arguments ) {
            base.ExecuteCommand (e, arguments);
            if (AllowExecution (e, arguments)) {

                string inText = arguments[0];
                string outText = "";

                if (inText == "?")
                    return Task.CompletedTask;

                for (int i = 0; i < inText.Length; i++) {
                    if (inText[i] == ' ') {
                        outText += "  ";
                    }else {
                        char letter = inText.ToLower ()[i];
                        if (available.Contains (letter)) {
                            outText += ":regional_indicator_" + inText.ToLower () [ i ] + ": ";
                        } else if (specifics.ContainsKey (letter)) {
                            outText += specifics [ letter ];
                        } else if (numbers.Contains (letter)) {
                            outText += NumberToString (letter) + " ";
                        } else if (!ignoreUnavailable) {
                            Program.messageControl.SendMessage (e, "Unavailable character detected: " + letter, true);
                            return Task.CompletedTask;
                        }
                    }
                }

                Program.messageControl.SendMessage (e, outText, true);
            }
            return Task.CompletedTask;
        }

        // Considering this is now used in more than one class, it might be wise to move it to a core class in order to remain structured.
        public static string NumberToString (char number) {
            switch (number) {
                case '0':
                    return ":zero:";

                case '1':
                    return ":one:";

                case '2':
                    return ":two:";

                case '3':
                    return ":three:";

                case '4':
                    return ":four:";

                case '5':
                    return ":five:";

                case '6':
                    return ":six:";

                case '7':
                    return ":seven:";

                case '8':
                    return ":eight:";

                case '9':
                    return ":nine:";
            }

            return "";
        }
    }
}
