using System.Collections;
using UnityEngine;
using echo17.EndlessBook;
using TMPro;
using Aviad;

namespace ai.aviad.AIBook
{

    public static class UIPositioning
    {
        // Reference resolution - adjust these to match your target design resolution
        private static readonly Vector2 REFERENCE_RESOLUTION = new Vector2(1920f, 1080f);

        // Get resolution-independent position as normalized coordinates (0-1)
        public static Vector2 GetNormalizedPosition(float normalizedX, float normalizedY)
        {
            return new Vector2(normalizedX, normalizedY);
        }

        // Convert normalized position to screen coordinates
        public static Vector2 NormalizedToScreen(Vector2 normalizedPos)
        {
            return new Vector2(
                normalizedPos.x * Screen.width,
                normalizedPos.y * Screen.height
            );
        }

        // Get resolution-independent size
        public static float GetScaledSize(float referenceSize, bool useHeight = false)
        {
            float scaleFactor = useHeight ?
                Screen.height / REFERENCE_RESOLUTION.y :
                Screen.width / REFERENCE_RESOLUTION.x;
            return referenceSize * scaleFactor;
        }

        // Get minimum scale factor to maintain aspect ratio
        public static float GetUniformScale()
        {
            return Mathf.Min(Screen.width / REFERENCE_RESOLUTION.x, Screen.height / REFERENCE_RESOLUTION.y);
        }
    }

    public class AIBook : MonoBehaviour
    {

        public AudioManager audioManager;
        private AIHandler aiHandler;
        public GameState gameState;

        [Header("Book Settings")]
        public EndlessBook book;
        public float stateAnimationTime = 1f;
        public EndlessBook.PageTurnTimeTypeEnum turnTimeType = EndlessBook.PageTurnTimeTypeEnum.TotalTurnTime;
        public float turnTime = 1f;

        [Header("AI Settings")]
        public AviadRunner runner;

        [Header("Lives/Score Display")]
        public Texture2D heartTexture; // assign this in the Inspector
        public TMP_Text scoreText;
        private bool hasEvaluatedChoiceThisTurn = false;

        [Header("UI Settings")]
        public int padding = 20;
        public int buttonWidth = 140;
        public int buttonHeight = 40;
        private int inputHeight = 180;
        private int inputWidth = 600;
        public Camera mainCamera; // For perspective warp
        public BoxCollider extents;
        public bool debugGUI = false;

        [Range(-1, 1)]
        public float quillOffsetX;
        [Range(-1, 1)]
        public float quillOffsetY;
        [Range(-1, 1)]
        public float quillPage5OffsetX;
        [Range(-1, 1)]
        public float quillPage5OffsetY;
        [Range(-1, 1)]
        public float nextArrowOffsetX;
        [Range(-1, 1)]
        public float nextArrowOffsetY;
        [Range(-1, 1)]
        public float diceOffsetX;
        [Range(-1, 1)]
        public float diceOffsetY;
        [Range(0, 10)]
        public float choiceSpacing;
        [Range(-1, 1)]
        public float choiceOffsetX;
        [Range(-1, 1)]
        public float choiceOffsetY;
        [Range(-1, 1)]
        public float livesOffsetX;
        [Range(-1, 1)]
        public float livesOffsetY;


        private string userInput = "";
        private string lastFocusedInput = "";
        private bool isInputFocused = false;
        private GUIStyle arrowButtonStyle;
        private GUIStyle diceButtonStyle;
        private GUIStyle choiceButtonStyle;
        private GUIStyle quillButtonStyle;
        private GUIStyle textInputStyle;
        private string selectedChoice = null;

        // Page state flags
        private bool showChoices = false;
        private bool showCharacterInput = false;
        private bool showCharacterEdit = false;
        private bool showLives = false;

        [Header("AI Response Page")]
        public TMP_Text titleText;          // The heading text (character agrees...)
        public TMP_Text explanationText;    // The explanation (after the choice number)
        public TMP_Text choiceText;         // The text of the selected choice

        [Header("Arrow Button Settings")]
        public Texture2D arrowTexture;

        [Header("Dice Button Settings")]
        public Texture2D diceTexture;

        [Header("Quill Button Settings")]
        public Texture2D quillTexture;
        private bool showTextInputOverride = false;

        [Header("Choice Button Settings")]
        public Texture2D choiceXTexture;

        [Header("Text Input Settings")]
        public Texture2D textInputBackground;
        public Font customFont;
        public int fontSize = 52;

        // Add this flag to control UI rendering during page turns
        private bool isPageTurning = false;
        private bool isInitialPageTurnComplete = false;

        private bool isGameOver = false;
        private bool showPlayAgainButton = false;
        public LowHealthController_URP lowHealthController;

        [Header("Atmosphere Text")]
        public TMP_Text atmosphereText;
        public TMP_Text authorText;

        private Vector3[] bookExtents;

        // Has the player clicked next on the choice page? We will turn when the AI is ready.
        private bool choicePageShouldTurn = false;

        #region Unity Lifecycle

        private async void Start()
        {
            audioManager.StartBackgroundMusic();
            aiHandler = new AIHandler(runner, gameState, audioManager);

            book.TurnToPage(1, turnTimeType, turnTime, stateAnimationTime, OnBookTurnToPageCompleted, OnPageTurnStart, OnPageTurnEnd);
            SubscribeToEvents();
        }

