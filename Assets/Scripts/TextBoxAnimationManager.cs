using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

/// <summary>
/// Matches TextBoxTypeWriter.js:
/// slide-in (0.9s, back.out(1.4)) then character typewriter ({letterDelayMs}).
/// Supports {highlighted} segments → TMP color tags.
/// </summary>
public class TextBoxAnimationManager : MonoBehaviour
{
    private static readonly Regex HighlightRegex = new(
        @"\{([^}]+)\}([.,!?;:]*)",
        RegexOptions.Compiled
    );

    [Header("References")]
    [SerializeField]
    private RectTransform textBoxRoot;

    [SerializeField]
    private CanvasGroup textBoxCanvasGroup;

    [SerializeField]
    private TextMeshProUGUI captionText;

    [Header("Slide")]
    [SerializeField]
    private float textBoxMoveTime = 0.9f;

    [SerializeField]
    private float backOvershoot = 1.4f;

    [Header("Typewriter")]
    [SerializeField]
    private float letterDelayMs = 30f;

    [SerializeField]
    private Color defaultTextColor = Color.white;

    [SerializeField]
    private Color defaultHighlightColor = new(0.91f, 0.77f, 0.28f); // #E8C547

    [SerializeField]
    private List<HighlightColorEntry> highlightColors = new();

    [Header("Demo (optional)")]
    [SerializeField]
    private bool playDemoOnStart;

    [SerializeField]
    [TextArea(2, 4)]
    private string demoCaption =
        "You are outmatched. Abaddon's forces crush you. {Hold the line!}";

    private Vector2 _restAnchoredPosition;
    private Coroutine _activeRoutine;
    private bool _isTyping;
    private bool _typewriterComplete;
    private int _visibleCharCount;
    private int _totalCharCount;
    private string _richText;

    public bool IsTyping => _isTyping;
    public bool IsTypewriterComplete => _typewriterComplete;

    [Serializable]
    public class HighlightColorEntry
    {
        public string key;
        public Color color = new(0.91f, 0.77f, 0.28f);
    }

    private void Awake()
    {
        if (textBoxRoot == null)
            textBoxRoot = GetComponent<RectTransform>();

        if (textBoxCanvasGroup == null && textBoxRoot != null)
            textBoxCanvasGroup = textBoxRoot.GetComponent<CanvasGroup>();

        if (captionText == null && textBoxRoot != null)
            captionText = textBoxRoot.GetComponentInChildren<TextMeshProUGUI>(true);

        _restAnchoredPosition = textBoxRoot.anchoredPosition;
        textBoxRoot.anchoredPosition = GetOffscreenBottomPosition(_restAnchoredPosition);

        if (captionText != null)
        {
            captionText.text = string.Empty;
            captionText.maxVisibleCharacters = 0;
            captionText.color = defaultTextColor;
        }
    }

    private void Start()
    {
        if (playDemoOnStart)
            Play(demoCaption);
    }

    /// <summary>
    /// Slide in from below, then type the caption (JS _playSlideIn → _startTypewriter).
    /// </summary>
    public void Play(string rawCaption)
    {
        if (_activeRoutine != null)
            StopCoroutine(_activeRoutine);

        _activeRoutine = StartCoroutine(PlayRoutine(rawCaption));
    }

    /// <summary>
    /// Like JS click-while-typing: reveal remaining characters immediately.
    /// </summary>
    public void CompleteTypewriter()
    {
        if (!_isTyping || captionText == null)
            return;

        _visibleCharCount = _totalCharCount;
        captionText.maxVisibleCharacters = _totalCharCount;
        _isTyping = false;
        _typewriterComplete = true;
    }

    public void HideInstant()
    {
        if (_activeRoutine != null)
        {
            StopCoroutine(_activeRoutine);
            _activeRoutine = null;
        }

        _isTyping = false;
        _typewriterComplete = false;
        textBoxRoot.anchoredPosition = GetOffscreenBottomPosition(_restAnchoredPosition);

        if (captionText != null)
        {
            captionText.text = string.Empty;
            captionText.maxVisibleCharacters = 0;
        }
    }

    private IEnumerator PlayRoutine(string rawCaption)
    {
        PrepareCaption(rawCaption);

        yield return TextBoxMoveIn();
        yield return TypewriterRoutine();

        _activeRoutine = null;
    }

    private void PrepareCaption(string rawCaption)
    {
        _isTyping = false;
        _typewriterComplete = false;
        _visibleCharCount = 0;
        _richText = BuildRichText(rawCaption ?? string.Empty, out _totalCharCount);

        if (captionText == null)
            return;

        captionText.text = _richText;
        captionText.maxVisibleCharacters = 0;
        captionText.ForceMeshUpdate();
        _totalCharCount = captionText.textInfo.characterCount;
    }

