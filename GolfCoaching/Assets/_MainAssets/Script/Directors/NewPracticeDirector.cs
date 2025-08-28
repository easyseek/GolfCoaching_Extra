using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using DG;
using DG.Tweening;

public class NewPracticeDirector : MonoBehaviour
{
    enum PRACTICESTEP
    {
        INIT,
        READY,
        PROPLAY,
        ADDRESSWAIT,
        PRACTICE

    }
    PRACTICESTEP praticeStep = PRACTICESTEP.INIT;

    [SerializeField] StepCheckTest stepCheck;
    [SerializeField] VideoPlayer ProVideoPlayer;
    [SerializeField] RenderTexture ProRenderTexture;

    [SerializeField] Camera CameraPro;
    [SerializeField] GameObject View3D;

    [SerializeField] Image imgSensorProgress;

    [SerializeField] GameObject objInfo;
    [SerializeField] TextMeshProUGUI txtInfo;
    Queue qMsgs = new Queue();

    int _layoutIndex = 0;
    //Vector2[] LayoutMask_Top = {new Vector2(1920, 1080), new Vector2(0, 0), new Vector2(0, 0) };
    Vector2[] LayoutVideo_Top = {new Vector2(1920, 1080), new Vector2(1152, 648), new Vector2(0, 0) };
    Vector2[] LayoutPosition_Top = {new Vector2(0, 540), new Vector2(0, 636), new Vector2(0, 0) };

    Vector2[] LayoutMask_Btm = { new Vector2(1080, 960), new Vector2(540, 960), new Vector2(1080, 1360) };
    Vector2[] LayoutVideo_Btm = { new Vector2(1104, 1472), new Vector2(720, 960), new Vector2(1104, 1472) };
    Vector2[] LayoutPosition_Btn = { new Vector2(0, 480), new Vector2(-270, 480), new Vector2(270, 480), new Vector2(0, 648) };

    //[SerializeField] Transform ProMask;
    [SerializeField] RectTransform ProVIdeo;
    [SerializeField] RectTransform FrontMask;
    [SerializeField] RectTransform FrontVIdeo;
    [SerializeField] RectTransform SideMask;
    [SerializeField] RectTransform SideVIdeo;



    enum SCREENLAYOUT
    {
        SPLIT_PRO_USER_FRONT,
        SPLIT_PRO_USER_SIDE,
        SPLIT_PRO_,
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stepCheck.PoseLock = true;
        SetLayout(_layoutIndex = 0);

        praticeStep = PRACTICESTEP.INIT;
        imgSensorProgress.fillAmount = 0;
        StartCoroutine(CoShowInfo());
        if (MocapManager.Instance != null)
            MocapManager.Instance.StartMatchPose();

    }

    IEnumerator LessonStart()
    {
        Debug.Log("LessonStart() START");

        ShowInfo(true, "������ �ÿ� ����", 2f);
        yield return new WaitForSeconds(1.0f);

        //1ȸ ���� ���� ��û
        StartCoroutine(ProVideoPlay(true));

        yield return new WaitForSeconds(4.0f);

        ShowInfo(true, "��巹�� �ڼ��� �����ϼ���.", 2f);

        //����� ��巹���ڼ� ���
        Debug.Log("LessonStart() ����� ���");
        stepCheck.PoseLock = false; //����üũ ����
        float isAddress = 0;
        //StartCoroutine(ProVideoPlay(true)); //������ �ݺ� ���

        float waiter = 0;

        while (isAddress < 1 || waiter < 1)
        {
            if (stepCheck.CheckAddress())
            {
                if (waiter > 1)
                    isAddress += 0.75f * Time.deltaTime;
                else
                {
                    isAddress = 0;
                    waiter += 0.5f * Time.deltaTime;
                }
            }
            else
            {
                isAddress = 0;
                waiter = 0;
                //waiter += Time.deltaTime;
                /*if (waiter > 20)
                {
                    waiter = 0;
                    ShowInfo(true, "��巹�� �ڼ��� �����ϼ���.", 2f);
                }*/
            }

            imgSensorProgress.fillAmount = Mathf.Clamp(isAddress, 0, 1f);

            yield return null;
        }

        Sequence seq = DOTween.Sequence();
        seq.Join(imgSensorProgress.transform.DOScale(2f, 1f)).Join(imgSensorProgress.DOFade(0, 1f));
        seq.Play();
        yield return new WaitForSeconds(1f);

        Debug.Log("LessonStart() ����� ��巹�� Ȯ��");
        //StopAllCoroutines();
        ProVideoPlayer.Stop();

        //3Dȭ�� ��ȯ        
        //Camera3D.depth = 2;
        imgSensorProgress.fillAmount = 0;
        //Camera.SetupCurrent(Camera3D);
        View3D.SetActive(true);
        ProVIdeo.gameObject.SetActive(false);
        //yield return new WaitUntil(() => stepCheck.GetPoseStep() == StepCheckTest.SWINGSTEP.TakeBack);
        StartCoroutine(PracticeStart());

        ShowInfo(true, "3D���� ���� ������ �ݺ��ϼ���.", 2f);

        Debug.Log("LessonStart() END");
    }

