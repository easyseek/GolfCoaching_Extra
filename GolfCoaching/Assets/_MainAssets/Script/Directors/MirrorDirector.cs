using DG.Tweening;
using Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;
using Debug = UnityEngine.Debug;

public class MirrorDirector : MonoBehaviour
{
    [SerializeField] webcamclient webcamclient;
    [SerializeField] TextMeshProUGUI txtAngle;
    [SerializeField] TextMeshProUGUI txtDebug;

    [Header("* Viewer Position")]
    [SerializeField] RectTransform PlayerViewFront;
    [SerializeField] RectTransform PlayerViewSide;
    [SerializeField] RectTransform ProVideoFront;
    [SerializeField] RectTransform ProVideoSide;
    [SerializeField] RectTransform ReplayFront;
    [SerializeField] RectTransform ReplaySide;

    [Header("* Video Player")]
    [SerializeField] VideoPlayer videoProFront;
    [SerializeField] VideoPlayer videoProSide;
    [SerializeField] VideoPlayerControlMirror VideoPlayerControl;
    [SerializeField] VideoPlayerControlMirror proVideoPlayerControl;

    [Header("* Loading")]
    [SerializeField] RectTransform ReplayReadyUpdown;
    [SerializeField] RectTransform ReplayReadySide;
    [SerializeField] RectTransform ReplayProcessUpdown;
    [SerializeField] GameObject BlurBackUpdown;
    [SerializeField] RectTransform ReplayProcessSide;
    [SerializeField] GameObject BlurBackSide;

    [Header("* Layout Option")]
    [SerializeField] Toggle tglProVideo;
    [SerializeField] Toggle tglReplay;
    [SerializeField] Toggle tglAlone;
    [SerializeField] Button btnSwap;
    [SerializeField] Toggle tglUpdown;

    [Header("* Replay Viewer")]
    [SerializeField] MirrorReplayListContoller mirrorReplayListContoller;

    [Header("* Signal")]
    [SerializeField] Image imgSignal;

    bool _layoutProcess = false;
    bool _isSwap = true;
    bool _isReplay = false;
    bool _isUpdown = true;

    float AvgVisible = 0;

    bool proFrontEnd = false;
    bool proSideEnd = false;

    float _lastHandDir;
    bool _handCheck = false;

    [Space(5)]
    [SerializeField] GameObject ReplayUserInfo;
    [SerializeField] RawImage rawImageFront;
    [SerializeField] RawImage rawImageSide;
    //int fps = 30;
    bool _isReplayUserInfo = false;
    //public int durationSeconds = 5;

    private List<Texture2D> framesFront = new();
    private List<Texture2D> framesSide = new();
    private bool isRecording = false;
    int widthFront;
    int heightFront;
    int widthSide;
    int heightSide;
    [SerializeField] TextMeshProUGUI txtButtonREC;
    bool checkTakeback = false;
    int checkTakebackFrame = 0;
    bool checkImpact = false;
    

    bool _replayReadyFront = false;
    bool _replayReadySide = false;

    string outputPathFront = string.Empty;
    string outputPathSide = string.Empty;


    enum RECODESTEP
    {
        READY = 0,
        RECORD,
        RECORDEND,
        MAKEFILE,
        MAKEFILEEND,
        REPLAY,
        REPLAYEND
    }
    RECODESTEP recStep = RECODESTEP.READY;

    public class ReplayInfo
    {
        public DateTime recordTime;
        public int replayIndex;
        public Texture2D thumbnail;
        public string frontPath;
        public string sidePath;

        public ReplayInfo(int index, string front, string side, Texture2D thumbnail)
        {
            recordTime = DateTime.Now;
            replayIndex = index;
            frontPath = front;
            sidePath = side;
            this.thumbnail = thumbnail; 
        }
    }
    int currentIndex = -1;
    Queue <ReplayInfo > qReplayInfos = new Queue<ReplayInfo>();

    void Start()
    {
        Application.targetFrameRate = 30;
        QualitySettings.vSyncCount = 0;

        Init();

        StartCoroutine(CoLandmarkProcesss());

        StartCoroutine(CheckPose());

        StartCoroutine(LeaveUserCheck());

        GameManager.Instance?.SetOptionPanel();
    }

