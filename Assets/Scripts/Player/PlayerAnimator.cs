using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerAnimator : MonoBehaviour {

    private PlayerController _player;

    void Awake() => _player = GetComponentInParent<PlayerController>();

    void Update() {
        if (_player == null) return;

        // Flip the sprite
        if (_player.Input.X != 0) transform.localScale = new Vector3(_player.Input.X > 0 ? 1 : -1, 1, 1);

    }

}
