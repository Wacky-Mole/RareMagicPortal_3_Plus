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
        [HarmonyPatch(typeof(TeleportWorld), "Teleport")]
        public class TeleportPatch
        {
            static bool Prefix(TeleportWorld __instance, Player player)
            {
                
                ZLog.Log("Hello Teleport player");
                if (player == null || Player.m_localPlayer != player)
                    return true;

                Ship ship = ShipTeleportHelper.FindShip(player);
                if (ship != null)
                {
                    ShipTeleportHelper.StoreRelativePositions(ship);
                    // I need to force teleport all players here and stop the trigger
                    //    if (!character.TeleportTo(this.m_targetPoint.GetTeleportPoint(), this.m_targetPoint.transform.rotation, false))
                    
                    ZDO zDO = ZDOMan.instance.GetZDO(__instance.m_nview.GetZDO().GetConnectionZDOID(ZDOExtraData.ConnectionType.Portal));

                    Vector3 position = zDO.GetPosition();
                    Quaternion rotation = zDO.GetRotation();
                    Vector3 vector = rotation * Vector3.forward;
                    Vector3 pos = position + vector * __instance.m_exitDistance + Vector3.up;
                    //player.TeleportTo(pos, rotation, distantTeleport: true);
                    Game.instance.IncrementPlayerStat(PlayerStatType.PortalsUsed);
                    
                    var offset = rotation * Vector3.forward * MagicPortalFluid.wacky9_portalBoatOffset.Value;
                    foreach (Player playertp in ship.m_players)
                    {
                        playertp.TeleportTo(pos + offset, rotation, true);
                        MagicPortalFluid.context.StartCoroutine(ShipTeleportHelper.WaitForTeleportCompletion(ship, player));
                        ZLog.Log("Starting timer");
                    }
                    return false;
                }
                return true;
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
                if (ship != null && ship.IsOwner())
                {
                    ZLog.Log("Teleporting Ship, " + ship.name);
                    ShipTeleportHelper.TeleportShip(ship, pos, rot);
                    
                }
            }
        }

        [HarmonyPatch(typeof(Ship), "Awake")]
        public class ShipAwakePatch
        {
            static void Postfix(Ship __instance)
            {
                ShipTeleportHelper.RegisterShip(__instance);
               // ZLog.Log("Ship added");
            }
        }

        [HarmonyPatch(typeof(Ship), "OnDestroyed")]
        public class ShipDestroyPatch
        {
            static void Prefix(Ship __instance)
            {
                ShipTeleportHelper.UnregisterShip(__instance);
               // ZLog.Log("Ship removed");
            }
        }

        public static class ShipTeleportHelper
        {
            private static List<Ship> ActiveShips = new List<Ship>();
            private static Dictionary<Player, Vector3> relativePositions = new Dictionary<Player, Vector3>();
            private static float waveHeightTargetHold = 0f;
            private static Vector3 holdVelocity = Vector3.zero;
            private static Vector3 offset = Vector3.zero;
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
                offset = targetRotation * Vector3.forward * MagicPortalFluid.wacky9_portalBoatOffset.Value;
                targetPos += offset;
                //targetPos.x++;

                // Create a reference variable for WaterVolume
                WaterVolume targetWaterVolume = null;
                WaterVolume currentWaterVolume = null;
                
                // Get the wave height at the target position
                float waveHeightTarget = Floating.GetWaterLevel(targetPos, ref targetWaterVolume);
                float waveHeightCurrent = Floating.GetWaterLevel(ship.transform.position, ref currentWaterVolume);
                waveHeightTargetHold = waveHeightTarget;

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
                holdVelocity = targetPos;
            }

            public static IEnumerator WaitForTeleportCompletion(Ship ship, Player player)
            {
                bool updateplayer = true;
                while (player.IsTeleporting())
                {
                    if (updateplayer)
                    {

                        updateplayer = false;
                    }
                    yield return new WaitForSeconds(0.5f);
                }
                ZLog.Log("Hello Wait for Teleport");
                WaterVolume targetWaterVolume = null;
                float waveHeightTarget = holdVelocity.y;
                ZLog.Log("Hello WaterHeight "+ waveHeightTarget);

                // Handle invalid values
                if (float.IsNaN(waveHeightTarget))
                {
                    waveHeightTarget = ship.transform.position.y;
                }

                foreach (var kvp in relativePositions)
                {
                    //kvp.Key.transform.position += offset; // for global offset
                    if(kvp.Key != player)
                        continue;
                    
                    Vector3 adjustedPlayerPosition = ship.transform.TransformPoint(kvp.Value); // for relative offset fix
                    adjustedPlayerPosition.y = waveHeightTarget + kvp.Value.y; // for wave height fix
                    
                   // player.TeleportTo(adjustedPlayerPosition + ship.transform.position, ship.transform.rotation, false);

                    kvp.Key.transform.position = adjustedPlayerPosition + offset;
                }
                relativePositions.Clear();
            }
        }
    }
}
