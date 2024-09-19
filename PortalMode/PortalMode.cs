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
            
        public void CheckModes(Player player)
        {
            if (player == null || Player.m_localPlayer != player) return;

            switch (currentMode)
            {
                case PortalMode.PasswordLock:
                case PortalMode.OneWayPasswordLock:
                    inputPopup.ShowPopup("Enter Password:", (input) =>
                    {
                        if (!CheckPassword(input))
                        {
                            player.Message(MessageHud.MessageType.Center, "$msg_incorrectpassword");
                            return;
                        }
                        Teleport(player);
                    });
                    break;
                case PortalMode.AllowedUsersOnly:
                    // Check if the player's name is in the allowed users list
                    if (!allowedUsers.Contains(player.GetPlayerName()))
                    {
                        // Notify the player that they are not allowed to use the portal
                        player.Message(MessageHud.MessageType.Center, "$msg_notallowed");
                        return;
                    }

                    // Teleport the player if they are allowed
                    Teleport(player);
                    break;

                case PortalMode.CordsPortal:
                    if (IsAdmin())
                    {
                        inputPopup.ShowPopup("Enter Coordinates (x,y,z):", (input) =>
                        {
                            if (TryParseCoordinates(input, out Vector3 coords))
                            {
                                SetCoordinates(coords);
                                Teleport(player);
                            }
                            else
                            {
                                player.Message(MessageHud.MessageType.Center, "$msg_invalidcoordinates");
                            }
                        });
                    }
                    break;
                case PortalMode.TransportNetwork:
                    // Listen for chat messages
                    break;
                default:
                    Teleport(player);
                    break;
            }
        }

        private bool CheckPassword(string inputPassword)
        {
            return inputPassword == password;
        }

        private bool TryParseCoordinates(string input, out Vector3 coords)
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
    }
}
