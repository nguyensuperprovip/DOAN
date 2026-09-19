using UnityEngine;
using UnityEngine.UI;

namespace MathDuel
{
    /// <summary>
    /// Counts down per-question time and visually fills/unfills a ring image.
    /// Attach to a child of the player panel; drag the filled Image here.
    /// </summary>
    public class QuestionTimer : MonoBehaviour
    {
        [Tooltip("UI Image set to Image Type = Filled (radial 360).")]
        public Image fillImage;

        /// <summary>Invoked when time runs out for the current question.</summary>
        public System.Action onTimeUp;

        private float _duration;
        private float _remaining;
        private bool _running;

        /// <summary>Start (or restart) the countdown.</summary>
        public void StartTimer(float duration)
        {
            _duration = duration;
            _remaining = duration;
            _running = duration > 0f;
            UpdateFill();
        }

        /// <summary>Stop the countdown without triggering the callback.</summary>
        public void StopTimer()
        {
            _running = false;
        }

        private void Update()
        {
            if (!_running) return;

            _remaining -= Time.deltaTime;
            UpdateFill();

            if (_remaining <= 0f)
            {
                _running = false;
                _remaining = 0f;
                UpdateFill();
                onTimeUp?.Invoke();
            }
        }

        private void UpdateFill()
        {
            if (fillImage != null && _duration > 0f)
                fillImage.fillAmount = Mathf.Clamp01(_remaining / _duration);
        }
    }
}
