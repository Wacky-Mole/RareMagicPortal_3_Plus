using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RareMagicPortal;
using BepInEx.Bootstrap;
using System.Reflection;
using RareMagicPortalPlus.Patches;
using UnityEngine;

namespace RareMagicPortal_3_Plus
{
    internal class TargetPortalPatches
    {

        [HarmonyPatch(typeof(Minimap), "SetMapMode")] // doesn't matter if targetportal is loaded or not
        public class LeavePortalModeOnMapCloseMagicPortal
        {
            internal static void Postfix(Minimap.MapMode mode)
            {
                if (mode != Minimap.MapMode.Large)
                {
                    MagicPortalFluid.Teleporting = false;
                }
            }
        }
        /*

        [HarmonyPatch(typeof(Game), nameof(Game.ConnectPortals))]
        [HarmonyPriority(Priority.HigherThanNormal)]
        private static class SkipPortalConnectingRMPOverride
        {
            internal class SkipPortalException5 : Exception { }
        }
        */


        [HarmonyPatch(typeof(Minimap), nameof(Minimap.OnMapLeftClick))]
        internal class MapLeftClickForRareMagic
        {
            internal class SkipPortalException2 : Exception { }

            [HarmonyPriority(Priority.HigherThanNormal)]
            internal static bool Prefix()
            {
                if (!MagicPortalFluid.Teleporting)
                {
                    return true;
                }

                // Check if TargetPortal mod is loaded
                if (!Chainloader.PluginInfos.ContainsKey("org.bepinex.plugins.targetportal"))
                {
                    return true;
                }
                string portalName = null;

                var activePins = functions.GetActivePins();

                foreach (Minimap.PinData pinData in activePins.Keys)
                {
                    pinData.m_save = true;
                }
                Minimap Minimap = Minimap.instance;
                Minimap.PinData? closestPin = Minimap.GetClosestPin(Minimap.ScreenToWorldPoint(Input.mousePosition), Minimap.m_removeRadius * (Minimap.m_largeZoom * 2f));

                foreach (Minimap.PinData pinData in activePins.Keys)
                {
                    pinData.m_save = false;
                }

                if (closestPin is null)
                {
                  //  MagicPortalFluid.RareMagicPortal.LogWarning("closeest is Null");
                    throw new SkipPortalException2();
                }

                if (!activePins.TryGetValue(closestPin, out ZDO portalZDO))
                {
                  //  MagicPortalFluid.RareMagicPortal.LogWarning("Try and Get Value");
                    throw new SkipPortalException2();
                }

                portalName = portalZDO.GetString("tag");

                if (portalName == null )
                {
                   // MagicPortalFluid.RareMagicPortal.LogWarning("PortalName = null or empty");
                    throw new SkipPortalException2(); // Stop TargetPortals from executing
                }
                
                Ship ship = Ships.ShipTeleportHelper.FindShip(Player.m_localPlayer);
                if (ship != null)
                {
                    if (!ship.IsOwner())
                    {
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Only the owner of Zone/ship can activate tp.");
                        throw new SkipPortalException2();
                    }
                    else
                    {
                        ZLog.Log("RMP TargetPortal with Ship Owner, ");
                        Vector3 position = portalZDO.GetPosition();
                        Quaternion rotation = portalZDO.GetRotation();
                        Vector3 vector = rotation * Vector3.forward;
                        Vector3 pos = position + vector * 1f + Vector3.up;
                        Ships.ShipTeleportHelper.ShipisTargetPortal = true;
                        Ships.BoatPlayerTP(ship, Player.m_localPlayer, position, rotation, vector,pos);
                        throw new SkipPortalException2();
                    }
                }

                if (!Player.m_localPlayer.IsTeleportable())
                {
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_noteleport");
                    return false;
                }

                if (PortalColorLogic.CrystalandKeyLogic(portalName, portalZDO.GetString(MagicPortalFluid._portalID)))
                {
                    ZLog.Log("Teleportation TRIGGER from TargetPortal, Passed RMP");
                    return true; // Allow TargetPortal to do its checks
                }
                else
                {
                   // MagicPortalFluid.RareMagicPortal.LogWarning("No Crystal Exception");
                    throw new SkipPortalException2(); // Stop TargetPortals from executing
                }          

            }

            internal static Exception? Finalizer(Exception __exception) => __exception is SkipPortalException2 ? null : __exception;
        }


        [HarmonyPatch(typeof(Minimap), nameof(Minimap.Start))]
        [HarmonyPriority(Priority.Low)]
        public class AddMinimapPortalIconRMP
        {
            internal static void Postfix(Minimap __instance)
            {
                // Make a shallow copy of the pins list
               // MagicPortalFluid.HoldPins = new List<Minimap.PinData>(Minimap.instance.m_pins);

            }
        }
    }
}
