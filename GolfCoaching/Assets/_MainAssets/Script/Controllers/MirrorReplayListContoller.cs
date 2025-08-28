using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using static MirrorDirector;

public class MirrorReplayListContoller : MonoBehaviour
{
    [Header("* Video Player")]
    [SerializeField] VideoPlayer videoProFront;
    [SerializeField] VideoPlayer videoProSide;
    [SerializeField] VideoPlayerControlMirror VideoPlayerControl;

    [Header("* Panel")]
    [SerializeField] GameObject PanelList;
    [SerializeField] GameObject PanelViewer;
    [SerializeField] GameObject SideOptionButton;

    [Header("* Replay Card")]
    [SerializeField] ReplayCard[] ReplayCards;

    List<MirrorDirector.ReplayInfo> _replayInfo;

    public void SetReplays(List<MirrorDirector.ReplayInfo> replayInfo)
    {
        _replayInfo = replayInfo;
        PanelList.SetActive(true);
        SideOptionButton.SetActive(false);

        SetCards();
    }

    void SetCards()
    {
        for (int i = 0; i < ReplayCards.Length; i++)
        {
            ReplayCards[i].gameObject.SetActive(false);
        }

        if (_replayInfo == null || _replayInfo.Count == 0)
            return;

        for (int i = 0; i < _replayInfo.Count; i++)
        {
            TimeSpan gapTime = DateTime.Now - _replayInfo[i].recordTime;

            string time = (gapTime.Minutes > 0) ? $"{gapTime.Minutes}분 전" : $"{gapTime.Seconds}초 전";
            ReplayCards[i].SetReplayCard(_replayInfo[i].thumbnail, time, _replayInfo[i].frontPath, _replayInfo[i].sidePath,
                (front, side) =>
                {
                    PanelViewer.SetActive(true);
                    VideoPlayerControl.PlayVideo(front, side);
                });
            ReplayCards[i].gameObject.SetActive(true);
        }
    }


    public void OnClick_Close()
    {
        VideoPlayerControl.OnClick_PlayStop();
        videoProFront.url = null;
        videoProSide.url = null;

        PanelList.SetActive(false);
        PanelViewer.SetActive(false);
        SideOptionButton.SetActive(true);
    }
    public void OnClick_Back()
    {
        PanelViewer.SetActive(false);

        SetCards();

        VideoPlayerControl.OnClick_PlayStop();
        videoProFront.url = null;
        videoProSide.url = null;
    }
}
