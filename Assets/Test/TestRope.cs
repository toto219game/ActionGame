using UnityEngine;

public class PendulumMovement : MonoBehaviour
{
    public float length = 5f; // 振り子の長さ
    public float gravity = 9.81f; // 重力加速度
    public float damping = 0.1f; // 摩擦などによる減衰

    private float angle; // 振り子の角度
    private float velocity; // 振り子の角速度

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        angle = Mathf.PI / 2; // 初期角度
        velocity = 0f;
    }

    void Update()
    {
        // 振り子の運動計算
        float angularAcceleration = -gravity / length * Mathf.Sin(angle) - damping * velocity;
        velocity += angularAcceleration * Time.deltaTime;
        angle += velocity * Time.deltaTime;

        // 振り子の位置を計算
        float xPos = length * Mathf.Sin(angle);
        float yPos = -length * Mathf.Cos(angle);

        // CharacterControllerを使ってプレイヤーを移動
        controller.Move(new Vector3(xPos, yPos, 0) - transform.position);
    }
}