        private void RestartGame()
        {
            Debug.Log("Restarting the game...");

            // Reset flags
            isGameOver = false;
            showPlayAgainButton = false;
            isInitialPageTurnComplete = false;
            aiHandler.ClearGenerationState();
            hasEvaluatedChoiceThisTurn = false;
            if (lowHealthController != null)
            {
                float newHealth = Mathf.Clamp01(5 / 5f);
                lowHealthController.SetPlayerHealthSmoothly(newHealth, 0.5f);
            }

            // Reset score and lives
            gameState.ResetPlayerInfo();
            scoreText.text = "Chapters: 0";
            if (atmosphereText != null)
            {
                atmosphereText.text = "";
            }
            if (authorText != null)
            {
                authorText.text = gameState.GetRandomScoreMessage(gameState.GetPlayerInfo().score);
            }

            // Reopen the book
            book.TurnToPage(1, turnTimeType, turnTime, stateAnimationTime, OnBookTurnToPageCompleted, OnPageTurnStart, OnPageTurnEnd);
            audioManager.StartBackgroundMusic();

            UpdatePageState();
        }

        private async void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        #endregion

        #region Event Management

        private void SubscribeToEvents()
        {
            gameState.OnCharacterChanged += OnCharacterChanged;
            gameState.OnLivesChanged += OnLivesChanged;
            gameState.OnScoreChanged += OnScoreChanged;
        }

        private void UnsubscribeFromEvents()
        {
            gameState.OnCharacterChanged -= OnCharacterChanged;
            gameState.OnLivesChanged -= OnLivesChanged;
            gameState.OnScoreChanged -= OnScoreChanged;
        }

        private void OnCharacterChanged(Character character)
        {
            if (book.CurrentPageNumber == 7)
            {
                LoadCurrentDescription();
            }
        }

        private void OnScoreChanged(int newScore)
        {
            scoreText.text = $"Chapters: {newScore}";
            if (authorText != null)
            {
                authorText.text = gameState.GetRandomScoreMessage(newScore);
            }
        }


        private void OnLivesChanged(int currentLives, int maxLives)
        {
            if (atmosphereText != null)
            {
                var messages = gameState.GetLivesMessages(currentLives);
                atmosphereText.text = (messages != null && messages.Length > 0)
                    ? messages[Random.Range(0, messages.Length)]
                    : "";
            }
        }


        #endregion

        #region Page Management

        // Fixed callback methods for page turn events
        protected virtual void OnPageTurnStart(echo17.EndlessBook.Page page, int pageNumberFront, int pageNumberBack, int pageNumberFirstVisible, int pageNumberLastVisible, Page.TurnDirectionEnum turnDirection)
        {
            isPageTurning = true;
            Debug.Log($"OnPageTurnStart: front [{pageNumberFront}] back [{pageNumberBack}] fv [{pageNumberFirstVisible}] lv [{pageNumberLastVisible}] dir [{turnDirection}]. UI elements hidden.");
            audioManager.PlayPageTurn();
        }

        protected virtual void OnPageTurnEnd(Page page, int pageNumberFront, int pageNumberBack, int pageNumberFirstVisible, int pageNumberLastVisible, Page.TurnDirectionEnum turnDirection)
        {
            isPageTurning = false;
            Debug.Log($"OnPageTurnEnd: front [{pageNumberFront}] back [{pageNumberBack}] fv [{pageNumberFirstVisible}] lv [{pageNumberLastVisible}] dir [{turnDirection}]. UI elements shown.");
            audioManager.StopPagesFlipping();
        }

        protected virtual void OnBookTurnToPageCompleted(EndlessBook.StateEnum fromState, EndlessBook.StateEnum toState, int currentPageNumber)
        {
            if (fromState == EndlessBook.StateEnum.OpenMiddle && toState == EndlessBook.StateEnum.ClosedFront)
            {
                audioManager.PlayBookClose();
                showPlayAgainButton = true;
                book.SetPageNumber(1);
            }
            isInitialPageTurnComplete = true;
            UpdatePageState();
            Debug.Log($"Page turn finished. Current page: {book.CurrentPageNumber}");
            audioManager.StopPagesFlipping();
            // Play open/close sounds based on state
            if (fromState == EndlessBook.StateEnum.ClosedFront && toState == EndlessBook.StateEnum.OpenMiddle)
            {
                audioManager.PlayBookOpen();
            }
        }

        private async void UpdatePageState()
        {
            if (!isInitialPageTurnComplete)
            {
                return;
            }

            int currentPage = book.CurrentPageNumber;
            bool oldShowCharacter = showCharacterInput;
            bool oldShowChoices = showChoices;
            bool oldShowCharacterEdit = showCharacterEdit;

            ResetPageFlags();
            hasEvaluatedChoiceThisTurn = false;

            switch (currentPage)
            {
                case 5:
                    book.SetPageNumber(3);
                    break;
                case 7:
                    showCharacterInput = true;
                    if (!oldShowCharacter) LoadCurrentDescription();
                    break;
                case 11:
                    showChoices = true;
                    showCharacterEdit = true;
                    showLives = true;
                    break;
                case 13:
                    showLives = true;
                    break;
                case 15:
                    CycleScenario();
                    showChoices = true;
                    showCharacterEdit = true;
                    showLives = true;
                    book.SetPageNumber(11);
                    await aiHandler.TriggerGeneration();
                    break;
            }

            currentPage = book.CurrentPageNumber;
            LogPageStateChange(currentPage, oldShowCharacter, oldShowChoices, oldShowCharacterEdit);
        }

        private void ResetPageFlags()
        {
            showChoices = false;
            showCharacterInput = false;
            showCharacterEdit = false;
            showLives = false;
        }

        private void LogPageStateChange(int currentPage, bool oldShowCharacter, bool oldShowChoices, bool oldShowCharacterEdit)
        {
            if (oldShowCharacter != showCharacterInput || oldShowChoices != showChoices || oldShowCharacterEdit != showCharacterEdit || oldShowCharacterEdit != showCharacterEdit)
            {
                Debug.Log($"Page {currentPage}: Character={showCharacterInput}, Choices={showChoices}, CharacterEdit={showCharacterEdit}, Lives={showLives}");
            }
        }

