using BepInEx.Configuration;
using BepInEx;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using YamlDotNet.Serialization;
using RareMagicPortal;
using UnityEngine.Rendering;

namespace RareMagicPortalPlus
{
    internal class PortalLimit
    {
        private static SyncedList VIPplayersList;
        private static PortalManager _portalManager;
        private static FileSystemWatcher fsw;
        private static string pathPortalData;
        
        

        [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
        private static class ZNetScene_Awake_Patch
        {
            private static void Postfix(ZNetScene __instance)
            {

                if (MagicPortalFluid.isServer)
                {
                    ZRoutedRpc.instance.Register("WackyPortal Portalplaced", PortalPlaced);
                    return;
                }

                ZRoutedRpc.instance.Register("RMPportal Data", new Action<long, bool>(ReceiveData_RMPPortals));

                List<GameObject> hammer = __instance.GetPrefab("Hammer").GetComponent<ItemDrop>().m_itemData.m_shared
                    .m_buildPieces
                    .m_pieces;


            }

            private static void PortalPlaced(long sender)
            {
                ZNetPeer peer = ZNet.instance.GetPeer(sender);
                if (peer == null) return;
                var id = peer.m_socket.GetHostName();
                if (_portalManager.PlayersPortalData.ContainsKey(id))
                {
                    _portalManager.PlayersPortalData[id]++;
                }
                else
                {
                    _portalManager.PlayersPortalData[id] = 1;
                }

                _portalManager.Save();
                ZRoutedRpc.instance.InvokeRoutedRPC(peer.m_uid, "RMPportal Data", _portalManager.CanBuildPortal(id));
            }

            private static void ReceiveData_RMPPortals(long sender, bool data)
            {
                MagicPortalFluid._canPlacePortal = data;
            }
        }



        [HarmonyPatch(typeof(Game), nameof(Game.Start))]
        static class Game_Start_PatchRMPPortal
        {
            static void Postfix()
            {
                MagicPortalFluid._canPlacePortal = false;
            }
        }

        [HarmonyPatch(typeof(ZNet), "RPC_PeerInfo")]
        private static class ZnetSync
        {
            private static void Postfix(ZRpc rpc)
            {
                if (!(ZNet.instance.IsServer() && ZNet.instance.IsDedicated())) return;
                ZNetPeer peer = ZNet.instance.GetPeer(rpc);
                string id = peer.m_socket.GetHostName();
                ZRoutedRpc.instance.InvokeRoutedRPC(peer.m_uid, "RMPportal Data", _portalManager.CanBuildPortal(id));
            }
        }


        [HarmonyPatch(typeof(ZDOMan), nameof(ZDOMan.HandleDestroyedZDO))]
        static class ZDOMan_Patch
        {
            private static readonly int WackyPortal_id = "WackyPortal_id".GetStableHashCode();

            static void Prefix(ZDOMan __instance, ZDOID uid)
            {
                if (!ZNet.instance.IsServer()) return;
                ZDO zdo = __instance.GetZDO(uid);
                if (zdo == null || string.IsNullOrEmpty(zdo.GetString(WackyPortal_id))) return;
                string id = zdo.GetString(WackyPortal_id);
                if (_portalManager.PlayersPortalData.ContainsKey(id))
                {
                    _portalManager.PlayersPortalData[id]--;
                    if (_portalManager.PlayersPortalData[id] < 0) _portalManager.PlayersPortalData[id] = 0;
                    ZNetPeer peer = ZNet.instance.GetPeerByHostName(id);
                    if (peer != null)
                    {
                        ZRoutedRpc.instance.InvokeRoutedRPC(peer.m_uid, "RMPportal Data",
                            _portalManager.CanBuildPortal(id));
                    }
                }

                _portalManager.Save();
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.PlacePiece))]
        static class PlacePiece_PatchRMPPre
        {
            private static bool Prefix(ref Player __instance, ref Piece piece, Vector3 pos, Quaternion rot, bool doAttack)
            {

                if (MagicPortalFluid.PortalNames.Contains(piece.name))
                {
                    if (!MagicPortalFluid._canPlacePortal && !Player.m_debugMode)
                    {
                        MessageHud.instance.ShowMessage(MessageHud.MessageType.Center,
                            "<color=red>Ward Limit</color>");
                        return false;
                    }
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(Player), nameof(Player.PlacePiece))]
        static class PlacePiece_PatchPortalPost
        {
            private static void Postfix(ref Player __instance, ref Piece piece, Vector3 pos, Quaternion rot, bool doAttack, bool __result)
            {
                if (__result)
                {
                    if (MagicPortalFluid.PortalNames.Contains(piece.name))
                    {
                        MagicPortalFluid._canPlacePortal = false;

                        var name = Game.instance.GetPlayerProfile().GetName();
                        var id = ZNet.m_onlineBackend == OnlineBackendType.Steamworks
                             ? PrivilegeManager.GetNetworkUserId().Split('_')[1]: PrivilegeManager.GetNetworkUserId();

                       var _znet =  piece.GetComponent<ZNetView>();


                        _znet.m_zdo.Set("creatorName", name);
                        _znet.m_zdo.Set("WackyPortal_id", id);

                        if (ZNet.instance.GetServerPeer() != null)
                            ZRoutedRpc.instance.InvokeRoutedRPC(ZNet.instance.GetServerPeer().m_uid, "WackyPortal Portalplaced",
                                new object[] { null });
                    }
                }
            }
        }

