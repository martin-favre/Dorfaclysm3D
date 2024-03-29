﻿using System;
using System.Collections;
using System.Collections.Generic;
using Items;
using Logging;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using static TMPro.TMP_Dropdown;

public class PlayerUpdateEvent
{
    private readonly PlayerComponent playerComponent;

    public PlayerUpdateEvent(PlayerComponent component)
    {
        this.playerComponent = component;
    }

    public PlayerComponent PlayerComponent => playerComponent;
}

public class PlayerComponent : MonoBehaviour, IObservable<PlayerUpdateEvent>
{
    public enum RequestState
    {
        Mining,
        Placing,
        Cancelling,
        QuickBlockRemove,
        BuildObject
    };
    LilLogger logger;
    [SerializeField] private TMP_Dropdown requestChooserDropdown = null;
    [SerializeField] private TMP_Dropdown blockChooserDropdown = null;
    [SerializeField] private GameObject blockBuildGhostObj = null;

    private RequestState requestState = RequestState.Mining;

    private Block plannedBuildBlock = new RockBlock();
    private BlockBuildGhoster plannedBuildGhost;

    private BuildingBlueprint plannedBlueprint = new BuildingBlueprint("Prefabs/BedPrefab", Vector3Int.zero, new List<Tuple<Type, int>>
    () {
        new Tuple<Type, int>(typeof(RockBlockItem), 3)
    });
    private List<IObserver<PlayerUpdateEvent>> observers = new List<IObserver<PlayerUpdateEvent>>();

    private static PlayerComponent instance;

    public static PlayerComponent Instance { get => instance; }
    public RequestState PlayerRequestState { get => requestState; }
    public BuildingBlueprint PlannedBlueprint { get => plannedBlueprint; }

    private void Awake()
    {
        logger = new LilLogger(gameObject.name);
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            logger.Log("Duplicate PlayerComponent", LogLevel.Warning);
            GameObject.Destroy(gameObject);
        }
    }

    private void Start()
    {
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
        Func<Vector3, Block>[] intToBlock = {
            (rot) => new RockBlock(rot),
            (rot) => new StairUpDownBlock(rot),
            (rot) => new GrassBlock(rot),
            (rot) => new WoodBlock(rot) };
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
        RequestState[] intToReq = { RequestState.Mining, RequestState.Placing, RequestState.Cancelling, RequestState.QuickBlockRemove, RequestState.BuildObject };
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
        NotifyObservers();
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
            else if (requestState == RequestState.BuildObject)
            {
                HandleBuildObject();
            }
        }

        if (requestState == RequestState.Placing && Input.GetKeyDown(KeyCode.R))
        {
            handleRotatePlacement();
            NotifyObservers();
        }
    }

    private void HandleBuildObject()
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
                    if (actor.gameObject.GetComponent<BuildingSite>() != null)
                    {
                        logger.Log("There was already a BuildingSite there");
                        return;
                    }
                }
                logger.Log("Placed a new BuildingSite");
                BuildingBlueprint blueprint = new BuildingBlueprint("Prefabs/BedPrefab", blockPos, new List<Tuple<Type, int>>
                () {
                    new Tuple<Type, int>(typeof(RockBlockItem), 3)
                });
                BuildingSite site = BuildingSite.InstantiateNew(blueprint);
            }
        }
        else
        {
            logger.Log("Did not hit a valid block ");
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

    private void NotifyObservers()
    {
        PlayerUpdateEvent update = new PlayerUpdateEvent(this);
        foreach(var obs in observers) {
            obs.OnNext(update);
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

    Type GetRequiredItemForBlock(Block block)
    {
        switch (block)
        {
            case RockBlock e:
                return typeof(RockBlockItem);
            case GrassBlock e:
                return typeof(GrassBlockItem);
            case WoodBlock e:
                return typeof(WoodBlockItem);
            case SnowBlock e:
                return typeof(SnowBlockItem);
            case StairUpDownBlock e:
                return typeof(RockBlockItem);
            default:
                logger.Log("Unknown blocktype " + block.ToString());
                return typeof(RockBlockItem);
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
                BlockBuildingSite site = BlockBuildingSite.InstantiateNew(blockPos, (Block)plannedBuildBlock.Clone(), GetRequiredItemForBlock(plannedBuildBlock));
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

    public IDisposable Subscribe(IObserver<PlayerUpdateEvent> observer)
    {
        return new GenericUnsubscriber<PlayerUpdateEvent>(observers, observer);
    }
}
