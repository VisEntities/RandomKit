using Newtonsoft.Json;
using Oxide.Core.Plugins;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

/*
 * Rewritten from scratch and maintained to present by VisEntities
 * Originally created by Orange, up to version 1.0.2
 */

namespace Oxide.Plugins
{
     [Info("Random Kit", "VisEntities", "2.0.0")]
     [Description("Gives players a random kit.")]
     public class RandomKit : RustPlugin
     {
        #region 3rd Party Dependencies

        [PluginReference]
        private readonly Plugin Kits;

        #endregion 3rd Party Dependencies

        #region Fields

        private static RandomKit _plugin;
        private static Configuration _config;
        private Dictionary<ulong, DateTime> _kitCooldowns = new Dictionary<ulong, DateTime>();

        #endregion Fields

        #region Configuration

        private class Configuration
        {
            [JsonProperty("Version")]
            public string Version { get; set; }

            [JsonProperty("Cooldown Seconds")]
            public float CooldownSeconds { get; set; }

            [JsonProperty("Kits")]
            public List<string> Kits { get; set; }
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();
            _config = Config.ReadObject<Configuration>();

            if (string.Compare(_config.Version, Version.ToString()) < 0)
                UpdateConfig();

            SaveConfig();
        }

        protected override void LoadDefaultConfig()
        {
            _config = GetDefaultConfig();
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(_config, true);
        }

        private void UpdateConfig()
        {
            PrintWarning("Config changes detected! Updating...");

            Configuration defaultConfig = GetDefaultConfig();

            if (string.Compare(_config.Version, "1.0.0") < 0)
                _config = defaultConfig;

            PrintWarning("Config update complete! Updated from version " + _config.Version + " to " + Version.ToString());
            _config.Version = Version.ToString();
        }

        private Configuration GetDefaultConfig()
        {
            return new Configuration
            {
                Version = Version.ToString(),
                CooldownSeconds = 30f,
                Kits = new List<string>
                {
                    "Resources",
                    "Components",
                    "Ammo",
                    "Food"
                }
            };
        }

        #endregion Configuration

        #region Oxide Hooks

        private void Init()
        {
            _plugin = this;
            PermissionUtil.RegisterPermissions();
        }

        private void Unload()
        {
            _config = null;
            _plugin = null;
        }

        #endregion Oxide Hooks

        #region Kits Integration

        public static bool TryGiveKit(BasePlayer player, string kitName)
        {
            if (!VerifyPluginBeingLoaded(_plugin.Kits))
                return false;


            object result = _plugin.Kits.Call("GiveKit", player, kitName);
            if (result is string)
                return false;

            return true;
        }

        #endregion Kits Integration

        #region Helper Functions

        private static bool VerifyPluginBeingLoaded(Plugin plugin)
        {
            if (plugin != null && plugin.IsLoaded)
                return true;
            else
                return false;
        }

        #endregion Helper Functions

        #region Utility Classes

        private static class PermissionUtil
        {
            public const string USE = "randomkit.use";

            public static void RegisterPermissions()
            {
                _plugin.permission.RegisterPermission(USE, _plugin);
            }

            public static bool VerifyHasPermission(BasePlayer player, string permissionName = USE)
            {
                return _plugin.permission.UserHasPermission(player.UserIDString, permissionName);
            }
        }

        #endregion Utility Classes

        #region Commands

        [ChatCommand("randomkit")]
        private void cmdGetRandomKit(BasePlayer player, string cmd, string[] args)
        {
            if (!PermissionUtil.VerifyHasPermission(player))
            {
                SendReplyToPlayer(player, Lang.NoPermission);
                return;
            }
            
            if (_config.Kits == null || _config.Kits.Count == 0)
            {
                SendReplyToPlayer(player, Lang.NoKitsAvailable);
                return;
            }

            if (_kitCooldowns.TryGetValue(player.userID, out DateTime lastUsed))
            {
                TimeSpan cooldownRemaining = DateTime.UtcNow - lastUsed;
                if (cooldownRemaining.TotalSeconds < _config.CooldownSeconds)
                {
                    SendReplyToPlayer(player, Lang.KitCooldown, Math.Ceiling(_config.CooldownSeconds - cooldownRemaining.TotalSeconds));
                    return;
                }
            }

            _kitCooldowns[player.userID] = DateTime.UtcNow;

            int randomIndex = Random.Range(0, _config.Kits.Count);
            string selectedKit = _config.Kits[randomIndex];

            if (!TryGiveKit(player, selectedKit))
            {
                SendReplyToPlayer(player, Lang.KitGiveError);
                return;
            }

            SendReplyToPlayer(player, Lang.KitGiven, selectedKit);
        }

        #endregion Commands

        #region Localization

        private class Lang
        {
            public const string NoPermission = "NoPermission";
            public const string KitGiven = "KitGiven";
            public const string KitCooldown = "KitCooldown";
            public const string NoKitsAvailable = "NoKitsAvailable";
            public const string KitGiveError = "KitGiveError";
        }

        protected override void LoadDefaultMessages()
        {
            lang.RegisterMessages(new Dictionary<string, string>
            {
                [Lang.NoPermission] = "You do not have permission to use this command.",
                [Lang.KitGiven] = "You have been given a random kit: <color=#FABE28>{0}</color>.",
                [Lang.KitCooldown] = "You must wait <color=#FABE28>{0}</color> more seconds before requesting another kit.",
                [Lang.NoKitsAvailable] = "There are no available kits.",
                [Lang.KitGiveError] = "There was an issue giving you the kit. Please try again later or contact an admin."
            }, this, "en");
        }

        private void SendReplyToPlayer(BasePlayer player, string messageKey, params object[] args)
        {
            string message = lang.GetMessage(messageKey, this, player.UserIDString);
            if (args.Length > 0)
                message = string.Format(message, args);

            SendReply(player, message);
        }

        #endregion Localization
    }
}