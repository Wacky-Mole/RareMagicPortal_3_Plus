using HarmonyLib;
using RareMagicPortal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.UI;
using static Heightmap;
using static RareMagicPortalPlus.PortalScreens.ps_patches;
using static UnityEngine.UI.Image;

namespace RareMagicPortalPlus.PortalScreens
{
    public class ps_patches
    {

        private static Sprite  defaultblack = null;
        private static GameObject privBKG = null;
        private static GameObject orginal5 = null;
        [HarmonyPatch(typeof(Hud), "Awake")]
        private class HudHud_AwakeRMP
        {
            private static void Postfix(Hud __instance)
            {

                //if (MagicPortalFluid.PortalImages.Value == MagicPortalFluid.Toggle.Off)
                 //   return;

                PortalImage.Initialize();
              
                PortalLayer layerSwirl = new PortalLayer
                {
                    LayerName = "Swirl",
                    LayerType = ScreenType.Static
                };
                // PortalLayers.Add(layerSwirl);

                 privBKG = GameObject.Find("_GameMain").transform.Find("LoadingGUI/PixelFix/IngameGui/HUD/LoadingBlack/Bkg").gameObject;
                if (privBKG != null)
                {
                    Image img = privBKG.GetComponent<Image>();         
                    defaultblack = img.sprite;
                    //img.sprite = PortalImage.BackgroundSprite;
                   // img.color = new Color(1f, 1f, 1f, 1f);
                }
            }
        }

        [HarmonyPatch(typeof(Uirotate), "Update")]
        private class UiRotate_UpdateRMP
        {
            private static bool Prefix(Uirotate __instance)
            {
                if (MagicPortalFluid.PortalImages.Value == MagicPortalFluid.Toggle.Off)
                    return true;

                if (MagicPortalFluid.PortalImagesFullScreenOnly.Value == MagicPortalFluid.Toggle.On)
                {
                    __instance.gameObject.SetActive(false);
                    return false;
                }

                string name = __instance.gameObject.name;
                if (!name.Contains("layer"))
                {
                    return true;
                }
                int index = int.Parse(name.Replace("layer", "")) - 1;
                if (index >= 0 && index < PortalImage.PortalLayers.Count)
                {
                    PortalLayer portalLayer = PortalImage.PortalLayers[index];
                    switch (portalLayer.LayerType)
                    {
                        case ScreenType.BiomeImage:
                            return false;
                        case ScreenType.Static:
                            return false;
                        case ScreenType.Rotating:
                            __instance.gameObject.SetActive(true);
                            portalLayer.Rotate();
                            break;
                        case ScreenType.Invisible:
                            __instance.gameObject.SetActive(false);
                            break;
                    }
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(Player), "TeleportTo")]
        private class Player_TeleportToRMPBiomeLayer
        {
            private static void Prefix(Vector3 pos)
            {
                if (MagicPortalFluid.PortalImages.Value == MagicPortalFluid.Toggle.Off)
                    return;

                if (privBKG == null || defaultblack == null)
                {
                    privBKG = GameObject.Find("_GameMain").transform.Find("LoadingGUI/PixelFix/IngameGui/HUD/LoadingBlack/Bkg").gameObject;
                    if (privBKG != null)
                    {
                        Image img2 = privBKG.GetComponent<Image>();
                        defaultblack = img2.sprite;
                    }
                }

                Image img = privBKG.GetComponent<Image>();
                PortalImage.LoadBackgroundSprite();
                img.sprite = PortalImage.BackgroundSprite;
                img.color = new Color(1f, 1f, 1f, 1f);

                if (MagicPortalFluid.PortalImagesFullScreenOnly.Value == MagicPortalFluid.Toggle.On)
                    return;


                if (PortalImage.PortalLayers.Count() == 0)
                {
                    GameObject gameObject = GameObject.Find("_GameMain").transform.Find("LoadingGUI/PixelFix/IngameGui/HUD/LoadingBlack/Teleporting/Swirl").gameObject;
                    int i = 0;
                    if (gameObject != null)
                    {
                        Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>();
                        foreach (Transform gamePortalLayer in componentsInChildren)
                        {
                            if (i == 0)
                            {
                                i++;
                                continue;
                            } // total is 8 0 is swirl, 7 is last

                            if (i == 5)
                                orginal5 = gamePortalLayer.gameObject;

                            string layerName = gamePortalLayer.gameObject.name;
                           // MagicPortalFluid.RareMagicPortal.LogWarning("layers found " + layerName);

                            PortalLayer portalLayer = new PortalLayer();
                            portalLayer.LayerName = layerName;
                            portalLayer.ImageComponent = gamePortalLayer.gameObject.GetComponent<Image>();
                            if (MagicPortalFluid.PortalImagesFullScreenOnly.Value == MagicPortalFluid.Toggle.On)
                                portalLayer.LayerType = ScreenType.Invisible;
                            else
                                portalLayer.LayerType = ScreenType.Rotating;

                            if (i < 5)
                                portalLayer.LayerType = ScreenType.Invisible;

                            portalLayer.RotationSpeed = 50f + i * 10f;

                            PortalImage.PortalLayers.Add(portalLayer); // 7 total 0-6

                            if (portalLayer != null)
                            {
                                //  portalLayer.UpdateImageSprite(gamePortalLayer);
                            }
                            i++;
                        }
                    }
                }

                Heightmap.Biome biome = WorldGenerator.instance.GetBiome(pos);
                try
                {
                    var biomeLayer = PortalImage.PortalLayers[4]; // ACTUAL 5
                    biomeLayer.LayerType = ScreenType.BiomeImage;
                    if (biomeLayer != null && biomeLayer.LayerType == ScreenType.BiomeImage && PortalImage.PortalBiomeTextures.ContainsKey(biome))
                    {
                        MagicPortalFluid.RareMagicPortal.LogInfo("Setting circle image to " + biome);
                        // biomeLayer.ChangeBiomeSprite(PortalImage.PortalBiomeTextures[biome]);

                        Image imageComponent = orginal5.GetComponent<Image>();
                        imageComponent.sprite = PortalImage.MaskSprite;//PortalImage.PortalBiomeTextures[biome];
                                                                       //imageComponent.rectTransform.sizeDelta = new Vector2(PortalImage.maskwidth, PortalImage.maskheight);
                        orginal5.SetActive(true);
                        biomeLayer.RotationSpeed = 0;

                        if (!orginal5.TryGetComponent(out Mask _))
                        {
                            orginal5.AddComponent<Mask>().showMaskGraphic = false;

                            GameObject childImageObject = new GameObject("ChildImage");
                            childImageObject.transform.SetParent(orginal5.transform, false);
                            childImageObject.AddComponent<Image>();
                        }
                        orginal5.GetComponent<Uirotate>().enabled = false;
                        var childImageTrans = orginal5.transform.Find("ChildImage");
                        var childImage = childImageTrans.GetComponent<Image>();
                        childImage.rectTransform.sizeDelta = new Vector2(PortalImage.maskwidth, PortalImage.maskheight);
                        childImage.sprite = PortalImage.PortalBiomeTextures[biome];

                    }
                }
                catch (Exception e) { MagicPortalFluid.RareMagicPortal.LogWarning("Errror catch on Setting Biome Image, restart to fix "); };
            }
        }
        public enum ScreenType
        { 

