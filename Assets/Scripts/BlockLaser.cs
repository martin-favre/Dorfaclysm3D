using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BlockLaser
{
    static public bool GetBlockPositionAtMouse(Vector3 mousePosition, out Vector3Int blockPosition) {
        return GetBlockPositionAtMouse(mousePosition, out blockPosition, 0.0001f);
    }
    static public bool GetBlockPositionAtMouse(Vector3 mousePosition, out Vector3Int blockPosition, float margin)
    {
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            Vector3 dir = ray.direction;
            Vector3 hitPoint = hit.point;

            // Hits register just outside the collider
            // So we need to inch forward a bit to ensure that the position 
            // is inside the block.
            hitPoint += ray.direction * margin;

            // Truncate the positions to get the indeces.
            int x = Mathf.FloorToInt(hitPoint.x);
            int y = Mathf.FloorToInt(hitPoint.y + 1f); //y starts at -1 for some reason
            int z = Mathf.FloorToInt(hitPoint.z);
            blockPosition = new Vector3Int(x, y, z);
            return true;
        }
        blockPosition = Vector3Int.zero;
        return false;
    }
}
