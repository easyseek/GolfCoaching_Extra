using UnityEngine;
using TMPro;

public class KeyboardService : MonoBehaviourSingleton<KeyboardService>
{
    //private static KeyboardService _instance;
    //public static KeyboardService Instance {
    //    get {
    //        if (_instance == null)
    //        {
    //            var go = new GameObject("[KeyboardService]");
    //            _instance = go.AddComponent<KeyboardService>();
    //            DontDestroyOnLoad(go);
    //        }
    //        return _instance;
    //    }
    //}

    [SerializeField] private GameObject keyboardCanvasPrefab;

    private GameObject keyboardCanvasInstance;
    private VirtualKeyboard virtualKeyboard;
    private KeyboardManager keyboardManager;

    /// <summary>
    /// TMP_InputField에 대해 키보드를 표시
    /// </summary>
    public void Show(TMP_InputField targetField)
    {
        if (targetField == null)
        {
            Debug.LogWarning("[KeyboardService] targetField is null.");
            return;
        }

        EnsureCanvas();

        if (keyboardCanvasInstance != null)
            keyboardCanvasInstance.SetActive(true);

        if (keyboardManager != null)
        {
            keyboardManager.AttachTarget(targetField);
            keyboardManager.ActivateKeyboard();
        }
        else
        {
            Debug.LogError("[KeyboardService] KeyboardManagerTMP not found on prefab.");
        }
    }

    /// <summary>
    /// 키보드 숨김
    /// </summary>
    public void Hide()
    {
        if (keyboardManager != null)
            keyboardManager.DeactivateKeyboard();

        if (keyboardCanvasInstance != null)
            keyboardCanvasInstance.SetActive(false);
    }

    private void EnsureCanvas()
    {
        if (keyboardCanvasInstance != null) return;

        keyboardCanvasInstance = keyboardCanvasPrefab;

        //if (prefab == null)
        //    prefab = Resources.Load<GameObject>("VirtualKeyboardCanvas");

        //if (prefab == null)
        //{
        //    Debug.LogError("[KeyboardService] Resources/VirtualKeyboardCanvas 프리팹을 찾을 수 없습니다.");
        //    return;
        //}

        //keyboardCanvasInstance = Instantiate(prefab);
        //DontDestroyOnLoad(keyboardCanvasInstance);

        virtualKeyboard = keyboardCanvasInstance.GetComponentInChildren<VirtualKeyboard>(true);
        keyboardManager = keyboardCanvasInstance.GetComponentInChildren<KeyboardManager>(true);

        if (virtualKeyboard == null || keyboardManager == null)
        {
            Debug.LogError("[KeyboardService] 프리팹에 VirtualKeyboard와 KeyboardManagerTMP가 모두 있어야 합니다.");
            return;
        }

        virtualKeyboard.SetKeyBoardManager(keyboardManager);
        keyboardManager.BindVirtualKeyboard(virtualKeyboard);

        virtualKeyboard.ConfigureBackspaceHold(true, 0.3f, 0.04f);

        keyboardCanvasInstance.SetActive(false);
    }
}
