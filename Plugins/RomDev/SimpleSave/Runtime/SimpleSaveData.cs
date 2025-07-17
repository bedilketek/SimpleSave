using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
namespace RomDev.SimpleSave
{
    [Serializable]
    public class SimpleSaveData
    {
        public const string SetGenericClassDataName = "SetGenericClassData";
        public const string GetGenericClassDataName = "GetGenericClassData";
        [SerializeField]
        private int internalIDCount;
        [SerializeField]
        private DictionaryBridge<string, SaveDataBase> saveDataBridge = new();
        private Dictionary<string, SaveDataBase> saveDataDict = new();
        public void OnSaveData()
        {
            saveDataBridge.CopyFromDictionary(saveDataDict);
            foreach (SaveDataBase saveData in saveDataDict.Values)
            {
                saveData.OnSaveData();
            }
        }
        public void OnLoadData()
        {
            saveDataBridge.CopyToDictionary(saveDataDict);
            foreach (SaveDataBase saveData in saveDataDict.Values)
            {
                saveData.OnLoadData();
                saveData.Reinit(this);
            }
        }
        public bool AutoSaveDataOnField(string saveKey, SaveDataType saveDataType, FieldInfo fieldInfo, object fieldHolder, CollectionType collectionType = CollectionType.None, int _levelDepth = 0)
        {
            SaveDataBase saveDataBase = null;
            bool isSaveSuccess = false;
            switch (saveDataType)
            {
                case SaveDataType.PrimitiveValue:
                    saveDataBase = new NativeValueData(this, saveDataType, collectionType);
                    isSaveSuccess = saveDataBase.SaveData(fieldInfo, fieldHolder, _levelDepth);
                    break;
                case SaveDataType.Container:
                    saveDataBase = new ContainerData(this, saveDataType, collectionType);
                    isSaveSuccess = saveDataBase.SaveData(fieldInfo, fieldHolder, _levelDepth);
                    break;
                case SaveDataType.AssetReference:
                    saveDataBase = new AssetReferenceData(this, saveDataType, collectionType);
                    isSaveSuccess = saveDataBase.SaveData(fieldInfo, fieldHolder, _levelDepth);
                    break;
            }
            if ( !isSaveSuccess)
            {
                //SimpleSaveDebugger.DebugWarning("");
                return false;
            }
            SetSaveData(saveKey, saveDataBase);
            return true;
        }
        public void AutoLoadDataOnField(string saveKey, FieldInfo fieldInfo, object fieldHolder)
        {
            SaveDataBase saveDataBase = GetSaveData(saveKey);
            if (saveDataBase == null)
            {
                SimpleSaveDebugger.DebugError("Is Null");
                return;
            }
            saveDataBase.LoadData(ref fieldInfo, fieldHolder);
        }
        public int GetInternalID()
        {
            int result = internalIDCount;
            internalIDCount++;
            return result;
        }
        private SaveDataBase GetSaveData(string saveKey)
        {
            if (!saveDataDict.TryGetValue(saveKey, out SaveDataBase _data))
            {
                return null;
            }
            return _data;
        }
        private void SetSaveData(string saveKey, SaveDataBase newData)
        {
            if (!saveDataDict.ContainsKey(saveKey))
            {
                saveDataDict.Add(saveKey, newData);
            }
            else
            {
                saveDataDict[saveKey] = newData;
            }
        }
    }
}

