using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
public class SmoothTest : MonoBehaviour
{
    //test
    private Vector3 transition;
    private Vector3 beforeKey;
    private Vector3 currentKey;
    private Vector3 stackKey;
    private float deltaTime;
    private float smoothTime = 2f;
    private Vector3 keyTransition;
    private float smoothValue;

    //�J�ڂ���֐�(0�`1�̒l�����Ԃ��Ȃ�)
    private float Smooth(float value)
    {
        value = (-value * 4 + 5) * value;
        float re = (-Mathf.Cos(value * 3.14f) + 1) / 2;
        return re;
    }

    private void Update()
    {
        //�Ȃ߂炩�⊮���镔���i��Ŋ֐����j
        currentKey = KeyInput();
        if (currentKey != beforeKey)
        {
            stackKey = keyTransition;
            deltaTime = 0f;
        }

        if (deltaTime < smoothTime)
        {
            deltaTime += Time.deltaTime;
            smoothValue = Smooth(deltaTime / smoothTime);
            transition = stackKey * (1 - smoothValue) + currentKey * smoothValue;
            Debug.Log(smoothValue);
        }
        else
        {
            transition = keyTransition;
        }

        keyTransition = transition;
    }
}
*/