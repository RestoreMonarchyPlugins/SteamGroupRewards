# Steam Group Rewards
Reward players for joining your Steam group with a rank.

## Features
- Plugin checks every RefreshTimeSeconds if player is in Steam group specified in configuration.
- If player is in the group, he gets a permission group specified in configuration.
- If player leaves the group, he loses a permission group specified in configuration.
- Optionally, you can enable announcement when player joins the group, so everyone can see it.
- All calls to Steam API are done in a separate thread to not block the main thread. So it won't cause any lag on your server.

I recommend adding a **/steam** command to your server that will open a link to your Steam group. You can create a **steam** permission group as well with permissions you want to give to players for joining, for example special kit or vault access.

## Credits
**Soer** for the idea and sponsoring the plugin.

## Configuration
```xml
<?xml version="1.0" encoding="utf-8"?>
<SteamGroupRewardsConfiguration xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <MessageColor>yellow</MessageColor>
  <SteamGroupName>RestoreMonarchy</SteamGroupName>
  <PermissionGroupID>vip</PermissionGroupID>
  <RefreshTimeSeconds>300</RefreshTimeSeconds>
  <EnableAnnouncement>true</EnableAnnouncement>
</SteamGroupRewardsConfiguration>
```

## Translations
```xml
<?xml version="1.0" encoding="utf-8"?>
<Translations xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <Translation Id="Announcement" Value="{0} received {1} for joining {2} Steam group!" />
  <Translation Id="Added" Value="You received {0} for joining our Steam group." />
  <Translation Id="Removed" Value="You lost {0} for leaving our Steam group." />
</Translations>
```