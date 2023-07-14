using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;


public class SweeperGhostBehavior : MonoBehaviour, IEnemyBehavior
{
    private CircleCollider2D collider;

    [SerializeField] private float turningOnDelay = 3.0f;
    [SerializeField] private bool canLight = false;
    private LightPickup targetLight;
    
    [Header("MOVEMENT")]
    [SerializeField] private SplineContainer routeSpline;
    [SerializeField] private float3 newPos, newTang, newUp;
    [SerializeField] private float alpha = 0.0f, normalizedTime = 0.0f;
    [SerializeField] private float dir = 1.0f;
    [SerializeField] private float offset = 0.2f;
    [SerializeField] private float moveSpeed = 0.2f;

    void Awake()
    {
        
    }

    void Start()
    {
        collider = GetComponent<CircleCollider2D>();
        alpha += offset;
        normalizedTime += offset;
    }

    private void HandleSplineMovement()
    {
        //aca para poder tener easing deberia modular moveSpeed pero no se como ahra
        alpha += Time.deltaTime * moveSpeed * dir;
        if (Mathf.Abs(0.0f - alpha) < 0.005f || Mathf.Abs(alpha - 1.0f) < 0.005)
        {
            dir *= -1.0f;
        }

        routeSpline.Spline.Evaluate(alpha, out newPos, out newTang, out newUp);
        transform.position = (Vector3)newPos + routeSpline.transform.position;
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
        Debug.Log(col);
        if (col.CompareTag("PickLight"))
        {
            targetLight = col.gameObject.GetComponent<LightPickup>();
            if (!targetLight.isOn && canLight)
            {
                targetLight.TurnOn();
                //StartCoroutine("TurnOnTimer");
            }
        }
        if (col.CompareTag("Blocker"))
        {
            Debug.Log("BLOCKER");

            dir *= -1;
        }

        // Para fantasmas con damage hacer un chequeo aca y tener un bool serializable que sea canDamage y checkeamos
    }

    IEnumerator ToggleBlockerDelay()
    {
        yield return new WaitForSeconds(1);
        //canToggleBlocker = true;
    }

    IEnumerator TurnOnTimer()
    {
        yield return new WaitForSeconds(turningOnDelay);
        targetLight.TurnOn();
        canLight = false;
    }

    public void Stop()
    {
        //currentState = SweeperGhostState.STOP;
        //route.Pause();
    }

    public void Reset()
    {
        //currentState = SweeperGhostState.WANDERING;
        //route.Restart(true);
    }

    private void OnDrawGizmos()
    {
        
        
    }
}
