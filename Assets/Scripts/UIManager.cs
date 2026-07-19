using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _textBoxRoot;

    [SerializeField]
    private TextBoxAnimationManager _textBoxAnimationManager;

    void Awake()
    {
        if (_textBoxAnimationManager == null && _textBoxRoot != null)
            _textBoxAnimationManager = _textBoxRoot.GetComponent<TextBoxAnimationManager>();
    }

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
        if (_textBoxRoot != null)
            _textBoxRoot.SetActive(true);

        if (_textBoxAnimationManager != null)
            _textBoxAnimationManager.Play(text);
    }

    void HideTextBox()
    {
        if (_textBoxAnimationManager != null)
            _textBoxAnimationManager.HideInstant();

        if (_textBoxRoot != null)
            _textBoxRoot.SetActive(false);
    }
}
