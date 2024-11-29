using HarmonyLib;
using RareMagicPortal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace RareMagicPortal_3_Plus.Patches
{
    internal class piecepatches
    {
        [HarmonyPatch(typeof(global::Player), "CheckCanRemovePiece")]
        internal static class Player_CheckforOwnerP
        {
            internal static bool Prefix(ref global::Player __instance, ref Piece piece)
            {
                if (piece == null)
                    return true;
                

                if (MagicPortalFluid.PortalNames.Contains(piece.name) && !__instance.m_noPlacementCost && MagicPortalFluid.ConfigCreator.Value) // portal and Configonly
                {
                    __instance.Message(MessageHud.MessageType.Center, "$rmp_youarenotcreator");
                    bool bool2 = piece.IsCreator();// nice
                    if (bool2)
                    { // can remove because is creator or creator only mode is On
                        return true;
                    }
                    else
                    {
                        __instance.Message(MessageHud.MessageType.Center, "$rmp_youarenotcreator");
                        return false;   
                    }
                }
                return true;    
            }
        }
        
        [HarmonyPatch(typeof(global::Player), "TryPlacePiece")]
        internal static class Player_MessageforPortal_PatchRMP
        {
            [HarmonyPrefix]
            private static bool Prefix(ref Player __instance, ref Piece piece)
            {
                if (piece == null || __instance == null) return true;

                if (MagicPortalFluid.PiecetoLookFor.Contains(piece.name) && !__instance.m_noPlacementCost) // wood_portal and stone_portal/ might remove this
                {
                    if (__instance.transform.position != null)
                        MagicPortalFluid.tempvalue = __instance.transform.position; // save position //must be assigned
                    else
                        MagicPortalFluid.tempvalue = new Vector3(0, 0, 0); 

                    var paulstation = CraftingStation.HaveBuildStationInRange(piece.m_craftingStation.m_name, MagicPortalFluid.tempvalue);
                    if (paulstation == null && !__instance.m_noPlacementCost)
                    {
                        return false; // should catch and stop it.
                    }
                    var paullvl = paulstation.GetLevel();
                    if (MagicPortalFluid.ConfigTableLvl.Value > 10 || MagicPortalFluid.ConfigTableLvl.Value < 1)
                        MagicPortalFluid.ConfigTableLvl.Value = 1;

                    if (paullvl + 1 > MagicPortalFluid.ConfigTableLvl.Value) // just for testing
                    {
                        MagicPortalFluid.piecehaslvl = true;
                    }
                    else
                    {
                        string worktablename = piece.m_craftingStation.name;
                        GameObject temp = functions.GetPieces().Find(g => Utils.GetPrefabName(g) == worktablename);
                        var name = temp.GetComponent<Piece>().m_name;
                        __instance.Message(MessageHud.MessageType.Center, "$rmp_needlvl " + MagicPortalFluid.ConfigTableLvl.Value + " " + name + " $rmp_forplacement");
                        MagicPortalFluid.piecehaslvl = false;
                        return false;
                    }
                }
                return true;
            }
        }



    }
}
