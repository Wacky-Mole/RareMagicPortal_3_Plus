using RareMagicPortal;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows;
using static RareMagicPortal.PortalColorLogic;
using static RareMagicPortal.PortalName;
using Guilds;
using Toggle = UnityEngine.UI.Toggle;
using System.Linq;

namespace RareMagicPortal_3_Plus.PortalMode
{
    public abstract class BasePopup : MonoBehaviour
    {
        public GameObject popupPrefab;

        internal GameObject Panel;
        internal GameObject Lists;
        protected GameObject popupInstance;
        protected Dropdown modeDropdown;
        protected Button submitButton;
        protected Text promptText;
        protected Button closeButton;
        
       // public BasePopup _popInstance = null;;

        // Update method signature to match the extended use case
        public void ShowPopup(Action<PortalModeClass.PortalMode, ModeSelectionPopup> onSubmit, string color, string PortalName, string zdo)
        {
            popupInstance = Instantiate(MagicPortalFluid.uiasset.LoadAsset<GameObject>("RMPUIpop"));

            Panel = popupInstance.transform.Find("Canvas/MainPanel/Panel").gameObject;
            closeButton = popupInstance.transform.Find("Canvas/MainPanel/Close").GetComponent<Button>();
            Lists = Panel.transform.Find("Lists").gameObject;
            modeDropdown = Lists.transform.Find("PortalMode/Dropdown").GetComponent<Dropdown>();
            

            if (modeDropdown == null )
            {
                Debug.LogError("Popup components not found. Please ensure the prefab has modeDropdown.");
                return;
            }

            Player player = Player.m_localPlayer;

            if (player != null && !InventoryGui.IsVisible())
            {
                // Force the player's inventory to open
                InventoryGui.instance.Show(null);
            }

            submitButton = Panel.transform.Find("SubmitButton/Button").GetComponent<Button>();
            promptText = Panel.transform.Find("ModeDescriptionText/Text")?.GetComponent<Text>();

            if (modeDropdown == null || submitButton == null)
            {
                Debug.LogError("Popup components not found. Please ensure the prefab has ModeDropdown and SubmitButton.");
                return;
            }

            // Set the prompt text if applicable
            if (promptText != null)
            {
                promptText.text = "Select Portal Mode: or else";
            }

            // Populate the dropdown with portal modes
            PopulateDropdown();

            closeButton.onClick.AddListener(CloseUI);

            // Add listener to the submit button
            submitButton.onClick.AddListener(() => OnSubmit(onSubmit, color, PortalName, zdo));
        }

        private void PopulateDropdown()
        {
            // Get all portal modes and add them to the dropdown
            List<string> modeNames = new List<string>(Enum.GetNames(typeof(PortalModeClass.PortalMode)));
            modeDropdown.ClearOptions();
            modeDropdown.AddOptions(modeNames);
        }

        private void CloseUI()
        {
            if (popupInstance != null)
            {
                // Remove listeners to prevent lingering callbacks
                closeButton.onClick.RemoveAllListeners();
                submitButton.onClick.RemoveAllListeners();

                // Destroy the popup instance safely
                Destroy(popupInstance);
                popupInstance = null;
            }

            // Also reset the static _popupInstance reference to null
            ModeSelectionPopup._popupInstance = null;
        }

        protected virtual void OnSubmit(Action<PortalModeClass.PortalMode, ModeSelectionPopup> onSubmit, string color, string PortalName, string zdo)
        {
            // Get the selected portal mode from the dropdown
            int selectedIndex = modeDropdown.value;
            PortalModeClass.PortalMode selectedMode = (PortalModeClass.PortalMode)selectedIndex;

            // Invoke the callback with the selected mode and an empty string for extra input
            onSubmit?.Invoke(selectedMode, ModeSelectionPopup._popupInstance);

            // Destroy the popup after submission
            Destroy(popupInstance);
            popupInstance = null;
        }
    }
    public class ModeSelectionPopup : BasePopup
    {
        public static ModeSelectionPopup _popupInstance = null;
        public GameObject password;
        public InputField passwordInputField;
        public GameObject coordinates;
        public InputField coordinatesInputField;
        public GameObject transportNet;
        public InputField transportNetInputField;
        public GameObject allowedUsers;
        public InputField allowedUsersInputField;
        public InputField addBlockField;
        public InputField addAllowField;
        public InputField weightField;
        public GameObject GuildsGame;
        public Dropdown GuilddropField;

