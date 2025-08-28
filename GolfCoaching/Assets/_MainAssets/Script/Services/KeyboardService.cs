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
    /// TMP_InputField�� ���� Ű���带 ǥ��
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
    /// Ű���� ����
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
        //    Debug.LogError("[KeyboardService] Resources/VirtualKeyboardCanvas �������� ã�� �� �����ϴ�.");
        //    return;
        //}

        //keyboardCanvasInstance = Instantiate(prefab);
        //DontDestroyOnLoad(keyboardCanvasInstance);

        virtualKeyboard = keyboardCanvasInstance.GetComponentInChildren<VirtualKeyboard>(true);
        keyboardManager = keyboardCanvasInstance.GetComponentInChildren<KeyboardManager>(true);

        if (virtualKeyboard == null || keyboardManager == null)
        {
            Debug.LogError("[KeyboardService] �����տ� VirtualKeyboard�� KeyboardManagerTMP�� ��� �־�� �մϴ�.");
            return;
        }

        virtualKeyboard.SetKeyBoardManager(keyboardManager);
        keyboardManager.BindVirtualKeyboard(virtualKeyboard);

        virtualKeyboard.ConfigureBackspaceHold(true, 0.3f, 0.04f);

        keyboardCanvasInstance.SetActive(false);
    }
}
