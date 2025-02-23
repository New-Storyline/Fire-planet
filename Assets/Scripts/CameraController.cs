using System;
using UnityEngine;

/// <summary>
/// Class responsible for camera movement and tile click detection
/// </summary>
public class CameraController : MonoBehaviour
{
    public delegate void OnTileClickedHandler(Vector2Int pos);

    public event OnTileClickedHandler TileClicked;

    public bool isCameraMoveEnabled = true;
    public bool isCanSelectTiles = true;

    private Vector3 camMinPos; private Vector3 camMaxPos;

    public void Update()
    {
        Moving();
        CheckMouseClick();
    }

    private void Moving() {

        if (!isCameraMoveEnabled)
            return;

        float axisX = Input.GetAxis("Horizontal");
        float axisZ = Input.GetAxis("Vertical");
        float axisY = Input.GetAxis("Mouse ScrollWheel");

        transform.position += new Vector3(axisX, axisY * GameConfig.SCROLL_WHEEL_SPEED_MULTIPLIER, axisZ) * GameConfig.CAMERA_SPEED * Time.deltaTime;
        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, camMinPos.x, camMaxPos.x), 
            Mathf.Clamp(transform.position.y, camMinPos.y, camMaxPos.y), 
            Mathf.Clamp(transform.position.z, camMinPos.z, camMaxPos.z)
        );
    }

    private void CheckMouseClick()
    {
        if (!isCanSelectTiles)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 hitPoint = hit.point;

                TileClicked?.Invoke(new Vector2Int((int)(hitPoint.x), (int)(hitPoint.z)));
            }
        }
    }

    public void SetMoveLimits(Vector3 minPos, Vector3 maxPos)
    {
        camMinPos = minPos;
        camMaxPos = maxPos;
    }


}
