using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[DisallowMultipleComponent]
public class VirtualKey : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("연결")]
    public VirtualKeyboard Keyboard;  // 각 키가 소속 VirtualKeyboard를 참조

    public enum kType
    {
        kCharacter,
        kOther,
        kReturn,
        kSpace,
        kBackspace,
        kShift,
        kTab,
        kCapsLock,
        kHangul,
        kSymbol_Star,
        kSymbol_Heart,
    }

    [Header("키 속성")]
    public float AlphaHitTestMinimumThreshold;
    public char KeyCharacter;
    public kType KeyType = kType.kCharacter;

    [SerializeField] private bool mKeepPresed;

    public bool KeepPressed {
        set => mKeepPresed = value;
        get => mKeepPresed;
    }

    // 라벨: TMP로 변경
    private TextMeshProUGUI mKeyText;
    private TextMeshProUGUI mShiftedText;

    // 색상
    static readonly Color Color_Deactivate = Color.white;
    static readonly Color Color_Active = Color.black;

    // 상태 캐시(불필요한 매 프레임 갱신 방지)
    private bool _lastShift;
    private bool _lastCaps;
    private VirtualKeyboard.kLanguage _lastLang;
    private char _lastKeyChar;
    private bool _firstRefreshDone;

    private bool _isHolding = false;
    private float _holdElapsed = 0f;
    private float _repeatElapsed = 0f;

    private void Awake()
    {
        // 알파 히트 테스트(키 이미지 가장자리 클릭 방지)
        if (AlphaHitTestMinimumThreshold > float.Epsilon)
        {
            var image = GetComponent<Image>();
            if (image != null) image.alphaHitTestMinimumThreshold = AlphaHitTestMinimumThreshold;
        }

        // 라벨 자동 찾기 (자식 "Text", "ShiftedText")
        var txtTransform = transform.Find("Text");
        if (txtTransform) mKeyText = txtTransform.GetComponent<TextMeshProUGUI>();

        var shifted = transform.Find("ShiftedText");
        if (shifted) mShiftedText = shifted.GetComponent<TextMeshProUGUI>();

        // Button 클릭 → KeyDown 연결(있을 때만)
        var btn = GetComponent<Button>();
        if (btn) btn.onClick.RemoveAllListeners();
    }

    private void OnEnable()
    {
        ForceRefreshVisual();

        _isHolding = false;
        _holdElapsed = 0f;
        _repeatElapsed = 0f;
    }

    private void Update()
    {
        if (_isHolding && KeyType == kType.kBackspace && Keyboard != null && Keyboard.BackspaceHoldEnabled)
        {
            _holdElapsed += Time.unscaledDeltaTime;

            if (_holdElapsed >= Keyboard.BackspaceFirstDelay)
            {
                _repeatElapsed += Time.unscaledDeltaTime;

                while (_repeatElapsed >= Keyboard.BackspaceRepeatInterval)
                {
                    Keyboard.KeyDown(this);

                    _repeatElapsed -= Keyboard.BackspaceRepeatInterval;
                }
            }
        }

        // Keyboard가 없으면 표시 갱신 불가
        if (Keyboard == null)
            return;

        // 상태 변화가 있을 때만 갱신
        if (!_firstRefreshDone ||
            _lastShift != Keyboard.PressedShift ||
            _lastCaps != Keyboard.CapLockOn ||
            _lastLang != Keyboard.Language ||
            _lastKeyChar != KeyCharacter)
        {
            RefreshVisual();
        }
    }

    private void RefreshVisual()
    {
        _firstRefreshDone = true;

        _lastShift = Keyboard.PressedShift;
        _lastCaps = Keyboard.CapLockOn;
        _lastLang = Keyboard.Language;
        _lastKeyChar = KeyCharacter;

        RefreshLabel();
    }

    private void RefreshLabel()
    {
        switch (KeyType)
        {
            case kType.kCharacter:
                if (!mKeyText) return;

                if (Keyboard.Language == VirtualKeyboard.kLanguage.kKorean)
                {
                    if (mShiftedText && !mShiftedText.gameObject.activeSelf)
                        mShiftedText.gameObject.SetActive(true);

                    // 표시용 라벨은 AutomateKR로 한글 자모 보여줌
                    if (Keyboard.PressedShift)
                    {
                        if (mShiftedText) mShiftedText.text = AutomateKR.GetHangulSound(KeyCharacter).ToString();
                        mKeyText.text = AutomateKR.GetHangulSound(char.ToUpper(KeyCharacter)).ToString();
                    }
                    else
                    {
                        if (mShiftedText) mShiftedText.text = AutomateKR.GetHangulSound(char.ToUpper(KeyCharacter)).ToString();
                        mKeyText.text = AutomateKR.GetHangulSound(KeyCharacter).ToString();
                    }
                }
                else
                {
                    var ch = (_lastCaps || _lastShift) ? char.ToUpper(KeyCharacter) : KeyCharacter;
                    mKeyText.text = ch.ToString();
                    if (mShiftedText && mShiftedText.gameObject.activeSelf)
                        mShiftedText.gameObject.SetActive(false);
                }
                break;

            case kType.kOther:
                if (mShiftedText && mKeyText)
                {
                    if (Keyboard.PressedShift)
                    {
                        mKeyText.color = Color_Deactivate;
                        mShiftedText.color = Color_Active;
                    }
                    else
                    {
                        mKeyText.color = Color_Active;
                        mShiftedText.color = Color_Deactivate;
                    }
                }
                break;

            case kType.kShift:
                if (mKeyText) mKeyText.color = Keyboard.PressedShift ? Color_Active : Color_Deactivate;
                break;

            case kType.kCapsLock:
                if (mKeyText) mKeyText.color = Keyboard.CapLockOn ? Color.white : Color_Deactivate;
                break;
        }
    }

    private void ForceRefreshVisual()
    {
        _firstRefreshDone = false; // 다음 Update에서 무조건 갱신
    }

    public bool HasShiftedChar() => mShiftedText != null;

    public void OnPointerDown(PointerEventData eventData)
    {
        if(Keyboard == null)
            return;

        // 길게 누르기 시작 처리 등 필요 시
        Keyboard.KeyDown(this);

        if(KeyType == kType.kBackspace && Keyboard.BackspaceHoldEnabled)
        {
            _isHolding = true;
            _holdElapsed = 0f;
            _repeatElapsed = 0f;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 길게 누르기 종료 처리 등 필요 시
        _isHolding = false;
    }

    [ExecuteInEditMode]
    public void GetKeyCharacterFromObjectName()
    {
        string name = transform.name;
        if (!string.IsNullOrEmpty(name))
        {
            KeyCharacter = name[0];
            ForceRefreshVisual();
        }
    }
}
