using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class LessonModeDirector : MonoBehaviour
{
    [SerializeField] VideoPlayerControl videoPlayer;

    private void Start()
    {
        Init();
    }

    private void Init()
    {
        GameManager.Instance.SetOptionPanel();

        //videoPlayer.SetTitle($"�������� / {GameManager.Instance.GetClubName()}����{GameManager.Instance.GetPoseName()}");
        videoPlayer.SetProName("�ֱ��� ����");

        videoPlayer.PlayVideo();


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
}
