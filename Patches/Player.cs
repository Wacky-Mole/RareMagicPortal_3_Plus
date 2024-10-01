using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RareMagicPortal;
using BepInEx.Bootstrap;
using System.Runtime.Remoting.Contexts;
using UnityEngine;
using static RareMagicPortal.PortalName;
using YamlDotNet.Serialization;
using System.IO;
using ItemManager;
using System.ComponentModel;
using YamlDotNet.Core.Tokens;

namespace RareMagicPortal_3_Plus.Patches
{
    internal class PlayerPatches
    {


        [HarmonyPatch(typeof(ZNetScene), "Awake")]
        [HarmonyPriority(Priority.Last)]
        private static class ZNetScene_Awake_PatchWRare
        {
            private static void Postfix()
            {
                {
                    MagicPortalFluid.Worldname = ZNet.instance.GetWorldName();// for singleplayer  // won't be ready for multiplayer
                    MagicPortalFluid.TargetPortalLoaded = Chainloader.PluginInfos.ContainsKey("org.bepinex.plugins.targetportal");

                    MagicPortalFluid.RareMagicPortal.LogInfo("Setting MagicPortal Fluid Afterdelay");
                    if (ZNet.instance.IsDedicated() && ZNet.instance.IsServer())
                    {
                        MagicPortalFluid.LoadIN();
                    }
                    else // everyone else
                    {
                        functions.GetAllMaterials();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(ZNet), "Shutdown")]
        internal class PatchZNetDisconnect
        {
            internal static bool Prefix()
            {
                MagicPortalFluid.RareMagicPortal.LogInfo("Logoff? Save text file, don't delete");

                //MagicPortalFluid.context.StopCoroutine(MagicPortalFluid.RemovedDestroyedTeleportWorldsCoroutine());
                //context.StopCoroutine(myCoroutineRMP);

                MagicPortalFluid.NoMoreLoading = true;
                MagicPortalFluid.JustWaitforInventory = true;

                if (ZNet.instance.IsServer() && ZNet.instance.IsDedicated() && MagicPortalFluid.RiskyYMLSave.Value == MagicPortalFluid.Toggle.On)
                {
                    var serializer = new SerializerBuilder()
                     .Build();
                    var yamlfull = MagicPortalFluid.WelcomeString + Environment.NewLine + serializer.Serialize(PortalColorLogic.PortalN); // build everytime

                    MagicPortalFluid.JustWrote = 1;
                    File.WriteAllText(MagicPortalFluid.YMLCurrentFile, yamlfull); //overwrite
                    string lines = "";
                    foreach (string line in System.IO.File.ReadLines(MagicPortalFluid.YMLCurrentFile)) // rethrough lines manually and add spaces, stupid
                    {
                        lines += line + Environment.NewLine;
                        if (line.Contains("Admin_only_Access")) // three spaces for non main objects
                        { lines += Environment.NewLine; }
                    }
                    File.WriteAllText(MagicPortalFluid.YMLCurrentFile, lines); //overwrite with extra goodies

                    MagicPortalFluid.JustWrote = 2;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.IsTeleportable))]
        public static class Is_Teleportable
        {
            [HarmonyPriority(Priority.LowerThanNormal)]
            internal static bool Prefix(ref bool __result, ref Inventory __instance)
            {
               //MagicPortalFluid.RareMagicPortal.LogInfo("Tele Check 1");
                if (__instance == null || Player.m_localPlayer == null || MagicPortalFluid.JustWaitforInventory)
                {
                    return true;
                }
               // MagicPortalFluid.RareMagicPortal.LogInfo("Tele Check 2");

                bool teleportAllowed = false;
                bool drinkActive = Player.m_localPlayer.m_seman.HaveStatusEffect("yippeTele".GetStableHashCode());

                if (drinkActive)
                {
                    teleportAllowed = true;
                }

              //  MagicPortalFluid.RareMagicPortal.LogInfo("Tele Check 3");
                Vector3 playerPosition = Player.m_localPlayer.transform.position;
                List<Piece> piecesFound = new List<Piece>();
                functions.GetAllTheDamnPiecesinRadius(playerPosition, 5f, piecesFound);

                //MagicPortalFluid.RareMagicPortal.LogInfo("Tele Check 4");
                Piece portalPiece = piecesFound.FirstOrDefault(piece => piece.TryGetComponent<TeleportWorld>(out _));
                if (portalPiece == null)
                {
                    return true; // No portal nearby
                }

                TeleportWorld portalW = portalPiece?.GetComponent<TeleportWorld>();
                if (portalW == null)
                {
                    return true; // Portal component not found
                }
                //MagicPortalFluid.RareMagicPortal.LogInfo("Tele Check 5");
                string portalName = portalW.GetText();
                if (string.IsNullOrEmpty(portalName))
                {
                    return true; // Invalid portal
                }

                if (!PortalColorLogic.PortalN.Portals.TryGetValue(portalName, out var portalData))
                {
                    return true; // Portal not found in the dictionary
                }
                //MagicPortalFluid.RareMagicPortal.LogInfo("Tele Check 6");
                string zdoID = portalW.m_nview.GetZDO().GetString(MagicPortalFluid._portalID);
                if (string.IsNullOrEmpty(zdoID) || !portalData.PortalZDOs.TryGetValue(zdoID, out var portalZDO))
                {
                    return true; // ZDO not found or invalid
                }
                string currentColor = portalZDO.Color;

               // MagicPortalFluid.RareMagicPortal.LogInfo("Tele Check 7");
                if (portalData.TeleportAnything )
                {
                    teleportAllowed = true;
                }
                //MagicPortalFluid.RareMagicPortal.LogInfo("Tele Check 8");
                if (!teleportAllowed && portalData.AdditionalProhibitItems != null)
                {
                    if (portalData.AdditionalProhibitItems.Count > 0)
                    {
                        var prohibitedItem = __instance.GetAllItems().FirstOrDefault(item =>
                        {
                            var prefab = ObjectDB.instance.GetItemPrefab(item.m_shared.m_name);
                            return prefab != null && portalData.AdditionalProhibitItems.Contains(prefab.name);
                        });

                        if (prohibitedItem != null)
                        {
                            Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Prohibited item detected: " + prohibitedItem.m_shared.m_name);
                            __result = false;
                            return false;
                        }
                    }
                }
                //MagicPortalFluid.RareMagicPortal.LogInfo("Tele Check 9");
                if (!teleportAllowed && MagicPortalFluid.ConfigMaxWeight.Value > 0)
                {
                    float playerWeight = __instance.GetTotalWeight();
                    if (playerWeight > MagicPortalFluid.ConfigMaxWeight.Value)
                    {
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "You are carrying too much, max is " + MagicPortalFluid.ConfigMaxWeight.Value);
                        MagicPortalFluid.TeleportingforWeight = Math.Min(MagicPortalFluid.TeleportingforWeight + 1, 10);
                        __result = false;
                        return false;
                    }
                    MagicPortalFluid.TeleportingforWeight = 0;
                }
                //MagicPortalFluid.RareMagicPortal.LogInfo("Tele Check 10");
                if (teleportAllowed && (!drinkActive || MagicPortalFluid.PortalDrinkDenyloc.Count == 0))
                {
                    __result = true;
                    return false;
                }

                if (drinkActive)
                {
                    var prohibitedItem = __instance.GetAllItems()
                        .FirstOrDefault(itemb => MagicPortalFluid.PortalDrinkDenyloc.Contains(ObjectDB.instance.GetItemPrefab(itemb.m_shared.m_name)?.name));

                    if (prohibitedItem != null)
                    {
                        Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Odin still doesn't allow carrying " + prohibitedItem.m_shared.m_name + " with Portal Drink");
                        __result = false;
                        return false;
                    }
                    __result = true;
                    return false;
                }


                if (portalData.AdditionalAllowItems != null && portalData.AdditionalAllowItems.Count > 0)
                {
                    if (ZoneSystem.instance.GetGlobalKey(GlobalKeys.TeleportAll))
                    {
                        // If a global key allows teleport for all items, we can return true directly
                        __result = true;
                        return false;
                    }

                    foreach (ItemDrop.ItemData item in __instance.m_inventory)
                    {
                        // If an item is normally not teleportable
                        if (!item.m_shared.m_teleportable)
                        {
                            if (portalData.AdditionalAllowItems.Contains(item.m_shared.m_name))
                                continue;

                            Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Cannot teleport due to item: " + item.m_shared.m_name);
                            __result = false;
                            return false;
                        }
                    }
                    __result = true;
                    return false;
                }
                return true;
            }
        }





