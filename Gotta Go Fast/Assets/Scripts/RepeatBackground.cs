using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles position of Background to create endless scrolling Background effect
/// </summary>
public class RepeatBackground : MonoBehaviour
{
    protected Vector3 startingPos;
    protected float repeatWidth;
    protected float repeatDivider;

    void Awake()
    {
        repeatDivider = 2.0f;
        startingPos = transform.position;
        repeatWidth = GetComponent<BoxCollider>().size.x / repeatDivider;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.position.x < startingPos.x - repeatWidth)
        {
            transform.position = startingPos;
        }
    }
}