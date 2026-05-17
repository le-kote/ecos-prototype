using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FootPositioner : MonoBehaviour
{
    [Header("Ray Settings")]
    public Transform[] ChainParts = {};
    public float MaxLegLength = 1;

    [Header("Ground detection")]
    public Transform Effector;
    public LayerMask GroundMask;
    public bool IsGrounded => Physics2D.OverlapCircle(Effector.position, .1f, GroundMask);

    private bool TryGetNextPoint(Vector3 origin, Vector3 target, out Vector3 point)
    {
        var direction = (target - origin).normalized;
        var maxLength = (target - origin).magnitude;

        var cast = Physics2D.Raycast(origin, direction, maxLength, GroundMask);

        point = cast ? cast.point : target;
        return cast;
    }

    private Vector3[] GetGroundPointsChain(out Vector3 last)
    {
        var result = new List<Vector3> { transform.position };
        bool pointFound = false;
        Transform prev = transform;

        foreach (var item in ChainParts)
        {
            pointFound = TryGetNextPoint(prev.position, item.position, out var point);
            result.Add(point);

            if (pointFound)
                break;

            prev = item;
        }

        if (!pointFound)
        {
            TryGetNextPoint(prev.position, prev.position - prev.up * MaxLegLength, out var point);
            result.Add(point);
        }

        last = result.Last();
        return result.ToArray();
    }

    public Vector3 GetGroundPos()
    {
        Transform prev = transform;

        foreach (var item in ChainParts)
        {
            if (TryGetNextPoint(prev.position, item.position, out var point))
                return point;

            prev = item;
        }

        TryGetNextPoint(prev.position, prev.position - prev.up * MaxLegLength, out var lastPoint);
        return lastPoint;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLineStrip(GetGroundPointsChain(out var last), true);
        Gizmos.DrawSphere(last, .1f);
    }
}
