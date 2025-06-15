using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace KenanDev.SimpleSave
{
    public class SimpleSaveService : MonoBehaviour
    {
        public int ServiceID
        {
            get
            {
                return serviceID;
            }
        }
        [SerializeField]
        private int serviceID;
        [SerializeField]
        private bool dataSaveAndLoadActive = true;
        [SerializeField]
        private bool overwriteGlobalSetting;
        [SerializeField]
        private bool autoLoadOnAwake;
        [SerializeField]
        private bool autoSaveOnDestroy;
        private List<ISimpleSave> simpleSaveClients = new();
#if UNITY_EDITOR
        public void ShowGUI()
        {
            EditorGUI.BeginChangeCheck();
            dataSaveAndLoadActive = EditorGUILayout.Toggle("Save and load data? ", dataSaveAndLoadActive);
            overwriteGlobalSetting = EditorGUILayout.Toggle("Overwrite Global Setting? ", overwriteGlobalSetting);
            if (overwriteGlobalSetting)
            {
                autoLoadOnAwake = EditorGUILayout.Toggle("Auto Load On Awake? ", autoLoadOnAwake);
                autoSaveOnDestroy = EditorGUILayout.Toggle("Auto Save On Destroy? ", autoSaveOnDestroy);
            }
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(this);
            }
        }
        void Reset()
        {
            serviceID = GetInstanceID();
        }
#endif
        private void Awake()
        {
            SimpleSave.RegisterRunningService(this);
            ISimpleSave[] _simpleSaveClients = GetComponents<ISimpleSave>();
            simpleSaveClients = _simpleSaveClients.ToList();

            if (IsShouldLoadData())
            {
                LoadClientsDatas();
            }
        }
        private void OnDestroy()
        {
            if (IsShouldSaveData())
            {
                SaveClientsDatas();
            }
            SimpleSave.UnregisterRunningService(this);
        }
        public void OnStartSaveProcess()
        {
            if (!dataSaveAndLoadActive) return;
            SaveClientsDatas();
        }
        public void OnStartLoadProcess()
        {
            if (!dataSaveAndLoadActive) return;
            LoadClientsDatas();
        }
        public void SaveClientsDatas()
        {
            foreach (ISimpleSave simpleSaveClient in simpleSaveClients)
            {
                simpleSaveClient.OnSaveData();
            }
        }
        public void LoadClientsDatas()
        {
            foreach (ISimpleSave simpleSaveClient in simpleSaveClients)
            {
                simpleSaveClient.OnLoadData();
            }
        }
        private bool IsShouldLoadData()
        {
            if (!dataSaveAndLoadActive) return false;
            if (overwriteGlobalSetting)
            {
                if (autoLoadOnAwake)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            if (SimpleSave.SimpleSaveManager == null)
            {
                return true;
            }
            if (SimpleSave.SimpleSaveManager.DefaultLoadOnAwake)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        private bool IsShouldSaveData()
        {
            if (!dataSaveAndLoadActive) return false;
            if (overwriteGlobalSetting)
            {
                if (autoSaveOnDestroy)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            if (SimpleSave.SimpleSaveManager == null)
            {
                return true;
            }
            if (SimpleSave.SimpleSaveManager.DefaultSaveOnDestroy)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

