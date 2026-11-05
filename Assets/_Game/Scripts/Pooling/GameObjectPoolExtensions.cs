using UnityEngine;

namespace Pooling
{
    public static class GameObjectPoolExtensions
    {
        public static GameObject Get(this GameObject prefab)
        {
            return Pooler.Get(prefab);
        }

        public static GameObject Get(this GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return Pooler.Get(prefab, position, rotation);
        }

        public static void Release(this GameObject instance)
        {
            Pooler.Release(instance);
        }

        public static void Release(this GameObject instance, float delay)
        {
            Pooler.Release(instance, delay);
        }
    }
}
