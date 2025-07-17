using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RomDev.SimpleSave.Demo
{
    public class SSDemoManual_SaveLoad : MonoBehaviour
    {
        public string saveName;
        public void SaveGame()
        {
            SimpleSave.SaveGameByName(saveName);
        }
        public void LoadGame()
        {
            SimpleSave.LoadGameByName(saveName);
        }
        public void DeleteSave()
        {
            SimpleSave.DeleteSaveDataByName(saveName);
        }
    }
}

