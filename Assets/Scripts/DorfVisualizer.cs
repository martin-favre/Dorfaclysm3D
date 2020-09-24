﻿using System.Collections;
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

    static readonly Vector3 offset = new Vector3(.5f, 1, .5f); // So the dorfs will be in the center of the block
    void Start()
    {
        actor = GetComponent<GridActor>();
        if (actor)
        {
            RecordPos(actor.GetPos());
        }
        transform.position = realTargetPos;
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
        }
    }

    void RecordPos(Vector3Int currentPos)
    {
        gridTargetPos = currentPos;
        realTargetPos = new Vector3(gridTargetPos.x, gridTargetPos.y, gridTargetPos.z) + offset;
        originPos = transform.position;
        journeyLength = (originPos - realTargetPos).magnitude;
        timeDiff = (Time.time - timeLastPos) * 2;
        if(timeDiff == 0) timeDiff = 0.001f;
        speed = 2 / (timeDiff);
        timeLastPos = Time.time;
    }
}