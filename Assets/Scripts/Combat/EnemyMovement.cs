using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    private Transform player;

    private void Awake()
    {
        var rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.freezeRotation = true;
    }

    private void Start()
    {
        // ������� ������ ���� ��� (����� ����� ����� reference)
        player = GameObject.FindWithTag("Player")?.transform;
        if (player == null) Debug.LogError("[EnemyMovement] ����� �� ������!");
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0; // �������� �� �����

        GetComponent<Rigidbody>().linearVelocity = direction * moveSpeed;
    }
}