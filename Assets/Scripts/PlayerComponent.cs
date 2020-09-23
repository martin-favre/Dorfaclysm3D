using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerComponent : MonoBehaviour
{
    void Update()
    {
        if(Input.GetMouseButtonDown(0)){
            Vector3Int blockPos;
            bool success = BlockLaser.GetBlockPositionAtMouse(Input.mousePosition, out blockPos);
            if(success) {
                Block block; 
                bool foundBlock = GridMap.TryGetBlock(blockPos, out block);
                if(foundBlock) {
                    MiningRequest req = new MiningRequest(blockPos, block.Type);
                    MiningRequestPool.Instance.PostRequest(req);
                }
            }
        }
    }
}
