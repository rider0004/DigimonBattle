#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class DigiEggCSVImporter : EditorWindow
{
    private TextAsset csvFile;
    private AnimatorOverrideController sharedEggAOC;
    private string digiEggFolder = "Assets/Data/DigiEgg";
    private string digivolutionFolder = "Assets/Data/Digivolution";

    [MenuItem("Tools/Import DigiEgg CSV")]
    public static void ShowWindow()
    {
        GetWindow<DigiEggCSVImporter>("DigiEgg CSV Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("DigiEgg & Digivolution CSV Importer", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);
        sharedEggAOC = (AnimatorOverrideController)EditorGUILayout.ObjectField("Shared Egg AOC", sharedEggAOC, typeof(AnimatorOverrideController), false);

        EditorGUILayout.Space();
        digiEggFolder = EditorGUILayout.TextField("DigiEgg Output Folder", digiEggFolder);
        digivolutionFolder = EditorGUILayout.TextField("Digivolution Output Folder", digivolutionFolder);

        EditorGUILayout.Space();
        if (GUILayout.Button("Generate ScriptableObjects", GUILayout.Height(30)))
        {
            if (csvFile == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a CSV file.", "OK");
                return;
            }
            if (sharedEggAOC == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign the Shared Egg Animator Override Controller (AOC).", "OK");
                return;
            }

            ImportCSV();
        }
    }

    private void ImportCSV()
    {
        EnsureDirectoryExists(digiEggFolder);
        EnsureDirectoryExists(digivolutionFolder);

        string[] lines = csvFile.text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1) return;

        int createdEggCount = 0;
        int createdDigivolutionCount = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            string[] row = lines[i].Split(',');
            if (row.Length < 2 || string.IsNullOrWhiteSpace(row[0]) || string.IsNullOrWhiteSpace(row[1])) continue;

            string digiEggId = row[0].Trim();
            string digimonId = row[1].Trim();

            // 1. Process DigiEgg (DigimonData + Shared AOC)
            CreateOrUpdateDigiEgg(digiEggId);
            createdEggCount++;

            // 2. Process Digivolution (DigivolutionData)
            CreateOrUpdateDigivolution(digiEggId, digimonId);
            createdDigivolutionCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Import Complete", 
            $"Successfully generated:\n- {createdEggCount} DigiEgg assets\n- {createdDigivolutionCount} Digivolution assets", "OK");
    }

    private void CreateOrUpdateDigiEgg(string digiEggId)
    {
        string assetPath = $"{digiEggFolder}/{digiEggId}.asset";
        DigimonData data = AssetDatabase.LoadAssetAtPath<DigimonData>(assetPath);
        bool isNew = data == null;

        if (isNew)
        {
            data = ScriptableObject.CreateInstance<DigimonData>();
        }

        SerializedObject serializedObject = new SerializedObject(data);

        // Identity
        serializedObject.FindProperty("digimonId").stringValue = digiEggId;
        serializedObject.FindProperty("displayName").stringValue = "DigiEgg";

        // Strings
        serializedObject.FindProperty("description").stringValue = string.Empty;
        serializedObject.FindProperty("basicSkill").stringValue = string.Empty;
        serializedObject.FindProperty("uniqueSkill").stringValue = string.Empty;

        // Stats & Level
        serializedObject.FindProperty("level").intValue = 0;
        serializedObject.FindProperty("health").intValue = 0;
        serializedObject.FindProperty("attack").intValue = 0;
        serializedObject.FindProperty("defence").intValue = 0;
        serializedObject.FindProperty("intelligence").intValue = 0;
        serializedObject.FindProperty("spirit").intValue = 0;
        serializedObject.FindProperty("speed").intValue = 0;

        // Attributes
        serializedObject.FindProperty("vaccine").intValue = 0;
        serializedObject.FindProperty("data").intValue = 0;
        serializedObject.FindProperty("virus").intValue = 0;

        // Fields
        serializedObject.FindProperty("cryptid").intValue = 0;
        serializedObject.FindProperty("beast").intValue = 0;
        serializedObject.FindProperty("aquatic").intValue = 0;
        serializedObject.FindProperty("bird").intValue = 0;
        serializedObject.FindProperty("machine").intValue = 0;
        serializedObject.FindProperty("bug").intValue = 0;
        serializedObject.FindProperty("plant").intValue = 0;
        serializedObject.FindProperty("seraph").intValue = 0;
        serializedObject.FindProperty("demon").intValue = 0;

        // Collections
        serializedObject.FindProperty("elements").ClearArray();
        serializedObject.FindProperty("archtypes").ClearArray();

        // --- ASSIGN SHARED SHARED ANIMATOR OVERRIDE CONTROLLER DIRECTLY ---
        SerializedProperty aocProperty = serializedObject.FindProperty("overrideController");
        if (aocProperty != null)
        {
            aocProperty.objectReferenceValue = sharedEggAOC;
        }

        serializedObject.ApplyModifiedProperties();

        if (isNew)
            AssetDatabase.CreateAsset(data, assetPath);
        else
            EditorUtility.SetDirty(data);
    }

    private void CreateOrUpdateDigivolution(string digiEggId, string digimonId)
    {
        string assetName = $"{digiEggId}_to_{digimonId}";
        string assetPath = $"{digivolutionFolder}/{assetName}.asset";

        DigivolutionData data = AssetDatabase.LoadAssetAtPath<DigivolutionData>(assetPath);
        bool isNew = data == null;

        if (isNew)
        {
            data = ScriptableObject.CreateInstance<DigivolutionData>();
        }

        SerializedObject serializedObject = new SerializedObject(data);

        // Core Digivolution Mapping
        serializedObject.FindProperty("from").stringValue = digiEggId;
        serializedObject.FindProperty("to").stringValue = digimonId;
        serializedObject.FindProperty("priority").intValue = 0;
        serializedObject.FindProperty("requiredDigimon").ClearArray();

        // General Conditions
        serializedObject.FindProperty("eggReturned").intValue = 0;
        serializedObject.FindProperty("battleCount").intValue = 0;
        serializedObject.FindProperty("winRate").floatValue = 0f;
        serializedObject.FindProperty("trainingCount").intValue = 0;
        serializedObject.FindProperty("trainingComplete").intValue = 0;

        // Stat Conditions
        serializedObject.FindProperty("health").intValue = 0;
        serializedObject.FindProperty("attack").intValue = 0;
        serializedObject.FindProperty("defence").intValue = 0;
        serializedObject.FindProperty("intelligence").intValue = 0;
        serializedObject.FindProperty("spirit").intValue = 0;
        serializedObject.FindProperty("speed").intValue = 0;

        // Attribute Conditions
        serializedObject.FindProperty("vaccine").floatValue = 0f;
        serializedObject.FindProperty("data").floatValue = 0f;
        serializedObject.FindProperty("virus").floatValue = 0f;

        // Field Conditions
        serializedObject.FindProperty("cryptid").floatValue = 0f;
        serializedObject.FindProperty("beast").floatValue = 0f;
        serializedObject.FindProperty("aquatic").floatValue = 0f;
        serializedObject.FindProperty("bird").floatValue = 0f;
        serializedObject.FindProperty("machine").floatValue = 0f;
        serializedObject.FindProperty("bug").floatValue = 0f;
        serializedObject.FindProperty("plant").floatValue = 0f;
        serializedObject.FindProperty("seraph").floatValue = 0f;
        serializedObject.FindProperty("demon").floatValue = 0f;

        serializedObject.ApplyModifiedProperties();

        if (isNew)
            AssetDatabase.CreateAsset(data, assetPath);
        else
            EditorUtility.SetDirty(data);
    }

    private static void EnsureDirectoryExists(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
    }
}
#endif