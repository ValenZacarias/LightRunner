using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

[ExecuteInEditMode]
public class SweeperGhostBehavior : MonoBehaviour, IEnemyBehavior
{
    [Header("LIGHTING")]
    [SerializeField] private float turningOnDelay = 3.0f;
    [SerializeField] private bool canLight = false;
    private LightPickup targetLight;

    [Header("MOVEMENT")]
    [SerializeField] private bool canMove = true;
    [SerializeField] private SplineContainer routeSpline;
    [SerializeField] private bool closedSpline = false;
    [SerializeField] [Range(0.0f, 1.0f)] private float offset = 0.0f;
    [SerializeField] private float moveSpeed = 0.2f;
    [SerializeField] private AnimationCurve speedCurve;
    
    private float alpha = 0.0f, closedAlpha = 0.0f;
    private float dir = 1.0f;
    private float3 newPos, newTang, newUp;
    private float3 offsetPos, offsetTang, offsetUp;

    void Awake()
    {
        if (routeSpline == null) Debug.LogError(transform.name + " SPLINE MISSING");

        alpha += offset + 0.05f;
        routeSpline.Evaluate(offset, out offsetPos, out offsetTang, out offsetUp);
        transform.position = (Vector3)offsetPos;
    }

    private void HandleSplineMovement()
    {
        if (!canMove) return;

        alpha += Time.deltaTime * moveSpeed * dir;

        if(closedSpline)
        {
            closedAlpha = alpha % 1.0f;
            routeSpline.Spline.Evaluate(speedCurve.Evaluate(closedAlpha), out newPos, out newTang, out newUp);
            transform.position = (Vector3)newPos + routeSpline.transform.position;
        }
        else
        {
            if (Mathf.Abs(0.0f - alpha) < 0.01f || Mathf.Abs(alpha - 1.0f) < 0.01)
            {
                dir *= -1.0f;
            }
            routeSpline.Spline.Evaluate(speedCurve.Evaluate(alpha), out newPos, out newTang, out newUp);
            transform.position = (Vector3)newPos + routeSpline.transform.position;
        }    
    }

    private void FixedUpdate()
    {
        HandleSplineMovement();
    }

    private void LateUpdate()
    {
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        //Debug.Log(col);
        if (col.CompareTag("PickLight"))
        {
            targetLight = col.gameObject.GetComponent<LightPickup>();
            if (!targetLight.isOn && canLight)
            {
                targetLight.TurnOn();
            }
        }
        if (col.CompareTag("Blocker"))
        {
            dir *= -1;
        }
    }

    IEnumerator TurnOnTimer()
    {
        yield return new WaitForSeconds(turningOnDelay);
        targetLight.TurnOn();
        canLight = false;
    }

    public void Stop()
    {
        canMove = false;
        //currentState = SweeperGhostState.STOP;
        //route.Pause();
    }

    public void Reset()
    {
        canMove = true;
        //currentState = SweeperGhostState.WANDERING;
        //route.Restart(true);
    }

    

    private void OnDrawGizmos()
    {
        routeSpline.Evaluate(offset, out offsetPos, out offsetTang, out offsetUp);
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere((Vector3)offsetPos, 0.4f);
    }
}
