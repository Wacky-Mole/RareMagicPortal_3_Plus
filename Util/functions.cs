using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using YamlDotNet.Serialization;
using static Heightmap;
using static RareMagicPortal.PortalName;

namespace RareMagicPortal
{
    internal class functions
    {

        internal static void GetAllTheDamnPiecesinRadius(Vector3 p, float radius, List<Piece> pieces)
        {
            foreach (Piece piece in Piece.s_allPieces)
            {
                if (piece.gameObject.layer == Piece.s_ghostLayer
                    || Vector3.Distance(p, piece.transform.position) >= radius)
                {
                    continue;
                }
                pieces.Add(piece);
            }
        }

        internal static CraftingStation GetCraftingStation(string name)
        {
            if (name == "")
            {
                return null;
            }
            foreach (Recipe recipe in ObjectDB.instance.m_recipes)
            {
                if (recipe?.m_craftingStation?.m_name == name)
                {
                    //Jotunn.Logger.LogMessage("got crafting station " + name);
                    return recipe.m_craftingStation;
                }
            }
            return null;
        }

        internal static List<GameObject> GetPieces()
        {
            List<GameObject> list = new List<GameObject>();
            if (!ObjectDB.instance)
            {
                return list;
            }
            ItemDrop itemDrop = ObjectDB.instance.GetItemPrefab("Hammer")?.GetComponent<ItemDrop>();
            if ((bool)itemDrop)
            {
                list.AddRange(Traverse.Create((object)itemDrop.m_itemData.m_shared.m_buildPieces).Field("m_pieces").GetValue<List<GameObject>>());
            }
            ItemDrop itemDrop2 = ObjectDB.instance.GetItemPrefab("Hoe")?.GetComponent<ItemDrop>();
            if ((bool)itemDrop2)
            {
                list.AddRange(Traverse.Create((object)itemDrop2.m_itemData.m_shared.m_buildPieces).Field("m_pieces").GetValue<List<GameObject>>());
            }
            return list;
        }

        public static void GetAllMaterials()
        {
            Material[] array = Resources.FindObjectsOfTypeAll<Material>();
            MagicPortalFluid.originalMaterials = new Dictionary<string, Material>();
            Material[] array2 = array;
            foreach (Material val in array2)
            {
                // Dbgl($"Material {val.name}" );
                MagicPortalFluid.originalMaterials[val.name] = val;
            }
        }

        /*
		void UpdateColorHexValue(object sender, EventArgs eventArgs)
		{
			_targetPortalColorHex.Value = $"#{GetColorHtmlString(_targetPortalColor.Value)}";
		}

		void UpdateColorValue(object sender, EventArgs eventArgs)
		{
			if (ColorUtility.TryParseHtmlString(_targetPortalColorHex.Value, out Color color))
			{
				_targetPortalColor.Value = color;
			}
		}
		*/

