using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
namespace KenanDev.SimpleSave
{
    public static class SimpleSave
    {
        public static string SaveFolderPath
        {
            get
            {
                return AutoGetAndMakeSaveFolder();
            }
        }
        public static string BaseSaveFileName
        {
            get
            {
                return SimpleSaveManager.BaseSaveName;
            }
        }
        public static SimpleSaveManagerDataSO SimpleSaveManager
        {
            get
            {
                if (simpleSaveDataSO == null)
                {
                    simpleSaveDataSO = Resources.Load<SimpleSaveManagerDataSO>(SimpleSaveHelper.DefaultSimpleSaveManagerDataName);
                }
                return simpleSaveDataSO;
            }
        }
        public static int SaveFileCount
        {
            get
            {
                return gameSavePathData.gameSavePathPacks.Count;
            }
        }
        private static string SystemSaveFolder
        {
            get
            {
                return AutoGetAndMakeSystemFolder();
            }
        }
        private static string SystemSaveFullFileName
        {
            get
            {
                return SystemSaveFolder + Path.DirectorySeparatorChar + SimpleSaveHelper.DefaultSavePackFileName + SimpleSaveHelper.JSON_EXT;
            }
        }
        [NonSerialized]
        private static Dictionary<int, SimpleSaveService> runningServices = new();
        [NonSerialized]
        private static Dictionary<string, SimpleSaveData> currentSaveData = new();
        [NonSerialized]
        private static GameSavePathData gameSavePathData = new();
        [NonSerialized]
        private static bool isInitialized;
        [NonSerialized]
        private static SimpleSaveManagerDataSO simpleSaveDataSO;
        /**
		 * <summary>Save game data using Unique ID.</summary>
		 * <param name="saveName">Game save Unique ID.</param>
		 * <returns> Is saving process succeeded or not</returns>
		 */
        public static bool SaveGameByName(string saveName)
        {
            AutoInitialize();
            int maxSaveCount = 0;
            if (SimpleSaveManager == null)
            {
                SimpleSaveDebugger.DebugWarning("Simple Save is not detected, using default max save file count");
                maxSaveCount = SimpleSaveHelper.DefaultMaxSave;
            }
            else
            {
                maxSaveCount = SimpleSaveManager.MaxSaveCount;
            }
            if (SaveFileCount >= maxSaveCount)
            {
                SimpleSaveDebugger.DebugWarning("Game already reach it's max save file count");
                return false;
            }
            if (string.IsNullOrEmpty(saveName))
            {
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                saveName = timestamp;
            }
            GameSaveData finalSaveData = new();
            foreach (KeyValuePair<int, SimpleSaveService> runningServicesPair in runningServices)
            {
                runningServicesPair.Value.OnStartSaveProcess();
            }
            foreach (SimpleSaveData saveData in currentSaveData.Values)
            {
                saveData.OnSaveData();
            }
            finalSaveData.simpleSaveData.CopyFromDictionary(currentSaveData);
            string jsonString = JsonUtility.ToJson(finalSaveData);
            string saveFileName = "";
            GameSavePathPack foundedGameSavePath = GetGameSavePathPackByName(saveName);
            if (foundedGameSavePath == null)
            {
                int targetSaveIndex = GetMaxSaveFileIndex() + 1;
                saveFileName = GetUniqueFileName(SaveFolderPath, BaseSaveFileName, SimpleSaveHelper.JSON_EXT);
                GameSavePathPack gameSavePathPack = new(saveName, targetSaveIndex, saveFileName);
                gameSavePathData.gameSavePathPacks.Add(gameSavePathPack);
                SaveGameSavePathData();
            }
            else
            {
                SimpleSaveDebugger.Debug("Overwrite Save file by name: " + saveName);
                saveFileName = foundedGameSavePath.saveFileName;
            }
            string fullSaveName = SaveFolderPath + Path.DirectorySeparatorChar + saveFileName;
            File.WriteAllText(fullSaveName, jsonString);
            SimpleSaveDebugger.Debug("Save data by name : " + saveName + " saved successfully");
            return true;
        }
        /**
		 * <summary>Load game data using Unique ID.</summary>
		 * <param name="saveName">Game save Unique ID.</param>
		 * <returns> Is loading process succeeded or not</returns>
		 */
        public static bool LoadGameByName(string saveName)
        {
            AutoInitialize();
            GameSavePathPack foundedSavePack = GetGameSavePathPackByName(saveName);
            if (foundedSavePack == null)
            {
                SimpleSaveDebugger.DebugError("No save file founded by name : " + saveName);
                return false;
            }
            if (!TryLoadData(foundedSavePack.saveFileName))
            {
                SimpleSaveDebugger.DebugError("Failed to load save file by name: " + saveName);
                return false;
            }
            foreach (KeyValuePair<int, SimpleSaveService> runningServicesPair in runningServices)
            {
                runningServicesPair.Value.OnStartLoadProcess();
            }
            SimpleSaveDebugger.Debug("Save data by name : " + saveName + " loaded successfully");
            return true;
        }
        /**
		 * <summary>Save SimpleSaveData with Unique ID.</summary>
		 * <param name="saveKey">SimpleSaveData Unique ID.</param>
         * <param name="data">Container of datas to save.</param>
		 */
        public static void SetData(string saveKey, SimpleSaveData data)
        {
            AutoInitialize();
            if (currentSaveData.ContainsKey(saveKey))
            {
                currentSaveData[saveKey] = data;
            }
            else
            {
                currentSaveData.Add(saveKey, data);
            }
        }
        /**
		 * <summary>Get SimpleSaveData by Unique ID.</summary>
		 * <param name="saveKey">SimpleSaveData's Unique ID.</param>
		 * <returns> SimpleSaveData</returns>
		 */
        public static SimpleSaveData GetSaveData(string saveKey)
        {
            AutoInitialize();
            if (!currentSaveData.TryGetValue(saveKey, out SimpleSaveData data))
            {
                // SimpleSaveDebugger.DebugError("There's no Save data with key: " + saveKey);
                return null;
            }
            else
            {
                return data;
            }
        }
         /**
		 * <summary>Delete Save data by specific name.</summary>
		 * <param name="saveName">Name of Save data to be deleted.</param>
		 * <returns>Is deletion process succeeded? </returns>
		 */
        public static bool DeleteSaveDataByName(string saveName)
        {
            AutoInitialize();
            GameSavePathPack foundedSavePack = GetGameSavePathPackByName(saveName);
            if (foundedSavePack == null)
            {
                SimpleSaveDebugger.DebugWarning("There's no save file by name : " + saveName);
                return false;
            }
            string fullSaveName = SaveFolderPath + Path.DirectorySeparatorChar + foundedSavePack.saveFileName;
            if (!File.Exists(fullSaveName))
            {
                SimpleSaveDebugger.DebugError("There's no file with path : " + fullSaveName);
                return false;
            }
            File.Delete(fullSaveName);
            gameSavePathData.gameSavePathPacks.Remove(foundedSavePack);
            SaveGameSavePathData();
            SimpleSaveDebugger.Debug("Save data by name : " + saveName + " deleted successfully");
            return true;
        }
        /**
		 * <summary>Delete all save file for this game.</summary>
		 */
        public static void DeleteAllSaveFile()
        {
            AutoInitialize();
            DirectoryInfo directoryInfo = new DirectoryInfo(SaveFolderPath);
            foreach (FileInfo fileInfo in directoryInfo.GetFiles())
            {
                File.Delete(fileInfo.FullName);
            }
            gameSavePathData.gameSavePathPacks.Clear();
            SaveGameSavePathData();
        }
        /**
		 * <summary>Get names of this game's Save datas.</summary>
		 * <returns>Names of Save datas </returns>
		 */
        public static string[] GetGameSaveNames()
        {
            AutoInitialize();
            return gameSavePathData.gameSavePathPacks.Select(x => x.saveName).ToArray();
        }
        /**
		 * <summary>Check if certain Save Data by name is existed.</summary>
		 * <param name="saveName">Target name to be checked.</param>
		 * <returns>Is Save data by name already existed? </returns>
		 */
        public static bool IsSaveByNameExisted(string saveName)
        {
            AutoInitialize();
            GameSavePathPack foundedSavePack = gameSavePathData.gameSavePathPacks.Where(x => x.saveName == saveName).FirstOrDefault();
            if (foundedSavePack == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public static void RegisterRunningService(SimpleSaveService simpleSaveService)
        {
            runningServices.Add(simpleSaveService.ServiceID, simpleSaveService);
        }
        public static void UnregisterRunningService(SimpleSaveService simpleSaveService)
        {
            runningServices.Remove(simpleSaveService.ServiceID);
        }
        private static string AutoGetAndMakeSaveFolder()
        {
            string saveFolderName = "";
            if (simpleSaveDataSO != null)
            {
                saveFolderName = simpleSaveDataSO.SaveFolderName;
                if (string.IsNullOrEmpty(saveFolderName))
                {
                    SimpleSaveDebugger.DebugWarning("No save folder path entered,\n using default folder instead which is : " + Application.persistentDataPath + Path.DirectorySeparatorChar + SimpleSaveHelper.DefaultSaveFolderName);
                    saveFolderName = SimpleSaveHelper.DefaultSaveFolderName;
                }
            }
            else
            {
                SimpleSaveDebugger.DebugWarning("No save folder path entered,\n using default folder instead which is : " + Application.persistentDataPath + Path.DirectorySeparatorChar + SimpleSaveHelper.DefaultSaveFolderName);
                saveFolderName = SimpleSaveHelper.DefaultSaveFolderName;
            }
            string saveFolderPath = Application.persistentDataPath + Path.DirectorySeparatorChar + saveFolderName;
            if (!Directory.Exists(saveFolderPath))
            {
                Directory.CreateDirectory(saveFolderPath);
            }
            return saveFolderPath;
        }
        private static string AutoGetAndMakeSystemFolder()
        {
            string fullSystemFolderPath = Application.persistentDataPath + Path.DirectorySeparatorChar + SimpleSaveHelper.DefaultSystemSaveFolder;
            if (!Directory.Exists(fullSystemFolderPath))
            {
                Directory.CreateDirectory(fullSystemFolderPath);
            }
            return fullSystemFolderPath;
        }
        public static void AutoInitialize()
        {
            if (isInitialized) return;
            isInitialized = true;
            AutoGetAndMakeSaveFolder();
            AutoGetAndMakeSystemFolder();
            AutoLoadAndMakeGameSavePathData();
        }
        private static bool TryLoadData(string saveFileName)
        {
            string fullSaveName = SaveFolderPath + Path.DirectorySeparatorChar + saveFileName;
            if (!File.Exists(fullSaveName))
            {
                SimpleSaveDebugger.DebugError("file with path : " + fullSaveName + "\n isn't existed");
                return false;
            }
            string jsonString = "";
            try
            {
                jsonString = File.ReadAllText(fullSaveName);
            }
            catch (System.Exception e)
            {
                SimpleSaveDebugger.DebugError("Failed to read save file at path: " + fullSaveName + "\n due to: " + e.Message);
                return false;
            }
            currentSaveData.Clear();
            GameSaveData finalSaveData = JsonUtility.FromJson<GameSaveData>(jsonString);
            foreach (SimpleSaveData simpleSaveData in finalSaveData.simpleSaveData.Values)
            {
                simpleSaveData.OnLoadData();
            }
            finalSaveData.simpleSaveData.CopyToDictionary(currentSaveData);
            // foreach (KeyValuePair<string, SimpleSaveData> simpleSaveDataPair in currentSaveData)
            // {
            //     currentSaveData.Add(simpleSaveDataPair.Key, simpleSaveDataPair.Value);
            //     simpleSaveDataPair.Value.OnLoadData();
            // }
            return true;
        }
        private static string GetUniqueFileName(string directory, string baseFileName, string extension)
        {
            int count = 0;
            string tempFileName = $"{baseFileName}({count})";
            string fullPath = Path.Combine(directory, tempFileName + extension);
            while (File.Exists(fullPath))
            {
                count++;
                tempFileName = $"{baseFileName}({count})";
                fullPath = Path.Combine(directory, tempFileName + extension);
            }

            return tempFileName + extension;
        }
        private static GameSavePathPack GetGameSavePathPackByName(string saveName)
        {
            return gameSavePathData.gameSavePathPacks.Where(x => x.saveName == saveName).FirstOrDefault();
        }
        private static int GetMaxSaveFileIndex()
        {
            int max = 0;
            foreach (GameSavePathPack gameSavePathPack in gameSavePathData.gameSavePathPacks)
            {
                if (gameSavePathPack.saveIndex > max)
                {
                    max = gameSavePathPack.saveIndex;
                }
            }
            return max;
        }
        private static void SaveGameSavePathData()
        {
            string jsonString = JsonUtility.ToJson(gameSavePathData);
            File.WriteAllText(SystemSaveFullFileName, jsonString);
        }
        private static string AutoLoadAndMakeGameSavePathData()
        {
            if (!File.Exists(SystemSaveFullFileName))
            {
                SaveGameSavePathData();
            }
            string jsonString = "";
            try
            {
                jsonString = File.ReadAllText(SystemSaveFullFileName);
            }
            catch (System.Exception e)
            {
                SimpleSaveDebugger.DebugError("Failed to read save file at path: " + SystemSaveFullFileName + "\n due to: " + e.Message);
                return null;
            }
            gameSavePathData = JsonUtility.FromJson<GameSavePathData>(jsonString); 
            return SystemSaveFullFileName;
        }
    }
    [Serializable]
    public class GameSavePathData
    {
        public List<GameSavePathPack> gameSavePathPacks = new();
    }
    [Serializable]
    public class GameSavePathPack
    {
        public string saveName;
        public int saveIndex;
        // public string fullSavePath;
        public string saveFileName;
        public GameSavePathPack(string _saveName, int _saveIndex, string _saveFileName)
        {
            saveName = _saveName;
            saveIndex = _saveIndex;
            saveFileName = _saveFileName;
        }
    }
    [Serializable]
    public class GameSaveData
    {
        public DictionaryBridge<string, SimpleSaveData> simpleSaveData = new();
    }
    [Serializable]
    public class SimpleSaveData
    {
        public const string SetGenericClassDataName = "SetGenericClassData";
        public const string GetGenericClassDataName = "GetGenericClassData";
        [SerializeField]
        private DictionaryBridge<string, SaveDataBase> saveDataBridge = new();
        private Dictionary<string, SaveDataBase> saveDataDict = new();
        public void OnSaveData()
        {
            saveDataBridge.CopyFromDictionary(saveDataDict);
            foreach (SaveDataBase saveData in saveDataDict.Values)
            {
                saveData.OnSaveData();
            }
        }
        public void OnLoadData()
        {
            saveDataBridge.CopyToDictionary(saveDataDict);
            Dictionary<string, SaveDataBase> overwriterDict = new();
            foreach (string saveDataKey in saveDataDict.Keys)
            {
                switch (saveDataDict[saveDataKey].SaveDataType)
                {
                    case SaveDataType.Float:
                        FloatData floatData = new(saveDataDict[saveDataKey]);
                        floatData.OnLoadData();
                        overwriterDict.Add(saveDataKey, floatData);
                        break;
                    case SaveDataType.Int:
                        IntData intData = new(saveDataDict[saveDataKey]);
                        intData.OnLoadData();
                        overwriterDict.Add(saveDataKey, intData);
                        break;
                    case SaveDataType.String:
                        StringData stringData = new(saveDataDict[saveDataKey]);
                        stringData.OnLoadData();
                        overwriterDict.Add(saveDataKey, stringData);
                        break;
                    case SaveDataType.GenericClass:
                        SaveDataBase saveDataBase = SetOnLoadGenericClassData(saveDataDict[saveDataKey]);
                        if (saveDataBase != null)
                        {
                            overwriterDict.Add(saveDataKey, saveDataBase);
                        }
                        else
                        {
                            // Debug.Log("null");
                        }
                        break;

                }
            }
            saveDataDict = overwriterDict;
        }
        public void SetFloatValue(string saveKey, float _value)
        {
            FloatData floatData = new(_value);
            SetSaveData(saveKey, floatData);
        }
        public void SetIntValue(string saveKey, int _value)
        {
            IntData intData = new(_value);
            SetSaveData(saveKey, intData);
        }
        public void SetStringValue(string saveKey, string _value)
        {
            StringData stringData = new(_value);
            SetSaveData(saveKey, stringData);
        }
        public void SetGenericClassData<T>(string saveKey, T _value, string classProjectName = "") where T : class
        {
            Type _type = typeof(T);
            if (_type.IsEquivalentTo(typeof(UnityEngine.Object)) || _type.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                SimpleSaveDebugger.DebugError("Can't Save Unity Object ! ");
                return;
            }
            GenericClass<T> genericClass = new(_value, classProjectName);
            // Debug.Log(classProjectName);
            SetSaveData(saveKey, genericClass);
            // Debug.Log(_type.Name);
        }
        public float GetFloatData(string saveKey)
        {
            SaveDataBase saveDataBase = GetSaveData(saveKey);
            if (saveDataBase == null)
            {
                // Debug.LogError("No Data with name : " + saveKey);
                return -1f;
            }
            FloatData floatData = saveDataBase as FloatData;
            if (floatData == null)
            {
                // Debug.LogError("Data with key : " + saveKey + "is not float value");
                return -1f;
            }
            return floatData.FloatValue;
        }
        public int GetIntData(string saveKey)
        {
            SaveDataBase saveDataBase = GetSaveData(saveKey);
            if (saveDataBase == null)
            {
                // Debug.LogError("No Data with name : " + saveKey);
                return -1;
            }
            IntData intData = saveDataBase as IntData;
            if (intData == null)
            {
                SimpleSaveDebugger.DebugError("Data with key : " + saveKey + "is not int value");
                return -1;
            }
            return intData.IntValue;
        }
        public string GetStringData(string saveKey)
        {
            SaveDataBase saveDataBase = GetSaveData(saveKey);
            if (saveDataBase == null)
            {
                // Debug.LogError("No Data with name : " + saveKey);
                return null;
            }
            StringData stringData = saveDataBase as StringData;
            if (stringData == null)
            {
                // Debug.LogError("Data with key : " + saveKey + "is not string value");
                return null;
            }
            return stringData.StringValue;
        }
        public T GetGenericClassData<T>(string saveKey) where T : class
        {
            SaveDataBase saveDataBase = GetSaveData(saveKey);
            if (saveDataBase == null)
            {
                // Debug.LogError("No Data with name : " + saveKey);
                return null;
            }
            // Debug.Log(typeof(T).Name);
            GenericClass<T> genericClassData = saveDataBase as GenericClass<T>;
            if (genericClassData == null)
            {
                // Debug.LogError("Data with key : " + saveKey + "is not " + typeof(T).Name + " class");
                return null;
            }
            return genericClassData.ClassValue;
        }
        private SaveDataBase GetSaveData(string saveKey)
        {
            if (!saveDataDict.TryGetValue(saveKey, out SaveDataBase _data))
            {
                return null;
            }
            return _data;
        }
        private void SetSaveData(string saveKey, SaveDataBase newData)
        {
            if (!saveDataDict.ContainsKey(saveKey))
            {
                saveDataDict.Add(saveKey, newData);
            }
            else
            {
                saveDataDict[saveKey] = newData;
            }
        }
        private SaveDataBase SetOnLoadGenericClassData(SaveDataBase saveDataBase)
        {
            string[] splittedString = saveDataBase.StringSaveValue.Split("|");
            if (splittedString.Length != 3)
            {
                SimpleSaveDebugger.DebugError("Error occured possibly due to modified save data");
                return null;
            }
            string typeFullAssemblyName = splittedString[1] + ", " + splittedString[0] + ", " + SimpleSaveHelper.HalfEndAssemblyName;
            try
            {
                Type savedType = Type.GetType(typeFullAssemblyName);
                if (savedType == null)
                {
                    SimpleSaveDebugger.DebugError("There's no class with name : " + typeFullAssemblyName);
                    return null;
                }
                Type genericType = typeof(GenericClass<>);
                Type[] typeArgs = { savedType };
                Type insertedArgGenericType = genericType.MakeGenericType(typeArgs);
                object[] finalArgs = { saveDataBase, splittedString[0] };
                object finalGenericType = Activator.CreateInstance(insertedArgGenericType, finalArgs);
                SaveDataBase finalSaveData = (SaveDataBase)finalGenericType;
                finalSaveData.OnLoadData();
                return finalSaveData;
            }
            catch (System.Exception e)
            {
                SimpleSaveDebugger.DebugError("Error occured: " + e.Message);
                return null;
            }
        }
    }
    [Serializable]
    public class SaveDataBase
    {
        public SaveDataType SaveDataType
        {
            get
            {
                return saveDataType;
            }
        }
        public string StringSaveValue
        {
            get
            {
                return stringSaveValue;
            }
        }
        [SerializeField]
        protected string stringSaveValue;
        [SerializeField]
        protected SaveDataType saveDataType;
        public virtual void OnSaveData()
        { }
        public virtual void OnLoadData()
        { }
    }
    public class FloatData : SaveDataBase
    {
        public float FloatValue
        {
            get
            {
                return floatValue;
            }
        }
        private float floatValue;
        public FloatData(float _value)
        {
            floatValue = _value;
            saveDataType = SaveDataType.Float;
        }
        public FloatData(SaveDataBase _saveDataBase)
        {
            stringSaveValue = _saveDataBase.StringSaveValue;
            saveDataType = SaveDataType.Float;
        }
        public override void OnSaveData()
        {
            stringSaveValue = floatValue.ToString();
        }
        public override void OnLoadData()
        {
            floatValue = float.Parse(stringSaveValue);
        }
    }
    public class IntData : SaveDataBase
    {
        public int IntValue
        {
            get
            {
                return intValue;
            }
        }
        private int intValue;
        public IntData(int _value)
        {
            intValue = _value;
            saveDataType = SaveDataType.Int;
        }
        public IntData(SaveDataBase _saveDataBase)
        {
            stringSaveValue = _saveDataBase.StringSaveValue;
            saveDataType = SaveDataType.Int;
        }
        public override void OnSaveData()
        {
            stringSaveValue = intValue.ToString();
        }
        public override void OnLoadData()
        {
            intValue = int.Parse(stringSaveValue);
        }
    }
    public class StringData : SaveDataBase
    {
        public string StringValue
        {
            get
            {
                return stringValue;
            }
        }
        private string stringValue;
        public StringData(string _value)
        {
            stringValue = _value;
            saveDataType = SaveDataType.String;
        }
        public StringData(SaveDataBase _saveDataBase)
        {
            stringSaveValue = _saveDataBase.StringSaveValue;
            saveDataType = SaveDataType.String;
        }
        public override void OnSaveData()
        {
            stringSaveValue = stringValue;
        }
        public override void OnLoadData()
        {
            stringValue = stringSaveValue;
        }
    }
    public class GenericClass<T> : SaveDataBase where T : class
    {
        public T ClassValue
        {
            get
            {
                return classValue;
            }
        }
        private T classValue;
        private string typeFullName;
        private string classProjectName;
        public GenericClass(T _value, string _classProjectName = "")
        {
            classValue = _value;
            Type _type = typeof(T);
            typeFullName = _type.FullName;
            saveDataType = SaveDataType.GenericClass;
            if (string.IsNullOrEmpty(_classProjectName))
            {
                classProjectName = SimpleSaveHelper.MainNamespaceProjectName;
            }
            else
            {
                classProjectName = _classProjectName;
            }
            // Debug.Log(typeFullName);
        }
        public GenericClass(SaveDataBase _saveDataBase, string _classProjectName = "")
        {
            stringSaveValue = _saveDataBase.StringSaveValue;
            Type _type = typeof(T);
            typeFullName = _type.FullName;
            saveDataType = SaveDataType.GenericClass;
            if (string.IsNullOrEmpty(_classProjectName))
            {
                classProjectName = SimpleSaveHelper.MainNamespaceProjectName;
            }
            else
            {
                classProjectName = _classProjectName;
            }
        }
        public override void OnSaveData()
        {
            stringSaveValue = classProjectName + "|" + typeFullName + "|" + JsonUtility.ToJson(classValue);
        }
        public override void OnLoadData()
        {
            string[] splittedString = stringSaveValue.Split("|");
            if (splittedString.Length != 3)
            {
                SimpleSaveDebugger.DebugError("Error occured possibly due to modified save data");
                return;
            }
            classProjectName = splittedString[0];
            string jsonSaveData = splittedString[2];
            try
            {
                classValue = JsonUtility.FromJson<T>(jsonSaveData);
            }
            catch (System.Exception)
            {
                Type _type = typeof(T);
                string typeName = _type.FullName;
                SimpleSaveDebugger.DebugError("Modified class with name" + typeName + ", or modified save file detected");
                classValue = default(T);
            }
        }
    }
    public enum SaveDataType : byte
    {
        Int = 0,
        Float = 1,
        String = 2,
        GenericClass = 3
    }
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
        [SerializeField]
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
    // [System.Serializable]
    // public class SerializedDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    // {
    //     [SerializeField] private List<TKey> keys = new List<TKey>();
    //     [SerializeField] private List<TValue> values = new List<TValue>();
    //     // save the dictionary to lists
    //     public void OnBeforeSerialize()
    //     {
    //         keys.Clear();
    //         values.Clear();
    //         foreach (KeyValuePair<TKey, TValue> pair in this) 
    //         {
    //             keys.Add(pair.Key);
    //             values.Add(pair.Value);
    //         }
    //     }

    //     // load the dictionary from lists
    //     public void OnAfterDeserialize()
    //     {
    //         this.Clear();
    //         if (keys.Count != values.Count) 
    //         {
    //             Debug.LogError("Tried to deserialize a SerializableDictionary, but the amount of keys ("
    //                 + keys.Count + ") does not match the number of values (" + values.Count 
    //                 + ") which indicates that something went wrong");
    //         }

    //         for (int i = 0; i < keys.Count; i++) 
    //         {
    //             this.Add(keys[i], values[i]);
    //         }
    //     }
    // }
}