        public Toggle allowEverythingBox;
        public Toggle hoverNameBox;
        public Toggle crystalsKeysBox;
        public GameObject crystalsKeysGameobject;
        public Toggle fastTeleportBox;
        public Image crystalsKeysBackgroundImage;

        private Color currentcolor;
        private string colorName;
        private string colorNameLower;
        public PortalModeClass.PortalMode selectedMode;
        public string portalName;
        public string zdo;

        public void DestorySelf()
        {
            _popupInstance = null;
            Destroy(popupInstance);
            popupInstance = null;
            
        }
        public void ShowModeSelectionPopup(Action<PortalModeClass.PortalMode, ModeSelectionPopup> onSubmit, string color, string PortalName, string Zdo)
        {
            ShowPopup(onSubmit, color, PortalName, zdo); // Call the base ShowPopup method with the correct signature

            _popupInstance = this;
            // Find the UI components specific to ModeSelectionPopup
            password = Lists.transform.Find("PasswordInputField").gameObject;
            passwordInputField = Lists.transform.Find("PasswordInputField/InputField")?.GetComponent<InputField>();
            coordinates = Lists.transform.Find("CoordinatesInputField").gameObject;
            coordinatesInputField = Lists.transform.Find("CoordinatesInputField/InputField")?.GetComponent<InputField>();
            transportNet = Lists.transform.Find("TransportNet").gameObject;
            transportNetInputField = Lists.transform.Find("TransportNet/InputField")?.GetComponent<InputField>();
            allowedUsers = Lists.transform.Find("AllowedUsers").gameObject;
            allowedUsersInputField = Lists.transform.Find("AllowedUsers/InputField")?.GetComponent<InputField>();
            addBlockField = Lists.transform.Find("AddRestrict/InputField")?.GetComponent<InputField>();
            addAllowField = Lists.transform.Find("AddAllow/InputField")?.GetComponent<InputField>();
            weightField = Lists.transform.Find("Weight/InputField")?.GetComponent<InputField>();
            GuildsGame = Lists.transform.Find("GuildInputField").gameObject;
            GuilddropField = Lists.transform.Find("GuildInputField/Dropdown")?.GetComponent<Dropdown>() ;

            allowEverythingBox = Lists.transform.Find("AllowEverything/Toggle")?.GetComponent<Toggle>();
            hoverNameBox = Lists.transform.Find("DisplayPortalName/Toggle")?.GetComponent<Toggle>();
            crystalsKeysBox = Lists.transform.Find("CrystalKeyMode/Toggle")?.GetComponent<Toggle>();
            crystalsKeysGameobject = Lists.transform.Find("CrystalKeyMode").gameObject;
            fastTeleportBox = Lists.transform.Find("FastTeleport/Toggle")?.GetComponent<Toggle>();
            crystalsKeysBackgroundImage = Lists.transform.Find("CrystalKeyMode/Toggle/Background")?.GetComponent<Image>();
            

            if (passwordInputField == null || coordinatesInputField == null || transportNetInputField == null)
            {
                Debug.LogError("Popup components not found. Please ensure the prefab has all necessary components.");
                return;
            }

            // Add listener to the dropdown to update UI based on selection
            modeDropdown.onValueChanged.AddListener(delegate { OnModeChanged(); });
            crystalsKeysBox.onValueChanged.AddListener(delegate { OnCrystalKeyChange(); });

            // Initially hide extra input fields
            password.SetActive(false);
            coordinates.SetActive(false);
            transportNet.SetActive(false);
            allowedUsers.SetActive(false);
            crystalsKeysGameobject.SetActive(false);
            GuildsGame.SetActive(false);


            currentcolor = PortalColorLogic.PortalColors[color].HexName;
            crystalsKeysBackgroundImage.color = currentcolor;
            colorName = color;
            colorNameLower = color.ToLower();

            portalName = PortalName;
            zdo = Zdo;

                
            selectedMode = PortalModeClass.GetCurrentMode(PortalName, zdo);
            
            RMP.LogMessage("Selected Mode is " + selectedMode);
            modeDropdown.value = (int)selectedMode;
            crystalsKeysBox.isOn = PortalColorLogic.PortalN.Portals[portalName].PortalZDOs[zdo].CrystalActive;
            OnCrystalKeyChange();
            PopulateSelected();

            // Override the submit button listener to handle the extended callback
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(() => OnSubmit(onSubmit, color, PortalName, zdo));
        }

