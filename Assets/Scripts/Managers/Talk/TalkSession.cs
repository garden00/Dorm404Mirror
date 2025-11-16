using System.Collections.Generic;

/// <summary>
/// 현재 진행 중인 대화의 상태를 관리
/// </summary>
public class TalkSession
{
    public bool IsActive { get; private set; }
    public TalkData CurrentData { get; private set; }
    public int CurrentLineIndex { get; private set; }

    public void StartSession(TalkData data)
    {
        CurrentData = data;
        CurrentLineIndex = -1; // NextLine() 호출 시 0이 되도록
        IsActive = true;
    }

    public void EndSession()
    {
        IsActive = false;
        CurrentData = null;
    }

    /// <summary>
    /// 다음 대사로 진행하고, 진행 가능 여부를 반환
    /// </summary>
    public bool NextLine()
    {
        if (!IsActive || CurrentData == null) return false;

        CurrentLineIndex++;
        return CurrentLineIndex < CurrentData.Lines.Count;
    }

    public TalkLine GetCurrentLine()
    {
        if (!IsActive || CurrentLineIndex >= CurrentData.Lines.Count)
        {
            return null;
        }
        return CurrentData.Lines[CurrentLineIndex];
    }
}