using HarmonyLib;
using RareMagicPortal;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static RareMagicPortalPlus.PortalScreens.ps_patches;

namespace RareMagicPortalPlus.PortalScreens
{
    public class ps_patches
    {
        [HarmonyPatch(typeof(Hud), "Awake")]
        private class HudHud_AwakeRMP
        {
            private static void Postfix(Hud __instance)
            {

                if (MagicPortalFluid.PortalImages.Value == MagicPortalFluid.Toggle.Off)
                    return;

                GameObject gameObject = GameObject.Find("_GameMain").transform.Find("LoadingGUI/PixelFix/IngameGui/HUD/LoadingBlack/Teleporting/Swirl").gameObject;
                if (gameObject != null)
                {
                    Transform[] componentsInChildren = gameObject.GetComponentsInChildren<Transform>();
                    foreach (Transform gamePortalLayer in componentsInChildren)
                    {
                        string layerName = gamePortalLayer.gameObject.name;
                        PortalLayer portalLayer = PortalImage.PortalLayers.FirstOrDefault(x => x.LayerName == layerName);
                        if (portalLayer != null)
                        {
                            portalLayer.UpdateImageSprite(gamePortalLayer);
                        }
                    }
                }

                GameObject gameObject2 = GameObject.Find("_GameMain").transform.Find("LoadingGUI/PixelFix/IngameGui/HUD/LoadingBlack/Bkg").gameObject;
                if (gameObject2 != null && PortalImage.BackgroundSprite != null)
                {
                    Image img = gameObject2.GetComponent<Image>();
                    img.sprite = PortalImage.BackgroundSprite;
                    img.color = new Color(1f, 1f, 1f, 1f);
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
        private class Player_TeleportToRMP
        {
            private static void Prefix(Vector3 pos)
            {
                if (MagicPortalFluid.PortalImages.Value == MagicPortalFluid.Toggle.Off)
                    return;

                Heightmap.Biome biome = WorldGenerator.instance.GetBiome(pos);
                var biomeLayer = PortalImage.PortalLayers.FirstOrDefault(x => x.LayerType == ScreenType.BiomeImage);
                if (biomeLayer != null && PortalImage.PortalBiomeTextures.ContainsKey(biome))
                {
                    biomeLayer.ChangeBiomeSprite(PortalImage.PortalBiomeTextures[biome]);
                }
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
        public static Dictionary<Heightmap.Biome, Sprite> PortalBiomeTextures = new Dictionary<Heightmap.Biome, Sprite>();

        // Method to initialize and load sprites and configurations
        public static void Initialize()
        {
            LoadBackgroundSprite();
            LoadPortalBiomeTextures();
            InitializePortalLayers();
        }

        private static void LoadBackgroundSprite()
        {
            // Load the background sprite from resources or user-defined path
            Texture2D texture = LoadTextureFromFile("Assets/Backgrounds/teleport_background.png");
            if (texture != null)
            {
                BackgroundSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            }
        }

        private static void LoadPortalBiomeTextures()
        {
            // Load biome-specific sprites
            foreach (Heightmap.Biome biome in Enum.GetValues(typeof(Heightmap.Biome)))
            {
                string path = $"Assets/BiomeTextures/{biome}.png";
                Texture2D texture = LoadTextureFromFile(path);
                if (texture != null)
                {
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                    PortalBiomeTextures[biome] = sprite;
                }
            }
        }

        private static void InitializePortalLayers()
        {
            // Initialize portal layers based on user configurations or defaults
            for (int i = 0; i < 5; i++) // Assuming there are 5 layers
            {
                PortalLayer layer = new PortalLayer
                {
                    LayerName = $"layer{i + 1}",
                    LayerType = ScreenType.Rotating, // Default to rotating, can be changed based on config
                    RotationSpeed = 50f + i * 10f // Different speed for each layer
                };
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
