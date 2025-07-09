using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace ai.aviad.AIBook
{
    // Character Display Script
    public class CharacterDisplay : MonoBehaviour
    {
        public GameState gameState;

        [Header("Character Display Elements")]
        public GameObject characterImage;
        public TMP_Text characterName;
        public TMP_Text characterDescription;
        public GameObject gameStateCharImage;
        public TMP_Text gameStateCharName;
        public TMP_Text gameStateCharDescription;
        public TMP_Text choicePrompt;

        [Header("Settings")]
        public bool updateOnStart = true;

        private void Start()
        {
            if (updateOnStart) UpdateDisplay();
        }

        private void OnEnable()
        {
            gameState.OnCharacterChanged += OnCharacterChanged;
        }

        private void OnDisable()
        {
            gameState.OnCharacterChanged -= OnCharacterChanged;
        }

        private void OnCharacterChanged(Character newCharacter)
        {
            UpdateDisplay(newCharacter);
        }

        public void UpdateDisplay()
        {
           Character currentCharacter = gameState.GetCurrentCharacter();
           UpdateDisplay(currentCharacter);
        }

        private void UpdateDisplay(Character character)
        {
            if (character == null) return;

            // Update image
            if (characterImage != null)
            {
                MeshRenderer renderer = characterImage.GetComponent<MeshRenderer>();
                if (renderer != null && character.material != null)
                    renderer.material = character.material;
            }

            // Update text
            if (characterName != null)
                characterName.text = character.name;

            if (characterDescription != null)
                characterDescription.text = character.description;

            // Update game state image
            if (gameStateCharImage != null)
            {
                MeshRenderer renderer = gameStateCharImage.GetComponent<MeshRenderer>();
                if (renderer != null && character.material != null)
                    renderer.material = character.material;
            }

            // Update game state text
            if (gameStateCharName != null)
                gameStateCharName.text = character.name;

            if (gameStateCharDescription != null)
                gameStateCharDescription.text = character.description;

            choicePrompt.text = $"Author, what does {character.name} do?";
        }

    }

}