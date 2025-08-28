using DG.Tweening;
using Enums;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoPlayerControl : MonoBehaviour
{
    [Header("* UI Components")]
    [SerializeField] FocusCoachingDirecter m_FocusCoachingdirector;
    [SerializeField] VideoFInishPanel m_VideoFinishPanel;

    [SerializeField] RectTransform TopPanel;
    [SerializeField] VideoPlayer videoPlayer;
    [SerializeField] TextMeshProUGUI txtLessonTitle;
    [SerializeField] TextMeshProUGUI txtProName;
    [SerializeField] TextMeshProUGUI txtPstTime;
    [SerializeField] TextMeshProUGUI txtRmnTime;
    [SerializeField] TextMeshProUGUI txtSpeedNor;

    [SerializeField] TextMeshProUGUI txtSpeed2X;
    [SerializeField] TextMeshProUGUI txtSpeed4X;
    [SerializeField] TextMeshProUGUI txtRepeat;
    [SerializeField] Image imgPause;
    [SerializeField] Image imgPlay;
    [SerializeField] Slider SldVideoContorol;

    [SerializeField] GameObject TimeSliderPanel;
    [SerializeField] GameObject RepeatSliderPanel;
    [SerializeField] GameObject ViewrWebCam;

    [SerializeField] Toggle tglRepeat;

    [SerializeField] Slider SldRepeatStart;
    [SerializeField] Slider SldRepeatEnd;
    [SerializeField] TextMeshProUGUI txtRepeatInfo;
    [SerializeField] TextMeshProUGUI txtRepeatStart;
    [SerializeField] TextMeshProUGUI txtRepeatEnd;
    [SerializeField] RectTransform imgRepeatRange;

    [SerializeField] Color activeColor = Color.white;

    private ProVideoData proVideoData;

    bool _isPlay = false;
    bool _isRepeat = false;
    float[] playSpeed = {1f, 0.5f, 0.25f };
    bool _isVideoPrepared = false;
    bool _loopVideo = false;

    float _repeatStartValue = 0;
    float _repeatEndValue = 1f;
    double _repeatStartTime = 0;
    double _repeatEndTime = 1f;

    bool _isPrepare = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tglRepeat.onValueChanged.AddListener(OnValueChanged_Repeat);
        //PlayVideo();
    }

    public void PlayVideo(string url = null, ProVideoData data = null)
    {
        if (!object.ReferenceEquals(data, null))
            proVideoData = data;

        // 비디오가 준비된 후 자동 재생
        if (!string.IsNullOrEmpty(url))
            videoPlayer.url = url;

        if (_isPrepare == false)
        {
            videoPlayer.isLooping = _loopVideo;
            videoPlayer.prepareCompleted += OnVideoPrepared;
            videoPlayer.loopPointReached += OnVideoEnd; // 비디오 끝 이벤트 추가
            videoPlayer.Prepare();
            _isPrepare = true;

            SldVideoContorol.onValueChanged.AddListener(OnValueChanged_SeekVideo);
            SldRepeatStart.onValueChanged.AddListener(OnValueChanged_RepeatStart);
            SldRepeatEnd.onValueChanged.AddListener(OnValueChanged_RepeatEnd);
        }

        OnClick_SetSpeed(0);
        
        txtRepeatInfo.gameObject.SetActive(false);
        txtRepeatStart.gameObject.SetActive(false);
        txtRepeatEnd.gameObject.SetActive(false);

        m_FocusCoachingdirector.LessonState = ELessonState.Play;

        StartCoroutine(CoUpdate());
    }

    public void StopVideo()
    {
        videoPlayer.Stop();
    }

    public void SetTitle(string strTitle)
    {
        txtLessonTitle.text = strTitle;
    }

    public void SetProName(string strProName)
    {
        txtProName.text = strProName;
    }

    IEnumerator CoUpdate()
    {
        while (true)
        {
            if (_isVideoPrepared)
            {
                if (_isRepeat && _isPlay)
                {
                    if (videoPlayer.time < _repeatStartTime || videoPlayer.time > _repeatEndTime)
                    {
                        videoPlayer.Pause();
                        yield return null;

                        videoPlayer.time = _repeatStartTime + 0.001d;
                        yield return null;

                        videoPlayer.Play();
                    }
                }

                UpdateTimeUI();
                UpdateTimeSlider();
            }
            yield return null;
        }
    }

    // 비디오가 준비된 후 호출
    void OnVideoPrepared(VideoPlayer vp)
    {
        _isVideoPrepared = true;
        videoPlayer.Play();
        _isPlay = true;

        // 총 재생 시간 설정
        txtRmnTime.text = FormatTime((float)videoPlayer.length);
        _repeatEndTime = videoPlayer.length;
    }

    // 비디오가 끝났을 때 호출되는 메서드
    void OnVideoEnd(VideoPlayer vp)
    {
        if (_loopVideo)
        {
            videoPlayer.Stop();
            videoPlayer.Play(); // 루프 재생
        }
        else
        {
            // 비디오가 끝난 후 처리 (루프가 아닐 때)
            Debug.Log("Video has ended.");
            m_FocusCoachingdirector.LessonState = ELessonState.End;
            m_VideoFinishPanel.gameObject.SetActive(true);
            StartCoroutine(m_VideoFinishPanel.SetData(proVideoData));
        }
    }

    public void OnClick_SetSpeed(int SpdType)
    {
        videoPlayer.playbackSpeed = playSpeed[SpdType];
        txtSpeedNor.color = SpdType == 0 ? activeColor : Color.white;
        txtSpeed2X.color = SpdType == 1 ? activeColor : Color.white;
        txtSpeed4X.color = SpdType == 2 ? activeColor : Color.white;
    }

    public void OnClick_PlayStop()
    {
        //_isPlay = !_isPlay;

        if (_isPlay == false)
        {
            if(_isRepeat && txtRepeatInfo.gameObject.activeInHierarchy == true)
            {
                SetRepeat(false);
            }

            if(TopPanel.transform.localPosition.y == 1380.0f)
            {
                _isPlay = true;
                imgPlay.enabled = false;
                videoPlayer.Play();
            }
            else
            {
                imgPause.enabled = true;
                imgPlay.enabled = false;
                TopPanel.DOLocalMoveY(1380f, 0.3f).From(960f).OnComplete(
                () => {
                    _isPlay = true;
                    imgPause.enabled = false;
                    /*if (_isRepeat)
                    {
                        if (videoPlayer.time < _repeatStartTime || videoPlayer.time > _repeatEndTime)
                        {
                            SldVideoContorol.value = _repeatStartValue;
                        }
                    }*/
                    videoPlayer.Play();
                });
            }
        }
        else
        {
            _isPlay = false;
            imgPlay.enabled = true;
            TopPanel.DOLocalMoveY(960, 0.3f).From(1380f);
            videoPlayer.Pause();
        }
    }

    public void OnClick_TopPanel()
    {
        TopPanel.DOLocalMoveY(1380f, 0.3f).From(960f);
    }


    public void OnClick_Repeat()
    {
        if(_isRepeat == false)
        {
            SetRepeat(true);
        }
        else
        {
            SetRepeat(false);
        }
    }

    void SetRepeat(bool isRepeat)
    {
        _isRepeat = isRepeat;

        if (_isRepeat)
        {
            txtRepeat.color = activeColor;
            TimeSliderPanel.SetActive(false);
            RepeatSliderPanel.SetActive(true);

            txtRepeatInfo.gameObject.SetActive(true);
        }
        else
        {
            txtRepeat.color = Color.white;
            TimeSliderPanel.SetActive(true);
            RepeatSliderPanel.SetActive(false);

            txtRepeatInfo.gameObject.SetActive(false);
            txtRepeatStart.gameObject.SetActive(false);
            txtRepeatEnd.gameObject.SetActive(false);

            _repeatStartValue = 0;
            _repeatEndValue = 1f;
            _repeatStartTime = 0;
            _repeatEndTime = videoPlayer.length;
            SldRepeatStart.SetValueWithoutNotify(_repeatStartValue);
            SldRepeatEnd.SetValueWithoutNotify(_repeatEndValue);
            SetRepeatRangeFill();
        }
    }

    public void OnValueChanged_SeekVideo(float sliderValue)
    {
        if (_isVideoPrepared)
        {
            double newTime = sliderValue * videoPlayer.length;
            videoPlayer.time = newTime; // 비디오 타임라인 이동
        }
    }

    public void OnValueChanged_Repeat(bool isOn)
    {
        _loopVideo = isOn;
        //videoPlayer.isLooping = isOn;
    }

    // 현재 시간 UI 업데이트
    void UpdateTimeUI()
    {
        txtPstTime.text = FormatTime((float)videoPlayer.time);        
    }

    // 타임라인 슬라이더 업데이트
    void UpdateTimeSlider()
    {
        if (_isVideoPrepared && videoPlayer.length > 0)
        {
            SldVideoContorol.SetValueWithoutNotify((float)(videoPlayer.time / videoPlayer.length)); // 슬라이더 값 업데이트
        }
    }


    
    public void OnValueChanged_RepeatStart(float sliderValue)
    {
        if (_isVideoPrepared)
        {
            if(txtRepeatInfo.gameObject.activeInHierarchy == true)
            {
                txtRepeatInfo.gameObject.SetActive(false);
                txtRepeatStart.gameObject.SetActive(true);
                txtRepeatEnd.gameObject.SetActive(true);
            }

            if (sliderValue > _repeatEndValue)
            {
                sliderValue = _repeatEndValue - 0.05f;
                SldRepeatStart.SetValueWithoutNotify(sliderValue);
            }
            _repeatStartTime = sliderValue * videoPlayer.length;
            _repeatStartValue = sliderValue;
            txtRepeatStart.text = FormatTime((float)_repeatStartTime);
            SetRepeatRangeFill();
        }        
    }

    public void OnValueChanged_RepeatEnd(float sliderValue)
    {
        if (_isVideoPrepared)
        {
            if (txtRepeatInfo.gameObject.activeInHierarchy == true)
            {
                txtRepeatInfo.gameObject.SetActive(false);
                txtRepeatStart.gameObject.SetActive(true);
                txtRepeatEnd.gameObject.SetActive(true);
            }

            if (_repeatStartValue > sliderValue)
            {
                sliderValue = _repeatStartValue + 0.05f;
                SldRepeatEnd.SetValueWithoutNotify(sliderValue);
            }
            _repeatEndTime = sliderValue * videoPlayer.length;
            _repeatEndValue = sliderValue;
            txtRepeatEnd.text = FormatTime((float)_repeatEndTime);
            SetRepeatRangeFill();
        }
    }

    void SetRepeatRangeFill()
    {
        imgRepeatRange.sizeDelta = new Vector2((_repeatEndValue - _repeatStartValue) * 800f, imgRepeatRange.sizeDelta.y);
        imgRepeatRange.anchoredPosition = new Vector2((_repeatStartValue * 400f) - (1-_repeatEndValue) * 400f, imgRepeatRange.anchoredPosition.y);
    }

    public void OnClick_CameraOff()
    {
        ViewrWebCam.SetActive(!ViewrWebCam.activeInHierarchy);
    }

    // 시간 포맷팅 (mm:ss)
    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

}