    IEnumerator ProVideoPlay(bool isLoop = false)
    {
        Debug.Log($"ProVideoPlay({isLoop}) START");

        ProRenderTexture.Release();
        yield return null;
        if (ProVideoPlayer.isPrepared == false)
        {
            ProVideoPlayer.Prepare();
            yield return new WaitUntil(() => ProVideoPlayer.isPrepared == true);
        }
        ProVideoPlayer.isLooping = false;//isLoop;
        ProVideoPlayer.Play();
        long endFrmae = (long)ProVideoPlayer.frameCount - 5;
        Debug.Log($"ProVideoPlayer.frameCount:{ProVideoPlayer.frameCount}");
        do
        {

            if (ProVideoPlayer.frame >= endFrmae)
            {
                Debug.Log($"RESTART");
                ProVideoPlayer.Pause();
                yield return new WaitForSeconds(0.5f);
                ProVideoPlayer.frame = 1; // �Ǵ� 0���� �����Ͽ� �ݺ� ����                
                yield return new WaitForSeconds(0.5f);
                ProVideoPlayer.Play();
            }
            yield return null;

        } while (isLoop);
        
        Debug.Log($"ProVideoPlay({isLoop}) END");
    }

    IEnumerator PracticeStart()
    {
        yield return null;
    }

    public void OnClick_Home()
    {
        StopAllCoroutines();
        if (MocapManager.Instance != null)
            MocapManager.Instance.StopMatchPose();

        SceneManager.LoadScene("ModeSelect");
    }

    void ShowInfo(bool isShow, string msg, float dueTime)
    {
        qMsgs.Enqueue(new msgContainer(msg, dueTime));
    }

    IEnumerator CoShowInfo()
    {
        while (true)
        {
            if(qMsgs.Count > 0)
            {
                msgContainer msg = (msgContainer)qMsgs.Dequeue();

                txtInfo.text = msg.msg;

                objInfo.transform.DOScale(Vector3.one, 0.5f).From(new Vector3(1, 0 ,1));
                
                yield return new WaitForSeconds(msg.dueTime + 0.5f);
                objInfo.transform.DOScale(new Vector3(1, 0, 1), 0.5f);
                yield return new WaitForSeconds(0.5f);
                //objInfo.SetActive(false);
                txtInfo.text = string.Empty;
            }
            yield return null;            
        }
    }

    public class msgContainer
    {
        public string msg;
        public float dueTime;

        public msgContainer(string msg, float dueTime)
        {
            this.msg = msg;
            this.dueTime = dueTime;
        }
    }

    public void OnClick_LayoutChange()
    {
        _layoutIndex = _layoutIndex == 4 ? _layoutIndex = 0 : _layoutIndex + 1;
        Debug.Log($"OnClick_LayoutChange() - {_layoutIndex}");
        SetLayout(_layoutIndex);
    }

    void SetLayout(int index)
    {
        if(index == 0) //������ ����Ʈ �Ʒ�
        {
            FrontMask.gameObject.SetActive(true);
            SideMask.gameObject.SetActive(false);

            ProVIdeo.anchoredPosition = LayoutPosition_Top[0];
            ProVIdeo.sizeDelta = LayoutVideo_Top[0];
            
            FrontMask.anchoredPosition = LayoutPosition_Btn[0];
            FrontMask.sizeDelta = LayoutMask_Btm[0];
            FrontVIdeo.sizeDelta = LayoutVideo_Btm[0];
        }
        else if (index == 1) //������ ���̵� �Ʒ�
        {
            FrontMask.gameObject.SetActive(false);
            SideMask.gameObject.SetActive(true);            

            ProVIdeo.anchoredPosition = LayoutPosition_Top[0];
            ProVIdeo.sizeDelta = LayoutVideo_Top[0];

            SideMask.anchoredPosition = LayoutPosition_Btn[0];
            SideMask.sizeDelta = LayoutMask_Btm[0];
            SideVIdeo.sizeDelta = LayoutVideo_Btm[0];

            
        }
        else if (index == 2) //������ ����Ʈ/���̵� �Ʒ�
        {
            ProVIdeo.anchoredPosition = LayoutPosition_Top[0];
            ProVIdeo.sizeDelta = LayoutVideo_Top[0];

            FrontMask.gameObject.SetActive(true);
            SideMask.gameObject.SetActive(true);

            FrontMask.anchoredPosition = LayoutPosition_Btn[1];
            FrontMask.sizeDelta = LayoutMask_Btm[1];
            FrontVIdeo.sizeDelta = LayoutVideo_Btm[1];

            SideMask.anchoredPosition = LayoutPosition_Btn[2];
            SideMask.sizeDelta = LayoutMask_Btm[1];
            SideVIdeo.sizeDelta = LayoutVideo_Btm[1];
        }
        else if (index == 3) //������ ����Ʈ �Ʒ�Ȯ��
        {
            FrontMask.gameObject.SetActive(true);
            SideMask.gameObject.SetActive(false);

            ProVIdeo.anchoredPosition = LayoutPosition_Top[1];
            ProVIdeo.sizeDelta = LayoutVideo_Top[1];

            FrontMask.anchoredPosition = LayoutPosition_Btn[3];
            FrontMask.sizeDelta = LayoutMask_Btm[2];
            FrontVIdeo.sizeDelta = LayoutVideo_Btm[2];
        }
        else if (index == 4) //������ ���̵� �Ʒ�Ȯ��
        {
            FrontMask.gameObject.SetActive(false);
            SideMask.gameObject.SetActive(true);

            ProVIdeo.anchoredPosition = LayoutPosition_Top[1];
            ProVIdeo.sizeDelta = LayoutVideo_Top[1];

            SideMask.anchoredPosition = LayoutPosition_Btn[3];
            SideMask.sizeDelta = LayoutMask_Btm[2];
            SideVIdeo.sizeDelta = LayoutVideo_Btm[2];
        }
    }
}