        private void PopulateSelected()
        {

            switch (selectedMode)
            {
                case PortalModeClass.PortalMode.PasswordLock:
                    passwordInputField.text = PortalColorLogic.PortalN.Portals[portalName].PortalZDOs[zdo].Password;
                    crystalsKeysBox.isOn = PortalColorLogic.PortalN.Portals[portalName].PortalZDOs[zdo].CrystalActive;
                    break;
                case PortalModeClass.PortalMode.OneWayPasswordLock:
                    passwordInputField.text = PortalColorLogic.PortalN.Portals[portalName].PortalZDOs[zdo].Password;
                    crystalsKeysBox.isOn = PortalColorLogic.PortalN.Portals[portalName].PortalZDOs[zdo].CrystalActive;
                    break;
                case PortalModeClass.PortalMode.CordsPortal:
                    //PortalModeClass.TryParseCoordinates(PortalColorLogic.PortalN.Portals[portalName].PortalZDOs[zdo].Coords, out Vector3 coords);
                    var vector = PortalColorLogic.PortalN.Portals[portalName].PortalZDOs[zdo].Coords;
                    //string vectorString = $"{vector.x},{vector.y},{vector.z}";
                    coordinatesInputField.text = vector;
                    crystalsKeysBox.isOn = PortalColorLogic.PortalN.Portals[portalName].PortalZDOs[zdo].CrystalActive;
                    break;
                case PortalModeClass.PortalMode.TransportNetwork:
                    transportNetInputField.text = portalName;
                    transportNetInputField.readOnly = true;
                    break;                
                case PortalModeClass.PortalMode.AllowedUsersOnly:
                    crystalsKeysBox.isOn = PortalColorLogic.PortalN.Portals[portalName].PortalZDOs[zdo].CrystalActive;

                    if (MagicPortalFluid.GuildsLoaded)
                    {
                        if (Guilds.API.IsLoaded())
                        {
                            GuildsGame.SetActive(true);

                            List<Guild> guilds = Guilds.API.GetGuilds();
                            GuilddropField.ClearOptions();

                            // Prepare a list of guild names and add "None" as the first option.
                            List<string> guildNames = new List<string> { "None" };
                            guildNames.AddRange(guilds.Select(g => g.Name));

                            GuilddropField.AddOptions(guildNames);

                            // If a guild is already assigned to the portal, set it as the current selection.
                            string assignedGuild = PortalColorLogic.PortalN.Portals[portalName].GuildOnly;
                            if (string.IsNullOrEmpty(assignedGuild))
                            {
                                GuilddropField.value = 0;
                            }
                            else
                            {
                                // Otherwise, find the index of the assigned guild and set it.
                                int selectedIndex = guildNames.FindIndex(g => g.Equals(assignedGuild, StringComparison.OrdinalIgnoreCase));
                                if (selectedIndex >= 0)
                                {
                                    GuilddropField.value = selectedIndex;
                                }
                            }
                        }
                    }

                    break;
                // Now for the Box checking
                case PortalModeClass.PortalMode.CrystalKeyMode:
                    crystalsKeysBox.isOn = true;
                    break;                
                case PortalModeClass.PortalMode.RandomTeleport:
                    crystalsKeysBox.isOn = PortalColorLogic.PortalN.Portals[portalName].PortalZDOs[zdo].CrystalActive;
                    break;                
                case PortalModeClass.PortalMode.OneWay:
                    crystalsKeysBox.isOn = PortalColorLogic.PortalN.Portals[portalName].PortalZDOs[zdo].CrystalActive;
                    break;
            }
            allowEverythingBox.isOn = PortalColorLogic.PortalN.Portals[portalName].TeleportAnything;
            fastTeleportBox.isOn = PortalColorLogic.PortalN.Portals[portalName].PortalZDOs[zdo].FastTeleport;
            hoverNameBox.isOn = PortalColorLogic.PortalN.Portals[portalName].PortalZDOs[zdo].ShowName;
            addBlockField.text = PortalColorLogic.PortalN.Portals[portalName].AdditionalProhibitItems != null
                ? string.Join(",", PortalColorLogic.PortalN.Portals[portalName].AdditionalProhibitItems)
                : string.Empty;

            addAllowField.text = PortalColorLogic.PortalN.Portals[portalName].AdditionalAllowItems != null
                ? string.Join(",", PortalColorLogic.PortalN.Portals[portalName].AdditionalAllowItems)
                : string.Empty;

            allowedUsersInputField.text = PortalColorLogic.PortalN.Portals[portalName].AllowedUsers != null
                ? string.Join(",", PortalColorLogic.PortalN.Portals[portalName].AllowedUsers)
                : string.Empty;

            weightField.text = PortalColorLogic.PortalN.Portals[portalName].MaxWeight.ToString();

            // Always check for blocked items, allowed items, weights, hover Portal Name, Fast Teleport, Allow Everything
        }
        private void OnModeChanged()
        {
            // Update the UI based on the selected mode
            selectedMode = (PortalModeClass.PortalMode)modeDropdown.value;
            UpdateModeDescription();
            OnCrystalKeyChange();
            UpdateUIForMode(selectedMode);
            PopulateSelected();

        }
        private void OnCrystalKeyChange()
        {
            if (selectedMode == PortalModeClass.PortalMode.CrystalKeyMode)
                crystalsKeysBox.isOn = true;

            UpdateModeDescription();
            if (crystalsKeysBox.isOn && crystalsKeysBox.IsActive())
            {
                promptText.text += $" <color={colorNameLower}> {colorName} Key or {MagicPortalFluid.ConfigCrystalsConsumable.Value} Crystal required. </color>";
            }           
            
        }

