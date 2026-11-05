using System.Collections.Generic;
using UnityEngine;

namespace Pooling
{
    /// <summary>
    /// Pool d'instances pour un prefab donné. Classe C# simple : ne connaît que ce prefab,
    /// c'est <see cref="Pooler"/> qui expose l'API publique et route par prefab.
    /// </summary>
    public class Pool
    {
        private readonly GameObject _prefab;
        private readonly Transform _container;
        private readonly Stack<PooledObject> _inactive = new();
        private readonly HashSet<PooledObject> _active = new();

        public Pool(GameObject prefab, Transform parent)
        {
            _prefab = prefab;
            _container = new GameObject($"Pool_{prefab.name}").transform;
            _container.SetParent(parent);
        }

        public void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
                _inactive.Push(CreateInstance());
        }

        public GameObject Get(Vector3? position = null, Quaternion? rotation = null)
        {
            PooledObject pooled = _inactive.Count > 0 ? _inactive.Pop() : CreateInstance();

            Transform t = pooled.transform;
            t.SetParent(null);
            t.SetPositionAndRotation(
                position ?? t.position,
                rotation ?? t.rotation);

            pooled.gameObject.SetActive(true);
            _active.Add(pooled);

            IPoolable[] poolables = pooled.GetComponentsInChildren<IPoolable>(true);
            for (int i = 0; i < poolables.Length; i++)
                poolables[i].OnGet();

            return pooled.gameObject;
        }

        public void Release(PooledObject pooled)
        {
            if (!_active.Remove(pooled))
            {
                Debug.LogWarning($"[Pool] Release('{pooled.name}') ignoré : déjà relâché ou étranger à ce pool.");
                return;
            }

            IPoolable[] poolables = pooled.GetComponentsInChildren<IPoolable>(true);
            for (int i = 0; i < poolables.Length; i++)
                poolables[i].OnRelease();

            pooled.gameObject.SetActive(false);
            pooled.transform.SetParent(_container);
            _inactive.Push(pooled);
        }

        public void Clear()
        {
            foreach (PooledObject pooled in _active)
                Object.Destroy(pooled.gameObject);

            foreach (PooledObject pooled in _inactive)
                Object.Destroy(pooled.gameObject);

            _active.Clear();
            _inactive.Clear();

            Object.Destroy(_container.gameObject);
        }

        private PooledObject CreateInstance()
        {
            GameObject instance = Object.Instantiate(_prefab, _container);
            instance.SetActive(false);

            if (!instance.TryGetComponent(out PooledObject pooled))
                pooled = instance.AddComponent<PooledObject>();

            pooled.Prefab = _prefab;
            pooled.OwnerPool = this;

            return pooled;
        }
    }
}
