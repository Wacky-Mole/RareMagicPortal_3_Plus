using HarmonyLib;
using RareMagicPortal;
using RareMagicPortal_3_Plus.Patches;
using RareMagicPortal_3_Plus.PortalMode;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Speech.Recognition;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using YamlDotNet.Serialization;

/*
public class MicListener : MonoBehaviour
{
    private SpeechRecognitionEngine recognizer;
    private bool isListening = false;
    private string[] availablePortals = { };

    private Player player;
    private TeleportWorld currentPortal;
    private float listeningRadius = 5f;

    private void Start()
    {
        player = Player.m_localPlayer;
        availablePortals = GetAvailablePortals();

        SetupSpeechRecognition();
    }

    private void SetupSpeechRecognition()
    {
        recognizer = new SpeechRecognitionEngine();
        recognizer.SetInputToDefaultAudioDevice();

        // Add grammar rules for recognizing "Teleport" followed by a destination
        Choices portalChoices = new Choices(availablePortals);
        GrammarBuilder grammarBuilder = new GrammarBuilder("Teleport");
        grammarBuilder.Append(portalChoices);
        Grammar grammar = new Grammar(grammarBuilder);

        recognizer.LoadGrammar(grammar);
        recognizer.SpeechRecognized += OnSpeechRecognized;
        recognizer.RecognizeAsync(RecognizeMode.Multiple);
    }

    public static string[] GetAvailablePortals()
    {
        if (PortalColorLogic.PortalN?.Portals == null)
        {
            return Array.Empty<string>(); // Return an empty array if Portals is null
        }

        // Extract the keys (portal names) from the dictionary into a string array
        string[] availablePortals = new string[PortalColorLogic.PortalN.Portals.Count];
        PortalColorLogic.PortalN.Portals.Keys.CopyTo(availablePortals, 0);
        return availablePortals;
    }

    private void OnSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
    {
        if (!isListening) return;

        Vector3 playerPosition = Player.m_localPlayer.transform.position;
        Vector3 portalPosition = TeleportWorldPatchs.LastPortalTrigger;
        float distance = Vector3.Distance(playerPosition, portalPosition);
        float someThreshold = listeningRadius;

        if (distance < someThreshold)
        {
            string[] words = e.Result.Text.Split(' ');
            if (words.Length == 2 && words[0].ToLower() == "teleport")
            {
                string destination = words[1];
                Debug.LogWarning($"Voice command recognized: {e.Result.Text}");
                destination  = destination.ToLower();

                if (PortalColorLogic.PortalN.Portals.ContainsKey(destination))
                {                 
                    MagicPortalFluid.RareMagicPortal.LogInfo("Player IS close enough to Tele Network");

                    var target = PortalColorLogic.PortalN.Portals[destination];
                    string cords = "";
                    foreach (var zd in target.PortalZDOs)
                    {
                        if (zd.Value.Active)
                            cords = zd.Value.Coords;
                    }

                    if (PortalModeClass.TryParseCoordinates(cords, out Vector3 targetCoords))
                    {
                        MagicPortalFluid.RareMagicPortal.LogInfo("Teleporting with Warp");
                        TeleportWorldPatchs.PerformTeleport(targetCoords);
                    }
                }
            }
        }
    }



    public void StartListening()
    {
        isListening = true;
        Debug.Log("Started listening for voice commands...");
    }

    public void StopListening()
    {
        isListening = false;
        Debug.Log("Stopped listening for voice commands.");
    }

    public void OnDestroy()
    {
        if (recognizer != null)
        {
            recognizer.SpeechRecognized -= OnSpeechRecognized;
            recognizer.Dispose();
        }
    }
}

*/

public class ContinuousWhisperRecognition : MonoBehaviour
{
    [Header("Whisper Settings")]
    public string openAiApiKey = "YOUR_API_KEY";
    private string whisperApiUrl = "https://api.openai.com/v1/audio/transcriptions";

    [Header("Microphone Settings")]
    public int sampleRate = 16000;
    private AudioClip recordingClip;
    private bool isRecording = false;
    private const int clipLength = 3; // Duration of each audio clip in seconds

    private void Start()
    {
        if (Microphone.devices.Length > 0)
        {
            StartCoroutine(ContinuousRecording());
        }
        else
        {
            Debug.LogError("No microphone devices found.");
        }
    }

    private IEnumerator ContinuousRecording()
    {
        while (true)
        {
            // Start recording a short clip
            recordingClip = Microphone.Start(null, false, clipLength, sampleRate);
            isRecording = true;
            Debug.Log("Recording started...");

            // Wait for the clip to finish recording
            yield return new WaitForSeconds(clipLength);

            // Stop recording and process the clip
            Microphone.End(null);
            isRecording = false;
            Debug.Log("Recording stopped.");

            // Convert AudioClip to WAV data
            byte[] wavData = AudioClipToWav(recordingClip);
            StartCoroutine(SendToWhisperApi(wavData));
        }
    }

    private IEnumerator SendToWhisperApi(byte[] wavData)
    {
        if (wavData == null || wavData.Length == 0) yield break;

        WWWForm form = new WWWForm();
        form.AddBinaryData("file", wavData, "audio.wav", "audio/wav");
        form.AddField("model", "whisper-1");

        UnityWebRequest request = UnityWebRequest.Post(whisperApiUrl, form);
        request.SetRequestHeader("Authorization", $"Bearer {openAiApiKey}");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            var responseText = request.downloadHandler.text;
            Debug.Log($"Response: {responseText}");
            HandleWhisperResponse(responseText);
        }
        else
        {
            Debug.LogError($"Error: {request.error}");
        }
    }

    private void HandleWhisperResponse(string jsonResponse)
    {

        var deserializer = new DeserializerBuilder()
            .Build();

        var response = deserializer.Deserialize<WhisperResponse>(jsonResponse);
        if (response != null && !string.IsNullOrEmpty(response.text))
        {
            Debug.Log($"Transcription: {response.text}");
            ProcessVoiceCommand(response.text);
        }
    }

    private void ProcessVoiceCommand(string command)
    {
        command = command.ToLower();
        if (command.StartsWith("teleport"))
        {
            string[] words = command.Split(' ');
            if (words.Length > 1)
            {
                string destination = words[1];
                TeleportPlayer(destination);
            }
        }
    }

    private void TeleportPlayer(string destination)
    {
        Debug.Log($"Teleporting to {destination}");
        // Implement your teleportation logic here
    }

    private byte[] AudioClipToWav(AudioClip clip)
    {
        if (clip == null) return null;

        float[] samples = new float[clip.samples];
        clip.GetData(samples, 0);
        return ConvertAudioClipToWav(samples, clip.channels, clip.frequency);
    }

    private byte[] ConvertAudioClipToWav(float[] samples, int channels, int sampleRate)
    {
        // WAV conversion logic here
        // This will convert the float array into a WAV file format
        return new byte[0]; // Replace this with actual conversion code
    }

    [Serializable]
    private class WhisperResponse
    {
        public string text;
    }
}
