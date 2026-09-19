using UnityEngine;
using UnityEngine.UI;

namespace MathDuel
{
    /// <summary>
    /// Manages a group of UI Buttons where only one can be "selected" at a time.
    /// Attach to the parent Panel and drag the child Buttons into the list.
    /// The selected button is highlighted with a distinct color.
    /// </summary>
    public class SelectableButtonGroup : MonoBehaviour
    {
        [Tooltip("Drag the child buttons here in order (index 0, 1, 2, …).")]
        public Button[] Buttons;

        [Header("Colors")]
        public Color normalColor   = new Color(0.25f, 0.25f, 0.25f, 1f);
        public Color selectedColor = new Color(0.2f, 0.6f, 1f, 1f);

        private int _selectedIndex = 0;

        /// <summary>Currently selected index (0-based).</summary>
        public int SelectedIndex => _selectedIndex;

        private void Start()
        {
            for (int i = 0; i < Buttons.Length; i++)
            {
                int index = i; // closure capture
                Buttons[i].onClick.AddListener(() => Select(index));
            }
            UpdateVisuals();
        }

        /// <summary>Programmatically select a button by index.</summary>
        public void Select(int index)
        {
            _selectedIndex = Mathf.Clamp(index, 0, Buttons.Length - 1);
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            for (int i = 0; i < Buttons.Length; i++)
            {
                ColorBlock cb = Buttons[i].colors;
                cb.normalColor = (i == _selectedIndex) ? selectedColor : normalColor;
                cb.selectedColor = cb.normalColor;
                cb.highlightedColor = cb.normalColor * 1.1f;
                Buttons[i].colors = cb;
            }
        }
    }
}
