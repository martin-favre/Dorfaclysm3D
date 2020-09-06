using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseClickDestroy : MonoBehaviour
{
    bool firingRequested = false;
    Vector3 firingOrigin;
    private void Awake()
    {
    }

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            firingOrigin = Input.mousePosition;
            firingRequested = true;
        }
    }

    void FixedUpdate()
    {
        if (firingRequested)
        {
            ShootDeathLaser(firingOrigin);
            firingRequested = false;
        }
    }

    static private void ShootDeathLaser(Vector3 origin)
    {
        Ray ray = Camera.main.ScreenPointToRay(origin);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Vector3 dir = ray.direction;
            Vector3 hitPoint = hit.point;
            float margin = 0.0001f;

            // Hits register just outside the collider
            // So we need to inch forward a bit to ensure that the position 
            // is inside the block.
            hitPoint += ray.direction * margin;

            // Truncate the positions to get the indeces.
            int x = Mathf.FloorToInt(hitPoint.x);
            int y = Mathf.FloorToInt(hitPoint.y + 1f); //y starts at -1 for some reason
            int z = Mathf.FloorToInt(hitPoint.z);
            Vector3Int blockPosition = new Vector3Int(x, y, z);
            Block block;
            GridMap.TryGetBlock(blockPosition, out block);
            print("Blockpos at " + blockPosition);
            if (block != null)
            {
                print("Destroyed a " + block.GetName());
                Block newBlock = new AirBlock();
                GridMap.SetBlock(blockPosition, newBlock);
            } else {
                Debug.LogError("Hit block, but no block was found at that position");
            }
        }

    }
}