        #endregion

        #region Data Management

        private void LoadCurrentDescription()
        {
            if (gameState == null)
            {
                Debug.LogWarning("GameState.Instance is null - retrying in next frame");
                StartCoroutine(RetryLoadDescription());
                return;
            }

            if (showCharacterInput)
            {
                LoadCharacterDescription();
            }
        }

        private IEnumerator RetryLoadDescription()
        {
            yield return new WaitForEndOfFrame();
            LoadCurrentDescription();
        }

        private void LoadCharacterDescription()
        {
            Character currentChar = gameState.GetCurrentCharacter();
            if (currentChar == null)
            {
                Debug.LogError("Current character is null!");
                return;
            }

            userInput = currentChar.description ?? "";
            Debug.Log($"Loaded character description for: {currentChar.name}");
        }

        private void LoadScenarioDescription()
        {
            Scenario currentScenario = gameState.GetCurrentScenario();
            if (currentScenario == null)
            {
                Debug.LogError("Current scenario is null!");
                return;
            }

            userInput = currentScenario.description ?? "";
            Debug.Log($"Loaded scenario description for: {currentScenario.name}");
        }

        private void UpdateCurrentDescription()
        {
            if (showCharacterInput)
            {
                UpdateCharacterDescription();
            }
        }

        private void UpdateCharacterDescription()
        {
            if (gameState == null) return;

            Character currentChar = gameState.GetCurrentCharacter();
            if (currentChar == null) return;

            currentChar.description = userInput;
            gameState.NotifyCharacterChanged();
            Debug.Log($"Updated character description for: {currentChar.name}");
        }

        private void Update()
        {
            bookExtents = ExtentsToScreenSpace();
            if (choicePageShouldTurn && aiHandler.Done)
            {
                HandleChoicePage();
                choicePageShouldTurn = false;
            }
        }

        #endregion

        #region GUI Rendering

        private async void OnGUI()
        {
            if (showPlayAgainButton)
            {
                RenderPlayAgainButton();
                return;
            }

            if (isGameOver)
            {
                return;
            }
            InitializeStyles();

            float screenW = Screen.width;
            float screenH = Screen.height;
            var points = ExtentsToScreenSpace();
            if (debugGUI && bookExtents.Length != 0)
            {
                RenderDebug(points);
            }
            // Only render UI elements if not currently turning pages
            if (!isPageTurning && isInitialPageTurnComplete)
            {
                // Render top navigation buttons
                GUILayout.BeginArea(new Rect(padding, padding, screenW - 2 * padding, 100));
                try
                {
                    RenderNavigationButtons();
                }
                finally
                {
                    GUILayout.EndArea();
                }

                // Render quill button
                RenderQuillButton(screenW, screenH);
                if (showCharacterEdit)
                {
                    RenderCharacterQuillButtonPage7(screenW, screenH);
                }
                // Render content area (text input or choices) in bottom right
                if (showCharacterInput && showTextInputOverride)
                {
                    RenderBottomRightTextInput(screenW, screenH);
                }
                if (showChoices)
                {
                    RenderChoiceButtons(screenW, screenH);
                }
                if (showLives)
                {
                    RenderLives(screenW, screenH);
                }

                RenderNextButton(screenW, screenH);
                RenderPageSpecificDiceButtons(screenW, screenH);
            }
        }

        private void RenderDebug(Vector3[] points)
        {
            foreach (var pt in points)
            {
                DrawCircle(pt.x, pt.y, 5, Color.green);
            }
        }

        private void InitializeStyles()
        {
            if (arrowButtonStyle != null) return;

            GUI.skin.settings.cursorColor = Color.black;

            Texture2D transparentTex = new Texture2D(1, 1);
            transparentTex.SetPixel(0, 0, new Color(0, 0, 0, 0));
            transparentTex.Apply();

            arrowButtonStyle = new GUIStyle(GUI.skin.button);
            arrowButtonStyle.normal.background = transparentTex;
            arrowButtonStyle.hover.background = transparentTex;
            arrowButtonStyle.active.background = transparentTex;
            arrowButtonStyle.focused.background = transparentTex;
            arrowButtonStyle.border = new RectOffset(0, 0, 0, 0);
            arrowButtonStyle.alignment = TextAnchor.MiddleCenter;

            diceButtonStyle = new GUIStyle(GUI.skin.button);
            diceButtonStyle.normal.background = transparentTex;
            diceButtonStyle.hover.background = transparentTex;
            diceButtonStyle.active.background = transparentTex;
            diceButtonStyle.focused.background = transparentTex;
            diceButtonStyle.border = new RectOffset(0, 0, 0, 0);
            diceButtonStyle.alignment = TextAnchor.MiddleCenter;

            quillButtonStyle = new GUIStyle(GUI.skin.button);
            quillButtonStyle.normal.background = transparentTex;
            quillButtonStyle.hover.background = transparentTex;
            quillButtonStyle.active.background = transparentTex;
            quillButtonStyle.focused.background = transparentTex;
            quillButtonStyle.border = new RectOffset(0, 0, 0, 0);
            quillButtonStyle.alignment = TextAnchor.MiddleCenter;

            choiceButtonStyle = new GUIStyle(GUI.skin.button);
            choiceButtonStyle.normal.background = transparentTex;
            choiceButtonStyle.hover.background = transparentTex;
            choiceButtonStyle.active.background = transparentTex;
            choiceButtonStyle.focused.background = transparentTex;
            choiceButtonStyle.border = new RectOffset(0, 0, 0, 0);
            choiceButtonStyle.alignment = TextAnchor.MiddleCenter;

            textInputStyle = new GUIStyle(GUI.skin.textArea);
            textInputStyle.fontSize = fontSize;

            if (textInputBackground != null)
            {
                textInputStyle.normal.background = textInputBackground;
                textInputStyle.hover.background = textInputBackground;
                textInputStyle.active.background = textInputBackground;
                textInputStyle.focused.background = textInputBackground;
            }

            if (customFont != null)
            {
                textInputStyle.font = customFont;
            }

            textInputStyle.normal.textColor = Color.black;
            textInputStyle.hover.textColor = Color.black;
            textInputStyle.active.textColor = Color.black;
            textInputStyle.focused.textColor = Color.black;
            textInputStyle.wordWrap = true;
            textInputStyle.alignment = TextAnchor.UpperLeft;
            textInputStyle.padding = new RectOffset(10, 10, 10, 10);
        }

