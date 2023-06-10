using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerAnimator : MonoBehaviour {
    [SerializeField] private Animator _anim;    
    [SerializeField] private ParticleSystem _trailParticles;       

    private PlayerController _player;
    private SpriteRenderer _spriteRenderer;

    void Awake()
    {
        _player = GetComponentInParent<PlayerController>();
        _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update() {

        if(_player.Input.X != 0)
        {
            _spriteRenderer.flipX = _player.Input.X < 0 ? true : false;
        }

        if(_player.isDashing){
            _trailParticles.Play();
        }

        //if (_player == null) return;

        // Flip the sprite
        //if (_player.Input.X != 0) transform.localScale = new Vector3(_player.Input.X > 0 ? 1 : -1, 1, 1); 

        // Splat
        if (_player.LandingThisFrame) {
            //_anim.SetTrigger(GroundedKey);
            // _source.PlayOneShot(_footsteps[Random.Range(0, _footsteps.Length)]);
        }

    }
    
    #region Animation Keys

    //private static readonly int GroundedKey = Animator.StringToHash("Grounded");
    // private static readonly int IdleSpeedKey = Animator.StringToHash("IdleSpeed");
    // private static readonly int JumpKey = Animator.StringToHash("Jump");

    #endregion

}
