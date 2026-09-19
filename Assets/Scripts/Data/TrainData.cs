using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Train Data", menuName = "Game/Data/Train Data")]
public class TrainData : ScriptableObject
{
    [SerializeField] private string trainId;
    [SerializeField] private string displayName;
    [SerializeField] private TrainInfo trainInfos;
    [SerializeField] private TrainInfo completedBonus;
    [SerializeField] private int length;

    public string TrainId => trainId;
    public string DisplayName => displayName;
    public int Length => length;

    public (IReadOnlyDictionary<Stat, int>, IReadOnlyDictionary<Attribute, int>, IReadOnlyDictionary<Field, int>) GetTrainInfo()
    {
        Dictionary<Stat, int> stats = new();
        foreach (Stat s in Constant.DIGIMON_STATS)
        {
            int value = completedBonus.Get(s);
            if (value > 0)
                stats.Add(s, value);
        }
        Dictionary<Attribute, int> attributes = new();
        foreach (Attribute a in Constant.DIGIMON_ATTRIBUTES)
        {
            int value = completedBonus.Get(a);
            if (value > 0)
                attributes.Add(a, value);
        }
        Dictionary<Field, int> fields = new();
        foreach (Field f in Constant.DIGIMON_FIELDS)
        {
            int value = completedBonus.Get(f);
            if (value > 0)
                fields.Add(f, value);
        }
        return (stats, attributes, fields);
    }

        public (IReadOnlyDictionary<Stat, int>, IReadOnlyDictionary<Attribute, int>, IReadOnlyDictionary<Field, int>) GetTrainBonus()
    {
        Dictionary<Stat, int> stats = new();
        foreach (Stat s in Constant.DIGIMON_STATS)
        {
            int value = trainInfos.Get(s);
            if (value > 0)
                stats.Add(s, value);
        }
        Dictionary<Attribute, int> attributes = new();
        foreach (Attribute a in Constant.DIGIMON_ATTRIBUTES)
        {
            int value = trainInfos.Get(a);
            if (value > 0)
                attributes.Add(a, value);
        }
        Dictionary<Field, int> fields = new();
        foreach (Field f in Constant.DIGIMON_FIELDS)
        {
            int value = trainInfos.Get(f);
            if (value > 0)
                fields.Add(f, value);
        }
        return (stats, attributes, fields);
    }
}

[System.Serializable]
public class TrainInfo
{
    [Header("Stats")]
    [SerializeField] private int health;
    [SerializeField] private int attack;
    [SerializeField] private int defence;
    [SerializeField] private int intelligence;
    [SerializeField] private int spirit;
    [SerializeField] private int speed;

    [Header("Attributes")]
    [SerializeField] private int vaccine;
    [SerializeField] private int data;
    [SerializeField] private int virus;

    [Header("Fields")]
    [SerializeField] private int cryptid;
    [SerializeField] private int beast;
    [SerializeField] private int aquatic;
    [SerializeField] private int bird;
    [SerializeField] private int machine;
    [SerializeField] private int bug;
    [SerializeField] private int plant;
    [SerializeField] private int seraph;
    [SerializeField] private int demon;

    public int Get(Stat stat)
    {
        return stat switch
        {
            Stat.Hp => health,
            Stat.Atk => attack,
            Stat.Def => defence,
            Stat.Int => intelligence,
            Stat.Spi => spirit,
            Stat.Spd => speed,
            _ => 0,
        };
    }

    public int Get(Attribute attribute)
    {
        return attribute switch
        {
            Attribute.Vaccine => vaccine,
            Attribute.Data => data,
            Attribute.Virus => virus,
            _ => 0,  
        };
    }

    public int Get(Field field)
    {
        return field switch
        {
            Field.Cryptid => cryptid,
            Field.Beast => beast,
            Field.Aquatic => aquatic,
            Field.Bird => bird,
            Field.Machine => machine,
            Field.Bug => bug,
            Field.Plant => plant,
            Field.Seraph => seraph,
            Field.Demon => demon,
            _ => 0
        };
    }
}