    private IEnumerator TextBoxMoveIn()
    {
        Vector2 end = _restAnchoredPosition;
        Vector2 start = GetOffscreenBottomPosition(end);

        textBoxRoot.anchoredPosition = start;

        float time = 0f;
        while (time < 1f)
        {
            time += Time.deltaTime / textBoxMoveTime;
            float t = Mathf.Clamp01(time);
            float eased = EaseOutBack(t, backOvershoot);
            textBoxRoot.anchoredPosition = Vector2.LerpUnclamped(start, end, eased);
            yield return null;
        }

        textBoxRoot.anchoredPosition = end;
    }

    private IEnumerator TypewriterRoutine()
    {
        if (captionText == null || _totalCharCount <= 0)
        {
            _typewriterComplete = true;
            yield break;
        }

        _isTyping = true;
        _typewriterComplete = false;

        float delay = Mathf.Max(0.001f, letterDelayMs / 1000f);

        // First char immediately (JS tick() before setInterval)
        _visibleCharCount = 1;
        captionText.maxVisibleCharacters = _visibleCharCount;

        while (_visibleCharCount < _totalCharCount)
        {
            yield return new WaitForSeconds(delay);

            if (!_isTyping)
                yield break;

            _visibleCharCount++;
            captionText.maxVisibleCharacters = _visibleCharCount;
        }

        _isTyping = false;
        _typewriterComplete = true;
    }

    private string BuildRichText(string raw, out int plainCharCount)
    {
        raw = raw.Replace("\\n", "\n");
        var sb = new StringBuilder(raw.Length + 32);
        int lastIndex = 0;
        plainCharCount = 0;

        foreach (Match match in HighlightRegex.Matches(raw))
        {
            if (match.Index > lastIndex)
            {
                string plain = raw.Substring(lastIndex, match.Index - lastIndex);
                sb.Append(plain);
                plainCharCount += plain.Length;
            }

            string word = match.Groups[1].Value;
            string trailing = match.Groups[2].Value;
            string highlighted = word + trailing;
            Color color = ResolveHighlightColor(highlighted);
            sb.Append("<color=#");
            sb.Append(ColorUtility.ToHtmlStringRGB(color));
            sb.Append('>');
            sb.Append(highlighted);
            sb.Append("</color>");
            plainCharCount += highlighted.Length;

            lastIndex = match.Index + match.Length;
        }

        if (lastIndex < raw.Length)
        {
            string tail = raw.Substring(lastIndex);
            sb.Append(tail);
            plainCharCount += tail.Length;
        }

        return sb.ToString();
    }

    private Color ResolveHighlightColor(string word)
    {
        if (highlightColors != null)
        {
            for (int i = 0; i < highlightColors.Count; i++)
            {
                var entry = highlightColors[i];
                if (entry == null || string.IsNullOrEmpty(entry.key))
                    continue;

                if (string.Equals(entry.key, word, StringComparison.Ordinal))
                    return entry.color;

                string stripped = word.TrimEnd('.', ',', '!', '?', ';', ':');
                if (string.Equals(entry.key, stripped, StringComparison.Ordinal))
                    return entry.color;
            }

            for (int i = 0; i < highlightColors.Count; i++)
            {
                if (
                    highlightColors[i] != null
                    && string.Equals(highlightColors[i].key, "default", StringComparison.OrdinalIgnoreCase)
                )
                    return highlightColors[i].color;
            }
        }

        return defaultHighlightColor;
    }

    private Vector2 GetOffscreenBottomPosition(Vector2 restAnchoredPosition)
    {
        var parent = textBoxRoot.parent as RectTransform;
        float viewportHeight = parent != null ? parent.rect.height : Screen.height;
        return new Vector2(restAnchoredPosition.x, restAnchoredPosition.y - viewportHeight);
    }

    private static float EaseOutBack(float t, float overshoot)
    {
        t -= 1f;
        return 1f + t * t * ((overshoot + 1f) * t + overshoot);
    }

    private void Reset()
    {
        textBoxRoot = GetComponent<RectTransform>();
        textBoxCanvasGroup = GetComponent<CanvasGroup>();
        captionText = GetComponentInChildren<TextMeshProUGUI>(true);
        textBoxMoveTime = 0.9f;
        backOvershoot = 1.4f;
        letterDelayMs = 30f;
    }
}
