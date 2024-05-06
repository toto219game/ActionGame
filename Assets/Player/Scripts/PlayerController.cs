using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class VectorSmooth
{
    private Vector3 beforeKey;
    private Vector3 currentKey;
    private Vector3 stackKey;
    private float moveDeltaTime;
    private float smoothTime = 0.2f;
    private Vector3 beforeTransition;

    public Vector3 SmoothInput(Vector3 input)
    {
        currentKey = input;
        if (currentKey != beforeKey)
        {
            stackKey = beforeTransition;
            moveDeltaTime = 0f;
        }

        beforeKey = input;

        if (moveDeltaTime < smoothTime)
        {
            float smoothValue;

            moveDeltaTime += Time.deltaTime;
            smoothValue = moveDeltaTime / smoothTime;
            beforeTransition = Vector3.Lerp(stackKey, currentKey, Mathf.Clamp(smoothValue, 0.4f, 1f));
            return beforeTransition;
        }
        else
        {
            return currentKey;
        }
    }
}

[System.Serializable]
public class GroundMove
{
    [SerializeField] private float groundSpeed = 10f;
    VectorSmooth smooth = new VectorSmooth();

    public Vector3 MoveVector(Vector3 input,float cameraRotationY)
    {
        Vector3 trans;
        trans = smooth.SmoothInput(input);
        trans = Quaternion.Euler(0f,cameraRotationY,0f) * trans;
        trans *= groundSpeed;
        trans.y = 0f;

        return trans;
    }
}

public class GroundRotation
{
    
    public static Quaternion Rotation(Vector3 trans,Quaternion current,float speed)
    {
        if (trans == Vector3.zero)
        {
            return current;
        }

        Quaternion next = Quaternion.LookRotation(trans);
        //回る角度が150度以上だったらそのままそれを代入
        if (Mathf.Abs(current.eulerAngles.y - next.eulerAngles.y) > 150)
        {
            return next;
        }
        //なめらかに回転
        return Quaternion.RotateTowards(current, next, speed);
    }
}

[System.Serializable]
public class PlayerCommand
{
    /*
     * キー入力に関するクラス
     * 基本的にここで入力を取得
     */

    //trueになるタイミング.GetKeyの後に続くのと一緒
    public enum Timing
    {
        keep,up,down
    }

    
    bool keyEnable; //このキー入力が使えるかどうか(キー入力をブロックしたい時に使える)
    [SerializeField] bool isMouse;   //マウスボタンでの入力かどうかの判定
    [SerializeField] KeyCode key;
    [SerializeField] int mouseButton;
    [SerializeField] Timing timing;

    public PlayerCommand(KeyCode k,Timing t,bool enable=true)
    {
        key = k;
        timing = t;
        isMouse = false;
        keyEnable = enable;
    }

    public  PlayerCommand(int num,Timing t, bool enable=true)
    {
        mouseButton = num;
        timing = t;
        isMouse = true;
        keyEnable = enable;
    }

    public void Enable()
    {
        keyEnable = true;
    }

    public void Disable()
    {
        keyEnable = false;
    }

    public bool CommandInput()
    {
        if (!keyEnable) return false;

        if (!isMouse)
        {
            switch (timing)
            {
                case Timing.down:
                    return Input.GetKeyDown(key);

                case Timing.up:
                    return Input.GetKeyUp(key);

                case Timing.keep:
                    return Input.GetKey(key);
            }
        }
        else
        {
            switch (timing)
            {
                case Timing.down:
                    return Input.GetMouseButtonDown(mouseButton);

                case Timing.up:
                    return Input.GetMouseButtonUp(mouseButton);

                case Timing.keep:
                    return Input.GetMouseButton(mouseButton);
            }
        }
        return false;
    }

}

//CharacterControllerを用いたもの
[System.Serializable]
public class PlayerController : MonoBehaviour
{

    /*
     * PlayerControllerの課題
     * 同じフレーム内でイベントが複数発行されると想定のステートへ遷移できない
     * 例　・同じボタンだが現在のステートによってステートの遷移を変えたい時など
     *      →グラップステートから同じボタンでフローティングに戻りたい時に戻れないなど
     *      
     * 解決策: イベントに優先順位をつける　→　めんどくさい、現在のステートによって優先順位が変わるのが大変
     *          その他)ステートの定義内でDispatchを済ませる　→　これもめんどくさい、いろんなステートから遷移可能の場合とてもだるい
     *          
     *          どうしようか
     */


    //カメラに関すること
    private Camera playerCamera;
    [System.NonSerialized]public float cameraRotation;

    //接地判定に関する
    public bool isGround { get; private set; }
    [System.NonSerialized] public float groundHeight;
    private const float rayOffset = 1f;
    private const float rayLength = 0.5f;

    //平面移動に関する
    [SerializeField] public float groundSpeed = 10f;
    [SerializeField] public float floatingSpeed = 35f;
    [SerializeField] public float playerMaxSpeed { get; private set; } = 15f;
    [SerializeField] public float playerMaxSpeedY { get; private set; } = 150f;
    //平面移動に関する
    public float rotateSpeed = 5f;

