using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RomDev.SimpleSave
{
    public enum SaveDataType
    {
        // Primitives: bool, byte, sbyte, short, ushort, int, uint, long, ulong, float, double, decimal, dan char
        // Built in : String
        PrimitiveValue = 0,
        Container = 1,
        AssetReference = 2,
        ClassWithPrimitives = 3
    }
    public enum CollectionType : byte
    {
        None = 0,
        Array = 1,
        List = 2
    }
}

