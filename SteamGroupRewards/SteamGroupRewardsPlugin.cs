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
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace RestoreMonarchy.SteamGroupRewards
{
    public class SteamGroupRewardsPlugin : RocketPlugin<SteamGroupRewardsConfiguration>
    {
        public static SteamGroupRewardsPlugin Instance { get; private set; }
        public UnityEngine.Color MessageColor { get; set; }

        public ulong[] GroupMembers { get; set; } = null;

        // Track consecutive times a player was not found in the group
        private ConcurrentDictionary<ulong, int> membershipMissingCount = new ConcurrentDictionary<ulong, int>();

        // Number of consecutive checks required before removing rewards
        private const int REQUIRED_MISSING_CHECKS = 3;

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
            membershipMissingCount.Clear();

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
                }
                catch (WebException ex)
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
            ulong steamId = player.CSteamID.m_SteamID;

            // Player is in permission group but not found in Steam group
            if (isInPermissionGroup && !isInSteamGroup)
            {
                // Increment missing count
                int currentCount = membershipMissingCount.AddOrUpdate(steamId,
                    1, // Initial value if key doesn't exist
                    (key, oldValue) => oldValue + 1 // Increment existing value
                );

                LogDebug($"Player {player.CharacterName} not found in Steam group. Missing count: {currentCount}/{REQUIRED_MISSING_CHECKS}");

                // Only remove if we've confirmed multiple times
                if (currentCount >= REQUIRED_MISSING_CHECKS)
                {
                    RocketPermissionsProviderResult result = R.Permissions.RemovePlayerFromGroup(group.Id, player);
                    if (result == RocketPermissionsProviderResult.Success)
                    {
                        string msg = Translate("Removed", group.DisplayName);
                        UnturnedChat.Say(player, msg, MessageColor);
                        LogDebug($"{player.CharacterName} has been removed from {group.Id} group after {REQUIRED_MISSING_CHECKS} confirmations!");

                        // Reset the counter after successful removal
                        membershipMissingCount.TryRemove(steamId, out _);
                    }
                    else
                    {
                        LogDebug($"Failed to remove {player.CharacterName} from {group.Id} group! Result: {result}");
                    }
                }
                return;
            }

            // If player is found in Steam group, reset their missing count
            if (isInSteamGroup)
            {
                membershipMissingCount.TryRemove(steamId, out _);
            }

            // Add player to permission group if they're in Steam group but not permission group
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
                }
                else
                {
                    LogDebug($"Failed to add {player.CharacterName} to {group.Id} group! Result: {result}");
                }
            }
        }
    }
}