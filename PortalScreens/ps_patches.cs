using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using UnityEngine;

namespace RareMagicPortalPlus.PortalScreens
{
    internal class ps_patches
    {
        /*
        [HarmonyPatch(typeof(Hud), "Awake")]
        private class HudHud_AwakeRMP
        {
            private static void Postfix(Hud __instance)
            {
                GameObject gameObject = GameObject.Find("_GameMain").transform.Find("LoadingGUI/PixelFix/IngameGui/HUD/LoadingBlack/Teleporting/Swirl").gameObject;
                if (gameObject != null)
                {
                    Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>();
                    Transform[] array = componentsInChildren;
                    foreach (Transform gamePortalLayer in array)
                    {
                        PortalLayer portalLayer = Mod.PortalLayers.Where((PortalLayer x) => x.LayerName == gamePortalLayer.gameObject.name.FirstOrDefault());
                        if (portalLayer != null)
                        {

                            portalLayer.UpdateImageSprite(gamePortalLayer).gameObject.GetComponent<Image>());
                        }
                    }
                }
                GameObject gameObject2 = GameObject.Find("_GameMain").transform.Find("LoadingGUI/PixelFix/IngameGui/HUD/LoadingBlack/Bkg").gameObject;
                if (gameObject2 != null && Mod.BackgroundSprite != null)
                {

                    gameObject2.GetComponent<Image>().set_sprite(Mod.BackgroundSprite);
                    gameObject2.GetComponent<Image>().set_color(new Color(1f, 1f, 1f, 1f));
                }
                else
                {
                   
                }
            }
        }

        [HarmonyPatch(typeof(Uirotate), "Update")]
        private class UiRotate_UpdateRMP
        {
            private static bool Prefix(Uirotate __instance)
            {
                string name = __instance.gameObject.name;
                if (!name.Contains("layer"))
                {
                    return true;
                }
                int index = int.Parse(name.Replace("layer", "")) - 1;
                PortalLayer portalLayer = Mod.PortalLayers[index];
                switch (portalLayer.LayerType)
                {
                    case LayerType.BiomeImage:
                    case LayerType.Static:
                        return false;
                    case LayerType.Rotating:
                        portalLayer.Rotate();
                        break;
                    case LayerType.Animated:
                        portalLayer.Animate();
                        break;
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(Player), "TeleportTo")]
        private class Player_TeleportToRMP
        {
            private static void Prefix(Vector3 pos)
            {
                Heightmap.Biome biome = WorldGenerator.instance.GetBiome(pos);
                Mod.PortalLayers.Where((PortalLayer x) => x.LayerType == LayerType.BiomeImage).First()?.ChangeBiomeSprite(Mod.PortalBiomeTextures[biome]);
            }
        }




*/

        public enum ScreenType
        {
            BiomeImage,
            Static,
            Rotating,
            Animated,
            Invisible
        }

    } 
}
