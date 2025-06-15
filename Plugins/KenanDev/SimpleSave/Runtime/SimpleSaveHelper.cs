using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace KenanDev.SimpleSave
{
    public static class SimpleSaveHelper
    {
        public static string RuntimeSimpleSaveAssemblyName
        {
            get
            {
                return RuntimeSimpleSaveProjectName + ", " + HalfEndAssemblyName;
            }
        }
        public static string MainNamespaceAssemblyName
        {
            get
            {
                return MainNamespaceProjectName + ", " + HalfEndAssemblyName;
            }
        }
        public const string RuntimeSimpleSaveProjectName = "KenanDev.SimpleSave";
        public const string MainNamespaceProjectName = "Assembly-CSharp";
        public const string HalfEndAssemblyName = "Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
        public const string DefaultSimpleSaveManagerDataName = "SimpleSaveManagerData";
        public const string DefaultSaveFolderName = "SavePath";
        public const string DefaultSaveFileName = "SaveFile";
        public const string DefaultSystemSaveFolder = "SimpleSave_System";
        public const string DefaultSavePackFileName = "SS_System";
        public const string JSON_EXT = ".json";
        public const string BIN_EXT = ".bin";
        public const int DefaultMaxSave = 5;
        public static T DeepCopy<T>(T obj)
        {
            string json = JsonUtility.ToJson(obj);
            return JsonUtility.FromJson<T>(json);
        }
    }
}

