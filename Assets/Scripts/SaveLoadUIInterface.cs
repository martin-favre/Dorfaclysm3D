using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveLoadUIInterface : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnSavePressed(){
        SaveLoadManager.RequestSave();
    }

    public void OnLoadPressed(){
        SaveLoadManager.RequestLoad();
    }
}
