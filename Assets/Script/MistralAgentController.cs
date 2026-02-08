using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;

public class MistralAgentController : MonoBehaviour
{
    [Header("Mistral 设置")]
    [Tooltip("在这里填入你的 API Key (以 op_ 开头，不是 ag_ 开头那个)")]
    public string apiKey = "在此处粘贴你的API_KEY"; // 【注意】这里还是要填你的密钥

    [Tooltip("你的 Agent ID (已自动填入)")]
    // ↓↓↓↓↓↓↓↓↓ 这里我已经帮你改好了 ↓↓↓↓↓↓↓↓↓
    public string agentId = "ag_019c3ace0e4277ab9da092430fb3cd45";
    // ↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑↑

    [Header("UI 组件")]
    public InputField playerInput;   // 玩家输入框
    public Button sendButton;        // 发送按钮
    public Text chatDisplay;         // 显示聊天内容的文本

    // Mistral API 地址
    private string apiUrl = "https://api.mistral.ai/v1/chat/completions";

    private List<Message> conversationHistory = new List<Message>();

    void Start()
    {
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnSendClicked);
        }
    }

    void OnSendClicked()
    {
        if (playerInput != null && !string.IsNullOrEmpty(playerInput.text))
        {
            string userText = playerInput.text;
            AppendToChat("你: " + userText);
            StartCoroutine(PostRequest(userText));
            playerInput.text = "";
        }
    }

    IEnumerator PostRequest(string userMessage)
    {
        // 1. 构建数据
        conversationHistory.Add(new Message { role = "user", content = userMessage });

        MistralRequest requestData = new MistralRequest
        {
            model = agentId, // 这里会自动使用上面定义的 ag_019c...
            messages = conversationHistory,
            stream = false
        };

        string jsonBody = JsonConvert.SerializeObject(requestData);
        byte[] rawBody = Encoding.UTF8.GetBytes(jsonBody);

        // 2. 发送请求
        using (UnityWebRequest request = new UnityWebRequest(apiUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(rawBody);
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", "Bearer " + apiKey);

            yield return request.SendWebRequest();

            // 3. 处理结果
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("API Error: " + request.error + "\n" + request.downloadHandler.text);
                AppendToChat("系统: 连接失败 (请检查 API Key 是否正确)");
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                MistralResponse response = JsonConvert.DeserializeObject<MistralResponse>(jsonResponse);

                if (response.choices != null && response.choices.Count > 0)
                {
                    string aiText = response.choices[0].message.content;
                    conversationHistory.Add(new Message { role = "assistant", content = aiText });
                    AppendToChat("Agent: " + aiText);
                }
            }
        }
    }

    void AppendToChat(string text)
    {
        if (chatDisplay != null)
        {
            chatDisplay.text += "\n" + text;
        }
        Debug.Log(text);
    }

    // --- JSON 数据结构 ---
    [System.Serializable]
    public class MistralRequest
    {
        public string model;
        public List<Message> messages;
        public bool stream;
    }

    [System.Serializable]
    public class Message
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    public class MistralResponse
    {
        public List<Choice> choices;
    }

    [System.Serializable]
    public class Choice
    {
        public Message message;
    }
}