using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace RareMagicPortal_3_Plus.PortalMode

{ 
    public class PasswordPopup : MonoBehaviour
    {
        public GameObject popupPrefab; // Assign a prefab with the UI elements in the Inspector

        private GameObject popupInstance;
        private InputField passwordInputField;
        private Button submitButton;

        public void ShowPasswordPopup(System.Action<string> onSubmit)
        {
            if (popupInstance != null) return;

            // Instantiate the popup
            popupInstance = Instantiate(popupPrefab, transform);

            // Find the InputField and Button components
            passwordInputField = popupInstance.transform.Find("PasswordInputField").GetComponent<InputField>();
            submitButton = popupInstance.transform.Find("SubmitButton").GetComponent<Button>();

            // Add listener to the submit button
            submitButton.onClick.AddListener(() => OnSubmit(onSubmit));
        }

        private void OnSubmit(System.Action<string> onSubmit)
        {
            string password = passwordInputField.text;
            onSubmit?.Invoke(password);

            // Destroy the popup after submission
            Destroy(popupInstance);
        }
    }


public class InputPopup : MonoBehaviour
{
    public GameObject popupPrefab; // Assign a prefab with the UI elements in the Inspector

    private GameObject popupInstance;
    private InputField inputField;
    private Button submitButton;

    public void ShowInputPopup(string prompt, System.Action<string> onSubmit)
    {
        if (popupInstance != null) return;

        // Instantiate the popup
        popupInstance = Instantiate(popupPrefab, transform);

        // Find the Text, InputField, and Button components
        Text promptText = popupInstance.transform.Find("PromptText").GetComponent<Text>();
        inputField = popupInstance.transform.Find("InputField").GetComponent<InputField>();
        submitButton = popupInstance.transform.Find("SubmitButton").GetComponent<Button>();

        // Set the prompt text
        promptText.text = prompt;

        // Add listener to the submit button
        submitButton.onClick.AddListener(() => OnSubmit(onSubmit));
    }

    private void OnSubmit(System.Action<string> onSubmit)
    {
        string input = inputField.text;
        onSubmit?.Invoke(input);

        // Destroy the popup after submission
        Destroy(popupInstance);
    }
}
}

