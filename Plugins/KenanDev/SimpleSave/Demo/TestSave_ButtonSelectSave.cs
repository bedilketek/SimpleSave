using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.Events;
namespace KenanDev.SimpleSave
{
    public class TestSave_ButtonSelectSave : MonoBehaviour
    {
        public GameObject ButtonObj
        {
            get
            {
                if (buttonObj == null)
                {
                    return gameObject;
                }
                return buttonObj;
            }
        }
        public string SaveName
        {
            get
            {
                return saveName;
            }
        }
        [SerializeField]
        private GameObject buttonObj;
        [SerializeField]
        private TextMeshProUGUI textMeshProUGUI;
        [SerializeField]
        private Button button;
        private string saveName;
        private Action<TestSave_ButtonSelectSave> callback;
        public void Initialize(string _saveName, Action<TestSave_ButtonSelectSave> _callback)
        {
            textMeshProUGUI.text = _saveName;
            saveName = _saveName;
            callback = _callback;
            button.onClick.AddListener(OnButtonClicked);
        }
        private void OnButtonClicked()
        {
            callback.Invoke(this);
        }
    }
}

