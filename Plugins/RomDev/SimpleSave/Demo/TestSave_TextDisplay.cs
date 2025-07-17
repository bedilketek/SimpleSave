using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
namespace RomDev.SimpleSave.Demo
{
    public class TestSave_TextDisplay : MonoBehaviour
    {
        [SerializeField]
        private TMP_InputField inputField;
        [SimpleSave(SaveDataType.PrimitiveValue)]
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

