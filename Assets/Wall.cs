using UnityEngine;

public class Wall : MonoBehaviour
{
    [SerializeField] private Animator _animator;

    private float _currentState = 5;

    private const string ANIM_NAME = "wall_combined";

    public float CurrentState { get { return _currentState; } }

    // Start is called before the first frame update
    void Start()
    {
        _animator.speed = 0;
        _currentState = 5;
        UpdateWall(_currentState);
    }

    public void UpdateWall(float value)
    {
        _currentState = value;
        _animator.Play(ANIM_NAME, 0, _currentState / 9);
    }
}
