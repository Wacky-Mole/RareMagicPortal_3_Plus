using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
                    if (IsTargetPortalModInstalled())
                    {
                        // Handle TargetPortal behavior
                    }
                    else
                    {
                        // Make portal invisible or private
                        gameObject.SetActive(false);
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
            // Implement your admin check logic here
            return true; // Placeholder for actual admin check
        }

        public void SetPassword(string pwd)
        {
            if (!IsAdmin()) return;

            password = pwd;
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
                    if (!CheckPassword(player))
                    {
                        player.Message(MessageHud.MessageType.Center, "$msg_incorrectpassword");
                        return;
                    }
                    break;
                case PortalMode.AllowedUsersOnly:
                    if (!IsUserAllowed(player.GetPlayerID()))
                    {
                        player.Message(MessageHud.MessageType.Center, "$msg_notallowed");
                        return;
                    }
                    break;
            }

            Teleport(player);
        }

        private bool CheckPassword(Player player)
        {
            // Implement password checking logic
            return true; // Placeholder
        }

        private void Teleport(Player player)
        {
            // Implement teleportation logic based on the currentMode
        }
    }

}
