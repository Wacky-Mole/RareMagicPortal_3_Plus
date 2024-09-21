using RareMagicPortal;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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


        // Update method signature to match the extended use case
        public void ShowPopup(Action<PortalModeClass.PortalMode, string> onSubmit)
        {
            popupInstance = Instantiate(MagicPortalFluid.uiasset.LoadAsset<GameObject>("RMPUIpop"));

            // Find the Dropdown and Button components
            Panel = popupInstance.transform.Find("Canvas/MainPanel/Panel").gameObject;
            Lists = Panel.transform.Find("Lists").gameObject;
            modeDropdown = Lists.transform.Find("PortalMode").GetComponent<Dropdown>();

            submitButton = Panel.transform.Find("SubmitButton").GetComponent<Button>();
            promptText = Panel.transform.Find("ModeDescriptionText")?.GetComponent<Text>();

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

            // Add listener to the submit button
            submitButton.onClick.AddListener(() => OnSubmit(onSubmit));
        }

        private void PopulateDropdown()
        {
            // Get all portal modes and add them to the dropdown
            List<string> modeNames = new List<string>(Enum.GetNames(typeof(PortalModeClass.PortalMode)));
            modeDropdown.ClearOptions();
            modeDropdown.AddOptions(modeNames);
        }

        protected virtual void OnSubmit(Action<PortalModeClass.PortalMode, string> onSubmit)
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
        private InputField passwordInputField;
        private InputField coordinatesInputField;
        private InputField transportNetInputField;

        public void ShowModeSelectionPopup(Action<PortalModeClass.PortalMode, string> onSubmit)
        {
            ShowPopup(onSubmit); // Call the base ShowPopup method with the correct signature

            // Find the UI components specific to ModeSelectionPopup
            passwordInputField = Lists.transform.Find("PasswordInputField")?.GetComponent<InputField>();
            coordinatesInputField = Lists.transform.Find("CoordinatesInputField")?.GetComponent<InputField>();
            transportNetInputField = Lists.transform.Find("TransportNet")?.GetComponent<InputField>();

            if (passwordInputField == null || coordinatesInputField == null || transportNetInputField == null)
            {
                Debug.LogError("Popup components not found. Please ensure the prefab has all necessary components.");
                return;
            }

            // Add listener to the dropdown to update UI based on selection
            modeDropdown.onValueChanged.AddListener(delegate { OnModeChanged(); });

            // Initially hide extra input fields
            passwordInputField.gameObject.SetActive(false);
            coordinatesInputField.gameObject.SetActive(false);

            // Update description for the default selection
            UpdateModeDescription();

            // Override the submit button listener to handle the extended callback
            submitButton.onClick.RemoveAllListeners();
            submitButton.onClick.AddListener(() => OnSubmit(onSubmit));
        }

        private void OnModeChanged()
        {
            // Update the UI based on the selected mode
            PortalModeClass.PortalMode selectedMode = (PortalModeClass.PortalMode)modeDropdown.value;
            UpdateModeDescription();
            UpdateUIForMode(selectedMode);
        }

        private void UpdateModeDescription()
        {
            // Update the description text based on the selected mode
            PortalModeClass.PortalMode selectedMode = (PortalModeClass.PortalMode)modeDropdown.value;
            //modeDescriptionText.text = GetModeDescription(selectedMode);
        }

        private void UpdateUIForMode(PortalModeClass.PortalMode selectedMode)
        {
            // Show or hide additional input fields based on the selected mode
            passwordInputField.gameObject.SetActive(selectedMode == PortalModeClass.PortalMode.PasswordLock);
            coordinatesInputField.gameObject.SetActive(selectedMode == PortalModeClass.PortalMode.CordsPortal);
        }

        private string GetModeDescription(PortalModeClass.PortalMode mode)
        {
            // Return a description for each mode
            return mode switch
            {
                PortalModeClass.PortalMode.Normal => "Normal: Standard portal behavior.",
                PortalModeClass.PortalMode.PasswordLock => "Password Lock: Requires a password to use the portal.",
                PortalModeClass.PortalMode.OneWay => "One-Way: Allows travel in one direction only.",
                PortalModeClass.PortalMode.AllowedUsersOnly => "Allowed Users Only: Only specified users can use the portal.",
                PortalModeClass.PortalMode.CordsPortal => "Coordinates Portal: Transports to specified coordinates.",
                PortalModeClass.PortalMode.TransportNetwork => "Transport Network: Connects to a network of predefined locations.",
                _ => "Select a mode to see its description."
            };
        }

        protected override void OnSubmit(Action<PortalModeClass.PortalMode, string> onSubmit)
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
}
