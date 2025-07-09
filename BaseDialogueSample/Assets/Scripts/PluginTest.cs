using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Aviad;

public class PluginTest : MonoBehaviour
{
    public AviadRunner runner;

    [Header("Dialogue Configuration")]
    public List<string> initialRoles = new List<string>();

    [TextArea(3, 10)]
    public List<string> initialContents = new List<string>();
    public string characterName = "Character";

    [Header("UI Elements")]
    public TMP_Text conversationText;
    public TMP_InputField userInputField;
    public Button sendButton;
    public Button resetButton;
    public Button startButton;

    private bool isConversationStarted = false;
    private bool isSending = false;

    private string assistantOutput = "";
    private string conversation = "";
    private List<string> additionalRoles = new List<string>();
    private List<string> additionalContents = new List<string>();

    private bool needsUIUpdate = false;


    private void Start()
    {
        startButton.onClick.AddListener(() =>
        {
            if (!isSending && runner.IsAvailable)
            {
                StartConversation();
            }
        });

        sendButton.onClick.AddListener(() => {
            if (!isSending)
            {
                SendUserMessage();
            }
        });

        userInputField.onEndEdit.AddListener((string text) => {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                if (!isSending)
                {
                    SendUserMessage();
                }
            }
        });

        resetButton.onClick.AddListener(() => {
            if (!isSending)
            {
                ResetConversation();
            }
        });

        UpdateObjectStates();
    }

    private void UpdateObjectStates()
    {
        startButton.gameObject.SetActive(!isConversationStarted);
        sendButton.gameObject.SetActive(isConversationStarted);
        resetButton.gameObject.SetActive(isConversationStarted);
        userInputField.gameObject.SetActive(isConversationStarted);
        conversationText.gameObject.SetActive(isConversationStarted);
    }

    private void ResetConversation()
    {
        runner.Reset();
        isConversationStarted = false;
        assistantOutput = "";
        conversation = "";
        additionalRoles.Clear();
        additionalContents.Clear();
        UpdateObjectStates();
    }

    private void StartConversation()
    {
        int count = Mathf.Min(initialRoles.Count, initialContents.Count);
        for (int i = 0; i < count; i++)
        {
            runner.AddTurnToContext(initialRoles[i], initialContents[i]);
        }
        try
        {
            isSending = true;
            isConversationStarted = true;
            UpdateObjectStates();
            UpdateConversationDisplay();
            assistantOutput = $"{characterName}: ";
            runner.Generate(UpdateAssistantResponse, CompleteTurn);
        }
        catch (System.Exception ex)
        {
            assistantOutput = "Error: " + ex.Message;
        }
    }

    private void SendUserMessage()
    {
        try
        {
            string userMessage = userInputField.text;
            if (!string.IsNullOrWhiteSpace(userMessage))
            {
                userInputField.text = "";
                additionalRoles.Add("users");
                additionalContents.Add($"User: {userMessage}");
                runner.AddTurnToContext("user", userMessage);
                isSending = true;
                assistantOutput = $"{characterName}: ";
                runner.Generate(UpdateAssistantResponse, CompleteTurn);
            }
        }
        catch (System.Exception ex)
        {
            assistantOutput = "Error: " + ex.Message;
        }
    }

    private void UpdateAssistantResponse(string partialResponse)
    {
        assistantOutput += partialResponse;
        UpdateConversationDisplay();
#if UNITY_WEBGL && !UNITY_EDITOR
        Update();
#endif
    }

    private void CompleteTurn(bool response)
    {
        additionalRoles.Add("assistant");
        additionalContents.Add(assistantOutput);
        assistantOutput = "";
        UpdateConversationDisplay();
        isSending = false;
    }

    private void UpdateConversationDisplay()
    {
        // Always include all user and assistant messages including the current assistantOutput
        conversation = "";
        int initialCount = Mathf.Min(initialRoles.Count, initialContents.Count);
        for (int i = 0; i < initialCount; i++)
        {
            conversation += $"{initialContents[i]}\n";
        }
        int additionalCount = Mathf.Min(additionalRoles.Count, additionalContents.Count);
        for (int i = 0; i < additionalCount; i++)
        {
            conversation += $"{additionalContents[i]}\n";
        }
        conversation += assistantOutput;
        needsUIUpdate = true;
    }

    private void Update()
    {
        if (needsUIUpdate)
        {
            conversationText.text = conversation;
            var scrollRect = conversationText.GetComponentInParent<ScrollRect>();
            if (scrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                scrollRect.verticalNormalizedPosition = 0f;
            }
            needsUIUpdate = false;
        }
    }
}