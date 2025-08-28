using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.Rendering;
using TMPro;

public class VirtualKeyboard : MonoBehaviour {

    public enum kLanguage
    {
        [InspectorName("한글")]
        kKorean,
        [InspectorName("영어")]
        kEnglish
    };

    [System.Serializable]
    public class ReturnEvent : UnityEvent<string> { }
    public delegate bool VirtualKeyboardReturnDelegate(string text);

    public kLanguage DefaultLanguage = kLanguage.kKorean;
    public VirtualKeyboardReturnDelegate OnReturnDelegate { get; set; }

    public ReturnEvent OnReturnEventHandler;
    
    public VirtualTextInputBox TextInputBox = null;

    public int MaxTextCount = 20;

    private bool _backspaceHoldEnabled = true;
    public bool BackspaceHoldEnabled {
        get { return _backspaceHoldEnabled; }
    }

    private float _backspaceFirstDelay = 0.35f;
    public float BackspaceFirstDelay {
        get { return _backspaceFirstDelay; }
    }

    private float _backspaceRepeatInterval = 0.05f;
    public float BackspaceRepeatInterval {
        get { return _backspaceRepeatInterval; }
    }

    protected AudioSource mAudioSource;
    protected bool mPressShift = false;
    protected bool mCapsLocked = false;
    protected kLanguage mLanguage = kLanguage.kKorean;
    protected Dictionary<char, char> CHARACTER_TABLE = new Dictionary<char, char>
    {
        {'1', '!'}, {'2', '@'}, {'3', '#'}, {'4', '$'}, {'5', '%'},{'6', '^'}, {'7', '&'}, {'8', '*'}, {'9', '('},{'0', ')'},
        { '`', '~'},   {'-', '_'}, {'=', '+'}, {'[', '{'}, {']', '}'}, {'\\', '|'}, {',', '<'}, {'.', '>'}, {'/', '?'}, {';', ':'}, {'\'', '"'}
    };

    private static readonly Dictionary<char, char> SHIFT_JAMO = new()
    {
        // 자음(쌍자음)
        {'ㄱ','ㄲ'}, {'ㄷ','ㄸ'}, {'ㅂ','ㅃ'}, {'ㅅ','ㅆ'}, {'ㅈ','ㅉ'},
        // 모음(장음)
        {'ㅐ','ㅒ'}, {'ㅔ','ㅖ'},
    };

    private KeyboardManager keyboardManager;

    void Awake()
    {
        mAudioSource = GetComponent<AudioSource>();
        OnReturnDelegate = PressedReturn; // 대리자 기능 할당

        VirtualKey[] keys = GetComponentsInChildren<VirtualKey>(true);

        foreach (VirtualKey k in keys)
            k.Keyboard = this;
    }

    private void OnEnable()
    {
        mLanguage = DefaultLanguage;
        mPressShift = false;
	}

    public void Clear()
    {
        if (TextInputBox != null)
        {
            TextInputBox.Clear();
        }
    }

    public void PlayKeyAudio()
    {
        if (mAudioSource != null)
        {
            mAudioSource.Play();
        }
    }

