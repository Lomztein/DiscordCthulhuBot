﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord;

namespace Adminthulhu {
    public static class Utility {

        public static List<SocketGuildUser> ForceGetUsers(ulong channelID) {
            SocketGuildChannel channel = GetServer ().GetChannel (channelID);
            List<SocketGuildUser> result = new List<SocketGuildUser> ();
            foreach (SocketGuildUser u in channel.Users) {
                result.Add (GetServer ().GetUser (u.Id));
            }
            return result;
        }

        private static int maxTries = 5;
        public static async Task SecureAddRole(SocketGuildUser user, SocketRole role) {
            int tries = 0;
            while (!user.Roles.Contains (role)) {
                if (tries > maxTries) {
                    Program.messageControl.SendMessage (SearchChannel (GetServer (), Program.dumpTextChannelName) as SocketTextChannel, "Error - tried to add role too many times.");
                    break;
                }
                tries++;
                ChatLogger.Log ("Adding role to " + user.Username + " - " + role.Name);
                await (user.AddRoleAsync (role));
                await Task.Delay (5000);
            }
        }

        public static async Task SecureRemoveRole(SocketGuildUser user, SocketRole role) {
            int tries = 0;
            while (user.Roles.Contains (role)) {
                if (tries > maxTries) {
                    Program.messageControl.SendMessage (SearchChannel (GetServer (), Program.dumpTextChannelName) as SocketTextChannel, "Error - tried to remove role too many times.");
                    break;
                }
                ChatLogger.Log ("Removing role from " + user.Username + " - " + role.Name);
                tries++;
                await (user.RemoveRoleAsync (role));
                await Task.Delay (5000);
            }
        }

        public static List<string> ConstructArguments(string fullCommand, out string command) {
            string toSplit = fullCommand.Substring (fullCommand.IndexOf (' ') + 1);
            List<string> arguments = new List<string> ();
            command = "";

            if (fullCommand.LastIndexOf (' ') != -1) {
                // FEEL THE SPAGHETTI.
                command = fullCommand.Substring (0, fullCommand.Substring (1).IndexOf (' ') + 1);
                string [ ] loc = toSplit.Split (';');
                for (int i = 0; i < loc.Length; i++) {

                    loc [ i ] = TrimSpaces (loc [ i ]);
                    arguments.Add (loc [ i ]);
                }
            } else {
                command = fullCommand;
            }

            return arguments;
        }

        public static SocketGuild GetServer(SocketChannel channel) {
            return (channel as SocketGuildChannel).Guild;
        }

        public static string GetUserName(SocketGuildUser user) {
            if (user == null)
                return "[ERROR - NULL USER REFERENCE]";
            if (user.Nickname == null)
                return user.Username;
            return user.Nickname;
        }

        public static string GetUserUpdateName(SocketGuildUser beforeUser, SocketGuildUser afterUser, bool before) {
            if (before) {
                if (beforeUser.Nickname == null)
                    return afterUser.Username;
                return beforeUser.Nickname;
            } else {
                if (afterUser.Nickname == null)
                    return afterUser.Username;
                return afterUser.Nickname;
            }
        }

        public static SocketGuildChannel GetMainChannel(SocketGuild server) {
            return SearchChannel (server, Program.mainTextChannelName);
        }

        [Obsolete]
        public static SocketGuildChannel GetChannelByName(SocketGuild server, string name) {
            if (server == null)
                return null;

            SocketGuildChannel channel = SearchChannel (server, name);
            return channel;
        }

        public static SocketGuildChannel SearchChannel(SocketGuild server, string name) {
            IEnumerable<SocketGuildChannel> channels = server.Channels;
            foreach (SocketGuildChannel channel in channels) {
                if (channel.Name.Length >= name.Length && channel.Name.Substring (0, name.Length) == name)
                    return channel;
            }

            return null;
        }

        public static SocketGuild GetServer() {
            if (Program.server != null)
                return Program.server;

            if (Program.discordClient != null) {
                IEnumerable<SocketGuild> servers = Program.discordClient.Guilds;
                if (servers.Count () != 0)
                    Program.server = servers.ElementAt (0);
            } else {
                return null;
            }

            return Program.server;
        }

        // I thought the TrimStart and TrimEnd functions would work like this, which they may, but I couldn't get them working. Maybe I'm just an idiot, but whatever.
        private static string TrimSpaces(string input) {
            if (input.Length == 0)
                return " ";

            string trimmed = input;
            while (trimmed [ 0 ] == ' ') {
                trimmed = trimmed.Substring (1);
            }

            while (trimmed [ trimmed.Length - 1 ] == ' ') {
                trimmed = trimmed.Substring (0, trimmed.Length - 1);
            }
            return trimmed;
        }

        public static SocketGuildUser FindUserByName(SocketGuild server, string username) {
            foreach (SocketGuildUser user in server.Users) {
                string name = GetUserName (user).ToLower ();
                if (name.Length >= username.Length && name.Substring (0, username.Length) == username.ToLower ())
                    return user;
            }
            return null;
        }

        public static string GetChannelName(SocketMessage e) {
            if (e.Channel as SocketDMChannel != null) {
                return "Private message: " + e.Author.Username;
            } else {
                return (e.Channel as SocketGuildChannel).Guild.Name + "/" + e.Channel.Name + "/" + e.Author.Username;
            }
        }

        public static async Task SetGame(string gameName) { // Wrapper functions ftw
            await Program.discordClient.SetGameAsync (gameName);
        }

        public static bool IsCorrectMessage(Cacheable<IUserMessage, ulong> message, SocketReaction reaction, ulong desiredMessage, string emojiName) {
            if (emojiName != null)
                return message.Id == desiredMessage && reaction.Emoji.Name == emojiName;
            return message.Id == desiredMessage;
        }
    }
}