    //ジャンプに関する
    public float gravity = 40f;
    public float JumpVelocity { get; set; } = 0f;

    [System.NonSerialized]public Vector3 moveVector;//プロパティにした方がいい？各要素にアクセスできなくなる

    //便利クラス
    [SerializeField] public GroundMove groundMove = new GroundMove();

    //キャラクターコントローラーコンポーネント
    [System.NonSerialized] public CharacterController character;

    //ステートマシン
    StateMachine<PlayerController> stateMachine;
    [System.NonSerialized] public List<EventID> eventPriority = new List<EventID>();
    
    //コマンド(キーコン)
    [SerializeField] PlayerCommand grapCommand = new PlayerCommand(0, PlayerCommand.Timing.down,false);

    //グラップテスト
    public Vector3 grapTarget;

    [System.NonSerialized] public LineRenderer line;
    private float grapStartOffset = 2f;

    //ギズモのためのもの
    /* bool isHit;
     RaycastHit hitobj;*/

    //イベントのenum
    public enum EventID
    {
        ground,
        jump,
        floating,
        grapleOn,
        grapleOff
    }

    //キー入力(WASD)
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

    //接地判定
    private bool IsGround()
    {
        Vector3 center = transform.position + Vector3.up * rayOffset;
        
        RaycastHit hit;
        LayerMask mask = 1 << LayerMask.NameToLayer("Ground");

        //isHit = Physics.BoxCast(center, Vector3.one * 0.5f, Vector3.down, transform.rotation, rayLength, mask);
        if (Physics.BoxCast(center,Vector3.one * 0.5f,Vector3.down,out hit,transform.rotation,rayLength,mask)){
            //hitobj = hit;
            groundHeight = hit.point.y;
            return true;
        }
        return false;
    }

    //能力のアンロック
    public void UnlockPlayerAbility(AbilityID id)
    {
        switch (id)
        {
            case AbilityID.blink:
                /*何らかの処理*/
                return;

            case AbilityID.sliding:
                /*何らかの処理*/
                return;

            case AbilityID.grapHook:
                grapCommand.Enable();
                return;

            case AbilityID.wallJump:
                /*何らかの処理*/
                return;

            default:
                return;

        }
    }
    
    //能力関連のステートの遷移は１フレームに１回にする関数
    private void ManageStateTransition()
    {
        foreach(EventID id in eventPriority)
        {
            switch (id)
            {
                case EventID.grapleOn:
                    ToGrapState();
                    return;

                case EventID.grapleOff:
                    ToGrapOffState();
                    return;

                default:
                    return;
            }
        }
        return;
    }

    //グラップステートへの移行について
    private void ToGrapState() 
    {
        if (grapCommand.CommandInput())
        {
            Vector3 center = transform.position + Vector3.up * grapStartOffset;
            RaycastHit hit;

            if (Physics.SphereCast(center,1.5f,playerCamera.transform.forward,out hit, 50f))
            {
                grapTarget = hit.point;
                stateMachine.Dispatch((int)EventID.grapleOn);
            }
        }
    }

    private void ToGrapOffState()
    {
        if (grapCommand.CommandInput())
        {
            stateMachine.Dispatch((int)EventID.grapleOff);
        }
    }
    
    private void Start()
    {
        //Debug.Log(Mathf.Atan(10 / 2 * Mathf.PI));
        
        playerCamera = Camera.main;
        character = GetComponent<CharacterController>();
        line = GetComponent<LineRenderer>();
        stateMachine = new StateMachine<PlayerController>(this);
        
        stateMachine.AddTransition<GroundState, JumpState>((int)EventID.jump);
        stateMachine.AddTransition<GroundState, FloatingState>((int)EventID.floating);
        stateMachine.AddTransition<JumpState, FloatingState>((int)EventID.floating);
        stateMachine.AddTransition<FloatingState, GroundState>((int)EventID.ground);
        stateMachine.AddTransition<FloatingState, GrapFookState>((int)EventID.grapleOn);
        stateMachine.AddTransition<GrapFookState, GrapOffState>((int)EventID.grapleOff);
        stateMachine.AddTransition<GrapOffState, GroundState>((int)EventID.ground);
        stateMachine.AddTransition<GrapOffState, GrapFookState>((int)EventID.grapleOn);


        if (isGround = IsGround())
        {
            stateMachine.Initialize<GroundState>();
            Debug.Log("start state : groundState");
        }
        else Debug.Log("start state : floatingState");
        stateMachine.Initialize<FloatingState>();

    }

    private void Update()
    {
        isGround = IsGround();
        cameraRotation = playerCamera.transform.localEulerAngles.y;

        stateMachine.OnUpdate();


        ManageStateTransition();
    }
    //ギズモ表示させたいとき使う
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
    */
}
