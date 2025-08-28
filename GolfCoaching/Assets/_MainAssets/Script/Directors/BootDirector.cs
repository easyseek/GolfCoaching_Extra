using UnityEngine;
using System;
using System.Threading;

public class BootDirector : MonoBehaviour
{
#if UNITY_STANDALONE_WIN
    private static Mutex mutex;
#endif

    private int width = 1080;
    private int height = 1920;

    private void Awake()
    {
#if UNITY_STANDALONE_WIN
        bool createdNew;
        mutex = new Mutex(true, "GolfCoaching24", out createdNew);

        if(!createdNew)
        {
            Application.Quit();
            return;
        }

        DontDestroyOnLoad(gameObject);
#endif
    }

    void Start()
    {
        Init();
    }

    private void Init()
    {
        if(Utillity.Instance.CheckInternet())
        {
            Debug.Log($"인터넷 접속 O");
            //Debug.Log($"{DateTime.UtcNow.ToString("O")}");
        }
        else
        {
            Debug.Log($"인터넷 접속 X");
        }

        Application.targetFrameRate = 30;
        QualitySettings.vSyncCount = 0;

        Utillity.Instance.SetResolution(width, height);

        GolfProDataManager.Instance.LoadProData();
    }

    private void OnApplicationQuit()
    {
#if UNITY_STANDALONE_WIN
        try
        {
            mutex?.ReleaseMutex();
        }
        catch (ApplicationException)
        {
            
        }
        finally
        {
            mutex = null;
        }
#endif
    }
}
