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
        //���p�x��150�x�ȏゾ�����炻�̂܂܂������
        if (Mathf.Abs(current.eulerAngles.y - next.eulerAngles.y) > 150)
        {
            return next;
        }
        //�Ȃ߂炩�ɉ�]
        return Quaternion.RotateTowards(current, next, speed);
    }
}

[System.Serializable]
public class PlayerCommand
{
    /*
     * �L�[���͂Ɋւ���N���X
     * ��{�I�ɂ����œ��͂��擾
     */

    //true�ɂȂ�^�C�~���O.GetKey�̌�ɑ����̂ƈꏏ
    public enum Timing
    {
        keep,up,down
    }

    
    bool keyEnable; //���̃L�[���͂��g���邩�ǂ���(�L�[���͂��u���b�N���������Ɏg����)
    [SerializeField] bool isMouse;   //�}�E�X�{�^���ł̓��͂��ǂ����̔���
    [SerializeField] KeyCode key;
    [SerializeField] int mouseButton;
    [SerializeField] Timing timing;

    public PlayerCommand(KeyCode k,Timing t,bool enable=false)
    {
        key = k;
        timing = t;
        isMouse = false;
        keyEnable = enable;
    }

    public  PlayerCommand(int num,Timing t, bool enable=false)
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

//CharacterController��p��������
[System.Serializable]
public class PlayerController : MonoBehaviour
{

    /*
     * 
     */


    //�J�����Ɋւ��邱��
    private Camera playerCamera;
    [System.NonSerialized]public float cameraRotation;

    //�ڒn����Ɋւ���
    public bool isGround { get; private set; }
    [System.NonSerialized] public float groundHeight;
    private const float rayOffset = 1f;
    private const float rayLength = 0.7f;

    //�����ʒu
    public Vector3 initPosition = Vector3.zero;

    //���ʈړ��Ɋւ���
    [SerializeField] public float floatingSpeed = 35f;
    [SerializeField] public float floatingMaxSpeed { get; private set; } = 15f;
    [SerializeField] public float playerMaxSpeedY { get; private set; } = 150f;

    //���ʈړ��Ɋւ���
    public float rotateSpeed = 5f;

    //�W�����v�Ɋւ���
    public float gravity = 40f;
    public float JumpVelocity { get; set; } = 0f;

    //���������臒l
    private float fallHeight = -100f;

    [System.NonSerialized]public Vector3 moveVector;//�v���p�e�B�ɂ������������H�e�v�f�ɃA�N�Z�X�ł��Ȃ��Ȃ�

    //�֗��N���X
    [SerializeField] public GroundMove groundMove = new GroundMove();

    //�L�����N�^�[�R���g���[���[�R���|�[�l���g
    [System.NonSerialized] public CharacterController character;

    //�X�e�[�g�}�V��
    StateMachine<PlayerController> stateMachine;
    [System.NonSerialized] public List<EventID> eventPriority = new List<EventID>();
    
    //�R�}���h(�L�[�R��)
    [SerializeField] PlayerCommand grapCommand = new PlayerCommand(0, PlayerCommand.Timing.down,false);
    [SerializeField] PlayerCommand blinkCommand = new PlayerCommand(KeyCode.E, PlayerCommand.Timing.down, false);

    //�L�[���͂��L�����ǂ����B�B�B���邩�ǂ����킩��Ȃ��ꉞ�u���Ă���=============
    private bool inputEnable = true;
    //=============================================================================

    //�O���b�v�����O�ɂ������ϐ�
    public Vector3 grapTarget;
    [System.NonSerialized] public LineRenderer line;
    private float grapStartOffset = 2f;

    //�M�Y���̂��߂̂���
    /* bool isHit;
     RaycastHit hitobj;*/

    //�C�x���g��enum
    public enum EventID
    {
        ground,
        jump,
        floating,
        blink,
        grapleOn,
        grapleOff
    }

