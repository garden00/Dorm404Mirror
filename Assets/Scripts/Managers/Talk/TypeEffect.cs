using UnityEngine;
using System.Collections;
using TMPro;
using System;

/// <summary>
/// 텍스트 타이핑 효과(Coroutine 기반)를 구현합니다. (ITypeEffect 구현)
/// </summary>
public class TypeEffect : MonoBehaviour, ITypeEffect
{
    [SerializeField] private float charsPerSecond = 30f;

    private Coroutine _typingCoroutine;
    public bool IsTyping { get; private set; }

    private string _fullText;
    private TMP_Text _currentLabel;
    private Action _onCompleteCallback;

    public void Run(string textToType, TMP_Text textLabel, Action onComplete)
    {
        if (IsTyping)
        {
            StopCoroutine(_typingCoroutine);
        }

        _fullText = textToType;
        _currentLabel = textLabel;
        _onCompleteCallback = onComplete;

        _typingCoroutine = StartCoroutine(TypeTextCoroutine());
    }

    private IEnumerator TypeTextCoroutine()
    {
        IsTyping = true;
        _currentLabel.text = ""; // 텍스트 초기화

        float delay = 1f / charsPerSecond;
        foreach (char c in _fullText)
        {
            _currentLabel.text += c;
            yield return new WaitForSeconds(delay);
        }

        CompleteTyping();
    }

    public void Complete()
    {
        if (!IsTyping) return;

        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }

        CompleteTyping();
    }

    private void CompleteTyping()
    {
        IsTyping = false;
        if (_currentLabel != null)
        {
            _currentLabel.text = _fullText;
        }
        _onCompleteCallback?.Invoke();
    }
}