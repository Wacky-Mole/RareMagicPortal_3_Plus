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
                
                Ship ship = ShipTeleportHelper.FindShip(player);
                if (ship != null)
                {
                    ShipTeleportHelper.StoreRelativePositions(ship);
                    ZDO zDO = ZDOMan.instance.GetZDO(__instance.m_nview.GetZDO().GetConnectionZDOID(ZDOExtraData.ConnectionType.Portal));

                    Vector3 position = zDO.GetPosition();
                    Quaternion rotation = zDO.GetRotation();
                    Vector3 vector = rotation * Vector3.forward;
                    Vector3 pos = position + vector * __instance.m_exitDistance + Vector3.up;
                    //player.TeleportTo(pos, rotation, distantTeleport: true);
                    Game.instance.IncrementPlayerStat(PlayerStatType.PortalsUsed);
                    
                    var offset = rotation * Vector3.forward * MagicPortalFluid.wacky9_portalBoatOffset.Value;
                    ShipTeleportHelper.holdposition = pos + offset;
                    foreach (Player playertp in ship.m_players)
                    {
                        player.TeleportTo(pos + offset, rotation, true);
                    }
                    ZLog.Log("Teleporting Ship, " + ship.name);
                    ShipTeleportHelper.TeleportShip(ship, pos + offset, rotation);
                    
                   MagicPortalFluid.context.StartCoroutine(ShipTeleportHelper.WaitForTeleportCompletion(ship, player));
                    return false;
                }
                return true;
            }
        }
        

        public static class ShipTeleportHelper
        {
            private static Dictionary<Player, Vector3> relativePositions = new Dictionary<Player, Vector3>();
            private static Vector3 holdVelocity = Vector3.zero;
            private static Vector3 offset = Vector3.zero;
            internal static Vector3 holdposition = Vector3.zero;
 


            public static Ship FindShip(Player player)
            {
                Ship[] allShips = UnityEngine.Object.FindObjectsOfType<Ship>();
                foreach (Ship ship in allShips)
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
                    if (player == null)
                    {
                        ZLog.LogError("A player in the ship's list is null!");
                        continue;
                    }
                    Vector3 relativePosition = ship.transform.InverseTransformPoint(player.transform.position);
                    relativePosition.y = 0;
                    relativePositions[player] = relativePosition;
                    ZLog.Log($"Stored relative position for {player.GetPlayerName()}: {relativePosition}");
      
                }
            }

            public static void TeleportShip(Ship ship, Vector3 targetPos, Quaternion targetRotation)
            {
                // Define a safe offset distance from the portal
                offset = targetRotation * Vector3.forward * (MagicPortalFluid.wacky9_portalBoatOffset.Value + 1f);
                //targetPos += offset;
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
                holdVelocity = targetPos;
            }

            public static IEnumerator WaitForTeleportCompletion(Ship ship, Player player)
            {
                bool updateplayer = true;
                while (player.IsTeleporting())
                {
                    yield return new WaitForSeconds(0.5f);
                }

                while (Ship.GetLocalShip() == null)
                {
                    yield return new WaitForSeconds(0.5f);
                }
                
                WaterVolume targetWaterVolume = null;
                float waveHeightTarget = holdVelocity.y;
                ZLog.Log("Hello WaterHeight "+ waveHeightTarget);

                Ship shipFind = Ship.GetLocalShip(); //ShipTeleportHelper.FindShip(player);
                
                // Handle invalid values
                if (float.IsNaN(waveHeightTarget))
                {
                    waveHeightTarget = shipFind.transform.position.y;
                }
                
                
                if (shipFind == null)
                {
                    ZLog.LogError("Ship reference is null during player teleport adjustment!");
                    yield break;
                }
               foreach (var kvp in relativePositions)
               {
                   if (!relativePositions.TryGetValue(kvp.Key, out var vp))
                   {
                       ZLog.LogError($"Missing relative position for player: {kvp.Key.GetPlayerName()}");
                       continue;
                   }
                   
                   Vector3 adjustedPlayerPosition = shipFind.transform.TransformPoint(relativePositions[kvp.Key]); // for relative offset fix
                   //kvp.Key.transform.position += offset; // for global offset
                   //adjustedPlayerPosition.y = waveHeightTarget + kvp.Value.y; // for wave height fix
                   // player.TeleportTo(adjustedPlayerPosition + ship.transform.position, ship.transform.rotation, false);
                   //Vector3 adjustedPlayerPosition = kvp.Key.transform.position + relativePositions[kvp.Key];
                   ZLog.Log($" Current position for Ship: {shipFind.transform.position}");
                   ZLog.Log($"Setting relative position for {player.GetPlayerName()}: {adjustedPlayerPosition}");

                   kvp.Key.transform.position = adjustedPlayerPosition;

               }
               relativePositions.Clear();
            }
        }
    }
}
