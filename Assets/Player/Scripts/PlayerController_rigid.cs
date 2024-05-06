/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//CharacterControllerを用いたもの
[System.Serializable]
public class PlayerController_rigid : MonoBehaviour
{

    private Camera playerCamera;
    public float cameraRotation;

    //接地判定など
    public bool isGround { get; private set; }
    [System.NonSerialized] public float groundHeight;
    private const float rayOffset = 1f;
    private const float rayLength = 0.5f;


    [SerializeField] public float groundSpeed = 10f;
    [SerializeField] public float floatingSpeed = 35f;
    [SerializeField] public float playerMaxSpeed { get; private set; } = 15f;
    //rotate
    public float rotateSpeed = 5f;

    //ジャンプなどに必要なモノ
    public float gravity = 40f;
    public float JumpVelocity { get; set; } = 0f;

    [System.NonSerialized] public Vector3 moveVector;//プロパティにした方がいい？各要素にアクセスできなくなる

    [SerializeField] public GroundMove groundMove = new GroundMove();
    [System.NonSerialized] public Rigidbody rb;

    //ステートマシンの定義
    StateMachine<PlayerController_rigid> stateMachine;


    //グラップテスト
    [Header("てすとだよ")]
    public Transform testTarget;

    //ギズモのためのもの
    bool isHit;
    RaycastHit hitobj;

    public enum EventID
    {
        ground,
        jump,
        floating,
        grapling,
        grapOff
    }
    ;
    //テスト

    public Vector3 KeyInput()
    {

        float inputX = 0;
        float inputZ = 0;

        if (Input.GetKey(KeyCode.W))
        {
            inputZ = 1;
        }
        if (Input.GetKey(KeyCode.S))
        {
            inputZ = -1;
        }
        if (Input.GetKey(KeyCode.D))
        {
            inputX = 1;
        }
        if (Input.GetKey(KeyCode.A))
        {
            inputX = -1;
        }

        return new Vector3(inputX, 0, inputZ).normalized;
    }

    private bool IsGround()
    {
        Vector3 center = transform.position + Vector3.up * rayOffset;

        RaycastHit hit;
        LayerMask mask = 1 << LayerMask.NameToLayer("Ground");

        //isHit = Physics.BoxCast(center, Vector3.one * 0.5f, Vector3.down, transform.rotation, rayLength, mask);
        if (Physics.BoxCast(center, Vector3.one * 0.5f, Vector3.down, out hit, transform.rotation, rayLength, mask))
        {
            //hitobj = hit;
            groundHeight = hit.point.y;
            return true;
        }
        return false;
    }



    private void Start()
    {
        //Debug.Log(Mathf.Atan(10 / 2 * Mathf.PI));

        playerCamera = Camera.main;
        rb = GetComponent<Rigidbody>();
        stateMachine = new StateMachine<PlayerController_rigid>(this);

        stateMachine.AddTransition<GroundState_rigid, JumpState_rigid>((int)EventID.jump);
        stateMachine.AddTransition<GroundState_rigid, FloatingState_rigid>((int)EventID.floating);
        stateMachine.AddTransition<JumpState_rigid, FloatingState_rigid>((int)EventID.floating);
        stateMachine.AddTransition<FloatingState_rigid, GroundState_rigid>((int)EventID.ground);
        stateMachine.AddTransition<FloatingState_rigid, GrapFookState_rigid>((int)EventID.grapling);
        stateMachine.AddTransition<GrapFookState_rigid, FloatingState_rigid>((int)EventID.grapOff);

        if (isGround = IsGround())
        {
            stateMachine.Initialize<GroundState_rigid>();
            Debug.Log("start state : groundState");
        }
        else Debug.Log("start state : floatingState");
        stateMachine.Initialize<FloatingState_rigid>();
    }

    private void Update()
    {
        isGround = IsGround();
        cameraRotation = playerCamera.transform.localEulerAngles.y;

        stateMachine.OnUpdate();

        if (Input.GetKeyDown(KeyCode.G))
        {
            stateMachine.Dispatch((int)EventID.grapling);
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            stateMachine.Dispatch((int)EventID.grapOff);
        }
    }

    private void OnDrawGizmos()
    {
        if (isHit)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position + Vector3.up * rayOffset + Vector3.down * hitobj.distance, Vector3.one);
        }
        else
        {
            Gizmos.DrawWireCube(transform.position + Vector3.up * rayOffset + Vector3.down * rayLength, Vector3.one);
        }


    }

}
*/