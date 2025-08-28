using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Burst;
using System.Collections;
using DG.Tweening;
using System.Threading;
using System.IO.Pipes;
using System.IO;
using System;
using System.Diagnostics;
using UnityEngine.DedicatedServer;
using Debug = UnityEngine.Debug;
using TMPro;
using System.Diagnostics.Tracing;
//using Michsky.LSS;

//[ShowOdinSerializedPropertiesInInspector]
[BurstCompile]


public class LoginDirector : MonoBehaviour//SerializedMonoBehaviour, ISerializationCallbackReceiver
{
    [SerializeField]  webcamclient wcclient;
    public TutorialController m_TutorialController;
    public CanvasGroup m_SpoexModePanel;

    // -----------------------------------------------------------
    public CanvasGroup m_LoginPanel;
    public CanvasGroup m_IntroPanel;

    private Thread pipe1ClientThread;
    private Thread pipe2ClientThread;

    public string PIPE1_NAME = "skeleton_pipe1";
    public string PIPE2_NAME = "skeleton_pipe2";

    public float m_FadeDuration = 1.0f;

    private bool isRunning = false;
    private bool isConnected = false;

    [SerializeField] TextMeshProUGUI txtVer;

    class appinfo
    {
        public string version;
    }

    private void Start()
    {
        GameManager.Instance.IsTutorial = false;

        if (m_LoginPanel != null)
        m_LoginPanel.alpha = 1f;
        m_IntroPanel.alpha = 0f;

        GetVersion();
        //StartPipeClient();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            StopPipeClient();
            Application.Quit();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            OnClick_TouchLoginTemp();
        }
    }

    #region Skeleton Pipe
    private void StartPipeClient()
    {
        isRunning = true;
        pipe1ClientThread = new Thread(Pipe1ClientThread);
        pipe1ClientThread.Start();

        pipe2ClientThread = new Thread(Pipe2ClientThread);
        pipe2ClientThread.Start();
    }

    private void Pipe1ClientThread()
    {
        while (isRunning)
        {
            try
            {
                Debug.Log("서버 연결을 대기 중입니다...");
                using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", PIPE1_NAME, PipeDirection.In))
                {
                    pipeClient.Connect(5000); // 5초 타임아웃
                    isConnected = true;
                    Debug.Log("서버와 연결되었습니다.");

                    using (StreamReader sr = new StreamReader(pipeClient))
                    {
                        while (isRunning)
                        {
                            if (pipeClient.IsConnected)
                            {
                                string message = sr.ReadLine();
                            }
                            else if(!pipeClient.IsConnected)
                            {
                                Debug.LogWarning("파이프 연결이 끊어졌습니다. 재연결을 시도합니다.");
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"파이프 클라이언트 오류: {e.Message}\n스택 트레이스: {e.StackTrace}");
                if (isConnected)
                {
                    Debug.Log("서버와의 연결이 끊어졌습니다.");
                    isConnected = false;
                }
                Thread.Sleep(1000); // 재연결 전 1초 대기
            }
        }
    }

    private void Pipe2ClientThread()
    {
        while (isRunning)
        {
            try
            {
                Debug.Log("서버 연결을 대기 중입니다...");
                using (NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", PIPE2_NAME, PipeDirection.In))
                {
                    pipeClient.Connect(5000); // 5초 타임아웃
                    isConnected = true;
                    Debug.Log("서버와 연결되었습니다.");

                    using (StreamReader sr = new StreamReader(pipeClient))
                    {
                        while (isRunning)
                        {
                            if (pipeClient.IsConnected)
                            {
                                string message = sr.ReadLine();
                            }
                            else if(!pipeClient.IsConnected)
                            {
                                Debug.LogWarning("파이프 연결이 끊어졌습니다. 재연결을 시도합니다.");
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"파이프 클라이언트 오류: {e.Message}\n스택 트레이스: {e.StackTrace}");
                if (isConnected)
                {
                    Debug.Log("서버와의 연결이 끊어졌습니다.");
                    isConnected = false;
                }
                Thread.Sleep(1000); // 재연결 전 1초 대기
            }
        }
    }

    public void StopPipeClient()
    {
        wcclient.StopPipeClient();

        isRunning = false;
        if (pipe1ClientThread != null)
        {
            pipe1ClientThread.Abort();
        }

        if (pipe2ClientThread != null)
        {
            pipe2ClientThread.Abort();
        }
    }
    #endregion

    void GetVersion()
    {
        try
        {
            string path = Application.dataPath + @"\..\appinfo.json";
            if (File.Exists(path))
            {
                string data = File.ReadAllText(path);
                txtVer.text = JsonUtility.FromJson<appinfo>(data).version;
            }
            else
                txtVer.text = "0.1.0";
        }
        catch
        {
            txtVer.text = "0.1.0";
        }
    }

    public void LoginSuccess()
    {
        StartCoroutine(TranstionToIntro());
    }

    private IEnumerator TranstionToIntro()
    {
        yield return m_LoginPanel.DOFade(0, m_FadeDuration).WaitForCompletion();

        yield return m_IntroPanel.DOFade(1, m_FadeDuration).WaitForCompletion();

        yield return new WaitForSeconds(1.5f);

        yield return m_IntroPanel.DOFade(0, m_FadeDuration).WaitForCompletion();

        StartCoroutine(LoginControl());
    }

    private IEnumerator LoginControl()
    {
        yield return new WaitForSeconds(1.5f);

        GameManager.Instance.SelectedSceneName = "Login";
        SceneManager.LoadScene("ProSelect");
    }

    public void OnClick_TouchLoginTemp()
    {
        LoginSuccess();
    }

    public void OnClick_Jump()
    {
        GameManager.Instance.SelectedSceneName = "PracticeMode";
        SceneManager.LoadScene("PracticeMode");
    }

    public void OnClick_Mode(int idx)
    {
        if(idx == 0)
        {
            GameManager.Instance.IsTutorial = false;

            m_LoginPanel.interactable = true;
            m_SpoexModePanel.interactable = false;
        }
        else
        {
            GameManager.Instance.IsTutorial = true;

            m_TutorialController.StartTutorial();
        }

        m_SpoexModePanel.DOFade(0, 0.5f).SetEase(Ease.OutCubic);
    }

    private void OnDestroy()
    {
        StopPipeClient();
    }

    public void OnClick_Shutdown()
    {
        StopPipeClient();

        ProcessStartInfo psi = new ProcessStartInfo();
        psi.FileName = "shutdown";
        psi.Arguments = "-s -t 5";
        psi.CreateNoWindow = true;
        psi.UseShellExecute = false;

        try
        {
            Process.Start(psi);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Shutdown Failed " + e.Message);
        }
        finally
        {
            Application.Quit();
        }
    }

    public void OnClick_Quit()
    {
        StopPipeClient();
        Application.Quit();
    }
}
