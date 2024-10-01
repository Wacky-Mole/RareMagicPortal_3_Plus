using BepInEx.Bootstrap;
using HarmonyLib;
using JetBrains.Annotations;
using RareMagicPortal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static RareMagicPortal.PortalName;
using RareMagicPortal_3_Plus.PortalMode;
using System.Diagnostics;
using ServerSync;
using Random = UnityEngine.Random;
using static UnityEngine.InputSystem.InputRemoting;
using YamlDotNet.Core.Tokens;
using System.ComponentModel;
using BepInEx.Logging;
using System.Security.Cryptography;
using TMPro;
using UnityEngine.Windows;
using System.Net.NetworkInformation;
using UnityEngine.UI;


namespace RareMagicPortal_3_Plus.Patches
{
    internal class TeleportWorldPatchs
    {


        /*
        [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.HaveTarget))] 
        [HarmonyPriority(Priority.LowerThanNormal)]
        public static class SetPortalsConnectedRMP
        {
            private static bool Prefix(TeleportWorld __instance,  ref bool __result)
            {

                //return true;//  OVERRIDE THIS FROM TARGET PORTAL
                if (!MagicPortalFluid.TargetPortalLoaded)
                    return true;



                if (__instance.m_nview == null || __instance.m_nview.GetZDO() == null)
                {
                    __result = false;
                }

                __result = __instance.m_nview.GetZDO().GetConnectionZDOID(ZDOExtraData.ConnectionType.Portal) != ZDOID.None;

                return false;
            }
        }

        */