        private void RenderNavigationButtons()
        {
            int currentPage = book.CurrentPageNumber;

            if (currentPage == 11 || currentPage == 15)
            {
                GUILayout.BeginHorizontal();

                GUILayout.EndHorizontal();
            }
        }

        private Vector3[] ExtentsToScreenSpace()
        {
            if (extents == null || mainCamera == null)
            {
                Debug.LogError("Cannot calculate book bounds. Set mainCamera and extents.");
                return new Vector3[0];
            }
            Bounds bounds = extents.bounds;

            // We are just concerned with the top face
            Vector3[] corners = new Vector3[8];
            corners[0] = new Vector3(bounds.min.x, bounds.max.y, bounds.min.z);
            corners[1] = new Vector3(bounds.min.x, bounds.max.y, bounds.max.z);
            corners[2] = new Vector3(bounds.max.x, bounds.max.y, bounds.min.z);
            corners[3] = bounds.max;

            // Convert each corner to screen space
            Vector3[] screenPoints = new Vector3[8];
            for (int i = 0; i < corners.Length; i++)
            {
                screenPoints[i] = mainCamera.WorldToScreenPoint(corners[i]);

            }
            CalculateScreenToBoundsMatrix(screenPoints);
            return screenPoints;

        }

        void DrawCircle(float centerX, float centerY, float radius, Color color)
        {

            // Draw multiple small rectangles to approximate a circle
            int segments = 20;
            GUI.color = color;

            for (int i = 0; i < segments; i++)
            {
                float angle = (i * 2 * Mathf.PI) / segments;
                float x = centerX + Mathf.Cos(angle) * radius;
                float y = centerY + Mathf.Sin(angle) * radius;
                GUI.DrawTexture(new Rect((x - 1), Screen.height - (y - 1), 2, 2), Texture2D.whiteTexture);
            }

            GUI.color = Color.white; // Reset color
        }

        private void RenderCharacterQuillButtonPage7(float screenW, float screenH)
        {
            if (quillTexture == null)
            {
                Debug.LogWarning("Quill texture not assigned! Please assign a quill PNG in the Inspector.");
                return;
            }

            float quillSize = UIPositioning.GetScaledSize(57f); // Scaled size
            float hoverScale = 1.1f;

            // Normalized position
            Vector2 normalizedPos = new Vector2(0.38f, 0.43f);
            normalizedPos.x += quillPage5OffsetX * 0.1f;
            normalizedPos.y += quillPage5OffsetY * 0.1f;

            Vector2 screenPos = UIPositioning.NormalizedToScreen(normalizedPos);
            float buttonX = screenPos.x - quillSize;
            float buttonY = screenPos.y;

            var transformedPos = bookExtents != null ?
                ScreenToBoundsSpace(new Vector2(buttonX, buttonY)) :
                new Vector2(buttonX, buttonY);

            Rect baseRect = new Rect(transformedPos.x, transformedPos.y, quillSize, quillSize);
            bool isHover = baseRect.Contains(Event.current.mousePosition);

            float size = isHover ? quillSize * hoverScale : quillSize;
            float offset = (size - quillSize) / 2f;

            Vector2 finalPos = new Vector2(buttonX - offset, buttonY - offset);
            if (bookExtents != null)
            {
                finalPos = ScreenToBoundsSpace(finalPos);
            }

            Rect scaledRect = new Rect(finalPos.x, finalPos.y, size, size);

            GUI.DrawTexture(scaledRect, quillTexture, ScaleMode.ScaleToFit);

            if (GUI.Button(scaledRect, "", quillButtonStyle))
            {
                if (!choicePageShouldTurn)
                {
                    NavigateToPage(7, "Character");
                }
            }
        }


        private void RenderPageSpecificDiceButtons(float screenW, float screenH)
        {
            int currentPage = book.CurrentPageNumber;

            if (currentPage == 7)
            {
                RenderDiceButton(screenW, screenH, "Character", CycleCharacter);
            }
            else if (currentPage == 9)
            {
                RenderDiceButton(screenW, screenH, "Scenario", CycleScenario);
            }
        }

