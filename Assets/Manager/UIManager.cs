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
    [SerializeField] private Canvas clearCanvas;
    [SerializeField] private Canvas pauseCanvas;

    private Timer timer;
    [SerializeField] private TextMeshProUGUI timeCounter;


    //Main�V�[���ł̗v�f
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

    //pause��ʕ\��
    public void DisplayPause()
    {
        pauseCanvas.gameObject.SetActive(true);
    }

    //pause��ʔ�\��
    public void HidePause()
    {
        pauseCanvas.gameObject.SetActive(false);
    }

    //�N���A�\��
    public void DisplayClear()
    {
        clearCanvas.gameObject.SetActive(true);
    }

    public void UIManagerInit()
    {
        timer = new Timer();
        timer.Init();
    }
}