        [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
        internal static class ZrouteMethodsClientRMP

        {
            internal static void Prefix()
            {
                ZRoutedRpc.instance.Register("RequestServerAnnouncementRMP", new Action<long, ZPackage>(functions.RPC_RequestServerAnnouncementRMP)); // Our Server Handler
                ZRoutedRpc.instance.Register("RequestServerAnnouncementRMPZDOFULL", new Action<long, ZPackage>(functions.RPC_RequestServerAnnouncementRMPZDOFULL)); // Our Server Handler
                //((MonoBehaviour)(object)MagicPortalFluid.context).StartCoroutine(MagicPortalFluid.RemovedDestroyedTeleportWorldsCoroutine()); // moved to this incase the stop and start joining
                                                                                                            //ZRoutedRpc.instance.Register("EventServerAnnouncementRMP", new Action<long, ZPackage>(RPC_EventServerAnnouncementRMP)); // event handler
            }
        }

        [HarmonyPatch(typeof(ZNet), "OnDestroy")]
        internal class PatchZNetDestory
        {
            internal static void Postfix()
            { // The Server send once last config sync before destory, but after Shutdown which messes stuff up.
                MagicPortalFluid.NoMoreLoading = false;
            }
        }

        [HarmonyPatch(typeof(Game), nameof(Game.SpawnPlayer))]
        public static class PlayerspawnExtarWait
        {
            private static void Postfix()
            {

                // Coroutine coroutine = context.StartCoroutine(WaitforMe()); // maybe
            }

        }

        [HarmonyPatch(typeof(Game), "SpawnPlayer")]
        internal static class Game_SpawnPreRMP
        {
            [HarmonyPrefix]
            internal static void Prefix()
            {
                {
                    MagicPortalFluid.LoadAllRecipeData(reload: true);
                    MagicPortalFluid.LoadIN();
                }
            }
        }

        [HarmonyPatch(typeof(Game), "SpawnPlayer")]
        internal static class Game_OnNewCharacterDone_Patch
        {
            [HarmonyPostfix]
            internal static void Postfix()
            {
                {
                    MagicPortalFluid.JustWaitforInventory = false;
                    MagicPortalFluid.StartingitemPrefab();

                    //((MonoBehaviour)(object)context).StartCoroutine(DelayedLoad()); // important
                }
            }
        }

        [HarmonyPatch(typeof(FejdStartup), "OnNewCharacterDone")]
        internal static class FejdStartup_OnNewCharacterDone_Patch
        {
            internal static void Postfix()
            {
                MagicPortalFluid.StartingFirsttime();
            }
        }




    }
}
