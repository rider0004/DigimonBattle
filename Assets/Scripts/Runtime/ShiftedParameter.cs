using System;
using System.Collections.Generic;
using System.Linq;

public interface IReadOnlyShiftedParameter<T> where T : struct, Enum
{
    IReadOnlyDictionary<T, int> Value { get; }
    float AsPercentage(T key);
}

public class ShiftedParameter<T> : IReadOnlyShiftedParameter<T> where T : struct, Enum
{
    private Dictionary<T, int> value = new();
    private HashSet<T> recentlyReduced = new();

    public IReadOnlyDictionary<T, int> Value => value;


    private ShiftedParameter(Dictionary<T, int> initialValue)
    {
        foreach (T key in initialValue.Keys)
            value[key] = initialValue != null && initialValue.TryGetValue(key, out int v) ? v : 0;
    }

    public static ShiftedParameter<T> Create(Dictionary<T, int> initialValue)
    {
        return new(initialValue);
    }

    public void Add(T key, int value)
    {
        this.value[key] += value;
    }

    public void Reset()
    {
        recentlyReduced = new();
        foreach (T key in value.Keys.ToList())
            value[key] = 0;
    }

    public float AsPercentage(T key)
    {
        return ((float) value[key]) / ((float) value.Values.Sum());
    }

    public bool Shift(IReadOnlyDictionary<T, int> targetIncreases)
    {
        Dictionary<T, int> validTargets = targetIncreases
            .Where(kvp => kvp.Value > 0)
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        
        if (!validTargets.Any()) return false;

        HashSet<T> targetKeys = validTargets.Keys.ToHashSet();
        List<T> nonTargets = value.Keys.Where(k => !targetKeys.Contains(k)).ToList();

        if (!nonTargets.Any()) return false;

        int totalShift = validTargets.Values.Sum();

        for (int i = 0; i < totalShift; i++)
        {
            List<T> eligibleCandidates = nonTargets
                .Where(k => value[k] > 0)
                .ToList();
            if (!eligibleCandidates.Any()) return false;

            List<T> unreducedCandidates = eligibleCandidates
                .Where(k => !recentlyReduced.Contains(k))
                .ToList();
            if (!unreducedCandidates.Any())
            {
                recentlyReduced.Clear();
                unreducedCandidates = eligibleCandidates;
            }

            int maxValue = unreducedCandidates.Max(k => value[k]);
            List<T> tiedByValue = unreducedCandidates
                .Where(k => value[k] == maxValue)
                .ToList();
            T toReduce = tiedByValue[UnityEngine.Random.Range(0, tiedByValue.Count)];

            value[toReduce]--;
            recentlyReduced.Add(toReduce);
        }

        foreach (T key in validTargets.Keys)
        {
            value[key] += validTargets[key];
        }

        return true;
    }
}