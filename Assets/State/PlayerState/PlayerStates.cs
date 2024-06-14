using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BaseState = StateMachine<PlayerController>.BaseState;
using EventID = PlayerController.EventID;

public class PlayerState : BaseState
{
    protected List<EventID> priorityList = new List<EventID>();
}

//��{�̃X�e�[�g============================================================
[System.Serializable]
public class GroundState:PlayerState
{    
    public GroundState()
    {
        priorityList.Add(EventID.grapleOn);
    }

    //��{�̂R�֐�===================================================================================================

    public override void Entry()
    {
        Debug.Log("GroundState Enter");
        owner.JumpVelocity = 0f;
        owner.eventPriority = priorityList;
    }

    public override void Update()
    {
        Quaternion currentRotation = owner.transform.rotation;
        owner.moveVector = owner.groundMove.MoveVector(owner.KeyInput(), owner.cameraRotation);

        //�ړ�����
        owner.character.Move(owner.moveVector * Time.deltaTime);
        //������n�ʂɍ��킹��
        owner.transform.position = new Vector3(owner.transform.position.x, owner.groundHeight, owner.transform.position.z);

        //���������]�̏���
        owner.transform.rotation = GroundRotation.Rotation(owner.moveVector, currentRotation, owner.rotateSpeed);

        if (Input.GetKey(KeyCode.Space))
        {
            stateMachine.Dispatch((int)EventID.jump);
            return;
            
        }
        if (!owner.isGround)
        {
            stateMachine.Dispatch((int)EventID.floating);
            return;
        }
    }
    public override void Exit() 
    {
        Debug.Log("GroundState Exit");
    }
}

public class JumpState : BaseState
{
    public override void Entry()
    {
        Debug.Log("JumpState Enter");
        owner.eventPriority.Clear();
        owner.moveVector.y = 15f;
        stateMachine.Dispatch((int)EventID.floating);
    }

    public override void Update()
    {
        Debug.Log("JumpState Update");
    }

    public override void Exit()
    {
        Debug.Log("JumpState Exit");
    }
}

public class FloatingState : PlayerState
{

    float floatDeltaTime = 0f;
    float moveY; //moveVector��y�������ꎞ�I�ɕۑ����Ă����ϐ�

    public FloatingState()
    {
        priorityList.Add(EventID.blink);
        priorityList.Add(EventID.grapleOn);
        priorityList.Add(EventID.clingWall);
    }

    //�w��̕b�������Ȃ��Əd�͂������Ȃ�����
    private void AddGravity(float interval = 0.1f)
    {
        if (floatDeltaTime > interval)
        {
            moveY -= owner.gravity * Time.deltaTime;
        }
    }


    private void TransitionToGroundState(float interval = 0.2f)
    {
        //interval�̕b����GroundState�ւ̑J�ڂ����b�N����
        if (floatDeltaTime < interval) return;
        if (owner.isGround)
        {
            stateMachine.Dispatch((int)EventID.ground);
        }
    }

    //��{�̂R�֐�===================================================================================================
    public override void Entry()
    {
        Debug.Log("FloatingState Enter");
        floatDeltaTime = 0f;
        owner.eventPriority = priorityList;
    }

    public override void Update()
    {
        //y����������������A��������Ȃ��Ɛ������X�s�[�h��臒l��ݒ�ł��Ȃ�
        moveY = owner.moveVector.y;
        owner.moveVector.y = 0f;

        floatDeltaTime += Time.deltaTime;

        Vector3 floatingVelocity = owner.InputVector() * owner.floatingSpeed * Time.deltaTime;

        owner.moveVector += floatingVelocity;

        //�ő呬�x�𒴂��Ă��琧������
        if (owner.moveVector.magnitude > owner.floatingMaxSpeed)
        {
            owner.moveVector *= owner.floatingMaxSpeed / owner.moveVector.magnitude;
        }
        //�㉺�̈ړ�(�W�����v�Ɨ���)
        AddGravity();
        owner.moveVector.y = Mathf.Clamp(moveY, -owner.playerMaxSpeedY, owner.playerMaxSpeedY);
        //�ړ�����
        owner.character.Move(owner.moveVector * Time.deltaTime);

        TransitionToGroundState(0.2f);
    }
}

//�\�͊֘A�̃X�e�[�g==========================================================

