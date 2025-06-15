using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KenanDev.SimpleSave.Demo
{
    public class KenanTestSave4 : MonoBehaviour
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
    }
}

