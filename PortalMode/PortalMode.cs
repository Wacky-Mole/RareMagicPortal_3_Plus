using RareMagicPortal;
using System.Collections.Generic;
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
            Rainbow = 2,
            PasswordLock = 3,
            OneWay = 4,
            OneWayPasswordLock = 5,
            AllowedUsersOnly = 6,
            TransportNetwork = 7,
            CordsPortal = 8
        }

        private ZNetView m_nview;
        private List<string> allowedUsers;
        private PortalMode currentMode = PortalMode.Normal;
        private string password;
        private Vector3 targetCoordinates;
        private BasePopup inputPopup;
        private Dictionary<string, Vector3> transportLocations;

        private void Awake()
        {
            allowedUsers = new List<string>();
            transportLocations = new Dictionary<string, Vector3>
            {
                { "Home", new Vector3(100, 0, 100) },
                { "Market", new Vector3(200, 0, 200) },
                // Add more predefined locations here
            };

            // Load initial state from ZDO
            currentMode = (PortalMode)m_nview.GetZDO().GetInt("PortalMode", (int)PortalMode.Normal);
            password = m_nview.GetZDO().GetString("PortalPassword", "");
            targetCoordinates = m_nview.GetZDO().GetVec3("PortalCoordinates", Vector3.zero);
        }

        public void SetMode(PortalMode mode)
        {
            if (!IsAdmin()) return;

            currentMode = mode;
            UpdatePortalYML();
            ApplyModeSettings();
        }

        private void ApplyModeSettings()
        {
            switch (currentMode)
            {
                case PortalMode.Normal:
                    // Normal portal behavior
                    break;
                case PortalMode.TargetPortal:
                    if (MagicPortalFluid.TargetPortalLoaded)
                    {
                        // Handle TargetPortal behavior
                    }
                    else
                    {
                        // Load Normal behavior if not installed
                    }
                    break;
                case PortalMode.Rainbow:
                    // Handle Rainbow behavior
                    break;
                case PortalMode.PasswordLock:
                    // Handle PasswordLock behavior
                    break;
                case PortalMode.OneWay:
                    // Handle OneWay behavior
                    break;
                case PortalMode.OneWayPasswordLock:
                    // Handle OneWayPasswordLock behavior
                    break;
                case PortalMode.AllowedUsersOnly:
                    // Handle AllowedUsersOnly behavior
                    break;
                case PortalMode.TransportNetwork:
                    // Handle TransportNetwork behavior
                    break;
                case PortalMode.CordsPortal:
                    // Handle CordsPortal behavior
                    break;
            }
        }

        private bool IsAdmin()
        {
            return MagicPortalFluid.isAdmin;
        }

        public void SetPassword(string pwd)
        {
            if (!IsAdmin()) return;

            password = pwd;
            UpdatePortalYML();
        }

        public void SetCoordinates(Vector3 coords)
        {
            if (!IsAdmin()) return;

            targetCoordinates = coords;
            UpdatePortalYML();
        }

        public void AddAllowedUser(string userId)
        {
            if (!IsAdmin()) return;

            if (!allowedUsers.Contains(userId))
            {
                allowedUsers.Add(userId);
                UpdatePortalYML();
            }
        }

        private void UpdatePortalYML()
        {
            // Serialize the current state of the portal to YML
            string portalName = m_nview.GetZDO().GetString("tag");
            string zdoId = m_nview.GetZDO().ToString();

            if (PortalColorLogic.PortalN.Portals.ContainsKey(portalName))
            {
                var portal = PortalColorLogic.PortalN.Portals[portalName];

                if (portal.PortalZDOs.ContainsKey(zdoId))
                {
                    var zdo = portal.PortalZDOs[zdoId];
                    zdo.SpecialMode = (int)currentMode;
                    zdo.Password = password;
                    zdo.Coords = targetCoordinates.ToString();
                    // Serialize the YML
                    var serializer = new SerializerBuilder().Build();
                    var yml = serializer.Serialize(PortalColorLogic.PortalN);

                    PortalColorLogic.ClientORServerYMLUpdate(portal, portalName, "", 0 , true );
                }
            }
        }
            
       

        private bool CheckPassword(string inputPassword)
        {
            return inputPassword == password;
        }


        private void Teleport(Player player)
        {
            if (currentMode == PortalMode.CordsPortal)
            {
                player.TeleportTo(targetCoordinates, Quaternion.identity, true);
            }
            else if (currentMode == PortalMode.OneWay)
            {
                // Implement one-way teleportation logic
                Vector3 oneWayTarget = GetOneWayTarget();
                player.TeleportTo(oneWayTarget, Quaternion.identity, true);
            }
            else
            {
                // Implement other teleportation logic based on the currentMode
            }
        }

        private Vector3 GetOneWayTarget()
        {
            // Define the target location for one-way teleportation
            return new Vector3(100, 0, 100); // Example coordinates
        }

        public void LockToOneWay()
        {
            if (!IsAdmin()) return;

            SetMode(PortalMode.OneWay);
            // Additional logic to lock the portal to one-way mode if needed
        }



        public static void HandlePortalModeSelection(TeleportWorld portalInstance, Player player, PortalModeClass.PortalMode selectedMode, string extraInput)
        {
            switch (selectedMode)
            {
                case PortalModeClass.PortalMode.PasswordLock:
                    SetPasswordLockMode(portalInstance, extraInput);
                    break;
                case PortalModeClass.PortalMode.AllowedUsersOnly:
                    AddAllowedUser(portalInstance, player.GetPlayerName());
                    break;
                case PortalModeClass.PortalMode.CordsPortal:
                    if (TryParseCoordinates(extraInput, out Vector3 coordinates))
                    {
                        SetCoordinates(portalInstance, coordinates);
                    }
                    else
                    {
                        player.Message(MessageHud.MessageType.Center, "Invalid coordinates format. Please enter in x,y,z format.");
                    }
                    break;
                // Add other portal modes logic as needed
                default:
                    SetPortalMode(portalInstance, selectedMode);
                    break;
            }
        }

        private static void SetPasswordLockMode(TeleportWorld portalInstance, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Password cannot be empty.");
                return;
            }

            // Assuming the portalInstance has a reference to its PortalModeClass instance
            PortalModeClass portalModeClass = portalInstance.GetComponent<PortalModeClass>();
            if (portalModeClass != null)
            {
                portalModeClass.SetPassword(password);
                portalModeClass.SetMode(PortalModeClass.PortalMode.PasswordLock);
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Portal is now locked with a password.");
            }
        }

        private static void AddAllowedUser(TeleportWorld portalInstance, string userName)
        {
            // Assuming the portalInstance has a reference to its PortalModeClass instance
            PortalModeClass portalModeClass = portalInstance.GetComponent<PortalModeClass>();
            if (portalModeClass != null)
            {
                portalModeClass.AddAllowedUser(userName);
                portalModeClass.SetMode(PortalModeClass.PortalMode.AllowedUsersOnly);
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"User {userName} added to the allowed users list.");
            }
        }

        private static void SetCoordinates(TeleportWorld portalInstance, Vector3 coordinates)
        {
            // Assuming the portalInstance has a reference to its PortalModeClass instance
            PortalModeClass portalModeClass = portalInstance.GetComponent<PortalModeClass>();
            if (portalModeClass != null)
            {
                portalModeClass.SetCoordinates(coordinates);
                portalModeClass.SetMode(PortalModeClass.PortalMode.CordsPortal);
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"Portal coordinates set to {coordinates}.");
            }
        }

        private static bool TryParseCoordinates(string input, out Vector3 coords)
        {
            coords = Vector3.zero;
            string[] parts = input.Split(',');
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

        private static void SetPortalMode(TeleportWorld portalInstance, PortalModeClass.PortalMode mode)
        {
            // Assuming the portalInstance has a reference to its PortalModeClass instance
            PortalModeClass portalModeClass = portalInstance.GetComponent<PortalModeClass>();
            if (portalModeClass != null)
            {
                portalModeClass.SetMode(mode);
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, $"Portal mode set to {mode}.");
            }
        }
    }
}

