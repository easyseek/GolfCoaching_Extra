using Enums;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoCardController : MonoBehaviour
{
    [SerializeField] VideoPlayer videoPlayer;
    
    [SerializeField] RawImage rawImage;

    [SerializeField] Image backgroundImage;

    [SerializeField] Texture2D defaultImage;

    //[SerializeField] RenderTexture[] targetRTs;
    private RenderTexture myRenderTexture;

    [SerializeField] TextMeshProUGUI m_ClubAndPoseText = null;
    [SerializeField] TextMeshProUGUI m_VideoNameText = null;
    [SerializeField] TextMeshProUGUI m_ViewsText = null;
    [SerializeField] TextMeshProUGUI m_VideoTimeText = null;

    [Header("[ ProData ]")]
    [SerializeField] Image m_ProProfile = null;
    [SerializeField] TextMeshProUGUI m_ProNameText = null;
    [SerializeField] TextMeshProUGUI m_ProOrganizationText = null;

    private ProVideoData m_VideoData;
    private ProInfoData m_ProData;

    private EVideoSourceType m_VideoSourceType = EVideoSourceType.None;

    private string videoURL;

    Action<ProVideoData> actVideoData;
    Action<ProVideoData> actFinishVideoData;

    public void SetProDetailVideoCard(ProVideoData data, int cardNum, Action<ProVideoData> act)
    {
        if (data == null)
            return;

        if (myRenderTexture == null)
        {
            myRenderTexture = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
            myRenderTexture.Create();
        }

        videoPlayer.targetTexture = myRenderTexture;
        //videoPlayer.targetTexture = targetRTs[cardNum];

        rawImage.texture = myRenderTexture;

        m_VideoData = data;
        actVideoData = act;

        videoURL = GameManager.Instance.LoadVideoURL($"{INI.proVideoPath}{data.uid}/{data.path}");
        videoPlayer.url = videoURL;

        videoPlayer.prepareCompleted += OnVideoPrepared;

        if(videoPlayer.gameObject.activeInHierarchy)
            videoPlayer.Prepare();
    }

    public void SetVideoCard(ProVideoData data, int cardNum, Action<ProVideoData> act)
    {
        if (data == null)
            return;

        //videoPlayer.targetTexture = targetRTs[cardNum];
        if (myRenderTexture == null)
        {
            myRenderTexture = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
            myRenderTexture.Create();
        }

        videoPlayer.targetTexture = myRenderTexture;

        rawImage.texture = myRenderTexture;
        //rawImage.texture = defaultImage;

        m_VideoData = data;
        m_ProData = GolfProDataManager.Instance.GetProInfoData(data.uid);

        actVideoData = act;

        m_ClubAndPoseText.text = $"{Utillity.Instance.ConvertEnumToString(data.clubFilter)} • {Utillity.Instance.ConvertEnumToString(data.poseFilter)}";

        m_VideoNameText.text = $"{data.name}";

        m_ViewsText.text = $"조회수 {Utillity.Instance.FormatViewsCount(data.views)}회";

        videoURL = GameManager.Instance.LoadVideoURL($"{INI.proVideoPath}{data.uid}/{data.path}");
        videoPlayer.url = videoURL;

        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.Prepare();
    }

    public void SetFinishVideoCard(ProVideoData data, int cardNum, Action<ProVideoData> act, EVideoSourceType sourceType = EVideoSourceType.None)
    {
        if (data == null)
            return;

        if (myRenderTexture == null)
        {
            myRenderTexture = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
            myRenderTexture.Create();
        }

        //videoPlayer.targetTexture = targetRTs[cardNum];
        videoPlayer.targetTexture = myRenderTexture;

        if (!object.ReferenceEquals(myRenderTexture, null))
            rawImage.texture = myRenderTexture;

        m_VideoSourceType = sourceType;
        m_VideoData = data;
        m_ProData = GolfProDataManager.Instance.GetProInfoData(data.uid);

        actFinishVideoData = act;

        if(!object.ReferenceEquals(backgroundImage, null))
        {
            backgroundImage.color = (sourceType == EVideoSourceType.Best && cardNum == 0) ? Utillity.Instance.HexToRGB(INI.Green700) : Color.white;
        }

        if (!object.ReferenceEquals(m_VideoNameText, null))
        {
            m_VideoNameText.text = (sourceType == EVideoSourceType.Best && cardNum == 0) ? $"<color=white>{data.name}</color>" : $"<color=black>{data.name}</color>";
        }
            
        if (!object.ReferenceEquals(m_ViewsText, null))
        {
            m_ViewsText.text = (sourceType == EVideoSourceType.Best && cardNum == 0) ? $"<color=white>조회수 {Utillity.Instance.FormatViewsCount(data.views)}회</color>" : $"<color=black>조회수 {Utillity.Instance.FormatViewsCount(data.views)}회</color>";
        }
            
        if (!object.ReferenceEquals(m_ProProfile, null))
        {
            StartCoroutine(GameManager.Instance.LoadImageCoroutine($"{INI.proImagePath}{data.uid}/{GolfProDataManager.Instance.GetProImageData(data.uid, Enums.EImageType.Thumbnail).path}", (Sprite sprite) => {
                if (sprite != null)
                {
                    m_ProProfile.sprite = sprite;
                }
                else
                    Debug.Log("썸네일 Sprite 로드 실패");
            }));
        }

        if (!object.ReferenceEquals(m_ProNameText, null))
        {
            m_ProNameText.text = (sourceType == EVideoSourceType.Best && cardNum == 0) ? $"<color=white>{m_ProData.name}</color>" : $"<color=black>{m_ProData.name}</color>";
        }

        if (!object.ReferenceEquals(m_ProOrganizationText, null))
        {
            m_ProOrganizationText.text = (sourceType == EVideoSourceType.Best && cardNum == 0) ? $"<color=white>{m_ProData.info}</color>" : $"<color=black>{m_ProData.info}</color>";
        }

        videoURL = GameManager.Instance.LoadVideoURL($"{INI.proVideoPath}{data.uid}/{data.path}");
        videoPlayer.url = videoURL;

        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.Prepare();
    }

    public void OnClick_VideoSelect()
    {
        if (actVideoData != null)
            actVideoData.Invoke(m_VideoData);
    }

    public void OnClick_FinishVideoSelect()
    {
        if (actFinishVideoData != null)
            actFinishVideoData.Invoke(m_VideoData);
    }

    private IEnumerator PlayVideo()
    {
        if(videoPlayer == null)
        {
            Debug.Log($"VideoPlayer is null");
            yield break;
        }

        //videoPlayer.Prepare();
        //while(!videoPlayer.isPrepared)
        //{
        //    yield return null;
        //}
        while (!videoPlayer.gameObject.activeInHierarchy)
        {
            yield return null;
        }

        while (true)
        {
            videoPlayer.time = 0;
            videoPlayer.Play();

            yield return new WaitForSeconds(3.0f);
            videoPlayer.Pause();

            //videoPlayer.isLooping = true;
            //yield break;
        }
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        rawImage.texture = vp.targetTexture;
        vp.Pause();

        if(m_VideoTimeText != null)
            m_VideoTimeText.text = Utillity.Instance.GetVideoTime(vp);
    }

    private void OnDestroy()
    {
        if (myRenderTexture != null)
        {
            myRenderTexture.Release();
            myRenderTexture = null;
        }
    }
}
