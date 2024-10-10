using BepInEx;
using Guilds;
using RareMagicPortal;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using YamlDotNet.Serialization;
using static RareMagicPortal.PortalName;

namespace RareMagicPortal_3_Plus.PortalMode
{
    public class PortalModeClass
    {
        public enum PortalMode
        {
            Normal = 0,
            TargetPortal = 1,
            CrystalKeyMode = 2,
            PasswordLock = 3,
            OneWay = 4,
            OneWayPasswordLock = 5,
            AllowedUsersOnly = 6,
            TransportNetwork = 7,
            CordsPortal = 8,
            Rainbow = 9,
            RandomTeleport = 10,
            AdminOnly = 11,
        }

        internal static bool CheckPassword(string inputPassword, string actualPassword)
        {
            return inputPassword == actualPassword;
        }

        public static void HandlePortalModeSelection(TeleportWorld portalInstance, Player player, PortalMode selectedMode, ModeSelectionPopup PopInstance)
        {

            // reset to default for special fields only like active // random teleport
            foreach (var port in PortalColorLogic.PortalN.Portals)
            {
                if (port.Key == PopInstance.portalName)
                {
                    foreach (var zdoEntry in port.Value.PortalZDOs)
                    {
                        zdoEntry.Value.Active = true;
                        zdoEntry.Value.RandomTeleport = false;
                        // zdoEntry.Value.OverridePortal = false;
                    }
                }
            }
            // set PortalNames to Default for specials
            PortalColorLogic.PortalN.Portals[PopInstance.portalName].Admin_only_Access = false;
            PortalColorLogic.PortalN.Portals[PopInstance.portalName].GuildOnly = "";



            if (selectedMode == PortalMode.Normal || selectedMode == PortalMode.AdminOnly || selectedMode == PortalMode.OneWay || selectedMode == PortalMode.OneWayPasswordLock)// Not for all
            {              
                foreach (var port in PortalColorLogic.PortalN.Portals)
                {
                    if (port.Key == PopInstance.portalName)
                    {
                        foreach (var zdoEntry in port.Value.PortalZDOs)
                        {
                            zdoEntry.Value.Color = PortalColorLogic.PortalN.Portals[PopInstance.portalName].Color;
                        }
                    }
                }
            }



            switch (selectedMode)
            {
                case PortalMode.Normal:
                    SetNormalMode(PopInstance);
                    break;
                case PortalMode.TargetPortal:
                    SetTargetPortalMode(PopInstance);
                    break;
                case PortalMode.CrystalKeyMode:
                    SetCrystalKeyMode(PopInstance);
                    break;
                case PortalMode.PasswordLock:
                    SetPasswordLockMode(PopInstance);
                    break;
                case PortalMode.OneWay:
                    SetOneWayMode(PopInstance);
                    break;
                case PortalMode.OneWayPasswordLock:
                    SetOneWayPasswordLockMode(PopInstance);
                    break;
                case PortalMode.AllowedUsersOnly:
                    AddAllowedUser(PopInstance);
                    break;
                case PortalMode.TransportNetwork:
                    SetTransportNetworkMode(PopInstance, portalInstance);
                    break;
                case PortalMode.CordsPortal:
                    if (TryParseCoordinates(PopInstance, out Vector3 coordinates))
                    {
                        SetCoordinates(PopInstance, PopInstance.coordinatesInputField.text);
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.Center, "Invalid coordinates format. Please enter in x,y,z format.");
                    }
                    break;
                case PortalMode.Rainbow:
                    SetRainbowMode(PopInstance);
                    break;
                case PortalMode.RandomTeleport:
                    SetRandomTeleportMode(PopInstance);
                    break;
                case PortalMode.AdminOnly:
                    SetAdminOnlyMode(PopInstance);
                    break;
                default:
                    SetDefaultMode(PopInstance, selectedMode);
                    break;
            }

            //YML UPDATE
            if (!MagicPortalFluid.isAdmin) return;
            HandleOtherToggles(PopInstance);
            PortalColorLogic.ClientORServerYMLUpdate(PortalColorLogic.PortalN.Portals[PopInstance.portalName], PopInstance.portalName);
            PopInstance.DestorySelf(); // Done and delete popup instance
        }

