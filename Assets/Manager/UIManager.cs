using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer
{
    private float deltaTime = 0f;
    private int msec = 0;
    private int sec = 0;
    private int min = 0;

    public void Init()
    {
        deltaTime = 0f;
    }

    public string UpdateTime()
    {
        deltaTime += Time.deltaTime;

        min = Mathf.FloorToInt(deltaTime / 60f);
        sec = Mathf.FloorToInt(deltaTime % 60f);
        msec = Mathf.FloorToInt((deltaTime - 60f * min - sec) * 100);

        return string.Format("{0:00}:{1:00}.{2:00}", min, sec, msec);
    }
}

public class UIManager : MonoBehaviour
{
    //�V���O���g����
    #region
    public static UIManager Instance;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    #endregion
    [SerializeField] private TextMeshProUGUI targetItemCounter;
    [SerializeField] private GameObject ClearText;

    private Timer timer;
    [SerializeField] private TextMeshProUGUI timeCounter;

    //�^�C�}�[�̍X�V
    public void TimerUpdate()
    {
        timeCounter.text = timer.UpdateTime();
    }

    //�c�������
    public void CounterUpdate(int num)
    {
        targetItemCounter.text = num.ToString();
    }
    //�N���A�\��
    public void DisplayClear()
    {
        ClearText.SetActive(true);
    }

    public void UIManagerInit()
    {
        timer = new Timer();
        timer.Init();
    }
}
