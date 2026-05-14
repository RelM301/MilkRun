using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    #region Variables
    [SerializeField] private Transform target;
    [SerializeField] private float speed = 3f;
    [SerializeField] private float chaseDistance = 50f;
    private Animator _animator;
    private bool _isChasing = false;
    #endregion

    #region Unity Callbacks
    void Start()
    {
        _animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!target) return;
        float distance = Vector3.Distance(transform.position, target.position);
        bool shouldChase = distance < chaseDistance;

        if (shouldChase && !_isChasing)
        {
            StartChasing();
        }
        else if (!shouldChase && _isChasing)
        {
            StopChasing();
        }

        // While chasing, report our distance to MusicManager every frame
        if (_isChasing && MusicManager.Instance != null)
            MusicManager.Instance.ReportChaserDistance(gameObject, distance);

        if (_isChasing) MoveAndRotate();
    }
    #endregion

    #region Private Methods
    private void StartChasing()
    {
        _isChasing = true;
        if (_animator) _animator.SetBool("IsChasing", true);
        if (MusicManager.Instance != null)
            MusicManager.Instance.RegisterChaser(gameObject);
    }

    private void StopChasing()
    {
        _isChasing = false;
        if (_animator) _animator.SetBool("IsChasing", false);
        if (MusicManager.Instance != null)
            MusicManager.Instance.UnregisterChaser(gameObject);
    }

    private void OnDisable()
    {
        if (_isChasing && MusicManager.Instance != null)
            MusicManager.Instance.UnregisterChaser(gameObject);
    }
    #endregion

    #region Public Methods
    void MoveAndRotate()
    {
        Vector3 direction = (target.position - transform.position);
        direction.y = 0;
        if (direction.sqrMagnitude < 0.01f) return;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
        transform.position += direction.normalized * speed * Time.deltaTime;
    }
    #endregion
}
