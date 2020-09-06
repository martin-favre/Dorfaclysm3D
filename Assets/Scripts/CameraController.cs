using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour, ISaveable
{

    public float mTranslationSpeed = 1f;
    public float mLookSpeedH = 4f;
    public float mLookSpeedV = 4f;
    public float mZomSpeed = 4f;
    public float mDragSpeed = 12f;

    float mYaw = 0;
    float mPitch = 0;

    Camera mCamera;
    // Start is called before the first frame update
    void Start()
    {
        mCamera = GetComponent<Camera>();
        mPitch = transform.eulerAngles.x;
        mYaw = transform.eulerAngles.y;
    }

    // Update is called once per frame
    void Update()
    {
        handleTranslationMovement();
        handleRotation();
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveLoadManager.RequestSave();
        }
        if (Input.GetKeyDown(KeyCode.F9))
        {
            SaveLoadManager.RequestLoad();
        }

    }

    void handleRotation()
    {
        //Look around with Right Mouse
        if (Input.GetMouseButton(1))
        {
            mYaw += mLookSpeedH * Input.GetAxis("Mouse X");
            mPitch -= mLookSpeedV * Input.GetAxis("Mouse Y");

            transform.eulerAngles = new Vector3(mPitch, mYaw, 0f);
        }

        //drag camera around with Middle Mouse
        if (Input.GetMouseButton(2))
        {
            transform.Translate(-Input.GetAxisRaw("Mouse X") * Time.deltaTime * mDragSpeed, -Input.GetAxisRaw("Mouse Y") * Time.deltaTime * mDragSpeed, 0);
        }

        //Zoom in and out with Mouse Wheel
        transform.Translate(0, 0, Input.GetAxis("Mouse ScrollWheel") * mZomSpeed, Space.Self);

    }

    void handleTranslationMovement()
    {
        Vector3 movementDirection = Vector3.zero;
        if (Input.GetKey(KeyCode.W)) movementDirection += new Vector3(0, 0, 1);
        if (Input.GetKey(KeyCode.A)) movementDirection += new Vector3(-1, 0, 0);
        if (Input.GetKey(KeyCode.S)) movementDirection += new Vector3(0, 0, -1);
        if (Input.GetKey(KeyCode.D)) movementDirection += new Vector3(1, 0, 0);
        translateCamera(movementDirection);
    }

    void translateCamera(Vector3 dir)
    {
        if (mCamera)
        {
            mCamera.transform.Translate(dir * mTranslationSpeed);
        }
    }


    [System.Serializable]
    private struct SaveData : IComponentSaveData
    {
        public SerializeableVector3 pos;
        public SerializeableQuaternion rot;

        public string GetAssemblyName()
        {
            return typeof(CameraController).AssemblyQualifiedName;
        }
    }
    public IComponentSaveData Save()
    {
        var save = new SaveData();
        save.pos = new SerializeableVector3(transform.position);
        save.rot = new SerializeableQuaternion(transform.rotation);
        return save;
    }

    public void Load(IComponentSaveData data)
    {
        var save = (SaveData)data;
        transform.position = save.pos.Get();
        transform.rotation = save.rot.Get();
    }
}
