using UnityEngine;
using System.Collections.Generic;

namespace WorldUI{
    public class Pool<T> where T : Component
    {
        readonly Queue<T> _pool = new();
        readonly T _prefab;
        readonly Transform _parent;

        public Pool(T prefab, Transform parent, int prewarm = 0)
        {
            _prefab = prefab; _parent = parent;
            for(int i = 0; i < prewarm; i++) Despawn(Object.Instantiate(_prefab,_parent));
        }

        public T Spawn()
        {
            var inst = _pool.Count > 0 ? _pool.Dequeue() : Object.Instantiate(_prefab, _parent);
            inst.gameObject.SetActive(true);
            return inst;
        }

        public void Despawn(T inst)
        {
            if (!inst) return;
            inst.gameObject.SetActive(false);
            inst.transform.SetParent(_parent, false);
            _pool.Enqueue(inst);
        }
    }
}
