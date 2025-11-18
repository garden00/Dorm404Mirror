using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


// 데이터 접근 계층
public interface ITalkRepository
{
    /// <summary>
    /// Scene ID에 맞는 대화 데이터를 로드
    /// </summary>
    TalkData LoadTalkData(int sceneId);
}

// 뷰 계층
public interface IDialogueView
{
    // TextMeshProUGUI 컴포넌트를 직접 노출 (TypeEffect가 사용)
    TMPro.TMP_Text DialogueTextComponent { get; }

    void Show();
    void Hide();
    void SetSpeakerName(string name);
    void SetPortrait(Sprite portrait);
    void UpdateText(string text); // TypeEffect가 아닌 즉시 텍스트 설정용
}

// 뷰 로직 계층
public interface ITypeEffect
{
    bool IsTyping { get; }

    /// <summary>
    /// 텍스트에 타이핑 효과를 적용합니다.
    /// </summary>
    void Run(string textToType, TMP_Text textLabel, Action onComplete);

    /// <summary>
    /// 현재 진행 중인 타이핑을 즉시 완료합니다.
    /// </summary>
    void Complete();
}

// 리소스 제공 계층
public interface IPortraitProvider
{
    /// <summary>
    /// 스피커 이름과 표정 ID로 적절한 Sprite를 반환합니다.
    /// </summary>
    Sprite GetPortrait(string speakerName, int expressionId);
}

/// <summary>
/// 대화 시스템의 전체 흐름을 제어하는 중앙 컨트롤러
/// </summary>
public class TalkManager : MonoBehaviour
{
    #region Singleton

    public static TalkManager Instance { get; private set; }
    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;


        // 주입된 컴포넌트에서 인터페이스를 가져옵니다.
        _repository = repositoryProvider as ITalkRepository;
        _view = viewProvider as IDialogueView;
        _typer = typeEffectProvider as ITypeEffect;
        //_portraits = portraitProvider as IPortraitProvider;

        _session = new TalkSession();

        // null 체크
        if (_repository == null) Debug.LogError("ITalkRepository is not assigned or invalid.");
        if (_view == null) Debug.LogError("IDialogueView is not assigned or invalid.");
        if (_typer == null) Debug.LogError("ITypeEffect is not assigned or invalid.");
        //if (_portraits == null) Debug.LogError("IPortraitProvider is not assigned or invalid.");
    }

    #endregion


    // [SerializeField]를 사용해 Unity Inspector에서 인터페이스 구현체를 주입합니다. (Unity식 DIP)
    [SerializeField] private MonoBehaviour repositoryProvider;
    [SerializeField] private MonoBehaviour viewProvider;
    [SerializeField] private MonoBehaviour typeEffectProvider;
    [SerializeField] private MonoBehaviour portraitProvider;

    // 실제 사용할 인터페이스 참조
    private ITalkRepository _repository;
    private IDialogueView _view;
    private ITypeEffect _typer;
    //private IPortraitProvider _portraits;

    private TalkSession _session;

    public event Action OnTalkStarted;
    public event Action OnTalkEnded;

    // (예시) 플레이어 입력 처리 (Space 키)
    void Update()
    {
        // (참고: 실제 프로젝트에서는 새 Input System의 .performed 이벤트를 사용하는 것이 좋습니다)
        if (_session.IsActive && Input.GetKeyDown(KeyCode.Space))
        {
            OnNextLineInput();
        }
    }

    /// <summary>
    /// 지정된 Scene ID로 대화를 시작합니다.
    /// </summary>
    public void StartDialogue(int sceneId)
    {
        if (_session.IsActive) return; // 이미 대화 중

        TalkData data = _repository.LoadTalkData(sceneId);
        if (data == null || data.Lines.Count == 0)
        {
            Debug.LogWarning($"No talk data found for sceneId: {sceneId}");
            return;
        }

        _session.StartSession(data);
        _view.Show();
        ShowNextLine();

        OnTalkStarted?.Invoke();
    }

    /// <summary>
    /// (UI 버튼이나 입력 시스템에서 호출) 다음 대사로 진행합니다.
    /// </summary>
    public void OnNextLineInput()
    {
        if (!_session.IsActive) return;

        // 1. 타이핑 중이면, 타이핑 즉시 완료
        if (_typer.IsTyping)
        {
            _typer.Complete();
        }
        // 2. 타이핑 완료 상태면, 다음 대사 진행
        else
        {
            ShowNextLine();
        }
    }

    private void ShowNextLine()
    {
        if (_session.NextLine())
        {
            // 다음 대사가 있음
            TalkLine currentLine = _session.GetCurrentLine();
            DisplayLine(currentLine);
        }
        else
        {
            // 대사 종료
            EndDialogue();
        }
    }

    private void DisplayLine(TalkLine line)
    {
        // 1. 화자 이름 설정
        _view.SetSpeakerName(line.Speaker);

        // 2. 초상화 설정
        //Sprite portrait = _portraits.GetPortrait(line.Speaker, line.PortraitId);
        //_view.SetPortrait(portrait);

        // 3. 텍스트 효과 적용 (ITypeEffect에 위임)
        _typer.Run(line.Text, _view.DialogueTextComponent, OnTypingComplete);
    }

    private void OnTypingComplete()
    {
        // 타이핑 완료 시 처리 (예: '다음' 화살표 표시)
        // nextIndicator.SetActive(true);
    }

    private void EndDialogue()
    {
        _session.EndSession();
        _view.Hide();
        Debug.Log("Dialogue ended.");

        OnTalkEnded?.Invoke();
    }
}