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

    //遷移する関数(0〜1の値しか返さない)
    private float Smooth(float value)
    {
        value = (-value * 4 + 5) * value;
        float re = (-Mathf.Cos(value * 3.14f) + 1) / 2;
        return re;
    }

    private void Update()
    {
        //なめらか補完する部分（後で関数化）
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