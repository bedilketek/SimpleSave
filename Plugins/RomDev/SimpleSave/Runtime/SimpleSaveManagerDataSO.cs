using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace RomDev.SimpleSave
{
    [CreateAssetMenu(fileName = "SimpleSaveManagerData", menuName = "SimpleSave/SimpleSaveManagerDataSO", order = 0)]
    public class SimpleSaveManagerDataSO : ScriptableObject
    {
        // Saving
        public int MaxSaveCount
        {
            get
            {
                return maxSaveCount;
            }
        }
        public string SaveFolderName
        {
            get
            {
                return saveFolderName;
            }
        }
        public string BaseSaveName
        {
            get
            {
                return baseSaveName;
            }
        }
        public bool DefaultLoadOnAwake
        {
            get
            {
                return defaultLoadOnAwake;
            }
        }
        public bool DefaultSaveOnDestroy
        {
            get
            {
                return defaultSaveOnDestroy;
            }
        }
        public int MaxContainerDepth
        {
            get
            {
                return maxContainerDepth;
            }
        }
        [SerializeField]
        private int maxSaveCount = 5;
        [SerializeField]
        private string saveFolderName = "SAVE";
        [SerializeField]
        private string baseSaveName = "gamesave";
        [SerializeField]
        private bool defaultLoadOnAwake;
        [SerializeField]
        private bool defaultSaveOnDestroy;
        [SerializeField]
        private int maxContainerDepth = 8;

        // Debugging
        public bool ShowRegularLog
        {
            get
            {
                return showRegularLog;
            }
        }
        public bool ShowWarningLog
        {
            get
            {
                return showWarningLog;
            }
        }
        public bool ShowErrorLog
        {
            get
            {
                return showErrorLog;
            }
        }
        [SerializeField]
        private bool showRegularLog;
        [SerializeField]
        private bool showWarningLog;
        [SerializeField]
        private bool showErrorLog;
#if UNITY_EDITOR
        public void ShowGUI()
        {
            EditorGUILayout.LabelField("Simple Save", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            DrawSettingGUI();

            EditorGUILayout.Space();
            ShowDebuggingGUI();

            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(this);
            }
        }
        private void DrawSettingGUI()
        {
            EditorGUILayout.LabelField("Save Settings");
            maxSaveCount = EditorGUILayout.IntField("Max Save: ", maxSaveCount);
            saveFolderName = EditorGUILayout.TextField("Save Folder Name; ", saveFolderName);
            baseSaveName = EditorGUILayout.TextField("Base save file name: ", baseSaveName);
            defaultLoadOnAwake = EditorGUILayout.Toggle("Default Load on awake? ", defaultLoadOnAwake);
            defaultSaveOnDestroy = EditorGUILayout.Toggle("Default save on destroy? ", defaultSaveOnDestroy);
            maxContainerDepth = EditorGUILayout.IntField("Max Container depth: ", maxContainerDepth);
            EditorGUILayout.Space();

            if (GUILayout.Button("Delete All data"))
            {
                ShowConsentToDeleteData();
            }
        }
        private void ShowDebuggingGUI()
        {
            EditorGUILayout.LabelField("Debugging Settings");
            showRegularLog = EditorGUILayout.Toggle("Show Regular Log", showRegularLog);
            showWarningLog = EditorGUILayout.Toggle("Show Warning Log", showWarningLog);
            showErrorLog = EditorGUILayout.Toggle("Show Error Log", showErrorLog);
        }
        private void ShowConsentToDeleteData()
        {
            if (EditorUtility.DisplayDialog("Save Deletion", "Are you sure to delete all save data ? ",
            "Yes, delete it", "No"))
            {
                SimpleSave.DeleteAllSaveFile();
            }
        }
        #endif
    }
}

