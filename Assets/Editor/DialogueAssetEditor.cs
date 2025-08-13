using System;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(DialogueAsset))]
public class DialogueAssetEditor : Editor
{
    [Serializable]
    private class DialogueExportData
    {
        public string dialogueId;
        public DialogueNodeExport[] nodes;
    }

    [Serializable]
    private class DialogueNodeExport
    {
        public string id;
        public string text;
        public DialogueOption[] options;
        public bool givesQuest;
        public DialogueQuestExport questData;
    }

    [Serializable]
    private class DialogueQuestExport
    {
        public string questId;
        public string title;
        public string description;
        public QuestObjectiveData[] objectives;
    }

    private ReorderableList nodeList;

    private void OnEnable()
    {
        var nodesProp = serializedObject.FindProperty("nodes");
        nodeList = new ReorderableList(serializedObject, nodesProp, true, true, true, true);

        nodeList.drawHeaderCallback = rect =>
            EditorGUI.LabelField(rect, "Dialogue Nodes");

        nodeList.drawElementCallback = (rect, index, active, focused) =>
        {
            var element = nodesProp.GetArrayElementAtIndex(index);
            rect.y += 2;

            var idRect = new Rect(rect.x, rect.y, 100, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(idRect,
                element.FindPropertyRelative("id"),
                GUIContent.none);

            var textRect = new Rect(rect.x + 105, rect.y, rect.width - 105, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(textRect,
                element.FindPropertyRelative("text"),
                GUIContent.none);

            var optionsProp = element.FindPropertyRelative("options");
            var optionsRect = new Rect(rect.x,
                rect.y + EditorGUIUtility.singleLineHeight + 4,
                rect.width,
                EditorGUI.GetPropertyHeight(optionsProp, true));
            EditorGUI.PropertyField(optionsRect,
                optionsProp,
                new GUIContent("Options"),
                true);

            float yOffset = EditorGUIUtility.singleLineHeight + 4 + EditorGUI.GetPropertyHeight(optionsProp, true) + 4;


            var givesQuestProp = element.FindPropertyRelative("givesQuest");
            var givesQuestRect = new Rect(rect.x, rect.y + yOffset, rect.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(givesQuestRect,
                givesQuestProp,
                new GUIContent("Gives Quest"));

            yOffset += EditorGUIUtility.singleLineHeight + 4;


            if (givesQuestProp.boolValue)
            {
                var questAssetProp = element.FindPropertyRelative("questAsset");
                var questAssetRect = new Rect(rect.x + 15,
                    rect.y + yOffset,
                    rect.width - 15,
                    EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(questAssetRect,
                    questAssetProp,
                    new GUIContent("Quest Asset"));
            }
        };

        nodeList.elementHeightCallback = index =>
        {
            var element = nodesProp.GetArrayElementAtIndex(index);
            var optionsProp = element.FindPropertyRelative("options");
            float height = 2 + EditorGUIUtility.singleLineHeight + 4 + EditorGUI.GetPropertyHeight(optionsProp, true) + 4;
            height += EditorGUIUtility.singleLineHeight + 4;

            if (element.FindPropertyRelative("givesQuest").boolValue)
                height += EditorGUIUtility.singleLineHeight + 4;

            return height;
        };
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty("dialogueId"));
        nodeList.DoLayoutList();
        EditorGUILayout.Space();

        if (GUILayout.Button("Save to JSON"))
        {
            serializedObject.ApplyModifiedProperties();
            SaveToJson();
            return;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void SaveToJson()
    {
        var asset = (DialogueAsset)target;
        var export = new DialogueExportData
        {
            dialogueId = asset.dialogueId,
            nodes = new DialogueNodeExport[asset.nodes.Count]
        };

        for (int i = 0; i < asset.nodes.Count; i++)
        {
            var node = asset.nodes[i];
            var exportNode = new DialogueNodeExport
            {
                id = node.id,
                text = node.text,
                options = node.options,
                givesQuest = node.givesQuest,
                questData = node.givesQuest && node.questAsset != null
                 ? new DialogueQuestExport
                 {
                     questId = node.questAsset.questId,
                     title = node.questAsset.title,
                     description = node.questAsset.description,
                     objectives = node.questAsset.objectives
                 }
                 : null
            };

            if (node.givesQuest && node.questAsset != null)
            {
                var q = node.questAsset;
                exportNode.questData = new DialogueQuestExport
                {
                    questId = q.questId,
                    title = q.title,
                    description = q.description,
                    objectives = q.objectives
                };
            }

            export.nodes[i] = exportNode;
        }

        string json = JsonUtility.ToJson(export, true);
        string path = EditorUtility.SaveFilePanel(
            "Save Dialogue JSON",
            Application.dataPath + "/Dialogues",
            asset.name + ".json",
            "json");

        if (string.IsNullOrEmpty(path)) return;

        File.WriteAllText(path, json);
        EditorUtility.DisplayDialog(
            "Saved",
            "Dialogue JSON saved to:\n" + path,
            "OK");
    }
}