public class BlinkState : PlayerState
{
    private float blinkSpeed = 40f;
    private float maxSpeed = 30f;
    private float blinkDeltaTime = 0f;
    private float blinkMaxTime = 0.2f;
    private float blinkForce = 3f;
    private float moveY = 0f;

    public BlinkState()
    {
        priorityList.Add(EventID.grapleOn);
    }

    public override void Entry()
    {
        Debug.Log("entry : BlinkState");
        //������
        blinkDeltaTime = 0f;
        owner.moveVector.y = 0f;

        Vector3 input;

        
        if (owner.KeyInput() == Vector3.zero)
        {
            //�������͂��ĂȂ��Ƃ��͑O��
            input = Vector3.forward;
        }
        else
        {
            //���͂��Ă�Ƃ��͂��̕�����
            input = owner.KeyInput();
        }

        owner.moveVector += Quaternion.Euler(0f, owner.cameraRotation, 0f) * input * blinkSpeed;

        owner.eventPriority = priorityList;
    }

    public override void Update()
    {
        blinkDeltaTime += Time.deltaTime;
        owner.character.Move(owner.moveVector * Time.deltaTime);

        
        if(blinkDeltaTime > blinkMaxTime)
        {
            moveY = owner.moveVector.y;
            owner.moveVector.y = 0f;

            owner.moveVector += owner.moveVector += owner.InputVector() * blinkForce * Time.deltaTime;
            if(owner.moveVector.magnitude > maxSpeed)
            {
                owner.moveVector *= maxSpeed / owner.moveVector.magnitude;
            }

            moveY -= owner.gravity * Time.deltaTime;
            owner.moveVector.y = Mathf.Clamp(moveY, -owner.playerMaxSpeedY, owner.playerMaxSpeedY);
        }

        if (owner.isGround)
        {
            Debug.Log("blink �� ground");
            stateMachine.Dispatch((int)EventID.ground);
        }
    }

    public override void Exit()
    {
        Debug.Log("BlinkState : Exit");
        foreach (EventID e in priorityList)
        {
            Debug.Log(e);
        }
    }

}

public class GrapOnState:PlayerState
{
    private Vector3 targetVector;
    private Vector3 centerPower;
    private float gravity = 55f;
    private float speed = 1f;
    private float forceFactor = 3f;

    //�΂˒萔�Ƃ�
    float maxSpringDistance = 0f;
    float springPower = 50f;
    float maxPower = 500f;
    float maxDifference = 0f;
    float difference = 0f;
    private float grapLength;

    //���[�v�̌��_
    float ropeOffset = 1f;

    public GrapOnState()
    {
        priorityList.Add(EventID.grapleOff);
        priorityList.Add(EventID.blink);
    }

    public override void Entry()
    {
        Debug.Log("GrapFookState Enter");
        owner.line.positionCount = 2;
        grapLength = (owner.grapTarget - owner.transform.position).magnitude;
        maxSpringDistance = grapLength * 0.8f;

        owner.eventPriority = priorityList;
    }

    public override void Update()
    {
        targetVector = owner.grapTarget - owner.transform.position;
        
        if(targetVector.magnitude > maxDifference)
        {
            difference = targetVector.magnitude - maxSpringDistance;
        }
        else
        {
            difference = 0f;
        }


        centerPower = Mathf.Clamp(difference * springPower, 0f, maxPower) * targetVector.normalized;
        if (Vector3.Dot(owner.moveVector,centerPower) > 0)
        {
            //�����ւ̑��x�ւ̌���
            centerPower *= 0.05f;
        }


        //�ړ�����
        owner.moveVector += (Vector3.down * gravity + centerPower + owner.KeyInput() * speed) * Time.deltaTime;
        owner.moveVector += owner.InputVector() * forceFactor * Time.deltaTime;
        owner.character.Move(owner.moveVector * Time.deltaTime);

        //���[�v�\��
        owner.line.SetPosition(0,owner.transform.position + Vector3.up * ropeOffset);
        owner.line.SetPosition(1, owner.grapTarget);
    }

    public override void Exit()
    {
        Debug.Log("GrapFookState Exit");
        owner.line.positionCount = 0;
    }
}

public class GrapOffState : PlayerState
{
    float maxSpeed;
    float grapOffForce = 5f;
    float gravity = 40f;
    float moveY; //moveVector��y�������ꎞ�I�ɕۑ����Ă����ϐ�

    public GrapOffState()
    {
        priorityList.Add(EventID.grapleOn);
        priorityList.Add(EventID.blink);
    }

