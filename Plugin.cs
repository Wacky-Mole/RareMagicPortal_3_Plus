// MagicPortalFluid
// a Valheim mod created by WackyMole. Do whatever with it. - WM
// assets from https://assetstore.unity.com/packages/3d/props/interior/free-alchemy-and-magic-pack-142991
// crystal assets from https://assetstore.unity.com/packages/3d/environments/fantasy/translucent-crystals-106274
//  Thank you https://github.com/redseiko/ComfyMods/tree/main/ColorfulPortals
//
// File:    MagicPortalFluid.cs
// Project: MagicPortalFluid
// create yml file for portal names, put example file in it and explain that it only does something with EnableCrystals and Consumable is true
// load portal names After every change
// black, yellow (normal), red, green, blue, purple, tan, gold, white
//
//
/*
 * Colors for Portals:
Black = mountain
Yellow = normal
Red = ashlands
Green = swamp
Tan = blackforest
Purple = mistlands
Cyan = deepnorth
Orange = plains
Tan = meadows
Gold = master/ endgame
White =endgame/ special
Portal Drink = rainbow mode? Or current white override.

// 1 Yellow // free passage - Maybe add Yellow Crystal and Key
// 2 red
// 3 green
// 4 blue
// 5 Purple
// 6 Tan
// 7 Cyan
// 8 Orange
// 20 Black /
// 21 White (Only allow free passage with PortalDrink or enablecrystals)
// 22 Gold

// under 100 doesn't have any
101 // Yellow Crystal required this many items
201 Yellow Crystal Grants accesss
301 Yellow $rmp_redKey_access Key Access
999 $rmp_noaccess

*/

using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ItemManager;
using LocalizationManager;
using PieceManager;
using RareMagicPortal.PortalWorld;
using ServerSync;
using StatusEffectManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using UnityEngine;
using UnityEngine.UIElements;
using YamlDotNet.Serialization;
using static Interpolate;

namespace RareMagicPortal
{
    public static class Globals
    {
        public static Color Gold = new Color(1f, 215f / 255f, 0, 1f);
        public static Color m_colorTargetfound = new Color(191f / 255f, 150f / 255f, 0, 25);
        public static Color lightcolor = new Color(1f, 100f / 255f, 0, 1f);

        //Material PortalDefMaterial = originalMaterials["portal_small"];
        public static Color flamesstart = new Color(1f, 194f / 255f, 34f / 255f, 1f);

        public static Color flamesend = new Color(1f, 0, 0, 1f);

        public static Color Purple = new Color(107f / 255f, 63f / 255f, 160 / 255f, 1f);
        public static Color Tan = new Color(210f / 255f, 180f / 255f, 140f / 255f, 1f);
        public static Color Brown = new Color(193f / 255f, 69f / 255f, 19f / 255f, 1f);

