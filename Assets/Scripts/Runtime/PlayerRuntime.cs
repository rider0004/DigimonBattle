using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRuntime
{
    private Dictionary<SlotType, List<PlayerSlot>> slots;

    private PlayerRuntime()
    {
        slots = new();
        foreach (SlotType s in Constant.SLOT_AMOUNT.Keys)
        {
            List<PlayerSlot> ps = new();
            for (int i = 0; i < Constant.SLOT_AMOUNT[s]; i++)
                ps.Add(PlayerSlot.Create(s));
            slots.Add(s, ps);
        }
    }

    public static PlayerRuntime Create()
    {
        return new();    
    }

    public IReadOnlyList<PlayerSlot> Get(SlotType type)
    {
        return slots[type];        
    }
}

public class PlayerSlot
{
    private DigimonRuntime occupy;
    private SlotType type;

    // ### Train ###
    private TrainData trainData;
    private int currentTrainCount;

    public event Action<TrainData, DigimonRuntime> OnTrainCompleted;
    public event Action<TrainData> OnInterupted;

    private PlayerSlot() {}

    public static PlayerSlot Create(SlotType type)
    {
        return new()
        {
            type = type,
            trainData = null,
            currentTrainCount = 0,
        };
    }

    public void ApplyTrainData(TrainData train)
    {
        trainData = train;
        currentTrainCount = train.Length;   
    }

    public void Train()
    {
        if (trainData == null) return;

        if (occupy == null) RemoveTrainData();

        (IReadOnlyDictionary<Stat, int>, IReadOnlyDictionary<Attribute, int>, IReadOnlyDictionary<Field, int>) info = trainData.GetTrainInfo();
        occupy.Train(info.Item1, false);
        occupy.Shift(info.Item2);
        occupy.Shift(info.Item3);

        currentTrainCount--;
        if (currentTrainCount <= 0)
        {
            (IReadOnlyDictionary<Stat, int>, IReadOnlyDictionary<Attribute, int>, IReadOnlyDictionary<Field, int>) bonus = trainData.GetTrainBonus();
            occupy.Train(bonus.Item1, true);
            occupy.Shift(bonus.Item2);
            occupy.Shift(bonus.Item3);

            TrainData completed = trainData;

            trainData = null;
            currentTrainCount = 0;

            OnTrainCompleted?.Invoke(completed, occupy);
        }
    }

    public void RemoveTrainData()
    {
        TrainData removed = trainData;

        trainData = null;
        currentTrainCount = 0;

        OnInterupted?.Invoke(removed);
    }
}