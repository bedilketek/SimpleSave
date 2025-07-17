using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RomDev.SimpleSave.Demo
{
    public class KenanTestSave2 : MonoBehaviour
    {
        [SimpleSave(SaveDataType.PrimitiveValue, CollectionType.None)]
        public int number;
        [SimpleSave(SaveDataType.PrimitiveValue, CollectionType.None)]
        public string textValue;
        [SimpleSave(SaveDataType.PrimitiveValue, CollectionType.List)]
        public List<int> arrayList = new();
        [SimpleSave(SaveDataType.AssetReference, CollectionType.List)]
        public List<GameObject> assetObjRef = new();
        [SimpleSave(SaveDataType.Container, CollectionType.List)]
        public List<CookiesCream> cookiesCreams = new();
        [Serializable, HideInInspector]
        public class CookiesCream
        {
            [SimpleSave(SaveDataType.PrimitiveValue, CollectionType.None)]
            public int cookiesNum;
        }
        [Serializable, HideInInspector]
        public class WaffleChoco
        {
            [SimpleSave(SaveDataType.PrimitiveValue, CollectionType.None)]
            public int raisinNum;
        }
    }
}

