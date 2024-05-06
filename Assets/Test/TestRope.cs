using UnityEngine;

public class PendulumMovement : MonoBehaviour
{
    public float length = 5f; // �U��q�̒���
    public float gravity = 9.81f; // �d�͉����x
    public float damping = 0.1f; // ���C�Ȃǂɂ�錸��

    private float angle; // �U��q�̊p�x
    private float velocity; // �U��q�̊p���x

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        angle = Mathf.PI / 2; // �����p�x
        velocity = 0f;
    }

    void Update()
    {
        // �U��q�̉^���v�Z
        float angularAcceleration = -gravity / length * Mathf.Sin(angle) - damping * velocity;
        velocity += angularAcceleration * Time.deltaTime;
        angle += velocity * Time.deltaTime;

        // �U��q�̈ʒu���v�Z
        float xPos = length * Mathf.Sin(angle);
        float yPos = -length * Mathf.Cos(angle);

        // CharacterController���g���ăv���C���[���ړ�
        controller.Move(new Vector3(xPos, yPos, 0) - transform.position);
    }
}