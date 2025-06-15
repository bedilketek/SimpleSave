// using System.Collections;
// using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
namespace KenanDev.SimpleSave
{
    public class SimpleSaveWindow : EditorWindow
    {
        [SerializeField]
        private SimpleSaveManagerDataSO simpleSaveManagerDataSO;
        [MenuItem("Window/SimpleSave")]
        public static void Init()
        {
            SimpleSaveWindow simpleSave = (SimpleSaveWindow)GetWindow(typeof(SimpleSaveWindow));
            simpleSave.titleContent.text = "Simple Save";
        }
        private void OnInspectorUpdate()
        {
            Repaint();
        }
        private void OnGUI()
        {
            TryLoadSimpleSaveManagerDataSO();
            // DrawSimpleSaveDataObject();
            if (simpleSaveManagerDataSO == null)
            {
                DrawEmptySimpleSaveData();
            }
            else
            {
                simpleSaveManagerDataSO.ShowGUI();
            }
        }
        private void TryLoadSimpleSaveManagerDataSO()
        {
            if (simpleSaveManagerDataSO != null)
            {
                return;
            }
            simpleSaveManagerDataSO = Resources.Load<SimpleSaveManagerDataSO>(SimpleSaveHelper.DefaultSimpleSaveManagerDataName);
        }
        private void DrawEmptySimpleSaveData()
        {
            EditorGUILayout.LabelField("There's no SimpleSaveManagerData detected!!\nPlease insert manually on this window or create in SimpleSave's Resources Folder");
        }
    }
}
#endif
