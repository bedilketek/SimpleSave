using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;
namespace RomDev.SimpleSave
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
        private readonly static SS_VersionData currentSimpleSaveVer = new(1, 0, 0);
        [NonSerialized]
        private static Dictionary<int, SimpleSaveService> runningServices = new();
        [NonSerialized]
        private static Dictionary<string, SimpleSaveData> currentSaveData = new();
        [NonSerialized]
        private static GameSavePathData gameSavePathData = new(currentSimpleSaveVer);
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
            GameSaveData finalSaveData = new(currentSimpleSaveVer);
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
            Debug.Log(fullSaveName);
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
            CheckSaveFileCompatibility(finalSaveData.savedAsVersion, "Save Data File ", true, true, true);
            finalSaveData.savedAsVersion = currentSimpleSaveVer;
            foreach (SimpleSaveData simpleSaveData in finalSaveData.simpleSaveData.Values)
            {
                simpleSaveData.OnLoadData();
            }
            finalSaveData.simpleSaveData.CopyToDictionary(currentSaveData);
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
            CheckSaveFileCompatibility(gameSavePathData.savedAsVersion, "Simple Save System File", true, true, true);
            gameSavePathData.savedAsVersion = currentSimpleSaveVer;
            return SystemSaveFullFileName;
        }
        private static bool CheckSaveFileCompatibility(SS_VersionData versionToCheck, string saveSourceName, bool considerMajor = true, bool considerMinor = true, bool considerPatch = false)
        {
            bool isOlder = false;
            if (considerMajor)
            {
                if (versionToCheck.MajorVersion < currentSimpleSaveVer.MajorVersion)
                {
                    isOlder = true;
                }
            }
            if (considerMinor)
            {
                if (versionToCheck.MinorVersion < currentSimpleSaveVer.MinorVersion)
                {
                    isOlder = true;
                }
            }
            if (considerPatch)
            {
                if (versionToCheck.PatchVersion < currentSimpleSaveVer.PatchVersion)
                {
                    isOlder = true;
                }
            }
            if (isOlder)
            {
                SimpleSaveDebugger.DebugWarning(saveSourceName + " has been saved using version " + versionToCheck.VersionName +
                ", current Version is " + currentSimpleSaveVer.VersionName + ", error's may occured");
                return false;
            }
            return true;
        }
    }
    [Serializable]
    public class GameSavePathData
    {
        public SS_VersionData savedAsVersion;
        public List<GameSavePathPack> gameSavePathPacks = new();
        public GameSavePathData(SS_VersionData _version)
        {
            savedAsVersion = _version;
        }
    }
    [Serializable]
    public class GameSavePathPack
    {
        public string saveName;
        public int saveIndex;
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
        public SS_VersionData savedAsVersion;

        public DictionaryBridge<string, SimpleSaveData> simpleSaveData = new();
        public GameSaveData(SS_VersionData _version)
        {
            savedAsVersion = _version;
        }
    }

}

