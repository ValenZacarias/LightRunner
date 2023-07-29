using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerAnimator : MonoBehaviour {
    [SerializeField] private Animator _anim;    
    [SerializeField] private GameObject _sprite;    
    [SerializeField] private ParticleSystem _trailParticles;       

    private PlayerController _player;
    private Vector3 initialSpriteScale;
    private int scaleMultiplier;
    // private SpriteRenderer _spriteRenderer;
    // private SpriteRenderer _spriteDashRenderer;

    void Awake()
    {
        _player = GetComponentInParent<PlayerController>();
        // _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        // _spriteDashRenderer = _spriteRenderer.GetComponentInChildren<SpriteRenderer>();
        initialSpriteScale = _sprite.transform.localScale;
    }

    void Update() {

        if(_player.Input.X != 0)
        {
            // _spriteRenderer.flipX = _player.Input.X < 0 ? true : false;
            // _spriteDashRenderer.flipX = _player.Input.X < 0 ? true : false;
            // _spriteRenderer.transform.localScale = new Vector3(_player.Input.X > 0 ? 1 : -1, 1, 1);
            scaleMultiplier = _player.Input.X > 0 ? 1 : -1;
            _sprite.transform.localScale = new Vector3(initialSpriteScale.x * scaleMultiplier, initialSpriteScale.y, initialSpriteScale.z);
        }

        if(_player.isDashing){
            _trailParticles.Play();
        }

        //if (_player == null) return;

        // Flip the sprite
        // if (_player.Input.X != 0) transform.localScale = new Vector3(_player.Input.X > 0 ? 1 : -1, 1, 1); 

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
