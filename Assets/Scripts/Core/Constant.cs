using System.Collections.Generic;
using System.Linq;

public static class Constant
{
    public static readonly IReadOnlyDictionary<int, int> DIGICORE_PER_LEVEL = new Dictionary<int, int>()
    {
        { 2, 1800 },
        { 3, 2160 },
        { 4, 2700 },
        { 5, 3600 },
        { 6, 5040 },
        { 7, 7200 },  
    };
    
    public static readonly IReadOnlyDictionary<int, int> DIGIVOLUTION_CYCLE_PER_LEVEL = new Dictionary<int, int>()
    {
        { 0, 5 },
        { 2, 5 },
        { 3, 10 },
        { 4, 15 },
        { 5, 20 },
        { 6, 20 },
        { 7, 20 },  
    };

    public static readonly IReadOnlyList<int> TRAINED_TICK_PER_STAT = new List<int>()
    {
        1, 3, 5, 8, 11, 14, 18, 22, 26, 30,
        35, 40, 45, 50, 55, 61, 67, 73, 79, 85,
        91, 98, 105, 112, 119, 126, 133, 140, 148, 156,
        164, 172, 180, 188, 196, 204, 213, 222, 231, 240,
        249, 258, 267, 276, 285, 295, 305, 315, 325, 335
    };

    public static readonly int MAX_TOTAL_TRAINED_TICK = 360;
    
    public static readonly int MAX_TRAINED_TICK_PER_STAT = 335;

    public static readonly IReadOnlyList<Stat> DIGIMON_STATS = new List<Stat>()
    {
        Stat.Hp, Stat.Atk, Stat.Def, Stat.Int, Stat.Spi, Stat.Spd,  
    };

    public static readonly IReadOnlyList<Attribute> DIGIMON_ATTRIBUTES = new List<Attribute>()
    {
        Attribute.Vaccine, Attribute.Data, Attribute.Virus,
    };

    public static readonly IReadOnlyList<Field> DIGIMON_FIELDS = new List<Field>()
    {
        Field.Cryptid, Field.Beast, Field.Aquatic, Field.Bird, Field.Machine, Field.Bug, Field.Plant, Field.Seraph, Field.Demon,
    };

    public static readonly float BASE_HERITAGE_BONUS = 0.16f;

    public static readonly IReadOnlyDictionary<SlotType, int> SLOT_AMOUNT = new Dictionary<SlotType, int>()
    {
        { SlotType.Front, 4 },
        { SlotType.Back, 4 },
        { SlotType.Train, 4 },
        { SlotType.Rest, 4 },  
    };

    public static readonly int BATTLE_SLOT_CAPACITY = 5;
}