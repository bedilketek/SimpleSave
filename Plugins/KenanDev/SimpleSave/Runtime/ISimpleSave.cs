using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KenanDev.SimpleSave
{
    public interface ISimpleSave
    {
        public void OnSaveData();
        public void OnLoadData();
    }
}

