using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RareMagicPortalPlus.Patches
{
    internal class ships
    {

        [HarmonyPatch(typeof(Teleport), "OnTriggerEnter")]
        public class TeleportPatch
        {
            static void Prefix(Teleport __instance, Collider collider)
            {
                Player player = collider.GetComponent<Player>();
                if (player == null || Player.m_localPlayer != player)
                    return;

                Ship ship = ShipTeleportHelper.FindShip(player);
                if (ship != null)
                {
                    ShipTeleportHelper.StoreRelativePositions(ship);
                }
            }
        }

        [HarmonyPatch(typeof(Player), "TeleportTo")]
        public class PlayerTeleportPatch
        {
            static void Postfix(Player __instance, Vector3 pos, Quaternion rot, bool distantTeleport)
            {
                Ship ship = ShipTeleportHelper.FindShip(__instance);
                if (ship != null)
                {
                    ShipTeleportHelper.TeleportShip(ship, pos, rot);
                }
            }
        }

        [HarmonyPatch(typeof(Player), "UpdateTeleport")]
        public class UpdateTeleportPatch
        {
            static void Postfix(Player __instance, float dt)
            {
                if (!__instance.IsTeleporting())
                    return;

                Ship ship = ShipTeleportHelper.FindShip(__instance);
                if (ship != null)
                {
                    ShipTeleportHelper.UpdateTeleportShip(ship, __instance);
                }
            }
        }

        [HarmonyPatch(typeof(Ship), "Awake")]
        public class ShipAwakePatch
        {
            static void Postfix(Ship __instance)
            {
                ShipTeleportHelper.RegisterShip(__instance);
                ZLog.Log("Ship added");
            }
        }

        [HarmonyPatch(typeof(Ship), "OnDestroyed")]
        public class ShipDestroyPatch
        {
            static void Prefix(Ship __instance)
            {
                ShipTeleportHelper.UnregisterShip(__instance);
            }
        }

        public static class ShipTeleportHelper
        {
            private static List<Ship> ActiveShips = new List<Ship>();
            private static Dictionary<Player, Vector3> relativePositions = new Dictionary<Player, Vector3>();

            public static void RegisterShip(Ship ship)
            {
                if (!ActiveShips.Contains(ship))
                {
                    ActiveShips.Add(ship);
                }
            }

            public static void UnregisterShip(Ship ship)
            {
                ActiveShips.Remove(ship);
            }

            public static Ship FindShip(Player player)
            {
                foreach (Ship ship in ActiveShips)
                {
                    if (ship.IsPlayerInBoat(player))
                    {
                        return ship;
                    }
                }
                return null;
            }

            public static void StoreRelativePositions(Ship ship)
            {
                relativePositions.Clear();
                foreach (Player player in ship.m_players)
                {
                    relativePositions[player] = ship.transform.InverseTransformPoint(player.transform.position);
                }
            }

            public static void TeleportShip(Ship ship, Vector3 targetPos, Quaternion targetRotation)
            {
                // Define a safe offset distance from the portal
                float offsetDistance = 10f; // Adjust this value as needed

                // Compute an offset in the forward direction of the teleport destination
                Vector3 offset = targetRotation * Vector3.forward * offsetDistance;
                targetPos += offset;

                // Apply the teleportation
                ship.transform.position = targetPos;
                ship.transform.rotation = targetRotation;

                foreach (var kvp in relativePositions)
                {
                    kvp.Key.transform.position = ship.transform.TransformPoint(kvp.Value);
                }
            }


            public static void UpdateTeleportShip(Ship ship, Player player)
            {
                ship.transform.position = player.transform.position;
                ship.transform.rotation = player.transform.rotation;

                foreach (var kvp in relativePositions)
                {
                    kvp.Key.transform.position = ship.transform.TransformPoint(kvp.Value);
                }
            }
        }

    }
}
