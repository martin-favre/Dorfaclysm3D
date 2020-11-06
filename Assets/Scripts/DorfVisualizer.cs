using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Moves the Dorf nicely to the position determined by the GridActor
public class DorfVisualizer : MonoBehaviour
{
    GridActor actor;
    Vector3 originPos;
    Vector3Int gridTargetPos;
    Vector3 realTargetPos;
    float timeDiff;
    float timeLastPos;
    float journeyLength = 0;

    public float speed;

    static readonly Vector3 offset = new Vector3(.5f, 0, .5f); // So the dorfs will be in the center of the block

    CameraController camController;
    MeshRenderer meshRenderer;

    void Start()
    {
        actor = GetComponent<GridActor>();
        transform.position = GridPosToRealPos(actor.GetPos());
        if (actor)
        {
            RecordPos(actor.GetPos());
        }
        camController = Camera.main.GetComponent<CameraController>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    void Update()
    {
        if (actor)
        {
            Vector3Int currentPos = actor.GetPos();
            // If the actor has moved to a new position
            if (currentPos != gridTargetPos)
            {
                // record the new target, record the timediff, record where I am
                RecordPos(currentPos);
            }
            if (journeyLength > 0 && journeyLength != float.NaN)
            {
                // Distance moved equals elapsed time times speed..
                float distCovered = (Time.time - timeLastPos) * speed;

                // Fraction of journey completed equals current distance divided by total distance.
                float fractionOfJourney = distCovered / journeyLength;
                Vector3 newPos = Vector3.Lerp(originPos, realTargetPos, fractionOfJourney);
                transform.position = newPos;
            }
            UpdateVisibility();
        }
    }

    void UpdateVisibility()
    {
        bool visible = true;
        if (camController && actor)
        {
            int myY = actor.GetPos().y;
            int maxy = camController.GetVerticalPosition();
            visible = myY < maxy;
        }
        if (meshRenderer)
        {
            meshRenderer.enabled = visible;
        }
    }

    Vector3 GridPosToRealPos(Vector3Int gridpos)
    {
        return new Vector3(gridpos.x, gridpos.y, gridpos.z) + offset;
    }

    void RecordPos(Vector3Int currentPos)
    {
        gridTargetPos = currentPos;
        realTargetPos = GridPosToRealPos(gridTargetPos);
        originPos = transform.position;
        journeyLength = (originPos - realTargetPos).magnitude;
        timeDiff = (Time.time - timeLastPos) * 2;
        if (timeDiff == 0) timeDiff = 0.001f;
        speed = 2 / (timeDiff);
        timeLastPos = Time.time;
    }
}
