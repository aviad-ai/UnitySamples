using System.Threading.Tasks;
using UnityEngine;
using Aviad;

public class PluginTest : MonoBehaviour
{
    private AviadDialogue dialogue = new AviadDialogue();

    private bool isConversationStarted = false;
    private bool isSending = false;

    private string userInput = "";
    private string assistantOutput = "";
    private string conversation = "";
    private Vector2 scrollPosition = Vector2.zero;

    private async void Start()
    {
        try
        {
            await dialogue.InitializeAsync();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[PluginTest] Init failed: " + ex.Message);
        }
    }

    private void OnGUI()
    {
        const int padding = 10;
        const int buttonWidth = 100;
        const int textAreaHeight = 150;
        const int inputHeight = 30;

        GUILayout.BeginArea(new Rect(padding, padding, Screen.width - 2 * padding, Screen.height - 2 * padding));

        GUILayout.Label("Aviad Plugin Test");

        if (!isConversationStarted)
        {
            if (GUILayout.Button("Start", GUILayout.Width(buttonWidth)) && !isSending)
            {
                if (dialogue.IsInitialized)
                {
                    _ = StartConversation();
                }
            }
        }
        else
        {
            GUILayout.Label("Conversation:");
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(textAreaHeight));
            GUILayout.Label(conversation);
            GUILayout.EndScrollView();

            GUILayout.Label("Your Input:");
            userInput = GUILayout.TextField(userInput, GUILayout.Height(inputHeight));

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Send", GUILayout.Width(buttonWidth)) && !isSending)
            {
                _ = SendUserMessage();
            }
            if (GUILayout.Button("Reset", GUILayout.Width(buttonWidth)) && !isSending)
            {
                ResetConversation();
            }
            GUILayout.EndHorizontal();
        }

        GUILayout.EndArea();
    }

    private void ResetConversation()
    {
        dialogue.Reset();
        isConversationStarted = false;
        userInput = "";
        assistantOutput = "";
        conversation = "";
    }

    private async Task StartConversation()
    {
        isSending = true;
        assistantOutput = $"{dialogue.CharacterName}: ";
        isConversationStarted = true;

        try
        {
            await dialogue.StartConversation(UpdateAssistantResponse);
        }
        catch (System.Exception ex)
        {
            assistantOutput = "Error: " + ex.Message;
        }

        isSending = false;
    }

    private async Task SendUserMessage()
    {
        isSending = true;

        try
        {
            if (!string.IsNullOrWhiteSpace(userInput))
            {
                string userMessage = userInput;
                assistantOutput = $"{dialogue.CharacterName}: ";
				userInput = "";
                await dialogue.Say(userMessage, UpdateAssistantResponse);
            }
        }
        catch (System.Exception ex)
        {
            assistantOutput = "Error: " + ex.Message;
        }

        isSending = false;
    }

    private void UpdateAssistantResponse(string partialResponse)
    {
        assistantOutput = $"{dialogue.CharacterName}: {partialResponse}";
        UpdateConversationDisplay();
    }

    private void UpdateConversationDisplay()
    {
        // Always include all user and assistant messages including the current assistantOutput
        conversation = "";

        for (int i = 2; i < dialogue.Contents.Count; i++)
        {
            conversation += $"{dialogue.Contents[i]}\n";
        }

        if (!conversation.EndsWith(assistantOutput + "\n"))
        {
            conversation += assistantOutput + "\n";
        }
        scrollPosition.y = float.MaxValue;
    }

    private async void OnDestroy()
    {
        await dialogue.Shutdown();
    }
}
