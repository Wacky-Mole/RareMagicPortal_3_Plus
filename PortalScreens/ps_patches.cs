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
        private static GameObject orginal4 = null;
        private static GameObject orginal5 = null;
        private static GameObject orginal6 = null;
        [HarmonyPatch(typeof(Hud), "Awake")]
        private class HudHud_AwakeRMP
        {
            private static void Postfix(Hud __instance)
            {

                if (MagicPortalFluid.PortalImages.Value == MagicPortalFluid.Toggle.Off)
                    return;

                PortalImage.Initialize();
                GameObject gameObject = GameObject.Find("_GameMain").transform.Find("LoadingGUI/PixelFix/IngameGui/HUD/LoadingBlack/Teleporting/Swirl").gameObject;

                PortalLayer layerSwirl = new PortalLayer
                {
                    LayerName = "Swirl",
                    LayerType = ScreenType.Static
                };
                // PortalLayers.Add(layerSwirl);

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

                        if (i == 4)
                            orginal4 = gamePortalLayer.gameObject;
                        if (i == 5)
                            orginal5 = gamePortalLayer.gameObject;                       
                        if (i == 6)
                            orginal6 = gamePortalLayer.gameObject;

                        string layerName = gamePortalLayer.gameObject.name;
                       // MagicPortalFluid.RareMagicPortal.LogWarning("layers found " + layerName);

                        PortalLayer portalLayer  = new PortalLayer();
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

               // defaultblack = GameObject.Find("_GameMain").transform.Find("LoadingGUI/PixelFix/IngameGui/HUD/LoadingBlack/Bkg").gameObject;
                 privBKG = GameObject.Find("_GameMain").transform.Find("LoadingGUI/PixelFix/IngameGui/HUD/LoadingBlack/Bkg").gameObject;
                if (privBKG != null)
                {
                    Image img = privBKG.GetComponent<Image>();         
                    defaultblack = img.sprite;
                    //img.sprite = PortalImage.BackgroundSprite;
                   // img.color = new Color(1f, 1f, 1f, 1f);
                }
                else
                {
                    // Handle case where background sprite is null or gameObject2 is null
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
                            portalLayer.Rotate();
                            break;
                        case ScreenType.Animated:
                            portalLayer.Animate();
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

                Image img = privBKG.GetComponent<Image>();
                PortalImage.LoadBackgroundSprite();
                img.sprite = PortalImage.BackgroundSprite;
                img.color = new Color(1f, 1f, 1f, 1f);

                Heightmap.Biome biome =  WorldGenerator.instance.GetBiome(pos);

                var biomeLayer = PortalImage.PortalLayers[5];
                biomeLayer.LayerType = ScreenType.BiomeImage;
                if (biomeLayer != null && biomeLayer.LayerType == ScreenType.BiomeImage && PortalImage.PortalBiomeTextures.ContainsKey(biome))
                {
                    MagicPortalFluid.RareMagicPortal.LogWarning("Setting circle image to " + biome);
                   // biomeLayer.ChangeBiomeSprite(PortalImage.PortalBiomeTextures[biome]);

                    Image imageComponent = orginal5.GetComponent<Image>();
                    imageComponent.sprite = PortalImage.MaskSprite;//PortalImage.PortalBiomeTextures[biome];
                    //imageComponent.rectTransform.sizeDelta = new Vector2(PortalImage.maskwidth, PortalImage.maskheight);
                    orginal4.SetActive(true);
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
                

                   
                    /*
                    for (int i = 1; i < 4; i++)
                    {
                        var biomeLayer = PortalImage.PortalLayers[i];
                        biomeLayer.LayerType = ScreenType.BiomeImage;

                        if (biomeLayer != null && biomeLayer.LayerType == ScreenType.BiomeImage && PortalImage.PortalBiomeTextures.ContainsKey(biome))
                        {
                            biomeLayer.ChangeBiomeSprite(PortalImage.PortalBiomeTextures[biome]);
                        }else
                        {
                            biomeLayer.LayerType = ScreenType.Invisible;
                        }
                    } */

                    /*
                    var biomeLayer = PortalImage.PortalLayers.FirstOrDefault(x => x.LayerType == ScreenType.BiomeImage);
                    if (biomeLayer != null && PortalImage.PortalBiomeTextures.ContainsKey(biome))
                    {
                        biomeLayer.ChangeBiomeSprite(PortalImage.PortalBiomeTextures[biome]);
                    } */
                }
            }

        public enum ScreenType
        {
            BiomeImage,
            Static,
            Rotating,
            Animated,
            Invisible
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

        // Method to initialize and load sprites and configurations
        public static void Initialize()
        {
            LoadBackgroundSprite();
            LoadPortalBiomeTextures();
            //InitializePortalLayers();
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
            // Load biome-specific sprites
            var files = Directory.GetFiles(MagicPortalFluid.BiomeTexturesFolder);
            foreach (Heightmap.Biome biome in Enum.GetValues(typeof(Heightmap.Biome)))
            {             
                var path = Path.Combine(MagicPortalFluid.BiomeTexturesFolder, $"{biome}.png");              
               // MagicPortalFluid.RareMagicPortal.LogWarning("searching for " + biome + " path " + path + " vs file " + files[0]);
               
                Texture2D texture = LoadTextureFromFile(path);
                if (texture != null)
                {
                    MagicPortalFluid.RareMagicPortal.LogWarning("Found image " + biome);

                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    PortalBiomeTextures[biome] = sprite;
                }
            }
            string maskPath = files.FirstOrDefault(file => Path.GetFileName(file).Equals("mask.png", StringComparison.OrdinalIgnoreCase));

            if (!string.IsNullOrEmpty(maskPath))
            {
                MagicPortalFluid.RareMagicPortal.LogWarning("Found mask");

                // Load the texture data from the file
                byte[] fileData = System.IO.File.ReadAllBytes(maskPath);
                Texture2D tex = new Texture2D(2, 2); // Initial size doesn't matter as LoadImage will replace it
                if (tex.LoadImage(fileData))
                {
                    // Successfully loaded texture data, now create a circular sprite
                    Sprite sprite = SpriteTools.CreateCircularSprite(tex); // Make sure this method exists
                    MaskSprite = sprite; // Assign the sprite to MaskSprite
                }
                else
                {
                    MagicPortalFluid.RareMagicPortal.LogWarning("Failed to load image data for mask.");
                }
            }
        }

        private static void InitializePortalLayers()
        {
            PortalLayer layerSwirl = new PortalLayer
            {
                LayerName = "Swirl",
                LayerType = ScreenType.Static
            };
           // PortalLayers.Add(layerSwirl);

            for (int i = 0; i < 7; i++) //  7 layers
            {
                PortalLayer layer = new PortalLayer();
                 
                layer.LayerName = $"layer{i + 1}";
                if (MagicPortalFluid.PortalImagesFullScreenOnly.Value == MagicPortalFluid.Toggle.On)
                    layer.LayerType = ScreenType.Invisible;
                else
                    layer.LayerType = ScreenType.Rotating;

                if (i<5)
                    layer.LayerType = ScreenType.Invisible;

                layer.RotationSpeed = 50f + i * 10f;
                   
                PortalLayers.Add(layer);
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

        public void UpdateImageSprite(Transform gamePortalLayer)
        {
            ImageComponent = gamePortalLayer.gameObject.GetComponent<Image>();
            if (ImageComponent != null)
            {
                // Assign the appropriate sprite based on LayerType or other logic
                switch (LayerType)
                {
                    case ps_patches.ScreenType.Static:
                        // Load a static sprite
                        ImageComponent.sprite = LoadSprite($"Assets/PortalLayers/{LayerName}.png");
                        break;
                    case ps_patches.ScreenType.Rotating:
                        // Load a rotating sprite
                        ImageComponent.sprite = LoadSprite($"Assets/PortalLayers/{LayerName}_rotating.png");
                        break;
                    case ps_patches.ScreenType.Animated:
                        // Start animation
                        break;
                    case ps_patches.ScreenType.BiomeImage:
                        // Will be set based on biome
                        break;
                    case ps_patches.ScreenType.Invisible:
                        // Hide the layer
                        ImageComponent.gameObject.SetActive(false);
                        break;
                }
            }
        }

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

        public void Animate()
        {
            if (ImageComponent != null)
            {
                // Implement animation logic, e.g., cycling through sprites
            }
        }

        private Sprite LoadSprite(string filePath)
        {
            Texture2D texture = PortalImage.LoadTextureFromFile(filePath);
            if (texture != null)
            {
                return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
            return null;
        }
    }
}
