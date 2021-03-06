﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System.Globalization;
using Newtonsoft.Json;

namespace Adminthulhu {
    public class UserConfiguration : IConfigurable {

        public static Dictionary<ulong, List<Setting>> userSettings;
        public static string settingsFileName = "usersettings";
        public static List<UserSettingsCommandBase> commands = new List<UserSettingsCommandBase> ();
        public static Dictionary<string, object> defaultValues = new Dictionary<string, object> ();

        public static void Initialize() {
            userSettings = SerializationIO.LoadObjectFromFile<Dictionary<ulong, List<Setting>>> (Program.dataPath + settingsFileName + Program.gitHubIgnoreType);
            if (userSettings == null)
                userSettings = new Dictionary<ulong, List<Setting>> ();

            UserConfiguration config = new UserConfiguration ();
            config.LoadConfiguration ();
        }

        public static void SaveSettings() {
            SerializationIO.SaveObjectToFile (Program.dataPath + settingsFileName + Program.gitHubIgnoreType, userSettings);
        }

        public static void AddCommand(UserSettingsCommandBase command) {
            commands.Add (command);
        }

        public static T GetSetting<T>(ulong userID, string key) {
            if (userSettings.ContainsKey (userID)) {
                List<Setting> set = userSettings [ userID ];
                foreach (Setting s in set) {
                    if (s.name == key) {
                        return Utility.SecureConvertObject<T> (s.value);
                    }
                }
            }

            return (T)Convert.ChangeType (defaultValues [ key ], typeof (T));
        }

        public static void SetSetting(ulong userID, string key, object value) {
            if (userSettings.ContainsKey (userID)) {
                List<Setting> set = userSettings [ userID ];
                foreach (Setting s in set) {
                    if (s.name == key) {
                        s.value = value;
                        userSettings [ userID ] = set;
                        SaveSettings ();
                        return;
                    }
                }

                set.Add (new Setting (key, value));
                userSettings [ userID ] = set;
                SaveSettings ();
                return;
            }

            userSettings.Add (userID, new List<Setting> ());
            userSettings [ userID ].Add (new Setting (key, value));
            SaveSettings ();
        }

        public static bool ToggleBoolean(ulong userID, string key) {
            bool newSetting = !GetSetting<bool> (userID, key);
            SetSetting (userID, key, newSetting);
            return newSetting;
        }

        public void LoadConfiguration() {
            foreach (UserSettingsCommandBase settingBase in commands) {
                defaultValues.Add (settingBase.key, BotConfiguration.GetSetting ("UserSettings." + settingBase.key + "Default", this, settingBase.superDefaultValue));
            }
        }

        public class Setting {
            public string name;
            public object value;

            public Setting(string _name, object _value) {
                name = _name;
                value = _value;
            }
        }

        public class OptInRole {
            public string name;
            public ulong roleID;

            public OptInRole(string _name, ulong _roleID) {
                name = _name; roleID = _roleID;
            }
        }
    }

    public abstract class UserSettingsCommandBase : Command { // It is said that names close to each other is a bad idea. /shrug

        public string key;
        public object superDefaultValue; // The value default from the code.

        public override void Initialize() {
            base.Initialize ();
            UserConfiguration.AddCommand (this);
        }
    }

    public class UserSettingsCommands : CommandSet {
        public UserSettingsCommands() {
            command = "settings";
            shortHelp = "User settings command set.";
            catagory = Category.Utility;

            commandsInSet = new Command [ ] { new CSetBirthday (), new CSetCulture (), new CAutomaticLooking (),
            new CToggleSnooping (), new CAutoManageGameRoles (), new ToggleAdvancedCommandMode (), new RoleSet () };
        }

        public class RoleSet : CommandSet, IConfigurable {
            public RoleSet() {
                command = "roles";
                shortHelp = "Togglable roles.";
                catagory = Category.Utility;

                commandsInSet = new Command [0];
            }

            public override void Initialize() {
                base.Initialize ();
            }

            public override void LoadConfiguration() {
                base.LoadConfiguration ();

                Dictionary<string, ulong> togglables = new Dictionary<string, ulong> ();
                togglables.Add ("International", 0);
                togglables.Add ("NSFW", 1);

                togglables = BotConfiguration.GetSetting ("UserSettings.Togglables", this, togglables);

                commandsInSet = new Command [ 0 ];
                List<Command> tCommands = new List<Command> ();
                foreach (var entry in togglables) {
                    CToggleRole cmd = new CToggleRole ();
                    cmd.command = entry.Key.ToLower ();
                    cmd.roleID = entry.Value;

                    tCommands.Add (cmd);
                }

                AddProceduralCommands (tCommands.ToArray ());
            }
        }

