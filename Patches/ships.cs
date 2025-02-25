using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RareMagicPortal;

namespace RareMagicPortalPlus.Patches
{
    internal class Ships
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
                if (__instance == null || __instance.IsDead()) // Ensure the player is valid
                    return;

                Ship ship = ShipTeleportHelper.FindShip(__instance);
                if (ship != null)
                {
                    ZLog.Log("Teleporting Ship, " + ship.name);
                    ShipTeleportHelper.TeleportShip(ship, pos, rot);
                    //MagicPortalFluid.context.StartCoroutine(ShipTeleportHelper.WaitForTeleportCompletion(ship, __instance));
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
                ZLog.Log("Ship removed");
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
                    if (ship.IsPlayerInBoat(player) && ship.HasPlayerOnboard())
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
                float offsetDistance = 40f; // Adjust as needed
                Vector3 offset = targetRotation * Vector3.forward * offsetDistance;
                targetPos += offset;
                //targetPos.x++;

                // Create a reference variable for WaterVolume
                WaterVolume targetWaterVolume = null;
                WaterVolume currentWaterVolume = null;
                
                // Get the wave height at the target position
                float waveHeightTarget = Floating.GetWaterLevel(targetPos, ref targetWaterVolume);
                float waveHeightCurrent = Floating.GetWaterLevel(ship.transform.position, ref currentWaterVolume);

                // Handle invalid values
                if (float.IsNaN(waveHeightTarget) || float.IsNaN(waveHeightCurrent))
                {
                    waveHeightTarget = targetPos.y;
                    waveHeightCurrent = ship.transform.position.y;
                }

                // Calculate height difference and prevent extreme shifts
                float heightDifference = Mathf.Clamp(waveHeightTarget - waveHeightCurrent, -5f, 5f);
                targetPos.y += heightDifference;

                ship.transform.position = targetPos;
                ship.transform.rotation = targetRotation;
            }

            public static IEnumerator WaitForTeleportCompletion(Ship ship, Player player)
            {
                while (player.IsTeleporting())
                {
                    yield return new WaitForSeconds(0.5f);
                }
                ZLog.Log("Hello Wait for Teleport");
                WaterVolume targetWaterVolume = null;
                float waveHeightTarget = Floating.GetWaterLevel(ship.transform.position, ref targetWaterVolume);
                ZLog.Log("Hello WaterHeight "+ waveHeightTarget);

                // Handle invalid values
                if (float.IsNaN(waveHeightTarget))
                {
                    waveHeightTarget = ship.transform.position.y;
                }

                foreach (var kvp in relativePositions)
                {
                    Vector3 adjustedPlayerPosition = ship.transform.TransformPoint(kvp.Value);
                    adjustedPlayerPosition.y = waveHeightTarget + kvp.Value.y;

                    kvp.Key.transform.position = adjustedPlayerPosition;
                }
                relativePositions.Clear();
            }
        }
    }
}
