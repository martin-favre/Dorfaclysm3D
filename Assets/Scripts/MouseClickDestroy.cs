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

        Vector3Int blockPosition;
        bool success = BlockLaser.GetBlockPositionAtMouse(origin, out blockPosition);
        if (success)
        {
            Block block;
            GridMap.Instance.TryGetBlock(blockPosition, out block);
            print("Blockpos at " + blockPosition);
            if (block != null)
            {
                print("Destroyed a " + block.GetName());
                Block newBlock = new AirBlock();
                GridMap.Instance.SetBlock(blockPosition, newBlock);
            }
            else
            {
                Debug.LogError("Hit block, but no block was found at that position");
            }
        }
    }
}
