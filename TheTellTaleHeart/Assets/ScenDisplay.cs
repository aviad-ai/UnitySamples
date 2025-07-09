using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

namespace ai.aviad.AIBook
{
    // Scenario Display Script
    public class ScenarioDisplay : MonoBehaviour
    {
        public GameState gameState;

        [Header("Scenario Display Elements")]
        public TMP_Text scenarioName;
        public TMP_Text scenarioDescription;
        public GameObject scenarioImage;
        public TMP_Text gameStateScenarioName;
        public TMP_Text gameStateScenarioDescription;
        public GameObject gameStateScenarioImage;
        public TMP_Text gameStateChoiceA;
        public TMP_Text gameStateChoiceB;
        public TMP_Text gameStateChoiceC;

        [Header("Settings")]
        public bool updateOnStart = true;

        private void Start()
        {
            if (updateOnStart) UpdateDisplay();
        }

        private void OnEnable()
        {
            gameState.OnScenarioChanged += OnScenarioChanged;
        }

        private void OnDisable()
        {
            gameState.OnScenarioChanged -= OnScenarioChanged;
        }

        private void OnScenarioChanged(Scenario newScenario)
        {
            UpdateDisplay(newScenario);
        }

        public void UpdateDisplay()
        {
            Scenario currentScenario = gameState.GetCurrentScenario();
            UpdateDisplay(currentScenario);
        }

        private void UpdateDisplay(Scenario scenario)
        {
            if (scenario == null) return;

            // Update text
            if (scenarioName != null)
                scenarioName.text = scenario.name;

            if (scenarioDescription != null)
                scenarioDescription.text = scenario.description;

            // Update image
            if (scenarioImage != null)
            {
                MeshRenderer renderer = scenarioImage.GetComponent<MeshRenderer>();
                if (renderer != null && scenario.material != null)
                    renderer.material = scenario.material;
            }
            // Update text
            if (gameStateScenarioName != null)
                gameStateScenarioName.text = scenario.name;

            if (gameStateScenarioDescription != null)
                gameStateScenarioDescription.text = scenario.description;

            // Update image
            if (gameStateScenarioImage != null)
            {
                MeshRenderer renderer = gameStateScenarioImage.GetComponent<MeshRenderer>();
                if (renderer != null && scenario.material != null)
                    renderer.material = scenario.material;
            }

            // Update action buttons
            if (scenario.availableChoices != null)
            {
                gameStateChoiceA.text = scenario.availableChoices[0];
                gameStateChoiceB.text = scenario.availableChoices[1];
                gameStateChoiceC.text = scenario.availableChoices[2];
            }
        }
    }
}