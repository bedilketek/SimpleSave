using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using KenanDev.SimpleSave.Demo;
namespace KenanDev.SimpleSave
{
    [CustomEditor(typeof(KenanTestSave4))]
    public class KenanTestSave4Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            KenanTestSave4 kenanTestSave4 = (KenanTestSave4)target;
            if (GUILayout.Button("Save game"))
            {
                kenanTestSave4.SaveGame();
            }
            if (GUILayout.Button("Load Game"))
            {
                kenanTestSave4.LoadGame();
            }
        }
    }
}
#endif
