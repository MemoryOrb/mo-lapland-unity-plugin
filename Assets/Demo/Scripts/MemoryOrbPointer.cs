using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MemoryOrbPointer : MonoBehaviour
{
    public float defaultLength = 4.0f;

    private LineRenderer lineRenderer = null;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        UpdateLength();
    }

    private void UpdateLength()
    {
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, GetEnd());
    }

    private Vector3 GetEnd()
    {
        Vector3 endPosition = CalculateEnd(defaultLength);
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, defaultLength))
        {
            endPosition = hit.point;
            lineRenderer.startColor = Color.cyan;
        } else
        {
            lineRenderer.startColor = Color.white;
        }
        return endPosition;
    }

    protected Vector3 CalculateEnd(float length)
    {
        return transform.position + (transform.forward * length);
    }
}
