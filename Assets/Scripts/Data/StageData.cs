using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StageData", menuName = "Game/Stage Data")]
public class StageData : ScriptableObject
{
    [Serializable]
    public class StageEntry
    {
        public int stageIndex;
        [TextArea(2, 5)] public string text;
        public bool showTextBox = true;
    }

    [SerializeField] private List<StageEntry> _stages = new();

    public bool TryGetEntry(int stageIndex, out StageEntry entry)
    {
        for (int i = 0; i < _stages.Count; i++)
        {
            if (_stages[i].stageIndex == stageIndex)
            {
                entry = _stages[i];
                return true;
            }
        }

        entry = null;
        return false;
    }
}
