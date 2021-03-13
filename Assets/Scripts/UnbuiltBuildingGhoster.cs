using System;
using System.Collections;
using System.Collections.Generic;
using Logging;
using UnityEngine;

public class UnbuiltBuildingGhoster : MonoBehaviour
{
    MeshFilter filter;
    MeshRenderer meshRenderer;
    Vector3 oldMousePosition;

    SimpleObserver<PlayerUpdateEvent> observer;

    Shader seeThroughShader;

    LilLogger logger;


    void Start()
    {
        logger = new LilLogger(typeof(UnbuiltBuildingGhoster).ToString());
        observer = new SimpleObserver<PlayerUpdateEvent>(PlayerComponent.Instance, OnPlayerSettingsUpdated);
        filter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        seeThroughShader = PrefabLoader.GetPrefab<Shader>("Shaders/SeeThroughShader");
        if(seeThroughShader == null) logger.Log("Unable to load Shaders/SeeThroughShader", LogLevel.Warning);
    }

    void Update()
    {
        if (Input.mousePosition != oldMousePosition)
        {
            UpdatePosition(Input.mousePosition);

            oldMousePosition = Input.mousePosition;
        }
    }
    void RenderNothing()
    {
        if (!meshRenderer) return;
        meshRenderer.enabled = false;
    }

    void OnPlayerSettingsUpdated(PlayerUpdateEvent update)
    {
        if (update.PlayerComponent.PlayerRequestState == PlayerComponent.RequestState.BuildObject && update.PlayerComponent.PlannedBlueprint != null)
        {
            UpdatePosition(Input.mousePosition);
            UpdateMesh(update.PlayerComponent.PlannedBlueprint);
        }
        else
        {
            this.filter.mesh = null;
            RenderNothing();
        }
    }

    private void UpdateMesh(BuildingBlueprint plannedBlueprint)
    {
        if (!this.filter) return;
        if (!this.meshRenderer) return;
        GameObject gObj = PrefabLoader.GetPrefab<GameObject>(plannedBlueprint.PrefabPath);
        if (gObj == null)
        {
            logger.Log("Unable to load prefab " + plannedBlueprint.PrefabPath, LogLevel.Warning);
            return;
        }


        MeshFilter filter = Helpers.GetComponent<MeshFilter>(gObj, logger);
        if (!filter) return;
        this.filter.mesh = filter.sharedMesh;

        MeshRenderer renderer = Helpers.GetComponent<MeshRenderer>(gObj, logger);
        if (!renderer) return;
        this.meshRenderer.material = renderer.sharedMaterial;
    }

    private void UpdatePosition(Vector3 newPos)
    {
        if (filter == null) return;

        Vector3Int blockPos;
        bool success = BlockLaser.GetBlockPositionAtMouse(newPos, out blockPos, -0.0001f);
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

        if (!(block is AirBlock))
        {
            RenderNothing();
            return;
        }
        transform.position = blockPos;
        this.meshRenderer.enabled = true;
    }
}
