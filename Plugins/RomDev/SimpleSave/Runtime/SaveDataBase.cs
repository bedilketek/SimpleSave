using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using RomDev.AssetObjectLocator;
namespace RomDev.SimpleSave
{
    [Serializable]
    public abstract class SaveDataBase
    {
        public SaveDataType SaveDataType
        {
            get
            {
                return saveDataType;
            }
        }
        [SerializeField]
        protected SaveDataType saveDataType;
        [SerializeField]
        protected CollectionType collectionType = CollectionType.None;
        protected SimpleSaveData simpleSaveData;
        public SaveDataBase(SimpleSaveData _simpleSaveData, SaveDataType _saveDataType, CollectionType _collectionType)
        {
            simpleSaveData = _simpleSaveData;
            saveDataType = _saveDataType;
            collectionType = _collectionType;
        }
        public abstract void OnSaveData();
        public abstract void OnLoadData();
        public abstract bool SaveData(FieldInfo fieldInfo, object fieldHolder, int levelDepth);
        public abstract bool LoadData(ref FieldInfo fieldInfo, object fieldHolder);
        public abstract bool SaveData(object value);
        public abstract bool LoadData(object source, out object result);
        public virtual void Reinit(SimpleSaveData _simpleSaveData)
        {
            simpleSaveData = _simpleSaveData;
        }
    }
    [Serializable]
    public class NativeValueData : SaveDataBase
    {
        [SerializeField]
        private string[] stringValue;
        public NativeValueData(SimpleSaveData _simpleSaveData, SaveDataType _saveDataType, CollectionType _collectionType) : base(_simpleSaveData, _saveDataType, _collectionType)
        {
        }
        public override void OnSaveData()
        { }
        public override void OnLoadData()
        { }
        public override bool SaveData(FieldInfo fieldInfo, object fieldHolder, int levelDepth)
        {
            if (collectionType == CollectionType.None)
            {
                stringValue = new string[1];
                stringValue[0] = fieldInfo.GetValue(fieldHolder).ToString();
                return true;
            }
            object fieldValue = fieldInfo.GetValue(fieldHolder);
            IEnumerable<object> collection = ((IEnumerable)fieldValue).Cast<object>();
            int collectionCount = collection.Count();
            stringValue = new string[collectionCount];
            int i = 0;
            foreach (object collectionItem in collection)
            {
                stringValue[i] = collectionItem.ToString();
                i++;
            }
            return true;
        }
        public override bool LoadData(ref FieldInfo fieldInfo, object fieldHolder)
        {
            if (collectionType == CollectionType.None)
            {
                fieldInfo.SetValue(fieldHolder, Convert.ChangeType(stringValue[0], fieldInfo.FieldType));
                return true;
            }
            switch (collectionType)
            {
                case CollectionType.Array:
                    Type arrayType = fieldInfo.FieldType;
                    Type elemOfArrayType = arrayType.GetElementType();
                    Array typedArray = Array.CreateInstance(elemOfArrayType, stringValue.Length);
                    for (int i = 0; i < stringValue.Length; i++)
                    {
                        typedArray.SetValue(Convert.ChangeType(stringValue[i], elemOfArrayType), i);
                    }
                    fieldInfo.SetValue(fieldHolder, typedArray);
                    break;
                case CollectionType.List:
                    Type listType = fieldInfo.FieldType;
                    Type[] genericArgs = listType.GetGenericArguments();
                    Type elemOfListType = genericArgs[0];
                    object listInstance = Activator.CreateInstance(listType);
                    MethodInfo addMethod = listType.GetMethod("Add");
                    for (int i = 0; i < stringValue.Length; i++)
                    {
                        object convertedValue = Convert.ChangeType(stringValue[i], elemOfListType);
                        addMethod.Invoke(listInstance, new object[] { convertedValue });
                    }
                    fieldInfo.SetValue(fieldHolder, listInstance);
                    break;
            }
            return true;
        }
        public override bool SaveData(object value)
        {
            if (collectionType == CollectionType.None)
            {
                stringValue = new string[1];
                stringValue[0] = value.ToString();
                return true;
            }
            IEnumerable<object> collection = ((IEnumerable)value).Cast<object>();
            int collectionCount = collection.Count();
            stringValue = new string[collectionCount];
            int i = 0;
            foreach (object collectionItem in collection)
            {
                stringValue[i] = collectionItem.ToString();
                i++;
            }
            return true;
        }
        public override bool LoadData(object source, out object result)
        {
            if (collectionType == CollectionType.None)
            {
                result = Convert.ChangeType(stringValue[0], source.GetType());
                return true;
            }
            result = null;
            switch (collectionType)
            {
                case CollectionType.Array:
                    Type arrayType = source.GetType();
                    Type elemOfArrayType = arrayType.GetElementType();
                    Array typedArray = Array.CreateInstance(elemOfArrayType, stringValue.Length);
                    for (int i = 0; i < stringValue.Length; i++)
                    {
                        typedArray.SetValue(Convert.ChangeType(stringValue[i], elemOfArrayType), i);
                    }
                    result = typedArray;
                    break;
                case CollectionType.List:
                    Type listType = source.GetType();
                    Type[] genericArgs = listType.GetGenericArguments();
                    Type elemOfListType = genericArgs[0];
                    object listInstance = Activator.CreateInstance(listType);
                    MethodInfo addMethod = listType.GetMethod("Add");
                    for (int i = 0; i < stringValue.Length; i++)
                    {
                        object convertedValue = Convert.ChangeType(stringValue[i], elemOfListType);
                        addMethod.Invoke(listInstance, new object[] { convertedValue });
                    }
                    result = listInstance;
                    break;
            }
            return true;
        }
    }
    [Serializable]
    public class ContainerData : SaveDataBase
    {
        [SerializeField]
        private ArrayWrapper<ContainerFieldData[]>[] containerFieldWrappers;
        [SerializeField]
        private int containerDataSpecialID;
        public ContainerData(SimpleSaveData _simpleSaveData, SaveDataType _saveDataType, CollectionType _collectionType) : base(_simpleSaveData, _saveDataType, _collectionType)
        {
            containerDataSpecialID = _simpleSaveData.GetInternalID();
        }
        public override void OnSaveData()
        { }
        public override void OnLoadData()
        { }
        public override bool SaveData(FieldInfo fieldInfo, object fieldHolder, int levelDepth)
        {
            if (levelDepth > SimpleSave.SimpleSaveManager.MaxContainerDepth)
            {
                SimpleSaveDebugger.DebugWarning("Container or Load iterator has exceeded the max container depth which is : " + SimpleSave.SimpleSaveManager.MaxContainerDepth);
                return false;
            }
            object fieldValue = fieldInfo.GetValue(fieldHolder);

            // Single Value
            if (collectionType == CollectionType.None)
            {
                List<SS_FieldData> ss_fieldDatas = SimpleSaveHelper.GetFieldInfosWithAttribute(fieldValue);
                ContainerFieldData[] containerFieldDatas = GetContainerFieldDatas(ss_fieldDatas, levelDepth);
                containerFieldWrappers = new ArrayWrapper<ContainerFieldData[]>[1];
                containerFieldWrappers[0] = new ArrayWrapper<ContainerFieldData[]>(containerFieldDatas);
                return true;
            }

            // Collection Value
            IEnumerable<object> collection = ((IEnumerable)fieldValue).Cast<object>();
            int collectionCount = collection.Count();
            containerFieldWrappers = new ArrayWrapper<ContainerFieldData[]>[collectionCount];
            int i = 0;
            foreach (object collectionItem in collection)
            {
                List<SS_FieldData> ss_fieldDatas = SimpleSaveHelper.GetFieldInfosWithAttribute(collectionItem);
                ContainerFieldData[] containerFieldDatas = GetContainerFieldDatas(ss_fieldDatas, levelDepth);
                containerFieldWrappers[i] = new ArrayWrapper<ContainerFieldData[]>(containerFieldDatas);
                i++;
            }
            return true;
        }
        public override bool LoadData(ref FieldInfo fieldInfo, object fieldHolder)
        {
            object fieldObject = fieldInfo.GetValue(fieldHolder);
            // Single Value
            if (collectionType == CollectionType.None)
            {
                ContainerFieldData[] containerFieldDatas = containerFieldWrappers[0].value;
                LoadFieldData(containerFieldDatas, fieldInfo.FieldType, fieldObject);
                return true;
            }
            // Collection Value
            switch (collectionType)
            {
                case CollectionType.Array:
                    Type arrayType = fieldInfo.FieldType;
                    Type elemOfArrayType = arrayType.GetElementType();
                    Array typedArray = Array.CreateInstance(elemOfArrayType, containerFieldWrappers.Length);

                    for (int i = 0; i < containerFieldWrappers.Length; i++)
                    {
                        object containerInstance = Activator.CreateInstance(elemOfArrayType);
                        ContainerFieldData[] containerFieldDatas = containerFieldWrappers[i].value;
                        LoadFieldData(containerFieldDatas, elemOfArrayType, containerInstance);
                        typedArray.SetValue(containerInstance, i);
                    }
                    fieldInfo.SetValue(fieldHolder, typedArray);
                    break;
                case CollectionType.List:
                    Type listType = fieldInfo.FieldType;
                    Type[] genericArgs = listType.GetGenericArguments();
                    Type elemOfListType = genericArgs[0];
                    object listInstance = Activator.CreateInstance(listType);
                    MethodInfo addMethod = listType.GetMethod("Add");
                    for (int i = 0; i < containerFieldWrappers.Length; i++)
                    {
                        object containerInstance = Activator.CreateInstance(elemOfListType);
                        ContainerFieldData[] containerFieldDatas = containerFieldWrappers[i].value;
                        LoadFieldData(containerFieldDatas, elemOfListType, containerInstance);
                        addMethod.Invoke(listInstance, new object[] { containerInstance });
                    }
                    fieldInfo.SetValue(fieldHolder, listInstance);
                    break;
            }
            return true;
        }
        private ContainerFieldData[] GetContainerFieldDatas(List<SS_FieldData> ss_fieldDatas, int levelDepth)
        {
            List<ContainerFieldData> CFDList = new();
            levelDepth++;
            int CFDSpecialID = simpleSaveData.GetInternalID();
            foreach (SS_FieldData ss_fieldData in ss_fieldDatas)
            {
                string saveKey = containerDataSpecialID + "_" + ss_fieldData.fieldInfo.Name + "_" + CFDSpecialID;
                bool isSuccess = simpleSaveData.AutoSaveDataOnField(saveKey, ss_fieldData.simpleSaveAttribute.SaveDataType, ss_fieldData.fieldInfo, ss_fieldData.fieldHolder, ss_fieldData.simpleSaveAttribute.CollectionType, levelDepth);
                if (!isSuccess)
                {
                    SimpleSaveDebugger.DebugError("Error");
                    break;
                }
                ContainerFieldData freshCFD = new(saveKey, ss_fieldData.fieldInfo.Name, ss_fieldData.simpleSaveAttribute.SaveDataType, ss_fieldData.simpleSaveAttribute.CollectionType);
                CFDList.Add(freshCFD);
            }
            ContainerFieldData[] containerFieldDatas = CFDList.ToArray();
            return containerFieldDatas;
        }
        private void LoadFieldData(ContainerFieldData[] containerFieldDatas, Type type, object fieldObject)
        {
            foreach (ContainerFieldData containerFieldData in containerFieldDatas)
            {
                FieldInfo childFieldInfo = type.GetField(containerFieldData.fieldName);
                if (childFieldInfo == null)
                {
                    SimpleSaveDebugger.DebugError("There's no field with name : " + containerFieldData.fieldName);
                    continue;
                }
                simpleSaveData.AutoLoadDataOnField(containerFieldData.saveKey, childFieldInfo, fieldObject);
            }
        }
        public override bool SaveData(object value)
        {
            throw new NotImplementedException();
        }
        public override bool LoadData(object source,out object value)
        {
            throw new NotImplementedException();
        }
    }
    [Serializable]
    public class AssetReferenceData : SaveDataBase
    {
        [SerializeField]
        private int[] targetAssetIDS;
        public AssetReferenceData(SimpleSaveData _simpleSaveData, SaveDataType _saveDataType, CollectionType _collectionType) : base(_simpleSaveData, _saveDataType, _collectionType)
        { }
        public override void OnSaveData()
        { }
        public override void OnLoadData()
        { }
        public override bool SaveData(FieldInfo fieldInfo, object fieldHolder, int levelDepth)
        {
            if (collectionType == CollectionType.None)
            {
                UnityEngine.Object unityObj = (UnityEngine.Object)fieldInfo.GetValue(fieldHolder);
                if (!AssetObjLocator.GetIDOfObject(unityObj, out int objID))
                {
                    return false;
                }
                targetAssetIDS = new int[1];
                targetAssetIDS[0] = objID;
                return true;
            }
            // Collection Value
            object fieldValue = fieldInfo.GetValue(fieldHolder);
            IEnumerable<object> collection = ((IEnumerable)fieldValue).Cast<object>();
            int collectionCount = collection.Count();
            targetAssetIDS = new int[collectionCount];
            int i = 0;
            foreach (object collectionItem in collection)
            {
                UnityEngine.Object unityObj = (UnityEngine.Object)collectionItem;
                if (!AssetObjLocator.GetIDOfObject(unityObj, out int objID))
                {
                    i++;
                    continue;
                }
                targetAssetIDS[i] = objID;
                i++;
            }
            return true;
        }
        public override bool LoadData(ref FieldInfo fieldInfo, object fieldHolder)
        {
            if (collectionType == CollectionType.None)
            {
                int objID = targetAssetIDS[0];
                if (!AssetObjLocator.GetObjectByID(objID, out UnityEngine.Object unityObj))
                {
                    return false;
                }
                fieldInfo.SetValue(fieldHolder, unityObj);
                return true;
            }

            // Collection Type
            switch (collectionType)
            {
                case CollectionType.Array:
                    Type arrayType = fieldInfo.FieldType;
                    Type elemOfArrayType = arrayType.GetElementType();
                    Array typedArray = Array.CreateInstance(elemOfArrayType, targetAssetIDS.Length);
                    for (int i = 0; i < targetAssetIDS.Length; i++)
                    {
                        // Don't Create with Activator! it will instantiate newobject on scene
                        object containerInstance;
                        if (!AssetObjLocator.GetObjectByID(targetAssetIDS[i], out UnityEngine.Object unityObj))
                        {
                        }
                        containerInstance = unityObj;
                        typedArray.SetValue(containerInstance, i);
                    }
                    fieldInfo.SetValue(fieldHolder, typedArray);
                    break;
                case CollectionType.List:
                    Type listType = fieldInfo.FieldType;
                    Type[] genericArgs = listType.GetGenericArguments();
                    Type elemOfListType = genericArgs[0];
                    object listInstance = Activator.CreateInstance(listType);
                    MethodInfo addMethod = listType.GetMethod("Add");
                    for (int i = 0; i < targetAssetIDS.Length; i++)
                    {
                        // Don't Create with Activator! it will instantiate newobject on scene
                        object containerInstance;
                        if (!AssetObjLocator.GetObjectByID(targetAssetIDS[i], out UnityEngine.Object unityObj))
                        {
                        }
                        containerInstance = unityObj;
                        addMethod.Invoke(listInstance, new object[] { containerInstance });
                    }
                    fieldInfo.SetValue(fieldHolder, listInstance);
                    break;
            }
            return true;
        }
        public override bool SaveData(object value)
        {
            throw new NotImplementedException();
        }
        public override bool LoadData(object source,out object value)
        {
            throw new NotImplementedException();
        }
    }
    [Serializable]
    public class ArrayWrapper<T>
    {
        [SerializeReference]
        public T value;
        public ArrayWrapper(T _value)
        {
            value = _value;
        }
    }
}



    // public class FloatData : SaveDataBase
    // {
    //     public float FloatValue
    //     {
    //         get
    //         {
    //             return floatValue;
    //         }
    //     }
    //     private float floatValue;
    //     public FloatData(float _value)
    //     {
    //         floatValue = _value;
    //         saveDataType = SaveDataType.Float;
    //     }
    //     public FloatData(SaveDataBase _saveDataBase)
    //     {
    //         stringSaveValue = _saveDataBase.StringSaveValue;
    //         saveDataType = SaveDataType.Float;
    //     }
    //     public override void OnSaveData()
    //     {
    //         stringSaveValue = floatValue.ToString();
    //     }
    //     public override void OnLoadData()
    //     {
    //         floatValue = float.Parse(stringSaveValue);
    //     }
    // }
    // public class IntData : SaveDataBase
    // {
    //     public int IntValue
    //     {
    //         get
    //         {
    //             return intValue;
    //         }
    //     }
    //     private int intValue;
    //     public IntData(int _value)
    //     {
    //         intValue = _value;
    //         saveDataType = SaveDataType.Int;
    //     }
    //     public IntData(SaveDataBase _saveDataBase)
    //     {
    //         stringSaveValue = _saveDataBase.StringSaveValue;
    //         saveDataType = SaveDataType.Int;
    //     }
    //     public override void OnSaveData()
    //     {
    //         stringSaveValue = intValue.ToString();
    //     }
    //     public override void OnLoadData()
    //     {
    //         intValue = int.Parse(stringSaveValue);
    //     }
    // }
    // public class StringData : SaveDataBase
    // {
    //     public string StringValue
    //     {
    //         get
    //         {
    //             return stringValue;
    //         }
    //     }
    //     private string stringValue;
    //     public StringData(string _value)
    //     {
    //         stringValue = _value;
    //         saveDataType = SaveDataType.String;
    //     }
    //     public StringData(SaveDataBase _saveDataBase)
    //     {
    //         stringSaveValue = _saveDataBase.StringSaveValue;
    //         saveDataType = SaveDataType.String;
    //     }
    //     public override void OnSaveData()
    //     {
    //         stringSaveValue = stringValue;
    //     }
    //     public override void OnLoadData()
    //     {
    //         stringValue = stringSaveValue;
    //     }
    // }
    // public class GenericClass<T> : SaveDataBase where T : class
    // {
    //     public T ClassValue
    //     {
    //         get
    //         {
    //             return classValue;
    //         }
    //     }
    //     private T classValue;
    //     private string typeFullName;
    //     private string classProjectName;
    //     public GenericClass(T _value, string _classProjectName = "")
    //     {
    //         classValue = _value;
    //         Type _type = typeof(T);
    //         typeFullName = _type.FullName;
    //         saveDataType = SaveDataType.GenericClass;
    //         if (string.IsNullOrEmpty(_classProjectName))
    //         {
    //             classProjectName = SimpleSaveHelper.MainNamespaceProjectName;
    //         }
    //         else
    //         {
    //             classProjectName = _classProjectName;
    //         }
    //         // Debug.Log(typeFullName);
    //     }
    //     public GenericClass(SaveDataBase _saveDataBase, string _classProjectName = "")
    //     {
    //         stringSaveValue = _saveDataBase.StringSaveValue;
    //         Type _type = typeof(T);
    //         typeFullName = _type.FullName;
    //         saveDataType = SaveDataType.GenericClass;
    //         if (string.IsNullOrEmpty(_classProjectName))
    //         {
    //             classProjectName = SimpleSaveHelper.MainNamespaceProjectName;
    //         }
    //         else
    //         {
    //             classProjectName = _classProjectName;
    //         }
    //     }
    //     public override void OnSaveData()
    //     {
    //         stringSaveValue = classProjectName + "|" + typeFullName + "|" + JsonUtility.ToJson(classValue);
    //     }
    //     public override void OnLoadData()
    //     {
    //         string[] splittedString = stringSaveValue.Split("|");
    //         if (splittedString.Length != 3)
    //         {
    //             SimpleSaveDebugger.DebugError("Error occured possibly due to modified save data");
    //             return;
    //         }
    //         classProjectName = splittedString[0];
    //         string jsonSaveData = splittedString[2];
    //         try
    //         {
    //             classValue = JsonUtility.FromJson<T>(jsonSaveData);
    //         }
    //         catch (System.Exception)
    //         {
    //             Type _type = typeof(T);
    //             string typeName = _type.FullName;
    //             SimpleSaveDebugger.DebugError("Modified class with name" + typeName + ", or modified save file detected");
    //             classValue = default(T);
    //         }
    //     }
    // }