    private void Init()
    {
        outputPathFront = Path.Combine(Application.persistentDataPath, "outputfront_{0}.mp4");
        outputPathSide = Path.Combine(Application.persistentDataPath, "outputside_{0}.mp4");

        int uid = GolfProDataManager.Instance.SelectProData.uid;

        string proPath = GolfProDataManager.Instance.SelectProData.videoData.
            Where(v => v.direction == EPoseDirection.Front && v.sceneType == ESceneType.ProSelect).
            Select(v => v.path).FirstOrDefault();

        videoProFront.url = GameManager.Instance.LoadVideoURL($"{INI.proVideoPath}{uid}/{proPath}");

        proPath = GolfProDataManager.Instance.SelectProData.videoData.
            Where(v => v.direction == EPoseDirection.Side && v.sceneType == ESceneType.ProSelect).
        Select(v => v.path).FirstOrDefault();

        videoProSide.url = GameManager.Instance.LoadVideoURL($"{INI.proVideoPath}{uid}/{proPath}");

        videoProFront.loopPointReached += OnProVideoEndFront;
        videoProSide.loopPointReached += OnProVideoEndSide;

        tglProVideo.onValueChanged.AddListener(OnValueChanged_Toggles);
        tglReplay.onValueChanged.AddListener(OnValueChanged_Toggles);
        tglAlone.onValueChanged.AddListener(OnValueChanged_Toggles);

        tglUpdown.onValueChanged.AddListener(OnValueChanged_Toggles);
        
        btnSwap.onClick.AddListener(OnClick_Swap);

        //기본
        for (int i = 0; i < 6; i++)
        {
            string frontPath = string.Format(outputPathFront, i);
            string sidePath = string.Format(outputPathSide, i);
            if (File.Exists(frontPath))
                File.Delete(frontPath);
            if (File.Exists(sidePath))
                File.Delete(sidePath);
        }

        imgSignal.enabled = false;
    }

    IEnumerator LeaveUserCheck()
    {
        float min = 0;

        while(true)
        {
            yield return new WaitForSeconds(1);
            if (webcamclient.visibilitySide > 0.25f)
                min = 0;
            else
            {
                min++;

                if (min >= 60) //1분뒤 시작화면으로
                {
                    GameManager.Instance.SelectedSceneName = string.Empty;
                    SceneManager.LoadScene("Login");
                }
            }
        }
    }

