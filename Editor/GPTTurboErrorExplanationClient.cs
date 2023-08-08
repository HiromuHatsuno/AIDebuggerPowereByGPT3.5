using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class GPTTurboErrorExplanationClient
{
    private static string APIEndPoint { get; set; }
    private static string ApiKey { get; set; }
    private static string Model { get; set; }
    private static string BackgroundInformation { get; set; }

    private static readonly List<ChatGPTMessage> _messageList = new List<ChatGPTMessage>();
    
    [InitializeOnLoadMethod]
    private static void Initialize()
    {
        LoadConfig();
        AddBackgroundInfoToMessages();
    }

    public static void SetConfig(string apiEndPoint, string apiKey, string model, string backgroundInformation)
    {
        APIEndPoint = apiEndPoint;
        ApiKey = apiKey;
        Model = model;
        BackgroundInformation = backgroundInformation;

        SaveConfig();
    }

    private static void SaveConfig()
    {
        PlayerPrefs.SetString("APIEndPoint", APIEndPoint);
        PlayerPrefs.SetString("ApiKey", ApiKey);
        PlayerPrefs.SetString("Model", Model);
        PlayerPrefs.SetString("BackgroundInformation", BackgroundInformation);
    }

    public static void LoadConfig()
    {
        APIEndPoint = PlayerPrefs.GetString("APIEndPoint");
        ApiKey = PlayerPrefs.GetString("ApiKey");
        Model = PlayerPrefs.GetString("Model");
        BackgroundInformation = PlayerPrefs.GetString("BackgroundInformation");
    }

    private static void AddBackgroundInfoToMessages()
    {
        _messageList.Add(new ChatGPTMessage { role = "system", content = BackgroundInformation });
    }

    public static async Task<string> HandleLogMessage(string logString,string errorDetail)
    {
        return await GetErrorExplanation($"{logString}エラーの詳細{errorDetail}");
    }

    private static async Task<string> GetErrorExplanation(string errorMessage)
    {
        await SendMessageToRequest(errorMessage);
        return GetLastResponseMessage();
    }

    private static async Task SendMessageToRequest(string userMessage)
    {
        AddUserMessageToMessages(userMessage);
        var requestData = CreateRequestData();
        await SendAPIRequest(JsonConvert.SerializeObject(requestData));
    }

    private static void AddUserMessageToMessages(string userMessage)
    {
        _messageList.Add(new ChatGPTMessage { role = "user", content = userMessage });
    }

    private static object CreateRequestData()
    {
        return new
        {
            model = Model,
            messages = _messageList
        };
    }

    private static async Task SendAPIRequest(string jsonData)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Authorization", "Bearer " + ApiKey);

        var httpContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(APIEndPoint, httpContent);

        if (response.IsSuccessStatusCode)
        {
            await HandleAPIResponse(await response.Content.ReadAsStringAsync());
        }
        else
        {
            Console.WriteLine($"Error: {response.StatusCode}");
        }
    }

    private static async Task HandleAPIResponse(string responseText)
    {
        var responseObject = JsonConvert.DeserializeObject<ChatGPTResponse>(responseText);
        _messageList.Add(responseObject.choices[0].message);
    }

    private static string GetLastResponseMessage()
    {
        return _messageList[_messageList.Count - 1].content;
    }

    public class ChatGPTMessage
    {
        [JsonProperty("role")] public string role;
        [JsonProperty("content")] public string content;
    }

    public class ChatGPTResponse
    {
        [JsonProperty("choices")] public Choice[] choices;

        public class Choice
        {
            public ChatGPTMessage message;
        }
    }
}