        private void UpdateModeDescription()
        {
            // Update the description text based on the selected mode
            promptText.text = GetModeDescription(selectedMode);
        }

        private void UpdateUIForMode(PortalModeClass.PortalMode selectedMode)
        {
            GuildsGame.SetActive(false);
            password.SetActive(selectedMode == PortalModeClass.PortalMode.PasswordLock || selectedMode == PortalModeClass.PortalMode.OneWayPasswordLock);
            coordinates.SetActive(selectedMode == PortalModeClass.PortalMode.CordsPortal);
            transportNet.SetActive(selectedMode == PortalModeClass.PortalMode.TransportNetwork);
            allowedUsers.SetActive(selectedMode == PortalModeClass.PortalMode.AllowedUsersOnly || selectedMode ==  PortalModeClass.PortalMode.PasswordLock || selectedMode == PortalModeClass.PortalMode.OneWayPasswordLock);
            crystalsKeysGameobject.SetActive(selectedMode == PortalModeClass.PortalMode.TargetPortal || selectedMode == PortalModeClass.PortalMode.PasswordLock 
                || selectedMode == PortalModeClass.PortalMode.OneWayPasswordLock || selectedMode == PortalModeClass.PortalMode.OneWay
                || selectedMode == PortalModeClass.PortalMode.CordsPortal || selectedMode == PortalModeClass.PortalMode.CrystalKeyMode
                || selectedMode == PortalModeClass.PortalMode.RandomTeleport );


        }

