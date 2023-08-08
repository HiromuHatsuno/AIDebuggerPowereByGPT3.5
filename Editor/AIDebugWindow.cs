using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AIDebugWindow : EditorWindow
{
    private static List<string> errorMessages;
    private static List<string> errorMessagesDetail;
    private string selectedErrorMessage = string.Empty;
    private string explanation = string.Empty;
    private Vector2 scrollPosition;

    private GUIStyle selectedButtonStyle;
    private GUIStyle buttonStyle;
    private GUIStyle textAreaStyle;

    private const int ButtonHeight = 50;
    private const int FontSize = 14;

    [MenuItem("Window/AIDebugger/AIDebug Helper")]
    public static void ShowWindow()
    {
        GetWindow<AIDebugWindow>("AIDebug Helper");
    }

    [InitializeOnLoadMethod]
    public static void Initialize()
    {
        errorMessages = new List<string>();
        errorMessagesDetail = new List<string>();
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private static void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (IsNotErrorOrException(type) || ErrorMessageAlreadyExists(logString)) return;
        AddErrorMessage(logString, stackTrace);
    }

    private static bool IsNotErrorOrException(LogType type)
    {
        return type != LogType.Exception && type != LogType.Error;
    }

    private static bool ErrorMessageAlreadyExists(string logString)
    {
        return errorMessages.Contains(logString);
    }

    private static void AddErrorMessage(string logString, string stackTrace)
    {
        errorMessages.Add(logString);
        errorMessagesDetail.Add(stackTrace);
    }

    private void OnGUI()
    {
        InitializeStyles();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        for (var i = 0; i < errorMessages.Count; i++)
        {
            HandleErrorMessageSelection(i);
        }

        EditorGUILayout.EndScrollView();

        if (string.IsNullOrEmpty(selectedErrorMessage)) return;
        DisplayExplanationText();
    }

    private void InitializeStyles()
    {
        selectedButtonStyle = CreateButtonStyle(Color.white, CreateTexture2D(Color.cyan));
        buttonStyle = CreateButtonStyle(GUI.skin.button.normal.textColor, GUI.skin.button.normal.background);
        textAreaStyle = CreateTextAreaStyle();
    }

    private GUIStyle CreateButtonStyle(Color textColor, Texture2D background)
    {
        return new GUIStyle(GUI.skin.button)
        {
            normal =
            {
                textColor = textColor,
                background = background
            },
            fontSize = FontSize
        };
    }

    private Texture2D CreateTexture2D(Color color, int width = 2, int height = 2)
    {
        Texture2D texture2D = new Texture2D(width, height);
        for (int y = 0; y < texture2D.height; y++)
        {
            for (int x = 0; x < texture2D.width; x++)
            {
                texture2D.SetPixel(x, y, color);
            }
        }

        texture2D.Apply();
        return texture2D;
    }

    private GUIStyle CreateTextAreaStyle()
    {
        return new GUIStyle(GUI.skin.textArea)
        {
            fontSize = FontSize,
            fontStyle = FontStyle.Bold
        };
    }

    private void HandleErrorMessageSelection(int i)
    {
        var isSelected = selectedErrorMessage == errorMessages[i];
        var style = isSelected ? selectedButtonStyle : buttonStyle;

        if (GUILayout.Button(errorMessages[i], style, GUILayout.Height(ButtonHeight)))
        {
            ToggleErrorMessageSelection(isSelected, i);
        }
    }

    private void ToggleErrorMessageSelection(bool isSelected, int i)
    {
        selectedErrorMessage = isSelected ? string.Empty : errorMessages[i];
        explanation = string.Empty;
        Repaint();
        if (!isSelected)
        {
            GetErrorExplanation(errorMessages[i], errorMessagesDetail[i]);
        }
    }

    private void DisplayExplanationText()
    {
        EditorGUILayout.LabelField($"Explanation for: {selectedErrorMessage}", EditorStyles.boldLabel);
        explanation = EditorGUILayout.TextArea(explanation, textAreaStyle, GUILayout.ExpandHeight(true));
    }

    private async void GetErrorExplanation(string errorMessage, string errorDetail)
    {
        explanation = await GPTTurboErrorExplanationClient.HandleLogMessage(errorMessage, errorDetail);
        Repaint();
    }
}