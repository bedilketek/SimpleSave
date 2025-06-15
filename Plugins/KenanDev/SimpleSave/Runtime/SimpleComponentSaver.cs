using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;



#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KenanDev.SimpleSave
{
    [RequireComponent(typeof(SimpleSaveService))]
    public class SimpleComponentSaver : MonoBehaviour, ISimpleSave
    {
        [SerializeField]
        private Component targetComponent;
        [SerializeField]
        private List<SimpleSaveSign> simpleSaveSigns = new();
        [SerializeField]
        private int saverID;
        private const string SimpleComponentName = "SimpleComponentSaver";
        private Action onDataLoaded;
        private Action onDataSaved;
#if UNITY_EDITOR
        [SerializeField]
        private Component cachedComponent;
        [SerializeField]
        private string componentTypeName;
        public void ShowGUI()
        {
            EditorGUI.BeginChangeCheck();

            EditorGUI.BeginChangeCheck();
            targetComponent = (Component)EditorGUILayout.ObjectField("Target To Save: ", targetComponent, typeof(Component), true);
            if (EditorGUI.EndChangeCheck())
            {
                if (targetComponent != null && cachedComponent != null)
                {
                    if (targetComponent != cachedComponent)
                    {
                        simpleSaveSigns.Clear();
                    }
                    GatherSaveDataFromTarget();
                    cachedComponent = targetComponent;
                    componentTypeName = targetComponent.GetType().Name;
                }
                else if (targetComponent != null && cachedComponent == null)
                {
                    simpleSaveSigns.Clear();
                    GatherSaveDataFromTarget();
                    cachedComponent = targetComponent;
                    componentTypeName = targetComponent.GetType().Name;
                }
                else if (targetComponent == null)
                {
                    simpleSaveSigns.Clear();
                }
            }
            if (targetComponent != null)
            {
                EditorGUILayout.LabelField(componentTypeName);
            }
            if (GUILayout.Button("Refresh Data"))
            {
                GatherSaveDataFromTarget();
            }
            if (simpleSaveSigns.Count > 0 && targetComponent != null)
            {
                EditorGUILayout.LabelField("Save Data: ");
                for (int i = 0; i < simpleSaveSigns.Count; i++)
                {
                    simpleSaveSigns[i].ShowGUI();
                }
            }
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(this);
            }
        }
        private void GatherSaveDataFromTarget()
        {
            if (targetComponent == null) return;
            List<string> availableFieldNames = new();
            availableFieldNames = simpleSaveSigns.Select(x => x.fieldName).ToList();

            List<string> oldFieldNames = simpleSaveSigns.Select(x => x.fieldName).ToList();

            FieldInfo[] fieldInfos = targetComponent.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (FieldInfo fieldInfo in fieldInfos)
            {
                Attribute[] customAtts = fieldInfo.GetCustomAttributes(typeof(KenanSaveAttribute)).ToArray();
                if (customAtts.Length <= 0) continue;

                KenanSaveAttribute kenanAtt = (KenanSaveAttribute)customAtts.Where(x => x is KenanSaveAttribute).FirstOrDefault();
                if (kenanAtt == null) continue;
                if (availableFieldNames.Contains(fieldInfo.Name))
                {
                    SimpleSaveSign _targetsaveSign = simpleSaveSigns.Where(x => x.fieldName == fieldInfo.Name).FirstOrDefault();
                    _targetsaveSign.saveDataType = kenanAtt.SaveDataType;
                    if (_targetsaveSign != null)
                    {
                        if (string.IsNullOrEmpty(kenanAtt.CustomName))
                        {
                            _targetsaveSign.nameToShow = fieldInfo.Name;
                        }
                        else
                        {
                            _targetsaveSign.nameToShow = kenanAtt.CustomName;
                        }
                    }
                    continue;
                }

                availableFieldNames.Add(fieldInfo.Name);

                SimpleSaveSign simpleSaveSign = new();
                simpleSaveSign.fieldName = fieldInfo.Name;
                simpleSaveSign.saveDataType = kenanAtt.SaveDataType;
                if (string.IsNullOrEmpty(kenanAtt.CustomName))
                {
                    simpleSaveSign.nameToShow = fieldInfo.Name;
                }
                else
                {
                    simpleSaveSign.nameToShow = kenanAtt.CustomName;
                }
                simpleSaveSigns.Add(simpleSaveSign);
            }
            List<string> nameToDelete = oldFieldNames.Except(availableFieldNames).ToList();
            List<SimpleSaveSign> saveSignToDeletes = simpleSaveSigns.Where(x => nameToDelete.Contains(x.fieldName)).ToList();
            foreach (SimpleSaveSign saveSignToDelete in saveSignToDeletes)
            {
                simpleSaveSigns.Remove(saveSignToDelete);
            }
        }
#endif
        private void Reset()
        {
            saverID = GetInstanceID();
        }
        public void OnSaveData()
        {
            if (targetComponent == null) return;
            SimpleSaveData simpleSaveData = new();
            Type holdingType = targetComponent.GetType();
            foreach (SimpleSaveSign simpleSaveSign in simpleSaveSigns)
            {
                if (!simpleSaveSign.isSave) continue;
                FieldInfo fieldInfo = holdingType.GetField(simpleSaveSign.fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (fieldInfo == null) continue;
                object fieldValue = fieldInfo.GetValue(targetComponent);
                switch (simpleSaveSign.saveDataType)
                {
                    case SaveDataType.Int:
                        if (fieldValue is not int)
                        {
                            continue;
                        }
                        int intValue = (int)fieldValue;
                        simpleSaveData.SetIntValue(simpleSaveSign.fieldName, intValue);
                        break;
                    case SaveDataType.Float:
                        if (fieldValue is not float)
                        {
                            continue;
                        }
                        float floatValue = (float)fieldValue;
                        simpleSaveData.SetFloatValue(simpleSaveSign.fieldName, floatValue);
                        break;
                    case SaveDataType.String:
                        if (fieldValue is not string)
                        {
                            continue;
                        }
                        string stringValue = (string)fieldValue;
                        simpleSaveData.SetStringValue(simpleSaveSign.fieldName, stringValue);
                        break;
                    case SaveDataType.GenericClass:
                        try
                        {
                            Type typeOfGenericClass = fieldInfo.FieldType;
                            MethodInfo genericMethod = typeof(SimpleSaveData).GetMethod(SimpleSaveData.SetGenericClassDataName).MakeGenericMethod(typeOfGenericClass);
                            string targetProjectAssemblyName = "";
                            if (!simpleSaveSign.assemblyNameIsNamespace)
                            {
                                targetProjectAssemblyName = simpleSaveSign.projectAssemblyName;
                            }
                            else
                            {
                                targetProjectAssemblyName = typeOfGenericClass.Namespace;
                            }

                            if (string.IsNullOrEmpty(targetProjectAssemblyName))
                            {
                                targetProjectAssemblyName = SimpleSaveHelper.MainNamespaceProjectName;
                            }
                            // Debug.Log(targetProjectAssemblyName);
                            genericMethod.Invoke(simpleSaveData, new object[] { simpleSaveSign.fieldName, fieldValue,  targetProjectAssemblyName});
                        }
                        catch (Exception e)
                        {
                            SimpleSaveDebugger.DebugError("Failed to save Generic Class Data due to : " + e.Message);
                        }
                        break;
                }
            }
            SimpleSave.SetData(SimpleComponentName + saverID, simpleSaveData);
            onDataSaved?.Invoke();
        }
        public void OnLoadData()
        {
            if (targetComponent == null) return;
            Type holdingType = targetComponent.GetType();
            SimpleSaveData simpleSaveData = SimpleSave.GetSaveData(SimpleComponentName + saverID);
            if (simpleSaveData == null) return;
            foreach (SimpleSaveSign simpleSaveSign in simpleSaveSigns)
            {
                if (!simpleSaveSign.isSave) continue;
                FieldInfo fieldInfo = holdingType.GetField(simpleSaveSign.fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (fieldInfo == null) continue;
                Type fieldType = fieldInfo.FieldType;
                switch (simpleSaveSign.saveDataType)
                {
                    case SaveDataType.Int:
                        if (fieldType != typeof(int)) continue;
                        int intValue = simpleSaveData.GetIntData(simpleSaveSign.fieldName);
                        fieldInfo.SetValue(targetComponent, intValue);
                        break;
                    case SaveDataType.Float:
                        if (fieldType != typeof(float)) continue;
                        float floatValue = simpleSaveData.GetFloatData(simpleSaveSign.fieldName);
                        fieldInfo.SetValue(targetComponent, floatValue);
                        break;
                    case SaveDataType.String:
                        if (fieldType != typeof(string)) continue;
                        string stringValue = simpleSaveData.GetStringData(simpleSaveSign.fieldName);
                        fieldInfo.SetValue(targetComponent, stringValue);
                        break;
                    case SaveDataType.GenericClass:
                        object gainedValue = null;
                        try
                        {
                            MethodInfo genericMethod = typeof(SimpleSaveData).GetMethod(SimpleSaveData.GetGenericClassDataName).MakeGenericMethod(fieldType);
                            gainedValue = genericMethod.Invoke(simpleSaveData, new object[] { simpleSaveSign.fieldName });
                            if (gainedValue.GetType() != fieldType)
                            {
                                continue;
                            }
                            fieldInfo.SetValue(targetComponent, gainedValue);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError("Can't do reflection due to : " + e.Message);
                        }
                        // Debug.Log(gainedValue.GetType().Name);
                        break;
                }
            }
            onDataLoaded?.Invoke();
        }
        public void Subscribe_OnDataSaved(Action _callback)
        {
            onDataSaved += _callback;
        }
        public void Subscribe_OnDataLoaded(Action _callback)
        {
            onDataLoaded += _callback;
        }
        public void Unsubscribe_OnDataSaved(Action _callback)
        {
            onDataSaved -= _callback;
        }
        public void Unsubscribe_OnDataLoaded(Action _callback)
        {
            onDataLoaded -= _callback;
        }
    }
    [Serializable]
    public class SimpleSaveSign
    {
        public bool isSave = true;
        public string fieldName;
        public string nameToShow;
        public SaveDataType saveDataType;
        public bool assemblyNameIsNamespace;
        public string projectAssemblyName;
#if UNITY_EDITOR
        public void ShowGUI()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(nameToShow + " : ");
            isSave = EditorGUILayout.Toggle(isSave);
            EditorGUILayout.EndHorizontal();
            if (saveDataType == SaveDataType.GenericClass)
            {
                assemblyNameIsNamespace = EditorGUILayout.Toggle("Asmdf name is namespace? ", assemblyNameIsNamespace);
                if (!assemblyNameIsNamespace)
                {
                    projectAssemblyName = EditorGUILayout.TextField("Asmdf Name: ", projectAssemblyName);
                }
            }
        }
        #endif
    }
}

