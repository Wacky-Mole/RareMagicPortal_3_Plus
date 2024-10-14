using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using RareMagicPortal.PortalWorld;
using RareMagicPortal_3_Plus.PortalMode;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using YamlDotNet.Serialization;
using static RareMagicPortal.PortalColorLogic;
using static RareMagicPortal.PortalName;
using static RareMagicPortal_3_Plus.TargetPortalPatches.MapLeftClickForRareMagic;
using Random = System.Random;

namespace RareMagicPortal
{
    internal class PortalColorLogic
    {
        // setups
        public static readonly ManualLogSource RMP =
            BepInEx.Logging.Logger.CreateLogSource(MagicPortalFluid.ModName);

        private static Color m_colorTargetfound = new Color(191f / 255f, 150f / 255f, 0, 25);
        private static Color lightcolor = new Color(1f, 100f / 255f, 0, 1f);

        //Material PortalDefMaterial = originalMaterials["portal_small"];
        public static Color flamesstart = new Color(1f, 194f / 255f, 34f / 255f, 1f);

        public static Color flamesend = new Color(1f, 0, 0, 1f);
        public static Color Gold = new Color(1f, 215f / 255f, 0, 1f);
        public static Color Purple = new Color(107f / 255f, 63f / 255f, 160f / 255f, 1f);
        public static Color Tan = new Color(210f / 255f, 180f / 255f, 140f / 255f, 1f);
        public static Color Brown = new Color(193f / 255f, 69f / 255f, 19f / 255f, 1f);
        public static Color Orange = new Color(204f / 255f, 85f / 255f, 0f, 1f);
        public static Color Cornsilk = new Color(1f, 248f / 255f, 220f / 255f, 1f);
        public static Color Yellow2 = new Color(139f / 255f, 128f / 255f, 0f, 1f);

        internal static PortalName PortalN;

        //internal static Player player = null; // need to keep it between patches
        private static int waitloop = 5;

        private static int rainbowWait = 0;
        private static string currentRainbow = "Yellow";
       // public static char NameIdentifier = '\u25B2';
        //private static string BiomeStringTempHolder = "";
        internal static bool reloaded = false;
        internal static Transform CheatswordColor;
        internal static bool inventoryRemove = false;
        internal static Dictionary<string, int> removeItems = new Dictionary<string, int>();

        public static ParticleSystem CheatSwordColor = null; //{ get; set; }// = new List<ParticleSystem>();

        internal enum PortalColor // gold - master should always be last or highest int
        {
            Yellow = 1,
            Red = 2,
            Green = 3,
            Blue = 4,
            Purple = 5,
            Tan = 6,
            Cyan = 7,
            Orange = 8,
            White = 20,
            Black = 21,
            Gold = 22,
        }

        public static Dictionary<string, (Color HexName, int Pos, bool Enabled, string NextColor, string MessageText)> PortalColors = new Dictionary<string, (Color, int, bool, string, string)>()
        {
            {nameof(PortalColor.Yellow),(Yellow2,(int)PortalColor.Yellow,         false, nameof(PortalColor.Red),   "Red Crystal Portal"  )},
            {nameof(PortalColor.Red), (Color.red,(int)PortalColor.Red,            false, nameof(PortalColor.Green), "Red Crystal Portal"  )},
            {nameof(PortalColor.Green), (Color.green,(int)PortalColor.Green,      false, nameof(PortalColor.Blue),  "Red Crystal Portal"  )},
            {nameof(PortalColor.Blue), (Color.blue,(int)PortalColor.Blue,         false, nameof(PortalColor.Purple),"Red Crystal Portal"  )},
            {nameof(PortalColor.Purple),( Purple,(int)PortalColor.Purple,         false, nameof(PortalColor.Tan),   "Red Crystal Portal"  )},
            {nameof(PortalColor.Tan), (Cornsilk,(int)PortalColor.Tan,             false, nameof(PortalColor.Cyan),  "Red Crystal Portal"  )},
            {nameof(PortalColor.Cyan), (Color.cyan,(int)PortalColor.Cyan,         false, nameof(PortalColor.Orange),"Red Crystal Portal"  )},
            {nameof(PortalColor.Orange),( Orange,(int)PortalColor.Orange,         false, nameof(PortalColor.White), "Red Crystal Portal"  )},
            {nameof(PortalColor.White), (Color.white,(int)PortalColor.White,      false, nameof(PortalColor.Black), "Red Crystal Portal"  )},
            {nameof(PortalColor.Black), (Color.black,(int)PortalColor.Black,      false, nameof(PortalColor.Gold),  "Red Crystal Portal"  )},
            {nameof(PortalColor.Gold), (Gold,(int)PortalColor.Gold,               false, nameof(PortalColor.Yellow),"Red Crystal Portal"  )}
        };

        internal static Dictionary<string, int> CrystalCount = new Dictionary<string, int>();
        internal static Dictionary<string, int> KeyCount = new Dictionary<string, int>();


        internal static KeyboardShortcut portalModeToggleModifierKey = new KeyboardShortcut(KeyCode.LeftShift); // default Target Portal
        public static void reloadcolors()
        {

            // Initialize CrystalCount and KeyCount for all PortalColors
            foreach (var color in PortalColors)
            {
                if (!CrystalCount.ContainsKey(color.Key))
                {
                    CrystalCount[color.Key] = 0;
                }
                if (!KeyCount.ContainsKey(color.Key))
                {
                    KeyCount[color.Key] = 0;
                }
            }

            // Reset all PortalColors to disabled
            foreach (var key in PortalColors.Keys.ToList())
            {
                var portalColor = PortalColors[key];
                portalColor.Enabled = false;
                PortalColors[key] = portalColor;
            }

            // Enable colors from configuration
            List<string> enabledColors = MagicPortalFluid.EnabledColors.Value.Split(',').Select(color => color.Trim()).ToList();
            foreach (var colorName in enabledColors)
            {
                if (PortalColors.ContainsKey(colorName))
                {
                    var portalColor = PortalColors[colorName];
                    portalColor.Enabled = true;
                    PortalColors[colorName] = portalColor;
                }
            }

            // Set the NextColor property for enabled PortalColors
            foreach (var colorKey in PortalColors.Keys.ToList())
            {
                var color = PortalColors[colorKey];
                if (color.Enabled)
                {
                    string currentColorName = colorKey;
                    PortalColor currentColorEnum = (PortalColor)Enum.Parse(typeof(PortalColor), currentColorName);

                    bool foundNext = false;
                    int loopCount = 0;

                    while (!foundNext && loopCount <= 30)
                    {
                        string nextColorName = currentColorEnum.Next().ToString();
                        if (PortalColors.TryGetValue(nextColorName, out var nextColor) && nextColor.Enabled)
                        {
                            color.NextColor = nextColorName;
                            foundNext = true;
                        }
                        else
                        {
                            currentColorEnum = (PortalColor)Enum.Parse(typeof(PortalColor), nextColorName);
                        }

                        loopCount++;
                    }

                    if (!foundNext)
                    {
                        RMP.LogWarning($"NextColor for {currentColorName} not found within 30 iterations");
                    }

                    PortalColors[colorKey] = color;
                }
            }
        }


        #region Patches

        [HarmonyPatch(typeof(TeleportWorld))] 
        private class TeleportWorldPatchRMP
        {   
            [HarmonyPostfix]
            [HarmonyPatch(nameof(TeleportWorld.Awake))]
            private static void TeleportWorldAwakepRfixRMP(ref TeleportWorld __instance)
            {
                if (!__instance)
                {
                    return;
                }
                if (!MagicPortalFluid._teleportWorldDataCache.ContainsKey(__instance))
                {
                    MagicPortalFluid._teleportWorldDataCache[__instance] = new TeleportWorldDataRMP(__instance);
                }             
                
                
                /*
                // Stone 'portal' prefab does not set this property.
                if (!__instance.m_proximityRoot)
                {
                    __instance.m_proximityRoot = __instance.transform;
                }

                // Stone 'portal' prefab does not set this property.
                if (!__instance.m_target_found)
                {
                    // The prefab does not have '_target_found_red' but instead '_target_found'.
                    GameObject targetFoundObject = __instance.gameObject.transform.Find("_target_found").gameObject;

                    // Disable the GameObject first, as adding component EffectFade calls its Awake() before being attached.
                    targetFoundObject.SetActive(false);
                    __instance.m_target_found = targetFoundObject.AddComponent<EffectFade>();
                    targetFoundObject.SetActive(true);
                }
                //RareMagicPortal.LogInfo("Adding Portal Awake for all Portals");
                */

            }

