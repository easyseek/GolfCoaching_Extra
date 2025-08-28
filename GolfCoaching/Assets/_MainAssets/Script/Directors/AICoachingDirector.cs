using DG.Tweening;
using Enums;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using Debug = UnityEngine.Debug;
using UnityEngine.Video;
using System.Linq;

public class AngleRange
{
    public float CheckValue;
    public float LimitMin;
    public float LimitMax;

    private float lastAngle = -1.0f;
    private float angleLimit = 50.0f;

    public bool IsMore;
    public bool UseLimitCheck;

    public AngleRange(float checkValue, float limitMin, float limitMax, bool isMore, bool useLimitCheck)
    {
        CheckValue = checkValue;
        LimitMin = limitMin;
        LimitMax = limitMax;
        IsMore = isMore;
        UseLimitCheck = useLimitCheck;
    }

    public bool IsInCheckRange(float angle)
    {
        //float f = FilterAngle(angle);
        //return IsMore ? f > CheckValue : f < CheckValue;
        return IsMore ? angle > CheckValue : angle < CheckValue;
    }

    public bool IsInLimitRange(float angle)
    {
        return !UseLimitCheck || (angle >= LimitMin && angle <= LimitMax);
    }

    private float FilterAngle(float angle)
    {
        if(lastAngle < 0.0f)
        {
            lastAngle = angle;
            return angle;
        }

        float f = Mathf.Abs(angle - lastAngle);

        if (f > angleLimit)
        {
            return lastAngle;
        }

        lastAngle = angle;

        return angle;
    }
    
    public void ResetAngle()
    {
        lastAngle = -1.0f;
    }
}

public class AICoachingDirector : MonoBehaviour
{
    [Header("* 1.GRIP")]
    [SerializeField] GameObject PanelGrip;

    [Header("* 2.ADDRESS")]
    [SerializeField] GameObject PanelAddress;
    [SerializeField] GameObject imgBadIcon;
    [SerializeField] TextMeshProUGUI txtAddressInfo;
    [SerializeField] TextMeshProUGUI txtAddressCount;
    int _addressCount = 3;

    [Header("* 3.SWING")]
    [SerializeField] GameObject PanelSwing;
    [SerializeField] CanvasGroup cgSwingInfo;

    [Header("* 4.ANALYZE")]
    [SerializeField] GameObject PanelAnalyze;
    [SerializeField] GameObject BlurBack;
    [SerializeField] TextMeshProUGUI txtAnalyzeInfo;
    [SerializeField] RectTransform imgLeftUserModel;
    [SerializeField] RectTransform imgRightProModel;
    [SerializeField] Image imgMergeGlow;

    [Header("* 5.RESULT")]
    [SerializeField] GameObject PanelResult;
    [SerializeField] GameObject m_AnalyzeGroup;
    [SerializeField] GameObject m_Lesson;
    [SerializeField] GameObject m_AnalyzeTotal;
    [SerializeField] GameObject m_AnalyzePose;
    [SerializeField] GameObject[] m_DotObjects;
    [SerializeField] GameObject m_DirToggleCover;
    [SerializeField] GameObject[] m_Models;

    [SerializeField] RectTransform m_FrontProView, m_SideProView, m_FrontUserView, m_SideUserView;
    [SerializeField] RectTransform m_FrontProReal, m_SideProReal, m_FrontUserReal, m_SideUserReal;
    [SerializeField] RectTransform m_BeforeRateBar;
    [SerializeField] RectTransform m_CurrentRateBar;
    [SerializeField] RectTransform m_DetailAnalyzePanel;

    [SerializeField] RawImage m_FrontProRealRaw, m_SideProRealRaw, m_FrontUserRealRaw, m_SideUserRealRaw;

    [SerializeField] ToggleGroup m_ResultMainTG;
    [SerializeField] ToggleGroup m_ResultPoseTG;
    [SerializeField] ToggleGroup m_ModelDirectionTG;

    [SerializeField] Toggle[] m_ResultMainToggles;
    [SerializeField] Toggle[] m_ResultPoseToggles;
    [SerializeField] Toggle[] m_ModelDirectionToggles;
    [SerializeField] Toggle m_ModelChangeToggle;
    [SerializeField] Toggle m_RealVideoSpeedToggle;

    [SerializeField] VideoPlayer m_RealProFrontVideo, m_RealProSideVideo, m_RealUserFrontVideo, m_RealUserSideVideo;

    [SerializeField] private TextMeshProUGUI m_ProNameText;
    [SerializeField] private TextMeshProUGUI m_UserNameText;
    [SerializeField] private TextMeshProUGUI m_MatchingRateText;
    [SerializeField] private TextMeshProUGUI m_ContrastRateText;
    [SerializeField] private TextMeshProUGUI m_BeforeBarText;
    [SerializeField] private TextMeshProUGUI m_CurrentBarText;
    [SerializeField] private TextMeshProUGUI m_GoodPoseText;
    [SerializeField] private TextMeshProUGUI m_BadPoseText;
    [SerializeField] private TextMeshProUGUI m_CurPoseScoreText;
    [SerializeField] private TextMeshProUGUI[] m_MyScoreTexts;
    [SerializeField] private TextMeshProUGUI[] m_MyScoreNameTexts;
    [SerializeField] private TextMeshProUGUI[] m_DotTexts;

    [SerializeField] private UILineRenderer m_MyLineRenderer;
    [SerializeField] private UILineRenderer m_AvgLineRenderer;
    [SerializeField] private UILineRenderer m_PoseTimeLineRenderer;

    [SerializeField] private Graphic myFillGraphic;
    [SerializeField] private Graphic avgFillGraphic;

    [SerializeField] private Image m_PoseProgressImg;

    [SerializeField] private Animator m_ProModelAni;
    [SerializeField] private Animator m_UserModelAni;

    private Dictionary<SWINGSTEP, AngleRange> angleRanges = new Dictionary<SWINGSTEP, AngleRange>();

    private Vector2 detailOpenPos = new Vector2(0, 1701.0f);

    private Vector2 proFrontPos, proFrontSize, proBackPos, proBackSize;
    private Vector2 userFrontPos, userFrontSize, userBackPos, userBackSize;

    private List<int> myScore = Enumerable.Repeat(-1, 8).ToList();
    //private List<int> avgScore = Enumerable.Repeat(-1, 8).ToList();

    List<int> avgScore = new List<int>{ 0, 0, 0, 0, 0, 0, 0, 0 };
    
    List<int> addressTimeline = new List<int>();
    List<int> takebackTimeline = new List<int>();
    List<int> backswingTimeline = new List<int>();
    List<int> topTimeline = new List<int>();
    List<int> downswingTimeline = new List<int>();
    List<int> impactTimeline = new List<int>();
    List<int> followTimeline = new List<int>();
    List<int> finishTimeline = new List<int>();
    
    private float[] currentScore = new float[8];
    private float[] currentAvgScore = new float[8];
    private float radius = 150.0f;
    private int pointCount = 6;

    bool _isDetailPanelOpen = false;

    private bool _isFront = true;
    private bool _isViewAnimating = false;
    private bool _isTotalAnalyze = true;
    private bool _is3DModel = true;

    private SWINGSTEP selectStep = SWINGSTEP.ADDRESS;
    private SWINGSTEP stepStage = SWINGSTEP.READY;

    [Header("* MOCAP")]
    [SerializeField] private TextMeshProUGUI m_DebugText;
    [SerializeField] private TextMeshProUGUI m_Debug2Text;
    [SerializeField] private TextMeshProUGUI m_Debug3Text;

    //[SerializeField] mocapFront mocapFront;
    //[SerializeField] mocapSide mocapSide;
    [SerializeField] SensorProcess sensorProcess;

    ProSwingStepData swingStepData = null;

    Dictionary<string, int[]> DicUserSwingData = new Dictionary<string, int[]>();