    //�L�[����(WASD)�@���K���ς�
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

        //���͂��L���������琳��ɕԂ��A�L������Ȃ�������[��
        if (inputEnable)
        {
            return new Vector3(inputX, 0, inputZ).normalized;
        }
        else
        {
            return Vector3.zero;
        }
        
    }

    //�ڒn����
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


    //�X�e�[�W���痎�������̏���
    private void IsFalling()
    {
        if (transform.position.y < fallHeight)
        {
            character.enabled = false;
            transform.position = initPosition;
            /*Debug.Log(initPosition);
            Debug.Log(transform.position);*/
            character.enabled = true;
        }
    }

    //�\�͂̃A�����b�N
    public void UnlockPlayerAbility(AbilityID id)
    {
        switch (id)
        {
            case AbilityID.blink:
                blinkCommand.Enable();
                return;

            case AbilityID.sliding:
                /*���炩�̏���*/
                return;

            case AbilityID.grapHook:
                grapCommand.Enable();
                return;

            case AbilityID.wallJump:
                /*���炩�̏���*/
                return;

            default:
                return;

        }
    }
    
    //�\�͊֘A�̃X�e�[�g�̑J�ڂ͂P�t���[���ɂP��Ɍ��肷��֐�
    private void ManageStateTransition()
    {

        //���m�F����������return���Ă��܂��I�I�I�I�I
        foreach(EventID id in eventPriority)
        {
            switch (id)
            {
                case EventID.blink:
                    if (blinkCommand.CommandInput())
                    {
                        stateMachine.Dispatch((int)EventID.blink);
                        return;
                    }
                    break;
                    


                case EventID.grapleOn:
                    if (grapCommand.CommandInput())
                    {
                        Vector3 center = transform.position + Vector3.up * grapStartOffset;
                        RaycastHit hit;

                        if (Physics.SphereCast(center, 5f, playerCamera.transform.forward, out hit, 50f))
                        {
                            grapTarget = hit.point;
                            stateMachine.Dispatch((int)EventID.grapleOn);
                        }
                        return;
                    }
                    break;
                    

                case EventID.grapleOff:
                    if (grapCommand.CommandInput())
                    {
                        stateMachine.Dispatch((int)EventID.grapleOff);
                        return;
                    }
                    break;

                default:
                    return;
            }
        }
        return;
    }

    //�u�����N�X�e�[�g�ւ̈ڍs�ɂ���
    private void ToBlinkState()
    {
        if (blinkCommand.CommandInput())
        {
            stateMachine.Dispatch((int)EventID.blink);
        }
    }

    //�O���b�v�X�e�[�g�ւ̈ڍs�ɂ���
    private void ToGrapState() 
    {
        if (grapCommand.CommandInput())
        {
            Vector3 center = transform.position + Vector3.up * grapStartOffset;
            RaycastHit hit;

            if (Physics.SphereCast(center,5f,playerCamera.transform.forward,out hit, 50f))
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
        stateMachine.AddTransition<FloatingState, BlinkState>((int)EventID.blink);
        stateMachine.AddTransition<BlinkState, GroundState>((int)EventID.ground);
        stateMachine.AddTransition<FloatingState, GrapFookState>((int)EventID.grapleOn);
        stateMachine.AddTransition<GrapFookState, GrapOffState>((int)EventID.grapleOff);
        stateMachine.AddTransition<GrapOffState, GroundState>((int)EventID.ground);
        stateMachine.AddTransition<GrapOffState, GrapFookState>((int)EventID.grapleOn);
        stateMachine.AddTransition<GrapOffState, BlinkState>((int)EventID.blink);


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
        IsFalling();
        isGround = IsGround();
        cameraRotation = playerCamera.transform.localEulerAngles.y;

        stateMachine.OnUpdate();


        ManageStateTransition();

    }
    //�M�Y���\�����������Ƃ��g��
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
