using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace KenanDev.SimpleSave
{
    [AttributeUsage(AttributeTargets.Field)]
    public class KenanSaveAttribute : System.Attribute
    {
        public SaveDataType SaveDataType
        {
            get
            {
                return saveDataType;
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
        public KenanSaveAttribute(SaveDataType _saveDataType, string _customName = "")
        {
            this.saveDataType = _saveDataType;
            customName = _customName;
        }
    }
}

