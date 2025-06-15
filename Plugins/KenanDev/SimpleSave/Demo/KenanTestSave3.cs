using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KenanDev.SimpleSave.Demo
{
    public class KenanTestSave3 : MonoBehaviour
    {
        [SerializeField]
        private GameObject uiSaveCanvasButtonModel;
        [SerializeField]
        private Transform buttonsParent;
        [SerializeField]
        private TextMeshProUGUI focusedSaveNameTMP;
        [SerializeField]
        private TMP_InputField inputField;
        private List<TestSave_ButtonSelectSave> uiSaveButtons = new();
        private TestSave_ButtonSelectSave currentFocusedButton;
        private void Awake()
        {
            GetAllSaves();
        }
        public void LoadGame()
        {
            if (currentFocusedButton != null)
            {
                SimpleSave.LoadGameByName(currentFocusedButton.SaveName);
            }
            else
            {
                Debug.LogWarning("Select one of save file first! ");
            }
        }
        public void SaveGame()
        {
            if (currentFocusedButton != null)
            {
                SimpleSave.SaveGameByName(currentFocusedButton.SaveName);
            }
            else
            {
                Debug.LogWarning("Select one of save file first! ");
            }
        }
        public void DeleteSaveFile()
        {
            if (currentFocusedButton != null)
            {
                SimpleSave.DeleteSaveDataByName(currentFocusedButton.SaveName);
                uiSaveButtons.Remove(currentFocusedButton);
                Destroy(currentFocusedButton.ButtonObj);
                currentFocusedButton = null;
                focusedSaveNameTMP.text = "No selected data";
            }
            else
            {
                Debug.LogWarning("Select one of save file first! ");
            }
        }
        public void CreateNewSaveGame()
        {
            string saveName = inputField.text;
            if (!SimpleSave.IsSaveByNameExisted(saveName))
            {
                TestSave_ButtonSelectSave buttonSave = CreateSaveButton(saveName);
                // Canvas.ForceUpdateCanvases();
                // RectTransform rectTransContainer = (RectTransform)buttonsParent;
                RectTransform buttonRect = (RectTransform)buttonSave.ButtonObj.transform;
                // LayoutRebuilder.ForceRebuildLayoutImmediate(rectTransContainer);
                buttonRect.localScale = new Vector3(1, 1, 1);
                LayoutRebuilder.ForceRebuildLayoutImmediate(buttonRect);
            }
            SimpleSave.SaveGameByName(saveName);
        }
        private void GetAllSaves()
        {
            string[] saveNames = SimpleSave.GetGameSaveNames();
            foreach (string saveName in saveNames)
            {
                CreateSaveButton(saveName);
            }
        }
        private TestSave_ButtonSelectSave CreateSaveButton(string saveName)
        {
            GameObject uiButtonObj = Instantiate(uiSaveCanvasButtonModel);
            uiButtonObj.transform.SetParent(buttonsParent);
            uiButtonObj.SetActive(true);
            TestSave_ButtonSelectSave uiButton = uiButtonObj.GetComponent<TestSave_ButtonSelectSave>();
            uiSaveButtons.Add(uiButton);
            uiButton.Initialize(saveName, OnButtonClicked);
            return uiButton;
        }
        private void OnButtonClicked(TestSave_ButtonSelectSave saveButton)
        {
            currentFocusedButton = saveButton;
            focusedSaveNameTMP.text = saveButton.SaveName;
        }
    }
}

