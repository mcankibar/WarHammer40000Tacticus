using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject _textBoxRoot;
    [SerializeField] private TextMeshProUGUI _textMeshProUGUI;

    void OnEnable()
    {
        GameManager.OnDialogueChanged += HandleDialogueChanged;
    }

    void OnDisable()
    {
        GameManager.OnDialogueChanged -= HandleDialogueChanged;
    }

    void HandleDialogueChanged(string text, bool show)
    {
        if (!show || string.IsNullOrEmpty(text))
        {
            HideTextBox();
            return;
        }

        ShowTextBox(text);
    }

    void ShowTextBox(string text)
    {
        if (_textMeshProUGUI != null)
            _textMeshProUGUI.text = text;

        if (_textBoxRoot != null)
            _textBoxRoot.SetActive(true);
    }

    void HideTextBox()
    {
        if (_textBoxRoot != null)
            _textBoxRoot.SetActive(false);
    }
}
