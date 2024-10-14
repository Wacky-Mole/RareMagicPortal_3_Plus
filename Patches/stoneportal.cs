using HarmonyLib;
using RareMagicPortal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RareMagicPortalPlus.Patches
{
    internal class stoneportal
    {

        const string PREFAB = "portal";

        [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake)), HarmonyPostfix]
        static void StonePortalRecipeRMP(ZNetScene __instance) => Fix(__instance);

        static void FixRecipe(ZNetScene zs, Piece piece)
        {
            if (!piece) return;
            //Log.LogInfo("Fixing Stone Portal recipe.");
            piece.m_enabled = true;
            piece.m_category = Piece.PieceCategory.Misc;
            piece.m_craftingStation = null;
            if (zs.m_namedPrefabs.TryGetValue(MagicPortalFluid.OrginalStonePortalconfigCraftingStation.Value.GetStableHashCode(), out var view))
            {
                if (view.TryGetComponent<CraftingStation>(out var craftingStation))
                    piece.m_craftingStation = craftingStation;
            }
            piece.m_description = "$piece_portal_description";
            piece.m_resources =MagicPortalFluid.OrginalStonePortalconfigRequirements.Value.Split(',').Select(s => s.Split(':')).Select(s =>
            {
                Piece.Requirement req = new();
                var id = s[0];
                if (!zs.m_namedPrefabs.TryGetValue(id.GetStableHashCode(), out var item)) return req;
                req.m_resItem = item.GetComponent<ItemDrop>();
                req.m_amount = 1;
                if (s.Length > 1)
                    int.TryParse(s[1], out req.m_amount);
                return req;
            }).Where(req => req.m_resItem).ToArray();
        }
        static void Fix(ZNetScene zs)
        {
            if (!zs) return;
            if (!zs.m_namedPrefabs.TryGetValue(PREFAB.GetStableHashCode(), out var portal)) return;
            FixRecipe(zs, portal.GetComponent<Piece>());
            if (!zs.m_namedPrefabs.TryGetValue("Hammer".GetStableHashCode(), out var hammer)) return;
            if (hammer.GetComponent<ItemDrop>() is { } item)
            {
                var pieces = item.m_itemData.m_shared.m_buildPieces.m_pieces;
                if (!pieces.Contains(portal))
                    pieces.Add(portal);
            }
        }

    }
}
