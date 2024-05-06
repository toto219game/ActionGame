using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BaseState = StateMachine<PlayerController>.BaseState;
using EventID = PlayerController.EventID;

//基本のステート============================================================
[System.Serializable]
public class GroundState:BaseState
{
    public override void Entry()
    {
        Debug.Log("GroundState Enter");
        owner.JumpVelocity = 0f;
    }

    public override void Update()
    {
        Quaternion currentRotation = owner.transform.rotation;
        owner.moveVector = owner.groundMove.MoveVector(owner.KeyInput(), owner.cameraRotation);

        //移動処理
        owner.character.Move(owner.moveVector * Time.deltaTime);
        //高さを地面に合わせる
        owner.transform.position = new Vector3(owner.transform.position.x, owner.groundHeight, owner.transform.position.z);

        //ここから回転の処理
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

    float jumpDeltaTime = 0f;
    float maxSpeedY = 150f;

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

        //最大速度を超えてたら制限する
        if (owner.moveVector.magnitude > owner.playerMaxSpeed)
        {
            owner.moveVector *= owner.playerMaxSpeed / owner.moveVector.magnitude;
        }
        //上下の移動(ジャンプと落下)
        owner.moveVector.y = moveY;
        owner.moveVector.y = Mathf.Clamp(owner.moveVector.y, -owner.playerMaxSpeedY, owner.playerMaxSpeedY);
        //移動処理
        owner.character.Move(owner.moveVector * Time.deltaTime);

        if (jumpDeltaTime > 0.1f)
        {
            owner.moveVector.y -= owner.gravity * Time.deltaTime;
        }
        //0.2秒はGroundStateへの遷移をロックする
        if (jumpDeltaTime < 0.2f) return;
        if (owner.isGround)
        {
            stateMachine.Dispatch((int)EventID.ground);
        }
    }
}

//===========================================================================

public class GrapFookState:BaseState
{
    private Vector3 targetVector;
    private Vector3 centerPower;
    private float gravity = 40f;
    private float speed = 1f;
    private float inputForce = 3f;

    //ばね定数とか
    float maxSpringDistance = 0f;
    float springPower = 50f;
    float maxPower = 500f;
    float maxDifference = 0f;
    float difference = 0f;
    private float grapLength;

    //ロープの原点
    float ropeOffset = 1f;

    public override void Entry()
    {
        Debug.Log("GrapFookState Enter");
        owner.line.positionCount = 2;
        grapLength = (owner.grapTarget - owner.transform.position).magnitude;
        maxSpringDistance = grapLength * 0.8f;
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
            //中央への速度への減衰
            centerPower *= 0.05f;
        }


        //移動処理
        owner.moveVector += (Vector3.down * gravity + centerPower + owner.KeyInput() * speed) * Time.deltaTime;
        owner.moveVector += Quaternion.Euler(0, owner.cameraRotation, 0) * owner.KeyInput() * inputForce * Time.deltaTime;
        owner.character.Move(owner.moveVector * Time.deltaTime);

        //ロープ表示
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
    float inputForce = 5f;
    float gravity = 40f;
    public override void Entry()
    {
        Debug.Log("GrapFookState Exit");
        maxSpeed = owner.moveVector.magnitude;
    }

    public override void Update()
    {
        float moveY = owner.moveVector.y;
        owner.moveVector.y = 0f;
        owner.moveVector += Quaternion.Euler(0f, owner.cameraRotation, 0f) * owner.KeyInput() * inputForce * Time.deltaTime;
        if(owner.moveVector.magnitude > maxSpeed)
        {
            owner.moveVector *= maxSpeed / owner.moveVector.magnitude;
        }

        
        moveY -= gravity * Time.deltaTime;
        owner.moveVector.y = moveY;
        owner.moveVector.y = Mathf.Clamp(owner.moveVector.y, -owner.playerMaxSpeedY, owner.playerMaxSpeedY);
        owner.character.Move(owner.moveVector * Time.deltaTime);
        Debug.Log(owner.moveVector);

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
