using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

namespace MathDuel
{
    /// <summary>
    /// Reads player choices from the setup UI, writes them into
    /// <see cref="GameSettings"/>, and loads the game scene.
    /// </summary>
    public class SetupMenuController : MonoBehaviour
    {
        [Header("Selection Groups")]
        public SelectableButtonGroup modeGroup;
        public SelectableButtonGroup rangeGroup;
        public SelectableButtonGroup targetGroup;
        public SelectableButtonGroup timerGroup;

        [Header("Custom Function")]
        public TMP_InputField functionInputField;
        public GameObject functionInputPanel;

        [Header("Scene")]
        public string gameSceneName = "GameScene";

        private readonly int[] _rangeValues  = { 10, 20, 50 };
        private readonly int[] _targetValues = { 5, 10, 15 };
        private readonly float[] _timerValues = { 0f, 5f, 8f };

        private void Update()
        {
            // Show/hide the function input field depending on selected mode
            if (functionInputPanel != null)
            {
                bool showFunc = modeGroup != null &&
                                modeGroup.SelectedIndex == (int)OperationMode.CustomFunction;
                functionInputPanel.SetActive(showFunc);
            }
        }

        /// <summary>
        /// Called by the "BẮT ĐẦU!" button's OnClick event.
        /// </summary>
        public void OnStartButtonPressed()
        {
            // Mode
            GameSettings.Mode = (OperationMode)(modeGroup != null ? modeGroup.SelectedIndex : 0);

            // Range
            int ri = rangeGroup != null ? rangeGroup.SelectedIndex : 0;
            GameSettings.MaxNumber = _rangeValues[Mathf.Clamp(ri, 0, _rangeValues.Length - 1)];

            // Target score
            int ti = targetGroup != null ? targetGroup.SelectedIndex : 0;
            GameSettings.TargetScore = _targetValues[Mathf.Clamp(ti, 0, _targetValues.Length - 1)];

            // Timer
            int tmi = timerGroup != null ? timerGroup.SelectedIndex : 0;
            GameSettings.TimerDuration = _timerValues[Mathf.Clamp(tmi, 0, _timerValues.Length - 1)];

            // Custom function
            if (functionInputField != null && !string.IsNullOrWhiteSpace(functionInputField.text))
                GameSettings.CustomFunction = functionInputField.text;

            SceneManager.LoadScene(gameSceneName);
        }
    }
}
