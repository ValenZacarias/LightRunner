using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct FrameInput
{
    public float X;
    public float Y;
    public bool JumpUp;
    public bool JumpDown;
    public bool DashUp;
    public bool DashDown;
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
    public bool JumpingThisFrame { get; private set; } // Only Animation purpouse 
    public bool LandingThisFrame { get; private set; } // Only Animation purpouse
    public Vector3 RawMovement { get; private set; }
    public bool Grounded => _colDown;

    private Vector3 _lastPosition;
    private float _currentHorizontalSpeed, _currentVerticalSpeed;

    // This is horrible, but for some reason colliders are not fully established when update starts...
    private bool _active;
    void Awake() => Invoke(nameof(Activate), 0.5f);
    void Activate() => _active = true;

    private void Start()
    {
        //spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        playerInput.OnJumpAction += PlayerInput_OnJumpAction;
        playerInput.OnDashAction += PlayerInput_OnDashAction;
    }

    private void Update()
    {
        if (!_active) return;
        // Calculate velocity
        Velocity = (transform.position - _lastPosition) / Time.deltaTime;
        _lastPosition = transform.position;

        if(useNewInputSystem) GatherInputNew();
        else GatherInput();
        
        
        RunCollisionChecks();

        CalculateWalk(); // Horizontal movement
        CalculateJumpApex(); // Affects fall speed, so calculate before gravity
        CalculateGravity(); // Vertical movement
        CalculateJump(); // Possibly overrides vertical
        CalculateDash();

        MoveCharacter(); // Actually perform the axis movement
        UpdateAnimator(); // Set animation parameters
    }


    #region Gather Input
    [Header("INPUT")] [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Vector2 inputVector;
    [SerializeField] private bool useNewInputSystem = false;
    private void GatherInput()
    {
        //inputVector = playerInput.GetMovementVectorNormalized();

        Input = new FrameInput
        {
            X = UnityEngine.Input.GetAxisRaw("Horizontal"),
            Y = UnityEngine.Input.GetAxisRaw("Vertical"),

            JumpDown = UnityEngine.Input.GetKeyDown(KeyCode.Z),
            JumpUp = UnityEngine.Input.GetKeyUp(KeyCode.Z),

            //DashDown = UnityEngine.Input.GetKeyDown(KeyCode.X),
            //DashUp = UnityEngine.Input.GetKeyUp(KeyCode.X)
        };
        
        if (Input.JumpDown)
        {
            _lastJumpPressed = Time.time;
        }

        if (UnityEngine.Input.GetKeyDown(KeyCode.X))
        {
            startDash = true;
        }
        if (UnityEngine.Input.GetKeyUp(KeyCode.X))
        {
            startDash = false;
        }

    }

    // NEW INPUT SYSTEM
    private void PlayerInput_OnJumpAction(object sender, System.EventArgs e)
    {
        //Debug.Log("PLAYER INPUT -> JUMP");
        if(useNewInputSystem) _lastJumpPressed = Time.time;
    }

    private void PlayerInput_OnDashAction(object sender, System.EventArgs e)
    {
        //Debug.Log("PLAYER INPUT -> DASH");
        if (useNewInputSystem) startDash = true;
    }

    private void GatherInputNew()
    {
        inputVector = playerInput.GetMovementVectorNormalized();

        Input = new FrameInput
        {
            X = inputVector.x,
            Y = inputVector.y,

            JumpDown = playerInput.JumpInputDown,
            JumpUp = playerInput.JumpInputUp,
        };
    }

    #endregion

    #region Collisions

    [Header("COLLISION")] [SerializeField] private Bounds _characterBounds;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private int _detectorCount = 3;
    [SerializeField] private float _detectionRayLength = 0.1f;
    [SerializeField] [Range(-1.0f, 1.0f)] private float _rayBuffer = 0.1f; // Prevents side detectors hitting the ground

    [SerializeField] private LayerMask _damageLayer;

    private RayRange _raysUp, _raysRight, _raysDown, _raysLeft;
    private bool _colUp, _colRight, _colDown, _colLeft;

    private float _timeLeftGrounded;

    // We use these raycast checks for pre-collision information
    private void RunCollisionChecks()
    {
        // Generate ray ranges. 
        CalculateRayRanged();

        // Ground
        LandingThisFrame = false;
        var groundedCheck = RunDetection(_raysDown);
        if (_colDown && !groundedCheck)
        {
            _timeLeftGrounded = Time.time; // Only trigger when first leaving

        }
        else if (!_colDown && groundedCheck)
        {
            _coyoteUsable = true; // Only trigger when first touching
            LandingThisFrame = true;
        }

        _colDown = groundedCheck;

        // The rest
        _colUp = RunDetection(_raysUp);
        _colLeft = RunDetection(_raysLeft);
        _colRight = RunDetection(_raysRight);

        bool RunDetection(RayRange range)
        {
            return EvaluateRayPositions(range).Any(point => Physics2D.Raycast(point, range.Dir, _detectionRayLength, _groundLayer));
        }
    }

    private void CalculateRayRanged()
    {
        // This is crying out for some kind of refactor. 
        var b = new Bounds(transform.position + _characterBounds.center, _characterBounds.size);

        _raysDown = new RayRange(b.min.x + _rayBuffer, b.min.y, b.max.x - _rayBuffer, b.min.y, Vector2.down);
        _raysUp = new RayRange(b.min.x + _rayBuffer, b.max.y, b.max.x - _rayBuffer, b.max.y, Vector2.up);
        _raysLeft = new RayRange(b.min.x, b.min.y + _rayBuffer, b.min.x, b.max.y - _rayBuffer, Vector2.left);
        _raysRight = new RayRange(b.max.x, b.min.y + _rayBuffer, b.max.x, b.max.y - _rayBuffer, Vector2.right);
    }


    private IEnumerable<Vector2> EvaluateRayPositions(RayRange range)
    {
        for (var i = 0; i < _detectorCount; i++)
        {
            var t = (float)i / (_detectorCount - 1);
            yield return Vector2.Lerp(range.Start, range.End, t);
        }
    }

    public event EventHandler OnDamageAction;

    private void OnTriggerEnter2D(Collider2D col)
    {
        //Debug.Log(col.gameObject);
        if (col.CompareTag("PickLight"))
        {
            LightPickup light = col.gameObject.GetComponent<LightPickup>();
            if (light.isOn)
            {
                light.TryTurnOff();
                _canDash = true;
                dashAnimTriggered = false;
            }
        }

        if (_damageLayer == (_damageLayer | (1 << col.gameObject.layer))) // si el objeto pertenece a la layermask de damage
        {
            Debug.Log("DEAD");
            OnDamageAction?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        Debug.Log(col);
        // para los damage enemy
        if (_damageLayer == (_damageLayer | (1 << col.gameObject.layer))) // si el objeto pertenece a la layermask de damage
        {
            Debug.Log("DEAD");
            OnDamageAction?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnDrawGizmos()
    {
        // Bounds
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position + _characterBounds.center, _characterBounds.size);

        // Rays
        if (Application.isPlaying)
        {
            CalculateRayRanged();
            Gizmos.color = Color.blue;
            foreach (var range in new List<RayRange> { _raysUp, _raysRight, _raysDown, _raysLeft })
            {
                foreach (var point in EvaluateRayPositions(range))
                {
                    Gizmos.DrawRay(point, range.Dir * _detectionRayLength);
                }
            }
        }

        if (!Application.isPlaying) return;

        // Draw the future position. Handy for visualizing gravity
        Gizmos.color = Color.red;
        var move = new Vector3(_currentHorizontalSpeed, _currentVerticalSpeed) * Time.deltaTime;
        Gizmos.DrawWireCube(transform.position + _characterBounds.center + move, _characterBounds.size);
    }

    #endregion

    #region Walk

    [Header("WALKING")] [SerializeField] private float _acceleration = 90;
    [SerializeField] private float _moveClamp = 13;
    [SerializeField] private float _deAcceleration = 60f;
    [SerializeField] private float _apexBonus = 2;

    private void CalculateWalk()
    {
        if (Input.X != 0)
        {
            // Set horizontal move speed
            _currentHorizontalSpeed += Input.X * _acceleration * Time.deltaTime;

            // clamped by max frame movement
            _currentHorizontalSpeed = Mathf.Clamp(_currentHorizontalSpeed, -_moveClamp, _moveClamp);

            // Apply bonus at the apex of a jump
            var apexBonus = Mathf.Sign(Input.X) * _apexBonus * _apexPoint;
            _currentHorizontalSpeed += apexBonus * Time.deltaTime;
        }
        else
        {
            // No input. Let's slow the character down
            _currentHorizontalSpeed = Mathf.MoveTowards(_currentHorizontalSpeed, 0, _deAcceleration * Time.deltaTime);
        }

        if (_currentHorizontalSpeed > 0 && _colRight || _currentHorizontalSpeed < 0 && _colLeft)
        {
            // Don't walk through walls
            _currentHorizontalSpeed = 0;
        }
    }

    #endregion

    #region Gravity

    [Header("GRAVITY")] [SerializeField] private float _fallClamp = -40f;
    [SerializeField] private float _minFallSpeed = 80f;
    [SerializeField] private float _maxFallSpeed = 120f;
    private float _fallSpeed;

    private void CalculateGravity()
    {
        if (_colDown)
        {
            // Move out of the ground
            if (_currentVerticalSpeed < 0) _currentVerticalSpeed = 0;
        }
        else
        {
            // Add downward force while ascending if we ended the jump early
            var fallSpeed = _endedJumpEarly && _currentVerticalSpeed > 0 ? _fallSpeed * _jumpEndEarlyGravityModifier : _fallSpeed;

            // Fall
            _currentVerticalSpeed -= fallSpeed * Time.deltaTime;

            // Clamp
            if (_currentVerticalSpeed < _fallClamp) _currentVerticalSpeed = _fallClamp;
        }
    }

    #endregion

    #region Jump

    [Header("JUMPING")] [SerializeField] private float _jumpHeight = 30;
    [SerializeField] private float _jumpApexThreshold = 10f;
    [SerializeField] private float _coyoteTimeThreshold = 0.1f;
    [SerializeField] private float _jumpBuffer = 0.1f;
    [SerializeField] private float _jumpEndEarlyGravityModifier = 3;
    private bool _coyoteUsable;
    private bool _endedJumpEarly = true;
    private float _apexPoint; // Becomes 1 at the apex of a jump
    private float _lastJumpPressed;
    private bool CanUseCoyote => _coyoteUsable && !_colDown && _timeLeftGrounded + _coyoteTimeThreshold > Time.time;
    private bool HasBufferedJump => _colDown && _lastJumpPressed + _jumpBuffer > Time.time;

    private void CalculateJumpApex()
    {
        if (!_colDown)
        {
            // Gets stronger the closer to the top of the jump
            _apexPoint = Mathf.InverseLerp(_jumpApexThreshold, 0, Mathf.Abs(Velocity.y));
            _fallSpeed = Mathf.Lerp(_minFallSpeed, _maxFallSpeed, _apexPoint);
        }
        else
        {
            _apexPoint = 0;
        }
    }

    private void CalculateJump()
    {
        // Jump if: grounded or within coyote threshold || sufficient jump buffer
        if (Input.JumpDown && CanUseCoyote || HasBufferedJump)
        {
            _currentVerticalSpeed = _jumpHeight;
            _endedJumpEarly = false;
            _coyoteUsable = false;
            _timeLeftGrounded = float.MinValue;
            JumpingThisFrame = true;
        }
        else
        {
            JumpingThisFrame = false;
        }

        // End the jump early if button released
        if (!_colDown && Input.JumpUp && !_endedJumpEarly && Velocity.y > 0)
        {
            // _currentVerticalSpeed = 0;
            _endedJumpEarly = true;
        }

        if (_colUp)
        {
            if (_currentVerticalSpeed > 0) _currentVerticalSpeed = 0;
        }
    }

    #endregion

    #region Dash
    [Header("DASH")] [SerializeField] private float _dashSpeed = 1.0f;
    [SerializeField] private bool _canDash = false;
    [SerializeField] private AnimationCurve dashCurve;
    [SerializeField] private float dashTotalTime = 0.5f;
    private Vector2 dir = new Vector2(0.0f, 0.0f);
    private float dashTime = 0.0f;
    private bool startDash;
    public bool isDashing;
    public GameObject DashIndicator;

    private bool tryDash;
    [SerializeField] private float tryDashWindow = 0.3f;
    // [SerializeField] private float dashCooldown = 1.0f;
    private void CalculateDash()
    {
        DashIndicator.SetActive(_canDash || isDashing);

        if (_canDash && startDash && dashTime <= 0.0f)
        {
            dir = new Vector2(Input.X, Input.Y).normalized;
            if (dir.magnitude == 0.0f) { return; }
            //Debug.Log("DASH START - vec:" + dir + " - mag:" + dir.magnitude);
            
            // hacer que dashee a algun lado cuando no se esta moviendo. Si estamos en el aire que vaya para arriba, si estamos en el suelo, para donde este mirando el vampi.
            // para lo del suelo hay que tener un vector de facing aca en el controller
            //if (dir.magnitude == 0.0f) dir = JumpingThisFrame ? new Vector2(0.0f, 1.0f) : new Vector2(1.0f, 0.0f); 
            
            _currentHorizontalSpeed = dir.normalized.x * _dashSpeed * dashCurve.Evaluate( 1 - dashTime / dashTotalTime) ;
            _currentVerticalSpeed = dir.normalized.y * _dashSpeed * dashCurve.Evaluate(1 - dashTime / dashTotalTime);
            dashTime = dashTotalTime;
            startDash = false;
            _canDash = false;
            isDashing = true;
            DashIndicator.SetActive(false);
        }
        else if(dashTime > 0.0f)
        {
            //Debug.Log("DASHING - vec:" + dir + " - mag:" + dir.magnitude);
            dir = new Vector2(Input.X, Input.Y).normalized;
            _currentHorizontalSpeed = dir.normalized.x * _dashSpeed * dashCurve.Evaluate(1 - dashTime / dashTotalTime);
            _currentVerticalSpeed = dir.normalized.y * _dashSpeed * dashCurve.Evaluate(1 - dashTime / dashTotalTime);
            dashTime -= Time.deltaTime;
        }
        else if(dashTime <= 0.0f && isDashing)
        {
            dashTime = 0.0f;
            isDashing = false;
        }

    }

    public void ResetDash()
    {
        
        _canDash = false;
    }

    #endregion

    #region Move

    [Header("MOVE")]
    [SerializeField, Tooltip("Raising this value increases collision accuracy at the cost of performance.")]
    private int _freeColliderIterations = 10;
    [SerializeField] private float _CharSizeScale = 1.0f;
    // We cast our bounds before moving to avoid future collisions
    private void MoveCharacter()
    {
        var pos = transform.position + _characterBounds.center;
        
        RawMovement = new Vector3(_currentHorizontalSpeed, _currentVerticalSpeed); // Used externally


        var move = RawMovement * Time.deltaTime;

        var furthestPoint = pos + move;

        // check furthest movement. If nothing hit, move and don't do extra checks
        var hit = Physics2D.OverlapBox(furthestPoint, _characterBounds.size, 0, _groundLayer);
        if (!hit)
        {
            transform.position += move;
            return;
        }

        // otherwise increment away from current pos; see what closest position we can move to
        var positionToMoveTo = transform.position;
        for (int i = 1; i < _freeColliderIterations; i++)
        {
            // increment to check all but furthestPoint - we did that already
            var t = (float)i / _freeColliderIterations;
            var posToTry = Vector2.Lerp(pos, furthestPoint, t);

            if (Physics2D.OverlapBox(posToTry, _characterBounds.size * _CharSizeScale, 0, _groundLayer))
            {
                transform.position = positionToMoveTo;
                dashTime *= 0.0f;
                // We've landed on a corner or hit our head on a ledge. Nudge the player gently
                if (i == 1)
                {
                    if (_currentVerticalSpeed < 0) _currentVerticalSpeed = 0;
                    var dir = transform.position - hit.transform.position;
                    transform.position += dir.normalized * move.magnitude;
                }

                return;
            }

            positionToMoveTo = posToTry;
        }
    }

    #endregion


    #region Animation

    [Header("ANIMATION")]
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer spriteDash;
    public Color dashColor;
    private bool dashAnimTriggered = false;
    private void UpdateAnimator()
    {

        animator.SetFloat("Speed_x", Mathf.Abs(_currentHorizontalSpeed));
        
        // Lo mejor aca seria trabajarlo con un Speed_y y hacer las comparaciones directamente en las transiciones del animator
        if(_currentVerticalSpeed > 0){
            animator.SetBool("Jumping", true);
            animator.SetBool("Falling", false);
        } 
        else if(_currentVerticalSpeed < 0){
            animator.SetBool("Jumping", false);
            animator.SetBool("Falling", true);
        }
        else
        {
            animator.SetBool("Jumping", false);
            animator.SetBool("Falling", false);
        }

        if(_colDown)
        {
            animator.SetBool("IsGrounded", true);
        }
        else
        {
            animator.SetBool("IsGrounded", false);
        }

        // spriteRenderer.color = _canDash ? dashColor : Color.white;
        spriteDash.enabled = _canDash;

        if(isDashing && !dashAnimTriggered){
            animator.SetTrigger("Dash");
            dashAnimTriggered = true;
        }

    }
    #endregion

    
}