            Invisible,
            BiomeImage,
            Static,
            Rotating,           
        }
    }

    public static class PortalImage
    {
        public static List<PortalLayer> PortalLayers = new List<PortalLayer>();
        public static Sprite BackgroundSprite;
        public static Sprite MaskSprite;
        public static Dictionary<Heightmap.Biome, Sprite> PortalBiomeTextures = new Dictionary<Heightmap.Biome, Sprite>();
        public static int maskwidth = 0;
        public static int maskheight = 0;
        public static void Initialize()
        {
            LoadBackgroundSprite();
            LoadPortalBiomeTextures();
        }

        internal static void LoadBackgroundSprite()
        {
            // Load the background sprite from resources or user-defined path
            //var background = Path.Combine(MagicPortalFluid.BackgroundFolder, "teleport_background.png");

            var files = Directory.GetFiles(MagicPortalFluid.BackgroundFolder);
            System.Random random = new System.Random();
            string randomFile = files[random.Next(files.Length)];

            Texture2D texture = LoadTextureFromFile(randomFile);
            if (texture != null)
            {
                BackgroundSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }

        private static void LoadPortalBiomeTextures()
        {
            var files = Directory.GetFiles(MagicPortalFluid.BiomeTexturesFolder);
            foreach (Heightmap.Biome biome in Enum.GetValues(typeof(Heightmap.Biome)))
            {             
                var path = Path.Combine(MagicPortalFluid.BiomeTexturesFolder, $"{biome}.png");
                //MagicPortalFluid.RareMagicPortal.LogWarning(" " + biome);
               // MagicPortalFluid.RareMagicPortal.LogWarning("searching for " + biome + " path " + path + " vs file " + files[0]);

                Texture2D texture = LoadTextureFromFile(path);
                if (texture != null)
                {
                   // MagicPortalFluid.RareMagicPortal.LogWarning("Found image " + biome);
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    PortalBiomeTextures[biome] = sprite;
                }
            }
            string maskPath = files.FirstOrDefault(file => Path.GetFileName(file).Equals("mask.png", StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(maskPath))
            {
              //  MagicPortalFluid.RareMagicPortal.LogWarning("Found mask");

                // Load the texture data from the file
                var tex = LoadTextureFromFile(maskPath);
                Sprite sprite = SpriteTools.CreateCircularSprite(tex); // Make sure this method exists
                MaskSprite = sprite; // Assign the sprite to MaskSprite
                
            }
        }

        public static Texture2D LoadTextureFromFile(string filePath)
        {
            // Implement loading texture from file
            if (System.IO.File.Exists(filePath))
            {
                byte[] fileData = System.IO.File.ReadAllBytes(filePath);
                Texture2D tex = new Texture2D(2, 2);
                if (tex.LoadImage(fileData))
                {
                    return tex;
                }
            }
            return null;
        }
    }

    public class PortalLayer
    {
        public string LayerName { get; set; }
        public ps_patches.ScreenType LayerType { get; set; }
        public Image ImageComponent { get; set; }
        public float RotationSpeed { get; set; } = 50f;

        public void ChangeBiomeSprite(Sprite newSprite)
        {
            if (ImageComponent != null && newSprite != null)
            {
                ImageComponent.sprite = newSprite;
            }
        }

        public void Rotate()
        {
            if (ImageComponent != null)
            {
                ImageComponent.transform.Rotate(Vector3.forward * Time.deltaTime * RotationSpeed);
            }
        }
    }
}