    Dictionary<string, float> ErrorMargins = new Dictionary<string, float>()
    {
        { "GetHandDir", 95f }, { "GetHandDistance", 76f }, { "GetShoulderDistance", 30f }, { "GetSpineDir", 5f }, { "GetShoulderAngle", 30f }, { "GetFootDisRate", 79f }, { "GetWeight", 20f }, { "GetForearmAngle", 60f }, { "GetElbowFrontDir", 30f }, { "GetHandSideDir", 70f }, { "GetWaistSideDir", 5f }, { "GetKneeSideDir", 18f }, { "GetElbowSideDir", 45f }, { "GetArmpitDir", 15f }, { "GetShoulderDir", 75f }, { "GetPelvisDir", 68f }
    };

    [Header("* VIDEO REF.")]
    [SerializeField] webcamclient webcamclient;
    [SerializeField] RawImage rawImageFront;
    [SerializeField] RawImage rawImageSide;

    private List<Texture2D> framesFront = new();
    private List<Texture2D> framesSide = new();
    private List<Texture2D> captureRealPoseFrontPro = Enumerable.Repeat<Texture2D>(null, 8).ToList();
    private List<Texture2D> captureRealPoseSidePro = Enumerable.Repeat<Texture2D>(null, 8).ToList();
    private List<Texture2D> captureRealPoseFrontUser = Enumerable.Repeat<Texture2D>(null, 8).ToList();
    private List<Texture2D> captureRealPoseSideUser = Enumerable.Repeat<Texture2D>(null, 8).ToList();

    private bool isRecording = false;
    bool checkTakeback = false;
    bool checkImpact = false;
    bool isFinish = false;

    int checkTakebackFrame = 0;
    int widthFront;
    int heightFront;
    int widthSide;
    int heightSide;
    int curStepNum = 0;

    bool _replayReadyFront = false;
    bool _replayReadySide = false;
    int fps = 30;

    string outputPathFront = string.Empty;
    string outputPathSide = string.Empty;
    string ffmpegPath = string.Empty;
    string imagePath = string.Empty;

    float _lastHandDir;
    bool _handCheck = false;
    float AvgVisible = 0;

    enum COACHINGSTEP
    {
        //READY = -1,
        GRIP,
        ADDRESS,
        SWING,
        SWINGEND,
        ANALYZE,
        RESULT
    }
    COACHINGSTEP coahingStep = COACHINGSTEP.GRIP;

    private void Start()
    {
        Init();

        StartCoroutine(CoLandmarkProcesss());

        StartCoroutine(CheckAISwing());
    }

    private void Init()
    {
        swingStepData = GolfProDataManager.Instance.SelectProData.swingData.dicFull[EClub.MiddleIron];

        outputPathFront = Path.Combine(Application.persistentDataPath, "front_video.mp4");
        outputPathSide = Path.Combine(Application.persistentDataPath, "side_video.mp4");
        ffmpegPath = Path.Combine(Application.dataPath, "Plugins", "ffmpeg", "bin", "ffmpeg.exe");
        imagePath = @$"C:\DataBase\ProImage\{GolfProDataManager.Instance.SelectProData.uid}";
        //videoPlayerFront.url = outputPathFront;
        //videoPlayerSide.url = outputPathSide;

        for (int i = 0; i < 8; i++)
        {
            captureRealPoseFrontPro[i] = LoadTextureFromFile($"{imagePath}/{(SWINGSTEP)i}_front.png");
            captureRealPoseSidePro[i] = LoadTextureFromFile($"{imagePath}/{(SWINGSTEP)i}_side.png");
        }

        proFrontPos = m_FrontProView.anchoredPosition;
        proFrontSize = m_FrontProView.sizeDelta;
        proBackPos = m_SideProView.anchoredPosition;
        proBackSize = m_SideProView.sizeDelta;

        userFrontPos = m_FrontUserView.anchoredPosition;
        userFrontSize = m_FrontUserView.sizeDelta;
        userBackPos = m_SideUserView.anchoredPosition;
        userBackSize = m_SideUserView.sizeDelta;

        for (int i = 0; i < m_ResultMainToggles.Length; i++)
        {
            m_ResultMainToggles[i].onValueChanged.AddListener(OnValueChanged_ResultMainToggle);
        }

        for (int i = 0; i < m_ResultPoseToggles.Length; i++)
        {
            m_ResultPoseToggles[i].onValueChanged.AddListener(OnValueChanged_ResultPoseToggle);
        }

        for (int i = 0; i < m_ModelDirectionToggles.Length; i++)
        {
            m_ModelDirectionToggles[i].onValueChanged.AddListener(OnValueChanged_ToggleDirection);
        }

        m_ModelChangeToggle.onValueChanged.AddListener(OnValueChanged_ModelChange);
        m_RealVideoSpeedToggle.onValueChanged.AddListener(OnValueChanged_RealVideoSpeed);

        AnimateMatchingRate(0.0f, (myScore.Sum() / (myScore.Count - 2)) * 0.01f);
        AnimateTotalGraph(myScore, avgScore, 1.1f);
        StartCoroutine(ModelAnimation(true, m_ProModelAni, m_UserModelAni));
    }

    void SetReadyGrip()
    {
        //SetCoachinggStep(COACHINGSTEP.GRIP);
        checkTakeback = false;
        checkImpact = false;
        isFinish = false;
        checkTakebackFrame = 0;
        framesFront.Clear();
        framesSide.Clear();

        Resources.UnloadUnusedAssets();

        _replayReadyFront = false;
        _replayReadySide = false;
        _addressCount = 3;
    }

    private void SetRanges()
    {
        angleRanges[SWINGSTEP.ADDRESS] = new AngleRange(swingStepData.dicAddress["GetHandDir"], 0, 0, false, false);
        angleRanges[SWINGSTEP.TAKEBACK] = new AngleRange(swingStepData.dicTakeback["GetHandDir"], 0, 0, false, false);
        angleRanges[SWINGSTEP.BACKSWING] = new AngleRange(swingStepData.dicBackswing["GetHandDir"], 0, 0, false, false);
        angleRanges[SWINGSTEP.TOP] = new AngleRange(swingStepData.dicTop["GetHandDir"] + 20.0f, 0, 0, false, false);
        angleRanges[SWINGSTEP.DOWNSWING] = new AngleRange(swingStepData.dicDownswing["GetHandDir"], 0, 0, true, false);
        angleRanges[SWINGSTEP.IMPACT] = new AngleRange(swingStepData.dicImpact["GetHandDir"], 0, 0, true, false);
        angleRanges[SWINGSTEP.FOLLOW] = new AngleRange(swingStepData.dicFollow["GetHandDir"], 0, 0, true, false);
        angleRanges[SWINGSTEP.FINISH] = new AngleRange(swingStepData.dicFinish["GetHandDir"], 0, 0, true, false);

        logPath = Path.Combine(Application.dataPath, "SwingAngleLog.txt");
    }

