using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BaseState = StateMachine<PlayerController>.BaseState;
using EventID = PlayerController.EventID;

//��{�̃X�e�[�g============================================================
[System.Serializable]
public class GroundState:BaseState
{
    public override void Entry()
    {
        Debug.Log("GroundState Enter");
        owner.JumpVelocity = 0f;
        owner.eventPriority.Clear();
        owner.eventPriority.Add(EventID.grapleOn);
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

public class FloatingState : BaseState
{

    float floatDeltaTime = 0f;
    float moveY; //moveVector��y�������ꎞ�I�ɕۑ����Ă����ϐ�

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

    public override void Entry()
    {
        Debug.Log("FloatingState Enter");
        floatDeltaTime = 0f;
        owner.eventPriority.Clear();
        owner.eventPriority.Add(EventID.blink);
        owner.eventPriority.Add(EventID.grapleOn);
        owner.eventPriority.Add(EventID.clingWall);
    }

    public override void Update()
    {
        //y����������������A��������Ȃ��Ɛ������X�s�[�h��臒l��ݒ�ł��Ȃ�
        moveY = owner.moveVector.y;
        owner.moveVector.y = 0f;

        floatDeltaTime += Time.deltaTime;

        Vector3 floatingVelocity = Quaternion.Euler(0, owner.cameraRotation, 0) * owner.KeyInput()
            * owner.floatingSpeed * Time.deltaTime;

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

public class BlinkState : BaseState
{
    private float blinkSpeed = 40f;
    private float maxSpeed = 30f;
    private float blinkDeltaTime = 0f;
    private float blinkMaxTime = 0.2f;
    private float blinkForce = 3f;
    private float moveY = 0f;

    public override void Entry()
    {
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

        owner.eventPriority.Clear();
        owner.eventPriority.Add(EventID.grapleOn);
    }

    public override void Update()
    {
        blinkDeltaTime += Time.deltaTime;
        owner.character.Move(owner.moveVector * Time.deltaTime);

        
        if(blinkDeltaTime > blinkMaxTime)
        {
            moveY = owner.moveVector.y;
            owner.moveVector.y = 0f;

            owner.moveVector += owner.moveVector += Quaternion.Euler(0f, owner.cameraRotation, 0f) * owner.KeyInput() * blinkForce * Time.deltaTime;
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
    }

}

public class GrapFookState:BaseState
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

    public override void Entry()
    {
        Debug.Log("GrapFookState Enter");
        owner.line.positionCount = 2;
        grapLength = (owner.grapTarget - owner.transform.position).magnitude;
        maxSpringDistance = grapLength * 0.8f;
        owner.eventPriority.Clear();
        owner.eventPriority.Add(EventID.grapleOff);
        owner.eventPriority.Add(EventID.blink);
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
        owner.moveVector += Quaternion.Euler(0, owner.cameraRotation, 0) * owner.KeyInput() * forceFactor * Time.deltaTime;
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

public class GrapOffState : BaseState
{
    float maxSpeed;
    float grapOffForce = 5f;
    float gravity = 40f;
    float moveY; //moveVector��y�������ꎞ�I�ɕۑ����Ă����ϐ�

    public override void Entry()
    {
        Debug.Log("GrapFookState Exit");
        maxSpeed = owner.moveVector.magnitude;
        owner.eventPriority.Clear();
        owner.eventPriority.Add(EventID.grapleOn);
        owner.eventPriority.Add(EventID.blink);
    }

    public override void Update()
    {
        //y����������������A��������Ȃ��Ɛ������X�s�[�h��臒l��ݒ�ł��Ȃ�
        moveY = owner.moveVector.y;
        owner.moveVector.y = 0f;

        owner.moveVector += Quaternion.Euler(0f, owner.cameraRotation, 0f) * owner.KeyInput() * grapOffForce * Time.deltaTime;
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

public class ClingWallState : BaseState
{
    Vector3 wallPointVector;
}
