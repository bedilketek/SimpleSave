using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using KenanDev.SimpleSave.Demo;
namespace KenanDev.SimpleSave
{
    [CustomEditor(typeof(KenanTestSave))]
    public class KenantTestSaveEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}
#endif

