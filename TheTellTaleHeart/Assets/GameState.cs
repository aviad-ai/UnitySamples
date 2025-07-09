using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ai.aviad.AIBook
{
    [System.Serializable]
    public class Character
    {
        public string name;
        public Material material;
        [TextArea(3, 5)]
        public string description;
    }

    [System.Serializable]
    public class Scenario
    {
        public string name;
        public Material material;
        [TextArea(3, 5)]
        public string description;
        public List<string> availableChoices;
    }

    [System.Serializable]
    public class PlayerInfo
    {
        public int lives;
        public int maxLives;
        public string playerText;
        public int score;
    }

    public class GameState : MonoBehaviour
    {
        [Header("Character Configuration")]
        public Character[] characters;

        [Header("Scenario Configuration")]
        public Scenario[] scenarios;

        [Header("Player Configuration")]
        public PlayerInfo playerInfo = new PlayerInfo();

        private int currentCharacterIndex = 0;
        private int currentScenarioIndex = 0;

        // Events for state changes
        public event Action<Character> OnCharacterChanged;
        public event Action<Scenario> OnScenarioChanged;
        public event Action<PlayerInfo> OnPlayerInfoChanged;

        // Specific player events for granular updates
        public event Action<int, int> OnLivesChanged; // current, max
        public event Action<int> OnScoreChanged;

        private Dictionary<int, string[]> livesMessages = new Dictionary<int, string[]>
        {
            { 4, new[] {
                "The Author feels a chill in the room.",
                "The Author’s pen feels heavier than it should.",
                "The Author rereads a line they don’t remember writing."
            }},
            { 3, new[] {
                "The Author’s heart skips a beat.",
                "The Author feels like they are being watched."
            }},
            { 2, new[] {
                "The Author's skin crawls.",
                "The Author’s eyes burn and begin to water."
            }},
            { 1, new[] {
                "The Author's chest tightens.",
                "The Author’s breathing becomes labored."
            }},
            { 0, new[] {
                "The Author clutches at their chest and collapses."
            }}
        };

        private Dictionary<string, string[]> scoreMessages = new Dictionary<string, string[]>
        {
            { "0-5", new[]
                {
                    "The Author has achieved nothing.",
                    "The Author is unsure if this is the right story.",
                }
            },
            { "6-10", new[]
                {
                    "The Author feels unsettled.",
                    "The Author cannot recall the beginning, only the end.",
                    "The characters do not look the author in the eye."
                }
            },
            { "11-15", new[]
                {
                    "The characters sneak furtive glances at the author.",
                    "The Author straightens their back.",
                }
            },
            { "20+", new[]
                {
                    "The characters trust the author now.",
                    "The Author’s hands do not falter.",
                }
            }
        };


        private void Awake()
        {
            InitializeDefaultPlayerInfo();
        }

        private void Start()
        {
            // Initialize with first character and scenario if available
            if (characters != null && characters.Length > 0)
            {
                NotifyCharacterChanged();
            }
            if (scenarios != null && scenarios.Length > 0)
            {
                NotifyScenarioChanged();
            }
            NotifyPlayerInfoChanged();
        }

        private void InitializeDefaultPlayerInfo()
        {
            if (string.IsNullOrEmpty(playerInfo.playerText))
                playerInfo.playerText = "The Author has achieved nothing.";
            if (playerInfo.lives <= 0)
                playerInfo.lives = playerInfo.maxLives;
        }

        public void ResetPlayerInfo()
        {
            playerInfo.lives = playerInfo.maxLives;
            playerInfo.score = 0;
            playerInfo.playerText = "The Author has achieved nothing.";

            NotifyPlayerInfoChanged();
            OnLivesChanged?.Invoke(playerInfo.lives, playerInfo.maxLives);
            OnScoreChanged?.Invoke(playerInfo.score);
        }

        #region Character Management

        public Character GetCurrentCharacter()
        {
            Debug.Log(characters.Length + "|" + currentCharacterIndex);
            if (characters == null || characters.Length == 0 || currentCharacterIndex < 0 || currentCharacterIndex >= characters.Length)
                return null;
            return characters[currentCharacterIndex];
        }

        public void CycleToNextCharacter()
        {
            if (characters == null || characters.Length == 0) return;

            currentCharacterIndex = (currentCharacterIndex + 1) % characters.Length;
            NotifyCharacterChanged();
            Debug.Log($"Cycled to character: {GetCurrentCharacter().name}");
        }

        public void SetCharacterByIndex(int index)
        {
            if (characters == null || index < 0 || index >= characters.Length) return;

            currentCharacterIndex = index;
            NotifyCharacterChanged();
            Debug.Log($"Set character to: {GetCurrentCharacter().name}");
        }

        public void NotifyCharacterChanged()
        {
            Character currentChar = GetCurrentCharacter();
            if (currentChar != null)
                OnCharacterChanged?.Invoke(currentChar);
        }

        #endregion

        #region Scenario Management

        public Scenario GetCurrentScenario()
        {
            if (scenarios == null || scenarios.Length == 0 || currentScenarioIndex < 0 || currentScenarioIndex >= scenarios.Length)
                return null;
            return scenarios[currentScenarioIndex];
        }

        public void CycleToNextScenario()
        {
            if (scenarios == null || scenarios.Length == 0) return;

            currentScenarioIndex = (currentScenarioIndex + 1) % scenarios.Length;
            NotifyScenarioChanged();
            Debug.Log($"Cycled to scenario: {GetCurrentScenario().name}");
        }

        public void SetScenarioByIndex(int index)
        {
            if (scenarios == null || index < 0 || index >= scenarios.Length) return;

            currentScenarioIndex = index;
            NotifyScenarioChanged();
            Debug.Log($"Set scenario to: {GetCurrentScenario().name}");
        }

        public void NotifyScenarioChanged()
        {
            Scenario currentScenario = GetCurrentScenario();
            if (currentScenario != null)
                OnScenarioChanged?.Invoke(currentScenario);
        }

        #endregion

        #region Player Info Management

        public PlayerInfo GetPlayerInfo()
        {
            return playerInfo;
        }

        public void SetPlayerText(string text)
        {
            playerInfo.playerText = text;
            NotifyPlayerInfoChanged();
        }

        public void ModifyLives(int amount)
        {
            playerInfo.lives = Mathf.Clamp(playerInfo.lives + amount, 0, playerInfo.maxLives);
            OnLivesChanged?.Invoke(playerInfo.lives, playerInfo.maxLives);
            NotifyPlayerInfoChanged();
            Debug.Log($"Lives: {playerInfo.lives}/{playerInfo.maxLives}");
        }

        public void SetLives(int lives)
        {
            playerInfo.lives = Mathf.Clamp(lives, 0, playerInfo.maxLives);
            OnLivesChanged?.Invoke(playerInfo.lives, playerInfo.maxLives);
            NotifyPlayerInfoChanged();
        }

        public void ModifyScore(int amount)
        {
            playerInfo.score = Mathf.Max(0, playerInfo.score + amount);
            OnScoreChanged?.Invoke(playerInfo.score);
            NotifyPlayerInfoChanged();
            Debug.Log($"Score: {playerInfo.score}");
        }

        public string[] GetLivesMessages(int lives)
        {
            return livesMessages.ContainsKey(lives) ? livesMessages[lives] : null;
        }

        public string GetRandomScoreMessage(int score)
        {
            string[] messages = null;

            if (score >= 20)
                messages = scoreMessages["20+"];
            else if (score >= 11)
                messages = scoreMessages["11-15"];
            else if (score >= 6)
                messages = scoreMessages["6-10"];
            else
                messages = scoreMessages["0-5"];

            return messages[UnityEngine.Random.Range(0, messages.Length)];
        }

        private void NotifyPlayerInfoChanged()
        {
            OnPlayerInfoChanged?.Invoke(playerInfo);
        }

        #endregion

    }

    // Helper class for JSON serialization of Lists
    [System.Serializable]
    public class SerializableList<T>
    {
        public List<T> items;

        public SerializableList(List<T> list)
        {
            items = list ?? new List<T>();
        }
    }
}