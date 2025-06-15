using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KenanDev.SimpleSave
{
    public static class SimpleSaveDebugger
    {
        private static SimpleSaveManagerDataSO SimpleSaveData
        {
            get
            {
                if (simpleSaveData == null)
                {
                    simpleSaveData = Resources.Load<SimpleSaveManagerDataSO>(SimpleSaveHelper.DefaultSimpleSaveManagerDataName);
                }
                return simpleSaveData;
            }
        }
        [NonSerialized]
        private static SimpleSaveManagerDataSO simpleSaveData;
        public static void Debug(string message)
        {
            if (SimpleSaveData == null)
            {
                UnityEngine.Debug.Log(message);
                return;
            }

            if (SimpleSaveData.ShowRegularLog)
            {
                UnityEngine.Debug.Log(message);
            }
        }
        public static void DebugWarning(string message)
        {
            if (SimpleSaveData == null)
            {
                UnityEngine.Debug.LogWarning(message);
                return;
            }

            if (SimpleSaveData.ShowWarningLog)
            {
                UnityEngine.Debug.LogWarning(message);
            }
        }
        public static void DebugError(string message)
        {
            if (SimpleSaveData == null)
            {
                UnityEngine.Debug.LogError(message);
                return;
            }

            if (SimpleSaveData.ShowErrorLog)
            {
                UnityEngine.Debug.LogError(message);
            }
        }
    }
}
