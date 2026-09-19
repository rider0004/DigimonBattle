using UnityEngine;

public class SkillData : ScriptableObject
{
    [SerializeField] private string skillId;
    [SerializeField] private string displayName;
    [SerializeField, TextArea(2, 4)] private string description;
}