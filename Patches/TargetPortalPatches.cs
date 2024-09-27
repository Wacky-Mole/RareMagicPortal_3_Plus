using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RareMagicPortal;
using BepInEx.Bootstrap;
using System.Reflection;
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

                try
                {
                    Minimap minimap = Minimap.instance;

                    // Get the TargetPortal.Map type
                    Type tpType = Type.GetType("TargetPortal.Map, TargetPortal");
                    if (tpType == null)
                    {
                        return true; // If the type cannot be found, do not interfere
                    }

                    // Get the 'activePins' property using reflection
                    PropertyInfo activePinsProperty = tpType.GetProperty("activePins", BindingFlags.NonPublic | BindingFlags.Static);
                    if (activePinsProperty == null)
                    {
                        return true; // If the property is not found, do not interfere
                    }

                    // Get the value of 'activePins'
                    var activePins = (Dictionary<Minimap.PinData, ZDO>)activePinsProperty.GetValue(null, null);

                    // Find the closest pin to the mouse position
                    Minimap.PinData? closestPin = minimap.GetClosestPin(minimap.ScreenToWorldPoint(Input.mousePosition), minimap.m_removeRadius * (minimap.m_largeZoom * 2f));
                    if (closestPin == null || !activePins.TryGetValue(closestPin, out ZDO portalZDO))
                    {
                        return true; // If no valid pin is found or it doesn't exist in activePins, skip further processing
                    }

                    // Get the portal name using your HandlePortalClick method
                    string portalName = null;
                    try
                    {
                        portalName = functions.HandlePortalClick();
                    }
                    catch
                    {
                        portalName = null;
                    }

                    if (portalName == null)
                    {
                        throw new SkipPortalException2(); // Stop TargetPortals from executing
                    }

                    if (!Player.m_localPlayer.IsTeleportable())
                    {
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_noteleport");
                        return false;
                    }

                    if (PortalColorLogic.CrystalandKeyLogic(portalName, portalZDO.ToString()))
                    {
                        return true; // Allow TargetPortal to do its checks
                    }
                    else
                    {
                        throw new SkipPortalException2(); // Stop TargetPortals from executing
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception for debugging purposes
                    // RMP.LogInfo($"Error while accessing activePins: {ex.Message}");
                    return true; // Ensure the original method runs if there is an error
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
                MagicPortalFluid.HoldPins = new List<Minimap.PinData>(Minimap.instance.m_pins);

                // RareMagicPortal.LogWarning("Here is MinimapStart");
            }
        }

        /* No hack anymore
        [HarmonyPatch(typeof(Minimap), nameof(Minimap.UpdatePins))]
        public class AddMinimapRewriteNames
        {
            internal static void Postfix(Minimap __instance)
            {
                if (__instance == null || __instance.m_pins == null)
                    return;

                foreach (Minimap.PinData pin in __instance.m_pins)
                {
                    if (pin.m_name != "TargetPortalIcon")
                        continue;

                    if (pin.m_name.Length > 0 && __instance.m_mode == Minimap.MapMode.Large)
                    {
                        string NewName = pin.m_name;

                        if (NewName.Contains(PortalColorLogic.NameIdentifier))
                        {
                            var index = NewName.IndexOf(PortalColorLogic.NameIdentifier);
                            NewName = NewName.Substring(0, index);
                        }

                        pin.m_NamePinData.PinNameText.name = Localization.instance.Localize(NewName); // doesn't work for some reason
                    }
                }
            }
        } */


    }
}