            [HarmonyPostfix]
            [HarmonyPriority(Priority.Low)]
            [HarmonyPatch(nameof(TeleportWorld.UpdatePortal))]
            private static void TeleportWorldUpdatePortalPostfixRMP(ref TeleportWorld __instance)
            {
                if (!__instance|| !__instance.m_nview|| __instance.m_nview.m_zdo == null)
                     return;
                                  
                try
                {
                    bool isthistrue = MagicPortalFluid._teleportWorldDataCache.TryGetValue(__instance, out TeleportWorldDataRMP teleportWorldData);
                    bool isplustrue = MagicPortalFluid._teleportWorldDataCacheDefault.TryGetValue(__instance, out ClassBase teleportWorldDataplus);

                    if (Player.m_localPlayer.m_seman.HaveStatusEffect("yippeTele".GetStableHashCode()))
                    {
                        if (isplustrue)
                            HandleYippe(teleportWorldDataplus);
                        else if (isthistrue)
                            HandleYippe(teleportWorldData);
                        

                        return;
                    } // end of yippe
                    else
                    {
                        //RMP.LogInfo("Hello you jerk");
                        string PortalName = __instance.m_nview.m_zdo.GetString("tag");
                        string zdoName = __instance.m_nview.GetZDO().GetString(MagicPortalFluid._portalID);

                        var portalData = PortalN.Portals[PortalName];
                        var zdoData = portalData.PortalZDOs[zdoName];

                        if(zdoData.SpecialMode == PortalModeClass.PortalMode.CordsPortal ||
                            zdoData.SpecialMode == PortalModeClass.PortalMode.TransportNetwork ||
                            zdoData.SpecialMode == PortalModeClass.PortalMode.RandomTeleport
                            )
                        {
                            __instance.m_hadTarget = true;
                        }


                        int colorint = CrystalandKeyLogicColor(out string currentcolor, out Color color, out string nextcolor, PortalName, "", __instance); // handles biomecolor now

                        if (isthistrue) {
                            if (color != teleportWorldData.LinkColor || color != teleportWorldData.OldColor)
                            {  // don't waste resources
                                teleportWorldData.TargetColor = color;
                                teleportWorldData.LinkColor = color;
                                SetTeleportWorldColors(teleportWorldData, true);
                            }
                        }
                        if (isplustrue)
                        {
                            if (color != teleportWorldDataplus.GetTargetColor() || color != teleportWorldDataplus.GetOldColor())
                            {  // don't waste resources
   
                                teleportWorldDataplus.SetTeleportWorldColors(color, true);
                            }
                        }
                    }
                }
                catch { } // catches beginning errors
            }
        }


