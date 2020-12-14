using System;
using System.Collections;
using System.Collections.Generic;
using Logging;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
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
    public TMP_Dropdown requestChooserDropdown;
    public TMP_Dropdown blockChooserDropdown;

    public GameObject blockBuildGhostObj;

    private Block plannedBuildBlock = new RockBlock();
    private BlockBuildGhoster plannedBuildGhost;


    private void Start()
    {
        logger = new LilLogger(gameObject.name);
        logger.Log("Playercomponent started");
        SetUpDropdown();
        if (blockBuildGhostObj)
        {
            blockBuildGhostObj.SetActive(false);
            plannedBuildGhost = blockBuildGhostObj.GetComponent<BlockBuildGhoster>();

            if (plannedBuildGhost)
            {
                plannedBuildGhost.setBlock(plannedBuildBlock);
            }
            else
            {
                logger.Log("Missing BlockBuildGhoster", LogLevel.Warning);
            }
        }
        else
        {
            logger.Log("Missing blockBuildGhost", LogLevel.Warning);
        }

    }

    void SetUpDropdown()
    {
        if (requestChooserDropdown)
        {
            DropdownEvent unityEvent = new DropdownEvent();
            unityEvent.AddListener(OnRequestDropdownChanged);
            requestChooserDropdown.onValueChanged = unityEvent;
            OnRequestDropdownChanged(requestChooserDropdown.value);
        }
        else
        {
            logger.Log("Missing request dropdown", LogLevel.Warning);
        }

        if (blockChooserDropdown)
        {
            DropdownEvent unityEvent = new DropdownEvent();
            unityEvent.AddListener(OnBlockDropdownChanged);
            blockChooserDropdown.onValueChanged = unityEvent;
            OnRequestDropdownChanged(blockChooserDropdown.value);
        }
        else
        {
            logger.Log("Missing block dropdown", LogLevel.Warning);
        }

    }

    private void OnBlockDropdownChanged(int index)
    {
        Func<Vector3, Block>[] intToBlock = { (rot) => new RockBlock(rot) , (rot) => new StairUpDownBlock(rot) };
        if (index < intToBlock.Length)
        {
            SetBlockToBuild(intToBlock[index](plannedBuildBlock.Rotation));
        }
        else
        {
            logger.Log("Dropdown index out of range", LogLevel.Error);
        }
    }

    private void SetBlockToBuild(Block block)
    {
        plannedBuildBlock = block;

        if (plannedBuildGhost)
        {
            plannedBuildGhost.setBlock(plannedBuildBlock);
        }
    }

    void OnRequestDropdownChanged(int index)
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
        OnRequestStateChanged(state);
    }

    void OnRequestStateChanged(RequestState newState)
    {
        if (blockBuildGhostObj != null) blockBuildGhostObj.SetActive(newState == RequestState.Placing);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
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
            else if (requestState == RequestState.Cancelling)
            {
                HandleCancelling();
            }
        }

        if (requestState == RequestState.Placing && Input.GetKeyDown(KeyCode.R))
        {
            handleRotatePlacement();
        }
    }

    private bool GetBlockAtMouse(Vector3 origin, out Vector3Int position, out Block block)
    {

        bool success = BlockLaser.GetBlockPositionAtMouse(Input.mousePosition, out position);
        if (success)
        {
            bool foundBlock = GridMap.Instance.TryGetBlock(position, out block);
            success = foundBlock;
        }
        else
        {
            block = null;
        }
        return success;
    }

    private void HandleCancelling()
    {

        Vector3Int blockPos;
        Block block;
        bool success = GetBlockAtMouse(Input.mousePosition, out blockPos, out block);
        if (success && !(block is AirBlock))
        {
            MiningRequestPool.Instance.CancelRequest(blockPos);
        }
        else
        {
            GameObject gObj;
            success = BlockLaser.GetGameobjectAtMouse(Input.mousePosition, out gObj);
            if (success)
            {
                BlockBuildingSite site = gObj.GetComponent<BlockBuildingSite>();
                if (site)
                {
                    GameObject.Destroy(gObj);
                }
            }
        }
    }

    private void handleRotatePlacement()
    {
        plannedBuildBlock.Rotation += new Vector3(0, 90, 0);
        if (plannedBuildGhost)
        {
            plannedBuildGhost.setBlock(plannedBuildBlock);
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
            if (foundBlock && block is AirBlock)
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
                BlockBuildingSite site = BlockBuildingSite.InstantiateNew(blockPos, (Block)plannedBuildBlock.Clone());
            }
        }
        else
        {
            logger.Log("Did not hit a valid block ");
        }
    }

    private void HandleMining()
    {
        logger.Log("Trying to mine");
        Vector3Int blockPos;
        Block block;
        bool success = GetBlockAtMouse(Input.mousePosition, out blockPos, out block);
        if (success)
        {
            logger.Log("Mined " + blockPos);
            MiningRequest req = new MiningRequest(blockPos, block.GetType());
            MiningRequestPool.Instance.PostRequest(req);
        }
    }
}
