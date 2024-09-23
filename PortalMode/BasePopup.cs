using RareMagicPortal;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static RareMagicPortal.PortalColorLogic;

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
        public static GameObject _popupInstance = null;


        // Update method signature to match the extended use case
        public void ShowPopup(Action<PortalModeClass.PortalMode, string> onSubmit, string color)
        {
            popupInstance = Instantiate(MagicPortalFluid.uiasset.LoadAsset<GameObject>("RMPUIpop"));

            _popupInstance = popupInstance;

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
            submitButton.onClick.AddListener(() => OnSubmit(onSubmit, color));
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
            Destroy(popupInstance);
            popupInstance = null;
        }

        protected virtual void OnSubmit(Action<PortalModeClass.PortalMode, string> onSubmit, string color)
        {
            // Get the selected portal mode from the dropdown
            int selectedIndex = modeDropdown.value;
            PortalModeClass.PortalMode selectedMode = (PortalModeClass.PortalMode)selectedIndex;

            // Invoke the callback with the selected mode and an empty string for extra input
            onSubmit?.Invoke(selectedMode, string.Empty);

            // Destroy the popup after submission
            Destroy(popupInstance);
            popupInstance = null;
        }
    }
    public class ModeSelectionPopup : BasePopup
    {
        private GameObject password;
        private InputField passwordInputField;
        private GameObject coordinates;
        private InputField coordinatesInputField;
        private GameObject transportNet;
        private InputField transportNetInputField;
        private GameObject allowedUsers;
        private InputField allowedUsersInputField;
        private InputField addBlockField;
        private InputField addAllowField;
        private InputField weightField;

        private Toggle allowEverythingBox;
        private Toggle hoverNameBox;
        private Toggle crystalsKeysBox;
        private GameObject crystalsKeysGameobject;
        private Toggle fastTeleportBox;
        public Image crystalsKeysBackgroundImage;

        private Color currentcolor;
        private string colorName;
        private string colorNameLower;


        public void ShowModeSelectionPopup(Action<PortalModeClass.PortalMode, string> onSubmit, string color)
        {
            ShowPopup(onSubmit, color); // Call the base ShowPopup method with the correct signature

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


            currentcolor = PortalColorLogic.PortalColors[color].HexName;
            crystalsKeysBackgroundImage.color = currentcolor;
            colorName = color;
            colorNameLower = color.ToLower();



            // Update description for the default selection
            UpdateModeDescription();

            // Override the submit button listener to handle the extended callback
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(() => OnSubmit(onSubmit, color));
        }

        private void OnModeChanged()
        {
            // Update the UI based on the selected mode
            PortalModeClass.PortalMode selectedMode = (PortalModeClass.PortalMode)modeDropdown.value;
            UpdateModeDescription();
            OnCrystalKeyChange();
            UpdateUIForMode(selectedMode);

        }
        private void OnCrystalKeyChange()
        {
            PortalModeClass.PortalMode selectedMode = (PortalModeClass.PortalMode)modeDropdown.value;
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
            PortalModeClass.PortalMode selectedMode = (PortalModeClass.PortalMode)modeDropdown.value;
            promptText.text = GetModeDescription(selectedMode);
        }

        private void UpdateUIForMode(PortalModeClass.PortalMode selectedMode)
        {

            password.SetActive(selectedMode == PortalModeClass.PortalMode.PasswordLock || selectedMode == PortalModeClass.PortalMode.OneWayPasswordLock);
            coordinates.SetActive(selectedMode == PortalModeClass.PortalMode.CordsPortal);
            transportNet.SetActive(selectedMode == PortalModeClass.PortalMode.TransportNetwork);
            allowedUsers.SetActive(selectedMode == PortalModeClass.PortalMode.AllowedUsersOnly);
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

        protected override void OnSubmit(Action<PortalModeClass.PortalMode, string> onSubmit, string color)
        {
            // Get the selected portal mode from the dropdown
            PortalModeClass.PortalMode selectedMode = (PortalModeClass.PortalMode)modeDropdown.value;
            string extraInput = "";

            // Collect additional input if needed
            if (selectedMode == PortalModeClass.PortalMode.PasswordLock)
            {
                extraInput = passwordInputField.text;
            }
            else if (selectedMode == PortalModeClass.PortalMode.CordsPortal)
            {
                extraInput = coordinatesInputField.text;
            }

            // Invoke the callback with the selected mode and any extra input
            onSubmit?.Invoke(selectedMode, extraInput);

            // Destroy the popup after submission
            Destroy(popupInstance);
            popupInstance = null;
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
        private static GameObject _popupInstance = null;
        private InputField _passwordInputField;
        private Button _submitButton;
        private Button _closeButton;

        // Method to show the password popup and handle the submission
        public void ShowPasswordPopup(Action<string> onSubmit)
        {        
            if (_popupInstance != null) return; // Prevent showing multiple popups

            // Instantiate the popup
            popupPrefab = Instantiate(MagicPortalFluid.uiasset.LoadAsset<GameObject>("RMPassPopup"));

            // Find the InputField and Button components in the popup
            _passwordInputField = popupPrefab.transform.Find("Canvas/MainPanel/Panel/PasswordInputField").GetComponentInChildren<InputField>();
            _submitButton = popupPrefab.transform.Find("Canvas/MainPanel/Panel/SubmitButton").GetComponentInChildren<Button>();
            _closeButton = popupPrefab.transform.Find("Canvas/MainPanel/Close").GetComponentInChildren<Button>();

            // Add listener to the submit button
            _submitButton.onClick.AddListener(() => OnSubmit(onSubmit));
            _closeButton.onClick.AddListener(() => CloseUI());
            _popupInstance = popupPrefab;
        }

        // Handle the submission of the password
        private void OnSubmit(Action<string> onSubmit)
        {
            string password = _passwordInputField.text; // Get the entered password
            onSubmit?.Invoke(password); // Call the callback with the entered password

            // Destroy the popup after submission
            Destroy(popupPrefab);
            popupPrefab = null; // Reset the instance reference
        }

        private void CloseUI()
        {
            Destroy(popupPrefab);
            popupPrefab = null;
        }
    }
}
