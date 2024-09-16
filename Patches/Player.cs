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

        [HarmonyPatch(typeof(Inventory), "IsTeleportable")]
        public static class istele     
        {
            [HarmonyPriority(Priority.LowerThanNormal)]
            internal static bool Prefix(ref bool __result, ref Inventory __instance)
            {
                if (__instance == null || Player.m_localPlayer == null)
                    return true;
                if (MagicPortalFluid.JustWaitforInventory)
                    return true;

                bool bo2 = false;
                bool drinkactive = false;
                if (Player.m_localPlayer.m_seman.HaveStatusEffect("yippeTele".GetStableHashCode()))
                {
                    bo2 = true;
                    drinkactive = true;
                }

                //RareMagicPortal.LogInfo("Here 1");
                Piece portal = null;
                String name = null;
                Vector3 hi = Player.m_localPlayer.transform.position;
                List<Piece> piecesfound = new List<Piece>();
                functions.GetAllTheDamnPiecesinRadius(hi, 5f, piecesfound);

                TeleportWorld portalW = null;
                foreach (Piece piece in piecesfound)
                {
                    if (piece.TryGetComponent<TeleportWorld>(out portalW))
                    {
                        break;
                    }
                }
                // RareMagicPortal.LogInfo("Here 2");

                if (portalW != null)
                {
                    //portalW = portal.GetComponent<TeleportWorld>();
                    name = portalW.GetText(); // much better
                    //RareMagicPortal.LogInfo("Here 2.1");
                    if (name != null)
                    {
                        var PortalName = name;
                        bool OdinsKin = false;
                        bool Free_Passage = false;
                        bool TeleportAny = false;
                        List<string> AdditionalProhibitItems;

                        string BiomeC = "";
                        string currentColor = "";
                        var flag = false;
                        if (PortalName.Contains(PortalColorLogic.NameIdentifier))
                        {
                            BiomeC = PortalName.Substring(PortalName.IndexOf(PortalColorLogic.NameIdentifier));//
                            var BiomeC1 = PortalName.Substring(PortalName.IndexOf(PortalColorLogic.NameIdentifier) + 1);
                            var index = PortalName.IndexOf(PortalColorLogic.NameIdentifier);
                            PortalName = PortalName.Substring(0, index);
                            flag = true;

                            int intS = Int32.Parse(BiomeC1);
                            PortalColorLogic.PortalColor pcol = (PortalColorLogic.PortalColor)intS;
                            currentColor = pcol.ToString();
                        }
                        

                        if (!PortalColorLogic.PortalN.Portals.ContainsKey(PortalName)) // if doesn't contain use defaults
                        {
                            PortalColorLogic.WritetoYML(PortalName, portalW.m_nview.m_zdo.ToString());
                        }
                        //RareMagicPortal.LogInfo("Here 3");
                        OdinsKin = PortalColorLogic.PortalN.Portals[PortalName].Admin_only_Access;
                        Free_Passage = PortalColorLogic.PortalN.Portals[PortalName].Free_Passage;
                        TeleportAny = PortalColorLogic.PortalN.Portals[PortalName].TeleportAnything;
                        AdditionalProhibitItems = PortalColorLogic.PortalN.Portals[PortalName].AdditionalProhibitItems;
                        var Playerlist = PortalColorLogic.PortalN.Portals[PortalName].AllowedUsers;
                        if (Playerlist.Count > 0) // block any teleport for a player not on list
                        {
                            var found = false;
                            foreach (var playerc in Playerlist)
                            {
                                if (playerc == Player.m_localPlayer.GetPlayerName())
                                {
                                    found = true;
                                }
                            }
                            if (!found)
                            {
                                MagicPortalFluid.RareMagicPortal.LogInfo($"Player is not in the Allowed List for " + PortalName);
                                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "Odin Deems " + Player.m_localPlayer.GetPlayerName() + " Not Worthy!");
                                __result = false;
                                return false;
                            }
                        }

                        if (TeleportAny && !flag || currentColor == MagicPortalFluid.TelePortAnythingColor.Value) // allows for teleport anything portal if EnableCrystals otherwise just white // currentcolor is often ""
                            bo2 = true;

                        //RareMagicPortal.LogInfo("Here 4");
                        if (!bo2 && AdditionalProhibitItems.Count > 0)
                        {
                            var instan = ObjectDB.instance;
                            foreach (ItemDrop.ItemData allItem in __instance.GetAllItems())
                            {
                                foreach (var item in AdditionalProhibitItems)
                                {
                                    GameObject go = instan.GetItemPrefab(item);
                                    if (go != null)
                                    {
                                        ItemDrop.ItemData data = go.GetComponent<ItemDrop>().m_itemData;
                                        if (data != null)
                                        {
                                            if (data.m_shared.m_name == allItem.m_shared.m_name)
                                            {
                                                MagicPortalFluid.RareMagicPortal.LogInfo($"Found Prohibited item {data.m_shared.m_name} Teleport Not Allowed");
                                                Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Prohibited item " + data.m_shared.m_name);
                                                __result = false;
                                                return false;
                                            }
                                        }
                                    }
                                }
                            }
                        }// end !bo2
                        //RareMagicPortal.LogInfo("Here 5");
                        if (!bo2 && MagicPortalFluid.ConfigMaxWeight.Value > 0 && (MagicPortalFluid.TeleportingforWeight > 0 || MagicPortalFluid.Teleporting))
                        {
                            var playerweight = __instance.GetTotalWeight();

                            if (playerweight > MagicPortalFluid.ConfigMaxWeight.Value)
                            {
                                MagicPortalFluid.RareMagicPortal.LogInfo($"Player Weight is greater than Max Portal Weight");
                                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "You are carrying too much, max is " + MagicPortalFluid.ConfigMaxWeight.Value);
                                MagicPortalFluid.TeleportingforWeight++;

                                if (MagicPortalFluid.TeleportingforWeight > 10)
                                    MagicPortalFluid.TeleportingforWeight = 0;

                                __result = false;
                                return false;
                            }
                            MagicPortalFluid.TeleportingforWeight = 0;
                        }
                        if (MagicPortalFluid.UsePortalProgression.Value == MagicPortalFluid.Toggle.On)
                        {
                            List<string> allowlist = new List<string>();
                            bool skip = false;
                            switch (currentColor)
                            {
                                case "Red": allowlist = MagicPortalFluid.PPRedAllows.Value.Replace(" ", "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList(); break;
                                case "Green": allowlist = MagicPortalFluid.PPGreenAllows.Value.Replace(" ", "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList(); break;
                                case "Blue": allowlist = MagicPortalFluid.PPBlueAllows.Value.Replace(" ", "").Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList(); break;
                                case "White":
                                    __result = true;
                                    return false;

                                default: skip = true; break;
                            }

                            if (skip)
                                return true;

                            foreach (var itemData in __instance.GetAllItems())
                            {
                                if (!itemData.m_shared.m_teleportable && itemData.m_dropPrefab != null && !allowlist.Contains(itemData.m_dropPrefab.name))
                                {
                                    __result = false;
                                    return false;
                                }
                            }

                            __result = true;
                            return false;
                        }
                    }
                }


                if (bo2) // if status effect is active or teleportany color
                {
                    if (MagicPortalFluid.PortalDrinkDenyloc.Count == 0 || !drinkactive) // might expand upon this in future
                    {
                        __result = true;
                        return false;
                    }
                    else
                    {
                        var instan = ObjectDB.instance;
                        foreach (ItemDrop.ItemData allItem in __instance.GetAllItems())
                        {
                            foreach (var item in MagicPortalFluid.PortalDrinkDenyloc)
                            {
                                GameObject go = instan.GetItemPrefab(item);
                                if (go != null)
                                {
                                    ItemDrop.ItemData data = go.GetComponent<ItemDrop>().m_itemData;
                                    if (data != null)
                                    {
                                        if (data.m_shared.m_name == allItem.m_shared.m_name)
                                        {
                                            MagicPortalFluid.RareMagicPortal.LogInfo($"Odin does not allow {data.m_shared.m_name} even with Portal Drink");
                                            Player.m_localPlayer.Message(MessageHud.MessageType.TopLeft, "Odin still doesn't allow " + data.m_shared.m_name);
                                            __result = false;
                                            return false;
                                        }
                                    }
                                }
                            }
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
