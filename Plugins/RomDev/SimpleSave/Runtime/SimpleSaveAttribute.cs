using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RomDev.SimpleSave
{
    [AttributeUsage(AttributeTargets.Field)]
    public class SimpleSaveAttribute : System.Attribute
    {
        public SaveDataType SaveDataType
        {
            get
            {
                return saveDataType;
            }
        }
        public CollectionType CollectionType
        {
            get
            {
                return collectionType;
            }
        }
        public string CustomName
        {
            get
            {
                return customName;
            }
        }
        private SaveDataType saveDataType;
        private string customName;
        private CollectionType collectionType;
        public SimpleSaveAttribute(SaveDataType _saveDataType = SaveDataType.PrimitiveValue, CollectionType _collectionType = CollectionType.None, string _customName = "")
        {
            this.saveDataType = _saveDataType;
            customName = _customName;
            collectionType = _collectionType;
        }
    }
}

