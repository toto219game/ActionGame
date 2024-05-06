using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
//CharacterControllerを用いたもの
public class PlayerController_backup : MonoBehaviour
{

    private Camera playerCamera;
    public float cameraRotation;

    private const float rayOffset = 1f;
    private const float rayLength = 0.5f;


    public CharacterController character;

    [SerializeField] public float groundSpeed = 10f;
    [SerializeField] private float floatingSpeed = 35f;
    [SerializeField] private const float playerMaxSpeed = 15f;


    private bool isGround = false;
    public float groundHeight;

    //ジャンプなどに必要なモノ
    private float gravity = 40f;
    private float velocity = 0f;
    private float jumpDeltaTime;


    public Vector3 transition;//プロパティにした方がいい？


    //ギズモのためのもの
    bool isHit;
    RaycastHit hitobj;


    //smooth
    private Vector3 beforeKey;
    private Vector3 currentKey;
    private Vector3 stackKey;
    private float moveDeltaTime;
    private float smoothTime = 0.2f;
    private Vector3 beforeTransition;

    //rotate
    public float rotateSpeed = 5f;

    enum PlayerBaseState
    {
        ground,
        floating
    }

    PlayerBaseState currentState;
    bool stateEnter = false;

    //関数部
    private void ChangeState(PlayerBaseState newState)
    {
        stateEnter = true;
        currentState = newState;
    }

    private Vector3 KeyInput()
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

    public Vector3 smoothInput()
    {
        currentKey = KeyInput();
        if (currentKey != beforeKey)
        {
            stackKey = beforeTransition;
            moveDeltaTime = 0f;
        }

        beforeKey = KeyInput();

        if (moveDeltaTime < smoothTime)
        {
            float smoothValue;

            moveDeltaTime += Time.deltaTime;
            smoothValue = moveDeltaTime / smoothTime;
            beforeTransition = Vector3.Lerp(stackKey, currentKey, Mathf.Clamp(smoothValue,0.4f,1f));
            return beforeTransition;
        }
        else
        {
            return currentKey;
        }
    }
    
    private bool IsGround()
    {
        Vector3 center = transform.position + Vector3.up * rayOffset;
        
        RaycastHit hit;
        LayerMask mask = 1 << LayerMask.NameToLayer("Ground");

        isHit = Physics.BoxCast(center, Vector3.one * 0.5f, Vector3.down, transform.rotation, rayLength, mask);
        if (Physics.BoxCast(center,Vector3.one * 0.5f,Vector3.down,out hit,transform.rotation,rayLength,mask)){
            hitobj = hit;
            groundHeight = hit.point.y;
            return true;
        }
        return false;
        
    }

    private void StateManeger()
    {
        switch (currentState)
        {
            case PlayerBaseState.ground:
                //enter処理=========================================
                if (stateEnter)
                {
                    stateEnter = false;

                    velocity = 0;
                }


                //update処理=========================================
                //Debug.Log("ground");

                GroundState();

                //exit処理=========================================
                if (Input.GetKey(KeyCode.Space) && isGround)
                {
                    ChangeState(PlayerBaseState.floating);
                    velocity = 15f;

                    return;
                }
                if (!isGround)
                { 
                    ChangeState(PlayerBaseState.floating);
                    return;
                }

                return;

            case PlayerBaseState.floating:
                //enter処理=========================================
                if (stateEnter)
                {
                    stateEnter = false;

                    jumpDeltaTime = 0f;
                    isGround = false;
                }

                //update処理=========================================
                //Debug.Log("floating");
                FloatingState();

                jumpDeltaTime += Time.deltaTime;

                if (jumpDeltaTime > 0.1f)
                {
                    velocity -= gravity * Time.deltaTime;
                }


                if (jumpDeltaTime < 0.2f) return;

                //exit処理=========================================
                if (isGround)
                {
                    ChangeState(PlayerBaseState.ground);
                }

                return;
        }
    }

    //groundステートの時の移動
    private void GroundState()
    {

        Quaternion currentRotation = transform.rotation;
        Quaternion nextRotation;

        transition = smoothInput();
        transition = Quaternion.Euler(0, cameraRotation, 0) * transition;        
        transition.y = 0f;

        transition *= groundSpeed;

        //移動処理
        character.Move(transition * Time.deltaTime);
        //高さを地面に合わせる
        transform.position = new Vector3(transform.position.x, groundHeight, transform.position.z);

        if (transition == Vector3.zero) return;

        //今回のtranstionの角度抽出
        nextRotation = Quaternion.LookRotation(transition.normalized);
        //回る角度が150度以上だったらそのままそれを代入
        if (Mathf.Abs(currentRotation.eulerAngles.y - nextRotation.eulerAngles.y) > 150)
        {
            transform.rotation = nextRotation;
            return;
        }
        //なめらかに回転
        transform.rotation = Quaternion.RotateTowards(currentRotation, nextRotation, rotateSpeed);
    }

    //floatingステートの時の移動
    private void FloatingState()
    {
        //transitionのy座標初期化
        transition.y = 0f;

        Vector3 floatingVelocity = Quaternion.Euler(0, cameraRotation, 0) * KeyInput() 
            * floatingSpeed * Time.deltaTime;

        transition += floatingVelocity;
        

        if(transition.magnitude > playerMaxSpeed)
        {
            transition *= playerMaxSpeed / transition.magnitude;
        }
        //上下の移動(ジャンプと落下)
        transition.y = velocity;
        

        character.Move(transition * Time.deltaTime);
    }

    private void Start()
    {
        playerCamera = Camera.main;
        character = GetComponent<CharacterController>();

        if (IsGround())
        {
            currentState = PlayerBaseState.ground;
            Debug.Log("start state : groundState");
        }
        Debug.Log("start state : floatingState");
    }

    private void Update()
    {
        isGround = IsGround();
        cameraRotation = playerCamera.transform.localEulerAngles.y;

        StateManeger();

    }
    /*
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