        [HarmonyPriority(Priority.High)]
        [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.Interact))]
        public static class PortalCheckOutside
        {
            internal class SkipPortalException4 : Exception { }
            internal static bool Prefix(TeleportWorld __instance, Humanoid human, bool hold)

            {
                if (hold)
                    return false;

                if (__instance.m_nview.IsValid())
                {
                    //RareMagicPortal.LogInfo($"Made it to Map during Portal Interact");
                    Piece portal = null;
                    portal = __instance.GetComponent<Piece>();
                    string PortalName = __instance.m_nview.m_zdo.GetString("tag");
                    string zdoName = __instance.m_nview.GetZDO().GetString(MagicPortalFluid._portalID);
                    var oneportal = PortalN.Portals[PortalName].PortalZDOs[zdoName];

                    Player closestPlayer = Player.m_localPlayer; //Player.GetClosestPlayer(__instance.m_proximityRoot.position, 5f);
                    bool sameperson = false;
                    if (portal.m_creator == closestPlayer.GetPlayerID())
                        sameperson = true;

                    if (portal != null && PortalName != "" && PortalName != "Empty tag")
                    {
                        if (Input.GetKey(MagicPortalFluid.portalRMPMODEKEY.Value.MainKey) && MagicPortalFluid.portalRMPMODEKEY.Value.Modifiers.All(Input.GetKey) && (MagicPortalFluid.isAdmin )) // popup box
                        {
                            if (ModeSelectionPopup._popupInstance != null)
                            {
                                return false; // Prevent the default interaction
                            }
                            int colorint = CrystalandKeyLogicColor(out string currentcolor, out Color color, out string nextcolorskip, PortalName, "", __instance);

                            ModeSelectionPopup popup = closestPlayer.GetComponent<ModeSelectionPopup>() ?? closestPlayer.gameObject.AddComponent<ModeSelectionPopup>();
                            popup.ShowModeSelectionPopup((selectedMode, PopInstance) =>
                            {
                                PortalModeClass.HandlePortalModeSelection(__instance, closestPlayer, selectedMode,  PopInstance);
                            }, currentcolor, PortalName, zdoName);
                             return false; // Prevent the default interaction
                        }

                        if (Input.GetKey(MagicPortalFluid.portalRMPKEY.Value.MainKey) && MagicPortalFluid.portalRMPKEY.Value.Modifiers.All(Input.GetKey) && CanChangePortalColor( MagicPortalFluid.isAdmin, sameperson, oneportal))
                        {
                            MagicPortalFluid.Globaliscreator = sameperson; // set this for yml permissions

                            int colorint = CrystalandKeyLogicColor(out string currentcolorskip, out Color colorskip, out string nextcolorskip, PortalName, "", __instance);
                            colorint = PortalColors[nextcolorskip].Pos; // inc 1 color// should loop around for last one
                            Color setcolor = PortalColors[nextcolorskip].HexName; // inc for hexname

                            if (MagicPortalFluid._teleportWorldDataCache.TryGetValue(__instance, out TeleportWorldDataRMP teleportWorldData))
                            {
                                teleportWorldData.TargetColor = setcolor;
                                RMP.LogInfo("setting color " + setcolor);

                                teleportWorldData.LinkColor = setcolor;
                                SetTeleportWorldColors(teleportWorldData, true);
                            }

                            //__instance.m_nview.m_zdo.Set(MagicPortalFluid._teleportWorldColorHashCode, Utils.ColorToVec3(setcolor));
                            //__instance.m_nview.m_zdo.Set(_teleportWorldColorAlphaHashCode, color);
                            //__instance.m_nview.m_zdo.Set(MagicPortalFluid._portalLastColoredByHashCode, Player.m_localPlayer?.GetPlayerID() ?? 0L);
                            //__instance.m_nview.m_zdo.Set(MagicPortalFluid._portalBiomeColorHashCode, "skip");
                            //RMP.LogInfo("New color int " + colorint);

                            if (MagicPortalFluid.ConfigUseBiomeColors.Value == MagicPortalFluid.Toggle.On)
                                oneportal.BiomeColor = "skip";

                            updateYmltoColorChange(PortalName, colorint, zdoName); // update yaml
                                                                                    //colorint = CrystalandKeyLogicColor(out string currentcolor, out Color color, out string nextcolor, PortalName, "", __instance);// Do this again now that it has been updated. // why do this again?

                            return false; // stop interaction on changing name
                            
                        }
                        if (Input.GetKey(portalModeToggleModifierKey.MainKey) && oneportal.SpecialMode == PortalModeClass.PortalMode.TargetPortal)
                        {
                            if (MagicPortalFluid.PreventTargetPortalFromChanging.Value == MagicPortalFluid.Toggle.On && !sameperson)
                                throw new SkipPortalException4();

                            if (MagicPortalFluid.PreventTargetPortalOwnerFromChanging.Value == MagicPortalFluid.Toggle.On && !MagicPortalFluid.isAdmin)
                                throw new SkipPortalException4();

                            return true;
                        }
                    }

                    if (Input.GetKey(portalModeToggleModifierKey.MainKey) && oneportal.SpecialMode == PortalModeClass.PortalMode.TargetPortal)
                    {
                        if (MagicPortalFluid.PreventTargetPortalFromChanging.Value == MagicPortalFluid.Toggle.On && !sameperson)
                            throw new SkipPortalException4();

                        if (MagicPortalFluid.PreventTargetPortalOwnerFromChanging.Value == MagicPortalFluid.Toggle.On && !MagicPortalFluid.isAdmin)
                            throw new SkipPortalException4();
                    }

                    if (sameperson || !sameperson && MagicPortalFluid.ConfigCreatorLock.Value == MagicPortalFluid.Toggle.Off || closestPlayer.m_noPlacementCost) // Only creator || not creator and not in lock mode || not in noplacementcost mode
                    {
                        return true;
                    }
                    else
                    {
                        human.Message(MessageHud.MessageType.Center, "$rmp_onlyownercanchange");
                        return false;
                    }
                }               
                return true;
            }
            internal static Exception? Finalizer(Exception __exception) => __exception is SkipPortalException4 ? null : __exception;
        }

        [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.RPC_SetTag))]
        public static class PortalCheckRename
        {
            internal static void Postfix(TeleportWorld __instance)
            {
                if (__instance.m_nview.IsValid() && __instance.m_nview.IsOwner())
                {
                    var name = __instance.m_nview.m_zdo.GetString("tag");
                    var check = __instance.m_nview.m_zdo.GetString(MagicPortalFluid._portalLastName);
                    var zdoname = __instance.m_nview.GetZDO().GetString(MagicPortalFluid._portalID);
                    if (name != check)
                    {
                        __instance.m_nview.m_zdo.Set(MagicPortalFluid._portalLastName, name);
                        __instance.m_nview.m_zdo.Set(MagicPortalFluid._portalZdo, zdoname);
                        // RMP.LogWarning("name " + name + " old name " + check);

                        if (check != "")
                        {
                            if (!PortalN.Portals.ContainsKey(name))
                            {
                                WritetoYML(name, zdoname, check);
                            }

                            if (!PortalN.Portals[name].PortalZDOs.ContainsKey(zdoname))
                            {
                                WritetoYML(name, zdoname, check, true);
                            }
                        }
                        else
                        {
                            if (!PortalN.Portals.ContainsKey(name))
                            {
                                WritetoYML(name, zdoname);
                            }

                            if (!PortalN.Portals[name].PortalZDOs.ContainsKey(zdoname))
                            {
                                WritetoYML(name, zdoname, "", true);
                            }
                        }
                    }
                }
            }
        }

       [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.GetHoverText))]
        public static class TeleportWorldGetHoverTextPostfixRMP
        {
            internal class SkipPortalException3 : Exception { }

            [HarmonyPriority(Priority.HigherThanNormal)]
            private static void Postfix(ref TeleportWorld __instance, ref string __result)
            {
                if (!__instance || MagicPortalFluid.NoMoreLoading || MagicPortalFluid.WaitSomeMore)
                {
                    return;
                }

                Piece portal = __instance.GetComponent<Piece>();
                Player closestPlayer = Player.m_localPlayer;
                bool isCreator = portal.m_creator == closestPlayer.GetPlayerID();
                string portalName = __instance.m_nview.GetZDO().GetString("tag");

                string zdoName = __instance.m_nview.GetZDO().GetString(MagicPortalFluid._portalID);
                if (zdoName == "")
                {
                    zdoName = CreatePortalID(portalName);
                    __instance.m_nview.GetZDO().Set(MagicPortalFluid._portalID, zdoName );
                    string ply = Player.GetPlayer(portal.m_creator)?.m_name;
                    if (ply == null)
                        ply = "";
                    PortalColorLogic.RMP.LogInfo(" Portal creator name is " + ply);

                    if (!PortalN.Portals.ContainsKey(portalName))
                    {
                        WritetoYML(portalName, zdoName);
                    }

                    if (!PortalN.Portals[portalName].PortalZDOs.ContainsKey(zdoName))
                    {
                        WritetoYML(portalName, zdoName, "", true, __instance, ply);
                    }
                }
                
               // RMP.LogInfo("ZDO name " + zdoName);

                int colorIndex = PortalColorLogic.CrystalandKeyLogicColor(
                    out string currentColor, out Color currentColorHex, out string nextColor, portalName, zdoName, __instance);

                // Helper
                var portaL = PortalColorLogic.PortalN.Portals[portalName];
                var portalZDO = portaL.PortalZDOs[zdoName];

                // Handle colors and progression
                if (currentColor == MagicPortalFluid.FreePassageColor.Value &&
                    MagicPortalFluid.ConfigUseBiomeColors.Value == MagicPortalFluid.Toggle.Off &&
                    !PortalColorLogic.PortalN.Portals[portalName].Free_Passage &&
                    MagicPortalFluid.JustSent == 0)
                {
                    PortalColorLogic.updateYmltoColorChange(portalName,
                        PortalColorLogic.PortalColors[MagicPortalFluid.FreePassageColor.Value].Pos,
                        zdoName);
                }

                if (portalName == "" && PortalColorLogic.reloaded)
                {
                    PortalColorLogic.RMP.LogInfo("Updating Blank Portals");
                    PortalColorLogic.updateYmltoColorChange("", colorIndex, zdoName);
                    PortalColorLogic.reloaded = false;
                }

                // Update hover text
                Color color = currentColorHex;
                

                if (portalName == "" && currentColor != MagicPortalFluid.DefaultColor.Value && MagicPortalFluid.JustSent == 0)
                {
                    colorIndex = PortalColorLogic.PortalColors.ContainsKey(MagicPortalFluid.DefaultColor.Value)
                        ? PortalColorLogic.PortalColors[MagicPortalFluid.DefaultColor.Value].Pos
                        : 1;
                }
              
                if ( __instance.m_nview.m_zdo.GetString(MagicPortalFluid._portalBiomeHashCode) == "" || string.IsNullOrEmpty(portalZDO.Biome) )
                {
                    string biome = closestPlayer.GetCurrentBiome().ToString();
                    portalZDO.Biome = biome;
                    if ( MagicPortalFluid.ConfigUseBiomeColors.Value == MagicPortalFluid.Toggle.On)
                    {
                        portalZDO.BiomeColor = functions.GetBiomeColor(biome);
                        colorIndex = PortalColors[portalZDO.BiomeColor].Pos;

                    } else
                    {
                        //PortalN.Portals[portalName].PortalZDOs[zdoName].BiomeColor = "skip"; // They only should get skip if they have been changed after the fact while in BiomeColor Mode
                    }

                    // __instance.m_nview.m_zdo.Set(MagicPortalFluid._portalLastColoredByHashCode, Player.m_localPlayer?.GetPlayerID() ?? 0L);
                    __instance.m_nview.m_zdo.Set(MagicPortalFluid._portalBiomeHashCode, biome);

                    PortalColorLogic.RMP.LogInfo("Setting ZDO Biome Data For First Time");

                    if (portalName == "")
                        PortalColorLogic.updateYmltoColorChange("", colorIndex, zdoName);
                    else
                        ClientORServerYMLUpdate(portaL, portalName, zdoName, colorIndex);
                }

                bool tagisset = portalZDO.ShowName; // PopUp Hover Text 
                if (tagisset)
                {
                    string hovertag = Localization.instance.Localize(string.Concat("$piece_portal $piece_portal_tag:", " ",
                        "[", portalName, "]"));
                    if (portalZDO.SpecialMode == PortalModeClass.PortalMode.RandomTeleport)
                        hovertag = "Danger, Danger: Random Teleport";
                    __result = hovertag;
                    MessageHud.instance.ShowMessage(MessageHud.MessageType.Center, hovertag, 1);
                }
                var portalData = PortalN.Portals[portalName];
                var zdoData = portalData.PortalZDOs[zdoName];
                //var currentmode = $"<color=#" + ColorUtility.ToHtmlStringRGB(currentColorHex) + ">" + portalZDO.SpecialMode.ToString() + " Mode</color>";
                var currentmode = portalZDO.SpecialMode.ToString() + " Mode";
                string Crystaltext = currentColor + " Portal";               
                if (zdoData.CrystalActive)                
                    Crystaltext = currentColor + " Crystal Portal";
             
                //Crystaltext = "<color=#" + ColorUtility.ToHtmlStringRGB(currentColorHex) + ">" + Crystaltext + "</color>";

                if (!string.IsNullOrEmpty(portalData.GuildOnly))
                {
                    currentmode = $"{portalData.GuildOnly} Only";
                }
                //__result = __result.Replace(Localization.instance.Localize("$piece_portal_connected"), mode + (mode is PortalMode.Public or PortalMode.Admin ? "" : $"
                //(Owner: {__instance.m_nview.GetZDO().GetString("TargetPortal PortalOwnerName")})")) + $"\n[<b><color=yellow>{portalModeToggleModifierKey.Value}</color> + <color=yellow>{Localization.instance.Localize("$KEY_Use")}</color></b>] Toggle Mode";
                UpdateHoverText(ref __result, currentColor, nextColor, isCreator, currentColorHex, portalName, Crystaltext, zdoData, currentmode);

                if (portalZDO.SpecialMode != PortalModeClass.PortalMode.TargetPortal && MagicPortalFluid.TargetPortalLoaded)
                {
                    throw new SkipPortalException3(); // stops targetportal
                }
            }

            private static void UpdateHoverText(ref string hoverText, string currentColor, string nextColor, bool isCreator, Color currentColorHex, string portalName, string Crystaltext, ZDOP portal, string currentmode)
            {
                if (portalName != "" && portalName != "Empty tag")
                {
                   // if (MagicPortalFluid.isAdmin || (isCreator && !portal.CrystalActive ))
                    
                    if (MagicPortalFluid.portalRMPKEY.Value.MainKey is KeyCode.None)
                    {
                        return;
                    }
                    string adminstring = "";
                    string crystalString = "";
                    string creatorChange = "";

                    
                    string newhoverText = $"\"{portalName}\"";
                    if (MagicPortalFluid.ConfigCreatorLock.Value == MagicPortalFluid.Toggle.On)
                    {
                        if (isCreator || MagicPortalFluid.isAdmin)
                        {

                        }else
                        {
                            hoverText = newhoverText;
                        }
                    }


                    if (MagicPortalFluid.isAdmin)
                    {
                        adminstring = "[<color=#" + ColorUtility.ToHtmlStringRGB(Color.yellow) + ">" + MagicPortalFluid.portalRMPMODEKEY.Value + " + " + "E</color>] Open Portal Mode UI ";
                    }

                    crystalString = $"<size=15><color=#{ColorUtility.ToHtmlStringRGB(currentColorHex)}>" + Crystaltext +"</color></size>";
                    
                    if (isCreator && !portal.CrystalActive && MagicPortalFluid.ConfigPreventCreatorsToChangeBiomeColor.Value == MagicPortalFluid.Toggle.Off || MagicPortalFluid.isAdmin)
                    {
                        creatorChange = $"<size={15}>[<color={"#" + ColorUtility.ToHtmlStringRGB(Color.yellow)}>{MagicPortalFluid.portalRMPKEY.Value +"+" + "E" }</color>] " +
                            $"Change <color=#{ColorUtility.ToHtmlStringRGB(currentColorHex)}>[{currentColor}]</color> to: " +
                            $"[<color=#{ColorUtility.ToHtmlStringRGB(PortalColorLogic.PortalColors[nextColor].HexName)}>{nextColor}</color>]</size>";
                    }

                    hoverText = string.Format(
                        "{0}\n{1} \n{2}\n{3}\n{4}",
                        hoverText,
                        creatorChange,
                        crystalString,
                        currentmode,
                        adminstring
                        );

                        /*
                        if (MagicPortalFluid.isAdmin)
                        {
                            hoverText = string.Format(
                                "{0}\n<size={4}>[<color={5}>{2}</color>] Change <color={7}>Portal</color>[{1}] Crystal to: [<color={8}>{3}</color>]</size>{6}\n{9} {10}",
                                hoverText,
                                currentColor,
                                MagicPortalFluid.portalRMPKEY.Value + " + " + "E",
                                nextColor,
                                15,
                                "#" + ColorUtility.ToHtmlStringRGB(Color.yellow),
                                crystalString,
                                "#" + ColorUtility.ToHtmlStringRGB(currentColorHex),
                                "#" + ColorUtility.ToHtmlStringRGB(PortalColorLogic.PortalColors[nextColor].HexName),
                                currentmode,
                                adminstring

                            );
                        }

                        else if  ( MagicPortalFluid.ConfigUseBiomeColors.Value == MagicPortalFluid.Toggle.On && MagicPortalFluid.ConfigPreventCreatorsToChangeBiomeColor.Value == MagicPortalFluid.Toggle.On ) // prevent them from changing
                        {
                            hoverText = string.Format(
                                "{0}\n<size={2}><color={4}>{1} Portal</color></size>\n{5}",
                                hoverText,
                                currentColor,
                                15,
                                "#" + ColorUtility.ToHtmlStringRGB(Color.yellow),
                                "#" + ColorUtility.ToHtmlStringRGB(currentColorHex),
                                currentmode
                            );
                        }
                        else
                        {
                            hoverText = string.Format(
                                "{0}\n<size={4}>[<color={5}>{2}</color>] Change <color={6}>Portal</color>[{1}] Color to: [<color={7}>{3}</color>] </size>\n{8}",
                                hoverText,
                                currentColor,
                                MagicPortalFluid.portalRMPKEY.Value + " + " + "E",
                                nextColor,
                                15,
                                "#" + ColorUtility.ToHtmlStringRGB(Color.yellow),
                                "#" + ColorUtility.ToHtmlStringRGB(currentColorHex),
                                "#" + ColorUtility.ToHtmlStringRGB(PortalColorLogic.PortalColors[nextColor].HexName),
                                currentmode
                                
                            );
                        }
                    }
                    else
                    {
                        if (portal.CrystalActive)
                        {
                            hoverText = string.Format(
                                "{0}\n<size={2}><color={5}>{1} Portal</color></size>\n<size={2}>{4}</size>\n{6}",
                                hoverText,
                                currentColor,
                                15,
                                "#" + ColorUtility.ToHtmlStringRGB(Color.yellow), 
                                Crystaltext,
                                "#" + ColorUtility.ToHtmlStringRGB(currentColorHex),
                                currentmode
                            );
                        }
                        else
                        {
                            hoverText = string.Format(
                                "{0}\n<size={2}><color={4}>{1} Portal</color></size>\n{5}",
                                hoverText,
                                currentColor,
                                15,
                                "#" + ColorUtility.ToHtmlStringRGB(Color.yellow),
                                "#" + ColorUtility.ToHtmlStringRGB(currentColorHex),
                                currentmode
                            );
                        }
                    }
                        */
                }
                else
                {
                    string jo = "Please name Portal, ";
                    string hi = " Color ";
                    hoverText = string.Format(
                        "{0}\n<size={1}><color={3}>{2}{5}</color><color={3}>{6}</color></size>\n{7}",
                        hoverText,
                        15,
                        jo,
                        "#" + ColorUtility.ToHtmlStringRGB(Color.yellow),
                        "#" + ColorUtility.ToHtmlStringRGB(currentColorHex),
                        hi,
                        currentColor,
                        currentmode

                    );
                }
            }

            internal static Exception? Finalizer(Exception __exception) => __exception is SkipPortalException3 ? null : __exception;
        }


        #endregion Patches

        internal static void BiomeLogicCheck(out string currentColor, out Color currentColorHex, out string nextcolor, out int Pos, string PortalName = "", bool skip = false) // for the ones that don't have an __instance
        {
            string BiomeC = "";
            currentColor = "skip";
            currentColorHex = Gold;
            nextcolor = "White";
            Pos = 0;

            if (BiomeC != "" && MagicPortalFluid.ConfigUseBiomeColors.Value == MagicPortalFluid.Toggle.On && !skip)
            {
                // RMP.LogInfo("BiomeC info is " + BiomeC);
                BiomeC = BiomeC.Remove(0, 1);
                int intS = Int32.Parse(BiomeC);
                PortalColor pcol = (PortalColor)intS;
                currentColor = pcol.ToString();
                //RMP.LogInfo("BiomeC Colar is " + currentColor);
                currentColorHex = PortalColors[currentColor].HexName;
                nextcolor = PortalColors[currentColor].NextColor;
                Pos = PortalColors[currentColor].Pos;
            }


        }

        internal static int CrystalandKeyLogicColor(out string currentColor, out Color currentColorHex, out string nextColor, string portalName, string zdoName = "", TeleportWorld instance = null, int overrideInt = 0)
        {

            int crystalCost = MagicPortalFluid.ConfigCrystalsConsumable.Value;

            if (zdoName == "")
                zdoName = instance.m_nview.GetZDO().GetString(MagicPortalFluid._portalID);

            Piece piece = null;
            if (instance != null)
                 piece = instance.GetComponent<Piece>();

            if (!PortalN.Portals.ContainsKey(portalName))
            {
                WritetoYML(portalName, zdoName);
            }

            if (!PortalN.Portals[portalName].PortalZDOs.ContainsKey(zdoName))
            {
                WritetoYML(portalName, zdoName, "", true, instance);
            }

            var portalData = PortalN.Portals[portalName];
            var zdoData = portalData.PortalZDOs[zdoName];



            // Check if the portal is under special conditions like admin access or free passage
            if (portalName != "" && portalName != "Empty tag")
            {

                // Check for free passage
                if (portalData.Free_Passage && MagicPortalFluid.FreePassageColor.Value != "none")
                {
                    currentColor = MagicPortalFluid.FreePassageColor.Value;
                    currentColorHex = PortalColors[currentColor].HexName;
                    nextColor = PortalColors[currentColor].NextColor;
                    return PortalColors[currentColor].Pos;
                }
            }

            if (MagicPortalFluid.ConfigUseBiomeColors.Value == MagicPortalFluid.Toggle.On && !string.IsNullOrEmpty(zdoData.Biome) && zdoData.BiomeColor != "skip")
            {
               // RMP.LogInfo("Use BiomeColors");
                string BioColor = "";
                if (zdoData.BiomeColor == "")
                {
                    string colorme = functions.GetBiomeColor(zdoData.Biome);
                    zdoData.BiomeColor = colorme;
                    BioColor = colorme;
                }
                if (BioColor == "")
                    BioColor = zdoData.BiomeColor;

                currentColor = BioColor;
                currentColorHex = PortalColors[currentColor].HexName;
                nextColor = PortalColors[currentColor].NextColor;
                return PortalColors[currentColor].Pos;

            }

            // Fallback to default or free passage color if the portal name is empty
            if (portalName == "")
            {
               // RMP.LogInfo("Name is empty");
                if (MagicPortalFluid.DefaultColor.Value == "None" || MagicPortalFluid.DefaultColor.Value == "none")
                {
                    currentColor = "Yellow";
                    currentColorHex = PortalColors[currentColor].HexName;
                    nextColor = PortalColors[currentColor].NextColor;
                    return PortalColors[currentColor].Pos;
                }

                currentColor = MagicPortalFluid.DefaultColor.Value;
                currentColorHex = PortalColors[currentColor].HexName;
                nextColor = PortalColors[currentColor].NextColor;
                return PortalColors[currentColor].Pos;
            }

            // need to decide for single or grouped portal colors now
            bool grouped = true;
            if (portalData.SpecialMode == PortalModeClass.PortalMode.TargetPortal || portalData.SpecialMode == PortalModeClass.PortalMode.RandomTeleport || portalData.SpecialMode == PortalModeClass.PortalMode.CordsPortal ||
                portalData.SpecialMode == PortalModeClass.PortalMode.TransportNetwork || portalData.SpecialMode == PortalModeClass.PortalMode.AllowedUsersOnly || portalData.SpecialMode == PortalModeClass.PortalMode.CrystalKeyMode ||
                portalData.SpecialMode == PortalModeClass.PortalMode.PasswordLock ) 
                grouped = false;  // so normal, one way, one way password, rainbow, adminonly are only ones gruoped


            if (!grouped) { // individual colors
                if (PortalColors.TryGetValue(zdoData.Color, out var portalColorData))// Check the portal color from the ZDO data // normal entry
                {
                    if (portalColorData.Enabled)
                    {
                        currentColor = zdoData.Color;
                        currentColorHex = portalColorData.HexName;
                        nextColor = portalColorData.NextColor;
                        return portalColorData.Pos;
                    }
                }
            } else
            { // grouped 
                if (PortalColors.TryGetValue(portalData.Color, out var portalColorData))// Check the portal color from the ZDO data // normal entry
                {
                    if (portalColorData.Enabled)
                    {
                        currentColor = portalData.Color;
                        currentColorHex = portalColorData.HexName;
                        nextColor = portalColorData.NextColor;
                        return portalColorData.Pos;
                    }
                }
            }
            
            

            RMP.LogInfo("default yELLOW");
            // Default fallback to Yellow
            currentColor = "Yellow";
            currentColorHex = PortalColors["Yellow"].HexName;
            nextColor = "Red";
            return PortalColors["Yellow"].Pos;
        }

        internal static void updateYmltoColorChange(string PortalName, int colorint, string zdoID, string BiomeCol = null)
        {
           // if (!PortalN.Portals.ContainsKey(PortalName)) // if doesn't contain use defaults
           // {
             //   WritetoYML(PortalName);
            //}
            //if (BiomeCol != null) Setting BiomeColor doesn't make since when it only tracks a pair of Portals and not each indiv
            // PortalN.Portals[PortalName].BiomeColor = BiomeCol;

            PortalColor Color = (PortalColor)colorint;
            string ColorName = Color.ToString();
            //RMP.LogWarning("Make sure to remove in release color "+ ColorName);

            //main set loop
           // PortalN.Portals[PortalName].TeleportAnything = false;
           // PortalN.Portals[PortalName].Admin_only_Access = false;
            //PortalN.Portals[PortalName].Free_Passage = false;

            PortalN.Portals[PortalName].Color = ColorName;
            PortalN.Portals[PortalName].PortalZDOs[zdoID].Color = ColorName;

            if (MagicPortalFluid.FreePassageColor.Value == ColorName) // for starting Portal Yellow
            {
                PortalN.Portals[PortalName].Free_Passage = true;
            }

            if (PortalN.Portals[PortalName].PortalZDOs[zdoID].CrystalActive)
            {

                if (MagicPortalFluid.FreePassageColor.Value == ColorName)
                {
                    PortalN.Portals[PortalName].Free_Passage = true;
                }
            }
            if (BiomeCol != null)
                PortalN.Portals[PortalName].PortalZDOs[zdoID].BiomeColor = BiomeCol;

            if (PortalName == "")
            {
                if (MagicPortalFluid.ConfigAddRestricted.Value != "")
                    PortalN.Portals[PortalName].AdditionalProhibitItems = MagicPortalFluid.ConfigAddRestricted.Value.Split(',').ToList();
            }

            var wacky = PortalN.Portals[PortalName];
            ClientORServerYMLUpdate(wacky, PortalName, zdoID, colorint);
        }

        internal static void ClientORServerYMLUpdate(Portal portal, string portalName, string zdoId ="", int colorInt = 0, bool zdoUpdate = false )
        {
            var serializer = new SerializerBuilder().Build();
            var serializedPortal = portalName + MagicPortalFluid.StringSeparator + serializer.Serialize(PortalN.Portals[portalName]);

            // Construct full YAML
            string fullYml = MagicPortalFluid.WelcomeString + Environment.NewLine + serializer.Serialize(PortalN);

            if (ZNet.instance.IsServer())
            {
                // Handling for dedicated server
                if (ZNet.instance.IsDedicated())
                {
                    if (MagicPortalFluid.RiskyYMLSave.Value == MagicPortalFluid.Toggle.Off)
                    {
                        WriteYmlToFile(fullYml);
                    }

                    // Update the appropriate YML data for clients
                    if (MagicPortalFluid.UseSmallUpdates.Value == MagicPortalFluid.Toggle.On)
                    {
                        MagicPortalFluid.YMLPortalSmallData.Value = serializedPortal;
                    }
                    else
                    {
                        MagicPortalFluid.YMLPortalData.Value = fullYml;
                    }
                }
                else // Handling for local server (not dedicated)
                {
                    if (MagicPortalFluid.RiskyYMLSave.Value == MagicPortalFluid.Toggle.Off)
                    {
                        WriteYmlToFile(fullYml);
                    }

                    if (MagicPortalFluid.ConfigEnableYMLLogs.Value == MagicPortalFluid.Toggle.On)
                    {
                        MagicPortalFluid.RareMagicPortal.LogInfo(fullYml);
                    }

                    if (MagicPortalFluid.UseSmallUpdates.Value == MagicPortalFluid.Toggle.On)
                    {
                        MagicPortalFluid.YMLPortalSmallData.Value = serializedPortal;
                    }
                    else
                    {
                        MagicPortalFluid.YMLPortalData.Value = fullYml;
                    }
                }
            }
            else
            {
                if (zdoUpdate)
                {
                    functions.ServerZDOymlUpdate(portalName, zdoId, 0, serializedPortal);
                }
                else
                {
                    // Handling for client connected to a server
                    functions.ServerZDOymlUpdate(portalName, zdoId, colorInt);
                }
            }
        }
      private static void WriteYmlToFile(string ymlContent)
      {
          MagicPortalFluid.JustWrote = 1;

          // Adding extra newlines for readability directly to the content
          string[] lines = ymlContent.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
          var formattedContent = new StringBuilder();

          foreach (string line in lines)
          {
              formattedContent.AppendLine(line);
              if (line.Contains("EndPart"))
              {
                  formattedContent.AppendLine(); // Add an extra newline
              }
          }

          File.WriteAllText(MagicPortalFluid.YMLCurrentFile, formattedContent.ToString());
          MagicPortalFluid.JustWrote = 2;
      }

        internal static bool CrystalandKeyLogic(string PortalName,string zdoID, string BiomeColor = "")
        {
            int CrystalForPortal = MagicPortalFluid.ConfigCrystalsConsumable.Value;
            bool OdinsKin = false;
            bool Free_Passage = false;

            var flag = false;
            string requiredColor = PortalN.Portals[PortalName].PortalZDOs[zdoID].Color;

            if (!string.IsNullOrEmpty(PortalN.Portals[PortalName].PortalZDOs[zdoID].Biome))
                BiomeColor = PortalN.Portals[PortalName].PortalZDOs[zdoID].BiomeColor;

            MagicPortalFluid.RareMagicPortal.LogInfo($"Checking CrystalKey for Portal " + PortalName);//+" currentcolor " + currentColor + " BiomeC " + BiomeC + "BiomeColor" + BiomeColor);
            /*
            if (!PortalN.Portals.ContainsKey(PortalName)) // if doesn't contain use defaults
            {
                WritetoYML(PortalName, zdoID);
            } */

            if (MagicPortalFluid.ConfigUseBiomeColors.Value == MagicPortalFluid.Toggle.On && BiomeColor != "skip" && !string.IsNullOrEmpty(PortalN.Portals[PortalName].PortalZDOs[zdoID].Biome) )
            {
                if (BiomeColor != "")
                {
                    flag = true;
                    requiredColor = PortalN.Portals[PortalName].PortalZDOs[zdoID].BiomeColor;
                }
            }
            

            OdinsKin = PortalN.Portals[PortalName].Admin_only_Access;
            Free_Passage = PortalN.Portals[PortalName].Free_Passage; 
            bool TeleportEvery = PortalN.Portals[PortalName].TeleportAnything;

            Player player = Player.m_localPlayer;
            if (OdinsKin && MagicPortalFluid.isAdmin && !flag )
            {
                player.Message(MessageHud.MessageType.TopLeft, "$rmp_kin_welcome"); // forgot this one
                return true;
            }
            else if (OdinsKin && !MagicPortalFluid.isAdmin && PortalN.Portals[PortalName].PortalZDOs[zdoID].CrystalActive && !flag) // If requires admin, but not admin, but only with enable crystals otherwise just a normal portal
            {
                player.Message(MessageHud.MessageType.Center, "$rmp_kin_only");
                //Teleporting = false;
                return false;
            }

            if (TeleportEvery && !flag ) // if no crystals, then just white, if crystals then free passage
            {
                player.Message(MessageHud.MessageType.TopLeft, "$rmp_freepassage");
                if (PortalN.Portals[PortalName].PortalZDOs[zdoID].CrystalActive)
                {
                    player.Message(MessageHud.MessageType.Center, "$rmp_allowsEverything");
                }
                return true;
            }

            if (!player.IsTeleportable() && !TeleportEvery)
            {
                player.Message(MessageHud.MessageType.Center, "$msg_noteleport");
                return false; // Early exit if the player cannot teleport.
            }

            if (PortalN.Portals[PortalName].PortalZDOs[zdoID].CrystalActive && !Free_Passage)
            {

                // Gather counts for Crystals and Keys
                Dictionary<string, int> CrystalCount = new();
                Dictionary<string, int> KeyCount = new();

                CrystalCount[nameof(PortalColor.Gold)] = player.m_inventory.CountItems(MagicPortalFluid.GemColorGold.Value);
                CrystalCount[nameof(PortalColor.Red)] = player.m_inventory.CountItems(MagicPortalFluid.GemColorRed.Value);
                CrystalCount[nameof(PortalColor.Green)] = player.m_inventory.CountItems(MagicPortalFluid.GemColorGreen.Value);
                CrystalCount[nameof(PortalColor.Blue)] = player.m_inventory.CountItems(MagicPortalFluid.GemColorBlue.Value);
                CrystalCount[nameof(PortalColor.Purple)] = player.m_inventory.CountItems(MagicPortalFluid.GemColorPurple.Value);
                CrystalCount[nameof(PortalColor.Tan)] = player.m_inventory.CountItems(MagicPortalFluid.GemColorTan.Value);
                CrystalCount[nameof(PortalColor.Yellow)] = player.m_inventory.CountItems(MagicPortalFluid.GemColorYellow.Value);
                CrystalCount[nameof(PortalColor.White)] = player.m_inventory.CountItems(MagicPortalFluid.GemColorWhite.Value);
                CrystalCount[nameof(PortalColor.Black)] = player.m_inventory.CountItems(MagicPortalFluid.GemColorBlack.Value);
                CrystalCount[nameof(PortalColor.Cyan)] = player.m_inventory.CountItems(MagicPortalFluid.GemColorCyan.Value);
                CrystalCount[nameof(PortalColor.Orange)] = player.m_inventory.CountItems(MagicPortalFluid.GemColorOrange.Value);

                KeyCount[nameof(PortalColor.Gold)] = player.m_inventory.CountItems(MagicPortalFluid.PortalKeyGold);
                KeyCount[nameof(PortalColor.Red)] = player.m_inventory.CountItems(MagicPortalFluid.PortalKeyRed);
                KeyCount[nameof(PortalColor.Green)] = player.m_inventory.CountItems(MagicPortalFluid.PortalKeyGreen);
                KeyCount[nameof(PortalColor.Blue)] = player.m_inventory.CountItems(MagicPortalFluid.PortalKeyBlue);
                KeyCount[nameof(PortalColor.Purple)] = player.m_inventory.CountItems(MagicPortalFluid.PortalKeyPurple);
                KeyCount[nameof(PortalColor.Tan)] = player.m_inventory.CountItems(MagicPortalFluid.PortalKeyTan);
                KeyCount[nameof(PortalColor.White)] = player.m_inventory.CountItems(MagicPortalFluid.PortalKeyWhite);
                KeyCount[nameof(PortalColor.Yellow)] = player.m_inventory.CountItems(MagicPortalFluid.PortalKeyYellow);
                KeyCount[nameof(PortalColor.Black)] = player.m_inventory.CountItems(MagicPortalFluid.PortalKeyBlack);
                KeyCount[nameof(PortalColor.Cyan)] = player.m_inventory.CountItems(MagicPortalFluid.PortalKeyCyan);
                KeyCount[nameof(PortalColor.Orange)] = player.m_inventory.CountItems(MagicPortalFluid.PortalKeyOrange);



                // Check Gold as Master Override
                if (MagicPortalFluid.ConfigEnableGoldAsMaster.Value == MagicPortalFluid.Toggle.On)
                {
                    if (KeyCount["Gold"] > 0)
                    {   
                        player.Message(MessageHud.MessageType.Center, "$rmp_gold_access");
                        return true;
                    }else if (CrystalCount["Gold"] >= MagicPortalFluid.ConfigCrystalsConsumable.Value)
                    {

                        player.m_inventory.RemoveItem(MagicPortalFluid.PortalKeyGold, MagicPortalFluid.ConfigCrystalsConsumable.Value);
                        player.Message(MessageHud.MessageType.TopLeft, $"$rmp_consumed {MagicPortalFluid.ConfigCrystalsConsumable.Value} {Gold}");
                        return true;
                    }
                }

                

                // Validate the player has either a crystal or key for the required color
                bool hasCrystal = CrystalCount.ContainsKey(requiredColor) && CrystalCount[requiredColor] > 0;
                bool hasKey = KeyCount.ContainsKey(requiredColor) && KeyCount[requiredColor] > 0;

                if ( hasKey)
                {
                     player.Message(MessageHud.MessageType.TopLeft, $"$rmp_{requiredColor.ToLower()}Key_access");         
                     return true;

                }else if (hasCrystal)
                {
                    if (hasCrystal && MagicPortalFluid.ConfigCrystalsConsumable.Value > 0)
                    {
                        string itemName = MagicPortalFluid.GetGemColorByName(requiredColor);
                        player.m_inventory.RemoveItem(itemName, MagicPortalFluid.ConfigCrystalsConsumable.Value);
                        player.Message(MessageHud.MessageType.TopLeft, $"$rmp_consumed {MagicPortalFluid.ConfigCrystalsConsumable.Value} {requiredColor}");
                        return true;
                    }
                }

                // If no access, send appropriate message based on required color
                MessageHud.MessageType hudMessageType = MagicPortalFluid.ConfigMessageLeft.Value == MagicPortalFluid.Toggle.On
                    ? MessageHud.MessageType.TopLeft
                    : MessageHud.MessageType.Center;

                switch (requiredColor)
                {
                    case "Yellow":
                        player.Message(hudMessageType, "$rmp_required_yellow");
                        break;
                    case "Red":
                        player.Message(hudMessageType, "$rmp_required_red");
                        break;
                    case "Green":
                        player.Message(hudMessageType, "$rmp_required_green");
                        break;
                    case "Blue":
                        player.Message(hudMessageType, "$rmp_required_blue");
                        break;
                    case "Purple":
                        player.Message(hudMessageType, "$rmp_required_purple");
                        break;
                    case "Tan":
                        player.Message(hudMessageType, "$rmp_required_tan");
                        break;
                    case "Cyan":
                        player.Message(hudMessageType, "$rmp_required_cyan");
                        break;
                    case "Orange":
                        player.Message(hudMessageType, "$rmp_required_orange");
                        break;
                    case "White":   
                        player.Message(hudMessageType, "$rmp_required_white");
                        break;
                    case "Black":
                        player.Message(hudMessageType, "$rmp_required_black");
                        break;
                    case "Gold":
                        player.Message(hudMessageType, "$rmp_required_gold");
                        break;
                    default:
                        player.Message(hudMessageType, "$rmp_noaccess");
                        break;
                }

                return false;

            }

           return true;
        }
        /*
        [HarmonyPatch(typeof(Player), nameof(Player.Update))]
        public static class UpdateRemoveItem
        {
            private static void Prefix(ref Player __instance)
            {
                if (inventoryRemove)
                {
                    if (!__instance.m_nview.IsValid() || !__instance.m_nview.IsOwner())
                    {
                        return;
                    }
                    //RMP.LogInfo("Actual Removal");
                    var itemhere = __instance.m_inventory.GetItem(removeItems.Last().Key);
                    if (itemhere == null)
                    {
                        RMP.LogInfo("item is null");
                    }
                    //itemhere.m_stack
                    //__instance.m_inventory.m_inventory.Remove(itemhere);
                    __instance.m_inventory.RemoveItem(itemhere, removeItems.Last().Value);
                    removeItems.Remove(removeItems.Last().Key);
                    inventoryRemove = false;
                    //__instance.m_inventory.Changed();
                }
            }
        }*/

        internal static void WritetoYML(string PortalName, string ZDOID = null, string oldname = "", bool writeZdoOnly = false, TeleportWorld instance = null, string creator = "") // this only happens if portal is not in yml file at all
        {
            RMP.LogInfo("Writing New YML");
            int colorint = 1;
            bool cloneoverride = false;
            bool namealreadyexists = false; // maybe

            PortalName.Portal paulgo;
            if (writeZdoOnly)
            {
                paulgo = PortalN.Portals[PortalName];
            }
            else
            {
                paulgo = new PortalName.Portal
                {
                };
                PortalN.Portals.Add(PortalName, paulgo);
            }
            PortalName.ZDOP zdoid = new PortalName.ZDOP { };
            PortalN.Portals[PortalName].PortalZDOs.Add(ZDOID, zdoid);
            if (instance != null)
            { }

            if (oldname != "")
            {
                PortalN.Portals[PortalName].PortalZDOs[ZDOID] = PortalN.Portals[oldname].PortalZDOs[ZDOID].Clone(); // copy from another if just changed portal name // maybe config?
                cloneoverride = true;
                PortalN.Portals[PortalName].SpecialMode = PortalN.Portals[oldname].SpecialMode;
                PortalN.Portals[PortalName].Free_Passage = PortalN.Portals[oldname].Free_Passage;
                PortalN.Portals[PortalName].TeleportAnything = PortalN.Portals[oldname].TeleportAnything;
                PortalN.Portals[PortalName].Admin_only_Access = PortalN.Portals[oldname].Admin_only_Access;
                PortalN.Portals[PortalName].AllowedUsers = PortalN.Portals[oldname].AllowedUsers;
                PortalN.Portals[PortalName].Color = PortalN.Portals[oldname].Color;
                PortalN.Portals[PortalName].MaxWeight = PortalN.Portals[oldname].MaxWeight;
                PortalN.Portals[PortalName].AdditionalAllowItems = PortalN.Portals[oldname].AdditionalAllowItems;
                PortalN.Portals[PortalName].AdditionalProhibitItems = PortalN.Portals[oldname].AdditionalProhibitItems;

            }

            if (creator != "")
            {
                PortalN.Portals[PortalName].PortalZDOs[ZDOID].Creator = creator;
            }

            if (!cloneoverride)
            {

                if (!writeZdoOnly)  // so set the init portal name stuff leave, everything else
                {
                    PortalN.Portals[PortalName].SpecialMode = MagicPortalFluid.DefaultMode.Value;
                    PortalN.Portals[PortalName].PortalZDOs[ZDOID].SpecialMode = MagicPortalFluid.DefaultMode.Value;
                    PortalN.Portals[PortalName].Free_Passage = false;
                    PortalN.Portals[PortalName].TeleportAnything = false;
                    PortalN.Portals[PortalName].Admin_only_Access = false;

                    if (MagicPortalFluid.DefaultMode.Value == PortalModeClass.PortalMode.CrystalKeyMode) 
                        PortalN.Portals[PortalName].PortalZDOs[ZDOID].CrystalActive = true;

                    if (MagicPortalFluid.EnableCrystalsforNewIfPossible.Value == MagicPortalFluid.Toggle.On)
                    {
                        if (MagicPortalFluid.DefaultMode.Value == PortalModeClass.PortalMode.TargetPortal && MagicPortalFluid.TargetPortalLoaded ||
                            MagicPortalFluid.DefaultMode.Value == PortalModeClass.PortalMode.RandomTeleport ||
                            MagicPortalFluid.DefaultMode.Value == PortalModeClass.PortalMode.PasswordLock ||
                            MagicPortalFluid.DefaultMode.Value == PortalModeClass.PortalMode.OneWayPasswordLock ||
                            MagicPortalFluid.DefaultMode.Value == PortalModeClass.PortalMode.OneWay ||
                            MagicPortalFluid.DefaultMode.Value == PortalModeClass.PortalMode.CordsPortal
                            )
                        {
                            PortalN.Portals[PortalName].PortalZDOs[ZDOID].CrystalActive = true;
                        }
                    }

                    if(MagicPortalFluid.DefaultMode.Value == PortalModeClass.PortalMode.AdminOnly )
                    {
                        PortalN.Portals[PortalName].Admin_only_Access = true;
                    }

                    if (MagicPortalFluid.ConfigAddRestricted.Value != "")
                        PortalN.Portals[PortalName].AdditionalProhibitItems = MagicPortalFluid.ConfigAddRestricted.Value.Split(',').ToList(); // one time

                    if (MagicPortalFluid.ConfigAllowItems.Value != "")
                        PortalN.Portals[PortalName].AdditionalAllowItems = MagicPortalFluid.ConfigAllowItems.Value.Split(',').ToList(); // one time

                    if (MagicPortalFluid.ConfigMaxWeight.Value != 0)
                        PortalN.Portals[PortalName].MaxWeight = MagicPortalFluid.ConfigMaxWeight.Value; // one time

                    if (MagicPortalFluid.FreePassageColor.Value == "None" || MagicPortalFluid.FreePassageColor.Value == "none") { } // already set something above
                    else if (MagicPortalFluid.DefaultColor.Value == MagicPortalFluid.FreePassageColor.Value || MagicPortalFluid.FreePassageColor.Value == "Yellow" && colorint == 1)
                    {
                        PortalN.Portals[PortalName].Free_Passage = true;
                        colorint = PortalColors[MagicPortalFluid.FreePassageColor.Value].Pos;
                    }

                    if (MagicPortalFluid.DefaultColor.Value == "None" || MagicPortalFluid.DefaultColor.Value == "none")
                    {
                        colorint = 1; // yellow
                        PortalN.Portals[PortalName].Color = "Yellow";
                    }
                    else
                    {
                        colorint = PortalColors[MagicPortalFluid.DefaultColor.Value].Pos;
                        PortalN.Portals[PortalName].Color = MagicPortalFluid.DefaultColor.Value;
                    }

                }
             
                if (MagicPortalFluid.DefaultColor.Value == "None" || MagicPortalFluid.DefaultColor.Value == "none")
                {
                    colorint = 1; // yellow
                    PortalN.Portals[PortalName].PortalZDOs[ZDOID].Color = "Yellow";
                }
                else
                {
                    colorint = PortalColors[MagicPortalFluid.DefaultColor.Value].Pos;
                    PortalN.Portals[PortalName].PortalZDOs[ZDOID].Color = MagicPortalFluid.DefaultColor.Value;
                }

            }

            var wacky = PortalN.Portals[PortalName];
            ClientORServerYMLUpdate(wacky, PortalName, ZDOID, colorint );
        }


        private static void HandleYippe(TeleportWorldDataRMP teleportWorldData)
        {
            // override color for teleportanything color
            if (MagicPortalFluid.PortalDrinkColor.Value == "Rainbow")
            {
                Color newCol = Color.yellow;// default
                Random rnd = new Random();
                PortalColor currentC = (PortalColor)Enum.Parse(typeof(PortalColor), currentRainbow);
                int pickcolor = rnd.Next(1, 12);
                var colorna = currentC.Next();
                for (int i = 1; i < pickcolor; i++)
                {
                    colorna.Next();
                }
                currentRainbow = colorna.ToString();
                //RMP.LogInfo("rainbow currently is " + colorna.ToString());
                newCol = PortalColors[colorna.ToString()].HexName;
                rainbowWait = 0;

                if (newCol != teleportWorldData.OldColor)
                {  // don't waste resources
                    teleportWorldData.TargetColor = newCol;
                    SetTeleportWorldColors(teleportWorldData, true);
                }
            }
            else if (MagicPortalFluid.PortalDrinkColor.Value == "Rainbow2") // Trying to copy Rainbow effect from cheatsword to portals, almost worked. ran out of time
            {
                if (CheatSwordColor == null)
                {
                    RMP.LogInfo("Set cheatsword");
                    var itemCS = ObjectDB.instance.GetItemPrefab("SwordCheat");// not used just for init
                    CheatSwordColor = itemCS.GetComponentInChildren<ParticleSystem>(true);
                    // Transform CheatswordColor = ObjectDB.instance.GetItemPrefab("WackyBox").transform.Find("wackyflames");//ObjectDB.instance.GetItemPrefab("SwordCheat").transform.Find("attach/equiped/Particle System");
                }
                if (CheatSwordColor == null)
                    RMP.LogInfo("Cheatsword is null");

                ParticleSystem.MinMaxGradient holdColor = CheatSwordColor.main.startColor;
                //ParticleSystem.MainModule main = CheatswordColor.GetComponent<ParticleSystem.MainModule>();
                //main.startColor = holdColor;
                //holdColor = main.startColor;
                //foreach (ParticleSystem system in CheatSwordColor)
                //{
                // ParticleSystem.MainModule main = system.main;
                // holdColor = main.startColor;

                //}

                RMP.LogInfo("Set holdColor");

                ParticleSystem system = teleportWorldData.BlueFlames[0];
                {
                    //system.GetComponent<Transform>().gameObject.SetActive(false);

                    var main2 = system.GetComponent<ParticleSystem.MainModule>();
                    var colover = system.GetComponent<ParticleSystem.ColorOverLifetimeModule>();
                    //system = CheatswordColor.GetComponent<ParticleSystem>();
                    //system.GetComponent<color>
                    // main2.startColor = holdColor;
                    colover.color = holdColor;
                    //ParticleSystem.ColorOverLifetimeModule colorOverLifetime = system.colorOverLifetime;
                    //colorOverLifetime.color = holdColor;
                    //ParticleSystem.MainModule main2 = system.main;
                    //main2.startColor = holdColor;

                    system.GetComponent<ParticleSystemRenderer>().material = MagicPortalFluid.originalMaterials["flame"];
                    //RMP.LogInfo("flame set");
                }
            }
            else
            {
                teleportWorldData.TargetColor = PortalColors[MagicPortalFluid.PortalDrinkColor.Value].HexName; // color update for yippe
                SetTeleportWorldColors(teleportWorldData, false, false);
            }
        }        
        private static void HandleYippe(ClassBase teleportWorldData)
        {
            /*
            // override color for teleportanything color
            if (MagicPortalFluid.PortalDrinkColor.Value == "Rainbow")
            {
                Color newCol = Color.yellow;// default
                Random rnd = new Random();
                PortalColor currentC = (PortalColor)Enum.Parse(typeof(PortalColor), currentRainbow);
                int pickcolor = rnd.Next(1, 12);
                var colorna = currentC.Next();
                for (int i = 1; i < pickcolor; i++)
                {
                    colorna.Next();
                }
                currentRainbow = colorna.ToString();
                //RMP.LogInfo("rainbow currently is " + colorna.ToString());
                newCol = PortalColors[colorna.ToString()].HexName;
                rainbowWait = 0;

                if (newCol != teleportWorldData.OldColor)
                {  // don't waste resources
                    teleportWorldData.TargetColor = newCol;
                    SetTeleportWorldColors(teleportWorldData, true);
                }
            }
            else if (MagicPortalFluid.PortalDrinkColor.Value == "Rainbow2") // Trying to copy Rainbow effect from cheatsword to portals, almost worked. ran out of time
            {
                if (CheatSwordColor == null)
                {
                    RMP.LogInfo("Set cheatsword");
                    var itemCS = ObjectDB.instance.GetItemPrefab("SwordCheat");// not used just for init
                    CheatSwordColor = itemCS.GetComponentInChildren<ParticleSystem>(true);
                    // Transform CheatswordColor = ObjectDB.instance.GetItemPrefab("WackyBox").transform.Find("wackyflames");//ObjectDB.instance.GetItemPrefab("SwordCheat").transform.Find("attach/equiped/Particle System");
                }
                if (CheatSwordColor == null)
                    RMP.LogInfo("Cheatsword is null");

                ParticleSystem.MinMaxGradient holdColor = CheatSwordColor.main.startColor;
                //ParticleSystem.MainModule main = CheatswordColor.GetComponent<ParticleSystem.MainModule>();
                //main.startColor = holdColor;
                //holdColor = main.startColor;
                //foreach (ParticleSystem system in CheatSwordColor)
                //{
                // ParticleSystem.MainModule main = system.main;
                // holdColor = main.startColor;

                //}

                RMP.LogInfo("Set holdColor");

                ParticleSystem system = teleportWorldData.BlueFlames[0];
                {
                    //system.GetComponent<Transform>().gameObject.SetActive(false);

                    var main2 = system.GetComponent<ParticleSystem.MainModule>();
                    var colover = system.GetComponent<ParticleSystem.ColorOverLifetimeModule>();
                    //system = CheatswordColor.GetComponent<ParticleSystem>();
                    //system.GetComponent<color>
                    // main2.startColor = holdColor;
                    colover.color = holdColor;
                    //ParticleSystem.ColorOverLifetimeModule colorOverLifetime = system.colorOverLifetime;
                    //colorOverLifetime.color = holdColor;
                    //ParticleSystem.MainModule main2 = system.main;
                    //main2.startColor = holdColor;

                    system.GetComponent<ParticleSystemRenderer>().material = MagicPortalFluid.originalMaterials["flame"];
                    //RMP.LogInfo("flame set");
                }
            }
            else
            {
                teleportWorldData.TargetColor = PortalColors[MagicPortalFluid.PortalDrinkColor.Value].HexName; // color update for yippe
                SetTeleportWorldColors(teleportWorldData, false, false);
            }
            */
        }

        private static  bool CanChangePortalColor( bool isAdmin, bool isCreator, ZDOP portalData)
        {
            // Check if admin override or PreventColorChange allows the player to change colors
            bool canChangeColor = isAdmin ||
                                  MagicPortalFluid.PreventColorChange.Value == MagicPortalFluid.Toggle.Off ||
                                  (MagicPortalFluid.PreventColorChange.Value == MagicPortalFluid.Toggle.On && isCreator);

            // If default biome colors are used, check if portal creators are prevented from changing it
            if (MagicPortalFluid.ConfigUseBiomeColors.Value == MagicPortalFluid.Toggle.On && canChangeColor)
            {
                if (MagicPortalFluid.ConfigPreventCreatorsToChangeBiomeColor.Value == MagicPortalFluid.Toggle.On && !isAdmin)
                {
                    canChangeColor = false;  // Prevent creators from changing colors when biome colors are enforced
                }
            }

            // If CrystalActive is active set to false
            if (portalData.CrystalActive && !isAdmin )
            {
                canChangeColor = false;
            }

            return canChangeColor;
        }



        private static void SetTeleportWorldColors(TeleportWorldDataRMP teleportWorldData, bool SetcolorTarget = false, bool SetMaterial = false)
        {
            teleportWorldData.OldColor = teleportWorldData.TargetColor;

            if (teleportWorldData.TargetColor == Gold)
            {
                try
                {
                    Material mat = MagicPortalFluid.originalMaterials["shaman_prupleball"];
                    foreach (Renderer red in teleportWorldData.MeshRend)
                    {
                        red.material = mat;
                    }
                }
                catch { }
            }
            else if (teleportWorldData.TargetColor == Color.black)
            {
                try
                {
                    Material mat = MagicPortalFluid.originalMaterials["silver_necklace"];
                    foreach (Renderer red in teleportWorldData.MeshRend)
                    {
                        red.material = mat;
                    }
                }
                catch { }
            }
            /*
            else if (teleportWorldData.TargetColor == Color.white)
            {
                try
                {
                    Material mat = MagicPortalFluid.originalMaterials["crystal_exterior"];
                    foreach (Renderer red in teleportWorldData.MeshRend)
                    {
                        red.material = mat;
                    }
                }
                catch { }
            } */
            /*
            else if (teleportWorldData.TargetColor == Tan)
            {
                try
                {
                    Material mat = originalMaterials["ball2"];
                    foreach (Renderer red in teleportWorldData.MeshRend)
                    {
                        red.material = mat;
                    }
                }
                catch { }
            }*/
            else
            {          // sets back to default
                Material mat = MagicPortalFluid.originalMaterials["portal_small"];
                foreach (Renderer red in teleportWorldData.MeshRend)
                {
                    red.material = mat;
                }
            }

            foreach (Light light in teleportWorldData.Lights)
            {
                /*
                if (teleportWorldData.TargetColor == Color.yellow) // trying to reset to default
                {
                    light.color = lightcolor;
                } */

                light.color = teleportWorldData.TargetColor;
            }

            foreach (ParticleSystem system in teleportWorldData.Sucks)
            {
                ParticleSystem.MainModule main = system.main;
                if (teleportWorldData.TargetColor == Color.white)
                {
                    main.startColor = teleportWorldData.TargetColor;
                    main.maxParticles = 1000;
                }
                else if (teleportWorldData.TargetColor == Orange)
                {
                    main.startColor = teleportWorldData.TargetColor;
                    main.maxParticles = 1000;
                }
                else if (teleportWorldData.TargetColor == Color.black)
                {
                    main.startColor = Color.white;
                    main.maxParticles = 1000;
                }
                else if (teleportWorldData.TargetColor == Yellow2)
                {
                    main.startColor = Yellow2;
                    main.maxParticles = 1000;
                }
                else if (teleportWorldData.TargetColor == Cornsilk)
                {
                    main.startColor = Brown;
                    main.maxParticles = 30;
                }
                else
                {
                    main.startColor = Color.black;
                    main.maxParticles = 1000;
                }
            }

            foreach (ParticleSystem system in teleportWorldData.Systems)
            {
                ParticleSystem.ColorOverLifetimeModule colorOverLifetime = system.colorOverLifetime;
                colorOverLifetime.color = new ParticleSystem.MinMaxGradient(teleportWorldData.TargetColor, teleportWorldData.TargetColor);
                /*
                if (teleportWorldData.TargetColor == Color.yellow) // trying to reset to default
                {
                    colorOverLifetime.color = new ParticleSystem.MinMaxGradient(flamesstart, flamesend);
                } */

                ParticleSystem.MainModule main = system.main;

                //system.GetComponent<Renderer>().material = MagicPortalFluid.originalMaterials["Portal_flame"];
                main.startColor = teleportWorldData.TargetColor;
            }


            // change blue flames material        

        /*
            foreach (Material material in teleportWorldData.Materials) // flames old way
            {
                //material.color = teleportWorldData.TargetColor; old way with flames
                .
            }*/

            foreach (ParticleSystem system in teleportWorldData.BlueFlames)
            {

                var shape = system.shape.radius;
                shape = 1.26f;
                var partcolor = teleportWorldData.TargetColor;

                if (teleportWorldData.TargetColor == Purple) // trying to reset to default
                {
                   //partcolor = new Color(164f / 255f, 16f/255f, 120f / 255f, 1f); //good pink
                   partcolor = new Color(37f / 255f, 0, 58f / 255f, 1f); 

                    system.GetComponent<Renderer>().material = MagicPortalFluid.originalMaterials["dragon_death_trail"];                   
                    //shape = 1.5f;
                }
                else if (teleportWorldData.TargetColor == Gold) // trying to reset to default
                {
                    system.GetComponent<Renderer>().material = MagicPortalFluid.originalMaterials["dragon_death_trail"];
                    //shape = 1.5f;
                }
                else if (teleportWorldData.TargetColor == Cornsilk) // trying to reset to default
                {
                    partcolor = Brown;
                    system.GetComponent<Renderer>().material = MagicPortalFluid.originalMaterials["teleport_suck"];
                }
                else if (teleportWorldData.TargetColor == Color.blue) 
                {
                    //system.GetComponent<Renderer>().material = MagicPortalFluid.originalMaterials["teleport_suck"];
                    system.GetComponent<Renderer>().material = MagicPortalFluid.originalMaterials["flame"];
                   // shape = 1.5f;
                }
                else if (teleportWorldData.TargetColor == Color.cyan)
                { 
                    system.GetComponent<Renderer>().material = MagicPortalFluid.originalMaterials["crystal_Dvergrcase"];
                    partcolor = new Color(78f / 255f, 205f / 255f, 196f / 255f, 1f);

                }
                else if (teleportWorldData.TargetColor == Color.white) 
                {
                    system.GetComponent<Renderer>().material = MagicPortalFluid.originalMaterials["teleport_suck"];
                } else
                {
                    system.GetComponent<Renderer>().material = MagicPortalFluid.originalMaterials["portal_flame"];
                }

                ParticleSystem.ColorOverLifetimeModule colorOverLifetime = system.colorOverLifetime;
                colorOverLifetime.color = partcolor; //new ParticleSystem.MinMaxGradient(partcolor, partcolor);
                

                if (teleportWorldData.TargetColor == Color.yellow) // trying to reset to default
                {
                    //colorOverLifetime.color = new ParticleSystem.MinMaxGradient(flamesstart, flamesend);
                } 

                ParticleSystem.MainModule main = system.main;

                main.startColor = partcolor;

                system.Clear();
                system.Simulate(0f);
                system.Play();
            }



            if (SetcolorTarget)
            {
                if (teleportWorldData.TargetColor == Color.black)
                {
                    teleportWorldData.TeleportW.m_colorTargetfound = Color.black * 10;
                }
                else if (teleportWorldData.TargetColor == Color.yellow) // trying to reset to default
                {
                    teleportWorldData.TeleportW.m_colorTargetfound = m_colorTargetfound * 7;
                }
                else if (teleportWorldData.TargetColor == Gold)
                {
                    teleportWorldData.TeleportW.m_colorTargetfound = teleportWorldData.TargetColor;
                }
                else if (teleportWorldData.TargetColor == Cornsilk)
                {
                    teleportWorldData.TeleportW.m_colorTargetfound = Brown * 3;
                }
                else if (teleportWorldData.TargetColor == Color.cyan) // cyan now
                {
                    teleportWorldData.TeleportW.m_colorTargetfound = teleportWorldData.TargetColor * 4;
                }
                else if (teleportWorldData.TargetColor == Color.blue) // cyan now
                {
                    teleportWorldData.TeleportW.m_colorTargetfound = teleportWorldData.TargetColor * 7;
                }
                else
                    teleportWorldData.TeleportW.m_colorTargetfound = teleportWorldData.TargetColor * 7; // set color // set intensity very high
            }
        }

        private static bool TryGetTeleportWorld(TeleportWorld key, out TeleportWorldDataRMP value)
        {
            if (key)
            {
                return MagicPortalFluid._teleportWorldDataCache.TryGetValue(key, out value);
            }

            value = default;
            return false;
        }

        private static string CreatePortalID(string name)
        {
            Random random = new Random();
            return (int)UnityEngine.Random.Range(1, 888) + "_" + (int)UnityEngine.Random.Range(1, 99999);
        }
    }

    public static class Extensions
    {
        public static T Next<T>(this T src) where T : struct
        {
            if (!typeof(T).IsEnum) throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

            T[] Arr = (T[])Enum.GetValues(src.GetType());
            int j = Array.IndexOf<T>(Arr, src) + 1;
            return (Arr.Length == j) ? Arr[0] : Arr[j];
        }
    }
}