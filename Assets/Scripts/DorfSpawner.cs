using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DorfSpawner : MonoBehaviour
{

    public bool correctDorfPositions = true;
    bool dorfsSpawned = false;

    List<GameObject> spawnedDorfs = new List<GameObject>();

    void Start()
    {
        
    }


    // Update is called once per frame
    void Update()
    {
        if (!dorfsSpawned)
        {
            dorfsSpawned = true;
            for (int i = 0; i < 10; i++)
            {
                Vector3Int middleOfMap = new Vector3Int(GridMap.Instance.GetSize().x / 2, 0, GridMap.Instance.GetSize().z / 2);
                spawnedDorfs.Add(DorfController.InstantiateDorf(middleOfMap).gameObject);
            }
        }

        if (GridMap.Instance.IsGenerationDone() && correctDorfPositions)
        {
            correctDorfPositions = false;
            foreach(GameObject g in spawnedDorfs){
                GridActor actor = g.GetComponent<GridActor>();
                Vector3Int pos = actor.GetPos();
                Vector3Int newPos = Vector3Int.zero;
                for (int y = 0; y < GridMap.Instance.GetSize().y; y++)
                {
                    newPos = new Vector3Int(pos.x, y, pos.z);
                    if (GridMap.Instance.GetBlock(newPos).supportsWalkingThrough())
                    {
                        break;
                    }
                }
                
                actor.Move(newPos);

            }
        }
    }
}