        [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.TargetFound))]
        [HarmonyPriority(Priority.LowerThanNormal)]
        public static class DisabldHaveTarget
        {
            internal static bool Prefix(TeleportWorld __instance, ref bool __result)
            {
                if (Player.m_localPlayer == null)
                    return true;

                string PortalName = __instance.m_nview.m_zdo.GetString("tag");
                var zdoname = __instance.m_nview.GetZDO().GetString(MagicPortalFluid._portalID);
                try
                {
                    var portal = PortalColorLogic.PortalN.Portals[PortalName];
                    var portalZDO = portal.PortalZDOs[zdoname];

                    if (!portalZDO.Active)
                    {
                        __result = false;
                        return false;
                    }

                    if (portal.SpecialMode == PortalModeClass.PortalMode.AdminOnly && !MagicPortalFluid.isAdmin || portal.SpecialMode == PortalModeClass.PortalMode.RandomTeleport)// allowed users
                    {
                        __result = false;
                        return false;
                    }
                    if (portal.SpecialMode == PortalModeClass.PortalMode.AllowedUsersOnly && (portal.AllowedUsers == null || !portal.AllowedUsers.Contains(Player.m_localPlayer.GetPlayerName())))// allowed users
                    {
                        __result = false;
                        return false;
                    }

                    if (portal.SpecialMode == PortalModeClass.PortalMode.CordsPortal )  // Override color showing 
                    {
                        __result = true;
                        return false;
                    }
                    if (portal.SpecialMode == PortalModeClass.PortalMode.TransportNetwork)
                    {
                        __instance.m_hadTarget = false;
                        __result = false;
                        return false;
                    }
                        
                    if (Player.m_localPlayer.m_seman.HaveStatusEffect("yippeTele".GetStableHashCode()))
                    {
                        __result = true;
                        return false;
                    }
                    if (MagicPortalFluid.TargetPortalLoaded && portal.SpecialMode == PortalModeClass.PortalMode.TargetPortal)
                    {
                        __instance.m_hadTarget = true;
                        if (MagicPortalFluid.Toggle.Off == MagicPortalFluid.ConfigTargetPortalAnimation.Value)
                        {
                            __result = false;
                            return false;
                        }else
                        {
                            __result = true;
                            return false;
                        }   
                    }                
                }
                catch { } // catch any that haven't been entered yet
                if (!MagicPortalFluid.TargetPortalLoaded)
                    return true;
                else if (!__instance.m_hadTarget && MagicPortalFluid.TargetPortalLoaded)
                {
                    __result = false;
                    return false;
                }
                // Override Target Portal
                if (__instance.m_nview == null || __instance.m_nview.GetZDO() == null)
                {
                    __result = false;
                }

                ZDOID connectionZDOID = __instance.m_nview.GetZDO().GetConnectionZDOID(ZDOExtraData.ConnectionType.Portal);
                if (connectionZDOID == ZDOID.None)
                {
                    __result = false;
                }

                if (ZDOMan.instance.GetZDO(connectionZDOID) == null)
                {
                    ZDOMan.instance.RequestZDO(connectionZDOID);
                    __result = false;
                }
                __result = true;
                return false;
            }
        }


        [HarmonyPatch(typeof(ZDOMan), nameof(ZDOMan.AddPeer))]
        public static class SendKnownPortalsOnConnectPeers
        {
            [UsedImplicitly]
            private static void Postfix(ZDOMan __instance, ZNetPeer netPeer)
            {
                if (ZNet.instance.IsServer())
                {
                    foreach (ZDO zdo in MagicPortalFluid.PortalsKnown.Values)
                    {
                        __instance.ForceSendZDO(netPeer.m_uid, zdo.m_uid);
                    }
                }
            }
        }

        private static IEnumerator GetandFetPortals()
        {
            while (true)
            {
                List<ZDO> portalList = ZDOMan.instance.GetPortals();
                var found = portalList.ToDictionary(zdo => zdo.ToString(), zdo => zdo);

                if (ZNet.instance.IsServer())
                {
                    var newPortals = found.Keys.Except(MagicPortalFluid.PortalsKnown.Select(zdo => zdo.ToString()));

                    foreach (string portalKey in newPortals)
                    {
                        if (found.TryGetValue(portalKey, out ZDO zdo))
                        {
                            ZDOMan.instance.ForceSendZDO(zdo.m_uid);
                        }
                    }
                }

                MagicPortalFluid.PortalsKnown = found;
                yield return new WaitForSeconds(10f);
            }
        }

        [HarmonyPatch(typeof(Game), nameof(Game.Start))]
        public class StartPortalGettingRMP
        {
            private static void Postfix()
            {
                MagicPortalFluid.PortalsKnown.Clear();
                Game.instance.StartCoroutine(GetandFetPortals());
                if (ZNet.instance.IsServer())
                    MagicPortalFluid.isAdmin = true;
            }
        }

        [HarmonyPatch(typeof(TeleportWorld), nameof(TeleportWorld.Awake))]
        private static class AddNewPortalToListRMP
        {
            private static void Postfix(TeleportWorld __instance)
            {
                if (__instance.TryGetComponent<WearNTear>(out var wear))
                {
                  // MagicPortalFluid.PortalsKnown.Add(__instance.m_nview.GetZDO().ToString(), __instance.m_nview.GetZDO());
                    Action? onDestroy = wear.m_onDestroyed;
                    wear.m_onDestroyed = () =>
                    {
                        onDestroy?.Invoke();
                        MagicPortalFluid.PortalsKnown.Remove(__instance.m_nview.GetZDO().ToString());
                    };
                }
            }
        }


        [HarmonyPatch(typeof(Player), nameof (Player.TeleportTo))]
        public static class FastTeleRMP
        {
            public static void Postfix(ref float ___m_teleportTimer, Player __instance, Vector3 pos, Quaternion rot, ref bool distantTeleport, ref bool __result)
            {
                UnityEngine.Debug.Log("distantTeleport teleport is " + !MagicPortalFluid.LastTeleportFast);
                distantTeleport = !MagicPortalFluid.LastTeleportFast;
                if (MagicPortalFluid.LastTeleportFast)
                {
                    float timerStartTime = 8f;
                    ___m_teleportTimer = timerStartTime;
                }

                MagicPortalFluid.LastTeleportFast = false;
                //logger.Log((LogLevel)16, (object)("Set teleport timer to " + timerStartTime));

                /* for reference
                if (__result && distantTeleport && ZNetScene.instance.IsAreaReady(pos))
                {
                    Vector2i zone = ZoneSystem.instance.GetZone(((Component)__instance).transform.position);
                    Vector2i zone2 = ZoneSystem.instance.GetZone(pos);
                    if (Mathf.Abs(zone.x - zone2.x) <= 1 && Mathf.Abs(zone.y - zone2.y) <= 1)
                    {
                        __instance.m_distantTeleport = false;
                    }
                }
                */
            }
        }




        private static Vector3 LastPortalTrigger = new Vector3();
        private static string LastPortalName = "";

        [HarmonyPatch(typeof(TeleportWorldTrigger), nameof(TeleportWorldTrigger.OnTriggerEnter))]
        internal class TeleportWorld_Teleport_CheckforCrystal
        {
            internal class SkipPortalException : Exception { }

            [HarmonyPriority(Priority.HigherThanNormal)]
            internal static bool Prefix(TeleportWorldTrigger __instance, Collider colliderIn)
            {
                if (colliderIn.GetComponent<Player>() != Player.m_localPlayer)
                {
                    throw new SkipPortalException();
                }
                
                string PortalName = __instance.m_teleportWorld.m_nview.m_zdo.GetString("tag");
                var zdoname = __instance.m_teleportWorld.m_nview.GetZDO().GetString(MagicPortalFluid._portalID);  
                var portal = PortalColorLogic.PortalN.Portals[PortalName];
                var portalZDO = portal.PortalZDOs[zdoname];
                if (!portalZDO.Active) // skip all
                {
                    throw new SkipPortalException();
                }
                if (portal.SpecialMode == PortalModeClass.PortalMode.AllowedUsersOnly && (portal.AllowedUsers == null || !portal.AllowedUsers.Contains(Player.m_localPlayer.GetPlayerName())) ||
                    portal.SpecialMode == PortalModeClass.PortalMode.AdminOnly && !MagicPortalFluid.isAdmin )// allowed users                 
                {
                    throw new SkipPortalException();
                }

                if ((portalZDO.SpecialMode == PortalModeClass.PortalMode.PasswordLock ||
                     portalZDO.SpecialMode == PortalModeClass.PortalMode.OneWayPasswordLock) &&
                    (portal.AllowedUsers == null || !portal.AllowedUsers.Contains(Player.m_localPlayer.GetPlayerName())))
            
                    {
                    if (PasswordPopup._popupInstance != null)
                    {
                        return false; // Prevent the default interaction
                    }
                    PasswordPopup popup = new();
                    popup.ShowPasswordPopup((password) =>
                    {
                        if (PortalModeClass.CheckPassword(password, PortalColorLogic.PortalN.Portals[PortalName].PortalZDOs[zdoname].Password))
                        {
                            UnityEngine.Debug.Log("Entered Correct password");
                            if (portal.AllowedUsers == null)
                            {
                                portal.AllowedUsers = new List<string>(); 
                            }
                            portal.AllowedUsers.Add(Player.m_localPlayer.GetPlayerName());

                            PortalColorLogic.ClientORServerYMLUpdate(portal, PortalName);
                            Player.m_localPlayer.Message(MessageHud.MessageType.Center,  "Password is correct, you may proceed");

                        }
                        else
                        {
                            UnityEngine.Debug.LogWarning("Incorrect password entered.");
                            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Incorrect password.");
                        }                      
                    });

                        return false;// while waiting
                }

                MagicPortalFluid.TeleportingforWeight = 1;// what?
                //MagicPortalFluid.m_hadTarget = __instance.m_teleportWorld.m_hadTarget;
                MagicPortalFluid.LastTeleportFast = portalZDO.FastTeleport;


                // Check crystal and key logic
                if (PortalColorLogic.CrystalandKeyLogic(PortalName, __instance.m_teleportWorld.m_nview.GetZDO().GetString(MagicPortalFluid._portalID), __instance.m_teleportWorld.m_nview.m_zdo.GetString(MagicPortalFluid._portalBiomeColorHashCode)))
                {
                    bool cancelTargetPortal = true;

                    if (portal.SpecialMode == PortalModeClass.PortalMode.CordsPortal)
                    {
                        string coords = portalZDO.Coords; // Assuming Coords is a string like "12.5, 34.6, 78.9"
                        if (!string.IsNullOrEmpty(coords))
                        {
                            string[] coordParts = coords.Split(',');

                            if (coordParts.Length == 3)
                            {
                                // Try parsing each coordinate into float
                                if (float.TryParse(coordParts[0].Trim(), out float x) &&
                                    float.TryParse(coordParts[1].Trim(), out float y) &&
                                    float.TryParse(coordParts[2].Trim(), out float z))
                                {
                                    // Successfully parsed the coordinates
                                    Vector3 position = new Vector3(x, y, z);

                                    Player.m_localPlayer.TeleportTo(position, Player.m_localPlayer.transform.rotation, true);
                                }
                            }
                        }                                
                    }

                    if (portalZDO.SpecialMode == PortalModeClass.PortalMode.RandomTeleport)
                    {
                        string funnyLine = "";
                        switch  ((int)Random.Range(0, 11))
                        {
                            case 0:
                            funnyLine = "May Odin look favorably one You"; break;
                            case 1:
                            funnyLine = "Good Luck"; break;
                            case 2:
                            funnyLine = "Suckerrr";break;
                            case 3:
                            funnyLine = "To Valhalla and beyond!";
                            break;             case 4:
                            funnyLine = "Here goes nothing—see you on the other side... hopefully alive!";
                            break;             case 5:
                            funnyLine = "Off to find some mythical loot. Wish me luck!";
                            break;             case 6:
                            funnyLine = "Hold my mead, I'm about to explore the unknown!";
                            break;             case 7:
                            funnyLine = "Portal jump initiated! Pray the gods are watching.";
                            break;             case 8:
                            funnyLine = "If I don't come back, tell my axe I loved it.";
                            break;             case 9:
                            funnyLine = "Embarking on a mystical journey. Send reinforcements if needed!";
                            break;             case 10:
                            funnyLine = "Here’s to hoping this portal leads to more loot and fewer enemies!";
                            break;             case 11:
                            funnyLine = "Taking the scenic route through a portal. What could possibly go wrong?";
                            break;

                            default: funnyLine = "One small step for a Viking";
                                break;

                        }
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, funnyLine);
                        string whatI = " did a random teleport: ";
                        Chat.instance.SendText(Talker.Type.Shout, whatI + funnyLine);

                        var points = functions.GenerateRandomPoint();
                        ((Character)Player.m_localPlayer).TeleportTo(new Vector3((float)points.x, Random.Range(0.5f, 3f), (float)points.y), Quaternion.identity, true);
                        ((Character)Player.m_localPlayer).m_lastGroundTouch = 0f;
                    }

                    if (portal.SpecialMode == PortalModeClass.PortalMode.TransportNetwork)
                    {
                        LastPortalTrigger = __instance.m_teleportWorld.m_nview.GetZDO().GetPosition();
                        LastPortalName = PortalName;
                        Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Warp to Location with /warp destination");
                        
                    }


                    if (portal.SpecialMode == PortalModeClass.PortalMode.TargetPortal) // only this one doesn't throw exception
                        cancelTargetPortal = false;


                    if (cancelTargetPortal && MagicPortalFluid.TargetPortalLoaded)
                    {

                            ZLog.Log("Teleportation TRIGGER from Mod");
                        __instance.m_teleportWorld.Teleport(colliderIn.GetComponent<Player>());

                        MagicPortalFluid.Teleporting = false;
                        throw new SkipPortalException();
                    }

                    MagicPortalFluid.Teleporting = true; // activates map for targetportal 
                    return true; // otherwise

                }
                else
                {
                    MagicPortalFluid.Teleporting = false;

                    if (MagicPortalFluid.TargetPortalLoaded)
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
                if (MagicPortalFluid.Teleporting && MagicPortalFluid.TargetPortalLoaded)
                {
                    UpdatePortalIcons();
                }
            }

            // Helper method to update portal icons
            private static void UpdatePortalIcons()
            {
                try
                {
                    //Minimap minimap = Minimap.instance;
                    //List<Minimap.PinData> pins = minimap.m_pins;
                   // MagicPortalFluid.RareMagicPortal.LogWarning(" Made it to Update Portal Icons");

                    // Get TargetPortal.Map's activePins property
                    var activePins = functions.GetActivePins();
                    if (activePins == null) return;

                    MagicPortalFluid.RareMagicPortal.LogWarning("Got TargetPortal Icons count " + activePins.Count() + " Going to reduce and color");
                    HashSet<Vector3> existingPins = new(activePins.Keys.Select(p => p.m_pos));

                    foreach (var know in MagicPortalFluid.PortalsKnown) // PortalsKnown key is zdo.toString()
                    {
                        string PortalName = know.Value.GetString("tag");
                        string zdoID = know.Value.GetString(MagicPortalFluid._portalID);
                       // MagicPortalFluid.RareMagicPortal.LogWarning("known "+ zdoID);
                        if (zdoID != "") //  if blank or if default is target Portal maybe
                        {             
                            int colorint = PortalColorLogic.CrystalandKeyLogicColor(
                                out string currentColor,
                                out Color currentColorHex,
                                out string nextColor,
                                PortalName,
                                zdoID
                            );                          
                            var portal = PortalColorLogic.PortalN.Portals[PortalName];
                            var portalZDO = portal.PortalZDOs[zdoID];
                            if (portalZDO.SpecialMode == PortalModeClass.PortalMode.TargetPortal) {
                               // MagicPortalFluid.RareMagicPortal.LogWarning(portalZDO + " Is in targetPortal Mode");
                                if (existingPins.Contains(know.Value.m_position))
                                {
                                    existingPins.Remove(know.Value.m_position);
                                   // MagicPortalFluid.RareMagicPortal.LogWarning("      Removed ");
                                }
                            }
                            //MagicPortalFluid.RareMagicPortal.LogWarning(" colorint for this icon " + colorint);
                            foreach (var pin in activePins.Keys)
                            {
                                if (pin.m_pos == know.Value.m_position)
                                {
                                    pin.m_icon = colorint == 0 || colorint == 999
                                        ? MagicPortalFluid.IconDefault
                                        : MagicPortalFluid.Icons[((PortalColorLogic.PortalColor)colorint).ToString()];
                                }
                            }
                        } else
                        {           
                            // remove all unZdoID portals
                        }
                    }
                    List<Minimap.PinData> remove = activePins.Keys.Where(p => existingPins.Contains(p.m_pos)).ToList();
                    foreach (Minimap.PinData pin in remove)
                    {
                        Minimap.instance.RemovePin(pin);
                        activePins.Remove(pin);
                    }

                    /*
                    foreach (var pin in activePins) // icons left
                    {
                        string zdoID = pin.Value.GetString(MagicPortalFluid._portalID);
                        MagicPortalFluid.RareMagicPortal.LogWarning("pins Left in active"+ zdoID);
                    } */
                }
                catch (Exception ex)
                {
                    // RMP.LogInfo($"Error in UpdatePortalIcons: {ex.Message}");
                }
            }

        }

        private static void OnChatMessage(string message)
        {
            if (message.StartsWith("warp"))
            {
                string destination = message.Substring(4).Trim().ToLower();
                if (PortalColorLogic.PortalN.Portals.ContainsKey(destination))
                {
                    Vector3 playerPosition = Player.m_localPlayer.transform.position;
                    Vector3 portalPosition = LastPortalTrigger;

                    if (LastPortalName == destination)
                    {

                    }
                    //MagicPortalFluid.RareMagicPortal.LogWarning(playerPosition + " Player Posistion vs Portal Position "+ portalPosition);
                    float distance = Vector3.Distance(playerPosition, portalPosition);
                    float someThreshold = 5f;

                    if (distance < someThreshold)
                    {
                        MagicPortalFluid.RareMagicPortal.LogInfo("Player IS close enough to Tele Network");

                        var target = PortalColorLogic.PortalN.Portals[destination];
                        string cords = "";
                        foreach (var zd in target.PortalZDOs)
                        {
                            if (zd.Value.Active)
                                cords = zd.Value.Coords;
                        }

                        if (PortalModeClass.TryParseCoordinates(cords, out Vector3 targetCoords))
                        {
                            MagicPortalFluid.RareMagicPortal.LogInfo("Teleporting with Warp");
                            PerformTeleport(targetCoords);
                        }
                        else
                        {
                            MagicPortalFluid.RareMagicPortal.LogWarning("Invalid portal coordinates.");
                        }
                    }
                    else
                    {
                        MagicPortalFluid.RareMagicPortal.LogInfo("Player is NOT close enough to Tele Network");
                    }
                }
                else
                {
                    // say nothing
                }
            }
            
        }

        private static void PerformTeleport(Vector3 targetCoords)
        {
            Player player = Player.m_localPlayer;

            // Visual Effect - Flash screen white
            // FlashScreenWhite();
            Transform parent = player.transform.Find("Visual/Armature/Hips/LeftUpLeg/LeftLeg/LeftFoot/LeftToeBase/LeftToeBase_end");
            Quaternion targetRotation = Quaternion.Euler(0f, 0f, 0f); 
            parent.rotation = targetRotation;

            Transform vfx = null;
            MagicPortalFluid.RareMagicPortal.LogInfo("fx now");
            vfx = MagicPortalFluid.Instantiate(MagicPortalFluid.fxRMP, parent).transform;       
            vfx.localPosition = new Vector3(0f, -0.01f, 0f); 
            vfx.localScale = new Vector3(0.01352632f, 0.01352632f, 0.01352632f);

            MagicPortalFluid.context.StartCoroutine(FlyWithDelay(player, targetCoords));     
            MagicPortalFluid.context.StartCoroutine(TeleportWithDelay(player, targetCoords));



        }        
        
        private static IEnumerator TeleportWithDelay(Player player, Vector3 targetCoords)
        {
            yield return new WaitForSeconds(5.5f);
            player.TeleportTo(targetCoords +new Vector3(1,0,1), Quaternion.identity, true);
        }

        private static IEnumerator FlyWithDelay(Player player, Vector3 targetCoords)
        {
            yield return new WaitForSeconds(4.5f);
            float ascendSpeed = 5f;
            float ascendDuration = 1f;
            Rigidbody playerRigidbody = player.GetComponent<Rigidbody>();
            playerRigidbody.velocity = new Vector3(0, 5f, 0);
            MagicPortalFluid.context.StartCoroutine(AscendCoroutine(playerRigidbody, ascendSpeed, ascendDuration));
        }



        private static IEnumerator AscendCoroutine(Rigidbody playerRigidbody, float speed, float duration)
        {
            float elapsedTime = 0f;
            while (elapsedTime < duration)
            {
                playerRigidbody.velocity = new Vector3(0, speed, 0); // Apply constant upward velocity
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            playerRigidbody.velocity = Vector3.zero; // Stop upward movement after the duration
        }


        [HarmonyPatch(typeof(Terminal), nameof(Terminal.TryRunCommand))]
        public static class Chat_TerminalMessage_Patch
        {
            public static void Postfix( string text )
            {
                // Check if the input command is a custom command
                if (text.StartsWith("warp"))
                {
                    OnChatMessage(text);
                }
            }
        }
    }
}
