using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace KenanDev.SimpleSave
{
    [CustomEditor(typeof(SimpleComponentSaver))]
    public class SimpleComponentSaverEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SimpleComponentSaver simpleComponentSaver = (SimpleComponentSaver)target;
            simpleComponentSaver.ShowGUI();
        }
    }
}

