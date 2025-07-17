using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RomDev.SimpleSave.Demo
{
    public class SSDemo_Scenario1 : MonoBehaviour
    {
        [SimpleSave(SaveDataType.PrimitiveValue)]
        public int intValue;
        [SimpleSave(SaveDataType.PrimitiveValue)]
        public string textValue;
        [SimpleSave(SaveDataType.PrimitiveValue, CollectionType.Array)]
        public int[] arrayValue;
        [SimpleSave(SaveDataType.AssetReference)]
        public GameObject assetRefValue;
        [SimpleSave(SaveDataType.Container)]
        public ParentClass parentClass;
        [Serializable]
        public class ParentClass
        {
            [SimpleSave(SaveDataType.PrimitiveValue)]
            public int valueInside;
        }
    }
}