        private void RenderDiceButton(float screenW, float screenH, string buttonType, System.Action onClickAction)
        {
            if (diceTexture == null)
            {
                Debug.LogWarning($"Dice texture not assigned! Please assign a dice PNG in the Inspector for {buttonType} button.");
                return;
            }

            float diceSize = UIPositioning.GetScaledSize(57f);
            float hoverScale = 1.1f;

            // Normalized position
            Vector2 normalizedPos = new Vector2(0.88f, 0.23f);
            normalizedPos.x += diceOffsetX * 0.1f;
            normalizedPos.y += diceOffsetY * 0.1f;

            Vector2 screenPos = UIPositioning.NormalizedToScreen(normalizedPos);
            float buttonX = screenPos.x - diceSize;
            float buttonY = screenPos.y;

            var transformedPos = bookExtents != null ?
                ScreenToBoundsSpace(new Vector2(buttonX, buttonY)) :
                new Vector2(buttonX, buttonY);

            Rect baseRect = new Rect(transformedPos.x, transformedPos.y, diceSize, diceSize);
            bool isHover = baseRect.Contains(Event.current.mousePosition);

            float size = isHover ? diceSize * hoverScale : diceSize;
            float offset = (size - diceSize) / 2f;

            Vector2 finalPos = new Vector2(buttonX - offset, buttonY - offset);
            if (bookExtents != null)
            {
                finalPos = ScreenToBoundsSpace(finalPos);
            }

            Rect scaledRect = new Rect(finalPos.x, finalPos.y, size, size);

            GUI.DrawTexture(scaledRect, diceTexture, ScaleMode.ScaleToFit);

            if (GUI.Button(scaledRect, "", diceButtonStyle))
            {
                onClickAction?.Invoke();
                Debug.Log($"Cycle {buttonType} dice button clicked");
            }
        }


        private void RenderQuillButton(float screenW, float screenH)
        {
            if (book.CurrentPageNumber != 7)
            {
                return;
            }
            if (quillTexture == null)
            {
                Debug.LogWarning("Quill texture not assigned! Please assign a quill PNG in the Inspector.");
                return;
            }

            float quillSize = UIPositioning.GetScaledSize(77f); // 77px at reference resolution
            float hoverScale = 1.1f;

            // Normalized position (0-1 coordinates)
            Vector2 normalizedPos = new Vector2(0.55f, 0.66f);

            // Apply offset using normalized coordinates
            normalizedPos.x += quillOffsetX * 0.1f; // Scale offset to reasonable range
            normalizedPos.y += quillOffsetY * 0.1f;

            // Convert to screen coordinates
            Vector2 screenPos = UIPositioning.NormalizedToScreen(normalizedPos);
            float buttonX = screenPos.x - quillSize;
            float buttonY = screenPos.y;

            // Apply book transformation if needed
            var transformedPos = bookExtents != null ?
                ScreenToBoundsSpace(new Vector2(buttonX, buttonY)) :
                new Vector2(buttonX, buttonY);

            Rect baseRect = new Rect(transformedPos.x, transformedPos.y, quillSize, quillSize);
            bool isHover = baseRect.Contains(Event.current.mousePosition);

            float size = isHover ? quillSize * hoverScale : quillSize;
            float offset = (size - quillSize) / 2f;

            // Calculate final position with hover offset
            Vector2 finalPos = new Vector2(buttonX - offset, buttonY - offset);
            if (bookExtents != null)
            {
                finalPos = ScreenToBoundsSpace(finalPos);
            }

            Rect scaledRect = new Rect(finalPos.x, finalPos.y, size, size);

            GUI.DrawTexture(scaledRect, quillTexture, ScaleMode.ScaleToFit);

            if (GUI.Button(scaledRect, "", quillButtonStyle))
            {
                showTextInputOverride = !showTextInputOverride;
                Debug.Log($"Quill button clicked - Text input override: {showTextInputOverride}");
            }
        }


        private void RenderBottomRightTextInput(float screenW, float screenH)
        {
            // Use normalized dimensions
            Vector2 normalizedSize = new Vector2(0.3f, 0.15f);
            Vector2 normalizedPos = new Vector2(0.50f, 0.68f);

            // Convert to screen coordinates
            Vector2 screenSize = new Vector2(
                normalizedSize.x * Screen.width,
                normalizedSize.y * Screen.height
            );
            Vector2 screenPos = UIPositioning.NormalizedToScreen(normalizedPos);

            // Apply book transformation if needed
            Vector2 finalPos = bookExtents != null ?
                ScreenToBoundsSpace(screenPos) :
                screenPos;

            GUILayout.BeginArea(new Rect(finalPos.x, finalPos.y, screenSize.x, screenSize.y));

            string controlName = "CharacterTextInput";
            GUI.SetNextControlName(controlName);

            string newInput = GUILayout.TextArea(
                userInput,
                textInputStyle,
                GUILayout.Width(screenSize.x),
                GUILayout.Height(screenSize.y - UIPositioning.GetScaledSize(40f))
            );


            const int maxChars = 140;
            if (newInput.Length > maxChars)
            {
                newInput = newInput.Substring(0, maxChars);
            }

            HandleFocusTracking(controlName);
            userInput = newInput;

            GUILayout.EndArea();
        }

