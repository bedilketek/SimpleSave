using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RomDev.SimpleSave
{
    [Serializable]
    public class ContainerFieldData
    {
        public string fieldName;
        public SaveDataType saveDataType;
        public CollectionType collectionType;
        public string saveKey;
        public ContainerFieldData(string _saveKey, string _fieldName, SaveDataType _saveDataType, CollectionType _collectionType)
        {
            saveKey = _saveKey;
            fieldName = _fieldName;
            saveDataType = _saveDataType;
            collectionType = _collectionType;
        }
    }
}

