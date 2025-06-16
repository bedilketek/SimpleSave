using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KenanDev.SimpleSave.Demo
{
    public class KenanTestSave : MonoBehaviour, ISimpleSave
    {
        public string saveKey = "TestSaveKey";
        public int num;
        public TestGood testGood;
        [HideInInspector]
        [Serializable]
        public class TestGood
        {
            public int id;
        }
        public void OnSaveData()
        {
            SimpleSaveData simpleSaveData = new();
            simpleSaveData.SetIntValue("num", num);
            simpleSaveData.SetGenericClassData<TestGood>("KenanTestGood", testGood, SimpleSaveDemoHelper.DemoSimpleSaveProjectName);
            SimpleSave.SetData(saveKey, simpleSaveData);
        }
        public void OnLoadData()
        {
            SimpleSaveData simpleSaveData = SimpleSave.GetSaveData(saveKey);
            if (simpleSaveData == null)
            {
                return;
            }
            num = simpleSaveData.GetIntData("num");
            TestGood _testGood = simpleSaveData.GetGenericClassData<TestGood>("KenanTestGood");
            // Debug.Log(_testGood.id);
            if (_testGood != null)
            {
                testGood = _testGood;
            }
        }
    }
}

