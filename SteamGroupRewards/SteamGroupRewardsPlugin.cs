using RestoreMonarchy.SteamGroupRewards.Helpers;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.API.Serialisation;
using Rocket.Core;
using Rocket.Core.Logging;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Linq;
using System.Net;

namespace RestoreMonarchy.SteamGroupRewards
{
    public class SteamGroupRewardsPlugin : RocketPlugin<SteamGroupRewardsConfiguration>
    {
        public static SteamGroupRewardsPlugin Instance { get; private set; }
        public UnityEngine.Color MessageColor { get; set; }

        public ulong[] GroupMembers { get; set; } = null;

        protected override void Load()
        {
            Instance = this;
            MessageColor = UnturnedChat.GetColorFromName(Configuration.Instance.MessageColor, UnityEngine.Color.green);

            InvokeRepeating(nameof(RefreshGroupMembers), 0, Configuration.Instance.RefreshTimeSeconds);

            U.Events.OnPlayerConnected += CheckPlayer;

            Logger.Log($"{Name} {Assembly.GetName().Version.ToString(3)} has been loaded!", ConsoleColor.Yellow);
        }

        protected override void Unload()
        {
            CancelInvoke(nameof(RefreshGroupMembers));

            U.Events.OnPlayerConnected -= CheckPlayer;

            Logger.Log($"{Name} has been unloaded!", ConsoleColor.Yellow);
        }

        public override TranslationList DefaultTranslations => new()
        {
            { "Announcement", "{0} received {1} for joining {2} Steam group!" },
            { "Added", "You received {0} for joining our Steam group." },
            { "Removed", "You lost {0} for leaving our Steam group." }
        };

        internal void LogDebug(string message)
        {
            if (Configuration.Instance.Debug)
            {
                Logger.Log($"Debug >> {message}", ConsoleColor.Gray);
            }
        }

        private void RefreshGroupMembers()
        {
            ThreadHelper.RunAsynchronously(() =>
            {
                try
                {
                    LogDebug("Refreshing group members...");
                    GroupMembers = SteamHelper.GetAllGroupMembers(Configuration.Instance.SteamGroupName);
                    LogDebug($"Group members refreshed! Total: {GroupMembers.Length}");

                    ThreadHelper.RunSynchronously(() =>
                    {
                        foreach (Player player in PlayerTool.EnumeratePlayers())
                        {
                            UnturnedPlayer unturnedPlayer = UnturnedPlayer.FromPlayer(player);
                            CheckPlayer(unturnedPlayer);
                        }
                    });
                } catch (WebException ex)
                {
                    Logger.LogError($"Failed to refresh Steam group members: {ex.Message}");
                }
            });
        }

        private void CheckPlayer(UnturnedPlayer player)
        {
            if (GroupMembers == null)
            {
                LogDebug("Group members not loaded yet!");
                return;
            }

            RocketPermissionsGroup group = R.Permissions.GetGroup(Configuration.Instance.PermissionGroupID);
            if (group == null)
            {
                LogDebug($"Permission group {Configuration.Instance.PermissionGroupID} not found!");
                return;
            }

            bool isInPermissionGroup = group.Members.Contains(player.Id);
            bool isInSteamGroup = GroupMembers.Contains(player.CSteamID.m_SteamID);

            if (isInPermissionGroup && !isInSteamGroup)
            {
                RocketPermissionsProviderResult result = R.Permissions.RemovePlayerFromGroup(group.Id, player);
                if (result == RocketPermissionsProviderResult.Success)
                {
                    string msg = Translate("Removed", group.DisplayName);
                    UnturnedChat.Say(player, msg, MessageColor);
                    LogDebug($"{player.CharacterName} has been removed from {group.Id} group!");
                }
                else
                {
                    LogDebug($"Failed to remove {player.CharacterName} from {group.Id} group! Result: {result}");
                    return;
                }
            }

            if (!isInPermissionGroup && isInSteamGroup)
            {
                RocketPermissionsProviderResult result = R.Permissions.AddPlayerToGroup(Configuration.Instance.PermissionGroupID, player);

                if (result == RocketPermissionsProviderResult.Success)
                {
                    string msg = Translate("Added", group.DisplayName);
                    UnturnedChat.Say(player, msg, MessageColor);
                    LogDebug($"{player.CharacterName} has been added to {group.Id} group!");

                    if (Configuration.Instance.EnableAnnouncement)
                    {
                        string announcement = Translate("Announcement", player.CharacterName, group.DisplayName, Configuration.Instance.SteamGroupName);
                        UnturnedChat.Say(announcement, MessageColor);
                    }
                } else
                {
                    LogDebug($"Failed to add {player.CharacterName} to {group.Id} group! Result: {result}");
                }

                return;
            }
        }
    }
}