        private void RenderChoiceButtons(float screenW, float screenH)
        {
            if (choiceXTexture == null)
            {
                Debug.LogWarning("Choice X texture not assigned! Please assign an X PNG in the Inspector.");
                return;
            }

            float buttonSize = UIPositioning.GetScaledSize(115f); // Base size in reference resolution
            float spacing = buttonSize * choiceSpacing;
            float hoverScale = 1.3f;

            // Normalized center position
            Vector2 normalizedCenter = new Vector2(0.52f, 0.49f);
            normalizedCenter.x += choiceOffsetX * 0.1f;
            normalizedCenter.y += choiceOffsetY * 0.1f;

            Vector2 screenCenter = UIPositioning.NormalizedToScreen(normalizedCenter);

            float totalHeight = (buttonSize * 3) + (spacing * 2);
            float startX = screenCenter.x - buttonSize / 2f;
            float startY = screenCenter.y - totalHeight / 2f;

            string[] choices = { "1", "2", "3" };

            for (int i = 0; i < choices.Length; i++)
            {
                string choice = choices[i];

                float buttonX = startX;
                float buttonY = startY + i * (buttonSize + spacing);

                var transformedPos = bookExtents != null ?
                    ScreenToBoundsSpace(new Vector2(buttonX, buttonY)) :
                    new Vector2(buttonX, buttonY);

                Rect baseRect = new Rect(transformedPos.x, transformedPos.y, buttonSize, buttonSize);
                bool isHover = baseRect.Contains(Event.current.mousePosition);

                float size = isHover ? buttonSize * hoverScale : buttonSize;
                float offset = (size - buttonSize) / 2f;

                Vector2 finalPos = new Vector2(buttonX - offset, buttonY - offset);
                if (bookExtents != null)
                {
                    finalPos = ScreenToBoundsSpace(finalPos);
                }

                Rect scaledRect = new Rect(finalPos.x, finalPos.y, size, size);

                bool isSelected = selectedChoice == choice;
                Color originalColor = GUI.color;
                GUI.color = isSelected ? Color.white : new Color(1f, 1f, 1f, 0.4f);

                GUI.DrawTexture(scaledRect, choiceXTexture, ScaleMode.ScaleToFit);
                GUI.color = originalColor;

                if (GUI.Button(scaledRect, "", choiceButtonStyle))
                {
                    selectedChoice = choice;
                    Debug.Log($"Choice {choice} clicked and selected");
                }
            }
        }


        private void RenderNextButton(float screenW, float screenH)
        {
            float baseSize = UIPositioning.GetScaledSize(115f);
            float hoverScale = 1.2f;

            // Normalized position
            Vector2 normalizedPos = new Vector2(0.90f, 0.84f);
            normalizedPos.x += nextArrowOffsetX * 0.1f;
            normalizedPos.y += nextArrowOffsetY * 0.1f;

            Vector2 screenPos = UIPositioning.NormalizedToScreen(normalizedPos);
            float buttonX = screenPos.x - baseSize;
            float buttonY = screenPos.y;

            var transformedPos = bookExtents != null ?
                ScreenToBoundsSpace(new Vector2(buttonX, buttonY)) :
                new Vector2(buttonX, buttonY);

            Rect baseRect = new Rect(transformedPos.x, transformedPos.y, baseSize, baseSize);
            bool isHover = baseRect.Contains(Event.current.mousePosition);

            float size = isHover ? baseSize * hoverScale : baseSize;
            float offset = (size - baseSize) / 2f;

            Vector2 finalPos = new Vector2(buttonX - offset, buttonY - offset);
            if (bookExtents != null)
            {
                finalPos = ScreenToBoundsSpace(finalPos);
            }

            Rect scaledRect = new Rect(finalPos.x, finalPos.y, size, size);

            GUI.DrawTexture(scaledRect, arrowTexture, ScaleMode.ScaleToFit);

            if (GUI.Button(scaledRect, "", arrowButtonStyle))
            {
                HandleNextButtonClick();
            }
        }


        private void RenderLives(float screenW, float screenH)
        {
            if (heartTexture == null)
            {
                Debug.LogWarning("Heart texture not assigned!");
                return;
            }

            float baseSize = UIPositioning.GetScaledSize(80f); // Adjust this if needed
            float spacing = baseSize * 0.05f;

            // Normalized position for initial heart
            Vector2 normalizedPos = new Vector2(0.235f, 0.85f);
            normalizedPos.x += livesOffsetX * 0.1f;
            normalizedPos.y += livesOffsetY * 0.1f;

            Vector2 screenStartPos = UIPositioning.NormalizedToScreen(normalizedPos);

            // Base pulse speed
            float basePulseSpeed = 2f;
            float pulseSpeed = basePulseSpeed + (5 - gameState.GetPlayerInfo().lives) * 1.5f;
            float pulseScale = 0.05f;

            for (int i = 0; i < gameState.GetPlayerInfo().lives; i++)
            {
                float scale = 1f + Mathf.Sin(Time.time * pulseSpeed + i) * pulseScale;
                float size = baseSize * scale;

                float guiX = screenStartPos.x + i * (baseSize + spacing) - (size - baseSize) / 2f;
                float guiY = screenStartPos.y - (size - baseSize) / 2f;

                if (bookExtents != null)
                {
                    var vec = ScreenToBoundsSpace(new Vector2(guiX, guiY));
                    guiX = vec.x;
                    guiY = vec.y;
                }

                Rect rect = new Rect(guiX, guiY, size, size);
                GUI.DrawTexture(rect, heartTexture, ScaleMode.ScaleToFit);
            }
        }

        private void RenderPlayAgainButton()
        {
            float baseSize = Screen.width * 0.1f; // 10% of screen width
            float hoverScale = 1.2f;

            // Center of the screen
            float buttonX = (Screen.width - baseSize) / 2f;
            float buttonY = (Screen.height - baseSize) / 2f;

            Rect baseRect = new Rect(buttonX, buttonY, baseSize, baseSize);

            bool isHover = baseRect.Contains(Event.current.mousePosition);

            float size = isHover ? baseSize * hoverScale : baseSize;
            float offset = (size - baseSize) / 2f;
            Rect scaledRect = new Rect(buttonX - offset, buttonY - offset, size, size);

            GUI.DrawTexture(scaledRect, arrowTexture, ScaleMode.ScaleToFit);
            if (GUI.Button(scaledRect, "", arrowButtonStyle))
            {
                RestartGame();
            }
        }

        #endregion

        #region Helper Methods

