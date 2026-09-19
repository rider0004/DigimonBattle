#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

public class DigimonCSVImporter : EditorWindow
{
    private TextAsset csvFile;
    private AnimatorController level2MasterController;
    private AnimatorController level3MasterController;
    private string targetFolder = "Assets/Data/Digimon";
    private string spritesBaseFolder = "Assets/Sprites/Monsters";

    [MenuItem("Tools/Import Digimon CSV")]
    public static void ShowWindow()
    {
        GetWindow<DigimonCSVImporter>("Digimon CSV Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Digimon CSV & Animation Importer", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);
        
        EditorGUILayout.Space();
        GUILayout.Label("Base Master Controllers", EditorStyles.boldLabel);
        level2MasterController = (AnimatorController)EditorGUILayout.ObjectField("Level 2 Base Controller (3 Anims)", level2MasterController, typeof(AnimatorController), false);
        level3MasterController = (AnimatorController)EditorGUILayout.ObjectField("Level 3+ Base Controller (7 Anims)", level3MasterController, typeof(AnimatorController), false);

        EditorGUILayout.Space();
        targetFolder = EditorGUILayout.TextField("Data Output Folder", targetFolder);
        spritesBaseFolder = EditorGUILayout.TextField("Sprites Base Folder", spritesBaseFolder);

        EditorGUILayout.Space();
        if (GUILayout.Button("Generate ScriptableObjects & Animations", GUILayout.Height(30)))
        {
            if (csvFile == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a CSV file.", "OK");
                return;
            }
            if (level2MasterController == null || level3MasterController == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign both Level 2 and Level 3+ Base Animator Controllers.", "OK");
                return;
            }

            ImportCSV();
        }
    }

    private void ImportCSV()
    {
        if (!Directory.Exists(targetFolder))
        {
            Directory.CreateDirectory(targetFolder);
        }

        string[] lines = csvFile.text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1) return;

        int createdCount = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            List<string> row = ParseCSVRow(lines[i]);
            if (row.Count < 26 || string.IsNullOrWhiteSpace(row[0])) continue;

            string digimonId = row[0].Trim();
            string assetPath = $"{targetFolder}/{digimonId}.asset";

            DigimonData data = AssetDatabase.LoadAssetAtPath<DigimonData>(assetPath);
            bool isNew = data == null;

            if (isNew)
            {
                data = ScriptableObject.CreateInstance<DigimonData>();
            }

            SerializedObject serializedObject = new SerializedObject(data);

            // Information
            serializedObject.FindProperty("digimonId").stringValue = row[0];
            serializedObject.FindProperty("displayName").stringValue = row[1];
            int monsterLevel = ParseInt(row[2]);
            serializedObject.FindProperty("level").intValue = monsterLevel;
            ParseEnumList<Element>(serializedObject.FindProperty("elements"), row[3]);
            ParseEnumList<Archtype>(serializedObject.FindProperty("archtypes"), row[4]);

            // Skills
            serializedObject.FindProperty("basicSkill").stringValue = row[5];
            serializedObject.FindProperty("uniqueSkill").stringValue = row[6];

            // Base Stats
            serializedObject.FindProperty("health").intValue = ParseInt(row[7]);
            serializedObject.FindProperty("attack").intValue = ParseInt(row[8]);
            serializedObject.FindProperty("defence").intValue = ParseInt(row[9]);
            serializedObject.FindProperty("intelligence").intValue = ParseInt(row[10]);
            serializedObject.FindProperty("spirit").intValue = ParseInt(row[11]);
            serializedObject.FindProperty("speed").intValue = ParseInt(row[12]);

            // Attributes
            serializedObject.FindProperty("vaccine").intValue = ParseInt(row[13]);
            serializedObject.FindProperty("data").intValue = ParseInt(row[14]);
            serializedObject.FindProperty("virus").intValue = ParseInt(row[15]);

            // Fields
            serializedObject.FindProperty("cryptid").intValue = ParseInt(row[16]);
            serializedObject.FindProperty("beast").intValue = ParseInt(row[17]);
            serializedObject.FindProperty("aquatic").intValue = ParseInt(row[18]);
            serializedObject.FindProperty("bird").intValue = ParseInt(row[19]);
            serializedObject.FindProperty("machine").intValue = ParseInt(row[20]);
            serializedObject.FindProperty("bug").intValue = ParseInt(row[21]);
            serializedObject.FindProperty("plant").intValue = ParseInt(row[22]);
            serializedObject.FindProperty("seraph").intValue = ParseInt(row[23]);
            serializedObject.FindProperty("demon").intValue = ParseInt(row[24]);

            // Sprite Dimensions & Descriptions
            serializedObject.FindProperty("wide").intValue = ParseInt(row[26]);
            serializedObject.FindProperty("description").stringValue = row.Count > 25 ? row[25] : "";

