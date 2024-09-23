using BepInEx;
using RareMagicPortal;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using YamlDotNet.Serialization;

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

        private bool CheckPassword(string inputPassword, string actualPassword)
        {
            return inputPassword == actualPassword;
        }

        public static void HandlePortalModeSelection(TeleportWorld portalInstance, Player player, PortalMode selectedMode,  ModeSelectionPopup PopInstance)
        {
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
                    SetTransportNetworkMode(PopInstance);
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
            PortalColorLogic.ClientORServerYMLUpdate(PortalColorLogic.PortalN.Portals[PopInstance.portalName], PopInstance.portalName);
            PopInstance.DestorySelf(); // Done and delete popup instance
        }

        public static void SetMode(PortalMode mode, string PortalName, string zdo)
        {
            if (!MagicPortalFluid.isAdmin) return;

            PortalMode checkmode = mode;

            switch (mode)
            {
                case PortalMode.TargetPortal:
                    if (!MagicPortalFluid.TargetPortalLoaded)
                        checkmode = PortalMode.Normal;
                    break;
                    // Add additional case handling as needed
            }

            PortalColorLogic.PortalN.Portals[PortalName].PortalZDOs[zdo].SpecialMode = (int)checkmode;
            PortalColorLogic.PortalN.Portals[PortalName].SpecialMode = (int)mode;
        }

        public static PortalMode GetCurrentMode(string PortalName, string zdo)
        {
            var specialMode = PortalColorLogic.PortalN.Portals[PortalName].PortalZDOs[zdo].SpecialMode;
            if (specialMode == 99)
                return PortalMode.Normal; // Default to Normal mode for undefined modes
            return (PortalMode)specialMode;
        }

        private bool IsAdmin()
        {
            return MagicPortalFluid.isAdmin;
        }

        // New Mode Handling Functions for each PortalMode

        private static void SetNormalMode(ModeSelectionPopup PopInstance)
        {
            SetMode(PortalMode.Normal, PopInstance.portalName, PopInstance.zdo);
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Portal is now in Normal mode.");
        }

        private static void SetTargetPortalMode(ModeSelectionPopup PopInstance)
        {
            SetMode(PortalMode.TargetPortal, PopInstance.portalName, PopInstance.zdo);
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Portal is now set to Target Portal mode.");
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

            PortalColorLogic.PortalN.Portals[PopInstance.portalName].PortalZDOs[PopInstance.zdo].Password = PopInstance.passwordInputField.text;
            SetMode(PortalMode.PasswordLock, PopInstance.portalName, PopInstance.zdo);
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Portal is now locked with a password.");
        }

        private static void SetOneWayMode(ModeSelectionPopup PopInstance)
        {
            SetMode(PortalMode.OneWay, PopInstance.portalName, PopInstance.zdo);
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Portal is now set to One-Way mode.");
        }

        private static void SetOneWayPasswordLockMode(ModeSelectionPopup PopInstance)
        {
            if (string.IsNullOrWhiteSpace(PopInstance.passwordInputField.text))
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Password cannot be empty.");
                return;
            }

            PortalColorLogic.PortalN.Portals[PopInstance.portalName].PortalZDOs[PopInstance.zdo].Password = PopInstance.passwordInputField.text;
            SetMode(PortalMode.OneWayPasswordLock, PopInstance.portalName, PopInstance.zdo);
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Portal is now set to One-Way Password Locked mode.");
        }

        private static void AddAllowedUser(ModeSelectionPopup PopInstance)
        {
            var allow = PopInstance.allowedUsersInputField.text.ToUpper();
            List<string> allowlist = allow.Split(',').ToList();

            PortalColorLogic.PortalN.Portals[PopInstance.portalName].AllowedUsers = allowlist;
            SetMode(PortalMode.AllowedUsersOnly, PopInstance.portalName, PopInstance.zdo);

            MagicPortalFluid.RareMagicPortal.LogMessage("Users added to allow list: " + allow);
        }

        private static void SetTransportNetworkMode(ModeSelectionPopup PopInstance)
        {
            SetMode(PortalMode.TransportNetwork, PopInstance.portalName, PopInstance.zdo);
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Portal is now in Transport Network mode.");
        }

        private static void SetCoordinates(ModeSelectionPopup PopInstance, string coordinates)
        {
            PortalColorLogic.PortalN.Portals[PopInstance.portalName].PortalZDOs[PopInstance.zdo].Coords = coordinates;
            SetMode(PortalMode.CordsPortal, PopInstance.portalName, PopInstance.zdo);
            MagicPortalFluid.RareMagicPortal.LogMessage($"Portal coordinates set to {coordinates}");
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

        private static void SetRainbowMode(ModeSelectionPopup PopInstance)
        {
            SetMode(PortalMode.Rainbow, PopInstance.portalName, PopInstance.zdo);
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Portal is now in Rainbow mode.");
        }

        private static void SetRandomTeleportMode(ModeSelectionPopup PopInstance)
        {
            SetMode(PortalMode.RandomTeleport, PopInstance.portalName, PopInstance.zdo);
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Portal is now set to Random Teleport mode.");
        }

        private static void SetAdminOnlyMode(ModeSelectionPopup PopInstance)
        {
            SetMode(PortalMode.AdminOnly, PopInstance.portalName, PopInstance.zdo);
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Portal is now set to Admin-Only mode.");
        }

        private static void SetDefaultMode(ModeSelectionPopup PopInstance, PortalMode pass = PortalMode.Normal)
        {
            SetMode(pass, PopInstance.portalName, PopInstance.zdo);
        }
    }
}
