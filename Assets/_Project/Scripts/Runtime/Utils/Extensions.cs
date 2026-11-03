using System;
using System.Collections.Generic;
using System.Linq;
using Steamworks;
using Random = UnityEngine.Random;

namespace Scripts.Runtime.Utils
{
    public static class Extensions
    {
        public static T GetRandomItem<T>(this IList<T> list)
        {
            return list[Random.Range(0, list.Count)];
        }

        public static T GetRandomItem<T>(this IEnumerable<T> enumerable)
        {
            var list = enumerable.ToList();
            return GetRandomItem(list);
        }

        /// <summary>
        /// Gets randomly N elements
        /// </summary>
        public static IEnumerable<T> GetRandomItemsCount<T>(this IList<T> list, int count)
        {
            for (int i = 0; i < count; i++)
            {
                T randomItem = list.GetRandomItem();
                yield return randomItem;
            }
        }

        /// <summary>
        /// Gets randomly N elements
        /// </summary>
        public static IEnumerable<T> GetRandomItemsCount<T>(this IEnumerable<T> enumerable, int count)
        {
            var list = enumerable.ToList();
            return GetRandomItemsCount(list, count);
        }
        
        public static T RandomValue<T>(bool exceptFirst = false) where T : Enum
        {
            var values = Enum.GetValues(typeof(T));

            int length = values.Length;

            return (T)values.GetValue(Random.Range(exceptFirst ? 1 : 0, length));
        }

#if !DISABLESTEAMWORKS
        public static CSteamID ToSteamID(this ulong id)
        {
            return new CSteamID(id);
        }
#endif

    }
}