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
                if (MagicPortalFluid.TargetPortalLoaded && !MagicPortalFluid.ConfigTargetPortalAnimation.Value)
                {
                    __result = false;
                    return false;
                }
                if (MagicPortalFluid.TargetPortalLoaded && MagicPortalFluid.ConfigTargetPortalAnimation.Value)
                {
                    __result = true;
                    return false;
                }
                return true;
            }
        }



        [HarmonyPatch(typeof(TeleportWorldTrigger), nameof(TeleportWorldTrigger.OnTriggerEnter))]  // for Crystals and Keys
        internal class TeleportWorld_Teleport_CheckforCrystal
        {
            internal class SkipPortalException : Exception
            {
            }

            //throw new SkipPortalException(); This is used for return false instead/ keeps other mods from loading patches.

            private static string OutsideP = null;

            [HarmonyPriority(Priority.HigherThanNormal)]
            internal static bool Prefix(TeleportWorldTrigger __instance, Collider colliderIn)
            {
                //finding portal name
                if (colliderIn.GetComponent<Player>() != Player.m_localPlayer)
                {
                    throw new SkipPortalException();
                }
                MagicPortalFluid.TeleportingforWeight = 1;

                //PortalColorLogic.player = collider.GetComponent<Player>();
                string PortalName = "";
                //if (!Chainloader.PluginInfos.ContainsKey("com.sweetgiorni.anyportal"))

                PortalName = __instance.m_teleportWorld.GetText();


                // end finding portal name
                MagicPortalFluid.m_hadTarget = __instance.m_teleportWorld.m_hadTarget;
                OutsideP = PortalName;
                // keep player and m_hadTarget for future patch for targetportal

                if (Chainloader.PluginInfos.ContainsKey("org.bepinex.plugins.targetportal"))
                {
                    MagicPortalFluid.Teleporting = true;
                    return true; // skip on checking because we don't know where this is going
                                 // we will catch in map for tele check
                }
                if (!MagicPortalFluid.m_hadTarget) // if no target continuie on with logic
                    return false;


                if (MagicPortalFluid.UsePortalProgression.Value)
                {
                    return true; // don't do crystalandkeylogic
                }
                
                if (PortalColorLogic.CrystalandKeyLogic(PortalName, __instance.m_teleportWorld.m_nview.m_zdo.ToString(), __instance.m_teleportWorld.m_nview.m_zdo.GetString(MagicPortalFluid._portalBiomeColorHashCode)))
                {
                    // Teleporting = true;
                    return true;
                }
                else // false never gets run
                {
                    MagicPortalFluid.Teleporting = false;
                    if (Chainloader.PluginInfos.ContainsKey("org.bepinex.plugins.targetportal")) // or any other mods that need to be skipped // this shoudn't be hit
                        throw new SkipPortalException();  // stops other mods from executing  // little worried about betterwards and loveisward
                    else return false;
                }

                //else return true;
            }

            internal static Exception? Finalizer(Exception __exception) => __exception is SkipPortalException ? null : __exception;

            [HarmonyPostfix]
            [HarmonyPriority(Priority.Low)]
            internal static void Postfix(TeleportWorldTrigger __instance)
            {
                if (MagicPortalFluid.Teleporting && Chainloader.PluginInfos.ContainsKey("org.bepinex.plugins.targetportal"))
                {
                    //RareMagicPortal.LogInfo($"Made it to Portal Trigger");
                    int colorint;
                    String PName;
                    String PortalName;
                    Minimap instance = Minimap.instance;
                    List<Minimap.PinData> paul = instance.m_pins;

                    //instance.ShowPointOnMap(__instance.transform.position);
                    try
                    {
                        //PortalName = HandlePortalClick(); //This is making minimap instance correctly
                    }
                    catch { }
                    //List<Minimap.PinData> paul = HoldPins;
                    foreach (Minimap.PinData pin in paul)
                    {
                        PName = null;
                        try
                        {
                            if (pin.m_icon.name == "TargetPortalIcon") // only selects correct icon now
                            {
                                PName = pin.m_name; // icons name - Portalname

                                string BiomeC = "";


                                Type TP = Type.GetType("TargetPortal.Map");
                                PropertyInfo myProperty = TP.GetProperty("activePins", BindingFlags.NonPublic | BindingFlags.Static);
                                Dictionary<Minimap.PinData, ZDO> activePins = (Dictionary<Minimap.PinData, ZDO>)myProperty.GetValue(null, null);

                                //Minimap.PinData? closestPin = Minimap.GetClosestPin(Minimap.ScreenToWorldPoint(Input.mousePosition), Minimap.m_removeRadius * (Minimap.m_largeZoom * 2f));

                                if (!activePins.TryGetValue(pin, out ZDO portalZDO))
                                {
                                    string hello = portalZDO.ToString();
                                }
                                //ZDO lookup
                                // Get Color for BiomeC

                                colorint = Int32.Parse(BiomeC);
                                colorint = PortalColorLogic.CrystalandKeyLogicColor(out string currentColor, out Color currentColorHex, out string nextcolor, PName); // kindof expensive task to do this cpu wize for all portals


                                if (colorint == 0 || colorint == 999)
                                    pin.m_icon = MagicPortalFluid.IconDefault;
                                else
                                {
                                    PortalColorLogic.PortalColor givemecolor = (PortalColorLogic.PortalColor)colorint;
                                    //RareMagicPortal.LogInfo(" Icon color here " + givemecolor.ToString());
                                    pin.m_icon = MagicPortalFluid.Icons[givemecolor.ToString()];
                                }

                                // pin.m_icon.name = "TargetPortalIcon"; 
                            }
                        }
                        catch { }
                    }// foreach
                }
            }
        }




    }
}
