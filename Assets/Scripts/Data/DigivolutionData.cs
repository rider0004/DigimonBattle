using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Digivolution Data", menuName = "Game/Data/Digivolution Data")]
public class DigivolutionData : ScriptableObject
{
    [SerializeField] private string from;
    [SerializeField] private string to;
    [SerializeField] private int priority;
    [SerializeField] private List<DigimonData> requiredDigimon;

    [Header("General Conditions")]
    [SerializeField] private int eggReturned;
    [SerializeField] private int battleCount;
    [SerializeField] private float winRate;
    [SerializeField] private int trainingCount;
    [SerializeField] private int trainingComplete;

    [Header("Stat Conditions")]
    [SerializeField] private int health;
    [SerializeField] private int attack;
    [SerializeField] private int defence;
    [SerializeField] private int intelligence;
    [SerializeField] private int spirit;
    [SerializeField] private int speed;

    [Header("Attribute Conditions")]
    [SerializeField, Range(0f, 1f)] private float vaccine;
    [SerializeField, Range(0f, 1f)] private float data;
    [SerializeField, Range(0f, 1f)] private float virus;

    [Header("Field Conditions")]
    [SerializeField, Range(0f, 1f)] private float cryptid;
    [SerializeField, Range(0f, 1f)] private float beast;
    [SerializeField, Range(0f, 1f)] private float aquatic;
    [SerializeField, Range(0f, 1f)] private float bird;
    [SerializeField, Range(0f, 1f)] private float machine;
    [SerializeField, Range(0f, 1f)] private float bug;
    [SerializeField, Range(0f, 1f)] private float plant;
    [SerializeField, Range(0f, 1f)] private float seraph;
    [SerializeField, Range(0f, 1f)] private float demon;

    public string From => from;
    public string To => to;
    public int Priority => priority;
    public List<DigimonData> RequiredDigimon => requiredDigimon;

    public int EggReturned => eggReturned;
    public int BattleCount => battleCount;
    public float WinRate => winRate;
    public int TrainingCount => trainingCount;
    public int TrainingComplete => trainingComplete;

    public int Health => health;
    public int Attack => attack;
    public int Defence => defence;
    public int Intelligence => intelligence;
    public int Spirit => spirit;
    public int Speed => speed;

    public float Vaccine => vaccine;
    public float Data => data;
    public float Virus => virus;

    public float Cryptid => cryptid;
    public float Beast => beast;
    public float Aquatic => aquatic;
    public float Bird => bird;
    public float Machine => machine;
    public float Bug => bug;
    public float Plant => plant;
    public float Seraph => seraph;
    public float Demon => demon;

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

    public float Get(Attribute attribute)
    {
        return attribute switch
        {
            Attribute.Vaccine => vaccine,
            Attribute.Data => data,
            Attribute.Virus => virus,
            _ => 0,  
        };
    }

    public float Get(Field field)
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

    public bool IsArchive(DigimonRuntime digimon)
    {
        return Constant.DIGIMON_ATTRIBUTES.All(a => digimon.Attributes.AsPercentage(a) >= Get(a))
            && Constant.DIGIMON_FIELDS.All(f => digimon.Fields.AsPercentage(f) >= Get(f))
            && Constant.DIGIMON_STATS.All(s => digimon.Stats[s] >= Get(s))
            && digimon.EggReturned >= eggReturned
            && digimon.BattleCount >= battleCount
            && digimon.WinRate >= winRate
            && digimon.TrainingCount >= trainingCount
            && digimon.CompletedTrainingCount >= trainingComplete;
    }
}