        public static void SetMode(PortalMode mode, string PortalName, string zdo, bool overrideport = false)
        {       
            PortalMode checkmode = mode;

            switch (mode) // extra check
            {
                case PortalMode.TargetPortal:
                    if (!MagicPortalFluid.TargetPortalLoaded)
                        checkmode = PortalMode.Normal;
                    break;
                    // Add additional case handling as needed
            }

            PortalColorLogic.PortalN.Portals[PortalName].PortalZDOs[zdo].SpecialMode = checkmode;
            PortalColorLogic.PortalN.Portals[PortalName].SpecialMode = checkmode;
            PortalColorLogic.PortalN.Portals[PortalName].PortalZDOs[zdo].Active = true;
        }

        public static void HandleOtherToggles(ModeSelectionPopup PopInstance)
        {
            PortalColorLogic.PortalN.Portals[PopInstance.portalName].PortalZDOs[PopInstance.zdo].CrystalActive = PopInstance.crystalsKeysBox.isOn;
            PortalColorLogic.PortalN.Portals[PopInstance.portalName].TeleportAnything = PopInstance.allowEverythingBox.isOn;
            PortalColorLogic.PortalN.Portals[PopInstance.portalName].PortalZDOs[PopInstance.zdo].FastTeleport = PopInstance.fastTeleportBox.isOn;
            PortalColorLogic.PortalN.Portals[PopInstance.portalName].PortalZDOs[PopInstance.zdo].ShowName = PopInstance.hoverNameBox.isOn;


            string weightInput = PopInstance.weightField.text.Trim();

            if (float.TryParse(weightInput, out float result))
            {
                if (result >= 0)
                {
                    Debug.Log("Converted to float: " + result);
                    PortalColorLogic.PortalN.Portals[PopInstance.portalName].MaxWeight = result;
                }
                else
                {
                    Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Weight cannot be negative, not saved");
                }
            }
            else
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Failed to Convert Weight to Float, not saved");
            }



            // Allow items
            string allowFieldInput = PopInstance.addAllowField.text;
            List<string> allowedItems = allowFieldInput.Trim().Split(',')
                .Select(item => item.Trim())
                .Where(item => !string.IsNullOrEmpty(item))
                .ToList();

            PortalColorLogic.PortalN.Portals[PopInstance.portalName].AdditionalAllowItems = allowedItems.Count > 0 ? allowedItems : null;

            // Block items
            string blockFieldInput = PopInstance.addBlockField.text;
            List<string> blockedItems = blockFieldInput.Trim().Split(',')
                .Select(item => item.Trim())
                .Where(item => !string.IsNullOrEmpty(item))
                .ToList();

            PortalColorLogic.PortalN.Portals[PopInstance.portalName].AdditionalProhibitItems = blockedItems.Count > 0 ? blockedItems : null;

            // Allowed users
            string allowedUsersInput = PopInstance.allowedUsersInputField.text;
            List<string> allowedUsers = allowedUsersInput.Trim().Split(',')
                .Select(user => user.Trim())
                .Where(user => !string.IsNullOrEmpty(user))
                .ToList();