    public override void Entry()
    {
        Debug.Log("GrapFookState Exit");
        maxSpeed = owner.moveVector.magnitude;

        owner.eventPriority = priorityList;
    }

    public override void Update()
    {
        //y����������������A��������Ȃ��Ɛ������X�s�[�h��臒l��ݒ�ł��Ȃ�
        moveY = owner.moveVector.y;
        owner.moveVector.y = 0f;

        owner.moveVector += owner.InputVector() * grapOffForce * Time.deltaTime;
        if(owner.moveVector.magnitude > maxSpeed)
        {
            owner.moveVector *= maxSpeed / owner.moveVector.magnitude;
        }

        //��������y�����ɑ΂��ďd�͂�������moveVector�ɖ߂�
        moveY -= gravity * Time.deltaTime;
        owner.moveVector.y = Mathf.Clamp(moveY, -owner.playerMaxSpeedY, owner.playerMaxSpeedY);
        owner.character.Move(owner.moveVector * Time.deltaTime);

        if (owner.isGround)
        {
            stateMachine.Dispatch((int)EventID.ground);
        }
    }

    public override void Exit()
    {
        Debug.Log("GrapOffState Exit");
    }
}

public class ClingWallState : PlayerState
{
    Vector3 wallPointVector;
    Vector3 wallPoint;
    Vector3 rayOrigin;
    float rayLength = 0.7f;
    RaycastHit hit;
    LayerMask mask;
    float cos;

    float clingMaxSpeed = 15f;
    float clingMinSpeed = 0.05f;
    float clingForce = 95f;
    float damplerForce = 50f;
    float deltaTime = 0f;
    float wallMaxTime = 1f;
    float intervalTime = 1f;

    Vector3 deltaVector;

    //���C���[�}�X�N�̎擾�̂��߂̃R���X�g���N�^
    public ClingWallState()
    {
        mask = 1 << LayerMask.NameToLayer("Ground");
        priorityList.Add(EventID.wallJump);
        priorityList.Add(EventID.wallOff);
        priorityList.Add(EventID.blink);
    }

    public override void Entry()
    {
        Debug.Log("Enter : ClingWallState");
        
        rayOrigin = owner.transform.position + Vector3.up * owner.wallRayOffset;
        wallPoint = owner.wallPoint;
        wallPointVector = wallPoint - rayOrigin;
        owner.moveVector.y = 0f;
        deltaTime = 0f;

        owner.eventPriority = priorityList;
    }

    public override void Update()
    {
        rayOrigin = owner.transform.position + Vector3.up * owner.wallRayOffset;

        if(Physics.Raycast(rayOrigin,wallPointVector,out hit, rayLength,mask) && deltaTime < wallMaxTime)
        {
            deltaTime += Time.deltaTime;
            wallPoint = hit.point;
            owner.wallNormal = hit.normal;

            deltaVector = Vector3.Cross(wallPointVector.normalized, Vector3.up);
            cos = Vector3.Dot(deltaVector, owner.InputVector());
            deltaVector *= cos * clingForce;

            owner.moveVector += (deltaVector + -owner.moveVector.normalized * damplerForce) * Time.deltaTime;
            if (owner.moveVector.magnitude > clingMaxSpeed)
            {
                owner.moveVector *= clingMaxSpeed / owner.moveVector.magnitude;
            }
            else if(owner.moveVector.magnitude < clingMinSpeed)
            {
                owner.moveVector = Vector3.zero;
            }

            owner.character.Move(owner.moveVector * Time.deltaTime);

        }
        else
        {
            stateMachine.Dispatch((int)EventID.wallOff);
        }
    }

    public override void Exit()
    {
        Debug.Log("Exit : ClingWallState");
    }
}
public class WallJumpState : PlayerState
{
    float wallJumpPower = 15f;
    public override void Entry()
    {
        Debug.Log("Enter: WallJumpState");
        owner.moveVector += Vector3.up * wallJumpPower + owner.wallNormal * wallJumpPower;
        stateMachine.Dispatch((int)EventID.wallOff);
    }

    public override void Exit()
    {
        Debug.Log("Exit : WallJumpState");
    }
}

public class WallOffState : FloatingState
{

    public WallOffState()
    {
        priorityList.Add(EventID.ground);
    }

    public override void Entry()
    {
        Debug.Log("enter : WalloffState");
        owner.eventPriority = priorityList;
    }


    public override void Exit()
    {
        Debug.Log("Exit : WallOffState");
    }
}