﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeParallax : MonoBehaviour
{
    public float xAmount = 5f;
    public float yAmount = 2f;

    private Transform cam;
    private Vector3 previousCamPos;

    // Start is called before the first frame update
    void Awake()
    {
        cam = Camera.main.transform;
    }

    private void Start()
    {
        previousCamPos = cam.position;
    }

    // Update is called once per frame
    void Update()
    {
        var amt = (previousCamPos - cam.position) * -transform.position.z;
        var nextPos = transform.position + new Vector3(amt.x * xAmount, amt.y * yAmount, 0);
        transform.position = Vector3.Lerp(transform.position, nextPos, Time.deltaTime);
        previousCamPos = cam.position;
    }
}