        private IEnumerator CloseBookAfterDelay(float delaySeconds)
        {
            yield return new WaitForSeconds(delaySeconds);
            // Determine shriek based on character name
            string characterNameLower = (gameState.GetCurrentCharacter()?.name ?? "").ToLowerInvariant();

            bool isMale = (characterNameLower == "dracula" || characterNameLower == "dorian gray");
            audioManager.PlayShriek(isMale);
            //yield return new WaitForSeconds(0.1f);
            audioManager.PlayBellChimes();
            yield return new WaitForSeconds(2f);

            Debug.Log("Closing the book because lives reached 0.");

            book.SetState(
                EndlessBook.StateEnum.ClosedFront,
                0.8f,
                OnBookTurnToPageCompleted
            );
        }

        private void HandleFocusTracking(string controlName)
        {
            bool currentlyFocused = GUI.GetNameOfFocusedControl() == controlName;

            if (currentlyFocused && !isInputFocused)
            {
                isInputFocused = true;
                lastFocusedInput = userInput;
                Debug.Log($"{controlName} gained focus");
            }
            else if (!currentlyFocused && isInputFocused)
            {
                isInputFocused = false;
                Debug.Log($"{controlName} lost focus");

                if (lastFocusedInput != userInput)
                {
                    UpdateCurrentDescription();
                }
            }
        }

        private void NavigateToPage(int pageNumber, string pageName)
        {
            isPageTurning = true;
            ResetPageFlags();
            Debug.Log($"{pageName} button clicked");

            if (Mathf.Abs(book.CurrentPageNumber - pageNumber) > 2)
            {
                audioManager.PlayPagesFlipping();
            }

            book.TurnToPage(pageNumber, turnTimeType, turnTime, stateAnimationTime, OnBookTurnToPageCompleted, OnPageTurnStart, OnPageTurnEnd);
        }

        private void CycleCharacter()
        {
            Debug.Log("Cycle Character button clicked");
            gameState.CycleToNextCharacter();
        }

        private void CycleScenario()
        {
            Debug.Log("Cycle Scenario button clicked");
            gameState.CycleToNextScenario();
        }

        private async void HandleNextButtonClick()
        {
            Debug.Log($"Next button clicked. Current page: {book.CurrentPageNumber}");

            UpdateCurrentDescription();

            int currentPage = book.CurrentPageNumber;

            if (currentPage == 1 || currentPage == 3)
            {
                if (runner.IsAvailable)
                {
                    book.TurnToPage(7, turnTimeType, turnTime, stateAnimationTime,
                        OnBookTurnToPageCompleted, OnPageTurnStart, OnPageTurnEnd);
                }
                else
                {
                    book.TurnToPage(currentPage + 2, turnTimeType, turnTime, stateAnimationTime,
                        OnBookTurnToPageCompleted, OnPageTurnStart, OnPageTurnEnd);
                }
                return;
            }
            if (currentPage == 9)
            {
                aiHandler.TriggerGeneration();
            }
            if (currentPage == 11)
            {
                // HandleChoicePage will either turn or not depending on its own logic;
                HandleChoicePage();
                return;
            }
            if (currentPage == 15)
            {
                aiHandler.ClearGenerationState();

            }
            book.TurnToPage(currentPage + 2, turnTimeType, turnTime, stateAnimationTime,
                OnBookTurnToPageCompleted, OnPageTurnStart, OnPageTurnEnd);
        }

        private void HandleChoicePage()
        {
            if (selectedChoice == null)
            {
                Debug.Log("No choice selected - please select a choice before proceeding");
                return;
            }

            if (!aiHandler.Done)
            {
                Debug.Log("Wait for the AI to make a choice");
                choicePageShouldTurn = true;
                return;
            }

            if (hasEvaluatedChoiceThisTurn)
            {
                Debug.Log("Choice already evaluated this turn. Ignoring additional clicks.");
                return;
            }
            hasEvaluatedChoiceThisTurn = true;

            try
            {
                Debug.Log($"Processing choice {selectedChoice} on choices page");
                aiHandler.RenderAssistantChoice(
                    selectedChoice,
                    titleText,
                    explanationText,
                    choiceText,
                    authorText,
                    atmosphereText,
                    lowHealthController,
                    () => {
                        isGameOver = true;
                        audioManager.StopBackgroundMusic();
                        audioManager.PlayFinalHeartbeat();
                        StartCoroutine(CloseBookAfterDelay(3f));
                    }
                );
                book.TurnToPage(book.CurrentPageNumber + 2, turnTimeType, turnTime, stateAnimationTime,
                    OnBookTurnToPageCompleted, OnPageTurnStart, OnPageTurnEnd);
                selectedChoice = null;
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[AI Integration] Failed: " + ex.Message);
            }
        }

        public Vector2 ScreenToBoundsSpace(Vector2 screenPoint)
        {
            if (!matrixValid)
            {
                Debug.LogError("Transform matrix not calculated. Call CalculateScreenToBoundsMatrix first.");
                return Vector2.zero;
            }

            // Apply perspective transformation
            Vector4 homogeneous = new Vector4(screenPoint.x, screenPoint.y, 0, 1);
            Vector4 transformed = screenToBoundsMatrix * homogeneous;

            // Convert back from homogeneous coordinates
            if (Mathf.Abs(transformed.w) < 1e-10)
            {
                Debug.LogError("Invalid transformation result");
                return Vector2.zero;
            }

            return new Vector2(transformed.x / transformed.w, transformed.y / transformed.w);
        }

