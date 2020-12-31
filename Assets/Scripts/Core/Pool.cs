using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// So whenever you want to get something from the pool, say
// Pool.singleton.Get("Coin"); and check if null, set position and set active

// To return gameObjects to the pool, simply SetActive(false);

namespace Bomber.Core
{

    [System.Serializable]
    public class PoolItem
    {
        public GameObject prefab;
        public int amount;
        public bool expandable = false;
    }


    public class Pool : MonoBehaviour
    {

        public static Pool singleton;
        public List<PoolItem> items;
        public List<GameObject> pooledItems;
        [SerializeField] Transform parent;


        void Awake()
        {
            singleton = this;
        }

        void Start()
        {
            pooledItems = new List<GameObject>();

            foreach (PoolItem item in items)
            {
                for (int i = 0; i < item.amount; i++)
                {
                    GameObject spawned = Instantiate(item.prefab);
                    spawned.transform.parent = parent;
                    spawned.SetActive(false);
                    pooledItems.Add(spawned);
                }
            }
        }

        public GameObject Get(string tag)
        {
            for (int i = 0; i < pooledItems.Count; i++)
            {
                if (!pooledItems[i].activeInHierarchy && pooledItems[i].tag == tag)
                {
                    return pooledItems[i];
                }

            }
            foreach (PoolItem item in items)
            {
                if (item.prefab.tag == tag && item.expandable)
                {
                    GameObject obj = Instantiate(item.prefab);
                    obj.SetActive(false);
                    pooledItems.Add(obj);
                    return obj;
                }
            }

            return null;
        }

    }
}

