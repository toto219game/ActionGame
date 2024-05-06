/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BaseState = StateMachine<PlayerController_rigid>.BaseState;
using EventID = PlayerController.EventID;

[System.Serializable]
public class GroundState_rigid : BaseState
{
    public override void Entry()
    {
        Debug.Log("GroundState Enter");
        owner.JumpVelocity = 0f;
    }

    public override void Update()
    {
        Quaternion currentRotation = owner.transform.rotation;
        owner.moveVector = owner.groundMove.MoveVector(owner.KeyInput(), Quaternion.Euler(0f, owner.cameraRotation, 0f));

        //�ړ�����
        owner.rb.velocity = owner.moveVector;
        //������n�ʂɍ��킹��
        owner.rb.position = new Vector3(owner.rb.position.x, owner.groundHeight, owner.rb.position.z);

        //���������]�̏���
        owner.rb.rotation = GroundRotation.Rotation(owner.moveVector, currentRotation, owner.rotateSpeed);
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

public class JumpState_rigid : BaseState
{
    public override void Entry()
    {
        Debug.Log("JumpState Enter");
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

public class FloatingState_rigid : BaseState
{

    float jumpDeltaTime = 0f;
    public override void Entry()
    {
        Debug.Log("FloatingState Enter");
        jumpDeltaTime = 0f;
    }

    public override void Update()
    {
        float moveY = owner.moveVector.y;
        owner.moveVector.y = 0f;
        jumpDeltaTime += Time.deltaTime;

        Vector3 floatingVelocity = Quaternion.Euler(0, owner.cameraRotation, 0) * owner.KeyInput()
            * owner.floatingSpeed * Time.deltaTime;

        owner.moveVector += floatingVelocity;

        //�ő呬�x�𒴂��Ă��琧������
        if (owner.moveVector.magnitude > owner.playerMaxSpeed)
        {
            owner.moveVector *= owner.playerMaxSpeed / owner.moveVector.magnitude;
        }
        //�㉺�̈ړ�(�W�����v�Ɨ���)
        owner.moveVector.y = moveY;

        //�ړ�����
        owner.rb.velocity = owner.moveVector;

        if (jumpDeltaTime > 0.1f)
        {
            owner.moveVector.y -= owner.gravity * Time.deltaTime;
        }
        //0.2�b��GroundState�ւ̑J�ڂ����b�N����
        if (jumpDeltaTime < 0.2f) return;
        if (owner.isGround)
        {
            stateMachine.Dispatch((int)EventID.ground);
        }
    }
}

public class GrapFookState_rigid : BaseState
{
    SpringJoint joint;
    public override void Entry()
    {
        owner.rb.AddForce(Vector3.down * 40f);
        joint = owner.gameObject.AddComponent<SpringJoint>();
        joint.autoConfigureConnectedAnchor = false;
        joint.connectedAnchor = owner.testTarget.transform.position;

        float distanceFromPoint = Vector3.Distance(owner.transform.position, owner.testTarget.transform.position);

        joint.maxDistance = 0.8f * distanceFromPoint;
        joint.minDistance = 0.25f * distanceFromPoint;

        joint.spring = 4.5f;
        joint.damper = 7f;
        joint.massScale = 4.5f;
    
    }
}*/