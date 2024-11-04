using RareMagicPortalPlus.PortalScreens;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace RareMagicPortal
{
    public class SpriteTools
    {
        private Sprite outcome;
        private Texture2D texture;
        private Texture2D rawTexture;
        private Texture2D secondaryLayer;
        private Sprite source;
        private Color tint;
        public float ratio = 0.5f;

        public void setTint(Color tint)
        {
            this.tint = tint;
        }

        public void setSecondaryLayer(Texture2D secondaryLayer)
        {
            this.secondaryLayer = secondaryLayer;
        }

        public void setRatio(int ratio)
        {
            this.ratio = ((float)ratio / 100);
        }

        public void setRatio(float ratio)
        {
            this.ratio = ratio;
        }
        internal byte[] ReadEmbeddedFileBytes(string fileName)
        {
            // Get the current executing assembly
            var assembly = Assembly.GetExecutingAssembly(); 

            // List all available resource names and find the one that matches the file name
            string resourceName = assembly.GetManifestResourceNames()
                                          .FirstOrDefault(name => name.EndsWith(fileName, System.StringComparison.OrdinalIgnoreCase));

            if (resourceName == null)
            {
                Debug.LogError($"Embedded resource '{fileName}' not found.");
                return null;
            }

            // Open a stream to read the embedded file
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                {
                    Debug.LogError($"Failed to open stream for embedded resource '{resourceName}'.");
                    return null;
                }

                // Read the stream into a byte array
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }
        /*
        private byte[] ReadEmbeddedFileBytes(string name)
        {
            var myType = typeof(SpriteTools);
            var n = myType.Namespace;
            using MemoryStream stream = new();
            System.Reflection.Assembly a = System.Reflection.Assembly.GetExecutingAssembly();
            string fileName = a.GetName().Name;
            Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName + "." + name).CopyTo(stream);
            return stream.ToArray();
        }*/


        public static Sprite CreateCircularSprite(Texture2D texture)
        {
            int width = texture.width;
            int height = texture.height;
            PortalImage.maskheight = height;
            PortalImage.maskwidth = width;


            int radius = Math.Min(width, height) / 2;
            Vector2 center = new Vector2(width / 2, height / 2);

            // Create a new Texture2D to store the circular texture
            Texture2D circularTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // Calculate the distance from the center
                    float distanceFromCenter = Vector2.Distance(new Vector2(x, y), center);

                    // If the pixel is within the circle, copy the color; otherwise, make it transparent
                    if (distanceFromCenter <= radius)
                    {
                        circularTexture.SetPixel(x, y, texture.GetPixel(x, y));
                    }
                    else
                    {
                        circularTexture.SetPixel(x, y, new Color(0, 0, 0, 0)); // Transparent
                    }
                }
            }

            // Apply changes to the texture
            circularTexture.Apply();

            // Create a sprite from the circular texture using the full texture dimensions
            Sprite circularSprite = Sprite.Create(circularTexture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
            return circularSprite;
        }



        public Texture2D loadTexture(string name)
        {
            Texture2D texture = new(64, 64);
            texture.LoadImage(ReadEmbeddedFileBytes(name));

            return texture;
        }

        public Texture2D CreateTexture2D(Texture2D rawTexture, bool useTint)
        {
            this.rawTexture = rawTexture;
            makeTexture(rawTexture.width, rawTexture.height);
            getTexturePixels();
            if (useTint && tint != null)
            {
                BlendTexture();
            }
            return texture;
        }

        public Texture2D CreateTexture2D(Sprite source, bool useTint)
        {
            this.source = source;
            makeTexture((int)source.rect.width, (int)source.rect.height);
            getSpritePixels();
            if (useTint && tint != null)
            {
                BlendTexture();
            }
            return texture;
        }

        private void MakeGrayscale()
        {
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    Color pixel = texture.GetPixel(x, y);
                    if (pixel.a > 0)
                    {
                        var gray = pixel.grayscale;
                        Color grayish = new Color(gray, gray, gray, pixel.a);
                        texture.SetPixel(x, y, grayish);
                    }
                }
            }
            texture.Apply();
        }

        public Sprite CreateSprite(Sprite source, bool useTint, bool grayscale = false)
        {
            this.source = source;
            makeTexture((int)source.rect.width, (int)source.rect.height);
            getSpritePixels();

            if (grayscale)
            {
                MakeGrayscale();
            }

            if (useTint && tint != null)
            {
                BlendTexture();
            }
            MakeSprite();
            return outcome;
        }

        public Sprite CreateSprite(Texture2D rawTexture, bool useTint, bool grayscale = false)
        {
            this.rawTexture = rawTexture;
            makeTexture(rawTexture.width, rawTexture.height);
            getTexturePixels();

            if (grayscale)
            {
                MakeGrayscale();
            }
            if (useTint && tint != null)
            {
                BlendTexture();
            }
            MakeSprite();
            outcome.name = "RMP_icon";
            return outcome;
        }

        public void makeTexture(int width, int height)
        {
            texture = new Texture2D(width, height);
        }

        private void getTexturePixels()
        {
            var pixels = rawTexture.GetPixels();
            texture.SetPixels(pixels);
            texture.Apply();
        }

        private void getSpritePixels()
        {
            var pixels = source.texture.GetPixels((int)source.textureRect.x,
                                                    (int)source.textureRect.y,
                                                    (int)source.textureRect.width,
                                                    (int)source.textureRect.height);
            texture.SetPixels(pixels);
            texture.Apply();
        }

        private void BlendTexture()
        {
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    Color pixel = texture.GetPixel(x, y);
                    if (pixel.a > 0)
                    {
                        texture.SetPixel(x, y, Color.Lerp(pixel, tint, ratio));
                        if (secondaryLayer != null)
                        {
                            Color secPixel = secondaryLayer.GetPixel(x, y);
                            if (secPixel.a > 0)
                            {
                                texture.SetPixel(x, y, secPixel);
                            }
                        }
                    }
                }
            }
            texture.Apply();
        }

        public Sprite convertTextureToSprite(Texture2D tex)
        {
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
        }

        private void MakeSprite()
        {
            outcome = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
        }

        public Texture2D GetTexture2D()
        {
            return texture;
        }

        public Sprite getSprite()
        {
            return outcome;
        }
    }
}