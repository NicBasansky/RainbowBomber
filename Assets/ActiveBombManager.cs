using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveBombManager : Singleton<ActiveBombManager>
{
    public List<GameObject> bombs = new List<GameObject>();

    public List<GameObject> GetAllActiveBombs()
    {
        return bombs;
    }

    public void Register(GameObject newGo)
    {
        bombs.Add(newGo);
    }

    public void Unregister(GameObject go)
    {
        foreach(GameObject b in bombs)
        {
            if ( ReferenceEquals(b, go) )
            {
                bombs.Remove(b);
                break;
            }
        }
    }


}
