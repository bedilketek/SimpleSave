
#if UNITY_EDITOR
using UnityEditor;
namespace KenanDev.SimpleSave
{
    [CustomEditor(typeof(SimpleSaveService))]
    public class SimpleSaveServiceEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SimpleSaveService simpleSaveService = (SimpleSaveService)target;
            simpleSaveService.ShowGUI();
        }
    }
}
#endif
