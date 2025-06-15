using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
namespace KenanDev.SimpleSave
{
    public class TestSave_TextDisplay : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField inputField;
        [SerializeField, KenanSave(SaveDataType.Int)]
        private int number;
        private SimpleComponentSaver simpleComponentSaver;
        private void Awake()
        {
            simpleComponentSaver = GetComponent<SimpleComponentSaver>();
            simpleComponentSaver.Subscribe_OnDataLoaded(OnDataLoaded);
            inputField.onEndEdit.AddListener( OnTextEndEdit);
        }
        private void OnTextEndEdit(string text)
        {
            int finalNum = int.Parse(text);
            number = finalNum;
        }
        private void OnDataLoaded()
        {
            inputField.text = number.ToString();
        }
    }
}

