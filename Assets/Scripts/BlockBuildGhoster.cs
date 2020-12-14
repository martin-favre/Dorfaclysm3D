using System;
using System.Collections;
using System.Collections.Generic;
using Logging;
using UnityEngine;

public class BlockBuildGhoster : MonoBehaviour
{
    Block plannedBlock = new AirBlock();

    Vector3 oldMousePos;

    BlockVisualizer visualizer;
    LilLogger logger;
    void Start()
    {
        logger = new LilLogger(gameObject.name);
        visualizer = GetComponent<BlockVisualizer>();
        if (visualizer)
        {
            visualizer.RenderBlock(plannedBlock);
        }
        else
        {
            logger.Log("No BlockVisualizer found on BlockBuildGhoster", LogLevel.Warning);
        }
        oldMousePos = Input.mousePosition;
    }

    void Update()
    {
        if (!visualizer) return;
        if (Input.mousePosition != oldMousePos)
        {
            oldMousePos = Input.mousePosition;
            UpdateBlock(oldMousePos);
        }
    }

    void RenderNothing()
    {
        if(!visualizer) return;
        visualizer.RenderBlock(new AirBlock());
    }

    public void setBlock(Block block){
        plannedBlock = block;
        UpdateBlock(oldMousePos);
    }

    void UpdateBlock(Vector3 newMousePos)
    {
        Vector3Int blockPos;
        bool success = BlockLaser.GetBlockPositionAtMouse(newMousePos, out blockPos, -0.0001f);
        if (!success)
        {
            RenderNothing();
            return;
        }

        Block block;
        success = GridMap.Instance.TryGetBlock(blockPos, out block);
        if (!success)
        {
            RenderNothing();
            return;
        }

        if(!(block is AirBlock)) {
            RenderNothing();
            return;
        }

        transform.position = blockPos;
        visualizer.RenderBlock(plannedBlock);
    }


}
