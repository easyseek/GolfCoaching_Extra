using Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class RecordingDirector : MonoBehaviour
{
    enum EProfileStep
    {
        GRIP,
        READY,
        SWING,
        RESULT
    }

    enum ELessonStep
    {
        CHECK,
        READY,
        RECORDING,
        RESULT
    }

    enum EPracticeStep
    {
        GRIP,
        SWING,
        RESULT
    }

    [SerializeField] private GameObject m_ProfilePanel;
    [SerializeField] private GameObject m_LessonPanel;
    [SerializeField] private GameObject m_PracticePanel;
    [SerializeField] private GameObject m_TopObj;

    [Header("------------------------------ Profile ------------------------------")]
    [Header("* 1.GRIP")]
    [SerializeField] private GameObject m_ProfileGribPanel;

    [Header("* 2.READY")]
    [SerializeField] private GameObject m_ProfileReadyPanel;

    [SerializeField] private TextMeshProUGUI m_ProfileCountText;

    [Header("* 3.SWING")]
    [SerializeField] private GameObject m_ProfileSwingPanel;

    [Header("* 3.RESULT")]
    [SerializeField] private GameObject m_ProfileResultPanel;

    [Header("------------------------------ Lesson ------------------------------")]
    [Header("* 1.CHECK")]
    [SerializeField] private GameObject m_CheckPanel;

    [SerializeField] private TextMeshProUGUI m_CountText;

    [Header("* 2.READY")]
    [SerializeField] private GameObject m_ReadyPanel;

    [Header("* 3.RECORDING")]
    [SerializeField] private GameObject m_RecordingPanel;
    [SerializeField] private GameObject m_LoadingPanel;

    [SerializeField] private Image m_RedDot;

    [SerializeField] private TextMeshProUGUI m_TimerText;

    private float blinkTime = 0.5f;
    private float blinkTimer;

    [Header("* 4.RESULT")]
    [SerializeField] private GameObject m_LessonResultPanel;

    [SerializeField] private RawImage m_LessonVideoThumbnail;

    [SerializeField] private TextMeshProUGUI m_VideoTimeText;

    [SerializeField] private TMP_InputField m_SubjectInput;

    [Space]
    [Header("------------------------------ Practice ------------------------------")]

    [Header("* 1.GRIP")]
    [SerializeField] private GameObject m_GuidePanel;

    [Header("* 2.SWING")]
    [SerializeField] private GameObject m_SwingPanel;
    [SerializeField] private GameObject m_SwingProgress;
    [SerializeField] private GameObject m_Check;

    [SerializeField] private Image m_ProgressBarImg;
    [SerializeField] private Image m_ClubImg;

    [SerializeField] private Toggle[] m_StepToggles;

    [SerializeField] private Sprite[] m_ClubSprites;

    [SerializeField] private TextMeshProUGUI m_InfoText;

    [Header("* 3.RESULT")]
    [SerializeField] private SwingCardViewer m_SwingCardViewer;

    [SerializeField] private PopupControl m_PopupControl;

    [SerializeField] private GameObject m_PracticeResultPanel;
    [SerializeField] private GameObject m_Main;
    [SerializeField] private GameObject m_SelectRetake;
    [SerializeField] private GameObject m_ReviewCancle;
    [SerializeField] private GameObject[] m_CaptureChecks;
    [SerializeField] private GameObject[] m_CaptureObjs;

    [SerializeField] private Toggle[] m_CheckToggles;

    [SerializeField] private RawImage[] m_CaptureRawImages;
    [SerializeField] private Image m_InfoImage;

    [SerializeField] private Sprite[] m_InfoSprites;

    [SerializeField] private TextMeshProUGUI m_ResultInfoText;
    [SerializeField] private TextMeshProUGUI m_ResultInfoSubText;
    [SerializeField] private TextMeshProUGUI m_ClubTypeText;

    [Header("* MOCAP")]
    //[SerializeField] mocapFront mocapFront;
    //[SerializeField] mocapSide mocapSide;
    [SerializeField] SensorProcess m_SensorProcess;
    [SerializeField] private TextMeshProUGUI m_AvgFrontText;
    [SerializeField] private TextMeshProUGUI m_AvgSideText;
    [SerializeField] private TextMeshProUGUI m_HandText;

    [Header("* VIDEO REF.")]
    [SerializeField] webcamclient webcamclient;
    [SerializeField] RawImage rawImageFront;
    [SerializeField] RawImage rawImageSide;

    private Texture2D captureFront = null;
    //private Texture2D captureSide = null;
    private List<Texture2D> framesFront = new();
    private List<Texture2D> framesSide = new();
    private List<Texture2D> captureRealPoseFrontPro = Enumerable.Repeat<Texture2D>(null, 8).ToList();
    private List<Texture2D> captureRealPoseSidePro = Enumerable.Repeat<Texture2D>(null, 8).ToList();
    private Dictionary<SWINGSTEP, Texture2D> captureFrontDic = new Dictionary<SWINGSTEP, Texture2D>();
    private Dictionary<SWINGSTEP, Texture2D> captureSideDic = new Dictionary<SWINGSTEP, Texture2D>();
    private List<SWINGSTEP> selectSwingStep = new List<SWINGSTEP>();

    //Front
    List<int> saveData_GetHandDir = new List<int>();
    List<int> saveData_GetHandDistance = new List<int>();
    List<int> saveData_GetShoulderDistance = new List<int>();
    List<int> saveData_GetSpineDir = new List<int>();
    List<int> saveData_GetShoulderAngle = new List<int>();
    List<int> saveData_GetFootDisRate = new List<int>();
    List<int> saveData_GetWeight = new List<int>();
    List<int> saveData_GetForearmAngle = new List<int>();
    List<int> saveData_GetElbowFrontDir = new List<int>();
    List<int> saveData_GetElbowRightFrontDir = new List<int>();

    //Side
    List<int> saveData_GetHandSideDir = new List<int>();
    List<int> saveData_GetWaistSideDir = new List<int>();
    List<int> saveData_GetKneeSideDir = new List<int>();
    List<int> saveData_GetElbowSideDir = new List<int>();
    List<int> saveData_GetArmpitDir = new List<int>();

    //combine
    List<int> saveData_GetShoulderDir = new List<int>();
    List<int> saveData_GetPelvisDir = new List<int>();

    Dictionary<string, int[]> ResultProData = new Dictionary<string, int[]>();

    // front
    int _iGetHandDir;
    int _iGetHandDistance;
    int _iGetShoulderDistance;
    int _iGetSpineDir;
    int _iGetShoulderAngle;
    int _iGetFootDisRate;
    int _iGetWeight;
    int _iGetForearmAngle;
    int _iGetElbowFrontDir;
    int _iGetElbowRightFrontDir;

    //Side
    int _iGetHandSideDir;
    int _iGetWaistSideDir;
    int _iGetKneeSideDir;
    int _iGetElbowSideDir;
    int _iGetArmpitDir;

    //combine
    int _iGetShoulderDir;
    int _iGetPelvisDir;

    private bool isRecording = false;
    bool isFinish = false;
    bool isSelectRetake = false;
    bool isReviewing = false;

    int widthFront;
    int heightFront;
    int widthSide;
    int heightSide;
    int curStepNum = 0;

    bool _replayReadyFront = false;
    bool _replayReadySide = false;
    int fps = 30;

    string ffmpegPath = string.Empty;

    float _lastHandDir;
    bool _handCheck = false;
    bool checkTakeback = false;
    bool checkImpact = false;
    float AvgVisible = 0;
    float _invisibleTimer = 0.2f;

    int checkTakebackFrame = 0;

    Vector3 _yFlip = new Vector3(1, -1, 1);

    readonly string reviewStr = "검토를 요청하면 수정이 어렵습니다.\r\n진행하시겠습니까?";
    readonly string retakeStr = "다시 촬영하면 이전 영상이 삭제됩니다.\r\n진행하시겠습니까?";
    readonly string backStr = "뒤로가면 영상이 삭제됩니다.\r\n진행하시겠습니까?";
    readonly string reviewCancleStr = "검토를 취소하면 영상이 삭제됩니다.\r\n진행하시겠습니까?";
    readonly string selectSwingStepStr = "다시 촬영할 자세를\r\n선택해주세요.";
    readonly string confirmReviewStr = "해당 영상이\r\n검토중으로 변경되었습니다.";
    readonly string recordingStopStr = "촬영 종료 시 영상이 삭제됩니다.\r\n계속하시겠습니까?";
    readonly string saveVideoStr = "촬영된 영상을 저장합니다.\r\n계속하시겠습니까?";

    EProfileStep profileStep = EProfileStep.GRIP;
    ELessonStep lessonStep = ELessonStep.CHECK;
    EPracticeStep practiceStep = EPracticeStep.GRIP;
    ESwingType swingType = ESwingType.Full;
    EClub club = EClub.MiddleIron;
    [SerializeField] ERecordingType recordingType = ERecordingType.None;

    string videoTimeStr;
    string path;
    string imagePath;
    string videoPath;
    string videoCSVPath;
    string profileFrontPath;
    string profileSidePath;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Init();

        StartCoroutine(CoLandmarkProcesss());

        StartCoroutine(CheckStepCoroutine());
    }

    private void Update()
    {
        //front
        //_iGetHandDir = (int)mocapFront.GetHandDir();
        _iGetHandDir = m_SensorProcess.iGetHandDir;

        if (practiceStep == EPracticeStep.SWING)
        {
            //front
            _iGetHandDistance = m_SensorProcess.iGetHandDistance;
            _iGetShoulderDistance = m_SensorProcess.iGetShoulderDistance;

            _iGetSpineDir = m_SensorProcess.iGetSpineDir;
            _iGetShoulderAngle = m_SensorProcess.iGetShoulderAngle;
            _iGetFootDisRate = m_SensorProcess.iGetFootDisRate;
            _iGetWeight = m_SensorProcess.iGetWeight;
            _iGetForearmAngle = m_SensorProcess.iGetForearmAngle;
            _iGetElbowFrontDir = m_SensorProcess.iGetElbowFrontDir;
            _iGetElbowRightFrontDir = m_SensorProcess.iGetElbowRightFrontDir;

            //Side
            _iGetHandSideDir = m_SensorProcess.iGetHandSideDir;
            _iGetWaistSideDir = m_SensorProcess.iGetWaistSideDir;
            _iGetKneeSideDir = m_SensorProcess.iGetKneeSideDir;
            _iGetElbowSideDir = m_SensorProcess.iGetElbowSideDir;
            _iGetArmpitDir = m_SensorProcess.iGetArmpitDir;

            //combine
            _iGetShoulderDir = m_SensorProcess.iGetShoulderDir;
            _iGetPelvisDir = m_SensorProcess.iGetPelvisDir;
        }


        //m_AvgFrontText.text = $"{(mocapFront.AvgVisibility * 100f).ToString("00")}%";
        //m_AvgSideText.text = $"{(mocapSide.AvgVisibility * 100f).ToString("00")}%";
    }

    private void Init()
    {
        recordingType = GameManager.Instance.RecordingType;

        if (recordingType == ERecordingType.Profile)
        {
            m_ProfilePanel.SetActive(true);
        }
        else if (recordingType == ERecordingType.Lesson)
        {
            rawImageSide.gameObject.SetActive(false);
            m_LessonPanel.SetActive(true);
        }
        else if (recordingType == ERecordingType.Practice)
        {
            m_PracticePanel.SetActive(true);
        }

        //path = Application.dataPath + "/Record";
        path = @$"C:\DataBase\ProSwing\{GolfProDataManager.Instance.SelectProData.uid}";

        videoPath = @$"C:\DataBase\ProVideo\{GolfProDataManager.Instance.SelectProData.uid}";
        videoPath = GetUniqueFilePath(videoPath, "driver_full", "mp4");
        //videoPath = Application.dataPath + "/Record" + $"/{GolfProDataManager.Instance.SelectProData.uid}";
        videoCSVPath = @$"C:\DataBase\ProVideo\{GolfProDataManager.Instance.SelectProData.uid}\{GolfProDataManager.Instance.SelectProData.uid}.csv";

        imagePath = @$"C:\DataBase\ProImage\{GolfProDataManager.Instance.SelectProData.uid}";
        //imagePath = Application.dataPath + "/Record" + $"/{GolfProDataManager.Instance.SelectProData.uid}";

        profileFrontPath = @$"C:\DataBase\ProVideo\{GolfProDataManager.Instance.SelectProData.uid}\front_video.mp4";
        profileSidePath = @$"C:\DataBase\ProVideo\{GolfProDataManager.Instance.SelectProData.uid}\side_video.mp4";

        //if (!Directory.Exists(imagePath))
        //{
        //    Directory.CreateDirectory(imagePath);
        //}

        //if (Directory.Exists(imagePath))
        //{
        //    isReviewing = true;
        //}

        ffmpegPath = Path.Combine(Application.dataPath, "Plugins", "ffmpeg", "bin", "ffmpeg.exe");

        for (int i = 0; i < m_CheckToggles.Length; i++)
        {
            int index = i;
            m_CheckToggles[i].onValueChanged.AddListener((isOn) => OnValueChanged_Check(index, isOn));
        }

        m_SubjectInput.onSelect.AddListener(_ => KeyboardService.Instance.Show(m_SubjectInput));

        swingType = GameManager.Instance.SwingType;
        club = GameManager.Instance.Club;
    }

    void SetReady()
    {
        if (recordingType == ERecordingType.Profile)
        {
            _replayReadyFront = false;
            _replayReadySide = false;
        }
        else if (recordingType == ERecordingType.Lesson)
        {
            m_TopObj.SetActive(true);
        }
        else if (recordingType == ERecordingType.Practice)
        {
            foreach (var toggle in m_StepToggles)
            {
                toggle.gameObject.SetActive(true);
            }

            _invisibleTimer = 0.2f;

            if (!isSelectRetake)
            {
                foreach (var obj in m_CaptureObjs)
                {
                    obj.SetActive(false);
                }

                captureFrontDic.Clear();
                captureSideDic.Clear();
            }

            m_ProgressBarImg.fillAmount = 0.0f;
            m_Check.SetActive(false);
            m_SwingProgress.SetActive(false);
            m_InfoText.text = string.Empty;
            m_TopObj.SetActive(true);
        }
    }

    private IEnumerator CheckStepCoroutine()
    {
        while (true)
        {
            yield return null;

            if (recordingType == ERecordingType.Profile)
            {
                var currentStep = profileStep;

                switch (currentStep)
                {
                    case EProfileStep.GRIP:
                        yield return StartCoroutine(HandleProfileGribStep());
                        break;

                    case EProfileStep.READY:
                        yield return StartCoroutine(HandleProfileReadyStep());
                        break;

                    case EProfileStep.SWING:
                        yield return StartCoroutine(HandleProfileSWINGStep());
                        break;

                    case EProfileStep.RESULT:
                        yield return StartCoroutine(HandleProfileResultStep());
                        break;
                }
            }
            else if (recordingType == ERecordingType.Lesson)
            {
                var currentStep = lessonStep;

                switch (currentStep)
                {
                    case ELessonStep.CHECK:
                        yield return StartCoroutine(HandleCheckStep());
                        break;

                    case ELessonStep.READY:
                        yield return StartCoroutine(HandleReadyStep());
                        break;

                    case ELessonStep.RECORDING:
                        yield return StartCoroutine(HandleRecordingStep());
                        break;

                    case ELessonStep.RESULT:
                        yield return StartCoroutine(HandleLessonResultStep());
                        break;
                }
            }
            else if (recordingType == ERecordingType.Practice)
            {
                var currentStep = practiceStep;

                switch (currentStep)
                {
                    case EPracticeStep.GRIP:
                        yield return StartCoroutine(HandleGripStep());
                        break;

                    case EPracticeStep.SWING:
                        yield return StartCoroutine(HandleSwingStep());
                        break;

                    case EPracticeStep.RESULT:
                        yield return StartCoroutine(HandleResultStep());
                        break;
                }
            }
        }
    }

    #region Profile
    private IEnumerator HandleProfileGribStep()
    {
        float timer = 0f;

        while (profileStep == EProfileStep.GRIP)
        {
            if ((m_SensorProcess.iGetHandDir > 160 && m_SensorProcess.iGetHandDir < 200)
                        && (m_SensorProcess.IsAddressHand())
                        && (m_SensorProcess.visibilityFront > 0.8f && m_SensorProcess.visibilitySide > 0.7f))
            {
                timer += Time.deltaTime;

                if (timer > 0.8f)
                {
                    SetRecordingStep(EProfileStep.READY);

                    yield break;
                }
            }
            else
            {
                timer = 0f;
            }

            yield return null;
        }
    }

    private IEnumerator HandleProfileReadyStep()
    {
        float timer = 3f;

        while (profileStep == EProfileStep.READY)
        {
            timer -= Time.deltaTime;

            m_ProfileCountText.text = $"{timer:0}";

            if (timer < 0)
            {
                SetRecordingStep(EProfileStep.SWING);
            }

            yield return null;
        }
    }

    private IEnumerator HandleProfileSWINGStep()
    {
        string str = string.Empty;
        float timer = 0;
        isRecording = true;
        StartCoroutine(CaptureFramesProfile());

        while (profileStep == EProfileStep.SWING)
        {
            if (isRecording)
            {
                if (checkTakeback == false && _handCheck && m_SensorProcess.iGetHandDir < 150f)
                {
                    checkTakeback = true;
                }

                if (checkTakeback == true && checkImpact == false && _lastHandDir > 170f)
                {
                    checkImpact = true;
                }
            }

            yield return null;
        }
    }

    private IEnumerator HandleProfileResultStep()
    {
        float timer = 0.0f;

        m_LoadingPanel.SetActive(true);

        StartCoroutine(SendFramesToFFmpeg(profileFrontPath, widthFront, heightFront, framesFront, () => _replayReadyFront = true));
        StartCoroutine(SendFramesToFFmpeg(profileSidePath, widthSide, heightSide, framesSide, () => _replayReadySide = true));

        while (profileStep == EProfileStep.RESULT)
        {
            yield return new WaitUntil(() => _replayReadyFront == true && _replayReadySide == true);

            m_LoadingPanel.SetActive(false);

            Back();

            yield return null;
        }
    }
    #endregion

    #region Lesson
    private IEnumerator HandleCheckStep()
    {
        SetReady();

        while (lessonStep == ELessonStep.CHECK)
        {
            yield return null;
        }
    }

    private IEnumerator HandleReadyStep()
    {
        float timer = 3f;

        while (lessonStep == ELessonStep.READY)
        {
            timer -= Time.deltaTime;

            m_CountText.text = $"{timer:0}";

            if (timer < 0)
            {
                SetRecordingStep(ELessonStep.RECORDING);
            }

            yield return null;
        }
    }

    private IEnumerator HandleRecordingStep()
    {
        string str = string.Empty;
        float timer = 0;
        isRecording = true;
        StartCoroutine(CaptureFrames());

        while (lessonStep == ELessonStep.RECORDING)
        {
            if (isRecording)
            {
                timer += Time.deltaTime;
                int hour = Mathf.FloorToInt(timer / 3600);
                int min = Mathf.FloorToInt((timer % 3600) / 60);
                int sec = Mathf.FloorToInt(timer % 60);

                str = $"{hour:00}:{min:00}:{sec:00}";
                m_TimerText.text = str;

                blinkTimer += Time.deltaTime;

                if (blinkTimer >= blinkTime)
                {
                    m_RedDot.enabled = !m_RedDot.enabled;
                    blinkTimer = 0.0f;
                }
            }
            else
            {
                //yield return new WaitUntil(() => _replayReadyFront == true);
                yield return new WaitForSeconds(3.0f);

                videoTimeStr = str.Substring(str.IndexOf(':') + 1);
                m_TopObj.SetActive(false);
                m_LoadingPanel.SetActive(false);
                SetRecordingStep(ELessonStep.RESULT);
            }

            yield return null;
        }
    }

    private IEnumerator HandleLessonResultStep()
    {
        float timer = 0.0f;

        m_LessonVideoThumbnail.texture = captureFront;
        m_VideoTimeText.text = videoTimeStr;

        m_SubjectInput.text = fileName.Replace(".mp4", "");

        while (lessonStep == ELessonStep.RESULT)
        {


            yield return null;
        }
    }
    #endregion

    #region Practice
    private IEnumerator HandleGripStep()
    {
        SetReady();

        List<SWINGSTEP> swingStep = isSelectRetake ? selectSwingStep : GameManager.Instance.Stance;

        float timer = 0f;

        while (practiceStep == EPracticeStep.GRIP)
        {
            if ((m_SensorProcess.iGetHandDir > 160 && m_SensorProcess.iGetHandDir < 200)
                        && (m_SensorProcess.IsAddressHand())
                        && (m_SensorProcess.visibilityFront > 0.8f && m_SensorProcess.visibilitySide > 0.7f))
            {
                timer += Time.deltaTime;

                if (timer > 0.8f)
                {
                    m_ClubImg.sprite = m_ClubSprites[(int)club];

                    for (int i = 0; i < m_StepToggles.Length; i++)
                    {
                        if (!swingStep.Contains((SWINGSTEP)i))
                        {
                            m_StepToggles[i].gameObject.SetActive(false);
                        }

                    }

                    SetResultData();

                    SetRecordingStep(EPracticeStep.SWING);

                    yield break;
                }
            }
            else
            {
                timer = 0f;
                //Debug.Log($"{m_SensorProcess.iGetHandDir}/{m_SensorProcess.iGetHandDistance}/{m_SensorProcess.visibilityFront}/{m_SensorProcess.visibilitySide}");
            }

            yield return null;
        }
    }

    private IEnumerator HandleSwingStep()
    {
        List<SWINGSTEP> swingStep = isSelectRetake ? selectSwingStep : GameManager.Instance.Stance;

        float timer = 0f;

        foreach (var step in swingStep)
        {
            m_StepToggles[(int)step].isOn = true;

            m_InfoText.text = $"{Utillity.Instance.ConvertEnumToString(step)} 자세를 잡아주세요";

            yield return new WaitForSeconds(2.0f);

            SaveData_Clear();

            m_InfoText.text = $"{Utillity.Instance.ConvertEnumToString(step)} 자세를 인식중입니다...";

            m_SwingProgress.SetActive(true);

            while (true)
            {
                timer += Time.deltaTime;

                m_ProgressBarImg.fillAmount = Mathf.Clamp01(timer / 2.0f);

                PoseRecored();

                if (timer >= 2.0f)
                {
                    timer = 0.0f;
                    m_InfoText.text = $"{Utillity.Instance.ConvertEnumToString(step)} 완료!";
                    m_Check.SetActive(true);

                    CaptureFrameFront(step);
                    CaptureFrameSide(step);

                    AudioManager.Instance.PlayNext();
                    break;
                }

                //if (mocapFront.AvgVisibility < 0.3f || mocapSide.AvgVisibility < 0.2f)
                //{
                //    _invisibleTimer -= Time.deltaTime;

                //    if (_invisibleTimer <= 0f)
                //    {
                //        _invisibleTimer = 0.2f;
                //        SetRecordingStep(EPracticeStep.GRIP);
                //        yield break;
                //    }
                //}

                yield return null;
            }

            yield return new WaitForSeconds(0.5f);

            m_ProgressBarImg.fillAmount = 0.0f;
            m_Check.SetActive(false);
            m_SwingProgress.SetActive(false);
            m_InfoText.text = string.Empty;

            ResultDataAdd((int)step);

            if (step == swingStep.Last())
            {
                if (SaveCsvFull())
                {
                    SetRecordingStep(EPracticeStep.RESULT);
                }
                else
                {
                    SetRecordingStep(EPracticeStep.GRIP);
                }

                yield break;
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator HandleResultStep()
    {
        SetResult();
        while (practiceStep == EPracticeStep.RESULT)
        {
            yield return null;
        }
    }

    private void SetResult()
    {
        m_TopObj.SetActive(false);
        m_Main.SetActive(!isReviewing);
        m_SelectRetake.SetActive(false);
        m_ReviewCancle.SetActive(isReviewing);

        m_InfoImage.sprite = isReviewing ? m_InfoSprites[1] : m_InfoSprites[0];
        m_ResultInfoText.text = isReviewing ? "영상을 검토중입니다." : "영상촬영이 완료되었습니다.";
        m_ResultInfoSubText.text = isReviewing ? "영업일 기준 최대 2일 이내 검토가 완료됩니다." : "데이터 분석을 위해 검토를 요청해주세요.";

        if (isReviewing)
        {
            m_InfoImage.sprite = m_InfoSprites[1];
            m_InfoText.text = "영상을 검토중입니다.";
        }
        else
        {
            m_InfoImage.sprite = m_InfoSprites[0];
            m_InfoText.text = "영상을 검토중입니다.";
        }

        List<SwingCardViewer.CardData> newList = new List<SwingCardViewer.CardData>();

        if (!isSelectRetake)
        {
            int cnt = 0;

            foreach (var step in GameManager.Instance.Stance)
            {
                int iStep = (int)step;

                m_CaptureObjs[iStep].SetActive(true);

                m_CaptureRawImages[(int)step].texture = captureFrontDic[step];

                newList.Add(new SwingCardViewer.CardData(Utillity.Instance.ConvertEnumToString(step), captureFrontDic[step]));
            }

            m_SwingCardViewer.SetCardList(newList);

            foreach (var step in GameManager.Instance.Stance)
            {
                int iStep = (int)step;
                int localIndex = cnt;

                m_CaptureObjs[iStep].GetComponent<Button>().onClick.RemoveAllListeners();
                m_CaptureObjs[iStep].GetComponent<Button>().onClick.AddListener(() => m_SwingCardViewer.ShowAtIndex(localIndex));

                cnt++;
            }
        }
        else
        {
            foreach (var step in selectSwingStep)
            {
                m_CaptureRawImages[(int)step].texture = captureFrontDic[step];
            }

            foreach (var step in GameManager.Instance.Stance)
            {
                newList.Add(new SwingCardViewer.CardData(step.ToString(), captureFrontDic[step]));
            }

            m_SwingCardViewer.SetCardList(newList);
        }

        for (int i = 0; i < m_CaptureChecks.Length; i++)
        {
            m_CaptureChecks[i].SetActive(false);
            m_CheckToggles[i].isOn = false;
        }
    }
    #endregion

    private void SetRecordingStep(Enum filter)
    {
        if (filter is ELessonStep LessonStep)
        {
            switch (filter)
            {
                case ELessonStep.CHECK:
                case ELessonStep.READY:
                case ELessonStep.RECORDING:
                case ELessonStep.RESULT:
                    NewShowPanel(LessonStep);

                    lessonStep = LessonStep;
                    break;

            }
        }
        else if (filter is EPracticeStep PracticeStep)
        {
            switch (filter)
            {
                case EPracticeStep.GRIP:
                case EPracticeStep.SWING:
                case EPracticeStep.RESULT:
                    NewShowPanel(filter);

                    practiceStep = PracticeStep;
                    break;

            }
        }
        else if (filter is EProfileStep ProfileStep)
        {
            switch (filter)
            {
                case EProfileStep.GRIP:
                case EProfileStep.READY:
                case EProfileStep.SWING:
                case EProfileStep.RESULT:
                    NewShowPanel(filter);

                    profileStep = ProfileStep;
                    break;
            }
        }
    }

    private void NewShowPanel(Enum filter)
    {
        if (filter is ELessonStep)
        {
            m_CheckPanel.SetActive(false);
            m_ReadyPanel.SetActive(false);
            m_RecordingPanel.SetActive(false);
            m_LessonResultPanel.SetActive(false);

            switch (filter)
            {
                case ELessonStep.CHECK:
                    m_CheckPanel.SetActive(true);
                    break;

                case ELessonStep.READY:
                    m_ReadyPanel.SetActive(true);
                    break;

                case ELessonStep.RECORDING:
                    m_RecordingPanel.SetActive(true);
                    break;

                case ELessonStep.RESULT:
                    m_LessonResultPanel.SetActive(true);
                    break;
            }
        }
        else if (filter is EPracticeStep)
        {
            m_GuidePanel.SetActive(false);
            m_SwingPanel.SetActive(false);
            m_PracticeResultPanel.SetActive(false);

            switch (filter)
            {
                case EPracticeStep.GRIP:
                    m_GuidePanel.SetActive(true);
                    break;

                case EPracticeStep.SWING:
                    m_InfoText.text = string.Empty;
                    m_SwingPanel.SetActive(true);
                    break;

                case EPracticeStep.RESULT:
                    m_PracticeResultPanel.SetActive(true);
                    //SceneManager.LoadScene(GameManager.Instance.SelectedSceneName);
                    break;
            }
        }
        else if (filter is EProfileStep)
        {
            m_ProfileGribPanel.SetActive(false);
            m_ProfileReadyPanel.SetActive(false);
            m_ProfileSwingPanel.SetActive(false);
            m_ProfileResultPanel.SetActive(false);

            switch (filter)
            {
                case EProfileStep.GRIP:
                    m_ProfileGribPanel.SetActive(true);
                    break;

                case EProfileStep.READY:
                    m_ProfileReadyPanel.SetActive(true);
                    break;

                case EProfileStep.SWING:
                    m_ProfileSwingPanel.SetActive(true);
                    break;

                case EProfileStep.RESULT:
                    //m_ProfileResultPanel.SetActive(true);
                    break;
            }
        }
    }

    public void ProceedReview()
    {
        Debug.Log($"검토중");
        //isReviewing = true;

        //if (!Directory.Exists(imagePath))
        //{
        //    Directory.CreateDirectory(imagePath);
        //}

        //SetResult();

        if (recordingType == ERecordingType.Lesson)
        {
            StartCoroutine(SaveRecordingVideo());
        }
        else if (recordingType == ERecordingType.Practice)
        {
            //m_PopupControl.ShowTopPanel(confirmReviewStr);

            SaveAllImages();
            GolfProDataManager.Instance.ReloadProSwingData();

            Back();
        }
    }

    public void ProceedRetake()
    {
        isSelectRetake = false;

        selectSwingStep.Clear();

        SetRecordingStep(EPracticeStep.GRIP);
    }

    public void ProceedSelectRetake()
    {
        isSelectRetake = true;

        SetRecordingStep(EPracticeStep.GRIP);
    }

    public void CancleReview()
    {
        isReviewing = false;

        isSelectRetake = false;
        SetRecordingStep(EPracticeStep.GRIP);
    }

    public void Back()
    {
        SceneManager.LoadScene(GameManager.Instance.SelectedSceneName);
    }

    public void Onclick_Button(string name)
    {
        switch (name)
        {
            case "Home":
                GameManager.Instance.SelectedSceneName = string.Empty;
                SceneManager.LoadScene("ModeSelect");
                break;

            case "Back":
                if (recordingType == ERecordingType.Lesson)
                {
                    m_PopupControl.ShowPopup(recordingStopStr, Utillity.Instance.HexToRGB(INI.Red), Back);
                }
                else if (recordingType == ERecordingType.Practice)
                {
                    if (practiceStep == EPracticeStep.GRIP || practiceStep == EPracticeStep.SWING)
                    {
                        Back();
                    }
                    else
                    {
                        if (isReviewing)
                            Back();
                        else
                            m_PopupControl.ShowPopup(backStr, Utillity.Instance.HexToRGB(INI.Red), Back);
                    }
                }
                else if (recordingType == ERecordingType.Profile)
                {
                    m_PopupControl.ShowPopup(recordingStopStr, Utillity.Instance.HexToRGB(INI.Red), Back);
                }
                break;

            case "Review":
                if (recordingType == ERecordingType.Lesson)
                {
                    m_PopupControl.ShowPopup(saveVideoStr, Utillity.Instance.HexToRGB(INI.Red), ProceedReview);
                }
                else if (recordingType == ERecordingType.Practice)
                {
                    m_PopupControl.ShowPopup(reviewStr, Utillity.Instance.HexToRGB(INI.Red), ProceedReview);
                }
                break;

            case "Review_Cancle":
                m_PopupControl.ShowPopup(reviewCancleStr, Utillity.Instance.HexToRGB(INI.Red), CancleReview);
                break;

            case "Retake":
                m_PopupControl.ShowPopup(retakeStr, Utillity.Instance.HexToRGB(INI.Red), ProceedRetake);
                break;

            case "Select_Retake":
                for (int i = 0; i < m_CaptureChecks.Length; i++)
                {
                    m_CaptureChecks[i].SetActive(true);
                    m_CheckToggles[i].isOn = false;
                }

                selectSwingStep.Clear();
                m_Main.SetActive(false);
                m_SelectRetake.SetActive(true);
                break;

            case "Record":
                if (recordingType == ERecordingType.Lesson)
                {
                    SetRecordingStep(ELessonStep.READY);
                }
                else if (recordingType == ERecordingType.Practice)
                {
                    if (selectSwingStep.Count == 0)
                        m_PopupControl.ShowTopPanel(selectSwingStepStr);
                    else
                        m_PopupControl.ShowPopup(retakeStr, Utillity.Instance.HexToRGB(INI.Red), ProceedSelectRetake);
                }
                break;

            case "RecordFinish":
                isFinish = true;
                m_LoadingPanel.SetActive(true);
                break;

            case "Cancle":
                foreach (GameObject item in m_CaptureChecks)
                {
                    item.SetActive(false);
                }

                m_Main.SetActive(true);
                m_SelectRetake.SetActive(false);
                break;

            case "Option":
                GameManager.Instance.OnClick_OptionPanel();
                break;
        }
    }

    public void OnValueChanged_Check(int idx, bool isOn)
    {
        int value = m_CheckToggles[idx].GetComponent<UIValueObject>().intValue;

        if (isOn)
        {
            if (!selectSwingStep.Contains((SWINGSTEP)value))
                selectSwingStep.Add((SWINGSTEP)value);

            selectSwingStep.Sort();
        }
        else
        {
            if (selectSwingStep.Contains((SWINGSTEP)value))
                selectSwingStep.Remove((SWINGSTEP)value);
        }
    }

    IEnumerator SaveRecordingVideo()
    {
        m_LoadingPanel.SetActive(true);

        //string frontPath = Path.Combine(videoPath, "driver_full_0.mp4");

        //string sidePath = GetUniqueFilePath(videoPath, "driver_full", "mp4");

        //StartCoroutine(SendFramesToFFmpeg(videoPath, widthFront, heightFront, framesFront, () => _replayReadyFront = true));
        //StartCoroutine(SendFramesToFFmpeg(sidePath, widthSide, heightSide, framesSide, () => _replayReadySide = true));

        //yield return new WaitUntil(() => _replayReadyFront == true);

        yield return new WaitForSeconds(2.0f);

        AppendCSVRow(
            fileName.Replace(".mp4", ""), fileName, 2, 5, 0, 4, 0, 0, DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ")
        );

        GolfProDataManager.Instance.ReloadProVideoData();

        m_LoadingPanel.SetActive(false);

        SetRecordingStep(ELessonStep.CHECK);
    }

    string fileName = string.Empty;
    private string GetUniqueFilePath(string directory, string baseFileName, string extension)
    {
        int index = 0;
        string filePath;

        do
        {
            fileName = $"{baseFileName}_{index}.{extension}";
            filePath = Path.Combine(directory, fileName);
            index++;
        }
        while (File.Exists(filePath));

        return filePath;
    }

    public void AppendCSVRow(string name, string path, int direction, int sceneType, int clubFilter,
                          int poseFilter, int favoriteCount, int views, string recently)
    {
        int newId = GetNextId();

        string newRow = $"{newId},{name},{path},{direction},{sceneType},{clubFilter},{poseFilter},{favoriteCount},{views},{recently}";

        using (StreamWriter sw = new StreamWriter(videoCSVPath, true))
        {
            sw.WriteLine(newRow);
        }

        //Console.WriteLine($"추가 완료: Id = {newId}");
    }

    private int GetNextId()
    {
        if (!File.Exists(videoCSVPath))
            return 10001;

        var lines = File.ReadAllLines(videoCSVPath).Skip(1);

        if (!lines.Any())
            return 10001;

        var maxId = lines
            .Select(line => int.TryParse(line.Split(',')[0], out int id) ? id : 0)
            .Max();

        return maxId + 1;
    }


    //-----------------------------------------------------------------------
    // 데이터 처리
    //-----------------------------------------------------------------------

    void PoseRecored()
    {
        //Front
        saveData_GetHandDir.Add(_iGetHandDir);

        saveData_GetHandDistance.Add(_iGetHandDistance);
        saveData_GetShoulderDistance.Add(_iGetShoulderDistance);

        saveData_GetSpineDir.Add(_iGetSpineDir);
        saveData_GetShoulderAngle.Add(_iGetShoulderAngle);
        saveData_GetFootDisRate.Add(_iGetFootDisRate);
        saveData_GetWeight.Add(_iGetWeight);
        saveData_GetForearmAngle.Add(_iGetForearmAngle);
        saveData_GetElbowFrontDir.Add(_iGetElbowFrontDir);
        saveData_GetElbowRightFrontDir.Add(_iGetElbowRightFrontDir);

        //Side
        saveData_GetHandSideDir.Add(_iGetHandSideDir);
        saveData_GetWaistSideDir.Add(_iGetWaistSideDir);
        saveData_GetKneeSideDir.Add(_iGetKneeSideDir);
        saveData_GetElbowSideDir.Add(_iGetElbowSideDir);
        saveData_GetArmpitDir.Add(_iGetArmpitDir);

        //combine
        saveData_GetShoulderDir.Add(_iGetShoulderDir);
        saveData_GetPelvisDir.Add(_iGetPelvisDir);
    }

    void SetResultData()
    {
        if (!isSelectRetake)
        {
            ResultProData.Clear();
            //Front
            ResultProData.Add("GetHandDir", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
            ResultProData.Add("GetHandDistance", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
            ResultProData.Add("GetShoulderDistance", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });

            ResultProData.Add("GetSpineDir", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
            ResultProData.Add("GetShoulderAngle", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
            ResultProData.Add("GetFootDisRate", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
            ResultProData.Add("GetWeight", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
            ResultProData.Add("GetForearmAngle", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
            ResultProData.Add("GetElbowFrontDir", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
            ResultProData.Add("GetElbowRightFrontDir", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
            //Side
            ResultProData.Add("GetHandSideDir", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
            ResultProData.Add("GetWaistSideDir", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
            ResultProData.Add("GetKneeSideDir", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
            ResultProData.Add("GetElbowSideDir", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
            ResultProData.Add("GetArmpitDir", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
            //combine
            ResultProData.Add("GetShoulderDir", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
            ResultProData.Add("GetPelvisDir", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });

        }
    }

    void ResultDataAdd(int step)
    {
        //Front
        if (step == 7) //피니시단계라면 팔로우+10
            ResultProData["GetHandDir"][step] = ResultProData["GetHandDir"][6] + 10;
        else
            ResultProData["GetHandDir"][step] = (int)saveData_GetHandDir.Average();
        ResultProData["GetHandDistance"][step] = (int)saveData_GetHandDistance.Average();
        ResultProData["GetShoulderDistance"][step] = (int)saveData_GetShoulderDistance.Average();

        ResultProData["GetSpineDir"][step] = (int)saveData_GetSpineDir.Average();
        ResultProData["GetShoulderAngle"][step] = (int)saveData_GetShoulderAngle.Average();
        ResultProData["GetFootDisRate"][step] = (int)saveData_GetFootDisRate.Average();
        ResultProData["GetWeight"][step] = (int)saveData_GetWeight.Average();
        ResultProData["GetForearmAngle"][step] = (int)saveData_GetForearmAngle.Average();
        ResultProData["GetElbowFrontDir"][step] = (int)saveData_GetElbowFrontDir.Average();
        ResultProData["GetElbowRightFrontDir"][step] = (int)saveData_GetElbowRightFrontDir.Average();

        //Side
        ResultProData["GetHandSideDir"][step] = (int)saveData_GetHandSideDir.Average();
        ResultProData["GetWaistSideDir"][step] = (int)saveData_GetWaistSideDir.Average();
        ResultProData["GetKneeSideDir"][step] = (int)saveData_GetKneeSideDir.Average();
        ResultProData["GetElbowSideDir"][step] = (int)saveData_GetElbowSideDir.Average();
        ResultProData["GetArmpitDir"][step] = (int)saveData_GetArmpitDir.Average();

        //combine
        ResultProData["GetShoulderDir"][step] = (int)saveData_GetShoulderDir.Average();
        ResultProData["GetPelvisDir"][step] = (int)saveData_GetPelvisDir.Average();
    }

    bool SaveCsvFull()
    {
        try
        {
            string output = string.Empty;
            output += "NAME,ADDRESS,TAKEBACK,BACKSWING,TOP,DOWNSWING,IMPACT,FOLLOW,FINISH\r\n";
            //Front
            output += "GetHandDir," + string.Join(",", ResultProData["GetHandDir"]) + "\r\n";
            output += "GetHandDistance," + string.Join(",", ResultProData["GetHandDistance"]) + "\r\n";
            output += "GetShoulderDistance," + string.Join(",", ResultProData["GetShoulderDistance"]) + "\r\n";

            output += "GetSpineDir," + string.Join(",", ResultProData["GetSpineDir"]) + "\r\n";
            output += "GetShoulderAngle," + string.Join(",", ResultProData["GetShoulderAngle"]) + "\r\n";
            output += "GetFootDisRate," + string.Join(",", ResultProData["GetFootDisRate"]) + "\r\n";
            output += "GetWeight," + string.Join(",", ResultProData["GetWeight"]) + "\r\n";
            output += "GetForearmAngle," + string.Join(",", ResultProData["GetForearmAngle"]) + "\r\n";
            output += "GetElbowFrontDir," + string.Join(",", ResultProData["GetElbowFrontDir"]) + "\r\n";
            output += "GetElbowRightFrontDir," + string.Join(",", ResultProData["GetElbowRightFrontDir"]) + "\r\n";
            //Side
            output += "GetHandSideDir," + string.Join(",", ResultProData["GetHandSideDir"]) + "\r\n";
            output += "GetWaistSideDir," + string.Join(",", ResultProData["GetWaistSideDir"]) + "\r\n";
            output += "GetKneeSideDir," + string.Join(",", ResultProData["GetKneeSideDir"]) + "\r\n";
            output += "GetElbowSideDir," + string.Join(",", ResultProData["GetElbowSideDir"]) + "\r\n";
            output += "GetArmpitDir," + string.Join(",", ResultProData["GetArmpitDir"]) + "\r\n";
            //combine
            output += "GetShoulderDir," + string.Join(",", ResultProData["GetShoulderDir"]) + "\r\n";
            output += "GetPelvisDir," + string.Join(",", ResultProData["GetPelvisDir"]);

            string filepath = path + "/" + $"{(int)swingType}_{(int)club}" + ".csv";
            File.WriteAllText(filepath, output);
            Debug.Log("Save File : " + filepath);
            return true;
        }
        catch (Exception e)
        {
            return false;
            //txtDebug.text = "Failed:" + e.Message;
        };

    }

    void SaveData_Clear()
    {
        //Front
        saveData_GetHandDir.Clear();
        saveData_GetHandDistance.Clear();

        saveData_GetSpineDir.Clear();
        saveData_GetShoulderAngle.Clear();
        saveData_GetFootDisRate.Clear();
        saveData_GetWeight.Clear();
        saveData_GetForearmAngle.Clear();
        saveData_GetElbowFrontDir.Clear();
        saveData_GetElbowRightFrontDir.Clear();
        //Side
        saveData_GetHandSideDir.Clear();
        saveData_GetWaistSideDir.Clear();
        saveData_GetKneeSideDir.Clear();
        saveData_GetElbowSideDir.Clear();
        saveData_GetArmpitDir.Clear();
        //combine
        saveData_GetShoulderDir.Clear();
        saveData_GetPelvisDir.Clear();
    }

    private void SaveAllImages()
    {
        string filepath = imagePath/* + "/" + $"{(int)swingType}_{(int)club}" + ".csv"*/;

        foreach (var pic in captureFrontDic)
        {
            SWINGSTEP step = pic.Key;
            Texture2D tex = pic.Value;

            byte[] bytes = tex.EncodeToPNG();
            string fileName = $"{step.ToString().ToLower()}_front.png";
            string filePath = Path.Combine(filepath, fileName);

            File.WriteAllBytes(filePath, bytes);
        }

        foreach (var pic in captureSideDic)
        {
            SWINGSTEP step = pic.Key;
            Texture2D tex = pic.Value;

            byte[] bytes = tex.EncodeToPNG();
            string fileName = $"{step.ToString().ToLower()}_side.png";
            string filePath = Path.Combine(filepath, fileName);

            File.WriteAllBytes(filePath, bytes);
        }
    }

    //-----------------------------------------------------------------------
    // 영상 처리부
    //-----------------------------------------------------------------------

    bool CaptureFrameFront(SWINGSTEP step = SWINGSTEP.READY)//RenderTexture renderTex)
    {
        Texture sourceTexFront = rawImageFront.texture;
        widthFront = sourceTexFront.width;
        heightFront = sourceTexFront.height;

        RenderTexture renderTexFront = RenderTexture.GetTemporary(widthFront, heightFront, 0);
        Graphics.Blit(sourceTexFront, renderTexFront);

        int zoomedWidth = Mathf.RoundToInt(widthFront / 1.2f);
        int zoomedHeight = Mathf.RoundToInt(heightFront / 1.2f);
        int startX = (widthFront - zoomedWidth) / 2;
        int startY = (heightFront - zoomedHeight) / 2;

        RenderTexture.active = renderTexFront;

        Texture2D captureFront = null;

        if (recordingType == ERecordingType.Lesson)
        {
            captureFront = new Texture2D(widthFront, heightFront, TextureFormat.RGB24, false);
            captureFront.ReadPixels(new Rect(0, 0, widthFront, heightFront), 0, 0);
        }
        else if (recordingType == ERecordingType.Practice)
        {
            captureFront = new Texture2D(zoomedWidth, zoomedHeight, TextureFormat.RGB24, false);
            captureFront.ReadPixels(new Rect(startX, startY, zoomedWidth, zoomedHeight), 0, 0);
        }
        else if (recordingType == ERecordingType.Profile)
        {
            captureFront = new Texture2D(widthFront, heightFront, TextureFormat.RGB24, false);
            captureFront.ReadPixels(new Rect(0, 0, widthFront, heightFront), 0, 0);

            if (checkTakeback == false)
            {
                checkTakebackFrame++;

                if (framesFront.Count > 90)
                {
                    checkTakebackFrame = 0;
                    framesFront.Clear();
                    framesSide.Clear();
                }

                if (_handCheck == false)
                {
                    RenderTexture.active = null;
                    RenderTexture.ReleaseTemporary(renderTexFront);

                    return false;
                }
            }
        }

        captureFront.Apply();

        if (recordingType == ERecordingType.Practice)
        {
            if (!isSelectRetake)
            {
                captureFrontDic.Add(step, captureFront);
            }
            else
            {
                captureFrontDic[step] = captureFront;
            }
        }
        else if (recordingType == ERecordingType.Profile)
        {
            framesFront.Add(captureFront);
        }
        //else if(recordingType == ERecordingType.Lesson)
        //{
        //    framesFront.Add(captureFront);
        //}

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(renderTexFront);

        return true;
    }

    void CaptureFrameSide(SWINGSTEP step = SWINGSTEP.READY)//RenderTexture renderTex)
    {
        Texture sourceTexSide = rawImageSide.texture;
        widthSide = sourceTexSide.width;
        heightSide = sourceTexSide.height;
        RenderTexture renderTexSide = RenderTexture.GetTemporary(widthSide, heightSide, 0);
        Graphics.Blit(sourceTexSide, renderTexSide);

        RenderTexture.active = renderTexSide;
        Texture2D captureSide = new Texture2D(widthSide, heightSide, TextureFormat.RGB24, false);
        captureSide.ReadPixels(new Rect(0, 0, widthSide, heightSide), 0, 0);
        captureSide.Apply();

        if (recordingType == ERecordingType.Practice)
        {
            if (!isSelectRetake)
                captureSideDic.Add(step, captureSide);
            else
                captureSideDic[step] = captureSide;
        }
        else if (recordingType == ERecordingType.Profile)
        {
            framesSide.Add(captureSide);
        }
        //else if(recordingType == ERecordingType.Lesson)
        //{
        //    framesSide.Add(captureSide);
        //}

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(renderTexSide);
    }

    //IEnumerator CaptureFrames()
    //{
    //    int retry = 0;

    //    while (isRecording)
    //    {
    //        yield return new WaitForEndOfFrame();
    //        //yield return null;

    //        if (CaptureFrameFront())//renderTexFront))
    //        {
    //            CaptureFrameSide();//renderTexSide);
    //        }

    //        if (isFinish == true)
    //        {
    //            isRecording = false;
    //        }

    //        //
    //        //if (mocapFront.AvgVisibility < 0.1f)
    //        //{
    //        //    if (retry > 5)
    //        //    {
    //        //        yield break;
    //        //    }
    //        //    else
    //        //    {
    //        //        retry++;
    //        //    }
    //        //}
    //    }

    //}

    IEnumerator CaptureFramesProfile()
    {
        int retry = 0;
        //txtDebug.text += "프레임 캡쳐 시작" + "\r\n";
        checkTakebackFrame = 0;
        int frameCount = 0;

        while (isRecording)
        {
            yield return new WaitForEndOfFrame();
            //yield return null;

            if (CaptureFrameFront())//renderTexFront))
            {
                CaptureFrameSide();//renderTexSide);
            }

            //if (checkImpact == true)
            if (checkImpact == true)
            {
                if (frameCount < 40)
                    frameCount++;
                else
                    isRecording = false;
            }

            //
            if (AvgVisible < 0.1f)
            {
                if (retry > 5)
                {
                    SetRecordingStep(EProfileStep.GRIP);
                    yield break;
                }
                else
                {
                    retry++;
                }
            }
        }

        //앞 프레임 삭제
        framesFront.RemoveRange(0, Math.Max(0, checkTakebackFrame - 30));
        framesSide.RemoveRange(0, Math.Max(0, checkTakebackFrame - 30));

        Debug.Log($"CaptureFrames() End");

        SetRecordingStep(EProfileStep.RESULT);
        //txtDebug.text += "프레임 캡쳐 종료" + "\r\n";
    }

    IEnumerator CaptureFrames()
    {
        string output = videoPath;
        int width = rawImageFront.texture.width;
        int height = rawImageFront.texture.height;

        int frameCount = 0;

        widthFront = width;
        heightFront = height;

        if (!TryStartFFmpeg(output, width, height, out var process, out var stdin, out var errorMsg))
        {
            Debug.LogError($"FFmpeg 실행 실패: {errorMsg}");
            yield break;
        }

        while (isRecording)
        {
            yield return new WaitForEndOfFrame();

            RenderTexture renderTex = RenderTexture.GetTemporary(width, height, 0);
            Graphics.Blit(rawImageFront.texture, renderTex);
            RenderTexture.active = renderTex;

            Texture2D frame = new Texture2D(width, height, TextureFormat.RGB24, false);
            frame.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            frame.Apply();

            if (frameCount == 0)
            {
                captureFront = Instantiate(frame);
            }

            byte[] raw = frame.GetRawTextureData();
            stdin.Write(raw, 0, raw.Length);

            UnityEngine.Object.DestroyImmediate(frame);
            RenderTexture.ReleaseTemporary(renderTex);
            RenderTexture.active = null;

            frameCount++;

            if (isFinish)
            {
                isRecording = false;
            }
        }

        stdin.Flush();
        stdin.Close();

        yield return new WaitUntil(() => process.HasExited);
        process.Close();

        GC.Collect();
        _replayReadyFront = true;
    }

    private IEnumerator SendFramesToFFmpeg(string output, int width, int height, List<Texture2D> frames, Action ComplateEvent)
    {
        yield return null;
        yield return null;

        Process process = null;
        Stream stdin = null;

        if (!TryStartFFmpeg(output, width, height, out process, out stdin, out string errorMsg))
        {
            //txtDebug.text += "FFmpeg 실행 실패: " + errorMsg + "\n";
            yield break;
        }

        //txtDebug.text += "FFmpeg 실행 성공\n";

        for (int i = 0; i < frames.Count; i++)
        {
            byte[] raw = frames[i].GetRawTextureData();
            stdin.Write(raw, 0, raw.Length);

            if (i % 20 == 0)
                yield return null;
        }

        stdin.Flush();
        stdin.Close();

        bool isExited = false;
        _ = System.Threading.Tasks.Task.Run(() => {
            _ = process.WaitForExit(5000);  // 메인 스레드 블로킹 안 함
            //process.WaitForExit();  // 메인 스레드 블로킹 안 함
            //txtDebug.text += $" ret:{ret} ";
            process.Close();
            isExited = true;  // 종료되었음을 표시
        });

        while (!isExited)
        {
            yield return null;
        }

        // 종료 후 후처리
        ComplateEvent?.Invoke();

        yield return new WaitForSeconds(0.1f);
    }

    private bool TryStartFFmpeg(string output, int width, int height, out Process process, out Stream stdin, out string errorMsg)
    {
        stdin = null;
        process = new Process();
        errorMsg = "";

        //string outputPath = Path.Combine(Application.persistentDataPath, "output.mp4");
        if (File.Exists(ffmpegPath) == false)
        {
            //txtDebug.text += "ffmpeg.exe 찾을 수 없음\n";
            return false;
        }

        try
        {
            process.StartInfo = new ProcessStartInfo {
                FileName = ffmpegPath,
                Arguments = $"-y -f rawvideo -pixel_format rgb24 -video_size {width}x{height} -framerate 30 -i - -vf vflip -c:v libx264 -pix_fmt yuv420p -profile:v baseline -movflags +faststart \"{output}\"",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            stdin = process.StandardInput.BaseStream;
            return true;
        }
        catch (System.ComponentModel.Win32Exception ex)
        {
            errorMsg = "Win32 오류: " + ex.Message;
            return false;
        }
        catch (System.Exception ex)
        {
            errorMsg = "일반 오류: " + ex.Message;
            return false;
        }
    }

    //-----------------------------------------------------------------------
    // 2D 랜드마크 처리부 
    //-----------------------------------------------------------------------
    //TODO:AICoaching에 사용 된 Director에 중복으로 사용되는 문제 개선필요
    IEnumerator CoLandmarkProcesss()
    {
        bool flip = false;

        while (true)
        {
            try
            {
                Vector2 shoulderLeft = new Vector2(webcamclient.poseData1["landmark_11"].x, webcamclient.poseData1["landmark_11"].y);
                Vector2 shoulderRight = new Vector2(webcamclient.poseData1["landmark_12"].x, webcamclient.poseData1["landmark_12"].y);
                Vector2 leftHand = new Vector2(webcamclient.poseData1["landmark_15"].x, webcamclient.poseData1["landmark_15"].y);
                Vector2 RightHand = new Vector2(webcamclient.poseData1["landmark_16"].x, webcamclient.poseData1["landmark_16"].y);

                //webcamclient.poseData1;
                if ((webcamclient.poseData1["landmark_15"].visibility + webcamclient.poseData1["landmark_13"].visibility)
                    - (webcamclient.poseData1["landmark_16"].visibility + webcamclient.poseData1["landmark_14"].visibility) < -0.1f)
                    GetHandDir(shoulderLeft, shoulderRight, leftHand, RightHand, new Vector2(webcamclient.poseData1["landmark_22"].x, webcamclient.poseData1["landmark_22"].y));
                else
                    GetHandDir(shoulderLeft, shoulderRight, leftHand, RightHand, new Vector2(webcamclient.poseData1["landmark_21"].x, webcamclient.poseData1["landmark_21"].y));

                _handCheck = CheckHandDis(webcamclient.poseData1["landmark_13"], webcamclient.poseData1["landmark_14"]);

                if (webcamclient.poseData1.Count > 32)
                {
                    AvgVisible = (webcamclient.poseData1["landmark_0"].visibility
                        + webcamclient.poseData1["landmark_11"].visibility
                        + webcamclient.poseData1["landmark_12"].visibility
                        + webcamclient.poseData1["landmark_13"].visibility
                        + webcamclient.poseData1["landmark_14"].visibility
                        + webcamclient.poseData1["landmark_25"].visibility
                        + webcamclient.poseData1["landmark_26"].visibility
                        + webcamclient.poseData1["landmark_15"].visibility
                        + webcamclient.poseData1["landmark_16"].visibility) / 9f;
                }
                else
                    AvgVisible = 0;
            }
            catch
            {
                AvgVisible = 0;
                _handCheck = false;
            }


            yield return null;
        }
    }

    public float GetHandDir(Vector2 shoulderLeft, Vector2 shoulderRight, Vector2 leftHand, Vector2 RightHand, Vector2 handVector)
    {
        // 어꺠중심과 손중심을 기준
        Vector2 shoulderVector = (shoulderLeft + shoulderRight) / 2;

        Vector3 dir = handVector - shoulderVector;
        dir.z = 0;

        //_lastHandDir = Quaternion.FromToRotation(Vector3.down, dir).eulerAngles.z;
        float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
        angle += 180f;
        _lastHandDir = angle;
        // 어깨 벡터와 손 벡터 간의 각도 계산
        return _lastHandDir;
    }

    bool CheckHandDis(webcamclient.Landmark2D leftHand, webcamclient.Landmark2D RightHand)
    {

        float shDis = GetHandDis(leftHand, RightHand);

        return shDis < 200f ? true : false;

    }
    float GetHandDis(webcamclient.Landmark2D leftHand, webcamclient.Landmark2D RightHand)
    {
        if (leftHand.visibility < 0.5f || RightHand.visibility < 0.5f)
        {
            return 999f;
        }
        else
        {
            float shDis = Vector2.Distance(new Vector2(leftHand.x, leftHand.y), new Vector2(RightHand.x, RightHand.y));
            return shDis;
        }

    }
}
