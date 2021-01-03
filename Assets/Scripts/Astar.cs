
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Schedulers;
using Priority_Queue;
using UnityEngine;

public class Astar
{
    public enum FailReason
    {
        NoFail,
        InvalidStartPosition,
        InvalidEndPosition,
        NoPossiblePath,
        Timeout,
        OutOfMemory
    }

    [System.Serializable]
    public class Result
    {
        public bool foundPath = false;
        public FailReason failReason = FailReason.NoFail;
        public Stack<Vector3Int> path = new Stack<Vector3Int>();
        public float executionTime = 0;
    }

    private class AstarNode : FastPriorityQueueNode
    {
        const int levelWeight = 40;
        const int estimationWeight = 10;
        readonly Vector3Int mPos;
        readonly int mLevel;
        readonly int mPriority;
        readonly AstarNode mParent;
        int remainingDistanceSquared = int.MaxValue;
        public AstarNode(Vector3Int pos, int level, int priority, AstarNode parent)
        {
            mPos = pos;
            mLevel = level + levelWeight;
            mPriority = priority;
            mParent = parent;
        }
        public AstarNode(Vector3Int pos, AstarNode parent, Vector3Int goal)
        {
            mPos = pos;
            mLevel = parent.GetLevel() + levelWeight;
            mPriority = GeneratePriority(goal);
            mParent = parent;
        }

        public int GeneratePriority(Vector3Int goal)
        {
            remainingDistanceSquared = Estimate(goal);
            return mLevel + remainingDistanceSquared * estimationWeight;
        }

        private int Estimate(Vector3Int goal)
        {
            // Squared Euclidean Distance
            Vector3Int diff = goal - mPos;
            int d = diff.x * diff.x + diff.y * diff.y + diff.z * diff.z;
            return d;
        }

        public Vector3Int GetPos() { return mPos; }
        public int GetLevel() { return mLevel; }
        public int GetPriority() { return mPriority; }
        public AstarNode GetParent() { return mParent; }
        public int GetRemainingDistanceSquared() { return remainingDistanceSquared; }
    }

    public Task<Result> CalculatePath(Vector3Int start, Vector3Int end){
        return CalculatePath(start, end, 0);

    }

    // minMargin is the number of squares you must be away from the target
    // maxMargin is the number of squared you may be away from the target
    // so if minMargin is 3 and if maxMargin is 5, 
    // your destination may then be anywhere between 3-5 squares
    public Task<Result> CalculatePath(Vector3Int start, Vector3Int end, int minMargin)
    {
        Stopwatch totalSw = Stopwatch.StartNew();
        Result result = new Result();
        minMargin = minMargin * minMargin + minMargin * minMargin+ minMargin * minMargin; // convert from tiles to squared distance
        if (!GridMap.Instance.IsPosInMap(start))
        {
            // explicitly don't care if our own block is passable, it just have to exist.
            // Will help if unit for some reason gets stuck inside a block
            result.failReason = FailReason.InvalidStartPosition;
            result.executionTime = totalSw.ElapsedMilliseconds;
            return Task.FromResult(result);
        }
        if (minMargin == 0 && !GridMap.Instance.IsPosFree(end)) // with a higher margin it's messier to check if end is valid
        {
            result.failReason = FailReason.InvalidEndPosition;
            result.executionTime = totalSw.ElapsedMilliseconds;
            return Task.FromResult(result);
        }
        if (start == end)
        {
            result.foundPath = true;
            result.executionTime = totalSw.ElapsedMilliseconds;
            return Task.FromResult(result);
        }
        const int maxEntries = 100000;
        Priority_Queue.FastPriorityQueue<AstarNode> nodeQueue = new Priority_Queue.FastPriorityQueue<AstarNode>(maxEntries);
        AstarNode currentNode = new AstarNode(start, 0, 0, null);
        currentNode.GeneratePriority(end);
        nodeQueue.Enqueue(currentNode, 0);

        Dictionary<Vector3Int, int> mapWeights = new Dictionary<Vector3Int, int>();

        int steps = 0;
        const int maxSteps = 10000;

        while (nodeQueue.Count > 0)
        {
            steps++;
            if (steps > maxSteps)
            {
                result.failReason = FailReason.Timeout;
                result.executionTime = totalSw.ElapsedMilliseconds;
                return Task.FromResult(result);
            }

            currentNode = nodeQueue.Dequeue();
            Vector3Int currentPos = currentNode.GetPos();
            if(currentNode.GetRemainingDistanceSquared() <= minMargin){
                Unravel(result, currentNode);
                result.executionTime = totalSw.ElapsedMilliseconds;
                return Task.FromResult(result);
            }
            else
            {
                foreach (var delta in DeltaPositions.DeltaPositions3D)
                {
                    Vector3Int newPos = currentPos + delta;
                    bool validStep = IsStepValid(newPos, currentPos, delta);
                    if (validStep)
                    {
                        AstarNode nextNode = new AstarNode(newPos, currentNode, end);
                        int weight;
                        bool weightExists = mapWeights.TryGetValue(newPos, out weight);
                        if (!weightExists || nextNode.GetPriority() < weight)
                        {
                            weight = nextNode.GetPriority();
                            mapWeights[newPos] = weight;
                            if (nodeQueue.Count < maxEntries)
                            {
                                nodeQueue.Enqueue(nextNode, weight);
                            }
                            else
                            {
                                result.failReason = FailReason.OutOfMemory;
                                result.executionTime = totalSw.ElapsedMilliseconds;
                                return Task.FromResult(result);
                            }
                        }
                    }
                }
            }
        }
        result.executionTime = totalSw.ElapsedMilliseconds;
        result.failReason = FailReason.NoPossiblePath;
        return Task.FromResult(result);
    }

    private void Unravel(Result result, AstarNode currentNode)
    {
        while (currentNode.GetParent() != null)
        {
            Vector3Int pos = currentNode.GetPos();
            result.path.Push(pos);
            currentNode = currentNode.GetParent();
        }
        result.foundPath = true;
    }

    public static bool IsStepValid(Vector3Int newPos, Vector3Int currentPos, Vector3Int delta)
    {
        if (!GridMap.Instance.IsPosFree(newPos)) return false;
        /**
        * If I am moving up, I want to know if my current block allows it
        *     i.e. does it allow climbing?
        * If I am moving down, I want to know if the block below me allows it
        *     i.e. is it passable?
        * If I am moving horizontal, I want to know if the block below my next pos
        * allows it i.e. Does it allow be to step on top of it. AirBlock says no,
        * rest says yes
        */

        if (delta.y != 0)
        {
            if (delta.y > 0)
            {
                Block block;
                GridMap.Instance.TryGetBlock(currentPos, out block);
                return block != null ? block.SupportsClimbing() : false;
            }
            else
            {
                return true; // dropping down is always ok, for now
            }
        }
        else
        {
            Vector3Int posBelowNext = newPos + Vector3Int.down;
            Block nextBlock;
            GridMap.Instance.TryGetBlock(posBelowNext, out nextBlock);
            if (nextBlock != null)
            {
                return nextBlock.SupportsWalkingOnTop();
            }
            else
            {
                return true; // You may walk on the bottom of the world
            }
        }
    }
}