        internal static void ServerZDOymlUpdate( string Portalname, string ZDOID, int Colorint, string PortUpdate = null) // MESSAGE SENDER
        {
            if (ZNet.instance.IsServer())// && ZNet.instance.IsDedicated()) removed dedicated  // so no singleplayer announcement
                return;
            if (MagicPortalFluid.JustSent > 0)
            {
                MagicPortalFluid.JustSent++;
                return;
            }
            if (PortUpdate != null)
            {
                ZPackage pkg = new ZPackage(); // Create ZPackage

                pkg.Write(Portalname + ","  + PortUpdate);
                MagicPortalFluid.RareMagicPortal.LogInfo($"Sending the Server a FULL ZDO update for {Portalname} with ZDO {ZDOID}");

                MagicPortalFluid.JustSent = 1;
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), "RequestServerAnnouncementRMPZDOFULL", new object[] { pkg });
            }
            else
            {
                ZPackage pkg = new ZPackage(); // Create ZPackage

                pkg.Write(Portalname + "," + Colorint + "," + ZDOID);
                MagicPortalFluid.RareMagicPortal.LogInfo($"Sending the Server a update for {Portalname} with Color {Colorint} with ZDO {ZDOID}");

                MagicPortalFluid.JustSent = 1;
                ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), "RequestServerAnnouncementRMP", new object[] { pkg });
            }
        }

        internal static string GetBiomeColor(string biome)
        {
            if (string.IsNullOrEmpty(biome))
            {
                return "Black"; // Default color indicating an unknown biome
            }

            string biomeColors = MagicPortalFluid.BiomeRepColors.Value;
            string[] biomeColorsArray = biomeColors.Split(',');

            // Search for a matching biome in the array
            var result = Array.Find(biomeColorsArray, s => s.Contains(biome));

            if (result == null)
            {
                return "Black"; // Return default if no matching biome is found
            }

            // Split to extract color, ensuring proper format handling
            string[] single = result.Split(':');
            if (single.Length < 2)
            {
                return "Black"; // Return default if format is incorrect
            }

            string singleForReal = single[1].Trim();
            return singleForReal;
        }

        public static void RPC_RequestServerAnnouncementRMP(long sender, ZPackage pkg) // MESSAGE RECIEVER
        {
            if (ZNet.instance.IsServer()) //&& ZNet.instance.IsDedicated() ) If any server than prepare to recieved message
            {
                if (pkg != null && pkg.Size() > 0)
                { // Check that our Package is not null, and if it isn't check that it isn't empty.
                    ZNetPeer peer = ZNet.instance.GetPeer(sender);
                    if (peer != null)
                    { // Confirm the peer exists
                      //string peerSteamID = ((ZSteamSocket)peer.m_socket).GetPeerID().m_SteamID.ToString(); no more steam
                        string playername = peer.m_playerName;// playername
                        string msg = pkg.ReadString();
                        string[] msgArray = msg.Split(',');
                        string PortalName = msgArray[0];
                        int Colorint = Convert.ToInt32(msgArray[1]);
                        string ZDOP = msgArray[2];
                        MagicPortalFluid.RareMagicPortal.LogInfo($"Server has recieved a YML update from {playername} for {PortalName} with Color {Colorint} for ZDO {ZDOP}");

                        PortalColorLogic.updateYmltoColorChange(PortalName, Colorint, ZDOP);

                        //YMLPortalData.Value has been updated
                        return;

                        //ZRoutedRpc.instance.InvokeRoutedRPC(ZRoutedRpc.instance.GetServerPeerID(), "EventServerAnnouncementRMP", new object[] { pkg }); // send to clients which is not needed will yml
                    }
                }
            }
        }

        public static void RPC_RequestServerAnnouncementRMPZDOFULL(long sender, ZPackage pkg) // MESSAGE RECIEVER
        {
            if (ZNet.instance.IsServer()) //&& ZNet.instance.IsDedicated() ) If any server than prepare to recieved message
            {
                if (pkg != null && pkg.Size() > 0)
                { // Check that our Package is not null, and if it isn't check that it isn't empty.
                    ZNetPeer peer = ZNet.instance.GetPeer(sender);
                    if (peer != null)
                    { // Confirm the peer exists
                        string playername = peer.m_playerName;// playername
                        string msg = pkg.ReadString();
                        string[] msgArray = msg.Split(',');
                        string PortalName = msgArray[0];
                        string portalUpdate = msgArray[1];

                        var deserializer = new DeserializerBuilder()
                            .Build();
                        var port = deserializer.Deserialize<PortalName.Portal>(portalUpdate);
                        string portalNCheck = PortalName;

                        if (PortalColorLogic.PortalN.Portals.ContainsKey(portalNCheck))
                        {
                            PortalColorLogic.PortalN.Portals[portalNCheck] = port;
                        }
                        else
                        {
                            PortalColorLogic.PortalN.Portals.Add(portalNCheck, port);
                        }

                        PortalColorLogic.ClientORServerYMLUpdate(PortalColorLogic.PortalN.Portals[portalNCheck], portalNCheck); // Would only be on server so it's fine. 

                        MagicPortalFluid.RareMagicPortal.LogInfo($"Server has recieved a YML PORTAL update from {playername} for {PortalName} ");

                        //PortalColorLogic.updateYmltoColorChange(PortalName, Colorint, ZDOP);

                        return;

                    }
                }
            }
        }

        /*
		public static void RPC_EventServerAnnouncementRMP(long sender, ZPackage pkg)

			return;
		}
		*/

        internal static string GetColorHtmlString(Color color)
        {
            return color.a == 1.0f
                ? ColorUtility.ToHtmlStringRGB(color)
                : ColorUtility.ToHtmlStringRGBA(color);
        }

        internal static string HandlePortalClick()
        {
            Minimap instance = Minimap.instance;
            List<Minimap.PinData> pins = instance.m_pins;
            Vector3 mousePosition = instance.ScreenToWorldPoint(Input.mousePosition);
            float searchRadius = instance.m_removeRadius * (instance.m_largeZoom * 2f);

            MagicPortalFluid.checkiftagisPortal = null;
            Minimap.PinData closestPin = null;
            float closestDistance = float.MaxValue;

            foreach (Minimap.PinData pin in pins)
            {
                float distance = Utils.DistanceXZ(mousePosition, pin.m_pos);
                if (distance < searchRadius && (distance < closestDistance || closestPin == null))
                {
                    closestPin = pin;
                    closestDistance = distance;
                }
            }

            if (closestPin != null && !string.IsNullOrEmpty(closestPin.m_name))
            {
                MagicPortalFluid.checkiftagisPortal = closestPin.m_name;

                // Check if the pin is related to TargetPortal
                if (closestPin.m_icon.name != "TargetPortalIcon")
                {
                    MagicPortalFluid.checkiftagisPortal = null;
                }
            }

            // Additional filtering
            if (MagicPortalFluid.checkiftagisPortal != null &&
                (MagicPortalFluid.checkiftagisPortal.Contains("$hud") || MagicPortalFluid.checkiftagisPortal.Contains("Day ")))
            {
                MagicPortalFluid.checkiftagisPortal = null;
            }

            return MagicPortalFluid.checkiftagisPortal;
        }

        public static (double x, double y) GenerateRandomPoint(double maxRadius = 10000)
        {
            System.Random rand = new System.Random();
            // Generate a random angle between 0 and 2π
            double theta = rand.NextDouble() * 2 * Math.PI;

            // Generate a random radius with uniform distribution
            double r = maxRadius * Math.Sqrt(rand.NextDouble());

            // Convert polar coordinates to Cartesian coordinates
            double x = r * Math.Cos(theta);
            double y = r * Math.Sin(theta);

            return (x, y);
        }

        public static Dictionary<Minimap.PinData, ZDO>? GetActivePins()
        {
            try
            {
                Type tpType = Type.GetType("TargetPortal.Map, TargetPortal");
                if (tpType == null)
                {
                    UnityEngine.Debug.LogError("TargetPortal.Map type could not be found.");
                    return null;
                }

                // Use GetField instead of GetProperty
                FieldInfo activePinsField = tpType.GetField("activePins", BindingFlags.NonPublic | BindingFlags.Static);
                if (activePinsField == null)
                {
                    UnityEngine.Debug.LogError("The activePins field could not be found.");
                    return null;
                }

                // Retrieve the value of the activePins field
                var activePins = activePinsField.GetValue(null);
                if (activePins == null)
                {
                    UnityEngine.Debug.LogError("The activePins field returned null.");
                    return null;
                }

                // Cast the result to the correct type
                return (Dictionary<Minimap.PinData, ZDO>?)activePins;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError("An error occurred while retrieving activePins: " + ex.Message);
                return null;
            }

        }

    }
}