using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // TODO: Publish other events for stage changes
    public static event Action<int> OnStageChanged;
    public static event Action<string, bool> OnDialogueChanged;

    [SerializeField] private StageData _stageData;
    [SerializeField] private int _startingStage;

    public int CurrentStage { get; private set; }

    void Start()
    {
        SetStage(_startingStage);
    }

    public void SetStage(int stageIndex)
    {
        CurrentStage = stageIndex;
        OnStageChanged?.Invoke(CurrentStage);
        PublishDialogue();
    }

    public void NextStage()
    {
        SetStage(CurrentStage + 1);
    }

    void PublishDialogue()
    {
        if (_stageData != null && _stageData.TryGetEntry(CurrentStage, out var entry))
        {
            OnDialogueChanged?.Invoke(entry.text, entry.showTextBox);
            return;
        }

        OnDialogueChanged?.Invoke(null, false);
    }
}
