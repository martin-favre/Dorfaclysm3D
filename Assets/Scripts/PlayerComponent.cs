using System;
using System.Collections;
using System.Collections.Generic;
using Logging;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using static TMPro.TMP_Dropdown;

public class PlayerComponent : MonoBehaviour
{
    public enum RequestState
    {
        Mining,
        Placing,
        Cancelling,
        QuickBlockRemove
    };
    RequestState requestState = RequestState.Mining;
    LilLogger logger;
    public TMP_Dropdown dropdown;


    private void Start()
    {
        logger = new LilLogger(gameObject.name);
        logger.Log("Playercomponent started");
        SetUpDropdown();
    }

    void SetUpDropdown()
    {
        if (dropdown)
        {

            DropdownEvent unityEvent = new DropdownEvent();
            unityEvent.AddListener(OnDropdownChanged);
            dropdown.onValueChanged = unityEvent;
            OnDropdownChanged(dropdown.value);
        }
        else
        {
            logger.Log("Missing dropdown", LogLevel.Warning);
        }

    }

    void OnDropdownChanged(int index)
    {
        RequestState[] intToReq = { RequestState.Mining, RequestState.Placing, RequestState.Cancelling, RequestState.QuickBlockRemove };
        if (index < intToReq.Length)
        {
            SetRequestState(intToReq[index]);
        }
        else
        {
            logger.Log("Dropdown index out of range", LogLevel.Error);
        }
    }

    private void OnDestroy()
    {
        logger.Log("Playercomponent started");
    }

    public void SetRequestState(RequestState state)
    {
        logger.Log("Got a new requeststate " + state);
        requestState = state;
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {

            if (requestState == RequestState.Mining)
            {
                HandleMining();
            }
            else if (requestState == RequestState.Placing)
            {
                HandlePlacing();
            }
            else if (requestState == RequestState.QuickBlockRemove)
            {
                ShootDeathLaser();
            }
        }
    }
    static private void ShootDeathLaser()
    {
        Vector3 origin = Input.mousePosition;
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

    private void HandlePlacing()
    {
        Vector3Int blockPos;
        bool success = BlockLaser.GetBlockPositionAtMouse(Input.mousePosition, out blockPos, -0.001f);
        if (success)
        {
            logger.Log("Pointed at " + blockPos);
            Block block;
            bool foundBlock = GridMap.Instance.TryGetBlock(blockPos, out block);
            logger.Log("FoundBlock " + foundBlock);
            if (foundBlock && block.Type == Block.BlockType.airBlock)
            {
                logger.Log("It was airblock ");

                GridActor[] actors = GridActorMap.GetGridActors(blockPos);
                foreach (GridActor actor in actors)
                {
                    if (actor.gameObject.GetComponent<BlockBuildingSite>() != null)
                    {
                        logger.Log("There was already a blockbuildingsite there");
                        return;
                    }
                }
                logger.Log("Placed a new blockbuildingsite");
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