        public class CSetBirthday : UserSettingsCommandBase {

            public CSetBirthday() {
                command = "birthday";
                shortHelp = "Set birthday.";
                key = "Birthday";
                superDefaultValue = null;

                AddOverload (typeof (void), "Set your birthday date to <date>, so we know when to congratulate you!");
            }

            public Task<Result> Execute(SocketUserMessage e, DateTime parse) {
                Birthdays.SetBirthday (e.Author.Id, parse);
                CultureInfo info = new CultureInfo (UserConfiguration.GetSetting<string> (e.Author.Id, "Culture"));
                return TaskResult (null, "You have succesfully set your birthday to **" + parse.ToString (info) + "**.");
            }
        }

        public class CSetCulture : UserSettingsCommandBase {
            public CSetCulture() {
                command = "culture";
                shortHelp = "Set culture.";
                key = "Culture";
                superDefaultValue = "en-US";
                AddOverload (typeof (CultureInfo), "Sets your preferred culture to <culture [language-COUNTRY]>. Used for proper formatting of stuff such as datetime.");
            }

            public Task<Result> Execute(SocketUserMessage e, CultureInfo info) {
                UserConfiguration.SetSetting (e.Author.Id, "Culture", info);
                return TaskResult (info, "Successfully sat culture to " + info.DisplayName);
            }

            public override void Initialize() {
                base.Initialize ();
                superDefaultValue = Program.cultureName;
            }
        }

        public class CAutomaticLooking : UserSettingsCommandBase {
            public CAutomaticLooking() {
                command = "autolooking";
                shortHelp = "Toggle automatic !looking command.";
                key = "AutoLooking";
                superDefaultValue = false;
                AddOverload (typeof (bool), "Toggles automatically enabling the !looking command.");
            }

            public Task<Result> Execute(SocketUserMessage e) {
                bool result = UserConfiguration.ToggleBoolean (e.Author.Id, "AutoLooking");
                return TaskResult (result, "Autolooking on voice channels enabled: " + result.ToString ());
            }
        }


        public class ToggleAdvancedCommandMode : UserSettingsCommandBase {

            public ToggleAdvancedCommandMode() {
                command = "advcommands";
                shortHelp = "Toggle advanced command mode.";
                AddOverload (typeof (bool), "Toggles advanced command help mode, shows return and input types.");
                key = "AdvancedCommandsMode";
                superDefaultValue = false;
            }

            public Task<Result> Execute(SocketUserMessage e) {
                bool result = UserConfiguration.ToggleBoolean (e.Author.Id, "AdvancedCommandsMode");
                return TaskResult (result, "Advanced commands mode enabled: " + result.ToString ());
            }

        }

        public class CToggleSnooping : UserSettingsCommandBase {
            public CToggleSnooping() {
                command = "snooping";
                shortHelp = "Toggle Adminthulhu snooping.";
                key = "AllowSnooping";
                superDefaultValue = true;

                AddOverload (typeof (bool), "Toggles non-critical bot snooping on you.");
            }

            public Task<Result> Execute(SocketUserMessage e) {
                bool result = UserConfiguration.ToggleBoolean (e.Author.Id, "AllowSnooping");
                return TaskResult (result, "Bot snooping enabled: " + result.ToString ());
            }
        }

        public class CAutoManageGameRoles : UserSettingsCommandBase {
            public CAutoManageGameRoles() {
                command = "autoroles";
                shortHelp = "Toggle automanage game roles.";
                key = "AutoManageGameRoles";
                superDefaultValue = false;
                AddOverload (typeof (bool), "Determines of game roles will be added to you automatically.");
            }

            public Task<Result> Execute(SocketUserMessage e) {
                bool result = UserConfiguration.ToggleBoolean (e.Author.Id, "AutoManageGameRoles");
                return TaskResult (result, "Auto roles enabled: " + result.ToString ());
            }
        }

        /// <summary>
        /// A generic command, defaults to NSFW.
        /// </summary>
        public class CToggleRole : Command {
            public ulong roleID = 0;
            public CToggleRole() {
                command = "toggle_role_base";
                shortHelp = "Command for role toggling a role.";
                AddOverload (typeof (bool), "Toggles the given role.");
            }

            public async Task<Result> Execute(SocketUserMessage e) {
                SocketRole role = Utility.GetServer ().GetRole (roleID);
                if ((e.Author as SocketGuildUser).Roles.Contains (role)) {
                    await Utility.SecureRemoveRole (e.Author as SocketGuildUser, role);
                    return new Result (false, $"Succesfully removed {command} role.");
                } else {
                    await Utility.SecureAddRole (e.Author as SocketGuildUser, role);
                    return new Result(true, $"Succesfully added {command} role.");
                }
            }
        }
    }
}