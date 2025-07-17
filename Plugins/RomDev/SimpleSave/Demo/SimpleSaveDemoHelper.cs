using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RomDev.SimpleSave.Demo
{
    public static class SimpleSaveDemoHelper
    {
        public static string DemoSimpleSaveAssemblyName
        {
            get
            {
                return DemoSimpleSaveProjectName + ", " + SimpleSaveHelper.HalfEndAssemblyName;
            }
        }
        public const string DemoSimpleSaveProjectName = "KenanDev.SimpleSave.Demo";
    }
}

