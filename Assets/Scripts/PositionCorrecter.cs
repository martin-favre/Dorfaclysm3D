using System.Collections;
using System.Collections.Generic;
using Logging;
using UnityEngine;

public class PositionCorrecter : MonoBehaviour
{
    SimpleObserver<Vector3Int> posObserver;
    LilLogger logger = new LilLogger("PositionCorrecter");
    void Start()
    {
        GridActor actor = Helpers.GetComponent<GridActor>(gameObject, logger);
        if (actor)
        {
            posObserver = new SimpleObserver<Vector3Int>(actor, OnNewPos);
            OnNewPos(actor.Position);
        }
        
    }

    void OnNewPos(Vector3Int newPos) {
        transform.position = newPos;
    }
}
