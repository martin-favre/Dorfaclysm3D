using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AStarTester : MonoBehaviour
{
    Vector3Int mTargetBlock = Vector3Int.zero;
    Task<Astar.Result> mAstarTask = null;
    public GameObject mPathShowerPrefab;
    public GameObject mTargetShowerPrefab;
    List<GameObject> mPathShowerObjs;
    GameObject startBlock;

    GameObject endBlock;



    void Start()
    {
        mPathShowerObjs = new List<GameObject>();
        for (int i = 0; i < 100; i++)
        {
            GameObject g = (GameObject)Instantiate(mPathShowerPrefab, transform);
            g.SetActive(false);
            mPathShowerObjs.Add(g);
        }

        startBlock = (GameObject)Instantiate(mTargetShowerPrefab, transform);
        startBlock.GetComponent<MeshRenderer>().material.color = Color.red;
        startBlock.SetActive(false);
        endBlock = (GameObject)Instantiate(mTargetShowerPrefab, transform);
        endBlock.SetActive(false);
        endBlock.GetComponent<MeshRenderer>().material.color = Color.blue;

    }

    void Update()
    {
        if (mAstarTask == null)
        {
            // foreach (GameObject g in mPathShowerObjs)
            // {
            //     g.SetActive(false);
            // }

            Vector3Int blockPos;
            bool success = BlockLaser.GetBlockPositionAtMouse(Input.mousePosition, out blockPos);
            blockPos += new Vector3Int(0, 1, 0);
            if (success)
            {
                Vector3Int start = new Vector3Int(0, 1, 0);
                mAstarTask = Task.Run(() => new Astar().CalculatePath(start, blockPos));
                startBlock.SetActive(true);
                startBlock.transform.position = start;
                endBlock.SetActive(true);
                endBlock.transform.position = blockPos;
            }
        }
        else
        {
            if (mAstarTask.IsCompleted)
            {
                foreach (GameObject g in mPathShowerObjs)
                {
                    g.SetActive(false);
                }
                Astar.Result result = mAstarTask.Result;
                if (result.foundPath)
                {
                    int gObjIndex = 0;
                    while (result.path.Count > 0)
                    {
                        Vector3Int step = result.path.Pop();
                        mPathShowerObjs[gObjIndex].transform.position = step;
                        mPathShowerObjs[gObjIndex].SetActive(true);
                        gObjIndex++;
                    }
                }
                else
                {
                    Debug.Log(result.failReason.ToString());
                }
                startBlock.SetActive(false);
                endBlock.SetActive(false);
                mAstarTask = null;
            }
        }
    }
}
