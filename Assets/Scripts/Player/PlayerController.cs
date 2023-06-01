using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct FrameInput
{
    public float X;
    public bool JumpUp;
    public bool JumpDown;
}

public struct RayRange
{
    public RayRange(float x1, float y1, float x2, float y2, Vector2 dir)
    {
        Start = new Vector2(x1, y1);
        End = new Vector2(x2, y2);
        Dir = dir;
    }

    public readonly Vector2 Start, End, Dir;
}

public class PlayerController : MonoBehaviour
{
    public Vector3 Velocity { get; private set; }
    public FrameInput Input { get; private set; }
    public Vector3 RawMovement { get; private set; }
    public bool JumpingThisFrame { get; private set; }
    public bool LandingThisFrame { get; private set; }
    //public bool Grounded => m_colDown;

    private Vector3 m_lastPosition;
    private float m_currentHorSpeed, m_currentVertSpeed = 0.0f;

    private BoxCollider col;

    void Start()
    {
        col = GetComponent<BoxCollider>();
    }

    void Update()
    {
        Velocity = (transform.position - m_lastPosition) / Time.deltaTime;

        GatherInput();

        CalculateWalk();
        CalculateGravity();
        CalculateJump();

        MoveCharacter();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.tag);
    }

    private void GatherInput()
    {
        Input = new FrameInput
        {
            JumpUp = UnityEngine.Input.GetButtonUp("Jump"),
            JumpDown = UnityEngine.Input.GetButtonDown("Jump"),
            X = UnityEngine.Input.GetAxisRaw("Horizontal")
        };

        if (Input.JumpDown)
        {
            //m_lastJumpPressed
            m_lastJumpPressed = Time.time;
        }
    }

    [Header("WALK")] [SerializeField] private float m_acceleration = 90.0f;
    [SerializeField] private float m_deAcceleration = 60.0f;
    [SerializeField] private float m_maxHorVel = 13.0f;

    private void CalculateWalk()
    {
        if (Input.X != 0)
        {
            m_currentHorSpeed += Input.X * m_acceleration * Time.deltaTime;
            m_currentHorSpeed = Mathf.Clamp(m_currentHorSpeed, -m_maxHorVel, m_maxHorVel);
        }
        else
        {
            m_currentHorSpeed = Mathf.MoveTowards(m_currentHorSpeed, 0.0f, m_deAcceleration * Time.deltaTime);
        }
    }

    [Header("JUMPING")] [SerializeField] private float m_jumpHeight = 30.0f;
    private float m_lastJumpPressed;
    private void CalculateJump()
    {
        if (Input.JumpDown)
        {
            m_currentVertSpeed = m_jumpHeight;
            JumpingThisFrame = true;
        }
        else
        {
            JumpingThisFrame = false;
        }
    }

    [Header("GRAVITY")] [SerializeField] private float m_fallClamp = -40.0f;
    [SerializeField] private float m_fallSpeed = 10.0f;
    private void CalculateGravity()
    {
        var fallSpeed = m_fallSpeed;
        m_currentVertSpeed -= fallSpeed * Time.deltaTime;
        /*if(m_colDown)
        {
            if (m_currentVertSpeed < 0) m_currentVertSpeed = 0;
        }
        else
        {
            var fallSpeed = m_fallSpeed;
            m_currentVertSpeed -= fallSpeed * Time.deltaTime;
        }*/
    }

    private void MoveCharacter()
    {
        RawMovement = new Vector3(m_currentHorSpeed, m_currentVertSpeed);
        var move = RawMovement * Time.deltaTime;

        transform.position += move;
    }

    [Header("COLLISION")] [SerializeField] private Bounds m_characterBounds;
    [SerializeField] private LayerMask m_groundLayer;
    [SerializeField] private int m_detectorCount = 3;
    [SerializeField] private float m_detectionRayLength = 0.1f;
    [SerializeField] [Range(0.1f, 0.3f)] private float m_rayBuffer = 0.1f;

    private RayRange _raysUp, _raysDown, _raysRight, _raysLeft;
    private bool _colUp, _colDown, _colRight, _colLeft;

    private float _timeLeftGrounded;

    private void RunCollisionChecks()
    {
        CalculateRayRanged();

        LandingThisFrame = false;
        var groundCheck = RunDetection(_raysDown);
        if (_colDown && !groundCheck) _timeLeftGrounded = Time.time;
        else if (!_colDown && groundCheck)
        {
            LandingThisFrame = true;
        }

        _colDown = groundCheck;

        _colUp = RunDetection(_raysUp);
        _colLeft = RunDetection(_raysLeft);
        _colRight = RunDetection(_raysRight);

        bool RunDetection(RayRange range)
        {
            return EvaluateRayPositions(range).Any(point => Physics2D.Raycast(point, range.Dir, m_detectionRayLength, m_groundLayer));
        }
    }

    private void CalculateRayRanged()
    {
        var b = new Bounds(transform.position + m_characterBounds.center, m_characterBounds.size);

        _raysUp = new RayRange(b.min.x + m_rayBuffer, b.max.y, b.max.x - m_rayBuffer, b.max.y, Vector2.up);
        _raysDown = new RayRange(b.min.x + m_rayBuffer, b.min.y, b.max.x - m_rayBuffer, b.min.y, Vector2.down);
        _raysLeft = new RayRange(b.min.x, b.min.y + m_rayBuffer, b.min.x, b.max.y - m_rayBuffer, Vector2.left);
        _raysRight = new RayRange(b.max.x, b.min.y + m_rayBuffer, b.max.x, b.max.y - m_rayBuffer, Vector2.right);
    }

    private IEnumerable<Vector2> EvaluateRayPositions(RayRange range)
    {
        for (var i = 0; i < m_detectorCount; i++)
        {
            var t = (float)i / (m_detectorCount - 1);
            yield return Vector2.Lerp(range.Start, range.End, t);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + m_characterBounds.center, m_characterBounds.size);

        if (!Application.isPlaying)
        {
            CalculateRayRanged();
            Gizmos.color = Color.blue;
            foreach (var range in new List<RayRange> { _raysUp, _raysRight, _raysDown, _raysLeft })
            {
                foreach (var point in EvaluateRayPositions(range))
                {
                    Gizmos.DrawRay(point, range.Dir * m_detectionRayLength);
                }
            }
        }

        if (!Application.isPlaying) return;

        Gizmos.color = Color.red;
        var move = new Vector3(m_currentHorSpeed, m_currentVertSpeed) * Time.deltaTime;
        Gizmos.DrawWireCube(transform.position + m_characterBounds.center + move, m_characterBounds.size);
    }

}