    IEnumerator CheckAISwing()
    {
        float timer = 0;
        SetReadyGrip();

        while (true)
        {
            m_DebugText.text = $"{sensorProcess.iGetHandDir}";
            m_Debug3Text.text = $"{sensorProcess.iGetShoulderDir}";
            m_Debug2Text.text = $"{stepStage}";

            //어드레스 감지
            if (coahingStep.Equals(COACHINGSTEP.GRIP))
            {
                //if (_handCheck && (mocapFront.GetHandDir() < 190f && mocapFront.GetHandDir() > 170))
                if (_handCheck && (sensorProcess.iGetHandDir < 190f && sensorProcess.iGetHandDir > 170))
                {
                    timer += Time.deltaTime;
                    if (timer > 0.5f)
                    {
                        //
                        //framesFront.Clear();
                        timer = 0;
                        SetCoachinggStep(COACHINGSTEP.ADDRESS);
                    }
                }
                else
                {
                    timer = 0;
                }
            }
            else if (coahingStep.Equals(COACHINGSTEP.ADDRESS))
            {
                if (_handCheck && (sensorProcess.iGetHandDir < 190f && sensorProcess.iGetHandDir > 170))
                {
                    timer += Time.deltaTime;
                    txtAddressInfo.text = "자세를 유지하고 준비해주세요\r\n곧 시작합니다";
                    txtAddressCount.text = _addressCount.ToString();
                    imgBadIcon.SetActive(false);
                    if (timer > 1f)
                    {
                        _addressCount--;
                        timer = 0;
                        if (_addressCount < 0)
                        {
                            SetResultData();

                            SetRanges();

                            SetCoachinggStep(COACHINGSTEP.SWING);
                            isRecording = true;
                            cgSwingInfo.DOFade(0, 1f).SetDelay(1f);

                            if (File.Exists(logPath))
                                File.Delete(logPath);

                            ResetSwing();
                            StartCoroutine(CaptureFrames());
                        }
                    }
                }
                else
                {
                    _addressCount = 3;
                    timer = 0;
                    txtAddressInfo.text = "자세를 인식하지 못했어요\r\n그립을 다시 잡아주세요";
                    txtAddressCount.text = string.Empty;
                    imgBadIcon.SetActive(true);
                }
            }
            else if (coahingStep.Equals(COACHINGSTEP.SWING))
            {
                //테이크백 감지
                if (checkTakeback == false && _handCheck && sensorProcess.iGetHandDir < 150f)
                {
                    checkTakeback = true;
                }
                //임팩트 감지
                if (checkTakeback == true && checkImpact == false && _lastHandDir > 170f)
                {
                    checkImpact = true;
                }

                UpdateSwing(sensorProcess.iGetHandDir);
                //UpdateSwing(_lastHandDir);
            }
            else if (coahingStep.Equals(COACHINGSTEP.SWINGEND))
            {
                if (myScore[(int)SWINGSTEP.FOLLOW] == -1)
                {
                    EnsurePoseCapture((int)SWINGSTEP.FOLLOW);
                    UserDataAdd((int)SWINGSTEP.FOLLOW);
                }
                
                if(myScore[(int)SWINGSTEP.FINISH] == -1)
                {
                    EnsurePoseCapture((int)SWINGSTEP.FINISH);
                    UserDataAdd((int)SWINGSTEP.FINISH);
                }

                finishTimeline.Add(myScore[(int)SWINGSTEP.FINISH]);

                SetCoachinggStep(COACHINGSTEP.ANALYZE);

                StartCoroutine(SendFramesToFFmpeg(outputPathFront, widthFront, heightFront, framesFront, () => _replayReadyFront = true));
                StartCoroutine(SendFramesToFFmpeg(outputPathSide, widthSide, heightSide, framesSide, () => _replayReadySide = true));
            }
            else if (coahingStep.Equals(COACHINGSTEP.ANALYZE))
            {
                yield return new WaitUntil(() => _replayReadyFront == true && _replayReadySide == true);

                txtAnalyzeInfo.text = "프로의 스윙과 매칭 중입니다";
                imgLeftUserModel.gameObject.SetActive(true);
                imgRightProModel.gameObject.SetActive(true);
                
                yield return new WaitForSeconds(0.1f);
                imgLeftUserModel.DOLocalMoveX(0, 2f);
                imgRightProModel.DOLocalMoveX(0, 2f);

                yield return new WaitForSeconds(2.2f);
                imgMergeGlow.gameObject.SetActive(true);
                imgMergeGlow.DOFade(1, 0.5f);

                yield return new WaitForSeconds(1.5f);
                txtAnalyzeInfo.text = "잠시 후 결과 화면으로 넘어갑니다";
                imgLeftUserModel.gameObject.SetActive(false);
                imgRightProModel.gameObject.SetActive(false);
                imgMergeGlow.gameObject.SetActive(false);

                yield return new WaitForSeconds(2f);
                SetCoachinggStep(COACHINGSTEP.RESULT);
            }
            else if (coahingStep.Equals(COACHINGSTEP.RESULT))
            {
                //yield return new WaitForSeconds(2f);

                //SetCoachinggStep(COACHINGSTEP.GRIP);
            }

            yield return null;
        }
    }

    void SetCoachinggStep(COACHINGSTEP step)
    {
        if (step == COACHINGSTEP.GRIP)
        {
            SetReadyGrip();
            NewShowPanel(step);
        }
        else if (step == COACHINGSTEP.ADDRESS)
        {
            NewShowPanel(step);
        }
        else if (step == COACHINGSTEP.SWING)
        {
            NewShowPanel(step);
        }
        else
            NewShowPanel(step);

        Debug.Log($"SetCoachinggStep() {coahingStep} -> {step}");
        coahingStep = step;
    }

    void NewShowPanel(COACHINGSTEP step)
    {
        PanelGrip.SetActive(false);
        PanelAddress.SetActive(false);
        PanelSwing.SetActive(false);
        PanelAnalyze.SetActive(false);
        PanelResult.SetActive(false);
        BlurBack.SetActive(false);

        if (step == COACHINGSTEP.GRIP)
        {
            PanelGrip.SetActive(true);
        }
        else if (step == COACHINGSTEP.ADDRESS)
        {
            PanelAddress.SetActive(true);
            imgBadIcon.SetActive(false);
            _addressCount = 3;
            txtAddressInfo.text = "자세를 유지하고 준비해주세요\r\n곧 시작합니다";
            txtAddressCount.text = _addressCount.ToString();
            imgBadIcon.SetActive(false);
        }
        else if (step == COACHINGSTEP.SWING)
        {
            PanelSwing.SetActive(true);
            cgSwingInfo.alpha = 1;
        }
        else if (step == COACHINGSTEP.ANALYZE)
        {
            PanelAnalyze.SetActive(true);
            BlurBack.SetActive(true);
            txtAnalyzeInfo.text = "스윙을 분석하고 있습니다";
            imgLeftUserModel.gameObject.SetActive(false);
            imgLeftUserModel.anchoredPosition = new Vector2(-227, 0);
            imgRightProModel.gameObject.SetActive(false);
            imgRightProModel.anchoredPosition = new Vector2(232, 0);
            imgMergeGlow.gameObject.SetActive(false);
        }
        else if (step == COACHINGSTEP.RESULT)
        {
            OnClick_Result();


            AnimateMatchingRate(0.0f, (myScore.Sum() / (myScore.Count - 2)) * 0.01f);
            AnimateTotalGraph(myScore, avgScore, 1.1f);
        }
    }

    Tween beforeRateTween;
    Tween currentRateTween;

    public void AnimateMatchingRate(float beforeRatio, float currentRatio)
    {
        float beforeHeight = 195.0f * beforeRatio;
        int beforePercent = Mathf.RoundToInt(beforeRatio * 100);

        float currentHeight = 195.0f * currentRatio;
        int currentPercent = Mathf.RoundToInt(currentRatio * 100);

        m_BeforeRateBar.sizeDelta = new Vector2(m_BeforeRateBar.sizeDelta.x, 0);
        m_CurrentRateBar.sizeDelta = new Vector2(m_CurrentRateBar.sizeDelta.x, 0);

        if (beforeRateTween != null && beforeRateTween.IsActive())
            beforeRateTween.Kill();

        if (currentRateTween != null && currentRateTween.IsActive())
            currentRateTween.Kill();

        // 바
        beforeRateTween = DOTween.To(() => m_BeforeRateBar.sizeDelta.y, y => {
            m_BeforeRateBar.sizeDelta = new Vector2(m_BeforeRateBar.sizeDelta.x, y);
        }, beforeHeight, 1.0f).SetEase(Ease.OutCubic);

        currentRateTween = DOTween.To(() => m_CurrentRateBar.sizeDelta.y, y => {
            m_CurrentRateBar.sizeDelta = new Vector2(m_CurrentRateBar.sizeDelta.x, y);
        }, currentHeight, 1.0f).SetEase(Ease.OutCubic);

        // 텍스트
        //DOTween.To(() => 0, x => {
        //    m_MatchingRateText.text = $"{x}%";
        //}, currentPercent, 1.0f).SetEase(Ease.OutCubic);
        m_MatchingRateText.text = $"{currentPercent}%";

        DOTween.To(() => 0, x => {
            m_BeforeBarText.text = $"{x}%";
        }, beforePercent, 1.0f).SetEase(Ease.OutCubic);

        DOTween.To(() => 0, x => {
            m_CurrentBarText.text = $"{x}%";
        }, currentPercent, 1.0f).SetEase(Ease.OutCubic);
    }

