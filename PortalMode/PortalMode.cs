using RareMagicPortal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

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
        private InputPopup inputPopup;
        private Dictionary<string, Vector3> transportLocations;

        private void Awake()
        {
            m_nview = GetComponent<ZNetView>();
            if (m_nview.GetZDO() == null)
            {
                enabled = false;
                return;
            }

            RegisterRPCMethods();
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

        private void RegisterRPCMethods()
        {
            m_nview.Register<int>("RPC_SetMode", RPC_SetMode);
            m_nview.Register<string>("RPC_SetPassword", RPC_SetPassword);
            m_nview.Register<Vector3>("RPC_SetCoordinates", RPC_SetCoordinates);
        }

        public void SetMode(PortalMode mode)
        {
            if (!IsAdmin()) return;

            currentMode = mode;
            m_nview.GetZDO().Set("PortalMode", (int)mode);
            m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetMode", (int)mode);
        }

        private void RPC_SetMode(long sender, int mode)
        {
            currentMode = (PortalMode)mode;
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
                        //Load Normal behavior if not installed
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
            m_nview.GetZDO().Set("PortalPassword", pwd);
            m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetPassword", pwd);
        }

        private void RPC_SetPassword(long sender, string pwd)
        {
            password = pwd;
        }

        public void SetCoordinates(Vector3 coords)
        {
            if (!IsAdmin()) return;

            targetCoordinates = coords;
            m_nview.GetZDO().Set("PortalCoordinates", coords);
            m_nview.InvokeRPC(ZNetView.Everybody, "RPC_SetCoordinates", coords);
        }

        private void RPC_SetCoordinates(long sender, Vector3 coords)
        {
            targetCoordinates = coords;
        }

        public void AddAllowedUser(string userId)
        {
            if (!IsAdmin()) return;

            if (!allowedUsers.Contains(userId))
            {
                allowedUsers.Add(userId);
                // Sync allowedUsers list across the network if needed
            }
        }

        public bool IsUserAllowed(string userId)
        {
            return allowedUsers.Contains(userId);
        }

        private void OnTriggerEnter(Collider collider)
        {
            Player player = collider.GetComponent<Player>();
            if (player == null || Player.m_localPlayer != player) return;

            switch (currentMode)
            {
                case PortalMode.PasswordLock:
                case PortalMode.OneWayPasswordLock:
                    inputPopup.ShowInputPopup("Enter Password:", (input) =>
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
                    if (!IsUserAllowed(player.GetPlayerName()))
                    {
                        player.Message(MessageHud.MessageType.Center, "$msg_notallowed");
                        return;
                    }
                    Teleport(player);
                    break;
                case PortalMode.CordsPortal:
                    if (IsAdmin())
                    {
                        inputPopup.ShowInputPopup("Enter Coordinates (x,y,z):", (input) =>
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
                    Chat.instance.m_onNewChatMessage += OnNewChatMessage;
                    break;
                default:
                    Teleport(player);
                    break;
            }
        }

        private void OnTriggerExit(Collider collider)
        {
            Player player = collider.GetComponent<Player>();
            if (player == null || Player.m_localPlayer != player) return;

            if (currentMode == PortalMode.TransportNetwork)
            {
                // Stop listening for chat messages
                Chat.instance.m_onNewChatMessage -= OnNewChatMessage;
            }
        }

        private void OnNewChatMessage(Talker.Type type, string user, string message)
        {
            if (type != Talker.Type.Normal || Player.m_localPlayer == null) return;

            // Check if the player is standing on the portal
            if (Vector3.Distance(Player.m_localPlayer.transform.position, transform.position) > 1.0f) return;

            // Check if the message matches a location name
            if (transportLocations.TryGetValue(message, out Vector3 targetLocation))
            {
                TeleportToLocation(Player.m_localPlayer, targetLocation);
            }
            else
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_invalidlocation");
            }
        }

        private void TeleportToLocation(Player player, Vector3 location)
        {
            player.TeleportTo(location, Quaternion.identity, true);
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
