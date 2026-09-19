#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using System.Collections.Generic;

public class MonsterAnimBatchProcessor
{
    [MenuItem("Tools/Batch Process Monster Animations")]
    public static void ProcessMonsterFolders()
    {
        // Select folders in the Project Window
        string[] selectedFolders = Selection.assetGUIDs
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(AssetDatabase.IsValidFolder)
            .ToArray();

        if (selectedFolders.Length == 0)
        {
            Debug.LogWarning("Please select at least one folder containing monster sprites in the Project window.");
            return;
        }

        int clipsCreated = 0;

        foreach (string folderPath in selectedFolders)
        {
            // Load all Sprite assets in the folder ordered by file name
            string[] spriteFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(s => s.EndsWith(".png") || s.EndsWith(".jpg") || s.EndsWith(".tga"))
                .OrderBy(s => s)
                .ToArray();

            List<Sprite> sprites = new List<Sprite>();
            foreach (string file in spriteFiles)
            {
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(file);
                if (sprite != null) sprites.Add(sprite);
            }

            int count = sprites.Count;

            // CASE 1: 5 Sprites (3 Animations: Idle, Win, Loss)
            if (count == 5)
            {
                CreateClip(folderPath, "Idle", 4, sprites[0], sprites[1]);
                CreateClip(folderPath, "Win", 4, sprites[3]);
                CreateClip(folderPath, "Loss", 4, sprites[4]);
                clipsCreated += 3;
            }
            // CASE 2: 12 Sprites (7 Animations: Idle, Walk, Run, Training, Win, Loss, Attack)
            // Note: If your folder has 12 sprites indexed 1 to 12, index 11 is '12 as attack'.
            else if (count >= 11)
            {
                CreateClip(folderPath, "Idle", 4, sprites[0], sprites[1]);
                CreateClip(folderPath, "Walk", 6, sprites[2], sprites[3]);
                CreateClip(folderPath, "Run", 8, sprites[4], sprites[5]);
                CreateClip(folderPath, "Training", 4, sprites[6], sprites[7]);
                CreateClip(folderPath, "Win", 4, sprites[8]);
                CreateClip(folderPath, "Loss", 4, sprites[9]);
                CreateClip(folderPath, "Attack", 6, sprites[10]);
                clipsCreated += 7;
            }
            else
            {
                Debug.LogWarning($"Skipped folder '{folderPath}': Expected 5 or 12 sprites, but found {count}.");
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Batch process complete! Created {clipsCreated} animation clips.");
    }

    private static void CreateClip(string folderPath, string animSuffix, float frameRate, params Sprite[] frames)
    {
        if (frames == null || frames.Length == 0 || frames[0] == null) return;

        AnimationClip clip = new AnimationClip { frameRate = frameRate };

        EditorCurveBinding binding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "",
            propertyName = "m_Sprite"
        };

        ObjectReferenceKeyframe[] keyframes;

        // If 2-frame animation (1-2 loop: 1 -> 2 -> 1)
        if (frames.Length == 2)
        {
            keyframes = new ObjectReferenceKeyframe[3];
            keyframes[0] = new ObjectReferenceKeyframe { time = 0f, value = frames[0] };
            keyframes[1] = new ObjectReferenceKeyframe { time = 1f / frameRate, value = frames[1] };
            keyframes[2] = new ObjectReferenceKeyframe { time = 2f / frameRate, value = frames[0] };
        }
        // Single frame static pose animation
        else
        {
            keyframes = new ObjectReferenceKeyframe[1];
            keyframes[0] = new ObjectReferenceKeyframe { time = 0f, value = frames[0] };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, binding, keyframes);

        // Set Loop Time on clip
        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = (frames.Length > 1); // Only loop multi-frame clips by default
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        // Save clip inside monster folder named FolderName_Suffix.anim
        string folderName = Path.GetFileName(folderPath);
        string outputPath = Path.Combine(folderPath, $"{folderName}_{animSuffix}.anim");
        AssetDatabase.CreateAsset(clip, outputPath);
    }
}
#endif