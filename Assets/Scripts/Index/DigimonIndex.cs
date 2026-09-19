using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Digimon Index", menuName = "Game/Index/Digimon Index")]
public class DigimonIndex : ScriptableObject
{
    [SerializeField] private List<DigimonData> list;
    private Dictionary<string, DigimonData> index;

    public void Init()
    {
        index = new();

        if (list == null || list.Count == 0) return;

        foreach (DigimonData entry in list)
        {
            index.Add(entry.DigimonId, entry);
        }
    }

    public DigimonData Get(string digimonId)
    {
        if (!index.ContainsKey(digimonId))
            return null;
        
        return index[digimonId];
    }
}