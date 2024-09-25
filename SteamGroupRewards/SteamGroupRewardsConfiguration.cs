using Rocket.API;

namespace RestoreMonarchy.SteamGroupRewards
{
    public class SteamGroupRewardsConfiguration : IRocketPluginConfiguration
    {
        public bool Debug { get; set; }
        public bool ShouldSerializeDebug() => Debug;
        public string MessageColor { get; set; }
        public string SteamGroupName { get; set; }
        public string PermissionGroupID { get; set; }
        public float RefreshTimeSeconds { get; set; }
        public bool EnableAnnouncement { get; set; }

        public void LoadDefaults()
        {
            Debug = false;
            MessageColor = "yellow";
            SteamGroupName = "RestoreMonarchy";
            PermissionGroupID = "vip";
            RefreshTimeSeconds = 300;
            EnableAnnouncement = true;
        }
    }
}
