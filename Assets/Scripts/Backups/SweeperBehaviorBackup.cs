using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

enum SweeperGhostState
{
    WANDERING,
    LIGHTUP,
    RETURNING,
    STOP
}

public class SweeperBehaviorBackup : MonoBehaviour, IEnemyBehavior
{
    private CircleCollider2D collider;
    private SplineAnimate route;
    private Spline routeSpline;

    [SerializeField] private float turningOnDelay = 3.0f;
    [SerializeField] private bool canLight = false;
    private LightPickup targetLight;

    [Header("BLOCKER")]
    public bool HasBlocker = false;
    [Range(0, 1.0f)] public float BlockerSplinePos = 0.5f;

    [SerializeField] bool blockerEnabled = false;
    [SerializeField] bool canToggleBlocker = true;
    [SerializeField] bool waitingForToggleBlocker = false;

    [Header("DEBUG")]
    [SerializeField] private float lastFrameT = 0.0f;
    [SerializeField] private float NormalizedTime = 0.0f;

    [SerializeField] private Spline OriginalSpline;
    [SerializeField] List<BezierKnot> OriginalKnots;
    [SerializeField] private float OriginalDuration;
    [SerializeField] private float OriginalSplineLenght;

    [SerializeField] private Spline splineR;
    [SerializeField] List<BezierKnot> LeftKnots = new List<BezierKnot>();
    [SerializeField] private float splineRLength;
    [SerializeField] private float nearestR_t;
    [SerializeField] private float3 nearestR_pos;

    [SerializeField] private Spline splineL;
    [SerializeField] List<BezierKnot> RightKnots = new List<BezierKnot>();
    [SerializeField] private float splineLLength;
    [SerializeField] private float nearestL_t;
    [SerializeField] private float3 nearestL_pos;

    [SerializeField] private bool movingForward = true;

    void Awake()
    {

    }

    void Start()
    {
        collider = GetComponent<CircleCollider2D>();
        route = GetComponent<SplineAnimate>();
        routeSpline = route.Container.Spline;
        OriginalKnots = routeSpline.Knots.ToList();
        OriginalDuration = route.Duration;
        OriginalSplineLenght = routeSpline.GetLength();
        GetSplineDivision();

    }

    public static float TriangleWave(float t, float p) // p = period 
    {
        return Mathf.Abs(t / p - Mathf.Floor(t / p + 1 / 2));
    }

    public static float TriangleWave2(float t, float p) // p = period 
    {
        return 2 / p * Mathf.Abs(((t - p / 2) % p) - p / 2);
    }

    [Header("MOVEMENT")]
    [SerializeField] private float3 newPos;
    [SerializeField] private float3 newTang;
    [SerializeField] private float3 newUp;
    [SerializeField] private float alpha = 0.1f;
    [SerializeField] private float dir = 1.0f;
    [SerializeField] private float period = 1.0f;
    [SerializeField] private float moveSpeed = 1.0f;

    private void HandleSplineMovement()
    {
        alpha += Time.deltaTime % 1.0f * moveSpeed * dir;

        if (Mathf.Abs(0.0f - alpha) < 0.0001f || Mathf.Abs(alpha - 1.0f) < 0.0001)
        {
            dir *= -1.0f;
        }

        routeSpline.Evaluate(alpha, out newPos, out newTang, out newUp);
        transform.position = (Vector3)newPos + route.Container.transform.position;
    }

    private void FixedUpdate()
    {
        //Debug.Log("FU");
        HandleSplineMovement();
    }

    void Update()
    {
        //HandleBlocker();


        if (Input.GetKeyDown(KeyCode.K) && canToggleBlocker)
        {
            Debug.Log("SWAP");
            dir *= -1.0f;
        }

        SplineUtility.GetNearestPoint(splineR, (float3)transform.position - (float3)route.Container.transform.position, out nearestR_pos, out nearestR_t);
        SplineUtility.GetNearestPoint(splineL, (float3)transform.position - (float3)route.Container.transform.position, out nearestL_pos, out nearestL_t);

    }
    private void LateUpdate()
    {
        if (route.NormalizedTime - lastFrameT > 0)
        {
            movingForward = true;
        }
        else
        {
            movingForward = false;
        }

        lastFrameT = route.NormalizedTime;
    }


    private void GetSplineDivision()
    {
        LeftKnots.Clear();
        RightKnots.Clear();
        float sum = 0.0f;
        float knotT = 0.0f;

        LeftKnots.Add(routeSpline.Knots.First());
        for (int i = 1; i < routeSpline.Knots.Count() - 1; ++i)
        {
            sum += routeSpline.GetCurveLength(i);
            knotT = sum / routeSpline.GetLength();

            if (knotT < BlockerSplinePos)
            {
                LeftKnots.Add(routeSpline.Knots.ElementAt(i));
            }
            else
            {
                RightKnots.Add(routeSpline.Knots.ElementAt(i));
            }
        }

        RightKnots.Add(routeSpline.Knots.Last());
        sum = 0.0f;
        knotT = 0.0f;

        splineR = new Spline(RightKnots);
        splineRLength = splineR.GetLength();

        splineL = new Spline(LeftKnots);
        splineLLength = splineL.GetLength();
    }

