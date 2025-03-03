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
using RareMagicPortalPlus.PortalScreens;
using YamlDotNet.Core.Tokens;
using RareMagicPortalPlus.Patches;

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
                MagicPortalFluid.RareMagicPortal.LogInfo("Logoff? Saving YML file");

                MagicPortalFluid.NoMoreLoading = true;
                MagicPortalFluid.JustWaitforInventory = true;
                PortalColorLogic.startupwait = 0;

                if (ZNet.instance.IsServer())
                {
                    var serializer = new SerializerBuilder()
                     .Build();
                    var yamlfull = MagicPortalFluid.WelcomeString + Environment.NewLine + serializer.Serialize(PortalColorLogic.PortalN); // build everytime
                    
                    MagicPortalFluid.JustWrote = 1;

                    // Adding extra newlines for readability directly to the content
                    string[] lines = yamlfull.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
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
                return true;
            }
        }


        [HarmonyPatch(typeof(Inventory), nameof(Inventory.IsTeleportable))]
        public static class Is_TeleportableRMP
        {
            [HarmonyPriority(Priority.LowerThanNormal)]
            internal static bool Prefix(ref bool __result, ref Inventory __instance)
            {
                if (__instance == null)
                {
                    MagicPortalFluid.RareMagicPortal.LogInfo("Inventory instance is null - aborting teleport check.");
                    __result = false;
                    return false;
                }

                if (Player.m_localPlayer == null)
                {
                    MagicPortalFluid.RareMagicPortal.LogInfo("Player.m_localPlayer is null - aborting teleport check.");
                    __result = false;
                    return false;
                }

                if (MagicPortalFluid.JustWaitforInventory)
                {
                    return true;
                }

                if (ZoneSystem.instance == null)
                {
                    MagicPortalFluid.RareMagicPortal.LogInfo("ZoneSystem.instance is null - skipping teleport check.");
                    return true;
                }

                bool teleportAllowed = false;
                bool drinkActive = Player.m_localPlayer.m_seman?.HaveStatusEffect("yippeTele".GetStableHashCode()) ?? false;

                if (drinkActive)
                {
                    teleportAllowed = true;
                }

                if (ZoneSystem.instance.GetGlobalKey(GlobalKeys.TeleportAll))
                    return true;

                Vector3 playerPosition = Player.m_localPlayer.transform.position;
                List<Piece> piecesFound = new List<Piece>();
                functions.GetAllTheDamnPiecesinRadius(playerPosition, 5f, piecesFound);

                Piece portalPiece = piecesFound.FirstOrDefault(piece => piece.TryGetComponent<TeleportWorld>(out _));
                if (portalPiece == null)
                {
                    return true; // No portal nearby
                }

                TeleportWorld portalW = portalPiece.GetComponent<TeleportWorld>();
                if (portalW == null)
                {
                    return true; // Portal component not found
                }

                string portalName = portalW.GetText();
                if (portalName == null)
                {
                    return true; // Invalid portal
                }

                if (PortalColorLogic.PortalN == null || PortalColorLogic.PortalN.Portals == null)
                {
                    MagicPortalFluid.RareMagicPortal.LogInfo("PortalColorLogic.PortalN or Portals is null.");
                    return true;
                }

                if (!PortalColorLogic.PortalN.Portals.TryGetValue(portalName, out var portalData))
                {
                    return true; // Portal not found in the dictionary
                }

                if (portalW.m_nview == null || portalW.m_nview.GetZDO() == null)
                {
                    MagicPortalFluid.RareMagicPortal.LogInfo("portalW.m_nview or ZDO is null.");
                    return true;
                }

                string zdoID = portalW.m_nview.GetZDO().GetString(MagicPortalFluid._portalID);
                if (string.IsNullOrEmpty(zdoID) || !portalData.PortalZDOs.TryGetValue(zdoID, out var portalZDO))
                {
                    return true; // ZDO not found or invalid
                }

                string currentColor = portalZDO.Color;
                if (!string.IsNullOrEmpty(portalZDO.BiomeColor) && portalZDO.BiomeColor != "skip")
                {
                    currentColor = portalZDO.BiomeColor;
                }

                if (portalData.TeleportAnything)
                {
                    teleportAllowed = true;
                }

                if (!teleportAllowed && portalData.AdditionalProhibitItems?.Count > 0)
                {
                    if (ObjectDB.instance == null)
                    {
                        MagicPortalFluid.RareMagicPortal.LogInfo("ObjectDB.instance is null - skipping item check.");
                        return true;
                    }

                    var prohibitedItem = __instance.GetAllItems().FirstOrDefault(item =>
                    {
                        var prefab = ObjectDB.instance.GetItemPrefab(item.m_shared.m_name);
                        return prefab != null && portalData.AdditionalProhibitItems.Contains(prefab.name);
                    });

                    if (prohibitedItem != null)
                    {
                        Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "$rmp_prohibited_item " + prohibitedItem.m_shared.m_name);
                        __result = false;
                        return false;
                    }
                }

                if (!teleportAllowed && MagicPortalFluid.ConfigMaxWeight.Value > 0)
                {
                    float playerWeight = __instance.GetTotalWeight();
                    if (playerWeight > MagicPortalFluid.ConfigMaxWeight.Value)
                    {
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$rmp_you_are_carrytoomuch " + MagicPortalFluid.ConfigMaxWeight.Value);
                        MagicPortalFluid.TeleportingforWeight = Math.Min(MagicPortalFluid.TeleportingforWeight + 1, 10);
                        __result = false;
                        return false;
                    }
                    MagicPortalFluid.TeleportingforWeight = 0;
                }

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
                        Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "$rmp_odin_doesnt_allow " + prohibitedItem.m_shared.m_name + " $rmp_with_portaldrink");
                        __result = false;
                        return false;
                    }
                    __result = true;
                    return false;
                }

                if (ZoneSystem.instance.GetGlobalKey(GlobalKeys.TeleportAll))
                {
                    __result = true;
                    return false;
                }

                if (portalZDO.CrystalActive || MagicPortalFluid.PreventColorChange.Value == MagicPortalFluid.Toggle.On || portalData.AdditionalAllowItems != null)
                {
                    // Define a dictionary mapping color names to configuration values as strings
                    var colorAllowsConfig = new Dictionary<string, string>
                    {
                        { "yellow", MagicPortalFluid.ColorYELLOWAllows.Value },
                        { "blue", MagicPortalFluid.ColorBLUEAllows.Value },
                        { "green", MagicPortalFluid.ColorGREENAllows.Value },
                        { "purple", MagicPortalFluid.ColorPURPLEAllows.Value },
                        { "tan", MagicPortalFluid.ColorTANAllows.Value },
                        { "cyan", MagicPortalFluid.ColorCYANAllows.Value },
                        { "orange", MagicPortalFluid.ColorORANGEAllows.Value },
                        { "black", MagicPortalFluid.ColorBLACKAllows.Value },
                        { "white", MagicPortalFluid.ColorWHITEAllows.Value },
                        { "gold", MagicPortalFluid.ColorGOLDAllows.Value }
                    };

                    // Start with the items from portalData.AdditionalAllowItems, if any
                    List<string> additionalAllows = portalData.AdditionalAllowItems != null
                        ? new List<string>(portalData.AdditionalAllowItems)
                        : new List<string>();

                    // Retrieve and split the color-specific allows, if applicable
                    if (colorAllowsConfig.TryGetValue(currentColor.ToLower(), out var configValue))
                    {
                        var colorSpecificAllows = configValue.Split(',')
                                                             .Select(s => s.Trim())  // Trim each item
                                                             .Where(s => !string.IsNullOrEmpty(s))
                                                             .ToList();
                        additionalAllows.AddRange(colorSpecificAllows);
                    }

                    if (portalW.m_allowAllItems)
                    {
                        __result = true;
                        return false;
                    }

                    // Remove any duplicates
                    additionalAllows = additionalAllows.Distinct().ToList();
                    // MagicPortalFluid.RareMagicPortal.LogInfo("additionalAllows contains:" + string.Join(",", additionalAllows));
                    foreach (var item in __instance.m_inventory)
                    {
                        if (!item.m_shared.m_teleportable && !additionalAllows.Contains(item.m_dropPrefab.name))
                        {
                            Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "$rmp_cant_teleport_due " + item.m_dropPrefab.name);
                            // MagicPortalFluid.RareMagicPortal.LogInfo("Cannot teleport:" + item.m_dropPrefab.name);
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
                ZRoutedRpc.instance.Register("RMPP Teleport Boat", new Action<long,Vector3>(Ships.HandleRemoteTeleport));
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


        [HarmonyPatch(typeof(Player), "OnDestroy")]
        private static class PlayerDeathRMPP
        {
            private static void Postfix()
            {
                if (Player.m_localPlayer == null)
                {
                    ps_patches.setBackgroundblack();
                    PortalColorLogic.deathwait = 0;
                }
                    
            }

        }
    }
}
