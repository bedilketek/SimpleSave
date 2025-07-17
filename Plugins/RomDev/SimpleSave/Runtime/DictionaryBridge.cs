using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace RomDev.SimpleSave
{
    [Serializable]
    public class DictionaryBridge<Tkey, Tvalue>
    {
        public List<Tkey> Keys
        {
            get
            {
                return keys;
            }
        }
        public List<Tvalue> Values
        {
            get
            {
                return values;
            }
        }
        [SerializeField]
        private List<Tkey> keys = new List<Tkey>();
        [SerializeReference]
        private List<Tvalue> values = new List<Tvalue>();
        #region Editor Variables
        [HideInInspector] public bool isSelected;
        [HideInInspector] public string varName;
        #endregion
        public bool CopyToDictionary(Dictionary<Tkey, Tvalue> serializableDict)
        {
            if(keys.Count != values.Count)
            {
                SimpleSaveDebugger.DebugError("Different element count between keys and value ! ");
                return false;
            }
            Tkey comparedvalue = default(Tkey);
            foreach(Tkey tkey in keys)
            {
                if (tkey.Equals(comparedvalue))
                {
                    SimpleSaveDebugger.DebugError("Same key value detected ! ");
                    return false;
                }
            }
            for(int i = 0; i < keys.Count ; i++)
            {
                serializableDict.Add(keys[i], values[i]);
            }
            return true;
        }
        public void CopyFromDictionary(Dictionary<Tkey, Tvalue> sourceDict)
        {
            keys.Clear();
            values.Clear();
            foreach (KeyValuePair<Tkey, Tvalue> sourceDictPair in sourceDict)
            {
                // Debug.Log("Good");
                keys.Add(sourceDictPair.Key);
                values.Add(sourceDictPair.Value);

            }
        } 
        public void AddData(Tkey _key, Tvalue _value)
        {
            keys.Add(_key);
            values.Add(_value);
        }
        public void SetValue(Tkey _key, Tvalue _value)
        {
            int keyIndex = keys.IndexOf(_key);
            if(keyIndex < 0 )
            {
                return;
            }
            values[keyIndex] = _value;
        }
        public void RemoveData(Tkey _key)
        {
            int keyIndex = keys.IndexOf(_key);
            if(keyIndex < 0 )
            {
                return;
            }
            keys.RemoveAt(keyIndex);
            values.RemoveAt(keyIndex);
        }
        public void RemoveDataByIndex(int indexTarget)
        {
            if ((keys.Count < (indexTarget - 1)) || indexTarget < 0)
            {
                return;
            }
            keys.RemoveAt(indexTarget);
            values.RemoveAt(indexTarget);
        }
        public bool ContainsKey(Tkey _key)
        {
            int keyIndex = keys.IndexOf(_key);
            if (keyIndex < 0)
            {
                // Debug.LogWarning("Can't find data");
                return false;
            }
            return true;
        }
        public Tvalue GetData(Tkey _key)
        {
            int keyIndex = keys.IndexOf(_key);
            if( keyIndex < 0 )
            {
                // Debug.LogWarning("Can't find data");
                return default;
            }
            return values[keyIndex];
        }
        public bool TryGetValue(Tkey _key, out Tvalue result)
        {
            int keyIndex = keys.IndexOf(_key);
            if( keyIndex < 0 )
            {
                SimpleSaveDebugger.DebugWarning("Can't find data");
                result = default;
                return false;
            }
            result = values[keyIndex];
            return true;
        }
        public Tvalue GetCopy(Tkey _key)
        {
            int keyIndex = keys.IndexOf(_key);
            if( keyIndex < 0 )
            {
                SimpleSaveDebugger.DebugWarning("Can't find data");
                return default;
            }
            return SimpleSaveHelper.DeepCopy(values[keyIndex]);
        }
        public void Clear()
        {
            keys.Clear();
            values.Clear();
        }
        public Dictionary<Tkey, Tvalue> GetAsDictionary()
        {
            Dictionary<Tkey, Tvalue> dict = new();
            for (int i = 0; i < keys.Count; i++)
            {
                dict.Add(keys[i], values[i]);
            }
            return dict;
        }
        
    }
}  