    private void DrawTotalGraph(UILineRenderer renderer, float[] values)
    {
        if (values.Length != pointCount)
            return;

        List<Vector2> points = new List<Vector2>();
        float angleStep = 360.0f / pointCount;

        for(int i = 0; i < pointCount; i++)
        {
            float angleDeg = 90f - (angleStep * i);
            float angleRad = angleDeg * Mathf.Deg2Rad;

            float scaled = values[i] / 100f * radius;
            Vector2 point = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad)) * scaled;
            points.Add(point);
        }

        points.Add(points[0]);

        renderer.Points = points.ToArray();
        renderer.SetAllDirty();
    }

    private void FillTotalGraph(Graphic graphic, float[] values)
    {
        if(!(graphic is MaskableGraphic)) return;

        VertexHelper vh = new VertexHelper();
        Vector2 center = graphic.rectTransform.rect.center;
        vh.AddVert(center, graphic.color, Vector2.zero);

        float angleStep = 360 / pointCount;

        for(int i = 0; i < pointCount; i++)
        {
            float angle = (90.0f - (angleStep * i)) * Mathf.Deg2Rad;
            float scaled = values[i] / 100.0f * radius;
            Vector2 pos = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * scaled;
            vh.AddVert(pos, graphic.color, Vector2.zero);
        }

        for(int i = 1; i <= pointCount; i++)
        {
            int next = (i % pointCount) + 1;
            vh.AddTriangle(0, i, next);
        }

        graphic.canvasRenderer.SetMesh(new Mesh());
        Mesh mesh = new Mesh();
        vh.FillMesh(mesh);
        graphic.canvasRenderer.SetMesh(mesh);
        graphic.canvasRenderer.SetColor(graphic.color);
    }

    string logPath = string.Empty;
    List<string> logList = new List<string>();

    void SaveToFile()
    {
        // 숫자들을 한 줄씩 문자열로 변환
        //string[] lines = logList.ConvertAll(f => f.ToString()).ToArray();

        // 한 번에 저장
        File.WriteAllLines(logPath, logList);
    }

    public void UpdateSwing(float angle)
    {
        var range = angleRanges[stepStage];

        if (!range.IsInLimitRange(angle))
        {
            ResetSwing();

            return;
        }

        if (range.IsInCheckRange(angle))
        {
            if (/*myScore[curStepNum] != -1 &&*/ ReferenceEquals(null, captureRealPoseFrontUser[curStepNum]))
            {
                //logList.Add($"front) {stepStage}, {angle}\n");
                captureRealPoseFrontUser[curStepNum] = CaptureTextureFromRawImage(rawImageFront);
            }

            if (/*myScore[curStepNum] != -1 &&*/ ReferenceEquals(null, captureRealPoseSideUser[curStepNum]))
            {
                //logList.Add($"side) {stepStage}, {angle}\n");
                captureRealPoseSideUser[curStepNum] = CaptureTextureFromRawImage(rawImageSide);
            }

            if (stepStage < SWINGSTEP.FINISH)
            {
                UserDataAdd(curStepNum);

                stepStage++;
                curStepNum++;
            }
            else
            {
                if (sensorProcess.iGetShoulderDir < swingStepData.dicFinish["GetShoulderDir"])
                {
                    UserDataAdd(curStepNum);

                    //SaveToFile();
                    isFinish = true;
                }
                //Finish 처리
                
                //isFinish = true;
                //stepStage = SWINGSTEP.READY;
            }
        }


        // ------------- 이슈있어서 Top이후 Finish 만 체크 ----------------
        //if (stepStage > SWINGSTEP.TOP)
        //{
        //    AngleRange finishRange = angleRanges[SWINGSTEP.FINISH];

        //    if (finishRange.IsInCheckRange(angle))
        //    {
        //        stepStage = SWINGSTEP.FINISH;

        //        isFinish = true;
        //        return;
        //    }
        //}

        //var range = angleRanges[stepStage];

        //if (!range.IsInLimitRange(angle))
        //{
        //    stepStage = SWINGSTEP.ADDRESS;

        //    return;
        //}

        //if (range.IsInCheckRange(angle))
        //{
        //    if (stepStage < SWINGSTEP.FINISH)
        //    {
        //        stepStage++;
        //    }
        //}
    }

    void EnsurePoseCapture(int stepIndex)
    {
        stepIndex = Mathf.Clamp(stepIndex, 0, 7);

        if (ReferenceEquals(null, captureRealPoseFrontUser[stepIndex]))
            captureRealPoseFrontUser[stepIndex] = CaptureTextureFromRawImage(rawImageFront);

        if (ReferenceEquals(null, captureRealPoseSideUser[stepIndex]))
            captureRealPoseSideUser[stepIndex] = CaptureTextureFromRawImage(rawImageSide);
    }

    public void ResetSwing()
    {
        stepStage = SWINGSTEP.ADDRESS;
        curStepNum = 0;

        myScore.Clear();
        myScore = Enumerable.Repeat(-1, 8).ToList();

        captureRealPoseFrontUser = Enumerable.Repeat<Texture2D>(null, 8).ToList();
        captureRealPoseSideUser = Enumerable.Repeat<Texture2D>(null, 8).ToList();
    }

    public SWINGSTEP GetCurStep()
    {
        return stepStage;
    }

    public void AnimateTotalGraph(List<int> targetScore, List<int> avgScore, float duration = 0.5f)
    {
        var list = new List<int>(targetScore);
        list.RemoveAt((int)SWINGSTEP.IMPACT);
        list.RemoveAt((int)SWINGSTEP.TOP);

        for (int i = 0; i < pointCount; i++)
        {
            int index = i;
            float start = 0f;
            float end = list[i];

            DOTween.To(() => start, x =>
            {
                currentScore[index] = x;

                if (index < m_MyScoreTexts.Length)
                    m_MyScoreTexts[index].text = $"{Mathf.RoundToInt(x)}%";

                DrawTotalGraph(m_MyLineRenderer, currentScore);
                FillTotalGraph(myFillGraphic, currentScore);

            }, end, duration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true)
            .OnComplete(() => {
                if (index == pointCount - 1)
                {
                    HighlightMaxMin();
                }
            });

            end = avgScore[i];

            DOTween.To(() => start, x => {
                currentAvgScore[index] = x;

                DrawTotalGraph(m_AvgLineRenderer, currentAvgScore);
                FillTotalGraph(avgFillGraphic, currentAvgScore);

            }, end, duration)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);
        }
    }

    private void HighlightMaxMin()
    {
        int maxIndex = 0;
        int minIndex = 0;

        for (int i = 1; i < pointCount; i++)
        {
            if (currentScore[i] > currentScore[maxIndex]) maxIndex = i;
            if (currentScore[i] < currentScore[minIndex]) minIndex = i;
        }

        for (int i = 0; i < pointCount; i++)
        {
            if (i >= m_MyScoreNameTexts.Length) continue;

            if (i == maxIndex)
            {
                m_GoodPoseText.text = Utillity.Instance.ConvertEnumToString((SWINGSTEP)maxIndex);
                m_MyScoreNameTexts[i].color = Utillity.Instance.HexToRGB(INI.Green500);
            }
            else if (i == minIndex)
            {
                m_BadPoseText.text = Utillity.Instance.ConvertEnumToString((SWINGSTEP)minIndex);
                m_MyScoreNameTexts[i].color = Utillity.Instance.HexToRGB(INI.Red);
            }
            else
                m_MyScoreNameTexts[i].color = Color.white;

            m_MyScoreNameTexts[i].SetAllDirty();
        }
    }

    private void DrawTimeline(List<int> rates)
    {
        float yMax = 215.0f;
        List<Vector2> points = new List<Vector2>();
        List<Vector2> drawPoints = new List<Vector2>();

        for (int i = 0; i < m_DotObjects.Length; i++)
            m_DotObjects[i].SetActive(false);
        
        for (int i = 0; i < rates.Count; i++)
        {
            int rate = rates[i];
            float y = (rate / 100.0f) * yMax;
            
            RectTransform rt = m_DotObjects[i].GetComponent<RectTransform>();
            Vector2 anchoredPos = rt.anchoredPosition;
            anchoredPos.y = y;
            rt.anchoredPosition = anchoredPos;

            m_DotObjects[i].SetActive(true);
           
            Image img = m_DotObjects[i].GetComponent<Image>();

            if (img != null)
            {
#if UNITY_EDITOR
                img.color = rate <= 29 ? Color.red : (rate <= 79 ? Color.yellow : Color.green);
#else
                img.color = rate <= 29 ? Utillity.Instance.HexToRGB(INI.Red) : (rate <= 79 ? Utillity.Instance.HexToRGB(INI.Yellow) : Utillity.Instance.HexToRGB(INI.Green500));
#endif
            }

            m_DotTexts[i].text = $"{rate}%";

            drawPoints.Add(new Vector2(rt.anchoredPosition.x, y));
        }

        m_PoseTimeLineRenderer.Points = drawPoints.ToArray();
        m_PoseTimeLineRenderer.SetAllDirty();

        for (int i = rates.Count; i < m_DotObjects.Length; i++)
            m_DotObjects[i].SetActive(false);
    }

    public void AnimateProgress(int value, float duration = 1.0f)
    {
        m_CurPoseScoreText.text = $"0<size=60%>%</size>";

#if UNITY_EDITOR
        m_PoseProgressImg.color = value <= 29 ? Color.red : (value <= 79 ? Color.yellow : Color.green);
#else
        m_PoseProgressImg.color = value <= 29 ? Utillity.Instance.HexToRGB(INI.Red) : (value <= 79 ? Utillity.Instance.HexToRGB(INI.Yellow) : Utillity.Instance.HexToRGB(INI.Green500));
#endif

        m_PoseProgressImg.fillAmount = 0f;
        m_PoseProgressImg.DOKill();
        m_PoseProgressImg.DOFillAmount(value / 100f, duration).SetEase(Ease.OutQuad);

        DOTween.To(() => 0, x => {
            m_CurPoseScoreText.text = $"{x:0}<size=60%>%</size>";
        }, value, duration).SetEase(Ease.OutQuad);
    }

    public void ToggleModelView(bool front, bool is3DModel)
    {
        if (_isViewAnimating /*|| _isFront == front*/) return;

        m_DirToggleCover.SetActive(true);
        _isViewAnimating = true;

        RectTransform toProFront = _isFront ? (is3DModel ? m_SideProView : m_SideProReal) : (is3DModel ? m_FrontProView : m_FrontProReal);
        RectTransform toProBack = _isFront ? (is3DModel ? m_FrontProView : m_FrontProReal) : (is3DModel ? m_SideProView : m_SideProReal);

        RectTransform toUserFront = _isFront ? (is3DModel ? m_SideUserView : m_SideUserReal) : (is3DModel ? m_FrontUserView : m_FrontUserReal);
        RectTransform toUserBack = _isFront ? (is3DModel ? m_FrontUserView : m_FrontUserReal) : (is3DModel ? m_SideUserView : m_SideUserReal);

        proFrontPos = is3DModel ? m_FrontProView.anchoredPosition : m_FrontProReal.anchoredPosition;
        proFrontSize = is3DModel ? m_FrontProView.sizeDelta : m_FrontProReal.sizeDelta;
        proBackPos = is3DModel ? m_SideProView.anchoredPosition : m_SideProReal.anchoredPosition;
        proBackSize = is3DModel? m_SideProView.sizeDelta : m_SideProReal.sizeDelta;

        userFrontPos = is3DModel ? m_FrontUserView.anchoredPosition : m_FrontUserReal.anchoredPosition;
        userFrontSize = is3DModel ? m_FrontUserView.sizeDelta : m_FrontUserReal.sizeDelta;
        userBackPos = is3DModel ? m_SideUserView.anchoredPosition : m_SideUserReal.anchoredPosition;
        userBackSize = is3DModel ? m_SideUserView.sizeDelta : m_SideUserReal.sizeDelta;

        Vector2 toProFrontPos = _isFront ? proFrontPos : proBackPos;
        Vector2 toProFrontSize = _isFront ? proFrontSize : proBackSize;
        Vector2 toProBackPos = _isFront ? proBackPos : proFrontPos;
        Vector2 toProBackSize = _isFront ? proBackSize : proFrontSize;

        Vector2 toUserFrontPos = _isFront ? userFrontPos : userBackPos;
        Vector2 toUserFrontSize = _isFront ? userFrontSize : userBackSize;
        Vector2 toUserBackPos = _isFront ? userBackPos : userFrontPos;
        Vector2 toUserBackSize = _isFront ? userBackSize : userFrontSize;

        toProFront.SetAsLastSibling();
        toUserFront.SetAsLastSibling();

        toProFront.DOAnchorPos(toProFrontPos, 0.35f).SetEase(Ease.InOutCubic);
        toProFront.DOSizeDelta(toProFrontSize, 0.35f).SetEase(Ease.InOutCubic);

        toUserFront.DOAnchorPos(toUserFrontPos, 0.35f).SetEase(Ease.InOutCubic);
        toUserFront.DOSizeDelta(toUserFrontSize, 0.35f).SetEase(Ease.InOutCubic).OnComplete(() => {
            _isFront = front;
            _isViewAnimating = false;
            m_DirToggleCover.SetActive(false);
        });

        toProBack.DOAnchorPos(toProBackPos, 0.35f).SetEase(Ease.InOutCubic);
        toProBack.DOSizeDelta(toProBackSize, 0.35f).SetEase(Ease.InOutCubic);

        toUserBack.DOAnchorPos(toUserBackPos, 0.35f).SetEase(Ease.InOutCubic);
        toUserBack.DOSizeDelta(toUserBackSize, 0.35f).SetEase(Ease.InOutCubic);
    }

    private IEnumerator ModelAnimation(bool isAnim, params Animator[] anims)
    {
        if(anims ==  null || anims.Length == 0)
            yield break;

        if(isAnim)
        {
            while(true)
            {
                float elapsedTime = 0f;

                while (elapsedTime < 2.0f)
                {
                    if (!_isTotalAnalyze || !_is3DModel)
                    {
                        for (int i = 0; i < anims.Length; i++)
                            anims[i].SetFloat("SwingValue", 0.0f);
                        break;
                    }

                    for (int i = 0; i < anims.Length; i++)
                        anims[i].SetFloat("SwingValue", Mathf.Lerp(0, 0.99f, elapsedTime / 2.0f));

                    elapsedTime += Time.deltaTime;

                    yield return null;
                }

                if (!_isTotalAnalyze || !_is3DModel)
                {
                    for (int i = 0; i < anims.Length; i++)
                        anims[i].SetFloat("SwingValue", 0.0f);
                    break;
                }

                for (int i = 0; i < anims.Length; i++)
                    anims[i].SetFloat("SwingValue", 0.99f);

                yield return new WaitForSeconds(1.0f);
            }
        }
        else
        {
            if(!_isTotalAnalyze)
            {
                if (selectStep == SWINGSTEP.ADDRESS)
                {
                    for (int i = 0; i < anims.Length; i++)
                        anims[i].SetFloat("SwingValue", 0.0f);
                }
                else if (selectStep == SWINGSTEP.TAKEBACK)
                {
                    for (int i = 0; i < anims.Length; i++)
                        anims[i].SetFloat("SwingValue", 0.23f);
                }
                else if (selectStep == SWINGSTEP.BACKSWING)
                {
                    for (int i = 0; i < anims.Length; i++)
                        anims[i].SetFloat("SwingValue", 0.35f);
                }
                else if (selectStep == SWINGSTEP.TOP)
                {
                    for (int i = 0; i < anims.Length; i++)
                        anims[i].SetFloat("SwingValue", 0.5f);
                }
                else if (selectStep == SWINGSTEP.DOWNSWING)
                {
                    for (int i = 0; i < anims.Length; i++)
                        anims[i].SetFloat("SwingValue", 0.61f);
                }
                else if (selectStep == SWINGSTEP.IMPACT)
                {
                    for (int i = 0; i < anims.Length; i++)
                        anims[i].SetFloat("SwingValue", 0.661f);
                }
                else if (selectStep == SWINGSTEP.FOLLOW)
                {
                    for (int i = 0; i < anims.Length; i++)
                        anims[i].SetFloat("SwingValue", 0.76f);
                }
                else if (selectStep == SWINGSTEP.FINISH)
                {
                    for (int i = 0; i < anims.Length; i++)
                        anims[i].SetFloat("SwingValue", 0.99f);
                }
            }

            yield return null;
        }
    }

    private void VideoControl()
    {
        if (_isTotalAnalyze)
        {
            m_FrontProRealRaw.texture = m_RealProFrontVideo.targetTexture;
            m_SideProRealRaw.texture = m_RealProSideVideo.targetTexture;

            m_FrontUserRealRaw.texture = m_RealUserFrontVideo.targetTexture;
            m_SideUserRealRaw.texture = m_RealUserSideVideo.targetTexture;

            m_RealProFrontVideo.Play();
            m_RealProSideVideo.Play();
            m_RealUserFrontVideo.Play();
            m_RealUserSideVideo.Play();
        }
        else
        {
            m_RealProFrontVideo.Stop();
            m_RealProSideVideo.Stop();
            m_RealUserFrontVideo.Stop();
            m_RealUserSideVideo.Stop();

            if(selectStep == SWINGSTEP.BACKSWING)
            {
                m_FrontProRealRaw.texture = captureRealPoseFrontPro[3];
                m_SideProRealRaw.texture = captureRealPoseSidePro[3];

                m_FrontUserRealRaw.texture = captureRealPoseFrontUser[3];
                m_SideUserRealRaw.texture = captureRealPoseSideUser[3];
            }
            else
            {
                m_FrontProRealRaw.texture = captureRealPoseFrontPro[(int)selectStep];
                m_SideProRealRaw.texture = captureRealPoseSidePro[(int)selectStep];

                m_FrontUserRealRaw.texture = captureRealPoseFrontUser[(int)selectStep];
                m_SideUserRealRaw.texture = captureRealPoseSideUser[(int)selectStep];
            }
        }
    }

    public void Onclick_Button(string name)
    {
        switch (name)
        {
            case "Home":
                GameManager.Instance.SelectedSceneName = string.Empty;
                SceneManager.LoadScene("ModeSelect");
                break;

            case "Option":
                GameManager.Instance.OnClick_OptionPanel();
                break;
        }
    }

    public void OnClick_Result()
    {
        m_ProNameText.text = $"{GolfProDataManager.Instance.SelectProData.infoData.name} 프로";

        myScore[(int)SWINGSTEP.TOP] = 0;
        myScore[(int)SWINGSTEP.IMPACT] = 0;

        PanelResult.SetActive(true);

        m_ResultMainToggles[0].isOn = true;
        m_ResultMainToggles[0].onValueChanged.Invoke(true);

        m_ModelChangeToggle.onValueChanged.Invoke(false);
        m_RealVideoSpeedToggle.onValueChanged.Invoke(false);
    }

    public void OnClick_Video()
    {

    }

    public void OnClick_Retry()
    {
        //BlurBack.SetActive(true);
        SetCoachinggStep(COACHINGSTEP.GRIP);
    }

    public void OnClick_RetryCancel()
    {
        BlurBack.SetActive(false);
    }

    public void OnClick_RetryApply()
    {
        BlurBack.SetActive(false);

        SetCoachinggStep(COACHINGSTEP.GRIP);
    }

    public void OnClick_DetailAnalyzePanel()
    {
        _isDetailPanelOpen = !_isDetailPanelOpen;

        BlurBack.SetActive(_isDetailPanelOpen);
        m_DetailAnalyzePanel.DOAnchorPosY(_isDetailPanelOpen ? detailOpenPos.y : 0, 0.3f).SetEase(_isDetailPanelOpen ? Ease.InCubic : Ease.OutCubic);
    }

    public void OnValueChanged_ResultMainToggle(bool isOn)
    {
        if (m_ResultMainTG.GetFirstActiveToggle() == null)
            return;

        int num = m_ResultMainTG.GetFirstActiveToggle().GetComponent<UIValueObject>().intValue;
        
        if (!isOn)
            return;

        if (num == 0)
        {
            _isTotalAnalyze = true;
            m_AnalyzeGroup.SetActive(true);
            m_Lesson.SetActive(false);
            m_AnalyzeTotal.SetActive(true);
            m_AnalyzePose.SetActive(false);

            OnValueChanged_ModelChange(_is3DModel);

            AnimateMatchingRate(0.0f, (myScore.Sum() / (myScore.Count - 2)) * 0.01f);
            AnimateTotalGraph(myScore, avgScore, 1.1f);

            StartCoroutine(ModelAnimation(true, m_ProModelAni, m_UserModelAni));
        }
        else if (num == 1)
        {
            _isTotalAnalyze = false;
            m_AnalyzeGroup.SetActive(true);
            m_Lesson.SetActive(false);
            m_AnalyzeTotal.SetActive(false);
            m_AnalyzePose.SetActive(true);

            if (!m_ResultPoseToggles[0].isOn)
                m_ResultPoseToggles[0].isOn = true;

            m_FrontUserReal.localScale = Vector3.one;
            m_SideUserReal.localScale = Vector3.one;

            AnimateProgress(addressTimeline.Count > 0 ? addressTimeline[addressTimeline.Count - 1] : 0);
            DrawTimeline(addressTimeline);

            StartCoroutine(ModelAnimation(false, m_ProModelAni, m_UserModelAni));
        }
        else if(num == 2)
        {
            _isTotalAnalyze = false;
            m_AnalyzeGroup.SetActive(false);
            m_Lesson.SetActive(true);
            m_AnalyzeTotal.SetActive(false);
            m_AnalyzePose.SetActive(false);

            StartCoroutine(ModelAnimation(false, m_ProModelAni, m_UserModelAni));
        }

        VideoControl();
    }

    public void OnValueChanged_ResultPoseToggle(bool isOn)
    {
        if (m_ResultPoseTG.GetFirstActiveToggle() == null)
            return;

        int num = m_ResultPoseTG.GetFirstActiveToggle().GetComponent<UIValueObject>().intValue;

        if (!isOn)
            return;

        selectStep = (SWINGSTEP)num;

        switch (num)
        {
            case 0:
                AnimateProgress(addressTimeline.Count > 0 ? addressTimeline[addressTimeline.Count - 1] : 0);
                DrawTimeline(addressTimeline);
                break;

            case 1:
                AnimateProgress(takebackTimeline.Count > 0 ? takebackTimeline[takebackTimeline.Count - 1] : 0);
                DrawTimeline(takebackTimeline);
                break;

            case 2:
                AnimateProgress(backswingTimeline.Count > 0 ? backswingTimeline[backswingTimeline.Count - 1] : 0);
                DrawTimeline(backswingTimeline);
                break;

            case 3:
                AnimateProgress(topTimeline.Count > 0 ? topTimeline[topTimeline.Count - 1] : 0);
                DrawTimeline(topTimeline);
                break;

            case 4:
                AnimateProgress(downswingTimeline.Count > 0 ? downswingTimeline[downswingTimeline.Count - 1] : 0);
                DrawTimeline(downswingTimeline);
                break;

            case 5:
                AnimateProgress(impactTimeline.Count > 0 ? impactTimeline[impactTimeline.Count - 1] : 0);
                DrawTimeline(impactTimeline);
                break;

            case 6:
                AnimateProgress(followTimeline.Count > 0 ? followTimeline[followTimeline.Count - 1] : 0);
                DrawTimeline(followTimeline);
                break;

            case 7:
                AnimateProgress(finishTimeline.Count > 0 ? finishTimeline[finishTimeline.Count - 1] : 0);
                DrawTimeline(finishTimeline);
                break;
        }

        StartCoroutine(ModelAnimation(false, m_ProModelAni, m_UserModelAni));
        VideoControl();
    }

    public void OnValueChanged_ToggleDirection(bool isOn)
    {
        if (m_ModelDirectionTG.GetFirstActiveToggle() == null)
            return;

        int num = m_ModelDirectionTG.GetFirstActiveToggle().GetComponent<UIValueObject>().intValue;

        if (!isOn)
            return;

        switch (num)
        {
            case 0:
                if (!_isFront)
                    ToggleModelView(true, _is3DModel);
                break;

            case 1:
                if (_isFront)
                    ToggleModelView(false, _is3DModel);
                break;
        }
    }

    public void OnValueChanged_ModelChange(bool isOn)
    {
        if (!_isFront)
        {
            ToggleModelView(true, _is3DModel);

            m_ModelDirectionToggles[0].isOn = true;
        }

        _is3DModel = isOn;

        m_Models[0].SetActive(isOn);
        m_Models[1].SetActive(!isOn);

        if (isOn)
            StartCoroutine(ModelAnimation(true, m_ProModelAni, m_UserModelAni));
        else
        {
            if(_isTotalAnalyze)
            {
                int uid = GolfProDataManager.Instance.SelectProData.uid;

                string proPath = GolfProDataManager.Instance.SelectProData.videoData.
                    Where(v => v.direction == EPoseDirection.Front && v.sceneType == ESceneType.ProSelect).
                    Select(v => v.path).FirstOrDefault();

                m_RealProFrontVideo.url = GameManager.Instance.LoadVideoURL($"{INI.proVideoPath}{uid}/{proPath}");

                proPath = GolfProDataManager.Instance.SelectProData.videoData.
                    Where(v => v.direction == EPoseDirection.Side && v.sceneType == ESceneType.ProSelect).
                Select(v => v.path).FirstOrDefault();

                m_RealProSideVideo.url = GameManager.Instance.LoadVideoURL($"{INI.proVideoPath}{uid}/{proPath}");

                m_RealUserFrontVideo.url = outputPathFront;
                m_RealUserSideVideo.url = outputPathSide;
            }
            else
            {
                VideoControl();
            }
        }
    }

    public void OnValueChanged_RealVideoSpeed(bool isOn)
    {
        if(isOn)
        {
            m_RealProFrontVideo.playbackSpeed = 0.25f;
            m_RealProSideVideo.playbackSpeed = 0.25f;
            m_RealUserFrontVideo.playbackSpeed = 0.25f;
            m_RealUserSideVideo.playbackSpeed = 0.25f;
        }
        else
        {
            m_RealProFrontVideo.playbackSpeed = 1.0f;
            m_RealProSideVideo.playbackSpeed = 1.0f;
            m_RealUserFrontVideo.playbackSpeed = 1.0f;
            m_RealUserSideVideo.playbackSpeed = 1.0f;
        }
    }

    //-----------------------------------------------------------------------
    // 영상 처리부
    //-----------------------------------------------------------------------
    //TODO:AICoaching에 사용 된 Director에 중복으로 사용되는 문제 개선필요

    private Texture2D CaptureTextureFromRawImage(RawImage raw)
    {
        RenderTexture rt = RenderTexture.GetTemporary(raw.texture.width, raw.texture.height, 0);
        Graphics.Blit(raw.texture, rt);

        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return tex;
    }

    bool CaptureFrameFront()//RenderTexture renderTex)
    {
        Texture sourceTexFront = rawImageFront.texture;
        widthFront = sourceTexFront.width;
        heightFront = sourceTexFront.height;
        RenderTexture renderTexFront = RenderTexture.GetTemporary(widthFront, heightFront, 0);
        Graphics.Blit(sourceTexFront, renderTexFront);

        RenderTexture.active = renderTexFront;
        Texture2D captureFront = new Texture2D(widthFront, heightFront, TextureFormat.RGB24, false);
        captureFront.ReadPixels(new Rect(0, 0, widthFront, heightFront), 0, 0);
        captureFront.Apply();

        if (checkTakeback == false)
        {
            checkTakebackFrame++;

            if (framesFront.Count > 90)
            {
                checkTakebackFrame = 0;
                framesFront.Clear();
                framesSide.Clear();//사이드도 같이 삭제
                captureRealPoseFrontUser.Clear();
                captureRealPoseSideUser.Clear();
            }

            if (_handCheck == false)
            {
                RenderTexture.active = null;
                RenderTexture.ReleaseTemporary(renderTexFront);

                return false;
            }
        }
        
        framesFront.Add(captureFront);

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(renderTexFront);

        return true;
    }

    void CaptureFrameSide()//RenderTexture renderTex)
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

        framesSide.Add(captureSide);

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(renderTexSide);
    }

    IEnumerator CaptureFrames()
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

            if (checkImpact == true)
            //if (isFinish == true)
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
                    SetCoachinggStep(COACHINGSTEP.GRIP);
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

        SetCoachinggStep(COACHINGSTEP.SWINGEND);
        //txtDebug.text += "프레임 캡쳐 종료" + "\r\n";
    }



    private IEnumerator SendFramesToFFmpeg(string output, int width, int height, List<Texture2D> frames, Action ComplateEvent)
    {
        //txtDebug.text += "영상 생성 시작\n";
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
            process.StartInfo = new ProcessStartInfo
            {
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

            //txtAngle.text = (flip ? "○" : "●");
            //txtAngle.text += _lastHandDir.ToString("0") + "\r\n" + AvgVisible.ToString("0.00") + " " + webcamclient.poseData1.Count.ToString("00");
            //txtAngle.color = _handCheck == false ? Color.red : Color.green;
            ///flip = !flip;

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

    //-----------------------------------------------------------------------
    // MOCAP 처리부 
    //-----------------------------------------------------------------------
    Texture2D LoadTextureFromFile(string filePath)
    {
        if (!File.Exists(filePath)) return null;

        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D tex = new Texture2D(2, 2);
        if (tex.LoadImage(fileData))
            return tex;

        return null;
    }

    void SetResultData()
    {
        DicUserSwingData.Clear();
        //Front
        DicUserSwingData.Add("GetHandDir", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
        DicUserSwingData.Add("GetHandDistance", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
        DicUserSwingData.Add("GetShoulderDistance", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });

        DicUserSwingData.Add("GetSpineDir", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
        DicUserSwingData.Add("GetShoulderAngle", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
        DicUserSwingData.Add("GetFootDisRate", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
        DicUserSwingData.Add("GetWeight", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
        DicUserSwingData.Add("GetForearmAngle", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
        DicUserSwingData.Add("GetElbowFrontDir", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
        //Side
        DicUserSwingData.Add("GetHandSideDir", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
        DicUserSwingData.Add("GetWaistSideDir", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
        DicUserSwingData.Add("GetKneeSideDir", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
        DicUserSwingData.Add("GetElbowSideDir", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
        DicUserSwingData.Add("GetArmpitDir", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
        //combine
        DicUserSwingData.Add("GetShoulderDir", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
        DicUserSwingData.Add("GetPelvisDir", new int[8] { 0, 0, 0, 0, 0, 0, 0, 0 });
    }

    void UserDataAdd(int step)
    {
        //Front
        DicUserSwingData["GetHandDir"][step] = sensorProcess.iGetHandDir;//(int)mocapFront.GetHandDir();
        DicUserSwingData["GetHandDistance"][step] = sensorProcess.iGetHandDistance;//(float)mocapFront.GetHandDistance();
        DicUserSwingData["GetShoulderDistance"][step] = sensorProcess.iGetShoulderDistance;//(float)mocapFront.GetHandDistance();
        
        DicUserSwingData["GetSpineDir"][step] = sensorProcess.iGetSpineDir;//(int)mocapFront.GetSpineDir();
        DicUserSwingData["GetShoulderAngle"][step] = sensorProcess.iGetShoulderAngle;//(int)mocapFront.GetShoulderAngle();
        DicUserSwingData["GetFootDisRate"][step] = sensorProcess.iGetFootDisRate;//(int)mocapFront.GetFootDisRate();
        DicUserSwingData["GetWeight"][step] = sensorProcess.iGetWeight;//(float)mocapFront.GetWeight();
        DicUserSwingData["GetForearmAngle"][step] = sensorProcess.iGetForearmAngle;//(float)mocapFront.GetForearmAngle();
        DicUserSwingData["GetElbowFrontDir"][step] = sensorProcess.iGetElbowFrontDir;//(float)mocapFront.GetForearmAngle();

        //Side
        DicUserSwingData["GetHandSideDir"][step] = sensorProcess.iGetHandSideDir;//(int)mocapSide.GetHandSideDir();
        DicUserSwingData["GetWaistSideDir"][step] = sensorProcess.iGetWaistSideDir;//(int)mocapSide.GetWaistSideDir();
        DicUserSwingData["GetKneeSideDir"][step] = sensorProcess.iGetKneeSideDir;//(int)mocapSide.GetKneeSideDir();
        DicUserSwingData["GetElbowSideDir"][step] = sensorProcess.iGetElbowSideDir;//(int)mocapSide.GetElbowSideDir();
        DicUserSwingData["GetArmpitDir"][step] = sensorProcess.iGetArmpitDir;//(int)mocapSide.GetArmpitDir();

        //combine
        DicUserSwingData["GetShoulderDir"][step] = sensorProcess.iGetShoulderDir;//(int)mocapFront.GetShoulderDir();
        DicUserSwingData["GetPelvisDir"][step] = sensorProcess.iGetPelvisDir;//(int)mocapFront.GetPelvisDir();

        string[] selectedKeys;

        switch (step)
        {
            case 0: selectedKeys = new string[] { "GetShoulderAngle", "GetWaistSideDir", "GetKneeSideDir" }; break;
            case 1: selectedKeys = new string[] { "GetForearmAngle", "GetShoulderAngle",  }; break;
            case 2: selectedKeys = new string[] { "GetShoulderDir", "GetPelvisDir", "GetForearmAngle", "GetWeight" }; break;
            case 3: selectedKeys = new string[] { "GetShoulderDir", "GetPelvisDir", "GetForearmAngle", "GetWeight" }; break;
            case 4: selectedKeys = new string[] { "GetShoulderDir", "GetHandSideDir", "GetSpineDir", "GetWeight" }; break;
            //case 5: selectedKeys = new string[] { "GetShoulderDir", "GetPelvisDir" }; break;
            //case 6: selectedKeys = new string[] { "GetPelvisDir" }; break;
            //case 7: selectedKeys = new string[] { "GetPelvisDir" }; break;
            default: selectedKeys = new string[] { }; break;
        }

        if (step <= 4)
        {
            myScore[step] = GetUserStepAverage(step, selectedKeys);
        }
        else
        {
            //int avg = 0;
            //int count = 0;

            //for (int i = 0; i <= 4; i++)
            //{
            //    avg += myScore[i];
            //    count++;
            //}

            //int baseScore = (count > 0) ? Mathf.RoundToInt((float)avg / count) : 0;

            int baseTotal = 0;

            for (int i = 0; i <= 4; i++)
            {
                baseTotal += myScore[i];
            }
            
            int baseScore = Mathf.RoundToInt(baseTotal / 5f);

            int variance = UnityEngine.Random.Range(-7, 8);
            myScore[step] = Mathf.Clamp(baseScore + variance, 0, 100);
        }

        //myScore[step] = GetUserStepAverage(step, selectedKeys);

        switch(step)
        {
            case 0: addressTimeline.Add(myScore[step]); break;
            case 1: takebackTimeline.Add(myScore[step]); break;
            case 2: backswingTimeline.Add(myScore[step]); break;
            case 3: topTimeline.Add(myScore[step]); break;
            case 4: downswingTimeline.Add(myScore[step]); break;
            case 5: impactTimeline.Add(myScore[step]); break;
            case 6: followTimeline.Add(myScore[step]); break;
            //case 7:
            //    finishTimeline.Clear();
            //    finishTimeline.Add(myScore[step]); 
        }
    }

    int GetUserStepAverage(int step, params string[] selectedKeys)
    {
        Dictionary<string, int> proDic = GetProStepDic(step);

        if (proDic == null)
            return 0;

        float total = 0;
        int count = 0;

        foreach (var key in selectedKeys)
        {
            if (!DicUserSwingData.ContainsKey(key) || !proDic.ContainsKey(key)) continue;

            float proValue = proDic[key];
            float userValue = DicUserSwingData[key][step];
            float tolerance = ErrorMargins.ContainsKey(key) ? ErrorMargins[key] : 1f;

            float diff = Mathf.Abs(userValue - proValue);
            float score;

            float angleThreshold = 5f;

            if (diff <= angleThreshold)
            {
                score = 100f;
            }
            else
            {
                float normalized = Mathf.Clamp01((diff - angleThreshold) / (tolerance - angleThreshold));
                score = 100 - Mathf.Pow(normalized, 0.5f) * 100.0f;
            }

            //float normalized = Mathf.Clamp01(diff / tolerance);

            //float score = normalized <= 0.05f ? 100 : 100 - Mathf.Pow(normalized, 0.5f) * 100.0f;

            //score = Mathf.Max(score, 40);

            score = Mathf.Clamp(score, 40, 100);

            //float score = 100 - (diff / tolerance * 100);
            if (Mathf.Approximately(score, 40))
            {
                score += UnityEngine.Random.Range(-5, 10);
                score = Mathf.Clamp(score, 35, 50);
            }

            total += score;
            count++;
        }

        int stepScore = (count > 0) ? Mathf.RoundToInt(total / count) : 0;

        if (stepScore == 0 && step > 0)
        {
            float prevTotal = 0;
            int prevCount = 0;
            for (int s = 0; s < step; s++)
            {
                prevTotal += myScore[s];
                prevCount++;
            }
            stepScore = (prevCount > 0) ? Mathf.RoundToInt(prevTotal / prevCount) : 0;
        }

        return stepScore;
    }

    Dictionary<string, int> GetProStepDic(int step)
    {
        switch (step)
        {
            case 0: return swingStepData.dicAddress;
            case 1: return swingStepData.dicTakeback;
            case 2: return swingStepData.dicBackswing;
            case 3: return swingStepData.dicTop;
            case 4: return swingStepData.dicDownswing;
            case 5: return swingStepData.dicImpact;
            case 6: return swingStepData.dicFollow;
            case 7: return swingStepData.dicFinish;
            default: return null;
        }
    }
}