    private void ReverseSpline(Spline spline)
    {
        Debug.Log("REVERSE");
        List<BezierKnot> reversed = new List<BezierKnot>();
        for (int i = spline.Knots.Count(); i < 0; i--)
        {
            Debug.Log("knot " + i);
            reversed.Add(spline.Knots.ElementAt(i));
        }

        spline.Knots = reversed;
    }

    private void HandleBlocker()
    {
        if (!waitingForToggleBlocker) return;

        route.Container.Spline.Reverse();

        return;

        if (movingForward)
        {
            canToggleBlocker = true;
            ToggleBlocker();
        }
        else
        {
            //si no hay blocker entoces esperamos a ir al origen de splineR o splineL
            if (!blockerEnabled)
            {
                if (route.NormalizedTime > BlockerSplinePos)
                {
                    if (Vector3.Distance(transform.position, (Vector3)RightKnots[0].Position + route.Container.transform.position) < 0.1f)
                    {
                        canToggleBlocker = true;
                        ToggleBlocker();
                        StartCoroutine("ToggleBlockerDelay");

                    }
                    else
                    {
                        //wait
                        Debug.Log("WAITING -> RIGHT");
                        canToggleBlocker = false;
                    }
                }
                else
                {
                    if (Vector3.Distance(transform.position, (Vector3)LeftKnots[0].Position + route.Container.transform.position) < 0.1f)
                    {
                        canToggleBlocker = true;
                        ToggleBlocker();
                        StartCoroutine("ToggleBlockerDelay");

                    }
                    else
                    {
                        //wait
                        Debug.Log("WAITING -> LEFT");
                        canToggleBlocker = false;
                    }
                }
            }
            else // volvemos a origen de spline original
            {
                if (Vector3.Distance(transform.position, (Vector3)OriginalKnots[0].Position + route.Container.transform.position) < 0.1f)
                {
                    canToggleBlocker = true;
                    ToggleBlocker();
                    StartCoroutine("ToggleBlockerDelay");

                }
                else
                {
                    //wait
                    Debug.Log("WAITING -> ORIGINAL");
                    canToggleBlocker = false;
                }
            }


        }
    }

    private void ToggleBlocker()
    {
        if (!canToggleBlocker) return;

        Debug.Log("TOGGLE");

        if (!blockerEnabled)
        {
            if (route.NormalizedTime > BlockerSplinePos)
            {
                float newT = 0.0f;
                float3 newPos = new float3(0.0f, 0.0f, 0.0f);
                SplineUtility.GetNearestPoint(splineR, (float3)transform.position - (float3)route.Container.transform.position, out newPos, out newT, 8, 4);

                route.Duration = OriginalDuration * splineRLength / OriginalSplineLenght;
                route.NormalizedTime = newT;
                routeSpline.Knots = RightKnots;

            }
            else
            {
                float newT = 0.0f;
                float3 newPos = new float3(0.0f, 0.0f, 0.0f);
                SplineUtility.GetNearestPoint(splineL, (float3)transform.position - (float3)route.Container.transform.position, out newPos, out newT);

                route.Duration = OriginalDuration * splineLLength / OriginalSplineLenght;
                route.NormalizedTime = newT;
                routeSpline.Knots = LeftKnots;
            }

            blockerEnabled = true;
        }
        else
        {
            routeSpline.Knots = OriginalKnots;
            float newT = 0.0f;
            float3 newPos = new float3(0.0f, 0.0f, 0.0f);
            SplineUtility.GetNearestPoint(routeSpline, (float3)transform.position - (float3)route.Container.transform.position, out newPos, out newT);

            route.Duration = OriginalDuration;
            route.NormalizedTime = newT;
            blockerEnabled = false;
        }

        waitingForToggleBlocker = false;
        canToggleBlocker = false;
        StartCoroutine("ToggleBlockerDelay");
    }

    private void SetNewRouteSpline(Spline newSpline)
    {
        routeSpline.Knots = newSpline.Knots;
        float newT = 0.0f;
        float3 newPos = new float3(0.0f, 0.0f, 0.0f);
        SplineUtility.GetNearestPoint(newSpline, (float3)transform.position - (float3)route.Container.transform.position, out newPos, out newT);

        route.Duration = OriginalDuration;
        route.NormalizedTime = newT;
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
        canToggleBlocker = true;
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
        if (HasBlocker)
        {
            foreach (BezierKnot lKnot in LeftKnots)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere((Vector3)lKnot.Position + route.Container.transform.position, 0.4f);
            }

            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere((Vector3)nearestR_pos + route.Container.transform.position, 0.4f);

            foreach (BezierKnot rKnot in RightKnots)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere((Vector3)rKnot.Position + route.Container.transform.position, 0.4f);
            }

            Gizmos.color = Color.green;
            Gizmos.DrawSphere((Vector3)nearestL_pos + route.Container.transform.position, 0.4f);

        }
    }
}

