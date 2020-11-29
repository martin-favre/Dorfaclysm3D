using System;
using System.Collections;
using System.Collections.Generic;
using Logging;
using UnityEngine;

public class BlockBuildGhoster : MonoBehaviour
{
    Block plannedBlock = new RockBlock();

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
        if(Input.GetKeyDown(KeyCode.R)){
            RotateBlock();
        }
    }

    private void RotateBlock()
    {
        plannedBlock.Rotation += new Vector3(0, 90, 0);
        UpdateBlock(oldMousePos);
    }

    void RenderNothing()
    {
        visualizer.RenderBlock(new AirBlock());
    }

    public void setBlockType(Block.BlockType type){
        switch(type){
            case Block.BlockType.rockBlock:
            plannedBlock = new RockBlock(plannedBlock.Rotation);
            break;
            case Block.BlockType.stairUpDownBlock:
            plannedBlock = new StairUpDownBlock(plannedBlock.Rotation);
            break;
            default:
            plannedBlock = new AirBlock(plannedBlock.Rotation);
            break;
        }
        UpdateBlock(oldMousePos);
    }

    public Block GetBlock(){
        return plannedBlock;
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

        if(block.Type != Block.BlockType.airBlock) {
            RenderNothing();
            return;
        }

        transform.position = blockPos;
        visualizer.RenderBlock(plannedBlock);
    }


}
