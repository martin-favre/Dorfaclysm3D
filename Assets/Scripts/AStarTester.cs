using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class AStarTester : MonoBehaviour
{
    Vector3Int targetBlock = Vector3Int.zero;
    Task<Astar.Result> astarTask = null;
    public GameObject pathShowerPrefab;
    public GameObject targetShowerPrefab;
    List<GameObject> pathShowerObjs;
    GameObject startBlock;

    GameObject endBlock;



    void Start()
    {
        pathShowerObjs = new List<GameObject>();
        for (int i = 0; i < 100; i++)
        {
            GameObject g = (GameObject)Instantiate(pathShowerPrefab, transform);
            g.SetActive(false);
            pathShowerObjs.Add(g);
        }

        startBlock = (GameObject)Instantiate(targetShowerPrefab, transform);
        startBlock.GetComponent<MeshRenderer>().material.color = Color.red;
        startBlock.SetActive(false);
        endBlock = (GameObject)Instantiate(targetShowerPrefab, transform);
        endBlock.SetActive(false);
        endBlock.GetComponent<MeshRenderer>().material.color = Color.blue;

    }

    void Update()
    {
        if (astarTask == null)
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
                astarTask = Task.Run(() => new Astar().CalculatePath(start, blockPos));
                startBlock.SetActive(true);
                startBlock.transform.position = start;
                endBlock.SetActive(true);
                endBlock.transform.position = blockPos;
            }
        }
        else
        {
            if (astarTask.IsCompleted)
            {
                foreach (GameObject g in pathShowerObjs)
                {
                    g.SetActive(false);
                }
                Astar.Result result = astarTask.Result;
                if (result.foundPath)
                {
                    int gObjIndex = 0;
                    while (result.path.Count > 0)
                    {
                        Vector3Int step = result.path.Pop().Get();
                        pathShowerObjs[gObjIndex].transform.position = step;
                        pathShowerObjs[gObjIndex].SetActive(true);
                        gObjIndex++;
                    }
                }
                else
                {
                    Debug.Log(result.failReason.ToString());
                }
                startBlock.SetActive(false);
                endBlock.SetActive(false);
                astarTask = null;
            }
        }
    }
}