        public static Dictionary<string, Material> originalMaterials;
    }

    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    [BepInDependency("org.bepinex.plugins.targetportal", BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency("org.bepinex.plugins.jewelcrafting", BepInDependency.DependencyFlags.SoftDependency)]  // it loads before this mod// not really required, but whatever
    [BepInIncompatibility("randyknapp.mods.advancedportals")]
    internal class MagicPortalFluid : BaseUnityPlugin
    {
        public const string PluginGUID = "WackyMole.RareMagicPortalPlus";
        public const string PluginName = "RareMagicPortalPlus";
        public const string PluginVersion = "3.0.0";

        internal const string ModName = PluginName;
        internal const string ModVersion = PluginVersion;
        internal const string Author = "WackyMole";
        internal const string ModGUID = Author + "." + ModName;
        internal static string ConfigFileName = PluginGUID + ".cfg";
        internal static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + "WackyMole.RareMagicPortal.cfg";
        internal static string YMLFULL = YMLFULLFOLDER + "World1.yml";

        //internal static string YMLFULLServer = Paths.ConfigPath + Path.DirectorySeparatorChar + "WackyMole" + ".PortalServerNames.yml";
        internal static string YMLFULLFOLDER = Path.Combine(Path.GetDirectoryName(Paths.ConfigPath + Path.DirectorySeparatorChar), "Portal_Names");

        internal static string ConnectionError = "";

        internal readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource RareMagicPortal =
            BepInEx.Logging.Logger.CreateLogSource(ModName);

        internal static readonly ConfigSync ConfigSync = new(ModGUID)
        { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = PluginVersion };

        internal static MagicPortalFluid? plugin;
        internal static MagicPortalFluid context;

        internal AssetBundle portalmagicfluid;
        public static ConfigEntry<bool> modEnabled;
        public static ConfigEntry<bool> isDebug;
        public static bool firstTime = false;
        public static ConfigEntry<int> nexusID;
        internal static List<RecipeData> recipeDatas = new List<RecipeData>();
        internal static string assetPath;
        internal static string assetPathyml;
        public static List<string> PiecetoLookFor = new List<string> { "portal_wood", "portal_stone" }; //name
        public static List<string> PieceTokenLookFor = new List<string> { "$piece_portal", "$piece_stone" }; //m_name
        public static Vector3 tempvalue;
        public static bool loadfilesonce = false;
        public static Dictionary<string, int> Ind_Portal_Consumption;
        public static int CurrentCrystalCount;
        public static bool isAdmin = true;
        public static bool isLocal = true;
        public static string Worldname = "demo_world";
        public static bool LoggingOntoServerFirst = true;
        internal static Dictionary<string, Material> originalMaterials;

        public static bool FluidwasTrue = false;
        public static bool piecehaslvl = false;
        public static string DefaultTable = "$piece_workbench";
        internal static string YMLCurrentFile = Path.Combine(YMLFULLFOLDER, Worldname + ".yml");
        internal static int JustWrote = 0;
        internal static bool JustWait = false;
        internal static int JustSent = 0;
        internal static bool JustRespawn = false;
        internal static bool NoMoreLoading = false;
        internal static bool WaitSomeMore = false;
        internal static bool Teleporting = false;
        internal static int TeleportingforWeight = 0;
        internal static string checkiftagisPortal = null;
        internal static bool JustStop = false;
        internal static bool JustWaitforInventory = true;
        internal static List<string> PortalDrinkDenyloc = new List<string>();

        internal static bool m_hadTarget = false;
        internal static List<Minimap.PinData> HoldPins;
        internal static bool Globaliscreator = false;

        internal static ConfigEntry<bool>? ConfigFluid;
        internal static ConfigEntry<int>? ConfigSpawn;
        internal static ConfigEntry<string>? ConfigTable;
        internal static ConfigEntry<int>? ConfigTableLvl;
        internal static ConfigEntry<bool>? ConfigCreator;
        internal static ConfigEntry<float>? ConfiglHealth;
        internal static ConfigEntry<bool>? ConfigCreatorLock;
        internal static ConfigEntry<int>? ConfigFluidValue;
        internal static ConfigEntry<bool>? ConfigEnableCrystalsNKeys;

        // internal static ConfigEntry<bool>? ConfigEnableKeys;
        internal static ConfigEntry<int>? ConfigCrystalsConsumable;

        internal static ConfigEntry<string>? DefaultColor;

        internal static ConfigEntry<int>? PortalDrinkTimer;
        internal static ConfigEntry<string>? PortalDrinkDeny;
        internal static ConfigEntry<Toggle>? ConfigEnableYMLLogs;
        internal static ConfigEntry<string>? ConfigAddRestricted;
        internal static ConfigEntry<string>? ConfigAllowItems;
        internal static ConfigEntry<Toggle>? ConfigEnableGoldAsMaster;
        internal static ConfigEntry<string>? ConfigEnableColorEnable;
        internal static ConfigEntry<KeyboardShortcut>? portalRMPKEY = null!;
        internal static ConfigEntry<KeyboardShortcut>? portalRMPMODEKEY = null!;
        internal static ConfigEntry<KeyboardShortcut>? portalRMPCRYSTALKEY = null!;
        internal static ConfigEntry<KeyboardShortcut>? portalRMPsacrifceKEY = null!;
        internal static ConfigEntry<Toggle>? ConfigMessageLeft;
        internal static ConfigEntry<Toggle>? ConfigTargetPortalAnimation;
        internal static ConfigEntry<int>? ConfigMaxWeight;
        internal static ConfigEntry<int>? MaxPortalsPerPerson;
        internal static ConfigEntry<Toggle>? AdminOnlyMakesPortals;
        internal static ConfigEntry<Toggle>? ConfigUseBiomeColors;
        internal static ConfigEntry<string>? BiomeRepColors;
        internal static ConfigEntry<string>? EnabledColors;
        internal static ConfigEntry<string>? FreePassageColor;
        internal static ConfigEntry<string>? AdminColor;
        internal static ConfigEntry<string>? PortalDrinkColor;
        internal static ConfigEntry<Toggle>? PreventColorChange;
        internal static ConfigEntry<string>? TelePortAnythingColor;
        internal static ConfigEntry<string>? GemColorGold;
        internal static ConfigEntry<string>? GemColorRed;
        internal static ConfigEntry<string>? GemColorGreen;
        internal static ConfigEntry<string>? GemColorBlue;
        internal static ConfigEntry<string>? GemColorPurple;
        internal static ConfigEntry<string>? GemColorTan;
        internal static ConfigEntry<string>? GemColorYellow;
        internal static ConfigEntry<string>? GemColorWhite;
        internal static ConfigEntry<string>? GemColorCyan;
        internal static ConfigEntry<string>? GemColorBlack;
        internal static ConfigEntry<string>? GemColorOrange;
        internal static ConfigEntry<Toggle>? RiskyYMLSave;
        internal static ConfigEntry<Toggle>? UseSmallUpdates;
        internal static ConfigEntry<Toggle>? UsePortalProgression;
        internal static ConfigEntry<Toggle>? PreventTargetPortalFromChanging;
        internal static ConfigEntry<string>? PPRed;
        internal static ConfigEntry<string>? PPGreen;
        internal static ConfigEntry<string>? PPBlue;
        internal static ConfigEntry<string>? PPWhite;
        internal static ConfigEntry<string>? PPRedAllows;
        internal static ConfigEntry<string>? PPGreenAllows;
        internal static ConfigEntry<string>? PPBlueAllows;
        internal static ConfigEntry<string>? ColorYELLOWAllows;
        internal static ConfigEntry<string>? ColorBLUEAllows;
        internal static ConfigEntry<string>? ColorGREENAllows;
        internal static ConfigEntry<string>? ColorPURPLEAllows;
        internal static ConfigEntry<string>? ColorTANAllows;
        internal static ConfigEntry<string>? ColorCYANAllows;
        internal static ConfigEntry<string>? ColorORANGEAllows;
        internal static ConfigEntry<string>? ColorBLACKAllows;
        internal static ConfigEntry<string>? ColorWHITEAllows;
        internal static ConfigEntry<string>? ColorGOLDAllows;

        public static string crystalcolorre = ""; // need to reset everytime maybe?
        public string message_eng_NO_Portal = $"Portal Crystals/Key Required"; // Blue Portal Crystal
        public string message_eng_MasterCost = $", Gold Crystals Required"; // 3, Master Crystals Required
        public string message_eng_NotCreator = $"";
        public string message_eng_Grants_Acess = $"";
        public string message_eng_Crystal_Consumed = $"";
        public string message_eng_Odins_Kin = $"Only Odin's Kin are Allowed";
        public string message_only_Owner_Can_Change = $"Only the Owner Can change Name";

        public static string CrystalMaster = "$item_PortalCrystalMaster";
        public static string CrystalRed = "$item_PortalCrystalRed";
        public static string CrystalGreen = "$item_PortalCrystalGreen";
        public static string CrystalBlue = "$item_PortalCrystalBlue";
        public static string CrystalPurple = "$item_PortalCrystalPurple";
        public static string CrystalTan = "$item_PortalCrystalTan";
        public static string CrystalYellow = "$item_PortalCrystalYellow";
        public static string CrystalWhite = "$item_PortalCrystalWhite";
        public static string CrystalCyan = "$item_PortalCrystalCyan";
        public static string CrystalBlack = "$item_PortalCrystalBlack";
        public static string CrystalOrange = "$item_PortalCrystalOrange";

        public static string PortalKeyGold = "$item_PortalKeyGold";
        public static string PortalKeyRed = "$item_PortalKeyRed";
        public static string PortalKeyGreen = "$item_PortalKeyGreen";
        public static string PortalKeyBlue = "$item_PortalKeyBlue";
        public static string PortalKeyPurple = "$item_PortalKeyPurple";
        public static string PortalKeyTan = "$item_PortalKeyTan";
        public static string PortalKeyYellow = "$item_PortalKeyYellow";
        public static string PortalKeyBlack = "$item_PortalKeyBlack";
        public static string PortalKeyWhite = "$item_PortalKeyWhite";
        public static string PortalKeyCyan = "$item_PortalKeyCyan";
        public static string PortalKeyOrange = "$item_PortalKeyOrange";

        private SpriteTools IconColor = new SpriteTools();

        public static Sprite IconBlack = null!;
        public static Sprite IconYellow = null!;
        public static Sprite IconRed = null!;
        public static Sprite IconGreen = null!;
        public static Sprite IconBlue = null!;
        public static Sprite IconGold = null!;
        public static Sprite IconWhite = null!;
        public static Sprite IconDefault = null!;
        public static Sprite IconPurple = null!;
        public static Sprite IconTan = null!;

        public static string ModelDefault = "small_portal";
        public static string Model1 = "Torus_cell.002";
        public static string Model2 = "RuneRing";
        public static string Model3 = "Gates";
        public static string Model4 = "QuadPortal";

        private static TeleportWorldDataCreator ClassDefault = new TeleportWorldDataCreatorA();
        private static TeleportWorldDataCreator ClassModel1 = new TeleportWorldDataCreatorB();
        private static TeleportWorldDataCreator ClassModel2 = new TeleportWorldDataCreatorC();
        private static TeleportWorldDataCreator ClassModel3 = new TeleportWorldDataCreatorD();
        private static TeleportWorldDataCreator ClassModel4 = new TeleportWorldDataCreatorE();

        public static Dictionary<string, Sprite> Icons = new Dictionary<string, Sprite>();

        internal static Localization english = null!;
        internal static Localization spanish = null!;

        public static CustomSE AllowTeleEverything = new CustomSE("yippeTele");
        public static List<StatusEffect> statusEffectactive;

        internal static readonly List<string> portalPrefabs = new List<string>();
        internal static char StringSeparator = 'Ⰴ'; // handcuffs  The fifth letter of the Glagolitic alphabet.

        public static string WelcomeString = "#Hello, this is the portal yml file. It keeps track of all portals in the world";

        private static Coroutine myCoroutineRMP;
        public static ItemDrop.ItemData Crystal { get; internal set; }

        internal static readonly int _teleportWorldColorHashCode = "TeleportWorldColorRMP".GetStableHashCode(); // I should probably change this
        internal static readonly int _teleportWorldColorAlphaHashCode = "TeleportWorldColorAlphaRMP".GetStableHashCode();
        internal static readonly int _portalLastColoredByHashCode = "PortalLastColoredByRMP".GetStableHashCode();
        internal static readonly int _portalCreatorHashCode = "PortalCreatorRMP".GetStableHashCode();
        internal static readonly int _portalBiomeHashCode = "PortalBiomeRMP".GetStableHashCode();
        internal static readonly int _portalBiomeColorHashCode = "PortalBiomeColorRMP".GetHashCode();
        internal static string PortalFluidname;
        internal static bool TargetPortalLoaded = false;

        internal static readonly Dictionary<TeleportWorld, TeleportWorldDataRMP> _teleportWorldDataCache = new();

        private static readonly Dictionary<TeleportWorld, ClassBase> _teleportWorldDataCacheDefault = new();

        private static readonly KeyboardShortcut _changePortalReq = new(KeyCode.E, KeyCode.LeftControl);
        private static readonly KeyboardShortcut _portalRMPsacrifceKEY = new(KeyCode.E, KeyCode.LeftControl);

        public enum Toggle
        {
            On = 1,
            Off = 0,
        }

        internal static IEnumerator RemovedDestroyedTeleportWorldsCoroutine()
        {
            WaitForSeconds waitThirtySeconds = new(seconds: 30f);
            List<KeyValuePair<TeleportWorld, TeleportWorldDataRMP>> existingPortals = new();
            int portalCount = 0;

            while (true)
            {
                yield return waitThirtySeconds;
                portalCount = _teleportWorldDataCache.Count;

                existingPortals.AddRange(_teleportWorldDataCache.Where(entry => entry.Key));
                _teleportWorldDataCache.Clear();

                foreach (KeyValuePair<TeleportWorld, TeleportWorldDataRMP> entry in existingPortals)
                {
                    _teleportWorldDataCache[entry.Key] = entry.Value;
                }

                existingPortals.Clear();

                if ((portalCount - _teleportWorldDataCache.Count) > 0)
                {
                    RareMagicPortal.LogInfo($"Removed {portalCount - _teleportWorldDataCache.Count}/{portalCount} portal references.");
                }
            }
        }

        internal IEnumerable WaitforMe()
        {
            yield return new WaitForSeconds(10);
            MagicPortalFluid.WaitSomeMore = false;
            yield break;
        }

        public void Awake()
        {
            CreateConfigValues();
            ReadAndWriteConfigValues();
            Localizer.Load();

            //LoadAssets();
           // PortalDrink();

            assetPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), typeof(MagicPortalFluid).Namespace);
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), (string)null);

           // SetupWatcher();
            //setupYMLFolderWatcher();

            YMLPortalData.ValueChanged += CustomSyncEventDetected;
            YMLPortalSmallData.ValueChanged += CustomSyncSmallEvent;

           // IconColors();

            RareMagicPortal.LogInfo($"MagicPortalFluid has loaded start assets");
        }

        internal void IconColors()
        {
            context = this;

            Texture2D tex = IconColor.loadTexture("portal.png");
            Texture2D temp = IconColor.loadTexture("portaliconTarget.png");
            IconDefault = IconColor.CreateSprite(temp, false);

            foreach (var col in PortalColorLogic.PortalColors)
            {
                if (col.Key == "Tan")
                {
                    IconColor.setTint(PortalColorLogic.Brown);
                    Icons.Add(col.Key, IconColor.CreateSprite(tex, true));
                }
                else if (col.Key == "Yellow")
                {
                    IconColor.setTint(new Color(201f / 255f, 204f / 255f, 63f / 255f, 1f));
                    Icons.Add(col.Key, IconColor.CreateSprite(tex, true));
                }
                else
                {
                    IconColor.setTint(col.Value.HexName);
                    Icons.Add(col.Key, IconColor.CreateSprite(tex, true));
                }
            }
        }

        internal static void LoadIN()
        {
            LoggingOntoServerFirst = true;
            setupYMLFile();
            ReadYMLValuesBoring();
            PortalColorLogic.reloadcolors();
        }

        // end startup

        internal void PortalDrink()
        {
            AllowTeleEverything.Name.English("Odin's Portal Blessing");
            AllowTeleEverything.Type = EffectType.Consume;
            AllowTeleEverything.Icon = "portaldrink.png";
            //AllowTeleEverything.IconSprite = portaldrind;
            AllowTeleEverything.Effect.m_startMessageType = MessageHud.MessageType.Center;
            AllowTeleEverything.Effect.m_startMessage = "$rmp_startmessage";
            AllowTeleEverything.Effect.m_stopMessageType = MessageHud.MessageType.Center; // Specify where the stop effect message shows
            AllowTeleEverything.Effect.m_stopMessage = "$rmp_stopmessage";
            AllowTeleEverything.Effect.m_tooltip = "$rmp_tooltip_drink";
            //AllowTeleEverything.Effect.m_stopEffects =
            AllowTeleEverything.Effect.m_ttl = PortalDrinkTimer.Value; // 2min
            AllowTeleEverything.Effect.m_time = 0f;// starts at 0
            AllowTeleEverything.Effect.m_flashIcon = true;
            //AllowTeleEverything.Effect.m_cooldown = DrinkDuration;
            //AllowTeleEverything.Effect.IsDone();// well be true if done
            AllowTeleEverything.AddSEToPrefab(AllowTeleEverything, "PortalDrink");
        }

        internal void LoadAssets()
        {
            //ObjectDB Dat = ObjectDB.instance;

            Item portalmagicfluid = new("portalmagicfluid", "portalmagicfluid", "assetsEmbedded");
            portalmagicfluid.Name.English("Magical Portal Fluid");
            portalmagicfluid.Description.English("Once a mythical essence, now made real with Odin's blessing");
            portalmagicfluid.DropsFrom.Add("gd_king", 1f, 1, 2); // Elder drop 100% 1-2 portalFluids
            portalmagicfluid.ToggleConfigurationVisibility(Configurability.Drop);

            PortalFluidname = portalmagicfluid.Prefab.name;

            Item PortalDrink = new("portalmagicfluid", "PortalDrink", "assetsEmbedded");
            PortalDrink.Name.English("Magical Portal Drink");
            PortalDrink.Description.English("Odin's Blood of Teleportation");
            PortalDrink.ToggleConfigurationVisibility(Configurability.Drop);

            Item PortalCrystalMaster = new("portalcrystal", "PortalCrystalMaster", "assetsEmbedded");
            PortalCrystalMaster.Name.English("Gold Portal Crystal");
            PortalCrystalMaster.Description.English("Odin's Golden Crystal allows for Golden Portal Traveling and maybe more Portals");
            PortalCrystalMaster.ToggleConfigurationVisibility(Configurability.Drop);

            Item PortalCrystalRed = new("portalcrystal", "PortalCrystalRed", "assetsEmbedded");
            PortalCrystalRed.Name.English("Red Portal Crystal");
            PortalCrystalRed.Description.English("Odin's Traveling Crystals allow for Red Portal Traveling");
            PortalCrystalRed.ToggleConfigurationVisibility(Configurability.Drop);
            // PortalCrystalRed.Snapshot();

            Item PortalCrystalYellow = new("portalcrystal", "PortalCrystalYellow", "assetsEmbedded");
            PortalCrystalYellow.Name.English("Yellow Portal Crystal");
            PortalCrystalYellow.Description.English("Odin's Traveling Crystals allow for Yellow Portal Traveling");
            PortalCrystalYellow.ToggleConfigurationVisibility(Configurability.Drop);
            //PortalCrystalYellow.Snapshot();

            Item PortalCrystalGreen = new("portalcrystal", "PortalCrystalGreen", "assetsEmbedded");
            PortalCrystalGreen.Name.English("Green Portal Crystal");
            PortalCrystalGreen.Description.English("Odin's Traveling Crystals allow for Green Portal Traveling");
            PortalCrystalGreen.ToggleConfigurationVisibility(Configurability.Drop);

            Item PortalCrystalBlue = new("portalcrystal", "PortalCrystalBlue", "assetsEmbedded");
            PortalCrystalBlue.Name.English("Blue Portal Crystal");
            PortalCrystalBlue.Description.English("Odin's Traveling Crystals allow for Blue Portal Traveling");
            PortalCrystalBlue.ToggleConfigurationVisibility(Configurability.Drop);
            //PortalCrystalBlue.Snapshot();

            Item PortalCrystalCyan = new("portalcrystal", "PortalCrystalCyan", "assetsEmbedded");
            PortalCrystalCyan.Name.English("Cyan Portal Crystal");
            PortalCrystalCyan.Description.English("Odin's Traveling Crystals allow for Cyan Portal Traveling");
            PortalCrystalCyan.ToggleConfigurationVisibility(Configurability.Drop);
            // PortalCrystalCyan.Snapshot();

            Item PortalCrystalPurple = new("portalcrystal", "PortalCrystalPurple", "assetsEmbedded");
            PortalCrystalPurple.Name.English("Purple Portal Crystal");
            PortalCrystalPurple.Description.English("Odin's Traveling Crystals allow for Purple Portal Traveling");
            PortalCrystalPurple.ToggleConfigurationVisibility(Configurability.Drop);

            Item PortalCrystalTan = new("portalcrystal", "PortalCrystalTan", "assetsEmbedded");
            PortalCrystalTan.Name.English("Tan Portal Crystal");
            PortalCrystalTan.Description.English("Odin's Traveling Crystals allow for Tan Portal Traveling");
            PortalCrystalTan.ToggleConfigurationVisibility(Configurability.Drop);

            Item PortalCrystalOrange = new("portalcrystal", "PortalCrystalOrange", "assetsEmbedded");
            PortalCrystalOrange.Name.English("Orange Portal Crystal");
            PortalCrystalOrange.Description.English("Odin's Traveling Crystals allow for Orange Portal Traveling");
            PortalCrystalOrange.ToggleConfigurationVisibility(Configurability.Drop);
            //PortalCrystalOrange.Snapshot();

            Item PortalCrystalWhite = new("portalcrystal", "PortalCrystalWhite", "assetsEmbedded");
            PortalCrystalWhite.Name.English("White Portal Crystal");
            PortalCrystalWhite.Description.English("Odin's Traveling Crystals allow for White Portal Traveling");
            PortalCrystalWhite.ToggleConfigurationVisibility(Configurability.Drop);
            // PortalCrystalWhite.Snapshot();

            Item PortalCrystalBlack = new("portalcrystal", "PortalCrystalBlack", "assetsEmbedded");
            PortalCrystalBlack.Name.English("Black Portal Crystal");
            PortalCrystalBlack.Description.English("Odin's Traveling Crystals allow for Black Portal Traveling");
            PortalCrystalBlack.ToggleConfigurationVisibility(Configurability.Drop);
            // PortalCrystalBlack.Snapshot();

            Item PortalKeyYellow = new("portalcrystal", "PortalKeyYellow", "assetsEmbedded");
            PortalKeyYellow.Name.English("Yellow Portal Key");
            PortalKeyYellow.Description.English("Unlock Portals Requiring The Yellow Key");
            PortalKeyYellow.ToggleConfigurationVisibility(Configurability.Disabled);
            //PortalKeyYellow.Snapshot();

            Item PortalKeyRed = new("portalcrystal", "PortalKeyRed", "assetsEmbedded");
            PortalKeyRed.Name.English("Red Portal Key");
            PortalKeyRed.Description.English("Unlock Portals Requiring The Red Key");
            PortalKeyRed.ToggleConfigurationVisibility(Configurability.Disabled);
            //PortalKeyRed.Snapshot();

            Item PortalKeyGold = new("portalcrystal", "PortalKeyGold", "assetsEmbedded");
            PortalKeyGold.Name.English("Gold Portal Key");
            PortalKeyGold.Description.English("Unlock Gold Portals and perhaps more Portals");
            PortalKeyGold.ToggleConfigurationVisibility(Configurability.Disabled);

            Item PortalKeyBlue = new("portalcrystal", "PortalKeyBlue", "assetsEmbedded");
            PortalKeyBlue.Name.English("Blue Portal Key");
            PortalKeyBlue.Description.English("Unlock Portals Requiring The Blue Key");
            PortalKeyBlue.ToggleConfigurationVisibility(Configurability.Disabled);
            //PortalKeyBlue.Snapshot();

            Item PortalKeyGreen = new("portalcrystal", "PortalKeyGreen", "assetsEmbedded");
            PortalKeyGreen.Name.English("Green Portal Key");
            PortalKeyGreen.Description.English("Unlock Portals Requiring The Green Key");
            PortalKeyGreen.ToggleConfigurationVisibility(Configurability.Disabled);

            Item PortalKeyPurple = new("portalcrystal", "PortalKeyPurple", "assetsEmbedded");
            PortalKeyPurple.Name.English("Purple Portal Key");
            PortalKeyPurple.Description.English("Unlock Portals Requiring The Purple Key");
            PortalKeyPurple.ToggleConfigurationVisibility(Configurability.Disabled);

            Item PortalKeyTan = new("portalcrystal", "PortalKeyTan", "assetsEmbedded");
            PortalKeyTan.Name.English("Tan Portal Key");
            PortalKeyTan.Description.English("Unlock Portals Requiring The Tan Key");
            PortalKeyTan.ToggleConfigurationVisibility(Configurability.Disabled);

            Item PortalKeyCyan = new("portalcrystal", "PortalKeyCyan", "assetsEmbedded");
            PortalKeyCyan.Name.English("Cyan Portal Key");
            PortalKeyCyan.Description.English("Unlock Portals Requiring The Cyan Key");
            PortalKeyCyan.ToggleConfigurationVisibility(Configurability.Disabled);
            //PortalKeyCyan.Snapshot();

            Item PortalKeyOrange = new("portalcrystal", "PortalKeyOrange", "assetsEmbedded");
            PortalKeyOrange.Name.English("Orange Portal Key");
            PortalKeyOrange.Description.English("Unlock Portals Requiring The Orange Key");
            PortalKeyOrange.ToggleConfigurationVisibility(Configurability.Disabled);
            PortalKeyOrange.Snapshot();

            Item PortalKeyWhite = new("portalcrystal", "PortalKeyWhite", "assetsEmbedded");
            PortalKeyWhite.Name.English("White Portal Key");
            PortalKeyWhite.Description.English("Unlock Portals Requiring The White Key");
            PortalKeyWhite.ToggleConfigurationVisibility(Configurability.Disabled);
            //PortalKeyWhite.Snapshot();

            Item PortalKeyBlack = new("portalcrystal", "PortalKeyBlack", "assetsEmbedded");
            PortalKeyBlack.Name.English("Black Portal Key");
            PortalKeyBlack.Description.English("Unlock Portals Requiring The Black Key");
            PortalKeyBlack.ToggleConfigurationVisibility(Configurability.Disabled);
            //PortalKeyBlack.Snapshot();
        }

        internal void UnLoadAssets()
        {
            portalmagicfluid.Unload(false);
        }

        public void setupYMLFolderWatcher()
        {
            if (!Directory.Exists(YMLFULLFOLDER))
            {
                Directory.CreateDirectory(YMLFULLFOLDER);
                //File.WriteAllText(YMLFULL, WelcomeString + yaml); //overwrites
            }

            FileSystemWatcher watcherfolder = new(YMLFULLFOLDER);
            watcherfolder.Changed += ReadYMLValues;
            watcherfolder.Created += ReadYMLValues;
            watcherfolder.Renamed += ReadYMLValues;
            watcherfolder.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcherfolder.IncludeSubdirectories = true;
            watcherfolder.EnableRaisingEvents = true;
        }

        internal static void setupYMLFile()

        {
            Worldname = ZNet.instance.GetWorldName();
            RareMagicPortal.LogInfo("WorldName " + Worldname);
            YMLCurrentFile = Path.Combine(YMLFULLFOLDER, Worldname + ".yml");

            if (!File.Exists(YMLCurrentFile))
            {
                PortalColorLogic.PortalN = new PortalName()  // kind of iffy in inside this
                {
                    Portals = new Dictionary<string, PortalName.Portal>
                    {
                        {"Demo_Portal_Name", new PortalName.Portal() {
							//Crystal_Cost_Master = 3,
						}},
                    }
                };
                PortalColorLogic.PortalN.Portals["Demo_Portal_Name"].AdditionalProhibitItems.Add("Stone");
                PortalColorLogic.PortalN.Portals["Demo_Portal_Name"].AdditionalProhibitItems.Add("Wood");

                var serializer = new SerializerBuilder()
                    .Build();
                var yaml = serializer.Serialize(PortalColorLogic.PortalN);
                WelcomeString = WelcomeString + Environment.NewLine;

                File.WriteAllText(YMLCurrentFile, WelcomeString + yaml); //overwrites
                RareMagicPortal.LogInfo("Creating Portal_Name file " + Worldname);
                JustWrote = 2;
            }
        }

        internal void CustomSyncSmallEvent()
        {
            if (ZNet.instance.IsServer() && ZNet.instance.IsDedicated())
            {
            }
            else
            {
                if (NoMoreLoading) return;

                if (!JustWait && !NoMoreLoading)
                {
                    RareMagicPortal.LogInfo("Receiving small Portal Update"); // temp
                    string SyncedString = YMLPortalSmallData.Value;
                    if (string.IsNullOrEmpty(SyncedString))
                    {
                        return;
                    }

                    var ind = SyncedString.IndexOf(StringSeparator);
                    string PortNam = SyncedString.Substring(0, ind);
                    SyncedString = SyncedString.Remove(0, ind + 1);

                    if (ConfigEnableYMLLogs.Value == Toggle.On)
                        RareMagicPortal.LogInfo("Portalname " + PortNam + " String: " + SyncedString);

                    var deserializer = new DeserializerBuilder()
                        .Build();

                    var ymlsmall = deserializer.Deserialize<PortalName.Portal>(SyncedString);
                    string portalNCheck = PortNam;

                    if (PortalColorLogic.PortalN.Portals.ContainsKey(portalNCheck))
                    {
                        PortalColorLogic.PortalN.Portals[portalNCheck] = ymlsmall;
                    }
                    else
                    {
                        PortalColorLogic.PortalN.Portals.Add(portalNCheck, ymlsmall);
                    }

                    JustWrote = 2;
                    JustSent = 0; // ready for another send
                }
            }
        }

        internal void CustomSyncEventDetected()
        {
            //Worldname = ZNet.instance.GetWorldName();
            if (String.IsNullOrEmpty(ZNet.instance.GetWorldName()))
                JustWait = true;
            else JustWait = false;

            if (!JustWait && !NoMoreLoading)
            {
                YMLCurrentFile = Path.Combine(YMLFULLFOLDER, Worldname + ".yml");
                if (LoggingOntoServerFirst)
                {
                    RareMagicPortal.LogInfo("You are now connected to Server World" + Worldname);
                    LoggingOntoServerFirst = false;
                }
                //RareMagicPortal.LogInfo("Recieving PortalUpdate"); // temp

                string SyncedString = YMLPortalData.Value;

                if (ConfigEnableYMLLogs.Value == Toggle.On)
                    RareMagicPortal.LogInfo(SyncedString);

                var deserializer = new DeserializerBuilder()
                    .Build();

                PortalColorLogic.PortalN.Portals.Clear();
                PortalColorLogic.PortalN = deserializer.Deserialize<PortalName>(SyncedString);
                if (ZNet.instance.IsServer() && ZNet.instance.IsDedicated())
                {
                    //RareMagicPortal.LogInfo("Server Portal UPdates Are being Saved " + Worldname);
                    //File.WriteAllText(YMLCurrentFile, SyncedString);
                }
                JustWrote = 2;
                JustSent = 0; // ready for another send
            }
            if (!ZNet.instance.IsServer())
            {
                isLocal = false;
                bool admin = ConfigSync.IsAdmin;
                isAdmin = admin;
                if (isAdmin)
                    RareMagicPortal.LogInfo("You are RMP admin - full update");
                else
                    RareMagicPortal.LogInfo("You are NOT RMP admin - full update");
            }
        }

        private IEnumerator WaitforReadWrote()
        {
            yield return new WaitForSeconds(1);

            JustWrote = 0; // lets manual update happen no problem
                           //StopCoroutine(WaitforReadWrote()); not needed
        }

        private IEnumerator WaitforJustSent()
        {
            yield return new WaitForSeconds(1);

            JustSent = 0;
        }

        internal void ReadYMLValues(object sender, FileSystemEventArgs e) // Thx Azumatt // This gets hit after writing
        {
            if (!File.Exists(YMLCurrentFile)) return;
            if (isAdmin && JustWrote == 0) // if local admin or ServerSync admin
            {
                var yml = File.ReadAllText(YMLCurrentFile);

                var deserializer = new DeserializerBuilder()
                    .Build();

                PortalColorLogic.PortalN.Portals.Clear();
                PortalColorLogic.PortalN = deserializer.Deserialize<PortalName>(yml);
                if (ZNet.instance.IsServer())//&& ZNet.instance.IsDedicated()) Any Server
                {
                    RareMagicPortal.LogInfo("Server Portal YML Manual UPdate " + Worldname);
                    YMLPortalData.Value = yml;
                }
                else
                {
                    RareMagicPortal.LogInfo("Client Admin Manual YML UPdate " + Worldname);
                    if (ConfigEnableYMLLogs.Value == Toggle.On)
                        RareMagicPortal.LogInfo(yml);
                }
            }

            if (JustWrote == 2)
            {
                JustWrote = 3; // stops from doing again
                StartCoroutine(WaitforReadWrote());
            }

            if (JustWrote == 1)
                JustWrote = 2;

            if (!isAdmin)
            {
                //RareMagicPortal.LogInfo("Portal Values Didn't change because you are not an admin");
            }
        }

        internal void SetupWatcher() // Thx Azumatt
        {
            FileSystemWatcher watcher = new(BepInEx.Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        internal static void ReadYMLValuesBoring() // Startup File
        {
            if (JustWait)
            {
                Worldname = ZNet.instance.GetWorldName();
                JustWait = false;
                YMLCurrentFile = Path.Combine(YMLFULLFOLDER, Worldname + ".yml");
                if (LoggingOntoServerFirst)
                {
                    RareMagicPortal.LogInfo("You are now connected to Server World" + Worldname);
                    LoggingOntoServerFirst = false;
                }

                isLocal = false;
                if (ZNet.instance.IsServer() && ZNet.instance.IsDedicated())
                {
                    //isDedServer = true;
                }
                string SyncedString = YMLPortalData.Value;

                if (ConfigEnableYMLLogs.Value == Toggle.On)
                    RareMagicPortal.LogInfo(SyncedString);

                var deserializer2 = new DeserializerBuilder()
                    .Build();

                //PortalN.Portals.Clear();
                PortalColorLogic.PortalN = deserializer2.Deserialize<PortalName>(SyncedString);
                JustWrote = 2;
                File.WriteAllText(YMLCurrentFile, WelcomeString + SyncedString); //overwrites
            }
            else
            {
                if (!File.Exists(YMLCurrentFile)) return;
                var yml = File.ReadAllText(YMLCurrentFile);

                var deserializer = new DeserializerBuilder()
                    .Build();
                //PortalN.Portals.Clear();
                PortalColorLogic.PortalN = new PortalName(); // init
                PortalColorLogic.PortalN = deserializer.Deserialize<PortalName>(yml);
                if (ConfigEnableYMLLogs.Value == Toggle.On)
                    RareMagicPortal.LogInfo(yml);

                if (ZNet.instance.IsServer()) // not just dedicated should send this
                {
                    YMLPortalData.Value = yml; // should only be one time and for server
                }
            }
        }

        internal void ReadConfigValues(object sender, FileSystemEventArgs e) // Thx Azumatt
        {
            if (!File.Exists(ConfigFileFullPath)) return;

            bool admin = ConfigSync.IsAdmin;
            bool locked = ConfigSync.IsLocked;
            bool islocaladmin = false;
            bool isdedServer = false;
            if (ZNet.instance.IsServer() && !ZNet.instance.IsDedicated())
                islocaladmin = true;
            if (ZNet.instance.IsServer() && ZNet.instance.IsDedicated())
                isdedServer = true;

            if (admin || islocaladmin || isdedServer) // Server Sync Admin Only
            {
                isAdmin = admin; // need to check this
                try
                {
                    if (islocaladmin)
                    {
                        isAdmin = true;
                        RareMagicPortal.LogInfo("ReadConfigValues loaded- you are a local admin");
                        Config.Reload();
                        ReadAndWriteConfigValues();
                        PortalChanger();
                    }
                    else if (isdedServer)
                    {
                        RareMagicPortal.LogInfo("ReadConfigValues loaded- you the dedicated Server");
                        Config.Reload();
                        ReadAndWriteConfigValues();
                        PortalChanger();
                    }
                    else if (ConfigSync.IsSourceOfTruth)
                    {
                        RareMagicPortal.LogInfo("ReadConfigValues loaded- you are an admin - on a server");
                        Config.Reload();
                        ReadAndWriteConfigValues();
                        PortalChanger();
                    }
                    else
                    {
                        // false so remote config is being used
                        RareMagicPortal.LogInfo("Server values loaded");
                        ReadAndWriteConfigValues();
                        PortalChanger();
                    }
                }
                catch
                {
                    RareMagicPortal.LogInfo($"There was an issue loading your {ConfigFileName}");
                    RareMagicPortal.LogInfo("Please check your config entries for spelling and format!");
                }
            }
            else
            {
                isAdmin = false;
                RareMagicPortal.LogInfo("You are not ServerSync admin");
                try
                {
                    if (ConfigSync.IsSourceOfTruth && !locked)
                    {
                        RareMagicPortal.LogInfo("ReadConfigValues loaded- You are an local admin"); // Server hits this on dedicated
                        ReadAndWriteConfigValues();
                        PortalChanger();
                        isAdmin = true; // local admin as well
                    }
                    else
                    {
                        RareMagicPortal.LogInfo("You are not an Server admin - Server Sync Values loaded "); // regular non admin clients
                        ReadAndWriteConfigValues();
                        PortalChanger();
                    }
                }
                catch
                {
                    RareMagicPortal.LogInfo($"There was an issue loading your {ConfigFileName}");
                    RareMagicPortal.LogInfo("Please check your config entries for spelling and format!");
                }
            }
        }

        // changing portals section
        internal static void PortalChanger()
        {
            var peter = functions.GetPieces().Find((GameObject g) => Utils.GetPrefabName(g) == "portal_wood"); //item prefab loaded from hammer
            if (peter != null)
            {
                WearNTear por = peter.GetComponent<WearNTear>();
                por.m_health = ConfiglHealth.Value; // set New Portal Health

                List<Piece.Requirement> requirements = new List<Piece.Requirement>();
                requirements.Add(new Piece.Requirement
                {
                    m_amount = 20,
                    m_resItem = ObjectDB.instance.GetItemPrefab("FineWood").GetComponent<ItemDrop>(),
                    m_recover = true
                });
                if (ConfigFluid.Value)
                { // make this more dynamic
                    requirements.Add(new Piece.Requirement
                    {
                        m_amount = 1,
                        m_resItem = ObjectDB.instance.GetItemPrefab("PortalMagicFluid").GetComponent<ItemDrop>(),
                        m_recover = true
                    });
                }
                requirements.Add(new Piece.Requirement
                {
                    m_amount = 10,
                    m_resItem = ObjectDB.instance.GetItemPrefab("GreydwarfEye").GetComponent<ItemDrop>(),
                    m_recover = true
                });
                requirements.Add(new Piece.Requirement
                {
                    m_amount = 2,
                    m_resItem = ObjectDB.instance.GetItemPrefab("SurtlingCore").GetComponent<ItemDrop>(),
                    m_recover = true
                });

                var CraftingStationforPaul = functions.GetCraftingStation(ConfigTable.Value);
                if (CraftingStationforPaul == null)
                {
                    CraftingStationforPaul.m_name = DefaultTable;
                }

                Piece petercomponent = peter.GetComponent<Piece>();
                petercomponent.m_craftingStation = functions.GetCraftingStation(CraftingStationforPaul.m_name); // sets crafting station workbench/forge /ect

                if (ConfigFluid.Value)
                    FluidwasTrue = true;

                if (ConfigFluid.Value || FluidwasTrue)
                    petercomponent.m_resources = requirements.ToArray(); // always update?

                //RareMagicPortal.LogInfo($"There changing fluid value {PortalFluidname}");
                ObjectDB.instance.GetItemPrefab(PortalFluidname).GetComponent<ItemDrop>().m_itemData.m_shared.m_value = ConfigFluidValue.Value;
            }       // if loop
        }

        internal static void StartingFirsttime()
        {
            firstTime = true;
        }

        internal static void StartingitemPrefab()
        {
            if (firstTime && ConfigSpawn.Value != 0 && ConfigFluid.Value)
            {
                RareMagicPortal.LogInfo("New Starting Item Set");
                Inventory inventory = ((Humanoid)Player.m_localPlayer).m_inventory;
                inventory.AddItem("PortalMagicFluid", ConfigSpawn.Value, 1, 0, 0L, "");
                firstTime = false;
            }
        }

        public static IEnumerator DelayedLoad()
        {
            yield return new WaitForSeconds(0.05f);
            LoadAllRecipeData(reload: true);
            //yield break;

            // I need to keep checking until the world name is populated- probably at respawn
            while (String.IsNullOrEmpty(ZNet.instance.GetWorldName()) && !NoMoreLoading)
            {
                yield return new WaitForSeconds(0.1f);
            }
            LoadIN();
            yield break;
        }

        internal static void LoadAllRecipeData(bool reload)
        {
            if (reload) // waits until the last seconds to reference and overwrite
            {
                PortalChanger();
            }
        }

        #region ConfigOptions

        internal static ConfigEntry<bool>? _serverConfigLocked;

        internal static readonly CustomSyncedValue<string> YMLPortalData = new(ConfigSync, "PortalYmlData", ""); // doesn't show up in config
        internal static readonly CustomSyncedValue<string> YMLPortalSmallData = new(ConfigSync, "PortalYmlSmallData", ""); // doesn't show up in config

        internal ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        internal ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        internal class ConfigurationManagerAttributes
        {
            public bool? Browsable = false;
        }

        #endregion ConfigOptions

        internal void CreateConfigValues()
        {
            _serverConfigLocked = config("1.General", "Force Server Config", true, "Force Server Config");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);

            ConfigEnableYMLLogs = config("1.General", "YML Portal Logs", Toggle.Off, "Show YML Portal Logs after Every update", false);

            RiskyYMLSave = config("1.General", "Risky Server Save", Toggle.Off, "Only save YML updates when server shuts down");

            UseSmallUpdates = config("1.General", "Use Small Server Updates", Toggle.On, "Only sends a tiny part of the YML to clients");

            PreventTargetPortalFromChanging = config("1.General", "PrventTargetPortalChange", Toggle.On, "Prevent People (non creator/admin) from changing TargetPortal Mode. Groups/Private/Public etc");

            portalRMPKEY = config("1.Portal Config", "Modifier key for Color", new KeyboardShortcut(KeyCode.CapsLock), "Modifier keY that has to be pressed while hovering over Portal + E", false);

            portalRMPMODEKEY = config("1.Portal Config", "Modifier key for PortalMode", new KeyboardShortcut(KeyCode.LeftControl), "Modifier key that has to be pressed while hovering over Portal + E", false);

            portalRMPCRYSTALKEY = config("1.Portal Config", "ON/OFF for Crystal Requirement", new KeyboardShortcut(KeyCode.LeftAlt), "Modifier key that has to be pressed while hovering over Portal + E", false);

            ConfigFluid = config("2.PortalFluid", "Enable Portal Fluid", false,
                            "Enable PortalFluid requirement?");

            ConfigSpawn = config("2.PortalFluid", "Portal Magic Fluid Spawn", 0,
                "How much PortalMagicFluid to start with on a new character?");

            // ConfigFluidValue = config("2.PortalFluid", "Portal Fluid Value", 0, "What is the value of MagicPortalFluid? " + System.Environment.NewLine + "A Value of 1 or more, makes the item saleable to trader"); Apart of ItemManager now

            ConfigTable = config("3.Portal Config", "CraftingStation Requirement", DefaultTable,
                "Which CraftingStation is required nearby?" + System.Environment.NewLine + "Default is Workbench = $piece_workbench, forge = $piece_forge, Artisan station = $piece_artisanstation " + System.Environment.NewLine + "Pick a valid table otherwise default is workbench"); // $piece_workbench , $piece_forge , $piece_artisanstation

            ConfigTableLvl = config("3.Portal Config", "Level of CraftingStation Req", 1,
                "What level of CraftingStation is required for placing Portal?");

            ConfigCreator = config("3.Portal Config", "Only Creator Can Deconstruct", true, "Only the Creator/Admin of the Portal can deconstruct it. It can still be destroyed");

            ConfiglHealth = config("3.Portal Config", "Portal Health", 400f, "Health of Portal");

            ConfigAddRestricted = config("3.Portal Config", "AdditionalProhibitItems", "", "Additional Items to Restrict by Default - 'Wood,Stone'");

            ConfigAllowItems = config("3.Portal Config", "AdditionalAllowItems", "", "Additional Items to be allowed by Default - 'Wood,Stone'");

            ConfigCreatorLock = config("3.Portal Config", "Only Creator Can Change Name", false, "Only Creator can change Portal name");

            ConfigTargetPortalAnimation = config("3.Portal Config", "Force Portal Animation", Toggle.Off, "Forces Portal Animation for Target Portal Mod, is not synced and only applies the config if the mod is loaded", false);

            ConfigMaxWeight = config("3.Portal Config", "Max Weight Allowed for new Portals", 0, "This affects all new/renamed portals - Enter the max weight that can transit through a portal at a time. Value of 0 disables the check");

            MaxPortalsPerPerson = config("3.Portal Config", "Max Portals Per Player", 0, "The YML keeps track of creator of Portals, a Value of 0 disables the check");

            AdminOnlyMakesPortals = config("3.Portal Config", "Only Admin Can Build", Toggle.Off, "Only The Admins Can Build Portals");

            ConfigEnableCrystalsNKeys = config("4.Portal Crystals", "CrystalActive", false, "Enable Portal Crystals and Keys Usage for All new ports by default" + System.Environment.NewLine + " Only Admins can change the colors on these portals");

            ConfigEnableGoldAsMaster = config("4.Portal Crystals", "Use Gold as Portal Master", Toggle.On, "Enabled Gold Key and Crystal as Master Key to all (Red,Green,Blue,Purple,Tan,Gold)");

            ConfigCrystalsConsumable = config("4.Portal Crystals", "Crystal Consume Per Transit", 1, "What is the number of crystals to consume for each Portal transport with Crystals enabled?" + System.Environment.NewLine + " Gold/Master gets set" + System.Environment.NewLine);

            ConfigMessageLeft = config("4.Portal Crystals", "Use Top Left Message", Toggle.Off, "In case a mod is interfering with Center Messages for Portal tags, display on TopLeft instead.");

            PortalDrinkTimer = config("5.Portal Drink", "Portal Drink Timer", 120, "How Long Odin's Drink lasts");

            PortalDrinkDeny = config("5.Portal Drink", "Portal Drink Wont Allow", "", "Deny list even with Portal Drink, 'Bronze,BlackMetal,BlackMetalScrap,Copper,CopperOre,CopperScrap,Tin,TinOre,IronOre,Iron,IronScrap,Silver,SilverOre,DragonEgg'");

            ConfigUseBiomeColors = config("6.BiomeColors", "Use Biome Colors by Default", Toggle.Off, "This will Override - Default Color and Use Specific Colors for Biomes");

            BiomeRepColors = config("6.BiomeColors", "Biome Colors", "Meadows:Tan,BlackForest:Blue,Swamp:Green,Mountain:Black,Plains:Orange,Mistlands:Purple,DeepNorth:Cyan,AshLands:Red,Ocean:Blue", "Biomes and their related Colors. - No spaces");

            DefaultColor = config("7.Colors", "Default Color", "Yellow", "Yellow,Red,Green,Blue,Purple,Tan,Cyan,Orange,White,Black,Gold are available Colors");

            EnabledColors = config("7.Colors", "Enabled Colors for Portals", "Yellow,Red,Green,Blue,Purple,Tan,Cyan,Orange,White,Black,Gold", "Yellow,Red,Green,Blue,Purple,Tan,Cyan,Orange,White,Black,Gold are available Colors that can be enabled, removing them disables the color");

            FreePassageColor = config("7.Colors", "Free Passage Color", "Yellow", "Yellow,Red,Green,Blue,Purple,Tan,Cyan,Orange,White,Black,Gold or none are the available Colors that can be selected for the Free Passage Color - Only 1 can be set - Default is Yellow");

            AdminColor = config("7.Colors", "Admin only Color", "none", "Yellow,Red,Green,Blue,Purple,Tan,Cyan,Orange,White,Black,Gold or none are the available Colors that can be selected for the Admin only portals - Only 1 can be set - Default is none");

            TelePortAnythingColor = config("7.Colors", "TelePortAnythingColor", "none", "Yellow,Red,Green,Blue,Purple,Tan,Cyan,Orange,White,Black,Gold or none are the available Colors that can be selected for the TeleportAnything only portals - Only 1 can be set - Default is none");

            PortalDrinkColor = config("7.Colors", "Portal Drink Color", "Rainbow", "Yellow,Red,Green,Blue,Purple,Tan,Cyan,Orange,White,Black,Gold or Rainbow (Alternates between colors every second) are the available Colors that can be selected for the Portal Drink Mode for Portals - Only 1 can be set - Default is Rainbow ");

            PreventColorChange = config("7.Colors", "Prevent Color Changing", Toggle.Off, "If true, only admins can change color. This will be overriden if CrystalActive is set for Portal");

            ColorYELLOWAllows = config("8.ColorsAllow", "Color Yellow Allows", "", "IF CrystalActive is active on Portal or 'Prevent Color Changing' is true then these additional Allows are active. 'Iron,Copper' ");

            ColorBLUEAllows = config("8.ColorsAllow", "Color Blue Allows", "", "IF CrystalActive is active on Portal or 'Prevent Color Changing' is true then these additional Allows are active. 'Iron,Copper' ");

            ColorGREENAllows = config("8.ColorsAllow", "Color Green Allows", "", "IF CrystalActive is active on Portal or 'Prevent Color Changing' is true then these additional Allows are active. 'Iron,Copper' ");

            ColorPURPLEAllows = config("8.ColorsAllow", "Color Purple Allows", "", "IF CrystalActive is active on Portal or 'Prevent Color Changing' is true then these additional Allows are active. 'Iron,Copper' ");

            ColorTANAllows = config("8.ColorsAllow", "Color Tan Allows", "", "IF CrystalActive is active on Portal or 'Prevent Color Changing' is true then these additional Allows are active. 'Iron,Copper' ");

            ColorCYANAllows = config("8.ColorsAllow", "Color Cyan Allows", "", "IF CrystalActive is active on Portal or 'Prevent Color Changing' is true then these additional Allows are active. 'Iron,Copper' ");

            ColorORANGEAllows = config("8.ColorsAllow", "Color Orange Allows", "", "IF CrystalActive is active on Portal or 'Prevent Color Changing' is true then these additional Allows are active. 'Iron,Copper' ");

            ColorBLACKAllows = config("8.ColorsAllow", "Color Black Allows", "", "IF CrystalActive is active on Portal or 'Prevent Color Changing' is true then these additional Allows are active. 'Iron,Copper' ");

            ColorWHITEAllows = config("8.ColorsAllow", "Color White Allows", "", "IF CrystalActive is active on Portal or 'Prevent Color Changing' is true then these additional Allows are active. 'Iron,Copper' ");

            ColorGOLDAllows = config("8.ColorsAllow", "Color Gold Allows", "", "IF CrystalActive is active on Portal or 'Prevent Color Changing' is true then these additional Allows are active. 'Iron,Copper' ");

            GemColorGold = config("9.CrystalSelector", "Use for Crystal Gold", CrystalMaster, "You can use default or use an item like JewelCrafting crystal - $jc_shattered_orange_crystal, $jc_uncut_purple_stone, $jc_black_socket, $jc_adv_blue_socket, $jc_perfect_purple_socket, " + System.Environment.NewLine + " This is the ItemDrop.shared.m_name, the correct name might not be easy to guess. Annoy Odins discord or use UnityExplorer - must reboot game");

            GemColorRed = config("9.CrystalSelector", "Use for Crystal Red", CrystalRed, "You can use default or use an item like JewelCrafting crystal - $jc_shattered_orange_crystal, $jc_uncut_purple_stone, $jc_black_socket, $jc_adv_blue_socket, $jc_perfect_purple_socket, " + System.Environment.NewLine + " This is the ItemDrop.shared.m_name, the correct name might not be easy to guess. Annoy Odins discord or use UnityExplorer - must reboot game");

            GemColorGreen = config("9.CrystalSelector", "Use for Crystal Green", CrystalGreen, "You can use default or use an item like JewelCrafting crystal - $jc_shattered_orange_crystal, $jc_uncut_purple_stone, $jc_black_socket, $jc_adv_blue_socket, $jc_perfect_purple_socket, " + System.Environment.NewLine + " This is the ItemDrop.shared.m_name, the correct name might not be easy to guess. Annoy Odins discord or use UnityExplorer - must reboot game");

            GemColorBlue = config("9.CrystalSelector", "Use for Crystal Blue", CrystalBlue, "You can use default or use an item like JewelCrafting crystal - $jc_shattered_orange_crystal, $jc_uncut_purple_stone, $jc_black_socket, $jc_adv_blue_socket, $jc_perfect_purple_socket, " + System.Environment.NewLine + " This is the ItemDrop.shared.m_name, the correct name might not be easy to guess. Annoy Odins discord or use UnityExplorer - must reboot game");

            GemColorYellow = config("9.CrystalSelector", "Use for Crystal Yellow", CrystalYellow, "You can use default or use an item like JewelCrafting crystal - $jc_shattered_orange_crystal, $jc_uncut_purple_stone, $jc_black_socket, $jc_adv_blue_socket, $jc_perfect_purple_socket, " + System.Environment.NewLine + " This is the ItemDrop.shared.m_name, the correct name might not be easy to guess. Annoy Odins discord or use UnityExplorer - must reboot game");

            GemColorPurple = config("9.CrystalSelector", "Use for Crystal Purple", CrystalPurple, "You can use default or use an item like JewelCrafting crystal - $jc_shattered_orange_crystal, $jc_uncut_purple_stone, $jc_black_socket, $jc_adv_blue_socket, $jc_perfect_purple_socket, " + System.Environment.NewLine + " This is the ItemDrop.shared.m_name, the correct name might not be easy to guess. Annoy Odins discord or use UnityExplorer - must reboot game");

            GemColorTan = config("9.CrystalSelector", "Use for Crystal Tan", CrystalTan, "You can use default or use an item like JewelCrafting crystal - $jc_shattered_orange_crystal, $jc_uncut_purple_stone, $jc_black_socket, $jc_adv_blue_socket, $jc_perfect_purple_socket, " + System.Environment.NewLine + " This is the ItemDrop.shared.m_name, the correct name might not be easy to guess. Annoy Odins discord or use UnityExplorer - must reboot game");

            GemColorCyan = config("9.CrystalSelector", "Use for Crystal Cyan", CrystalCyan, "You can use default or use an item like JewelCrafting crystal - $jc_shattered_orange_crystal, $jc_uncut_purple_stone, $jc_black_socket, $jc_adv_blue_socket, $jc_perfect_purple_socket, " + System.Environment.NewLine + " This is the ItemDrop.shared.m_name, the correct name might not be easy to guess. Annoy Odins discord or use UnityExplorer - must reboot game");

            GemColorOrange = config("9.CrystalSelector", "Use for Crystal Orange", CrystalOrange, "You can use default or use an item like JewelCrafting crystal - $jc_shattered_orange_crystal, $jc_uncut_purple_stone, $jc_black_socket, $jc_adv_blue_socket, $jc_perfect_purple_socket, " + System.Environment.NewLine + " This is the ItemDrop.shared.m_name, the correct name might not be easy to guess. Annoy Odins discord or use UnityExplorer - must reboot game");

            GemColorWhite = config("9.CrystalSelector", "Use for Crystal White", CrystalWhite, "You can use default or use an item like JewelCrafting crystal - $jc_shattered_orange_crystal, $jc_uncut_purple_stone, $jc_black_socket, $jc_adv_blue_socket, $jc_perfect_purple_socket, " + System.Environment.NewLine + " This is the ItemDrop.shared.m_name, the correct name might not be easy to guess. Annoy Odins discord or use UnityExplorer - must reboot game");

            GemColorBlack = config("9.CrystalSelector", "Use for Crystal Black", CrystalBlack, "You can use default or use an item like JewelCrafting crystal - $jc_shattered_orange_crystal, $jc_uncut_purple_stone, $jc_black_socket, $jc_adv_blue_socket, $jc_perfect_purple_socket, " + System.Environment.NewLine + " This is the ItemDrop.shared.m_name, the correct name might not be easy to guess. Annoy Odins discord or use UnityExplorer - must reboot game");
        }

        internal void ReadAndWriteConfigValues()
        {
            /*
            if (CraftingStationlvl > 10 || CraftingStationlvl < 1)
                CraftingStationlvl = 1;

            */
            if (PortalDrinkDeny.Value != "")
                PortalDrinkDenyloc = MagicPortalFluid.PortalDrinkDeny.Value.Split(',').ToList();

            PortalColorLogic.reloaded = true;
            AllowTeleEverything.Effect.m_cooldown = PortalDrinkTimer.Value;

            PortalColorLogic.reloadcolors();
        }

        /* maybe keep?
                internal static void HandleTeleport(Minimap Instancpass) // this is just for testing
                {
                    Minimap instance = Instancpass;
                    List<Minimap.PinData> paul = instance.m_pins;
                    Vector3 pos = instance.ScreenToWorldPoint(Input.mousePosition);
                    float radius = instance.m_removeRadius * (instance.m_largeZoom * 2f);

                    Quaternion rotation = Quaternion.Euler(0f, 0f, 0f);

                    instance.SetMapMode(Minimap.MapMode.Small);

                    Player.m_localPlayer.TeleportTo(pos + rotation * Vector3.forward + Vector3.up, rotation, true);
                }

                static void SetParticleColors(
               IEnumerable<Light> lights,
               IEnumerable<ParticleSystem> systems,
               IEnumerable<ParticleSystemRenderer> renderers,
               Color targetColor)
                {
                    var targetColorGradient = new ParticleSystem.MinMaxGradient(targetColor);

                    foreach (ParticleSystem system in systems)
                    {
                        var colorOverLifetime = system.colorOverLifetime;

                        if (colorOverLifetime.enabled)
                        {
                            colorOverLifetime.color = targetColorGradient;
                        }

                        var sizeOverLifetime = system.sizeOverLifetime;

                        if (sizeOverLifetime.enabled)
                        {
                            var main = system.main;
                            main.startColor = targetColor;
                        }
                    }

                    foreach (ParticleSystemRenderer renderer in renderers)
                    {
                        renderer.material.color = targetColor;
                    }

                    foreach (Light light in lights)
                    {
                        light.color = targetColor;
                    }
                }

        */
    }// end of  class

    public static class ObjectExtensions
    {
        public static T Ref<T>(this T o) where T : UnityEngine.Object
        {
            return o ? o : null;
        }
    }
}// end of namespace