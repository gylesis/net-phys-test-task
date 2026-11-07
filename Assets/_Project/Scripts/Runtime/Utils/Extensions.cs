using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace Scripts.Runtime.Utils
{
    public static class Extensions
    {
        public static T GetRandomItem<T>(this IList<T> list)
        {
            return list[Random.Range(0, list.Count)];
        }
    }
}