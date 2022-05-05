using System;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class StepUpTest : MonoBehaviour
{
    public Vector3 startPosition;
    public Vector3 endPosition;
    public float speed = 10;
    public float stepUpHeight = 1f;
    public float moveScale = 0;
    public CapsuleCollider Capsule { get; private set; }
    public Vector3 hitPoint { get; private set; }

    void Awake()
    {
        Capsule = GetComponent<CapsuleCollider>();
    }

    void Update()
    {
        if (moveScale == 1f)
        {
            moveScale = 0f;
            transform.position = startPosition;
        }

        moveScale = Mathf.MoveTowards(moveScale, 1f, speed * Time.deltaTime);
        Vector3 finalPosition = Vector3.Lerp(startPosition, endPosition, moveScale);
        Vector3 move = finalPosition - transform.position;
        move.y = 0;
        Move(move);
    }

    void Move(Vector3 move)
    {
        Vector3 topPoint = transform.position + transform.up * (Capsule.height / 2);
        Vector3 topSphere = transform.position + transform.up * ((Capsule.height / 2) - Capsule.radius);
        Vector3 center = transform.position + Capsule.center;
        Vector3 centerOut = transform.position + Capsule.center + (Vector3.forward * Capsule.radius);
        Vector3 baseSphere = transform.position + -transform.up * ((Capsule.height / 2) - Capsule.radius);
        Vector3 basePoint = transform.position + -transform.up * (Capsule.height / 2);
        float radius = Capsule.radius;

        Physics.CapsuleCast(topSphere, baseSphere, radius, move.normalized, out RaycastHit hit, move.magnitude);
        if (hit.IsHit() == false)
        {
            transform.position += move;
            Debug.Log("NoHit");
            return;
        }

        // {   // Debug Information

        //     Debug.DrawLine(topPoint, topPoint + move, Color.green);
        //     Debug.DrawLine(topPoint, topPoint + ((hit.point - topPoint).normalized * hit.distance), Color.red);

        //     Debug.DrawLine(center, center + move, Color.green);
        //     Debug.DrawLine(center, center + ((hit.point - center).normalized * hit.distance), Color.red);

        //     Debug.DrawLine(centerOut, centerOut + move, Color.green);
        //     Debug.DrawLine(centerOut, centerOut + ((hit.point - centerOut).normalized * hit.distance), Color.red);

        //     Debug.DrawLine(basePoint, basePoint + move, Color.green);
        //     Debug.DrawLine(basePoint, basePoint + ((hit.point - basePoint).normalized * hit.distance), Color.red);

        //     Vector3 closestPoint = Capsule.ClosestPoint(hit.point);
        //     Debug.DrawLine(closestPoint, closestPoint + ((hit.point - closestPoint).normalized * hit.distance), Color.red);

        // }

        Vector3 finalPos = transform.position;
        Vector3 vectorBaseToObstacleTop = hit.point - basePoint;
        Vector3 vectorBaseToObstacleBase = Vector3.ProjectOnPlane(vectorBaseToObstacleTop, transform.up);
        Vector3 vectorObstacleBaseToObstacleTop = Vector3.ProjectOnPlane(vectorBaseToObstacleTop, transform.forward);
        float obstacleHeight = vectorObstacleBaseToObstacleTop.magnitude;
        Debug.DrawLine(basePoint, basePoint + vectorBaseToObstacleTop, Color.red);
        Debug.DrawLine(basePoint, basePoint + vectorBaseToObstacleBase, Color.red);
        Debug.DrawLine(basePoint, basePoint + vectorObstacleBaseToObstacleTop, Color.blue);

        if (obstacleHeight <= stepUpHeight)
        {
            finalPos.y += obstacleHeight;
        }

        finalPos += move;

        hitPoint = hit.point;
        transform.position = finalPos;
    }

    void OnDrawGizmos()
    {
        if (hitPoint == Vector3.zero)
            return;

        Gizmos.DrawSphere(hitPoint, 0.001f);
        // Debug.Break();
    }
}
