using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class DigimonRuntime
{
    private DigimonData digimonData;
    private int currentDigicore;
    private Dictionary<Stat, int> stats;
    private Dictionary<Stat, int> trainedTick;
    private ShiftedParameter<Attribute> attributes;
    private ShiftedParameter<Field> fields;
    private Dictionary<int, DigimonData> lineageDigimon;
    private int eggReturned;
    private int battleCount;
    private int winCount;
    private int trainingCount;
    private int completedTrainingCount;
    private int digivolutionCycle;

    public DigimonData DigimonData => digimonData;
    public int CurrentDigicore => currentDigicore;
    public IReadOnlyDictionary<Stat, int> Stats => stats;
    public IReadOnlyShiftedParameter<Attribute> Attributes => attributes;
    public IReadOnlyShiftedParameter<Field> Fields => fields;
    public IReadOnlyDictionary<int, DigimonData> LineageDigimon;
    public int EggReturned => eggReturned;
    public int BattleCount => battleCount;
    public float WinRate => battleCount > 0 ? (float) winCount / (float) battleCount : 0;
    public int TrainingCount => trainingCount;
    public int CompletedTrainingCount => completedTrainingCount;
    public int DigivolutionCycle => digivolutionCycle;

    public event Action<int, int> OnDigicoreChanged;
    public event Action<IReadOnlyDictionary<Stat, int>> OnTrainedTickChanged;
    public event Action<IReadOnlyDictionary<Attribute, int>> OnAttributesChanged;
    public event Action<IReadOnlyDictionary<Field, int>> OnFieldsChanged;

    private DigimonRuntime() {}

    public void DigicoreChange(int amount)
    {
        currentDigicore = Mathf.Max(0, currentDigicore - amount);
        OnDigicoreChanged?.Invoke(currentDigicore, Constant.DIGICORE_PER_LEVEL[digimonData.Level]);
    }

    public void Train(IReadOnlyDictionary<Stat, int> amounts, bool isComplete)
    {
        ApplyTrainedTick(amounts);
        trainingCount++;
        if (isComplete)
            completedTrainingCount++;
    }

    private void ApplyTrainedTick(IReadOnlyDictionary<Stat, int> amounts)
    {
        int totalTick = trainedTick.Values.Sum();
        if (totalTick >= Constant.MAX_TOTAL_TRAINED_TICK)
            return;
        
        int availableTick = Constant.MAX_TOTAL_TRAINED_TICK - totalTick;
        foreach (Stat s in amounts.Keys)
        {
            int possibleTick = Mathf.Min(Constant.MAX_TRAINED_TICK_PER_STAT - trainedTick[s], amounts[s]);
            trainedTick[s] += possibleTick;
            availableTick = Mathf.Max(0, availableTick - possibleTick);
        }
        OnTrainedTickChanged?.Invoke(trainedTick);
    }

    public void ApplyCycle(SlotType type)
    {
        if ((digimonData.Level < 2 && type == SlotType.Rest) || (digimonData.Level >= 2 && type != SlotType.Rest))
        {
            digivolutionCycle -= 1;
            if (digivolutionCycle <= 0) 
                Digivolution();
        }
    }

    private void RecalculateStat()
    {
        Dictionary<Stat, int> baseStats = new();
        foreach (Stat s in Constant.DIGIMON_STATS)
            baseStats.Add(s, digimonData.Get(s));
        for (int i = digimonData.Level - 1; i >= 2; i--)
            foreach (Stat s in Constant.DIGIMON_STATS)
                baseStats[s] += Mathf.FloorToInt(
                    lineageDigimon[i].Get(s) * Mathf.Pow(Constant.BASE_HERITAGE_BONUS, digimonData.Level - i)
                );
        stats = baseStats;
    }

    public void Digivolution()
    {
        IReadOnlyList<DigivolutionData> possibleDigivolutions = GameManager.Instance
            .DigivolutionIndex.GetPossibleDigivolution(digimonData.DigimonId)
            .Where(d => d.IsArchive(this))
            .ToList();
        
        if (possibleDigivolutions.Count == 0)
        {
            ReturnToEgg();
            return;
        }

        DigivolutionData valid = possibleDigivolutions.Aggregate((min, next) => 
            next.Priority < min.Priority ? next : min);

        digimonData = GameManager.Instance.DigimonIndex.Get(valid.To);
        currentDigicore = Constant.DIGICORE_PER_LEVEL[digimonData.Level];
        
        if (lineageDigimon.ContainsKey(digimonData.Level - 1))
            lineageDigimon[digimonData.Level - 1] = GameManager.Instance.DigimonIndex.Get(valid.From);
        else lineageDigimon.Add(digimonData.Level - 1, GameManager.Instance.DigimonIndex.Get(valid.From));

        RecalculateStat();
        foreach (Attribute a in Constant.DIGIMON_ATTRIBUTES)
            attributes.Add(a, digimonData.Get(a));
        foreach (Field f in Constant.DIGIMON_FIELDS)
            fields.Add(f, digimonData.Get(f));
    }

    private void ReturnToEgg()
    {
        DigimonData validEgg = GameManager.Instance.DigimonIndex.Get(
            GameManager.Instance.DigivolutionIndex
            .GetPossiblePreDigivolution(lineageDigimon[2].DigimonId)
            .Aggregate((min, next) => next.Priority < min.Priority ? next : min)
            .From
        );

        digimonData = validEgg;
        foreach (Stat s in Constant.DIGIMON_STATS)
        {
            stats[s] = 0;
            trainedTick[s] = 0;
        }
        attributes.Reset();
        fields.Reset();

        eggReturned += 1;
        battleCount = 0;
        winCount = 0;
        trainingCount = 0;
        completedTrainingCount = 0;        
    }

    public void Shift(IReadOnlyDictionary<Attribute, int> targets)
    {
        if (attributes.Shift(targets))
            OnAttributesChanged?.Invoke(Attributes.Value);
    }

    public void Shift(IReadOnlyDictionary<Field, int> targets)
    {
        if (fields.Shift(targets))
            OnFieldsChanged?.Invoke(Fields.Value);
    }

    public (int, int) GetStat(Stat stat)
    {
        for (int i = 0; i < Constant.TRAINED_TICK_PER_STAT.Count; i++)
        {
            if (Constant.TRAINED_TICK_PER_STAT[i] > trainedTick[stat])
            {
                return (stats[stat] + i, i);
            }
        }

        return (stats[stat] + 50, 50);
    }

    public override string ToString()
    {
        string t = "";
        t += $"Lv.{digimonData.Level} {digimonData.DisplayName}";

        t += "\n## Stats ##\n";
        foreach (Stat s in Constant.DIGIMON_STATS)
        {
            (int, int) value = GetStat(s);
            t += $"{Enum.GetName(typeof(Stat), s)}: {value.Item1} (+{value.Item2}) ";
        }

        t += "\n## Attributes ##\n";
        foreach (Attribute attribute in Constant.DIGIMON_ATTRIBUTES)
            t += $"{Enum.GetName(typeof(Attribute), attribute)}: {attributes.Value[attribute]} ";
        
        t += "\n## Fields ##\n";
        foreach (Field field in Constant.DIGIMON_FIELDS)
            t += $"{Enum.GetName(typeof(Field), field)}: {fields.Value[field]} ";

        if (digimonData.Level > 2)
        {
            t += "\n## Lineage ##\n";
            for (int i = digimonData.Level - 1; i >= 2; i--)
            {
                t += $"{lineageDigimon[i].DisplayName}";
                if (i > 2)
                    t += $" <- ";
            }
        }

        t += "\n## ETC ##\n"
            + $"EggRevert: {eggReturned} WinRate: {WinRate} ({winCount}/{battleCount}) TrainCount: {trainingCount} ({completedTrainingCount})";
        return t;
    }

    public static DigimonRuntime Convert(string digimonId)
    {
        DigimonRuntime digimonRuntime = new()
        {
            digimonData = GameManager.Instance.DigimonIndex.Get(digimonId),
            lineageDigimon = new(),
            eggReturned = 0,
            battleCount = 0,
            winCount = 0,
            trainingCount = 0,
            completedTrainingCount = 0,
        };

        digimonRuntime.currentDigicore = Constant.DIGICORE_PER_LEVEL[digimonRuntime.digimonData.Level];

        digimonRuntime.trainedTick = new();
        foreach (Stat stat in Constant.DIGIMON_STATS)
        {
            digimonRuntime.trainedTick.Add(stat, 0);   
        }
        Dictionary<Attribute, int> attributes = new();
        foreach (Attribute attribute in Constant.DIGIMON_ATTRIBUTES)
            attributes.Add(attribute, digimonRuntime.digimonData.Get(attribute));
        
        Dictionary<Field, int> fields = new();
        foreach (Field field in Constant.DIGIMON_FIELDS)
            fields.Add(field, digimonRuntime.digimonData.Get(field));
        
        string currentDigimon = digimonId;
        for (int i = digimonRuntime.digimonData.Level - 1; i >= 2; i--)
        {
            IReadOnlyList<string> possibleDigimon = GameManager.Instance
                .DigivolutionIndex.GetPossiblePreDigivolution(currentDigimon)
                .Select(d => d.From)
                .ToList();
            DigimonData selectedData = GameManager.Instance.DigimonIndex.Get(possibleDigimon[UnityEngine.Random.Range(0, possibleDigimon.Count)]);
            digimonRuntime.lineageDigimon.Add(i, selectedData);

            foreach (Attribute attribute in Constant.DIGIMON_ATTRIBUTES)
                attributes[attribute] += selectedData.Get(attribute);
            foreach (Field field in Constant.DIGIMON_FIELDS)
                fields[field] += selectedData.Get(field);

            currentDigimon = selectedData.DigimonId;
        }

        digimonRuntime.RecalculateStat();
        digimonRuntime.attributes = ShiftedParameter<Attribute>.Create(attributes);
        digimonRuntime.fields = ShiftedParameter<Field>.Create(fields);
        digimonRuntime.digivolutionCycle = Constant.DIGIVOLUTION_CYCLE_PER_LEVEL[digimonRuntime.digimonData.Level];

        return digimonRuntime;
    }
}