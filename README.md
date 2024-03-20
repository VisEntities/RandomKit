This plugin enables players to receive a randomly selected kit from a predefined list upon entering a command. It includes cooldowns to limit how often players can request a new kit.

[Demonstration](https://youtu.be/7MOk1MfTxNU)

--------------------

## Permissions

- `randomkit.use` - Allows players to use the `randomkit` command to receive a random kit.

----------------------

## Chat Commands

- `randomkit` - Gives the player a random kit from the list defined in the config, subject to cooldown restrictions to prevent frequent use.

----------------

## Configuration

```json
{
  "Version": "2.0.0",
  "Cooldown Seconds": 30.0,
  "Kits": [
    "Resources",
    "Components",
    "Ammo",
    "Food"
  ]
}
```

-----------------

## Localization

```json
{
  "NoPermission": "You do not have permission to use this command.",
  "KitGiven": "You have been given a random kit: <color=#FABE28>{0}</color>.",
  "KitCooldown": "You must wait <color=#FABE28>{0}</color> more seconds before requesting another kit.",
  "NoKitsAvailable": "There are no available kits.",
  "KitGiveError": "There was an issue giving you the kit. Please try again later or contact an admin."
}
```


---------------

## Credits
 * Rewritten from scratch and maintained to present by **VisEntities**
 * Originally created by **Orange**, up to version 1.0.2