using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Digimon Data", menuName = "Game/Data/Digimon Data")]
public class DigimonData : ScriptableObject
{
    [Header("Information")]
    [SerializeField] private string digimonId;
    [SerializeField] private string displayName;
    [SerializeField] private int level;
    [SerializeField, TextArea(2, 4)] private string description;
    [SerializeField] private List<Element> elements;
    [SerializeField] private List<Archtype> archtypes;

    [Header("Skills")]
    [SerializeField] private string basicSkill;
    [SerializeField] private string uniqueSkill;

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

    [Header("Display")]
    [SerializeField] private int wide;
    public AnimatorOverrideController overrideController;

    public string DigimonId => digimonId;
    public string DisplayName => displayName;
    public int Level => level;
    public string Description => description;
    public IReadOnlyList<Element> Elements => elements;
    public IReadOnlyList<Archtype> Archtypes => archtypes;

    public string BasicSkill => basicSkill;
    public string UniqueSkill => uniqueSkill;

    public int Health => health;
    public int Attack => attack;
    public int Defence => defence;
    public int Intelligence => intelligence;
    public int Spirit => spirit;
    public int Speed => speed;

    public int Vaccine => vaccine;
    public int Data => data;
    public int Virus => virus;

    public int Cryptid => cryptid;
    public int Beast => beast;
    public int Aquatic => aquatic;
    public int Bird => bird;
    public int Machine => machine;
    public int Bug => bug;
    public int Plant => plant;
    public int Seraph => seraph;
    public int Demon => demon;

    private DigimonData() {}

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