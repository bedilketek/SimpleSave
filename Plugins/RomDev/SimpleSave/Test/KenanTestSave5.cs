using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace RomDev.SimpleSave.Demo
{
    public class KenanTestSave5 : MonoBehaviour
    {
        public string saveFileName;
        public Zoo zoo = new();
        [Serializable, HideInInspector]
        public class Zoo
        {
            public DictionaryBridge<string, Animal> animalDict = new();
        }
        [Serializable, HideInInspector]
        public class Animal
        {
            public virtual void Speak()
            {
                Debug.Log("Speak");
            }
        }
        [Serializable, HideInInspector]
        public class Turtle : Animal
        {
            public string text;
            public override void Speak()
            {
                Debug.Log("Turtle: " + text);
            }
        }
        [Serializable, HideInInspector]
        public class Kong : Animal
        {
            public string text;
            [SerializeField]
            public NumberData<int> num;
            public override void Speak()
            {
                Debug.Log("Kong: " + text +", " + num.num);
            }
        }
        [Serializable]
        public class NumberData<T>
        {
            [SerializeReference]
            public T num;
        }
        public void MakeAnimals()
        {
            zoo.animalDict.Clear();
            Turtle turtle = new();
            turtle.text = "Iam Turtle";
            zoo.animalDict.AddData("turtle", turtle);
            //
            Kong kong = new();
            kong.text = "Iam Good";
            NumberData<int> numberData = new();
            numberData.num = 89;
            kong.num = numberData;
            zoo.animalDict.AddData("kong", kong);
        }
        public void SaveData()
        {
            string jsonString = JsonUtility.ToJson(zoo);
            string filePath = Application.persistentDataPath + Path.DirectorySeparatorChar + saveFileName + ".json";
            File.WriteAllText(filePath, jsonString);
        }
        public void LoadData()
        {
            string filePath = Application.persistentDataPath + Path.DirectorySeparatorChar + saveFileName + ".json";
            string jsonString = File.ReadAllText(filePath);
            zoo = JsonUtility.FromJson<Zoo>(jsonString);
        }
        public void PrintAnimalsContent()
        {
            foreach (Animal _animal in zoo.animalDict.Values)
            {
                _animal.Speak();
            }
        }
    }
}

