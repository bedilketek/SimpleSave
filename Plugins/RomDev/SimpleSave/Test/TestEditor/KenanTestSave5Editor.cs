using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
namespace RomDev.SimpleSave.Demo
{
    [CustomEditor(typeof(KenanTestSave5))]
    public class KenanTestSave5Editor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            KenanTestSave5 kenanTestSave5 = (KenanTestSave5)target;
            if (GUILayout.Button("Make Animals"))
            {
                kenanTestSave5.MakeAnimals();
            }
            if (GUILayout.Button("Save Data"))
            {
                kenanTestSave5.SaveData();
            }
            if (GUILayout.Button("Load Data"))
            {
                kenanTestSave5.LoadData();
            }
            if (GUILayout.Button("Print Contents"))
            {
                kenanTestSave5.PrintAnimalsContent();
            }
        }
    }
}

#endif
