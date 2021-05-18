using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualAimingThrowing : MonoBehaviour
{
    [SerializeField] private LineRenderer _line;
    [SerializeField] private SpriteRenderer _circle;
    [SerializeField] private LayerMask collisionLayer;

    private float _radius;

    private void Awake()
    {
        _circle.transform.rotation = Quaternion.Euler(90, 0, 0);
    }
    public void SetRadius(float newRadius)
    {
        _radius = newRadius;
        _circle.transform.localScale = Vector3.one * _radius * 2f;
    }

    public void SetVisualThrowing(Vector3 startPosition, Vector3 launchDirection, float launchForce)
    {
        List<Vector3> points = new List<Vector3>();
        _line.positionCount = 50;
        int index = 0;
        do
        {
            points.Add(GetThrowingPointPosition(startPosition, index * 0.03f, launchDirection, launchForce));
            index++;
        } while (points[index - 1].y > 0.25f && !Physics.CheckSphere(points[index - 1], 0.05f, collisionLayer));
        _line.positionCount = points.Count;
        _line.SetPositions(points.ToArray());
        Vector3 circlePosition = points[points.Count - 1];
        circlePosition.y = 0.025f;
        _circle.transform.position = circlePosition;
    }
    private Vector3 GetThrowingPointPosition(Vector3 startPosition, float t, Vector3 launchDirection, float launchForce)
    {
        Vector3 pos = startPosition + (launchDirection.normalized * launchForce * t)
            + 0.5f * Physics.gravity * (t * t);
        return pos;
    }
}
