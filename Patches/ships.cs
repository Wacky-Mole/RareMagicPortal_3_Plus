using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RareMagicPortal;

namespace RareMagicPortalPlus.Patches
{
    // neeed to check for multiple player and disable inventory checking
    internal class Ships
    {
        [HarmonyPatch(typeof(TeleportWorld), "Teleport")]
        public class TeleportPatch
        {
            static bool Prefix(TeleportWorld __instance, Player player)
            {
                return PlayerBoatChecker(__instance, player);
            }
        }

        internal static void HandleRemoteTeleport(long sender,Vector3 position)
        {
            
           // Player player = Player.m_localPlayer;
           // Ship ship = ShipTeleportHelper.FindShip(player);
            //ShipTeleportHelper.StoreRelativePositions(ship);
            //ZLog.Log("RMP Recieved Orders to Teleport Player with Ship Owner, " + ship.name);
           // player.TeleportTo(position, rotation, distance);
            MagicPortalFluid.context.StartCoroutine(ShipTeleportHelper.WaitForTeleportCompletion(position));
        }
    
        internal static bool PlayerBoatChecker(TeleportWorld __instance, Player player)
        {
            Ship ship = ShipTeleportHelper.FindShip(player);
            if (ship != null)
            {
                if (ship.IsOwner())
                {
                    ShipTeleportHelper.StoreRelativePositions(ship);
                    ZDO zDO = ZDOMan.instance.GetZDO(__instance.m_nview.GetZDO()
                        .GetConnectionZDOID(ZDOExtraData.ConnectionType.Portal));

                    Vector3 position = zDO.GetPosition();
                    Quaternion rotation = zDO.GetRotation();
                    Vector3 vector = rotation * Vector3.forward;
                    Vector3 pos = position + vector * __instance.m_exitDistance + Vector3.up;
                    //player.TeleportTo(pos, rotation, distantTeleport: true);
                    Game.instance.IncrementPlayerStat(PlayerStatType.PortalsUsed);

                    var randomMultiplier = UnityEngine.Random.Range(0.5f, 3f);
                    var offset = rotation * Vector3.forward *
                                 (MagicPortalFluid.wacky9_portalBoatOffset.Value * randomMultiplier);

                    ShipTeleportHelper.holdposition = pos + offset;

                    foreach (Player playertp in ship.m_players)
                    {
                        if (playertp == null)
                        {
                            ZLog.LogError("Null player found in ship.m_players!");
                            continue;
                        }
                        playertp.TeleportTo(pos + offset, rotation, true);
                        //var znetHold = ZNet.instance.GetPeerByPlayerName(playertp.GetPlayerName()); had problems with this approach
                        //ZRoutedRpc.instance.InvokeRoutedRPC(znetHold.m_uid, "RMPP Teleport Boat", ShipTeleportHelper.relativePositions[playertp]);
                        
                        /*
                        if (player == playertp)
                        {
                            
                        }
                        else
                        {
                            ZLog.Log("Sending RMP Teleporting Player " + playertp.GetPlayerName());
                            var znetHold = ZNet.instance.GetPeerByPlayerName(playertp.GetPlayerName());
                            ZRoutedRpc.instance.InvokeRoutedRPC(znetHold.m_uid, "RMPP Teleport Boat", pos + offset, rotation, true);
                        }   */
                        
                    }
                    
                    ZLog.Log("RMP Teleporting Ship, " + ship.name);
                    ShipTeleportHelper.TeleportShip(ship, pos + offset, rotation);

                    MagicPortalFluid.context.StartCoroutine(ShipTeleportHelper.WaitForTeleportCompletion());
                    return false;
                }
                else
                {
                   // ShipTeleportHelper.StoreRelativePositions(ship);
                    return false;
                }
            }
            return true;
        }
        

        public static class ShipTeleportHelper
        {
            internal static Dictionary<Player, Vector3> relativePositions = new Dictionary<Player, Vector3>();
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
                    //relativePosition.y = 0;
                    relativePositions[player] = relativePosition;
                   // ZLog.Log($"Stored relative position for {player.GetPlayerName()}: {relativePosition}");
      
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

            public static IEnumerator WaitForTeleportCompletion(Vector3 Relativeposition)
            {
                Player player = Player.m_localPlayer;
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
               // ZLog.Log("Hello WaterHeight "+ waveHeightTarget);

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
                  if (kvp.Key != player)
                       continue;
                   
                 //  ZLog.Log($"Player relative stored position {player.GetPlayerName()}: {vp}");
                   Vector3 adjustedPlayerPosition = shipFind.transform.TransformPoint(vp); // for relative offset fix
                   //kvp.Key.transform.position += offset; // for global offset
                   //adjustedPlayerPosition.y = waveHeightTarget + kvp.Value.y; // for wave height fix
                   // player.TeleportTo(adjustedPlayerPosition + ship.transform.position, ship.transform.rotation, false);
                   //Vector3 adjustedPlayerPosition = kvp.Key.transform.position + relativePositions[kvp.Key];
                   //ZLog.Log($" Current position for Ship: {shipFind.transform.position}");
                  // ZLog.Log($" Current position for Player: {player.transform.position}");
                  ZLog.Log($"Setting relative position for {player.GetPlayerName()}: {adjustedPlayerPosition}");

                   kvp.Key.transform.position = adjustedPlayerPosition;

               }
               relativePositions.Clear();
            }
        }
    }
}