        private (float guiX, float guiY) ScreenToBookSpace(float x, float y)
        {
            //Relative scale
            var width = Screen.width;
            var height = Screen.height;

            var bookWidth = System.Math.Abs(bookExtents[2].x - bookExtents[0].x);
            var bookHeight = System.Math.Abs(bookExtents[0].y - bookExtents[1].y);

            //For now just base on y
            var relScale = bookWidth / width;
            var centerBook = new Vector2((float)(bookExtents[0].x + bookWidth / 2.0), (float)(bookExtents[1].y + bookHeight / 2.0));
            var centerScreen = new Vector2(width / 2, height / 2);

            //Calculate position offset
            var translation = centerBook - centerScreen;

            // now scale and return input coords
            var outX = x * relScale;
            var outY = y * relScale;
            var finalPos = new Vector2(outX, outY) + translation;
            return (finalPos.x, finalPos.y);


        }

        private Matrix4x4 screenToBoundsMatrix;
        private bool matrixValid = false;
        private void CalculateScreenToBoundsMatrix(Vector3[] screenPoints)
        {
            if (screenPoints.Length < 4)
            {
                Debug.LogError("Need at least 4 screen points to calculate transformation matrix");
                matrixValid = false;
                return;
            }

            Vector2 topLeft = new Vector2(screenPoints[1].x, Screen.height - screenPoints[1].y);
            Vector2 bottomLeft = new Vector2(screenPoints[0].x, Screen.height - screenPoints[0].y);
            Vector2 topRight = new Vector2(screenPoints[3].x, Screen.height - screenPoints[3].y);
            Vector2 bottomRight = new Vector2(screenPoints[2].x, Screen.height - screenPoints[2].y);

            // Calculate the transformation matrix using bilinear interpolation
            // This handles perspective distortion of the quad
            screenToBoundsMatrix = CalculatePerspectiveTransform(
                new Vector2(0, Screen.height), Vector2.zero, new Vector2(Screen.width, 0), new Vector2(Screen.width, Screen.height),
                bottomLeft, topLeft, topRight, bottomRight

            );

            matrixValid = true;
        }

        private Matrix4x4 CalculatePerspectiveTransform(
            Vector2 src0, Vector2 src1, Vector2 src2, Vector2 src3,
            Vector2 dst0, Vector2 dst1, Vector2 dst2, Vector2 dst3)
        {
            // This calculates a perspective transformation matrix
            // From source quad (screen space) to destination quad (bounds space 0-1)

            // Set up the system of equations for perspective transform
            // Using homogeneous coordinates

            float[,] A = new float[8, 8];
            float[] b = new float[8];

            // For each point pair, we get 2 equations
            Vector2[] src = { src0, src1, src2, src3 };
            Vector2[] dst = { dst0, dst1, dst2, dst3 };

            for (int i = 0; i < 4; i++)
            {
                int row = i * 2;

                // X equation
                A[row, 0] = src[i].x;
                A[row, 1] = src[i].y;
                A[row, 2] = 1;
                A[row, 3] = 0;
                A[row, 4] = 0;
                A[row, 5] = 0;
                A[row, 6] = -dst[i].x * src[i].x;
                A[row, 7] = -dst[i].x * src[i].y;
                b[row] = dst[i].x;

                // Y equation
                A[row + 1, 0] = 0;
                A[row + 1, 1] = 0;
                A[row + 1, 2] = 0;
                A[row + 1, 3] = src[i].x;
                A[row + 1, 4] = src[i].y;
                A[row + 1, 5] = 1;
                A[row + 1, 6] = -dst[i].y * src[i].x;
                A[row + 1, 7] = -dst[i].y * src[i].y;
                b[row + 1] = dst[i].y;
            }

            // Solve the system using Gaussian elimination
            float[] h = SolveLinearSystem(A, b);

            if (h == null)
            {
                Debug.LogError("Could not solve transformation matrix");
                return Matrix4x4.identity;
            }

            // Construct the transformation matrix
            Matrix4x4 matrix = new Matrix4x4();
            matrix.m00 = h[0]; matrix.m01 = h[1]; matrix.m02 = 0; matrix.m03 = h[2];
            matrix.m10 = h[3]; matrix.m11 = h[4]; matrix.m12 = 0; matrix.m13 = h[5];
            matrix.m20 = 0; matrix.m21 = 0; matrix.m22 = 1; matrix.m23 = 0;
            matrix.m30 = h[6]; matrix.m31 = h[7]; matrix.m32 = 0; matrix.m33 = 1;

            return matrix;
        }

        private float[] SolveLinearSystem(float[,] A, float[] b)
        {
            int n = b.Length;
            float[] x = new float[n];

            // Gaussian elimination with partial pivoting
            for (int i = 0; i < n; i++)
            {
                // Find pivot
                int maxRow = i;
                for (int k = i + 1; k < n; k++)
                {
                    if (Mathf.Abs(A[k, i]) > Mathf.Abs(A[maxRow, i]))
                        maxRow = k;
                }

                // Swap rows
                for (int k = i; k < n; k++)
                {
                    float temp = A[maxRow, k];
                    A[maxRow, k] = A[i, k];
                    A[i, k] = temp;
                }
                float tempB = b[maxRow];
                b[maxRow] = b[i];
                b[i] = tempB;

                // Check for singular matrix
                if (Mathf.Abs(A[i, i]) < 1e-10)
                {
                    Debug.LogError("Matrix is singular");
                    return null;
                }

                // Eliminate column
                for (int k = i + 1; k < n; k++)
                {
                    float factor = A[k, i] / A[i, i];
                    for (int j = i; j < n; j++)
                    {
                        A[k, j] -= factor * A[i, j];
                    }
                    b[k] -= factor * b[i];
                }
            }

            // Back substitution
            for (int i = n - 1; i >= 0; i--)
            {
                x[i] = b[i];
                for (int j = i + 1; j < n; j++)
                {
                    x[i] -= A[i, j] * x[j];
                }
                x[i] /= A[i, i];
            }

            return x;
        }

        #endregion
    }
}