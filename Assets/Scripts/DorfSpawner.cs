using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DorfSpawner : MonoBehaviour
{
    void Start()
    {
        for(int i = 0; i < 20; i++){
        DorfController.InstantiateDorf(new Vector3Int(1,1,5));

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
