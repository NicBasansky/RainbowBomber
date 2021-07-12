using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotManager : Singleton<BotManager>
{
    public List<GameObject> bots = new List<GameObject>();

    public List<GameObject> GetAIGameObjects()
    {
        return bots;
    }

    public void Register(GameObject newGo)
    {
        bots.Add(newGo);
    }

    public void Remove(GameObject go)
    {
        bots.Remove(go);
    }
}