            PortalColorLogic.PortalN.Portals[PopInstance.portalName].AllowedUsers = allowedUsers.Count > 0 ? allowedUsers : null;


        }
        public static PortalMode GetCurrentMode(string PortalName, string zdo)
        {
            var specialMode = PortalColorLogic.PortalN.Portals[PortalName].PortalZDOs[zdo].SpecialMode;
            return (PortalMode)specialMode;
        }

        private bool IsAdmin()
        {
            return MagicPortalFluid.isAdmin;
        }

        private static void SetNormalMode(ModeSelectionPopup PopInstance)
        {
            foreach (var port in PortalColorLogic.PortalN.Portals)
            {
                if (port.Key == PopInstance.portalName)
                {
                    foreach (var zdoEntry in port.Value.PortalZDOs)
                    {
                        zdoEntry.Value.SpecialMode = PortalMode.Normal;
                        zdoEntry.Value.CrystalActive = false;
                    }
                }
            }
            PopInstance.crystalsKeysBox.isOn = false;
            SetMode(PortalMode.Normal, PopInstance.portalName, PopInstance.zdo);
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Portal is now in Normal mode.");
        }

        private static void SetTargetPortalMode(ModeSelectionPopup PopInstance)
        {
            if (MagicPortalFluid.TargetPortalLoaded)
            {
                SetMode(PortalMode.TargetPortal, PopInstance.portalName, PopInstance.zdo);
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Portal is now set to Target Portal mode.");
            }
            else
            {
                SetMode(PortalMode.Normal, PopInstance.portalName, PopInstance.zdo);
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "TargetPortal Mod is NOT Installed, Setting to Normal Mode");

                foreach (var port in PortalColorLogic.PortalN.Portals)
                {
                    if (port.Key == PopInstance.portalName)
                    {
                        foreach (var zdoEntry in port.Value.PortalZDOs)
                        {
                            zdoEntry.Value.Color = PortalColorLogic.PortalN.Portals[PopInstance.portalName].Color;
                        }
                    }
                }
            }                        
        }

        private static void SetCrystalKeyMode(ModeSelectionPopup PopInstance)
        {
            SetMode(PortalMode.CrystalKeyMode, PopInstance.portalName, PopInstance.zdo);
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Portal is now in Crystal Key mode.");
        }

        private static void SetPasswordLockMode(ModeSelectionPopup PopInstance)
        {
            if (string.IsNullOrWhiteSpace(PopInstance.passwordInputField.text))
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Password cannot be empty.");
                return;
            }
            foreach (var port in PortalColorLogic.PortalN.Portals)
            {
                if (port.Key == PopInstance.portalName)
                {
                    foreach (var zdoEntry in port.Value.PortalZDOs)
                    {
                        zdoEntry.Value.Password = PopInstance.passwordInputField.text;
                        zdoEntry.Value.SpecialMode = PortalMode.PasswordLock;
                    }
                }
            }
            //PortalColorLogic.PortalN.Portals[PopInstance.portalName].PortalZDOs[PopInstance.zdo].Password = PopInstance.passwordInputField.text;
            SetMode(PortalMode.PasswordLock, PopInstance.portalName, PopInstance.zdo);
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "All Portals with this name are now locked with a password.");
        }

        private static void SetOneWayMode(ModeSelectionPopup PopInstance)
        {
            foreach (var port in PortalColorLogic.PortalN.Portals)
            {
                if (port.Key == PopInstance.portalName)
                {
                    foreach (var zdoEntry in port.Value.PortalZDOs)
                    {
                        zdoEntry.Value.SpecialMode = PortalMode.OneWay;
                        zdoEntry.Value.Active = false;
                    }
                }
            }
            SetMode(PortalMode.OneWay, PopInstance.portalName, PopInstance.zdo, true);
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Portal is now set to One-Way mode. All other portals with name are disabled");
        }

        private static void SetOneWayPasswordLockMode(ModeSelectionPopup PopInstance)
        {
            if (string.IsNullOrWhiteSpace(PopInstance.passwordInputField.text))
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Password cannot be empty.");
                return;
            }
            foreach (var port in PortalColorLogic.PortalN.Portals)
            {
                if (port.Key == PopInstance.portalName)
                {
                    foreach (var zdoEntry in port.Value.PortalZDOs)
                    {
                        zdoEntry.Value.Password = PopInstance.passwordInputField.text;
                        zdoEntry.Value.SpecialMode = PortalMode.OneWayPasswordLock;
                        zdoEntry.Value.Active = false;
                    }
                }
            }

            SetMode(PortalMode.OneWayPasswordLock, PopInstance.portalName, PopInstance.zdo, true);
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Portal is now set to One-Way Password Locked mode. All other portals with name are disabled");
        }

        private static void AddAllowedUser(ModeSelectionPopup PopInstance)
        {
            if (Guilds.API.IsLoaded()) {
                if (PopInstance.GuilddropField.value == 0)
                {
                  PortalColorLogic.PortalN.Portals[PopInstance.portalName].GuildOnly = "";                
                }else
                {
                    List<Guild> guilds = Guilds.API.GetGuilds();
                    List<string> guildNames = new List<string> { "None" };
                    guildNames.AddRange(guilds.Select(g => g.Name));

                    int selectedIndex = PopInstance.GuilddropField.value;

                    if (selectedIndex > 0 && selectedIndex < guildNames.Count) // Ensure that a valid guild is selected.
                    {
                        string selectedGuild = guildNames[selectedIndex]; // Get the selected guild's name.
                        PortalColorLogic.PortalN.Portals[PopInstance.portalName].GuildOnly = selectedGuild; // Set GuildOnly to the selected guild.

                        MagicPortalFluid.RareMagicPortal.LogMessage("Guild Members Only");
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "All Portals with this name are now in Guild Members Only mode.");
                        SetMode(PortalMode.AllowedUsersOnly, PopInstance.portalName, PopInstance.zdo);
                        return;
                    }
                }
            }

            SetMode(PortalMode.AllowedUsersOnly, PopInstance.portalName, PopInstance.zdo);

            MagicPortalFluid.RareMagicPortal.LogMessage("Allowed Users Mode");
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "All Portals with this name are now in Allowed Users Only mode.");
        }

        private static void SetTransportNetworkMode(ModeSelectionPopup PopInstance, TeleportWorld portalInstance)
        {

            if (portalInstance == null)
            {
                Debug.LogError("portalInstance is null in SetTransportNetworkMode");
                return;
            }

            if (PopInstance == null)
            {
                Debug.LogError("PopInstance is null in SetTransportNetworkMode");
                return;
            }

            foreach (var port in PortalColorLogic.PortalN.Portals)
            {
                if (port.Key == PopInstance.portalName)
                {
                    foreach (var zdoEntry in port.Value.PortalZDOs)
                    {
                        zdoEntry.Value.Active = false;
                    }
                }
            }

            Vector3 altcords = portalInstance.m_nview.GetZDO().GetPosition();

            var currentPortalZDO = PortalColorLogic.PortalN.Portals[PopInstance.portalName].PortalZDOs[PopInstance.zdo];

            currentPortalZDO.Coords = $"{altcords.x},{altcords.y},{altcords.z}";
            currentPortalZDO.CrystalActive = false;

            SetMode(PortalMode.TransportNetwork, PopInstance.portalName, PopInstance.zdo, true);
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Portal is now in Transport Network mode. All other portals with this name are deactivated");
        }

        private static void SetCoordinates(ModeSelectionPopup PopInstance, string coordinates)
        {
            PortalColorLogic.PortalN.Portals[PopInstance.portalName].PortalZDOs[PopInstance.zdo].Coords = coordinates;
            SetMode(PortalMode.CordsPortal, PopInstance.portalName, PopInstance.zdo);
            MagicPortalFluid.RareMagicPortal.LogMessage($"Portal coordinates set to {coordinates}");
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Portal is now is now set to Coordinates "+ coordinates);
        }

        public static bool TryParseCoordinates(ModeSelectionPopup PopInstance, out Vector3 coords)
        {
            coords = Vector3.zero;
            string[] parts = PopInstance.coordinatesInputField.text.Split(',');
            if (parts.Length != 3) return false;

            if (float.TryParse(parts[0], out float x) &&
                float.TryParse(parts[1], out float y) &&
                float.TryParse(parts[2], out float z))
            {
                coords = new Vector3(x, y, z);
                return true;
            }

            return false;
        }

        public static bool TryParseCoordinates(string coord, out Vector3 coords)
        {
            coords = Vector3.zero;
            string[] parts = coord.Split(',');
            if (parts.Length != 3) return false;

            if (float.TryParse(parts[0], out float x) &&
                float.TryParse(parts[1], out float y) &&
                float.TryParse(parts[2], out float z))
            {
                coords = new Vector3(x, y, z);
                return true;
            }

            return false;
        }

        private static void SetRainbowMode(ModeSelectionPopup PopInstance)
        {

            PortalColorLogic.PortalN.Portals[PopInstance.portalName].PortalZDOs[PopInstance.zdo].SpecialMode = PortalMode.Rainbow;
            PortalColorLogic.PortalN.Portals[PopInstance.portalName].PortalZDOs[PopInstance.zdo].Active = true;

            //SetMode(PortalMode.Rainbow, PopInstance.portalName, PopInstance.zdo);
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Portal is now in Rainbow mode.");
        }

        private static void SetRandomTeleportMode(ModeSelectionPopup PopInstance)
        {
            foreach (var port in PortalColorLogic.PortalN.Portals)
            {
                if (port.Key == PopInstance.portalName)
                {
                    foreach (var zdoEntry in port.Value.PortalZDOs)
                    {
                        zdoEntry.Value.RandomTeleport = true;
                    }
                }
            }
            SetMode(PortalMode.RandomTeleport, PopInstance.portalName, PopInstance.zdo);
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Portals with this name are now set to Random Teleport mode.");
        }

        private static void SetAdminOnlyMode(ModeSelectionPopup PopInstance)
        {
            PortalColorLogic.PortalN.Portals[PopInstance.portalName].Admin_only_Access = true;
            SetMode(PortalMode.AdminOnly, PopInstance.portalName, PopInstance.zdo);
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Portal with this name are Admin-Only mode.");
        }

        private static void SetDefaultMode(ModeSelectionPopup PopInstance, PortalMode pass = PortalMode.Normal)
        {
            SetMode(pass, PopInstance.portalName, PopInstance.zdo);
        }

    }
}
