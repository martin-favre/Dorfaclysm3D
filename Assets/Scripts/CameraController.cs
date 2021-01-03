using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour, ISaveableComponent, IObservable<CameraController>
{
    [System.Serializable]
    private class SaveData : GenericSaveData<CameraController>
    {
        public Vector3 pos;
        public Quaternion rot;
        public Vector3 referencePos;
        public Vector2 rotationAngle;
    }

    public float translationSpeed = 1f;
    public float zoomSpeed = 4f;
    public float dragSpeed = 12f;

    public float shellSize = 1;
    public Vector2 minMaxZoom;

    private GameObject viewReference;
    Vector2 rotationAngle = Vector2.zero;
    public Vector2 rotationLimits = new Vector2(5, 80);

    public Vector3 initialPosition = new Vector3(0.5f, 0.5f, 7.5f);


    List<IObserver<CameraController>> onVerticalLevelChanged = new List<IObserver<CameraController>>();
    int verticalLevel = 1;

    Vector2 rotationRequest;
    Vector3 requestedTranslationMovement;
    float requestedScrollDelta;

    // Start is called before the first frame update
    void Start()
    {
        viewReference = Instantiate(new GameObject(), initialPosition, transform.rotation) as GameObject;
        transform.SetParent(viewReference.transform);
        verticalLevel = Mathf.RoundToInt(viewReference.transform.position.y);

        // Update rotation on start
        // We'll have a bit of choppyness on the first rotation otherwise
        ReadRotationInput();
        UpdateRotation();

        UpdateSubscribers();
    }

    // Update is called once per frame
    void Update()
    {
        ReadTranslationInput();
        if (Input.GetMouseButton(1))
        {
            ReadRotationInput();
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            // Currently handled in update as it seems to work fine.
            // Might move to FixedUpdate if this is no longer the case
            HandleVerticalMovement();
        }
        else
        {
            ReadScrollInput();
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

    void UpdateRotation()
    {
        rotationAngle.x += rotationRequest.x * dragSpeed * Time.deltaTime;
        rotationAngle.y -= rotationRequest.y * dragSpeed * Time.deltaTime;
        rotationAngle.y = ClampAngle(rotationAngle.y, rotationLimits.x, rotationLimits.y);

        Quaternion rotation = Quaternion.Euler(rotationAngle.y, rotationAngle.x, 0);


        Vector3 refPos = viewReference.transform.position;
        Vector3 deltaPos = refPos - transform.position;
        Vector3 negDistance = new Vector3(0.0f, 0.0f, -deltaPos.magnitude);
        Vector3 position = rotation * negDistance + viewReference.transform.position;

        transform.position = position;
        transform.rotation = rotation;
        rotationRequest = Vector2.zero;
    }

    void UpdateScroll()
    {

        if (requestedScrollDelta == 0) return;
        Vector3 refPos = viewReference.transform.position;
        Vector3 deltaPos = refPos - transform.position;
        Vector3 direction = deltaPos.normalized;
        Vector3 reverseDirection = -direction;

        Vector3 shellPos = refPos + reverseDirection * minMaxZoom.x;
        deltaPos = shellPos - transform.position;



        float deltaPosMagn = deltaPos.magnitude;
        if (requestedScrollDelta > 0)
        {
            transform.position += (deltaPos * zoomSpeed * Time.deltaTime);
            // we are scrolling forward/zooming in

        }
        else if (requestedScrollDelta < 0)
        {
            if (deltaPosMagn < 0.1f) deltaPos *= 5;
            if (deltaPosMagn > minMaxZoom.y) return;
            transform.position += (deltaPos * -zoomSpeed * Time.deltaTime);
            // we are scrolling backwards/zooming out
        }

        requestedScrollDelta = 0;

    }

    void FixedUpdate()
    {

        if (rotationRequest != Vector2.zero)
        {
            UpdateRotation();
        }
        if (requestedTranslationMovement != Vector3.zero)
        {
            viewReference.transform.position += requestedTranslationMovement * Time.deltaTime;
            requestedTranslationMovement = Vector3.zero;
        }
        UpdateScroll();
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
            if (verticalLevel > GridMap.Instance.GetSize().y) verticalLevel = GridMap.Instance.GetSize().y;
            viewReference.transform.position = new Vector3(refPos.x, verticalLevel, refPos.z);
        }
        else if (scrollDelta < 0)
        {
            Vector3 refPos = viewReference.transform.position;
            verticalLevel -= 1;
            if (verticalLevel < 0) verticalLevel = 0;
            viewReference.transform.position = new Vector3(refPos.x, verticalLevel, refPos.z);
        }
        if (viewReference.transform.position != oldPos)
        {
            foreach (var a in onVerticalLevelChanged)
            {
                UpdateSubscribers();
            }
        }
    }

    private void UpdateSubscribers()
    {
        foreach (var a in onVerticalLevelChanged)
        {
            a.OnNext(this);
        }
    }

    private void ReadScrollInput()
    {
        requestedScrollDelta += Input.GetAxis("Mouse ScrollWheel");
    }


    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
    void ReadRotationInput()
    {
        rotationRequest += new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }

    void ReadTranslationInput()
    {
        Vector3 movementDirection = Vector3.zero;
        Vector3 refPos = viewReference.transform.position;
        Vector3 deltaPos = refPos - transform.position;

        Vector3 direction = new Vector3(deltaPos.x, 0, deltaPos.z).normalized;
        Vector3 rotDir = Quaternion.AngleAxis(-90, Vector3.up) * direction;

        if (Input.GetKey(KeyCode.W)) movementDirection += direction;
        if (Input.GetKey(KeyCode.A)) movementDirection += rotDir;
        if (Input.GetKey(KeyCode.S)) movementDirection -= direction;
        if (Input.GetKey(KeyCode.D)) movementDirection -= rotDir;
        requestedTranslationMovement = movementDirection * translationSpeed;
    }

    public IGenericSaveData Save()
    {
        var save = new SaveData();
        save.pos = transform.position;
        save.rot = transform.rotation;
        save.referencePos = viewReference.transform.position;
        save.rotationAngle = rotationAngle;
        return save;
    }

    public void Load(IGenericSaveData data)
    {
        var save = (SaveData)data;
        transform.position = save.pos;
        transform.rotation = save.rot;
        initialPosition = save.referencePos;
        rotationAngle = save.rotationAngle;
    }

    public IDisposable Subscribe(IObserver<CameraController> observer)
    {
        IDisposable sub = new GenericUnsubscriber<CameraController>(onVerticalLevelChanged, observer);
        observer.OnNext(this);
        return sub;
    }

    public bool PositionShouldBeVisible(Vector3Int pos)
    {
        return pos.y <= GetVerticalPosition();
    }
}