    public void Onclick_Button()
    {
        GameObject obj = EventSystem.current.currentSelectedGameObject;

        switch (obj.name)
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

    public void OnClick_ReplayViewer()
    {
        List<ReplayInfo> replayInfo = qReplayInfos.ToList();
        replayInfo.Reverse();

        VideoPlayerControl.StopVideo();
        mirrorReplayListContoller.SetReplays(replayInfo);
    }

    public void OnValueChanged_ProVideo(bool isOn)
    {
        //ProVideoGroup.SetActive(isOn);
    }

    public void OnValueChanged_SideView(bool isOn)
    {
        _isReplay = isOn;
    }

    public void OnValueChanged_Toggles(bool isOn)
    {
        if (_layoutProcess)
            return;

        _layoutProcess = true;

        _isUpdown = tglUpdown.isOn;
        bool ProVideo = tglProVideo.isOn;

        _isReplay = tglReplay.isOn;

        if(ProVideo)
        {
            videoProFront.Play();
            videoProSide.Play();
            ReplayReadyUpdown.gameObject.SetActive(false);
            ReplayReadySide.gameObject.SetActive(false);
            ReplayUserInfo.SetActive(false);
            proVideoPlayerControl.gameObject.SetActive(true);
            proVideoPlayerControl.PlayVideo();
        }
        else if(_isReplay)
        {
            videoProFront.Stop();
            videoProSide.Stop();
            ReplayReadyUpdown.gameObject.SetActive(_isUpdown ? true : false);
            ReplayReadySide.gameObject.SetActive(!_isUpdown ? true : false);
            _isReplayUserInfo = false;
            ReplayUserInfo.SetActive(false);
            proVideoPlayerControl.gameObject.SetActive(false);
            proVideoPlayerControl.StopVideo();
        }
        else
        {
            videoProFront.Stop();
            videoProSide.Stop();
            ReplayReadyUpdown.gameObject.SetActive(false);
            ReplayReadySide.gameObject.SetActive(false);
            ReplayUserInfo.SetActive(false);
            proVideoPlayerControl.gameObject.SetActive(false);
            proVideoPlayerControl.StopVideo();
        }
        
        VideoPlayerControl.gameObject.SetActive(false);// this.VideoPlayerControl.GetPrepared());
        btnSwap.gameObject.SetActive(!tglAlone.isOn);
        StartCoroutine(SetViewerLayout(_isUpdown, ProVideo, _isReplay, _isSwap));
    }

    public void OnClick_Swap()
    {
        _isSwap = !_isSwap;
        //OnValueChanged_Toggles(true);
        bool ProVideo = tglProVideo.isOn;
        StartCoroutine(SetViewerLayout(_isUpdown, ProVideo, _isReplay, _isSwap));
    }

    IEnumerator SetViewerLayout(bool UpDown, bool ProVideo, bool Replay, bool swap)
    {
        if (UpDown)
        {            
            if (ProVideo || Replay)
            {
                if (swap == false)
                {
                    //상하, 프로ON
                    PlayerViewFront.anchoredPosition = new Vector2(0, 0);
                    PlayerViewSide.anchoredPosition = new Vector2(540, 0);
                    ProVideoFront.anchoredPosition = new Vector2(0, -920);
                    ProVideoSide.anchoredPosition = new Vector2(540, -920);
                    ReplayFront.anchoredPosition = new Vector2(0, -920);
                    ReplaySide.anchoredPosition = new Vector2(540, -920);

                    ReplayReadyUpdown.anchoredPosition = new Vector2(0, -920);
                    ReplayProcessUpdown.anchoredPosition = new Vector2(0, -920);
                }
                else
                {
                    //하상, 프로ON
                    PlayerViewFront.anchoredPosition = new Vector2(0, -920);
                    PlayerViewSide.anchoredPosition = new Vector2(540, -920);
                    ProVideoFront.anchoredPosition = new Vector2(0, 0);
                    ProVideoSide.anchoredPosition = new Vector2(540, 0);
                    ReplayFront.anchoredPosition = new Vector2(0, 0);
                    ReplaySide.anchoredPosition = new Vector2(540, 0);

                    ReplayReadyUpdown.anchoredPosition = new Vector2(0, 0);
                    ReplayProcessUpdown.anchoredPosition = new Vector2(0, 0);
                }
            }
            else
            {
                //상하, 프로OFF
                PlayerViewFront.anchoredPosition = new Vector2(0, -228);
                PlayerViewSide.anchoredPosition = new Vector2(540, -228);
                ProVideoFront.anchoredPosition = new Vector2(0, -920);
                ProVideoSide.anchoredPosition = new Vector2(540, -920);
                ReplayFront.anchoredPosition = new Vector2(0, -920);
                ReplaySide.anchoredPosition = new Vector2(540, -920);

                ReplayReadyUpdown.anchoredPosition = new Vector2(0, -920);
                ReplayProcessUpdown.anchoredPosition = new Vector2(0, -920);
            }
        }
        else
        {
            if (ProVideo || Replay)
            {
                if (swap == false)
                {
                    //좌우, 프로ON
                    PlayerViewFront.anchoredPosition = new Vector2(0, 0);
                    PlayerViewSide.anchoredPosition = new Vector2(0, -920);
                    ProVideoFront.anchoredPosition = new Vector2(540, 0);
                    ProVideoSide.anchoredPosition = new Vector2(540, -920);
                    ReplayFront.anchoredPosition = new Vector2(540, 0);
                    ReplaySide.anchoredPosition = new Vector2(540, -920);

                    ReplayReadySide.anchoredPosition = new Vector2(540, 0);
                    ReplayProcessSide.anchoredPosition = new Vector2(540, 0);
                }
                else
                {
                    //우좌, 프로ON
                    PlayerViewFront.anchoredPosition = new Vector2(540, 0);
                    PlayerViewSide.anchoredPosition = new Vector2(540, -920);
                    ProVideoFront.anchoredPosition = new Vector2(0, 0);
                    ProVideoSide.anchoredPosition = new Vector2(0, -920);
                    ReplayFront.anchoredPosition = new Vector2(0, 0);
                    ReplaySide.anchoredPosition = new Vector2(0, -920);

                    ReplayReadySide.anchoredPosition = new Vector2(0, 0);
                    ReplayProcessSide.anchoredPosition = new Vector2(0, 0);
                }
            }
            else
            {
                //좌우, 프로OFF
                PlayerViewFront.anchoredPosition = new Vector2(270, 0);
                PlayerViewSide.anchoredPosition = new Vector2(270, -920);
                ProVideoFront.anchoredPosition = new Vector2(0, -920);
                ProVideoSide.anchoredPosition = new Vector2(540, -920);
                ReplayFront.anchoredPosition = new Vector2(0, -920);
                ReplaySide.anchoredPosition = new Vector2(540, -920);

                ReplayReadySide.anchoredPosition = new Vector2(540, 0);
                ReplayProcessSide.anchoredPosition = new Vector2(540, 0);
            }
        }

        ProVideoFront.gameObject.SetActive(ProVideo);
        ProVideoSide.gameObject.SetActive(ProVideo);
        ReplayFront.gameObject.SetActive(Replay);
        ReplaySide.gameObject.SetActive(Replay);

        btnSwap.gameObject.SetActive((ProVideo || Replay) ? true : false);

        ReplayProcessUpdown.gameObject.SetActive(false);
        ReplayProcessSide.gameObject.SetActive(false);

        

        yield return new WaitForEndOfFrame();

        _layoutProcess = false;
    }
    /*
    public void OnClick_TestREC()
    {
        if (isRecording)
        {
            txtButtonREC.text = "REC";
            txtButtonREC.color = Color.black;
            StopRecording();
        }
        else
        {
            txtButtonREC.text = "STOP";
            txtButtonREC.color = Color.red;
            StartRecording();
        }
    }

    public void StartRecording()
    {

        isRecording = true;
        framesFront.Clear();
        framesSide.Clear();
        StartCoroutine(CaptureFrames());
    }

    public void StopRecording()
    {

        isRecording = false;
    }
    */



    //-----------------------------------------------------------------------
    // 영상 처리부
    //-----------------------------------------------------------------------
    //TODO:AICoaching에 사용 된 Director에 중복으로 사용되는 문제 개선필요
    IEnumerator CheckPose()
    {
        float timer = 0;
        SetReady();

        while (true)
        {
            yield return new WaitUntil(() => _isReplay == true);

            //어드레스 감지
            if(recStep.Equals(RECODESTEP.READY))
            {
                if (_handCheck && (_lastHandDir < 190f && _lastHandDir > 170))
                {
                    txtDebug.text = "어드레스 감지";
                    imgSignal.enabled = true;
                    imgSignal.color = Color.green;
                    timer += Time.deltaTime;
                    if (timer > 0.25f)
                    {
                        isRecording = true;
                        //framesFront.Clear();
                        recStep = RECODESTEP.RECORD;
                        StartCoroutine(CaptureFrames());
                        txtDebug.text = "녹화 중";
                        imgSignal.color = Color.red;
                        if (_isReplayUserInfo == true)
                            ReplayUserInfo.SetActive(false);
                    }
                }
                else
                {
                    timer = 0;
                    checkTakeback = false;
                    checkImpact = false;
                    checkTakebackFrame = 0;
                    txtDebug.text = "준비";
                    imgSignal.enabled = false;
                }
            }

            else if (recStep.Equals(RECODESTEP.RECORD))
            {
                //테이크백 감지
                if (checkTakeback == false && _handCheck && _lastHandDir < 150f)
                {
                    checkTakeback = true;
                }
                //임팩트 감지
                if (checkTakeback == true && checkImpact == false && _lastHandDir > 170f)
                {
                    checkImpact = true;
                }
            } 
            else if (recStep.Equals(RECODESTEP.RECORDEND))
            {
                if (_isUpdown)
                {
                    ReplayProcessUpdown.gameObject.SetActive(true);
                    BlurBackUpdown.SetActive(ReplayReadyUpdown.gameObject.activeInHierarchy);
                }
                else
                {
                    ReplayProcessSide.gameObject.SetActive(true);
                    BlurBackSide.SetActive(ReplayReadySide.gameObject.activeInHierarchy);
                }

                this.VideoPlayerControl.gameObject.SetActive(false);
                txtDebug.text = " 리플레이 준비 중";
                recStep = RECODESTEP.MAKEFILE;
                //VideoPlayerControl.ReleaseVIdeo();
                yield return null;
                currentIndex++;
                if (currentIndex > 5)
                {
                    qReplayInfos.Dequeue();
                    currentIndex = 0;
                }
                string frontPath = string.Format(outputPathFront, currentIndex);
                string sidePath = string.Format(outputPathSide, currentIndex);
                StartCoroutine(SendFramesToFFmpeg(frontPath, widthFront, heightFront, framesFront, () => _replayReadyFront = true));
                StartCoroutine(SendFramesToFFmpeg(sidePath, widthSide, heightSide, framesSide, () => _replayReadySide = true));

                //if (qReplayInfos.Count > 5)

                qReplayInfos.Enqueue(new ReplayInfo(currentIndex, frontPath, sidePath, framesFront[0]));
                yield return new WaitUntil(() => _replayReadyFront == true && _replayReadySide == true);
                recStep = RECODESTEP.REPLAY;

                yield return StartCoroutine(Replay());
            }

            //if(_isReplay == false)
            //    yield break;

            yield return null;
        }
    }

    IEnumerator Replay()
    {
        videoProFront.Stop();
        videoProSide.Stop();
        VideoPlayerControl.gameObject.SetActive(true);
        yield return null;
        //VideoPlayerControl.StopVideo();


        if (_isUpdown)
        {
            ReplayReadyUpdown.gameObject.SetActive(false);
            ReplayProcessUpdown.gameObject.SetActive(false);
        }
        else
        {
            ReplayReadySide.gameObject.SetActive(false);
            ReplayProcessSide.gameObject.SetActive(false);
        }

        

        if (_isReplay == false)
        {
            SetReady();
            yield break;
        }
        VideoPlayerControl.ReleaseVIdeo();
        //yield return null;
        //VideoPlayerControl.StopVideo();
        yield return new WaitForSeconds(0.1f);
        txtDebug.text = "리플레이 재생 " + currentIndex;

        //VideoPlayerControl.StopVideo();
        string frontPath = string.Format(outputPathFront, currentIndex);
        string sidePath = string.Format(outputPathSide, currentIndex);
        VideoPlayerControl.PlayVideo(frontPath, sidePath);
        //VideoPlayerControl.PlayVideo(outputPathFront, outputPathSide);

        yield return new WaitForSeconds((float)VideoPlayerControl.GetEndTime());
        if(_isReplayUserInfo == false)
        {
            _isReplayUserInfo = true;
            ReplayUserInfo.SetActive(true);
        }
        recStep = RECODESTEP.REPLAYEND;
        //txtDebug.text = "리플레이 종료";
        yield return new WaitForSeconds(0.5f);
        
        SetReady();
    }

    void SetReady()
    {
        recStep = RECODESTEP.READY;
        //txtDebug.text = "준비";
        checkTakeback = false;
        checkImpact = false;
        checkTakebackFrame = 0;

        framesFront.Clear();
        framesSide.Clear();

        Resources.UnloadUnusedAssets();

        _replayReadyFront = false;
        _replayReadySide = false;
    }

    
    bool CaptureFrameFront()//RenderTexture renderTex)
    {
        Texture sourceTexFront = rawImageFront.texture;
        widthFront = sourceTexFront.width;
        heightFront = sourceTexFront.height;
        RenderTexture renderTexFront = RenderTexture.GetTemporary(widthFront, heightFront, 0);
        Graphics.Blit(sourceTexFront, renderTexFront);

        RenderTexture.active = renderTexFront;
        Texture2D tex = new Texture2D(widthFront, heightFront, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, widthFront, heightFront), 0, 0);
        tex.Apply();
        
        if (checkTakeback == false)
        {
            checkTakebackFrame++;

            if (framesFront.Count > 90)
            {
                checkTakebackFrame = 0;
                framesFront.Clear();
                framesSide.Clear();//사이드도 같이 삭제
            }

            if (_handCheck == false)
            {
                RenderTexture.active = null;
                RenderTexture.ReleaseTemporary(renderTexFront);

                return false;
            }
        }
        
        framesFront.Add(tex);

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
        Texture2D tex = new Texture2D(widthSide, heightSide, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, widthSide, heightSide), 0, 0);
        tex.Apply();
                
