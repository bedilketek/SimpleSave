
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace RomDev.SimpleSave.Demo
{
    [CustomEditor(typeof(SSDemoManual_SaveLoad))]
    public class SSDemoManual_SaveLoadEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            SSDemoManual_SaveLoad sSDemoManual_SaveLoad = (SSDemoManual_SaveLoad)target;
            if (GUILayout.Button("Save Game"))
            {
                sSDemoManual_SaveLoad.SaveGame();
            }
            if (GUILayout.Button("Load Game"))
            {
                sSDemoManual_SaveLoad.LoadGame();
            }
            if (GUILayout.Button("Delete Save"))
            {
                sSDemoManual_SaveLoad.DeleteSave();
            }
        }
    }
}

#endif