    public void KeyDown(VirtualKey _key)
    {
        if (_key.KeyType != VirtualKey.kType.kReturn)
        {
            //PlayKeyAudio();
        }
        Debug.Log($"[KeyDown] {_key.KeyType}");

        if (_key.KeyType == VirtualKey.kType.kShift)
        {
            mPressShift = !mPressShift;
            return;
        }
        if (_key.KeyType == VirtualKey.kType.kCapsLock)
        {
            mCapsLocked = !mCapsLocked;
            return;
        }
        if (_key.KeyType == VirtualKey.kType.kHangul)
        {
            mLanguage = (mLanguage == kLanguage.kKorean) ? kLanguage.kEnglish : kLanguage.kKorean;
            mPressShift = false;
            return;
        }

        if (TextInputBox == null)
        {
            Debug.Log($"[KeyDown] InputBox 널임 {_key.KeyType}");
            return;
        }

        if (TextInputBox != null)
        {
            Debug.Log($"[KeyDown] InputBox 널아님 {_key.KeyType}");
            switch (_key.KeyType)
            {
                //case VirtualKey.kType.kShift:
                //    {
                //        mPressShift = !mPressShift;
                //    }
                //    break;
                //case VirtualKey.kType.kCapsLock:
                //    {
                //        mCapsLocked = !mCapsLocked;
                //    }
                //    break;
                case VirtualKey.kType.kHangul:
                    {
                        mLanguage = (mLanguage == kLanguage.kKorean) ? kLanguage.kEnglish : kLanguage.kKorean;

                        mPressShift = false;
                    }
                    break;
                case VirtualKey.kType.kSpace:
                    {
                        if (TextInputBox.TextField.Length < MaxTextCount)
                        {
                            TextInputBox.KeyDown(_key);
                        }
                    }
                    break;
                case VirtualKey.kType.kBackspace:
                    {
                        TextInputBox.KeyDown(_key);
                    }
                    break;
                case VirtualKey.kType.kReturn:
                    {
                        if(TextInputBox != null)
                        {
                            TextInputBox.FinishComposition(true);
                        }

                        OnReturnEventHandler?.Invoke(TextInputBox != null ? TextInputBox.TextField : string.Empty);

                        bool ok = true;

                        if (OnReturnDelegate != null)
                        {
                            ok = OnReturnDelegate.Invoke(TextInputBox != null ? TextInputBox.TextField : string.Empty);
                        }

                        if (ok)
                        {
                            PlayKeyAudio();
                        }
                            
                        if (keyboardManager != null)
                        {
                            keyboardManager.DeactivateAllObjects();
                        }
                    }
                    break;
                case VirtualKey.kType.kCharacter:
                    {
                        if (TextInputBox.TextField.Length < MaxTextCount)
                        {
                            char keyCharacter = _key.KeyCharacter;

                            // 영어 모드: 그대로
                            if (mLanguage == kLanguage.kEnglish)
                            {
                                if (mPressShift) { keyCharacter = char.ToUpper(keyCharacter); mPressShift = false; }
                                if (mCapsLocked) keyCharacter = char.ToUpper(keyCharacter);
                                TextInputBox.KeyDown(keyCharacter);
                            }
                            else // 한글 모드: 반드시 KeyDownHangul()로 보냄
                            {
                                // 영문키 → 호환 자모 매핑
                                char jamo = AutomateKR.GetHangulSound(keyCharacter);

                                // Shift가 켜져 있으면 한글 Shift 매핑 적용(한 번만)
                                if (mPressShift)
                                {
                                    jamo = ApplyKoreanShift(jamo);
                                    mPressShift = false; // 1회성
                                }

                                // CapsLock은 한글에 영향 없음(원하면 확장 가능)
                                TextInputBox.KeyDownHangul(jamo);
                            }
                        }
                    }
                    break;
                case VirtualKey.kType.kOther:
                    {
                        if (TextInputBox.TextField.Length < MaxTextCount)
                        {
                            char keyCharacter = _key.KeyCharacter;

                            // Shift 키가 눌렸을 경우, CHARACTER_TABLE에서 해당 특수 문자 찾기
                            if (mPressShift && CHARACTER_TABLE.TryGetValue(keyCharacter, out char specialChar))
                            {
                                keyCharacter = specialChar; // 특수 문자로 치환
                            }

                            TextInputBox.KeyDown(keyCharacter); // 변환된 키 또는 기본 키를 입력 처리
                            mPressShift = false; // Shift 키 상태 해제 (필요에 따라 유지하려면 이 줄을 제거)
                        }
                    }
                    break;

                case VirtualKey.kType.kSymbol_Star:
                    //https://www.unicodepedia.com/groups/miscellaneous-symbols/
                    /*WHITE STAR*/
                    TextInputBox.KeyDown('\u2606');
                    break;
                case VirtualKey.kType.kSymbol_Heart:
                    /*WHITE HEART*/
                    TextInputBox.KeyDown('\u2661');
                    break;

            }

            //var proInstance = ProSelect_ProLayout.Instance;
            //if (proInstance != null) proInstance.SetProLayoutActivate(TextInputBox.TextField);
        }
    }

    private char ApplyKoreanShift(char jamo)
    {
        return SHIFT_JAMO.TryGetValue(jamo, out var s) ? s : jamo;
    }

    public bool PressedShift
    {
        get
        {
            return mPressShift;
        }
    }

    public bool CapLockOn
    {
        get
        {
            return mCapsLocked;
        }
    }

    public kLanguage Language 
    {
        get
        {
            return mLanguage;
        }
    }

    private bool PressedReturn(string inputString) // 엔터 입력 함수
    {
        if (keyboardManager != null) keyboardManager.DeactivateAllObjects(); // 가상 키보드 비활성화
        return !string.IsNullOrEmpty(inputString); // 단어에 해당하는 프로 Pro UI만 활성화, 그 외의 Pro UI는 비활성화
    }

    public void SetKeyBoardManager(KeyboardManager obj) // 키보드 매니저를 설정하는 함수
    {
        if (keyboardManager == null || keyboardManager != obj) // 값이 없거나 매개변수와 값이 다르다면
            keyboardManager = obj; // 매개변수 값 대입
    }

    public void ConfigureBackspaceHold(bool enabled, float firstDelay, float repeatInterval)
    {
        _backspaceHoldEnabled = enabled;
        _backspaceFirstDelay = Mathf.Max(0f, firstDelay);
        _backspaceRepeatInterval = Mathf.Max(0.001f, repeatInterval);
    }
}