        framesSide.Add(tex);

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(renderTexSide);
    }
    
    
    IEnumerator CaptureFrames()
    {
        //float interval = 1f / fps;
        int retry = 0;
        //txtDebug.text += "프레임 캡쳐 시작" + "\r\n";
        checkTakebackFrame = 0;
        int frameCount = 0;


        while (isRecording)
        {
            yield return new WaitForEndOfFrame();

            
            if (CaptureFrameFront())
            {
                CaptureFrameSide();
            }

            //yield return new WaitForSeconds(interval);

            if (checkImpact == true)
            {
                txtDebug.text = "녹화 중+";
                imgSignal.color = Color.yellow;
                if (frameCount < 45)
                    frameCount++;
                else
                    isRecording = false;
            }
            else
            {
                if (AvgVisible < 0.1f)
                {
                    if (retry > 5)
                    {
                        SetReady();
                        yield break;
                    }
                    else
                    {
                        retry++;
                        txtDebug.text = $"녹화 중 (감지불가-{retry})";
                    }
                }
                else
                {
                    txtDebug.text = $"녹화 중";
                    imgSignal.color = Color.red;
                }
            }

            if (_isReplay == false)
            {
                txtDebug.text = $"녹화 중지";
                SetReady();
                yield break;
            }
            
        }


        if (_isReplay)
        {
            imgSignal.color = Color.black;
            //앞 프레임 삭제
            framesFront.RemoveRange(0, Math.Max(0, checkTakebackFrame - 30));
            framesSide.RemoveRange(0, Math.Max(0, checkTakebackFrame - 30));

            txtDebug.text = $"녹화 종료({Math.Max(0, checkTakebackFrame - 30)})";
            recStep = RECODESTEP.RECORDEND;
            //txtDebug.text += "프레임 캡쳐 종료" + "\r\n";
            yield return null;
        }
        else
        {
            txtDebug.text = $"녹화 중지";
            SetReady();
        }
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

        for(int i = 0; i < frames.Count; i++)
        {
            byte[] raw = frames[i].GetRawTextureData();
            stdin.Write(raw, 0, raw.Length);

            if (i % 20 == 0)
                yield return null;
        }

        stdin.Flush();
        stdin.Close();

        bool isExited = false;
        _ = System.Threading.Tasks.Task.Run(() =>
        {
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

        string ffmpegPath = Path.Combine(Application.dataPath, "Plugins", "ffmpeg", "bin", "ffmpeg.exe");
        if(File.Exists(ffmpegPath) == false)
        {
            //txtDebug.text += "ffmpeg.exe 찾을 수 없음\n";
            return false;
        }

        try
        {
            process.StartInfo = new ProcessStartInfo
            {
                FileName = ffmpegPath,
                Arguments = $"-y -f rawvideo -pixel_format rgb24 -video_size {width}x{height} -framerate 30 -i - -c:v libx264 -pix_fmt yuv420p -profile:v baseline -movflags +faststart \"{output}\"",
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };

            //process.StartInfo.RedirectStandardError = true;
            //process.StartInfo.RedirectStandardOutput = true;

            process.OutputDataReceived += (s, e) => Debug.Log($"FFmpeg [Out]: {e.Data}");
            //process.ErrorDataReceived += (s, e) => Debug.LogError($"FFmpeg [Err]: {e.Data}");

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


    void OnProVideoEndFront(VideoPlayer vp)
    {
        proFrontEnd = true;
        videoProFront.Pause();
        PlayProVideo();
    }

    void OnProVideoEndSide(VideoPlayer vp)
    {
        proSideEnd = true;
        videoProSide.Pause();
        PlayProVideo();
    }
    

    void PlayProVideo()
    {
        //Debug.Log($"PlayProVideo() {proFrontEnd} / {proSideEnd}");
        if (proFrontEnd && proSideEnd)
        {
            proFrontEnd = false;
            proSideEnd = false;

            videoProFront.time = 0;
            videoProFront.Play();

            videoProSide.time = 0;
            videoProSide.Play();
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
            catch {
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
        if(leftHand.visibility < 0.5f || RightHand.visibility < 0.5f)
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
