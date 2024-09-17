using BepInEx.Bootstrap;
using HarmonyLib;
using RareMagicPortal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace RareMagicPortal_3_Plus.Patches
{
    internal class TeleportWorldPatchs
    {

        [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.TargetFound))]
        public static class DisabldHaveTarget
        {
            internal static bool Prefix(ref bool __result)
            {
                if (Player.m_localPlayer == null)
                    return true;

                if (Player.m_localPlayer.m_seman.HaveStatusEffect("yippeTele".GetStableHashCode()))
                {
                    __result = true;
                    return false;
                }
                if (MagicPortalFluid.TargetPortalLoaded && MagicPortalFluid.ConfigTargetPortalAnimation.Value == MagicPortalFluid.Toggle.Off)
                {
                    __result = false;
                    return false;
                }
                if (MagicPortalFluid.TargetPortalLoaded && MagicPortalFluid.ConfigTargetPortalAnimation.Value == MagicPortalFluid.Toggle.On)
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }


        [HarmonyPatch(typeof(TeleportWorldTrigger), nameof(TeleportWorldTrigger.OnTriggerEnter))]
        internal class TeleportWorld_Teleport_CheckforCrystal
        {
            private static readonly string targetPortalPluginKey = "org.bepinex.plugins.targetportal";

            // Throw exception only if necessary to skip patches from other mods
            internal class SkipPortalException : Exception { }

            [HarmonyPriority(Priority.HigherThanNormal)]
            internal static bool Prefix(TeleportWorldTrigger __instance, Collider colliderIn)
            {
                // Early return if the collider is not the local player
                if (colliderIn.GetComponent<Player>() != Player.m_localPlayer)
                {
                    throw new SkipPortalException();
                }

                MagicPortalFluid.TeleportingforWeight = 1;
                string portalName = __instance.m_teleportWorld.GetText();
                MagicPortalFluid.m_hadTarget = __instance.m_teleportWorld.m_hadTarget;

                // If TargetPortal mod is loaded, handle with its logic
                bool targetPortalLoaded = Chainloader.PluginInfos.ContainsKey(targetPortalPluginKey);
                if (targetPortalLoaded)
                {
                    MagicPortalFluid.Teleporting = true;
                    return true; // Skip further checks, let TargetPortal handle it
                }

                // If there is no target or we use portal progression, proceed with custom logic
                if (!MagicPortalFluid.m_hadTarget || MagicPortalFluid.UsePortalProgression.Value == MagicPortalFluid.Toggle.On)
                {
                    return true;
                }

                // Check crystal and key logic
                if (PortalColorLogic.CrystalandKeyLogic(portalName, __instance.m_teleportWorld.m_nview.m_zdo.ToString(), __instance.m_teleportWorld.m_nview.m_zdo.GetString(MagicPortalFluid._portalBiomeColorHashCode)))
                {
                    return true;
                }
                else
                {
                    MagicPortalFluid.Teleporting = false;
                    if (targetPortalLoaded)
                    {
                        throw new SkipPortalException();
                    }
                    return false;
                }
            }

            internal static Exception? Finalizer(Exception __exception) => __exception is SkipPortalException ? null : __exception;

            [HarmonyPostfix]
            [HarmonyPriority(Priority.Low)]
            internal static void Postfix(TeleportWorldTrigger __instance)
            {
                if (MagicPortalFluid.Teleporting && Chainloader.PluginInfos.ContainsKey(targetPortalPluginKey))
                {
                    UpdatePortalIcons();
                }
            }

            // Helper method to update portal icons
            private static void UpdatePortalIcons()
            {
                try
                {
                    Minimap minimap = Minimap.instance;
                    List<Minimap.PinData> pins = minimap.m_pins;

                    // Get TargetPortal.Map's activePins property
                    var activePins = GetActivePins();
                    if (activePins == null) return;

                    foreach (Minimap.PinData pin in pins)
                    {
                        if (pin.m_icon.name == "TargetPortalIcon" && activePins.TryGetValue(pin, out ZDO portalZDO))
                        {
                            int colorint = PortalColorLogic.CrystalandKeyLogicColor(
                                out string currentColor,
                                out Color currentColorHex,
                                out string nextColor,
                                pin.m_name,
                                portalZDO.ToString()
                            );

                            // Update the icon based on the colorint
                            pin.m_icon = colorint == 0 || colorint == 999 ? MagicPortalFluid.IconDefault : MagicPortalFluid.Icons[((PortalColorLogic.PortalColor)colorint).ToString()];
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception for debugging purposes
                    // RMP.LogInfo($"Error in UpdatePortalIcons: {ex.Message}");
                }
            }

            // Helper method to get TargetPortal.Map's activePins property using reflection
            private static Dictionary<Minimap.PinData, ZDO>? GetActivePins()
            {
                try
                {
                    Type tpType = Type.GetType("TargetPortal.Map, TargetPortal");
                    if (tpType == null) return null;

                    PropertyInfo activePinsProperty = tpType.GetProperty("activePins", BindingFlags.NonPublic | BindingFlags.Static);
                    if (activePinsProperty == null) return null;

                    return (Dictionary<Minimap.PinData, ZDO>?)activePinsProperty.GetValue(null, null);
                }
                catch
                {
                    return null;
                }
            }
        }

    }
}
