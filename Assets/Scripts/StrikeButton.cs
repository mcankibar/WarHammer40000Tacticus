using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// Put on the StrikeButton parent (has CanvasGroup).
/// Child Button is wired in code — no Inspector OnClick required.
/// </summary>
public class StrikeButton : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Button _button;
    [SerializeField] private float _fadeDuration = 0.35f;
    [SerializeField] private UnityEvent _onStrike;

    private Coroutine _fadeRoutine;
    private bool _isFading;

    void Awake()
    {
        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();

        if (_button == null)
            _button = GetComponentInChildren<Button>(true);

        if (_button == null)
            return;

        // Avoid Color Tint looking like a "slight fade".
        _button.transition = Selectable.Transition.None;

        // Runtime bind so Play Mode / lost Inspector wiring can't break this.
        _button.onClick.RemoveListener(OnClicked);
        _button.onClick.AddListener(OnClicked);
    }

    void OnDestroy()
    {
        if (_button != null)
            _button.onClick.RemoveListener(OnClicked);
    }

    public void OnClicked()
    {
        if (_isFading)
            return;

        if (_button != null)
            _button.interactable = false;

        if (_fadeRoutine != null)
            StopCoroutine(_fadeRoutine);

        _fadeRoutine = StartCoroutine(FadeOutAndRaise());
    }

    IEnumerator FadeOutAndRaise()
    {
        _isFading = true;

        if (_canvasGroup == null)
            _canvasGroup = GetComponent<CanvasGroup>();

        if (_canvasGroup == null)
        {
            _onStrike?.Invoke();
            _isFading = false;
            yield break;
        }

        float startAlpha = _canvasGroup.alpha;
        float duration = Mathf.Max(0.01f, _fadeDuration);
        float time = 0f;

        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.interactable = false;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            _canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }

        _canvasGroup.alpha = 0f;
        _onStrike?.Invoke();
        _isFading = false;
        _fadeRoutine = null;
    }
}
