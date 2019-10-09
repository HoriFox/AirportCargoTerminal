using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float zoomSpeed = 2f;
    public float dragSpeed = 6f;

    public Texture2D cursor1;

    Camera mainCamera;

    public void Start()
    {
        mainCamera = transform.GetComponent<Camera>();
    }

    void Update()
    {
        //Left Mouse
        if (Input.GetMouseButtonDown(0))
        {
            Cursor.SetCursor(cursor1, Vector2.zero, CursorMode.Auto);
        }
        //Left Mouse
        if (Input.GetMouseButton(0))
        {
            transform.Translate(-Input.GetAxis("Mouse X") * dragSpeed, -Input.GetAxisRaw("Mouse Y") * dragSpeed, 0);
        }
        //Left Mouse
        if (Input.GetMouseButtonUp(0))
        {
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }

        // Приближение и удаление с помощью колёсика
        mainCamera.orthographicSize = mainCamera.orthographicSize - Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
    }
}
