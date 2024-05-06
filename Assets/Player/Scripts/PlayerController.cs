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

//CharacterController��p��������
[System.Serializable]
public class PlayerController : MonoBehaviour
{

    /*
     * PlayerController�̉ۑ�
     * �����t���[�����ŃC�x���g���������s�����Ƒz��̃X�e�[�g�֑J�ڂł��Ȃ�
     * ��@�E�����{�^���������݂̃X�e�[�g�ɂ���ăX�e�[�g�̑J�ڂ�ς��������Ȃ�
     *      ���O���b�v�X�e�[�g���瓯���{�^���Ńt���[�e�B���O�ɖ߂肽�����ɖ߂�Ȃ��Ȃ�
     *      
     * ������: �C�x���g�ɗD�揇�ʂ�����@���@�߂�ǂ������A���݂̃X�e�[�g�ɂ���ėD�揇�ʂ��ς��̂����
     *          ���̑�)�X�e�[�g�̒�`����Dispatch���ς܂���@���@������߂�ǂ������A�����ȃX�e�[�g����J�ډ\�̏ꍇ�ƂĂ����邢
     *          
     *          �ǂ����悤��
     */


    //�J�����Ɋւ��邱��
    private Camera playerCamera;
    [System.NonSerialized]public float cameraRotation;

    //�ڒn����Ɋւ���
    public bool isGround { get; private set; }
    [System.NonSerialized] public float groundHeight;
    private const float rayOffset = 1f;
    private const float rayLength = 0.5f;

    //���ʈړ��Ɋւ���
    [SerializeField] public float groundSpeed = 10f;
    [SerializeField] public float floatingSpeed = 35f;
    [SerializeField] public float playerMaxSpeed { get; private set; } = 15f;
    [SerializeField] public float playerMaxSpeedY { get; private set; } = 150f;
    //���ʈړ��Ɋւ���
    public float rotateSpeed = 5f;

    //�W�����v�Ɋւ���
    public float gravity = 40f;
    public float JumpVelocity { get; set; } = 0f;

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

    //�O���b�v�e�X�g
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
        grapleOn,
        grapleOff
    }

    //�L�[����(WASD)
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

    //�\�͂̃A�����b�N
    public void UnlockPlayerAbility(AbilityID id)
    {
        switch (id)
        {
            case AbilityID.blink:
                /*���炩�̏���*/
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
    
    //�\�͊֘A�̃X�e�[�g�̑J�ڂ͂P�t���[���ɂP��ɂ���֐�
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

    //�O���b�v�X�e�[�g�ւ̈ڍs�ɂ���
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
