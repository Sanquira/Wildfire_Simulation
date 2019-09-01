using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareMapCamera : MonoBehaviour
{
    private const float MIN_CAMERA_OFFSET = 1;

    public float stickMinZoom, stickMaxZoom;
    public float swivelMinZoom, swivelMaxZoom;
    public float moveSpeedMinZoom, moveSpeedMaxZoom;
    public float rotationSpeed;

    //public TerrainGrid grid;

    Transform swivel, stick;

    float zoom = 1;
    float rotationAngle;

    void Awake()
    {
        rotationAngle = transform.localRotation.eulerAngles.y;
        swivel = transform.GetChild(0);
        stick = swivel.GetChild(0);
        AdjustZoom(0);
        AdjustRotation(0);
        AdjustPosition(0, 0);
    }

    void Update()
    {
        float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
        if (zoomDelta != 0f)
        {
            AdjustZoom(zoomDelta);
        }

        float rotationDelta = Input.GetAxis("Rotation");
        if (rotationDelta != 0)
        {
            AdjustRotation(rotationDelta);
        }

        float xDelta = Input.GetAxis("Horizontal");
        float zDelta = Input.GetAxis("Vertical");
        if (xDelta != 0 || zDelta != 0)
        {
            AdjustPosition(xDelta, zDelta);
        }
    }

    private void AdjustRotation(float rotationDelta)
    {
        rotationAngle += rotationDelta * rotationSpeed * Time.deltaTime;
        if (rotationAngle < 0f)
        {
            rotationAngle += 360f;
        }
        else if (rotationAngle >= 360f)
        {
            rotationAngle -= 360f;
        }
        transform.localRotation = Quaternion.Euler(0, rotationAngle, 0);
    }

    private void AdjustPosition(float xDelta, float zDelta)
    {
        Vector3 direction = transform.localRotation * new Vector3(xDelta, 0, zDelta).normalized;
        float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
        float distance = Mathf.Lerp(moveSpeedMinZoom, moveSpeedMaxZoom, zoom) * damping * Time.deltaTime;

        Vector3 position = transform.localPosition;
        position += direction * distance;
        transform.localPosition = ClampPosition(position);
    }

    void AdjustZoom(float zoomDelta)
    {
        zoom = Mathf.Clamp01(zoom + zoomDelta);
        float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
        stick.localPosition = new Vector3(0, 0, distance);

        float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
        swivel.localRotation = Quaternion.Euler(angle, 0, 0);
    }

    Vector3 ClampPosition(Vector3 position)
    {
        Terrain activeTerrain = Terrain.activeTerrain;
        Collider terrainColider = activeTerrain.GetComponent<Collider>();
        position.x = Mathf.Clamp(position.x, terrainColider.bounds.min.x, terrainColider.bounds.max.x);
        position.z = Mathf.Clamp(position.z, terrainColider.bounds.min.z, terrainColider.bounds.max.z);

        position.y = Terrain.activeTerrain.SampleHeight(position);
        return position;
    }
}
