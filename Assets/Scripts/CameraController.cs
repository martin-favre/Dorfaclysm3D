using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour, ISaveableComponent
{

    public float translationSpeed = 1f;
    public float zoomSpeed = 4f;
    public float dragSpeed = 12f;

    public float shellSize = 1;

    public Transform viewReference;

    float rotationAngleX = 0.0f;
    float rotationAngleY = 0.0f;

    public Vector2 rotationLimits = new Vector2(5, 80);

    Action onVerticalLevelChanged;
    int verticalLevel = 1;

    // Start is called before the first frame update
    void Start()
    {
        if (viewReference)
        {
            verticalLevel = Mathf.RoundToInt(viewReference.transform.position.y);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (viewReference)
        {

            HandleTranslationMovement();
            HandleRotation();
            if (Input.GetKey(KeyCode.LeftShift))
            {
                HandleVerticalMovement();
            }
            else
            {
                HandleScroll();
            }
        }
        if (Input.GetKeyDown(KeyCode.F5))
        {
            SaveLoadManager.RequestSave();
        }
        if (Input.GetKeyDown(KeyCode.F9))
        {
            SaveLoadManager.RequestLoad();
        }

    }

    public void SetOnVerticalLevelChanged(Action onChanged)
    {
        this.onVerticalLevelChanged = onChanged;
    }
    public void ClearOnVerticalLevelChanged()
    {
        this.onVerticalLevelChanged = null;
    }

    public int GetVerticalPosition()
    {
        return verticalLevel;
    }

    private void HandleVerticalMovement()
    {
        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
        Vector3 oldPos = viewReference.transform.position;
        if (scrollDelta > 0)
        {
            Vector3 refPos = viewReference.transform.position;
            verticalLevel += 1;
            if(verticalLevel > GridMap.Instance.GetSize().y) verticalLevel = GridMap.Instance.GetSize().y;
            viewReference.transform.position = new Vector3(refPos.x, verticalLevel, refPos.z);
        }
        else if (scrollDelta < 0)
        {
            Vector3 refPos = viewReference.transform.position;
            verticalLevel -= 1;
            if(verticalLevel < 0) verticalLevel = 0;
            viewReference.transform.position = new Vector3(refPos.x, verticalLevel, refPos.z);
        }
        if (onVerticalLevelChanged != null && viewReference.transform.position != oldPos)
        {
            onVerticalLevelChanged();
        }
    }

    private void HandleScroll()
    {
        Vector3 refPos = viewReference.position;
        Vector3 deltaPos = refPos - transform.position;
        Vector3 direction = deltaPos.normalized;
        Vector3 reverseDirection = -direction;

        Vector3 shellPos = refPos + reverseDirection * shellSize;
        deltaPos = shellPos - transform.position;


        float scrollDelta = Input.GetAxis("Mouse ScrollWheel");

        if (scrollDelta > 0)
        {
            transform.position += (deltaPos * zoomSpeed);
            // we are scrolling forward/zooming in

        }
        else if (scrollDelta < 0)
        {
            if (deltaPos.magnitude < 0.1f) deltaPos *= 5;
            transform.position += (deltaPos * -zoomSpeed);
            // we are scrolling backwards/zooming out

        }
    }


    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
    void HandleRotation()
    {
        if (Input.GetMouseButton(1))
        {

            Vector3 refPos = viewReference.position;
            Vector3 deltaPos = refPos - transform.position;
            rotationAngleX += Input.GetAxis("Mouse X") * dragSpeed;
            rotationAngleY -= Input.GetAxis("Mouse Y") * dragSpeed;
            rotationAngleY = ClampAngle(rotationAngleY, rotationLimits.x, rotationLimits.y);

            Quaternion rotation = Quaternion.Euler(rotationAngleY, rotationAngleX, 0);


            Vector3 negDistance = new Vector3(0.0f, 0.0f, -deltaPos.magnitude);
            Vector3 position = rotation * negDistance + viewReference.position;

            transform.rotation = rotation;
            transform.position = position;
        }
    }

    void HandleTranslationMovement()
    {
        Vector3 movementDirection = Vector3.zero;
        Vector3 refPos = viewReference.position;
        Vector3 deltaPos = refPos - transform.position;

        Vector3 direction = new Vector3(deltaPos.x, 0, deltaPos.z).normalized;
        Vector3 rotDir = Quaternion.AngleAxis(-90, Vector3.up) * direction;

        if (Input.GetKey(KeyCode.W)) movementDirection += direction;
        if (Input.GetKey(KeyCode.A)) movementDirection += rotDir;
        if (Input.GetKey(KeyCode.S)) movementDirection -= direction;
        if (Input.GetKey(KeyCode.D)) movementDirection -= rotDir;
        TranslateCamera(movementDirection);
    }

    void TranslateCamera(Vector3 dir)
    {
        viewReference.position += dir * translationSpeed;
    }


    [System.Serializable]
    private class SaveData : GenericSaveData<CameraController>
    {
        public SerializeableVector3 pos;
        public SerializeableQuaternion rot;
    }
    public IGenericSaveData Save()
    {
        var save = new SaveData();
        save.pos = new SerializeableVector3(transform.position);
        save.rot = new SerializeableQuaternion(transform.rotation);
        return save;
    }

    public void Load(IGenericSaveData data)
    {
        var save = (SaveData)data;
        transform.position = save.pos.Get();
        transform.rotation = save.rot.Get();
    }
}
