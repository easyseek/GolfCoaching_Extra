using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class VideoPlayerControlMirror : MonoBehaviour
{
    [SerializeField] VideoPlayer videoPlayerFront;
    [SerializeField] VideoPlayer videoPlayerSide;
    [SerializeField] TextMeshProUGUI txtSpeedNor;
    [SerializeField] TextMeshProUGUI txtSpeed2X;
    [SerializeField] TextMeshProUGUI txtSpeed4X;
    [SerializeField] Image imgButtonPlay;
    [SerializeField] Sprite spPlay;
    [SerializeField] Sprite spPause;
    [SerializeField] Slider SldVideoContorol;
    [SerializeField] Color activeColor = Color.white;

    bool _isPrepareFront = false;
    bool _isPrepareSide = false;
    bool _isVideoPrepared = false;
    bool _isPlay = false;
    bool _isEnd = false;
    int _speed = 0;

    float[] playSpeed = { 1f, 0.5f, 0.25f };

    private void Start()
    {
        SldVideoContorol.onValueChanged.AddListener(OnValueChanged_SeekVideo);
        videoPlayerFront.prepareCompleted += OnVideoPreparedFront;
        videoPlayerFront.loopPointReached += OnVideoEnd; // 비디오 끝 이벤트 추가

        OnClick_SetSpeed(1);
    }

    public void PlayVideo(string urlFront = null, string urlSide = null)
    {
        // 비디오가 준비된 후 자동 재생
        if (!string.IsNullOrEmpty(urlFront))
            videoPlayerFront.url = urlFront;

        if (!string.IsNullOrEmpty(urlSide))
            videoPlayerSide.url = urlSide;

        if (_isPrepareFront == false)
        {            
            videoPlayerFront.Prepare();
            _isPrepareFront = true;;
        }

        if (_isPrepareSide == false)
        {
            videoPlayerSide.Prepare();
            _isPrepareSide = true; ;
        }

        OnClick_SetSpeed(_speed);
        StartCoroutine(CoUpdate());

        videoPlayerFront.Play();
        videoPlayerSide.Play();
        _isPlay = true;
        _isEnd = false;
        imgButtonPlay.sprite = spPause;
    }

    void OnVideoPreparedFront(VideoPlayer vp)
    {
        _isVideoPrepared = true;
    }

    void OnVideoPreparedSide(VideoPlayer vp)
    {
        
    }

    public double GetEndTime()
    {
        return videoPlayerFront.length;
    }

    public bool GetPrepared()
    {
        return _isVideoPrepared;
    }

    public bool isPlay()
    {
        return _isPlay;
    }

    public void StopVideo()
    {
        _isPlay = false;
        imgButtonPlay.sprite = spPlay;
        videoPlayerFront.Pause();
        videoPlayerSide.Pause();
        StopAllCoroutines();
    }

    public void ReleaseVIdeo()
    {
        videoPlayerFront.Stop();
        videoPlayerSide.Stop();
        videoPlayerFront.clip = null;
        videoPlayerFront.url = null;
        videoPlayerSide.clip = null;
        videoPlayerSide.url = null;
    }

    IEnumerator CoUpdate()
    {
        while (true)
        {
            if (_isVideoPrepared)
            {
                UpdateTimeSlider();
            }
            yield return null;
        }
    }

    // 비디오가 끝났을 때 호출되는 메서드
    void OnVideoEnd(VideoPlayer vp)
    {
        //Debug.Log("Video has ended.");
        //_isEnd = true;
        //_isPlay = false;
        //imgButtonPlay.sprite = spPlay;
        videoPlayerFront.Pause();
        videoPlayerSide.Pause();

        videoPlayerFront.time = 0;
        videoPlayerFront.Play();

        videoPlayerSide.time = 0;
        videoPlayerSide.Play();
    }

    public void OnClick_SetSpeed(int SpdType)
    {
        videoPlayerFront.playbackSpeed = playSpeed[SpdType];
        videoPlayerSide.playbackSpeed = playSpeed[SpdType];

        txtSpeedNor.color = SpdType == 0 ? activeColor : Color.white;
        txtSpeed2X.color = SpdType == 1 ? activeColor : Color.white;
        txtSpeed4X.color = SpdType == 2 ? activeColor : Color.white;

        _speed = SpdType;
    }

    public void OnClick_PlayStop()
    {
        if (_isPlay == false)
        {
            if(_isEnd)
            {
                videoPlayerFront.Stop();
                videoPlayerSide.Stop();
                _isEnd = false;
            }

            imgButtonPlay.sprite = spPause;
            _isPlay = true;
            videoPlayerFront.Play();
            videoPlayerSide.Play();
        }
        else
        {
            _isPlay = false;
            imgButtonPlay.sprite = spPlay;
            videoPlayerFront.Pause();
            videoPlayerSide.Pause();
        }
    }

    public void OnValueChanged_SeekVideo(float sliderValue)
    {
        if (_isVideoPrepared)
        {
            double newTime = sliderValue * videoPlayerFront.length;
            videoPlayerFront.time = newTime; // 비디오 타임라인 이동
            videoPlayerSide.time = newTime; // 비디오 타임라인 이동
        }
    }


    // 타임라인 슬라이더 업데이트
    void UpdateTimeSlider()
    {
        if (_isVideoPrepared && videoPlayerFront.length > 0)
        {
            SldVideoContorol.SetValueWithoutNotify((float)(videoPlayerFront.time / videoPlayerFront.length)); // 슬라이더 값 업데이트
        }
    }
}
