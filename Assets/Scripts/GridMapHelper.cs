using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GridMapHelper
{
    public static bool GetClosestPassablePosition(Vector3Int origin, int maxSearchDepth, out Vector3Int result)
    {
        SortedSet<Vector3Int> failedPositions = new SortedSet<Vector3Int>();
        Stack<Tuple<Vector3Int, int>> testPositions = new Stack<Tuple<Vector3Int, int>>(); // stack of positions and their depth
        testPositions.Push(new Tuple<Vector3Int, int>(origin, 0));

        while (testPositions.Count > 0)
        {
            Tuple<Vector3Int, int> current = testPositions.Pop();
            Vector3Int currentPos = current.Item1;
            int currentDepth = current.Item2;
            if (GridMap.IsPosFree(currentPos))
            {
                result = currentPos;
                return true;
            }
            else
            {
                failedPositions.Add(currentPos);
                if (currentDepth < maxSearchDepth)
                {
                    foreach (Vector3Int delta in DeltaPositions.DeltaPositions3D)
                    {
                        Vector3Int nextPos = currentPos + delta;
                        if (!failedPositions.Contains(nextPos))
                        {
                            testPositions.Push(new Tuple<Vector3Int, int>(nextPos, currentDepth + 1));
                        }
                    }
                }
            }

        }
        result = Vector3Int.zero;
        return false;
    }
}