        private enum PlayerStatus
        {
            VIP,
            User
        }

        private static PlayerStatus GetPlayerStatus(string id)
        {
            return ZNet.instance.ListContainsId(VIPplayersList, id) ? PlayerStatus.VIP : PlayerStatus.User;
        }

        private class PortalManager
        {
            private readonly string _path;
            public readonly Dictionary<string, int> PlayersPortalData = new();


            public PortalManager(string path)
            {
                _path = path;
                if (!File.Exists(_path))
                {
                    File.Create(_path).Dispose();
                }
                else
                {
                    string data = File.ReadAllText(_path);
                    if (!string.IsNullOrEmpty(data))
                    {
                        var deserializer = new DeserializerBuilder().Build();
                        PlayersPortalData = deserializer.Deserialize<Dictionary<string, int>>(data);

                    }
                }
            }

            public bool CanBuildPortal(string id)
            {
                if (!PlayersPortalData.ContainsKey(id)) return true;
                return GetPlayerStatus(id) switch
                {
                    PlayerStatus.VIP => PlayersPortalData[id] < MagicPortalFluid.MaxAmountOfPortals_VIP.Value,
                    PlayerStatus.User => PlayersPortalData[id] < MagicPortalFluid.MaxAmountOfPortals.Value,
                    _ => false
                };
            }

            public void Save()
            {

                var serializer = new SerializerBuilder().Build();
                File.WriteAllText(_path, serializer.Serialize(PlayersPortalData));
            }
        }


        private void ServerSidePortalInit()
        {
            if (!Directory.Exists(MagicPortalFluid.YMLFULLFOLDER)) Directory.CreateDirectory(MagicPortalFluid.YMLFULLFOLDER);
            pathPortalData = Path.Combine(MagicPortalFluid.YMLFULLFOLDER, MagicPortalFluid.Worldname + "_PlayerPortals.json");
            _portalManager = new PortalManager(pathPortalData);
            var VIPPath = Path.Combine(MagicPortalFluid.YMLFULLFOLDER, "VIPplayers.txt");
            VIPplayersList = new SyncedList(VIPPath, "");



            fsw = new FileSystemWatcher(pathPortalData)
            {
                Filter = Path.GetFileName(pathPortalData),
                EnableRaisingEvents = true,
                //IncludeSubdirectories = true,
                SynchronizingObject = ThreadingHelper.SynchronizingObject
            };
            //fsw.Changed += ConfigChanged;
        }

        private void ConfigChanged(object sender, FileSystemEventArgs e)
        {
            MagicPortalFluid.print($"[RareMagic Portal] RMP Portal Count changed...");
           // MagicPortalFluid.context.StartCoroutine(DelayReloadConfigFile(pathPortalData));
        }

        private static IEnumerator DelayReloadConfigFile(ConfigFile file)
        {
            yield return new WaitForSecondsRealtime(2.5f);
            file.Reload();
        }






























    }
}
