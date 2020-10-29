using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerComponent : MonoBehaviour
{
    enum RequestState
    {
        Mining,
        Placing
    };
    RequestState requestState = RequestState.Mining;
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

            if (requestState == RequestState.Mining)
            {
                HandleMining();
            }
            else
            {
                HandlePlacing();
            }
        }
        if(Input.GetKeyDown(KeyCode.Tab)) {
            if(requestState == RequestState.Mining) {
                requestState = RequestState.Placing;
            } else {
                requestState = RequestState.Mining;
            }
        }
    }

    private void HandlePlacing()
    {
        Vector3Int blockPos;
        bool success = BlockLaser.GetBlockPositionAtMouse(Input.mousePosition, out blockPos, -0.001f);
        if (success)
        {
            print("Pointed at " + blockPos);
            Block block;
            bool foundBlock = GridMap.Instance.TryGetBlock(blockPos, out block);
            print("FoundBlock " + foundBlock);
            if (foundBlock && block.Type == Block.BlockType.airBlock)
            {

                print("It was airblock ");
                BlockBuildingSite site = BlockBuildingSite.InstantiateNew(blockPos);
            }
        }
    }

    private void HandleMining()
    {
        Vector3Int blockPos;
        bool success = BlockLaser.GetBlockPositionAtMouse(Input.mousePosition, out blockPos);
        if (success)
        {
            Block block;
            bool foundBlock = GridMap.Instance.TryGetBlock(blockPos, out block);
            if (foundBlock)
            {
                MiningRequest req = new MiningRequest(blockPos, block.Type);
                MiningRequestPool.Instance.PostRequest(req);
            }
        }
    }
}
