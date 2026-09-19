#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class DigivolutionCSVImporter : EditorWindow
{
    private TextAsset csvFile;
    private string digimonFolder = "Assets/Data/Digimon";
    private string outputFolder = "Assets/Data/Digivolution";

    [MenuItem("Tools/Import Digivolution CSV")]
    public static void ShowWindow()
    {
        GetWindow<DigivolutionCSVImporter>("Digivolution CSV Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Digivolution CSV Importer", EditorStyles.boldLabel);
        csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);
        digimonFolder = EditorGUILayout.TextField("Digimon Assets Folder", digimonFolder);
        outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);

        if (GUILayout.Button("Generate Digivolution Data"))
        {
            if (csvFile == null)
            {
                EditorUtility.DisplayDialog("Error", "Please assign a CSV file.", "OK");
                return;
            }
            ImportCSV();
        }
    }

    private void ImportCSV()
    {
        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        string[] lines = csvFile.text.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length <= 1) return;

        int createdCount = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            List<string> row = ParseCSVRow(lines[i]);
            if (row.Count < 27 || string.IsNullOrWhiteSpace(row[0]) || string.IsNullOrWhiteSpace(row[1])) continue;

            string fromId = row[0].Trim();
            string toId = row[1].Trim();
            string assetName = $"{fromId}_to_{toId}";
            string assetPath = $"{outputFolder}/{assetName}.asset";

            DigivolutionData data = AssetDatabase.LoadAssetAtPath<DigivolutionData>(assetPath);
            bool isNew = data == null;

            if (isNew)
            {
                data = ScriptableObject.CreateInstance<DigivolutionData>();
            }

            SerializedObject serializedObject = new SerializedObject(data);

            // Core Info
            serializedObject.FindProperty("from").stringValue = fromId;
            serializedObject.FindProperty("to").stringValue = toId;
            serializedObject.FindProperty("priority").intValue = ParseInt(row[2]);

            // Required Digimon List (Parses IDs like [digimonA, digimonB] and links assets if found)
            ParseRequiredDigimonList(serializedObject.FindProperty("requiredDigimon"), row[3], digimonFolder);

            // General Conditions
            serializedObject.FindProperty("eggReturned").intValue = ParseInt(row[4]);
            serializedObject.FindProperty("battleCount").intValue = ParseInt(row[5]);
            serializedObject.FindProperty("winRate").floatValue = ParseFloat(row[6]);
            serializedObject.FindProperty("trainingCount").intValue = ParseInt(row[7]);
            serializedObject.FindProperty("trainingComplete").intValue = ParseInt(row[8]);

            // Stat Conditions
            serializedObject.FindProperty("health").intValue = ParseInt(row[9]);
            serializedObject.FindProperty("attack").intValue = ParseInt(row[10]);
            serializedObject.FindProperty("defence").intValue = ParseInt(row[11]);
            serializedObject.FindProperty("intelligence").intValue = ParseInt(row[12]);
            serializedObject.FindProperty("spirit").intValue = ParseInt(row[13]);
            serializedObject.FindProperty("speed").intValue = ParseInt(row[14]);

            // Attribute Conditions
            serializedObject.FindProperty("vaccine").floatValue = ParseFloat(row[15]);
            serializedObject.FindProperty("data").floatValue = ParseFloat(row[16]);
            serializedObject.FindProperty("virus").floatValue = ParseFloat(row[17]);

            // Field Conditions
            serializedObject.FindProperty("cryptid").floatValue = ParseFloat(row[18]);
            serializedObject.FindProperty("beast").floatValue = ParseFloat(row[19]);
            serializedObject.FindProperty("aquatic").floatValue = ParseFloat(row[20]);
            serializedObject.FindProperty("bird").floatValue = ParseFloat(row[21]);
            serializedObject.FindProperty("machine").floatValue = ParseFloat(row[22]);
            serializedObject.FindProperty("bug").floatValue = ParseFloat(row[23]);
            serializedObject.FindProperty("plant").floatValue = ParseFloat(row[24]);
            serializedObject.FindProperty("seraph").floatValue = ParseFloat(row[25]);
            serializedObject.FindProperty("demon").floatValue = ParseFloat(row[26]);

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
        EditorUtility.DisplayDialog("Import Complete", $"Successfully created/updated {createdCount} Digivolution ScriptableObjects.", "OK");
    }

    private static int ParseInt(string input)
    {
        int.TryParse(input, out int result);
        return result;
    }

    private static float ParseFloat(string input)
    {
        float.TryParse(input, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float result);
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

    private static void ParseRequiredDigimonList(SerializedProperty listProp, string raw, string searchFolder)
    {
        listProp.ClearArray();
        string clean = raw.Replace("[", "").Replace("]", "").Replace("\"", "").Trim();
        if (string.IsNullOrEmpty(clean)) return;

        string[] digimonIds = clean.Split(',');
        int arrayIndex = 0;

        foreach (string rawId in digimonIds)
        {
            string id = rawId.Trim();
            if (string.IsNullOrEmpty(id)) continue;

            string expectedPath = $"{searchFolder}/{id}.asset";
            DigimonData digimonAsset = AssetDatabase.LoadAssetAtPath<DigimonData>(expectedPath);

            if (digimonAsset != null)
            {
                listProp.InsertArrayElementAtIndex(arrayIndex);
                SerializedProperty elementProp = listProp.GetArrayElementAtIndex(arrayIndex);
                elementProp.objectReferenceValue = digimonAsset;
                arrayIndex++;
            }
            else
            {
                Debug.LogWarning($"[Digivolution CSV Importer] Could not find DigimonData asset for ID '{id}' at path '{expectedPath}'.");
            }
        }
    }
}
#endif