        private string GetModeDescription(PortalModeClass.PortalMode mode)
        {
            // Return a description for each mode
            return mode switch
            {
                PortalModeClass.PortalMode.Normal => "Normal:   Standard portal behavior.",
                PortalModeClass.PortalMode.TargetPortal => "Tareget Portal:   You walk into it and a map appears, you click on the portal visual to be transported. ",
                PortalModeClass.PortalMode.CrystalKeyMode => "Crystal and Key:  This is just Crystal&Key mode by itself, a lot of other modes also have crystal and key checkbox that can be enabled. ",
                PortalModeClass.PortalMode.PasswordLock => "Password Lock:  Requires a password to use the portal. Then that playername is added to allowed list.",
                PortalModeClass.PortalMode.OneWay => "One-Way:  Allows travel in one direction only. Deactives whatever portal this is connected to.",
                PortalModeClass.PortalMode.OneWayPasswordLock => "One-Way Password Lock:  Allows travel in one direction only, but only if you enter the password. Afterwords that playername is added to allow list. Deactives whatever portal this is connected to.",
                PortalModeClass.PortalMode.AllowedUsersOnly => "Allowed Users Only:  Only specified users can use the portal.",
                PortalModeClass.PortalMode.CordsPortal => "Coordinates Portal:  Transports player to specified coordinates.",
                PortalModeClass.PortalMode.TransportNetwork => "Transport Network:  Connects to a network of predefined locations. The name gets hidden, portal does not appear active. If you type name into chat/speak a correct name, you are taken there.",
                PortalModeClass.PortalMode.RandomTeleport => "Random Telport:  This will teleport a person to a random coordinate anywhere on the map, dangerous. ",
                PortalModeClass.PortalMode.AdminOnly => "Admin Only:  Only admins can use this portal. ",
                PortalModeClass.PortalMode.Rainbow => "Rainbow:  This is a standard portal, but with flashing colors.",
                _ => "Select a mode to see its description."
            };
        }

        protected override void OnSubmit(Action<PortalModeClass.PortalMode, ModeSelectionPopup> onSubmit, string color, string PortalName, string zdo)
        {
            // Get the selected portal mode from the dropdown
            PortalModeClass.PortalMode selectedMode = (PortalModeClass.PortalMode)modeDropdown.value;


            // Invoke the callback with the selected mode and any extra input
            onSubmit?.Invoke(selectedMode, _popupInstance);

            // Destroy the popup after submission
            //Destroy(popupInstance);
            //popupInstance = null;
        }
    }


    /*
     * passwordPopup.ShowPasswordPopup((password) =>
        {
    Debug.Log("Entered Password: " + password);
    // Handle password logic here
        });
    */
    public class PasswordPopup : MonoBehaviour
    {
        public GameObject popupPrefab;
        public static GameObject _popupInstance = null;
        private InputField _passwordInputField;
        private Button _submitButton;
        private Button _closeButton;

        // Method to show the password popup and handle the submission
        public void ShowPasswordPopup(Action<string> onSubmit)
        {
            if (_popupInstance != null) return; // Prevent showing multiple popups

            popupPrefab = Instantiate(MagicPortalFluid.uiasset.LoadAsset<GameObject>("RMPassPopup"));
            if (popupPrefab == null)
            {
                Debug.LogError("Failed to load popup prefab.");
                return;
            }

            Player player = Player.m_localPlayer;

            if (player != null && !InventoryGui.IsVisible())
            {
                // Force the player's inventory to open
                InventoryGui.instance.Show(null);
            }

            _passwordInputField = popupPrefab.transform.Find("Canvas/MainPanel/Panel/PasswordInputField").GetComponentInChildren<InputField>();
            _submitButton = popupPrefab.transform.Find("Canvas/MainPanel/Panel/SubmitButton").GetComponentInChildren<Button>();
            _closeButton = popupPrefab.transform.Find("Canvas/MainPanel/Close").GetComponentInChildren<Button>();

            _submitButton.onClick.AddListener(() => OnSubmit(onSubmit));
            _closeButton.onClick.AddListener(() => CloseUI());
            _popupInstance = popupPrefab;
        }

        private void OnSubmit(Action<string> onSubmit)
        {
            string password = _passwordInputField.text; // Get the entered password
            onSubmit?.Invoke(password); // Call the callback with the entered password

            // Destroy the popup after submission
            CloseUI();
        }

        private void CloseUI()
        {
            Destroy(popupPrefab);
            _popupInstance = null;
        }
    }

}
