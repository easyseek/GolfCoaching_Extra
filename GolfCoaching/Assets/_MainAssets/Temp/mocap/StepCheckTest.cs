using NUnit.Framework.Interfaces;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class StepCheckTest : MonoBehaviour
{
    public enum SWINGSTEP
    {
        Address = 0,
        TakeBack,
        BackSwing,
        DownSwing,
        Impact,
        Follow,
        Finish,        
        Ready,
        None,
        BackSwingEnd
    }
    SWINGSTEP _Step = SWINGSTEP.None;

    [SerializeField] Transform DebugObject;

    [SerializeField] Animator _Animator;
    
    [SerializeField] Transform LeftHand;
    [SerializeField] Transform RightHand;

    [SerializeField] Transform LeftPelvis;
    [SerializeField] Transform RightPelvis;

    [SerializeField] Transform LeftShoulder;
    [SerializeField] Transform RightShoulder;

    [SerializeField] Transform LeftFoot;
    [SerializeField] Transform RightFoot;

    [SerializeField] Transform Head;

    private float HandAngle = 0;
    private float HandAngleHist = 0;

    private float CheckTimer = 0;
    private float CheckTimerValue = 0;

    Vector3 _midPelvis;
    Vector3 _misHands;
    Vector3 _misShoulder;

    public CalKinetic calKinetic;
    public mocap mocap;
    public bool PoseLock = false;

    [SerializeField] TMPro.TextMeshProUGUI txtStatus;

    [SerializeField] ModelPoseController[] modelPoseControllers;

    [SerializeField] Image[] imgPoseButtons;
    [SerializeField] Transform[] CamPositions;
    [SerializeField] Transform Camera3D;
    [SerializeField] Transform ShadowRoot;
    [SerializeField] Transform UserRoot;
    float _speed = 200f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
        if(ShadowRoot != null && UserRoot != null)
            OnClick_PoseView(7);

        yield return new WaitForSeconds(2f);
        SetStep(SWINGSTEP.Ready);
    }

    // Update is called once per frame
    /*void Update()
    {
        CheckPose();
        GetHandDir();

        if (_Step != SWINGSTEP.None)
        {
            GetShoulderDir();
            GetPelvisDir();            
        }

        GetDragValue();
    }*/

    public SWINGSTEP GetPoseStep()
    {
        return _Step;
    }

    //��� ȸ��
    void GetShoulderDir()
    {
        // ����� �߾��� �������� �ϴ� ���� ���
        Vector3 PelvisVector = RightPelvis.position - LeftPelvis.position;
        Vector3 shoulderVector = RightShoulder.position - LeftShoulder.position;

        // ��� ���Ϳ� ��� ���� ���� ȸ�� ���� ���
        float angle = Vector3.SignedAngle(PelvisVector, shoulderVector, Vector3.forward);
        calKinetic.ShoulderValue = angle / 180f;
    }

    //��� ȸ��
    void GetPelvisDir()
    {
        // ����� �߾��� �������� �ϴ� ���� ���
        Vector3 FootVector = RightFoot.position - LeftFoot.position;
        Vector3 PelvisVector = RightPelvis.position - LeftPelvis.position;

        // ��� ���Ϳ� ��� ���� ���� ȸ�� ���� ���
        float angle = Vector3.SignedAngle(FootVector, PelvisVector, Vector3.forward);
        calKinetic.PelvisValue = angle / 180f;
    }

    void GetHandDir()
    {
        // ����߽ɰ� ���߽��� ����
        Vector3 shoulderVector = (RightShoulder.position + LeftShoulder.position) / 2;
        Vector3 handVector = (RightHand.position + LeftHand.position) /2;
        Vector3 dir = handVector - shoulderVector;
        dir.z = 0;

        // ��� ���Ϳ� �� ���� ���� ���� ���
        HandAngle = Quaternion.FromToRotation(Vector3.up, dir).eulerAngles.z;
        //Debug.Log($"Hand Angle : {HandAngle}");
        Debug.DrawLine(shoulderVector, handVector, Color.red);        
    }

    bool AngleCheck(float from, float to)
    {
        if (HandAngle > from && HandAngle < to)
            return true;
        else
            return false;
    }

    bool AngleCheck(float value, bool isUp)
    {
        if (isUp && HandAngle > value)
            return true;
        else if (!isUp && HandAngle < value)
            return true;
        else
            return false;
    }

    void CheckPose()
    {
        if (PoseLock) return;

        if (_Step == SWINGSTEP.Ready)   //�غ� - ��巹�� ���� �ȵ�
        {
            float hDis = Vector3.Distance(LeftHand.position, RightHand.position);
            if (hDis < 0.2f)
            {
                if (AngleCheck(175, 190))
                {
                    CheckTimer -= Time.deltaTime;
                }
            }
            else
            {
                CheckTimer = CheckTimerValue;
            }
            if (CheckTimer < 0)
            {
                SetStep(SWINGSTEP.Address);
            }
        }
        else if (_Step == SWINGSTEP.Address) //��巹�� �� ����
        {
            if (AngleCheck(175, false))
            {
                SetStep(SWINGSTEP.TakeBack);
            }
        }
        else if (_Step == SWINGSTEP.TakeBack) //����ũ�� �� ����
        {
            if (AngleCheck(100, false))
            {
                HandAngleHist = HandAngle + 5;
                SetStep(SWINGSTEP.BackSwing);
            }
            else
            {
                _Animator.Play(_Step.ToString(), 0, NormalizeValue(HandAngle, 100f, 175f, true));
                //Debug.Log(HandAngle.ToString());
            }
        }
        else if (_Step == SWINGSTEP.BackSwing) //�齺�� �� ����
        {
            _misHands = (LeftHand.position + RightHand.position) / 2;

            if (AngleCheck(80, true))
            {
                //SetStep(SWINGSTEP.DownSwing);
                SetStep(SWINGSTEP.BackSwingEnd);
            }
            else
            {
                if (HandAngleHist < HandAngle)
                {
                    if ((HandAngleHist + 5) < HandAngle)
                    {
                        HandAngleHist = HandAngle;
                        //SetStep(SWINGSTEP.DownSwing);
                        SetStep(SWINGSTEP.BackSwingEnd);
                    }
                }
                else
                {
                    HandAngleHist = HandAngle;
                }

                //else
                {

                    _Animator.Play(_Step.ToString(), 0, NormalizeValue(HandAngle, 40f, 100f, true));
                    //Debug.Log(HandAngle.ToString());
                }
            }
        }
        else if (_Step == SWINGSTEP.BackSwingEnd) //�ٿ�� ���� �ִϸ��̼� ó��
        {
            if (_Animator.GetCurrentAnimatorStateInfo(0).IsName(SWINGSTEP.BackSwing.ToString()) == true)
            {
                float animTime = _Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                if (animTime >= 1.0f)
                {
                    _Step = SWINGSTEP.BackSwing;
                    SetStep(SWINGSTEP.DownSwing);
                }
            }
        }         
        else if (_Step == SWINGSTEP.DownSwing) //�ٿ�� �� ����
        {
            if (AngleCheck(140, true))
            {
                SetStep(SWINGSTEP.Impact);
            }
            else
            {
                //_Animator.Play(_Step.ToString(), 0, NormalizeValue(HandAngle, 80f, 140f, false));
                _Animator.Play(_Step.ToString(), 0, NormalizeValue(HandAngle, HandAngleHist, 140f, false));
                //Debug.Log(HandAngle.ToString());
            }
        }
        else if (_Step == SWINGSTEP.Impact) //����Ʈ �� ����
        {
            if (AngleCheck(180, true))
            {
                SetStep(SWINGSTEP.Follow);
            }
            else
            {
                //_Animator.Play(_Step.ToString(), 0, NormalizeValue(HandAngle, 140f, 190f, false));
                //Debug.Log(HandAngle.ToString());
            }
        }
        else if (_Step == SWINGSTEP.Follow) //�ȷο� �� ����
        {
            if (AngleCheck(270, true))
            {
                SetStep(SWINGSTEP.Finish);
            }
            else
            {
                _Animator.Play(_Step.ToString(), 0, NormalizeValue(HandAngle, 180f, 270f, false));
                //Debug.Log(HandAngle.ToString());
            }
        }
        else if (_Step == SWINGSTEP.Finish) //�ǴϽ� �� ó��
        {
            if (AngleCheck(100, 260))
            {
                SetStep(SWINGSTEP.Ready);
            }
            else
            {
                //_Animator.Play(_Step.ToString(), 0, NormalizeValue(HandAngle, 270f, 360f, false));
                //Debug.Log(HandAngle.ToString());
            }
        }
    }
    /*
    void CheckPose()
    {
        if (_Step == SWINGSTEP.Ready)   //�غ� - ��巹�� ���� �ȵ�
        {
            _midPelvis = (LeftPelvis.position + RightPelvis.position) / 2;
            _misHands = (LeftHand.position + RightHand.position) / 2;
            float hDis = Vector3.Distance(LeftHand.position, RightHand.position);
            if (hDis < 0.2f)
            {
                float pDis = Vector3.Distance(_midPelvis, _misHands);
                if (pDis < 0.5f)
                {                    
                    CheckTimer -= Time.deltaTime;
                }
            }
            else
            {
                CheckTimer = CheckTimerValue;
            }
            if (CheckTimer < 0)
            {
                SetStep(SWINGSTEP.Address);                
            }
        }
        else if (_Step == SWINGSTEP.Address) //��巹�� �� ����
        {
            _midPelvis = (LeftPelvis.position + RightPelvis.position) / 2;
            _misHands = (LeftHand.position + RightHand.position) / 2;

            float dis = Vector3.Distance(_midPelvis, _misHands);
            if (dis > 0.3f)
            {
                SetStep(SWINGSTEP.TakeBack);
            }
        }
        else if (_Step == SWINGSTEP.TakeBack) //����ũ�� �� ����
        {
            _misHands = (LeftHand.position + RightHand.position) / 2;

            if (Head.position.y < _misHands.y)
            {
                SetStep(SWINGSTEP.BackSwing);
            }
        }
        else if (_Step == SWINGSTEP.BackSwing) //�齺�� �� ����
        {
            _misHands = (LeftHand.position + RightHand.position) / 2;

            if (Head.position.y > _misHands.y)
            {
                SetStep(SWINGSTEP.DownSwing);
            }
        }
        else if (_Step == SWINGSTEP.DownSwing) //�ٿ�� �� ����
        {
            _midPelvis = (LeftPelvis.position + RightPelvis.position) / 2;
            _misHands = (LeftHand.position + RightHand.position) / 2;

            _midPelvis = (LeftPelvis.position + RightPelvis.position) / 2;
            _misHands = (LeftHand.position + RightHand.position) / 2;
            float hDis = Vector3.Distance(LeftHand.position, RightHand.position);
            if (hDis < 0.2f)
            {
                float pDis = Vector3.Distance(_midPelvis, _misHands);
                if (pDis < 0.5f)
                {
                    SetStep(SWINGSTEP.Impact);
                }
            }
        }
        else if (_Step == SWINGSTEP.Impact) //����Ʈ �� ����
        {
            _midPelvis = (LeftPelvis.position + RightPelvis.position) / 2;
            _misHands = (LeftHand.position + RightHand.position) / 2;

            if (Vector3.Distance(_midPelvis, _misHands) > 0.2f)
            {
                SetStep(SWINGSTEP.Follow);
            }
        }
        else if (_Step == SWINGSTEP.Follow) //�ȷο� �� ����
        {
            _misHands = (LeftHand.position + RightHand.position) / 2;

            if (Head.position.y < _misHands.y)
            {
                SetStep(SWINGSTEP.Finish);
            }
        }
        else if (_Step == SWINGSTEP.Finish) //�ǴϽ� �� ó��
        {
            _misHands = (LeftHand.position + RightHand.position) / 2;

            if (Head.position.y > _misHands.y)
            {
                SetStep(SWINGSTEP.Ready);
            }
            
        }
    }*/

    float NormalizeValue(float value, float min, float max, bool isReverse)
    {
        if(isReverse)
            return 1 - Mathf.Clamp01((value - min) / (max - min));
        else
            return Mathf.Clamp01((value - min) / (max - min));
    }


    bool useSnap = true;
    void SetStep(SWINGSTEP step)
    {
        Debug.Log($"SetStep() {_Step.ToString()} >> {step.ToString()}");

        if (step == SWINGSTEP.BackSwingEnd)
        {
            _Step = step;
            return;
        }

        if(txtStatus != null)
            txtStatus.text = _Step.ToString();
        _Animator.Play(step.ToString());

        if (step == SWINGSTEP.Ready)
        {
            CheckTimer = CheckTimerValue = 0.3f;
        }
        else if (step == SWINGSTEP.Address)
        {
            CheckTimer = CheckTimerValue = 2f;            
        }
        else if (step == SWINGSTEP.TakeBack)
        {
            SetModelPoseSnap();
        }
        else if (step == SWINGSTEP.BackSwing)
        {
            SetModelPoseSnap();
        }
        else if (step == SWINGSTEP.DownSwing)
        {
            SetModelPoseSnap();
        }
        else if (step == SWINGSTEP.Impact)
        {   
            SetModelPoseSnap();
        }
        else if (step == SWINGSTEP.Follow)
        {
            SetModelPoseSnap();
        }
        else if (step == SWINGSTEP.Finish)
        {
            SetModelPoseSnap();
            StartCoroutine(FinishCheck());

        }

        _Step = step;
        if (imgPoseButtons.Length > 0)
        {
            if ((int)_Step < 7 && imgPoseButtons[(int)_Step] != null)
                imgPoseButtons[(int)_Step].color = Color.green;
        }
        //StartCoroutine(CheckAniEnd());
    }

    IEnumerator FinishCheck()
    {
        bool isEnd = false;
        while (!isEnd)
        {
            if (_Animator.GetCurrentAnimatorStateInfo(0).IsName("Finish") == true)
            {
                // ���ϴ� �ִϸ��̼��̶�� �÷��� ������ üũ
                float animTime = _Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;
                if (animTime >= 1.0f)
                {
                    
                    // �ִϸ��̼� ����
                    SetModelPoseSnap();
                    //Debug.LogError("FinishCheck Done");
                    isEnd = true;
                    useSnap = false;
                }
            }
            yield return null;
        }
    }

    void SetModelPoseSnap()
    {
        if (useSnap == false) return;

        modelPoseControllers[(int)_Step].gameObject.SetActive(true);
        modelPoseControllers[(int)_Step].SetChest(calKinetic.tarTr[3].localRotation);
        modelPoseControllers[(int)_Step].SetPelvis(calKinetic.PelvisObject.localRotation);
    }

    void ClearModelPoseSnap()
    {
        for(int i = 0; i < modelPoseControllers.Length; i++) 
            modelPoseControllers[i].gameObject.SetActive(false);
    }

    IEnumerator CheckAniEnd()
    {

        while(_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            yield return null;
        }
        GetShoulderDir();
    }

    public bool CheckAddress()
    {
        //if (mocap.visibleCount < 10) return false;
        float hDis = Vector3.Distance(LeftHand.position, RightHand.position);
        if (hDis < 0.2f)
        {
            if (AngleCheck(175, 190))
            {
                return true;
            }
            else
                return false;
        }
        else
        {
            return false;
        }
    }

    public void OnClick_PoseView(int index)
    {
        ShadowRoot.rotation = Quaternion.identity;
        UserRoot.rotation = Quaternion.identity;

        Camera3D.position = CamPositions[index].position;
        Debug.Log($"OnClick_PoseView({index}) - {CamPositions[index].position}");
    }

    void GetDragValue()
    {
        if(ShadowRoot != null && UserRoot != null)
        //Mouse
        if (Input.GetMouseButton(0))
        {
            //if (EventSystem.current.IsPointerOverGameObject())
            //    return;

            float yAngle = Input.GetAxis("Mouse X") * _speed * Time.deltaTime;

            ShadowRoot.transform.Rotate(0, yAngle, 0, Space.World);
            UserRoot.transform.Rotate(0, yAngle, 0, Space.World);
        }
    }
}
