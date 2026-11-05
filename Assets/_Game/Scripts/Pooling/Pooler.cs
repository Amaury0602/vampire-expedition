using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pooling
{
    /// <summary>
    /// Point d'entrée statique du système de pooling. Crée paresseusement un root
    /// DontDestroyOnLoad au premier usage ; un <see cref="Pool"/> par prefab.
    /// </summary>
    public static class Pooler
    {
        private static Dictionary<GameObject, Pool> _pools;
        private static Transform _root;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _pools = new Dictionary<GameObject, Pool>();
            _root = null;
        }

        public static void SetPool(GameObject prefab, int count)
        {
            GetOrCreatePool(prefab).Prewarm(count);
        }

        public static void SetPool<T>(T prefab, int count) where T : Component
        {
            SetPool(prefab.gameObject, count);
        }

        public static GameObject Get(GameObject prefab)
        {
            return GetOrCreatePool(prefab).Get();
        }

        public static GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return GetOrCreatePool(prefab).Get(position, rotation);
        }

        public static T Get<T>(T prefab) where T : Component
        {
            return Get(prefab.gameObject).GetComponent<T>();
        }

        public static T Get<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            return Get(prefab.gameObject, position, rotation).GetComponent<T>();
        }

        public static void Release(GameObject instance)
        {
            if (!instance.TryGetComponent(out PooledObject pooled))
            {
                Debug.LogWarning($"[Pooler] Release('{instance.name}') ignoré : pas de PooledObject sur cette instance.");
                return;
            }

            pooled.CancelDelayedRelease();
            pooled.OwnerPool.Release(pooled);
        }

        public static void Release(GameObject instance, float delay)
        {
            if (!instance.TryGetComponent(out PooledObject pooled))
            {
                Debug.LogWarning($"[Pooler] Release('{instance.name}') ignoré : pas de PooledObject sur cette instance.");
                return;
            }

            pooled.StartDelayedRelease(delay);
        }

        public static void Release(Component instance)
        {
            Release(instance.gameObject);
        }

        public static void Release(Component instance, float delay)
        {
            Release(instance.gameObject, delay);
        }

        public static void Clear(GameObject prefab)
        {
            if (!_pools.TryGetValue(prefab, out Pool pool))
            {
                Debug.LogWarning($"[Pooler] Clear('{prefab.name}') ignoré : aucun pool pour ce prefab.");
                return;
            }

            pool.Clear();
            _pools.Remove(prefab);
        }

        public static void Clear<T>(T prefab) where T : Component
        {
            Clear(prefab.gameObject);
        }

        private static Pool GetOrCreatePool(GameObject prefab)
        {
            if (_pools.TryGetValue(prefab, out Pool pool))
                return pool;

            if (_root == null)
            {
                GameObject rootObject = new("Pooler");
                UnityEngine.Object.DontDestroyOnLoad(rootObject);
                _root = rootObject.transform;
            }

            pool = new Pool(prefab, _root);
            _pools.Add(prefab, pool);
            return pool;
        }
    }
}
