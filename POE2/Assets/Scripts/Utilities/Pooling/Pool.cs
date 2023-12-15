using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Pool<T> {
    public List<PooledObject> pool;
    public int currentIndex;

    public void CreatePoolFromGameObject(GameObject basicObject, int size, bool dontDestroyOnLoad) {

        this.pool = new List<PooledObject>();

        for (int i = 0; i < size; i++) {
            PooledObject pooledObject = new PooledObject {
                gameObject = UnityEngine.Object.Instantiate(basicObject) as GameObject
            };

            if (pooledObject.gameObject) {
                if (dontDestroyOnLoad) UnityEngine.Object.DontDestroyOnLoad(pooledObject.gameObject);

                T gameObjectT = pooledObject.gameObject.GetComponent<T>();
                if (gameObjectT != null) pooledObject.gameObjectT = gameObjectT;
            }
            pool.Add(pooledObject);
        }
    }

    public T GetNext() {
        T obj = default;
        if (pool[currentIndex] != null) obj = pool[currentIndex].gameObjectT;
        if (++currentIndex >= pool.Count) currentIndex = 0;
        return obj;
    }

    [Serializable]
    public class PooledObject {
        public GameObject gameObject;
        public T gameObjectT;
    }
}