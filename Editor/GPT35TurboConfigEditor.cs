using UnityEditor;
using UnityEngine;

public class GPT35TurboConfigEditor : EditorWindow
{
    private const string WindowTitle = "GPT-3.5 Turbo Config";
    private const string ConfigurationLabel = "GPT-3.5 Turbo Configurations";
    private const string SaveButtonLabel = "Save Configurations";

    private string apiEndPoint = string.Empty;
    private string apiKey = string.Empty;
    private string model = string.Empty;
    private string backgroundInformation = string.Empty;

    [MenuItem("Window/AIDebugger/" + WindowTitle)]
    public static void ShowWindow()
    {
        GetWindow<GPT35TurboConfigEditor>(WindowTitle);
    }

    private void OnGUI()
    {
        DisplayConfigurationTitle();
        GatherInputInformation();
        ProcessSaveConfiguration();
    }

    private void DisplayConfigurationTitle()
    {
        GUILayout.Label(ConfigurationLabel, EditorStyles.boldLabel);
    }

    private void GatherInputInformation()
    {
        apiEndPoint = EditorGUILayout.TextField("API End Point", apiEndPoint);
        apiKey = EditorGUILayout.TextField("API Key", apiKey);
        model = EditorGUILayout.TextField("Model", model);
        backgroundInformation = EditorGUILayout.TextField("Background Information", backgroundInformation);
    }

    private void ProcessSaveConfiguration()
    {
        if (!GUILayout.Button(SaveButtonLabel)) return;
        GPTTurboErrorExplanationClient.SetConfig(apiEndPoint, apiKey, model, backgroundInformation);
    }
}