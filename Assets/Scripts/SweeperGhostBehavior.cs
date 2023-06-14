using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

enum SweeperGhostState
{
    WANDERING,
    LIGHTUP,
    RETURNING,
    STOP
}

public class SweeperGhostBehavior : MonoBehaviour
{
    private CircleCollider2D collider;
    private SplineAnimate route;

    [SerializeField] private float turningOnDelay = 3.0f;
    [SerializeField] private bool canLight = false;
    private LightPickup targetLight;


    void Start()
    {
        collider = GetComponent<CircleCollider2D>();
        route = GetComponent<SplineAnimate>();
    }

    void Update()
    {
  
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("PickLight"))
        {
            targetLight = col.gameObject.GetComponent<LightPickup>();
            if (!targetLight.isOn && canLight)
            {
                targetLight.TurnOn();
                //StartCoroutine("TurnOnTimer");
            }
        }

        // Para fantasmas con damage hacer un chequeo aca y tener un bool serializable que sea canDamage y checkeamos
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
        route.Pause();
    }

    public void Reset()
    {
        //currentState = SweeperGhostState.WANDERING;
        route.Restart(true);
    }

    private void OnDrawGizmos()
    {
    }



}
