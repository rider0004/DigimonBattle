using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "New Digivolution Index", menuName = "Game/Index/Digivolution Index")]
public class DigivolutionIndex : ScriptableObject
{
    [SerializeField] private List<DigivolutionData> list;
    private Dictionary<string, List<DigivolutionData>> fromIndex;
    private Dictionary<string, List<DigivolutionData>> toIndex;

    public void Init()
    {
        fromIndex = new();
        toIndex = new();

        if (list == null || list.Count == 0) return;

        foreach (DigivolutionData entry in list)
        {
            if (!fromIndex.ContainsKey(entry.From))
                fromIndex.Add(entry.From, new());
            fromIndex[entry.From].Add(entry);

            if (!toIndex.ContainsKey(entry.To))
                toIndex.Add(entry.To, new());
            toIndex[entry.To].Add(entry);
        }
    }

    public IReadOnlyList<DigivolutionData> GetPossibleDigivolution(string fromId)
    {
        List<DigivolutionData> possibleDigivolution = new();
        if (!fromIndex.ContainsKey(fromId)) 
            return possibleDigivolution;
        foreach (DigivolutionData entry in fromIndex[fromId])
            possibleDigivolution.Add(entry);
        return possibleDigivolution;
    }

    public IReadOnlyList<DigivolutionData> GetPossiblePreDigivolution(string toId)
    {
        List<DigivolutionData> possiblePreDigivolution = new();
        if (!toIndex.ContainsKey(toId))
            return possiblePreDigivolution;
        foreach (DigivolutionData entry in toIndex[toId])
            possiblePreDigivolution.Add(entry);
        return possiblePreDigivolution;
    }
}