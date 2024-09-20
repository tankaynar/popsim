using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private Vector2 speed;
    private Vector2 accelaration;
    private float zoom;

    [SerializeField] private float wasdSpeed;
    [SerializeField] private float dampen;
    [SerializeField] private float mouseSpeed;
    [SerializeField] private float scrollSpeed;
    
    private void LateUpdate()
    {
        // ---------- movement ----------
        accelaration = Vector2.zero;
        accelaration.x = Input.GetAxis("Horizontal")*wasdSpeed;
        accelaration.y = Input.GetAxis("Vertical")*wasdSpeed;
        
        if (Input.GetMouseButton(1))
        {
            accelaration -= new Vector2(Input.GetAxis("Mouse X") * mouseSpeed, Input.GetAxis("Mouse Y") * mouseSpeed);
        }
        
        speed *= 1f - dampen;
        
        speed += accelaration;

        transform.position += (Vector3)speed*Time.deltaTime;
        // ---------- ----------
        
        // scrolling
        Camera.main.orthographicSize += Input.GetAxis("Mouse ScrollWheel")*scrollSpeed*Camera.main.orthographicSize;
    }
}
