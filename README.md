# Steam Group Rewards

A simple Unturned plugin that gives players special ranks/permissions for joining your Steam group.

## How It Works

- The plugin checks if players are members of your Steam group
- Members get special permissions on your server
- Players lose these permissions if they leave the group
- All Steam API calls run in separate threads to prevent server lag

## Configuration Options

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

- **MessageColor**: Color of plugin messages in chat (yellow, green, red, etc.)
- **SteamGroupName**: Your Steam group name (from the URL)
- **PermissionGroupID**: Permission group to give players when they join your Steam group
- **RefreshTimeSeconds**: How often to check for group membership changes (in seconds)
- **EnableAnnouncement**: When true, announces to all players when someone gets rewards

## Important Note

Due to Steam's caching system, membership changes may take about 10 minutes to be detected by the plugin, even if you set a shorter refresh time.

## Tips

- Create a `/steam` command on your server that opens a link to your Steam group, making it easy for players to join.  
  You can use [RichMessageAnnouncer](https://restoremonarchy.com/servers/plugins/richmessageannouncer) plugin to create this command.

### Reward Idea: Special Kit Access

One popular reward idea is to give Steam group members access to an exclusive kit. To do this:

1. Create a permission group called "steam" (or any name you prefer)
2. Set up this group with the "kit.steam" permission
3. Change the `PermissionGroupID` in the plugin configuration from "vip" to your group name

Here's an example permission group configuration:

```xml
<Group>
  <Id>steam</Id>
  <DisplayName>Steam</DisplayName>
  <Prefix />
  <Suffix />
  <Color>66c0f4</Color>
  <Members>
    <Member>76561198016438091</Member>
  </Members>
  <ParentGroup>default</ParentGroup>
  <Priority>100</Priority>
  <Permissions>
    <Permission Cooldown="0">kit.steam</Permission>
  </Permissions>
</Group>
```

This gives your Steam group members access to a special kit while also displaying their name in Steam's blue color in chat.

## Translations

```xml
<?xml version="1.0" encoding="utf-8"?>
<Translations xmlns:xsd="http://www.w3.org/2001/XMLSchema" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
  <Translation Id="Announcement" Value="{0} received {1} for joining {2} Steam group!" />
  <Translation Id="Added" Value="You received {0} for joining our Steam group." />
  <Translation Id="Removed" Value="You lost {0} for leaving our Steam group." />
</Translations>
```

## Credits

**Soer** for the idea and sponsoring the plugin.