using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace RomDev.SimpleSave
{
    [Serializable]
    public class SS_FieldData
    {
        public FieldInfo fieldInfo;
        public SimpleSaveAttribute simpleSaveAttribute;
        public object fieldHolder;
        public SS_FieldData(FieldInfo _fieldInfo, SimpleSaveAttribute _simpleSaveAttribute, object _fieldHolder)
        {
            fieldInfo = _fieldInfo;
            simpleSaveAttribute = _simpleSaveAttribute;
            fieldHolder = _fieldHolder;
        }
    }
}

