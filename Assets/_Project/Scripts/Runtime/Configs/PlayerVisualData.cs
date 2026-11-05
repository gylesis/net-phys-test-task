using System;
using UnityEngine;

namespace Scripts.Runtime.Configs
{
    [Serializable]
    public class PlayerVisualData
    {
        public SkinType SkinType;
        public Color SkinColor;
    }
    
    public enum SkinType : byte
    {
        Default,
        Red,
        Blue,
        Yellow
    }
}