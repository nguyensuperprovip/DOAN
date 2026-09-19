using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace MathDuel
{
    /// <summary>
    /// Controls one player's half of the game screen: question display,
    /// answer buttons, score, and optional per-question timer.
    /// </summary>
    public class PlayerPanel : MonoBehaviour
    {
        [Header("UI References")]
        public TMP_Text questionText;
        public Button[] answerButtons;   // length 3
        public TMP_Text[] answerLabels;  // length 3 (children of the buttons)
        public TMP_Text scoreText;

        [Header("Optional Timer")]
        public QuestionTimer timer;

        /// <summary>
        /// Raised when this player scores a point.
        /// The GameManager subscribes to decide if the target has been reached.
        /// </summary>
        public System.Action<PlayerPanel> OnPlayerScored;

        private int _score;
        private QuestionData _current;

        /// <summary>Current score.</summary>
        public int Score => _score;

        // ────────────────────────────────────────────────────────────────
        private void OnEnable()
        {
            for (int i = 0; i < answerButtons.Length; i++)
            {
                int idx = i;
                answerButtons[i].onClick.AddListener(() => OnAnswerClicked(idx));
            }
        }

        private void OnDisable()
        {
            for (int i = 0; i < answerButtons.Length; i++)
                answerButtons[i].onClick.RemoveAllListeners();
        }

        // ────────────────────────────────────────────────────────────────

        /// <summary>Generate and display a new question.</summary>
        public void LoadNewQuestion()
        {
            _current = QuestionGenerator.Generate();

            questionText.text = _current.QuestionText;
            for (int i = 0; i < answerLabels.Length; i++)
                answerLabels[i].text = _current.AllAnswers[i].ToString();

            // Enable buttons
            foreach (var btn in answerButtons) btn.interactable = true;

            // Start timer if configured
            if (timer != null && GameSettings.TimerDuration > 0f)
            {
                timer.onTimeUp = HandleTimeUp;
                timer.StartTimer(GameSettings.TimerDuration);
            }
        }

        /// <summary>Reset score to 0 and refresh display.</summary>
        public void ResetScore()
        {
            _score = 0;
            UpdateScoreDisplay();
        }

        // ────────────────────────────────────────────────────────────────

        private void OnAnswerClicked(int index)
        {
            if (_current == null) return;

            // Stop timer
            if (timer != null) timer.StopTimer();

            if (_current.AllAnswers[index] == _current.CorrectAnswer)
            {
                _score++;
                UpdateScoreDisplay();
                OnPlayerScored?.Invoke(this);
            }

            // Load next question regardless of correct/wrong
            LoadNewQuestion();
        }

        /// <summary>
        /// Called when time runs out. Skips the question without scoring.
        /// Override-friendly: change to _score-- if you want to penalize.
        /// </summary>
        private void HandleTimeUp()
        {
            LoadNewQuestion();
        }

        private void UpdateScoreDisplay()
        {
            if (scoreText != null)
                scoreText.text = _score.ToString();
        }
    }
}
