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
        internal class MapLeftClickForRareMagic // for magic portal
        {
            internal class SkipPortalException2 : Exception // skip all other mods if targetportal is installed and passes everything else
            {
            }

            [HarmonyPriority(Priority.HigherThanNormal)]
            internal static bool Prefix()
            {
                if (!MagicPortalFluid.Teleporting)
                {
                    return true;
                }
                if (!Chainloader.PluginInfos.ContainsKey("org.bepinex.plugins.targetportal"))
                { // check to see if targetportal is loaded
                    return true;
                }
                //RareMagicPortal.LogInfo($"Made it to Map during Telecheck");
                string PortalName;
                Minimap Minimap = Minimap.instance;

                Type TP = Type.GetType("TargetPortal.Map");
                PropertyInfo myProperty = TP.GetProperty("activePins", BindingFlags.NonPublic | BindingFlags.Static);
                Dictionary<Minimap.PinData, ZDO> activePins = (Dictionary<Minimap.PinData, ZDO>)myProperty.GetValue(null, null);

                Minimap.PinData? closestPin = Minimap.GetClosestPin(Minimap.ScreenToWorldPoint(Input.mousePosition), Minimap.m_removeRadius * (Minimap.m_largeZoom * 2f));

                if (!activePins.TryGetValue(closestPin, out ZDO portalZDO))
                {
                    //use PortalName and ZDO to get portal color and data
                    string hello = portalZDO.ToString();
                }
                // portalZDO.m_uid
                // var portal = portalZDO.GetPrefab();


                MagicPortalFluid.HoldPins = Minimap.m_pins;
                //return true;

                try
                {
                    PortalName = functions.HandlePortalClick(); //my handleportal click
                }
                catch { PortalName = null; }
                if (PortalName == null)
                {
                    throw new SkipPortalException2();//return false; and stop TargetPortals from executing
                }
                if (!Player.m_localPlayer.IsTeleportable())
                {
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_noteleport");
                    return false;
                }

                if (MagicPortalFluid.UsePortalProgression.Value == MagicPortalFluid.Toggle.On)
                {
                    return true; // Don't do CrystalandKeyLogic check
                }

                if (PortalColorLogic.CrystalandKeyLogic(PortalName, portalZDO.ToString()))
                {
                    //RareMagicPortal.LogInfo($"True, so TargetPortalShould Take over");

                    //HandleTeleport(Instancpass);
                    return true; // allow TargetPortal to do it's checks
                                 //throw new SkipPortalException2();//return false; and stop TargetPortals from executing
                }
                else
                {
                    //RareMagicPortal.LogInfo($"TargetPortal is forced to stop");
                    throw new SkipPortalException2();//return false; and stop TargetPortals from executing
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
                MagicPortalFluid.HoldPins = Minimap.instance.m_pins;
                //RareMagicPortal.LogWarning("Here is MinimapStart");
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
