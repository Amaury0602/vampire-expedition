using UnityEngine;

namespace Pooling
{
    public static class ComponentPoolExtensions
    {
        public static void SetPool<T>(this T prefab, int count) where T : Component
        {
            Pooler.SetPool(prefab, count);
        }

        public static T Get<T>(this T prefab) where T : Component
        {
            return Pooler.Get(prefab);
        }

        public static T Get<T>(this T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            return Pooler.Get(prefab, position, rotation);
        }

        public static void Release(this Component instance)
        {
            Pooler.Release(instance);
        }

        public static void Release(this Component instance, float delay)
        {
            Pooler.Release(instance, delay);
        }

        public static void Clear<T>(this T prefab) where T : Component
        {
            Pooler.Clear(prefab);
        }
    }
}
