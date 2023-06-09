using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

enum GhostState
{
    WANDERING,
    LIGHTUP,
    RETURNING
}

public class WandererGhostBehavior : MonoBehaviour
{
    private CircleCollider2D collider;
    private SplineAnimate route;

    [SerializeField] private float routeReturnPositionTime;
    private Vector3 lastRoutePosition;
    private GhostState currentState;

    [SerializeField] private float chaseSpeed = 3.0f;
    [SerializeField] private float turningOnDelay = 3.0f;
    [SerializeField] private float returnPossitionOffset = 0.05f;
    private Vector3 targetLightPosition;
    private LightPickup targetLight;

    // Start is called before the first frame update
    void Start()
    {
        collider = GetComponent<CircleCollider2D>();
        route = GetComponent<SplineAnimate>();
        currentState = GhostState.WANDERING;
        route.StartOffset = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {
        if(currentState == GhostState.LIGHTUP && targetLightPosition != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetLightPosition, chaseSpeed*Time.deltaTime);
            if(Vector3.Magnitude(transform.position - targetLightPosition) <= 0.1f)
            {
                StartCoroutine("TurnOnTimer");
            }
        }

        if(currentState == GhostState.RETURNING && lastRoutePosition != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, lastRoutePosition, chaseSpeed * Time.deltaTime);
            if(transform.position == lastRoutePosition)
            {
                Debug.Log("BEFORE: " + route.NormalizedTime);
                route.NormalizedTime = routeReturnPositionTime;

                Debug.Log("AFTER: " + route.NormalizedTime);
                route.Play();
                currentState = GhostState.WANDERING;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("PickLight"))
        {
            targetLight = col.gameObject.GetComponent<LightPickup>();
            if (!targetLight.isOn)
            {
                targetLightPosition = col.gameObject.transform.position;

                routeReturnPositionTime = (route.NormalizedTime + returnPossitionOffset)%1.0f; //Modulo 1 para que si se pase de 1 vuelva a cero
                lastRoutePosition = route.Container.EvaluatePosition(routeReturnPositionTime);
                currentState = GhostState.LIGHTUP;

                route.Pause();
            }
        }
    }

    private void OnDrawGizmos()
    {
        if(lastRoutePosition != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(lastRoutePosition, 0.5f);
        }
        
    }
    IEnumerator TurnOnTimer()
    {
        //Debug.Log("Starting timer");
        yield return new WaitForSeconds(turningOnDelay);
        targetLight.TurnOn();
        currentState = GhostState.RETURNING;
    }


}