using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace KenanDev.SimpleSave.Demo
{
    public class KenanTestSave2 : MonoBehaviour
    {
        [KenanSave(SaveDataType.Int, "Number")]
        public int number;
        [KenanSave(SaveDataType.String, "Text")]
        public string textValue;
        [KenanSave(SaveDataType.GenericClass, "Cookies Cream")]
        public CookiesCream cookiesCream;
        [Serializable, HideInInspector]
        public class CookiesCream
        {
            public int cookiesNum;
        }
    }
}

