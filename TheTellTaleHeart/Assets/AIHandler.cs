using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using Aviad;
using System.Linq;

namespace ai.aviad.AIBook
{
    public class AIHandler
    {
        private GenerationState generationState;

        private readonly AviadRunner runner;
        private readonly GameState gameState;
        private readonly AudioManager audioManager;

        public bool HasChoice => generationState.hasChoice;
        public bool Done => generationState.complete;

        public AIHandler(AviadRunner runner, GameState gameState, AudioManager audioManager)
        {
            this.runner = runner;
            this.gameState = gameState;
            this.audioManager = audioManager;
        }

        /// <summary>
        /// Starts a new AI generation request.
        /// </summary>
        public async Task TriggerGeneration()
        {
            try
            {
                Character currentChar = gameState.GetCurrentCharacter();
                Scenario currentScenario = gameState.GetCurrentScenario();

                if (currentChar == null || currentScenario == null)
                {
                    Debug.LogError("Missing character or scenario data");
                    return;
                }

                string[] gameChoices = currentScenario.availableChoices?.ToArray();
                if (gameChoices == null || gameChoices.Length == 0)
                    return;

                string formattedChoices = string.Join(
                    "\n",
                    gameChoices.Select((choice, index) => $"{index + 1}. {choice}")
                );

                const string SYSTEM = "Act as the character would in the scenario. Respond with choice number, then thoughts from the character's perspective.";

                Debug.Log($"Calling AI with: Character={currentChar.name}, Scenario={currentScenario.name}, Choices={formattedChoices}");

                runner.Reset();
                runner.AddTurnToContext("system", $"{SYSTEM}\nCHARACTER: {currentChar.description}");
                runner.AddTurnToContext("user", $"SCENARIO: {currentScenario.description}\n{formattedChoices}");

                generationState = new GenerationState();

                runner.Generate(OnGenerationToken, OnGenerationDone);
            }
            catch (System.Exception ex)
            {
                Debug.LogError("AI Model Error: " + ex.Message);
            }
        }

        public void ClearGenerationState()
        {
            generationState = null;
        }

        private void OnGenerationToken(string token)
        {
            generationState?.AddChunk(token);
        }

        private void OnGenerationDone(bool success)
        {
            generationState?.Done();
            runner.DebugContext();
        }

        /// <summary>
        /// Applies the generated choice, updates score/lives, and returns the outcome.
        /// </summary>
        public void RenderAssistantChoice(
            string selectedChoice,
            TMP_Text titleText,
            TMP_Text explanationText,
            TMP_Text choiceText,
            TMP_Text authorText,
            TMP_Text atmosphereText,
            LowHealthController_URP lowHealthController,
            System.Action onGameOver
        )
        {
            if (generationState == null)
                generationState = new GenerationState();
            if (generationState.choice == null)
                generationState.SetDefault();

            string characterName = gameState.GetCurrentCharacter()?.name ?? "The character";
            // Determine the text of the model's choice
            string modelChoiceText = "";
            var currentScenario = gameState.GetCurrentScenario();
            if (currentScenario != null && int.TryParse(generationState.choice, out int choiceNumber))
            {
                int index = choiceNumber - 1;
                if (index >= 0 && index < currentScenario.availableChoices.Count)
                {
                    modelChoiceText = currentScenario.availableChoices[index];
                }
            }

            // Correct or incorrect
            if (string.Equals(generationState.choice, selectedChoice, System.StringComparison.OrdinalIgnoreCase))
            {
                gameState.ModifyScore(1);
                Debug.Log($"Correct choice! Score incremented to {gameState.GetPlayerInfo().score}");
                titleText.text = $"{characterName} agrees with The Author's choice.";
                if (atmosphereText != null) atmosphereText.text = "";
            }
            else
            {
                gameState.ModifyLives(-1);;
                Debug.Log($"Incorrect choice. Lives decremented to {gameState.GetPlayerInfo().lives}");

                if (lowHealthController != null)
                {
                    float newHealth = Mathf.Clamp01(gameState.GetPlayerInfo().lives / (float)gameState.GetPlayerInfo().maxLives);
                    lowHealthController.SetPlayerHealthSmoothly(newHealth, 0.5f);
                }

                if (gameState.GetPlayerInfo().lives > 0)
                {
                    audioManager.PlayHeartbeat();
                    if (Random.Range(0, 10) < 6)
                    {
                        audioManager.PlayCrows();
                    }
                }
                else if (gameState.GetPlayerInfo().lives == 0)
                {
                    onGameOver?.Invoke();
                }

                titleText.text = $"{characterName} disagrees with The Author's choice.";
            }

            explanationText.text = $"{characterName} thinks {generationState.explanation}.";
            choiceText.text = modelChoiceText;
        }

        /// <summary>
        /// Helper class to track the generation state.
        /// </summary>
        private class GenerationState
        {
            public string choice = "";
            public bool hasChoice;
            public string explanation = "";
            public bool complete;

            public void AddChunk(string chunk)
            {
                if (!hasChoice)
                {
                    int switchIndex = -1;
                    for (int i = 0; i < chunk.Length; i++)
                    {
                        if (!char.IsDigit(chunk[i]))
                        {
                            switchIndex = i;
                            break;
                        }
                    }
                    if (switchIndex == -1)
                    {
                        choice += chunk;
                    }
                    else
                    {
                        choice += chunk.Substring(0, switchIndex);
                        explanation += chunk.Substring(switchIndex).TrimStart();
                        hasChoice = true;
                    }
                }
                else
                {
                    if (string.IsNullOrEmpty(explanation))
                    {
                        explanation += chunk.TrimStart();
                    }
                    else
                    {
                        explanation += chunk;
                    }
                }
            }

            public void SetDefault()
            {
                choice = "1";
                hasChoice = true;
                explanation = "they are temporarily confused.";
            }

            public void Done()
            {
                complete = true;
            }
        }
    }
}