            // --- SELECT BASE MASTER CONTROLLER ACCORDING TO LEVEL ---
            AnimatorController targetBaseController = (monsterLevel <= 2) ? level2MasterController : level3MasterController;

            // --- AUTOMATIC ANIMATOR OVERRIDE CONTROLLER GENERATION ---
            string monsterFolder = $"{spritesBaseFolder}/{digimonId}";
            string aocPath = $"{monsterFolder}/AOC_{digimonId}.overrideController";

            AnimatorOverrideController aoc = AssetDatabase.LoadAssetAtPath<AnimatorOverrideController>(aocPath);

            if (aoc == null && Directory.Exists(monsterFolder))
            {
                aoc = new AnimatorOverrideController(targetBaseController);
                AssetDatabase.CreateAsset(aoc, aocPath);
            }
            else if (aoc != null)
            {
                // Ensure existing AOC uses the correct base controller for its level
                aoc.runtimeAnimatorController = targetBaseController;
            }

            if (aoc != null)
            {
                AnimationClip idle = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{monsterFolder}/{digimonId}_Idle.anim");
                AnimationClip win = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{monsterFolder}/{digimonId}_Win.anim");
                AnimationClip loss = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{monsterFolder}/{digimonId}_Loss.anim");

                AnimationClip walk = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{monsterFolder}/{digimonId}_Walk.anim");
                AnimationClip run = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{monsterFolder}/{digimonId}_Run.anim");
                AnimationClip training = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{monsterFolder}/{digimonId}_Training.anim");
                AnimationClip attack = AssetDatabase.LoadAssetAtPath<AnimationClip>($"{monsterFolder}/{digimonId}_Attack.anim");

                List<KeyValuePair<AnimationClip, AnimationClip>> overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                aoc.GetOverrides(overrides);

                List<KeyValuePair<AnimationClip, AnimationClip>> updatedOverrides = new List<KeyValuePair<AnimationClip, AnimationClip>>();

                foreach (var pair in overrides)
                {
                    string clipName = pair.Key.name;
                    AnimationClip targetClip = pair.Value;

                    if (clipName.Contains("Idle") && idle != null) targetClip = idle;
                    else if (clipName.Contains("Win") && win != null) targetClip = win;
                    else if (clipName.Contains("Loss") && loss != null) targetClip = loss;
                    else if (clipName.Contains("Walk") && walk != null) targetClip = walk;
                    else if (clipName.Contains("Run") && run != null) targetClip = run;
                    else if (clipName.Contains("Training") && training != null) targetClip = training;
                    else if (clipName.Contains("Attack") && attack != null) targetClip = attack;

                    updatedOverrides.Add(new KeyValuePair<AnimationClip, AnimationClip>(pair.Key, targetClip));
                }

                aoc.ApplyOverrides(updatedOverrides);
                EditorUtility.SetDirty(aoc);

                // Assign AOC to DigimonData property
                SerializedProperty aocProperty = serializedObject.FindProperty("overrideController");
                if (aocProperty != null)
                {
                    aocProperty.objectReferenceValue = aoc;
                }
            }

            serializedObject.ApplyModifiedProperties();

            if (isNew)
            {
                AssetDatabase.CreateAsset(data, assetPath);
            }
            else
            {
                EditorUtility.SetDirty(data);
            }

            createdCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Import Complete", $"Successfully created/updated {createdCount} Digimon ScriptableObjects and animations.", "OK");
    }

    private static int ParseInt(string input)
    {
        int.TryParse(input, out int result);
        return result;
    }

    private static List<string> ParseCSVRow(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        string current = "";

        foreach (char c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.Trim());
                current = "";
            }
            else
            {
                current += c;
            }
        }
        result.Add(current.Trim());
        return result;
    }

    private static void ParseEnumList<T>(SerializedProperty listProp, string raw) where T : struct, Enum
    {
        listProp.ClearArray();
        string clean = raw.Replace("[", "").Replace("]", "").Replace("\"", "").Trim();
        if (string.IsNullOrEmpty(clean)) return;

        string[] items = clean.Split(',');
        int arrayIndex = 0;

        foreach (string rawItem in items)
        {
            string itemStr = rawItem.Trim();
            if (string.IsNullOrEmpty(itemStr)) continue;

            if (Enum.TryParse<T>(itemStr, true, out T parsedEnum))
            {
                listProp.InsertArrayElementAtIndex(arrayIndex);
                SerializedProperty elementProp = listProp.GetArrayElementAtIndex(arrayIndex);
                
                int enumIndex = Array.FindIndex(elementProp.enumNames, name => name.Equals(parsedEnum.ToString(), StringComparison.OrdinalIgnoreCase));
                if (enumIndex >= 0)
                {
                    elementProp.enumValueIndex = enumIndex;
                }
                arrayIndex++;
            }
        }
    }
}
#endif