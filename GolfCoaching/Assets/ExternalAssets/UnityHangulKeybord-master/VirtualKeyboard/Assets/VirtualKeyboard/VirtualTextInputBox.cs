using UnityEngine;
using TMPro;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class VirtualTextInputBox : MonoBehaviour
{
    [SerializeField] private TMP_InputField input;

    public string TextField => input ? input.text : string.Empty;

    private HangulComposer _ime = new HangulComposer();
    private int _compStart = -1;
    private string _compRendered = string.Empty;

    public void Bind(TMP_InputField field)
    {
        input = field;
    }

    public void Clear()
    {
        if (!input) return;

        input.text = string.Empty;
        SetCaretAndSelection(0);
        input.ForceLabelUpdate();
        input.ActivateInputField();
    }

    // VirtualKeyboard에서 Space/Backspace 같은 특수키 처리용으로 호출
    public void KeyDown(VirtualKey key)
    {
        if (!input)
            return;

        switch (key.KeyType)
        {
            case VirtualKey.kType.kSpace:
                CommitIfComposing();
                InsertPlain(" ");
                break;

            case VirtualKey.kType.kBackspace:
                BackspaceSmart();
                break;

            // Return은 VirtualKeyboard에서 이벤트/닫기 처리
            default:
                break;
        }
    }

    // 일반 문자 입력
    public void KeyDown(char ch)
    {
        if (!input)
            return;

        CommitIfComposing();
        InsertPlain(ch.ToString());
        //InsertText(ch.ToString());
    }

    // 한글 입력(조합 로직을 외부에서 처리한다면 여기서는 삽입만)
    public void KeyDownHangul(char ch)
    {
        if (!input)
            return;

        // ascii가 들어오면 내부에서 한 번 더 매핑 시도
        if (ch <= 127)
        {
            char mapped = AutomateKR.GetHangulSound(ch);
            if (mapped != '\0') ch = mapped;
        }

        // 자모가 아니면 그냥 일반 삽입(폴백)
        if (!IsCompatJamo(ch))
        {
            InsertPlain(ch.ToString());
            return;
        }

        // ↓ 여기부터는 조합기 흐름 (이미 넣어둔 HangulComposer 사용)
        if (!_ime.HasBuffer)
        {
            _compStart = CaretEnd();
            _compRendered = "";
        }

        string replace, append;
        bool startNew;

        _ime.Input(ch, out replace, out append, out startNew);

        ReplaceAtComposition(replace);

        if (!string.IsNullOrEmpty(append))
        {
            InsertAtCaret(append);
            _compStart = CaretEnd() - append.Length;
            _compRendered = append;
        }
        else
        {
            _compRendered = replace;
        }

        if (startNew)
        {
            if (!string.IsNullOrEmpty(append))
            {
                // append를 방금 넣었다면 그 글자(새 블록)의 시작으로 이동
                _compStart = CaretEnd() - append.Length;
                _compRendered = append;
            }
            else
            {
                // append가 없고 replace만 갱신된 경우
                _compStart = CaretEnd() - replace.Length;
                _compRendered = replace;
            }
        }

        input.ForceLabelUpdate();
    }

    // 호환자모 여부(간단 판정)
    private bool IsCompatJamo(char c)
    {
        return (c >= 'ㄱ' && c <= 'ㅎ') || (c >= 'ㅏ' && c <= 'ㅣ');
    }

    // --- 내부 유틸 ---

    private void SetCaretAndSelection(int pos)
    {
        pos = Mathf.Max(0, pos);

        // 선택을 pos로 합치고 캐럿 위치도 맞춤
        input.selectionStringAnchorPosition = pos;
        input.selectionStringFocusPosition = pos;

        // 일부 버전에서 캐럿 보정
        input.caretPosition = pos;
    }

    private void ResetComposition()
    {
        _ime.Reset();
        _compStart = -1;
        _compRendered = string.Empty;
    }

    private void CommitIfComposing()
    {
        if (!_ime.HasBuffer)
            return;

        _ime.Reset();
        _compStart = -1;
        _compRendered = string.Empty;
    }

    private void CommitCompositionKeepCaret()
    {
        _ime.Reset();
        _compStart = -1;
        _compRendered = "";
    }

    private void InsertPlain(string s)
    {
        int start, end;
        GetSelection(out start, out end);
        var text = input.text ?? string.Empty;

        string before = text.Substring(0, start);
        string after = text.Substring(end);

        input.text = before + s + after;

        int caret = start + s.Length;
        SetCaret(caret);
    }

    private void ReplaceAtComposition(string replace)
    {
        // 조합 블록이 없으면 append처럼 삽입
        var text = input.text ?? string.Empty;

        if (_compStart < 0)
        {
            int caret = CaretEnd();
            string before = text.Substring(0, caret);
            string after = text.Substring(caret);
            input.text = before + replace + after;

            SetCaret(caret + replace.Length);
            _compStart = caret;
            return;
        }

        // 기존 조합 결과(_compRendered)를 'replace'로 교체
        int start = Mathf.Clamp(_compStart, 0, text.Length);
        int oldLen = Mathf.Clamp(_compRendered.Length, 0, text.Length - start);

        string before2 = text.Substring(0, start);
        string after2 = text.Substring(start + oldLen);
        input.text = before2 + replace + after2;

        int caret2 = start + replace.Length;
        SetCaret(caret2);
    }

    public void FinishComposition(bool moveCareToEnd = true)
    {
        if (input == null)
            return;

        _ime.Reset();
        _compStart = -1;
        _compRendered = "";

        if(moveCareToEnd)
        {
            int end = input.text != null ? input.text.Length : 0;
            input.selectionAnchorPosition = end;
            input.selectionFocusPosition = end;
            input.caretPosition = end;
        }

        input.ForceLabelUpdate();
    }

    private void InsertAtCaret(string s)
    {
        int caret = CaretEnd();
        string text = input.text ?? string.Empty;

        string before = text.Substring(0, caret);
        string after = text.Substring(caret);
        input.text = before + s + after;

        SetCaret(caret + s.Length);
    }

    private void BackspaceSmart()
    {
        // 조합 중이면 조합 상태에서 한 단계씩 지우기
        if (_ime.HasBuffer)
        {
            string replace;
            bool bufferEmpty;
            _ime.Backspace(out replace, out bufferEmpty);

            if (bufferEmpty)
            {
                // 현재 조합 글자 자체를 텍스트에서 제거
                var text = input.text ?? string.Empty;
                if (_compStart >= 0)
                {
                    int _start = Mathf.Clamp(_compStart, 0, text.Length);
                    int len = Mathf.Clamp(_compRendered.Length, 0, text.Length - _start);

                    string before = text.Substring(0, _start);
                    string after = text.Substring(_start + len);
                    input.text = before + after;

                    SetCaret(_start);
                }
                ResetComposition();
            }
            else
            {
                // 조합 결과를 'replace'로 바꿈
                ReplaceAtComposition(replace);
                _compRendered = replace;
            }

            input.ForceLabelUpdate();
            return;
        }

        // 평범한 백스페이스
        var t = input.text ?? string.Empty;
        int a, b; GetSelection(out a, out b);
        int start = Mathf.Min(a, b);
        int end = Mathf.Max(a, b);

        if (end > start)
        {
            string before = t.Substring(0, start);
            string after = t.Substring(end);
            input.text = before + after;
            SetCaret(start);
        }
        else if (start > 0)
        {
            string before = t.Substring(0, start - 1);
            string after = t.Substring(start);
            input.text = before + after;
            SetCaret(start - 1);
        }

        input.ForceLabelUpdate();
    }

    private void GetSelection(out int start, out int end)
    {
        int a = input.selectionStringAnchorPosition;
        int b = input.selectionStringFocusPosition;
        start = Mathf.Min(a, b);
        end = Mathf.Max(a, b);
    }

    private int CaretEnd()
    {
        int a = input.selectionStringAnchorPosition;
        int b = input.selectionStringFocusPosition;
        return Mathf.Max(a, b);
    }

    private void SetCaret(int pos)
    {
        pos = Mathf.Clamp(pos, 0, input.text != null ? input.text.Length : 0);
        input.selectionStringAnchorPosition = pos;
        input.selectionStringFocusPosition = pos;
        input.caretPosition = pos;
    }

    private class HangulComposer
    {
        private static readonly char[] L_LIST = { 'ㄱ', 'ㄲ', 'ㄴ', 'ㄷ', 'ㄸ', 'ㄹ', 'ㅁ', 'ㅂ', 'ㅃ', 'ㅅ', 'ㅆ', 'ㅇ', 'ㅈ', 'ㅉ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ' };
        private static readonly char[] V_LIST = { 'ㅏ', 'ㅐ', 'ㅑ', 'ㅒ', 'ㅓ', 'ㅔ', 'ㅕ', 'ㅖ', 'ㅗ', 'ㅘ', 'ㅙ', 'ㅚ', 'ㅛ', 'ㅜ', 'ㅝ', 'ㅞ', 'ㅟ', 'ㅠ', 'ㅡ', 'ㅢ', 'ㅣ' };
        private static readonly char[] T_LIST = { '\0', 'ㄱ', 'ㄲ', 'ㄳ', 'ㄴ', 'ㄵ', 'ㄶ', 'ㄷ', 'ㄹ', 'ㄺ', 'ㄻ', 'ㄼ', 'ㄽ', 'ㄾ', 'ㄿ', 'ㅀ', 'ㅁ', 'ㅂ', 'ㅄ', 'ㅅ', 'ㅆ', 'ㅇ', 'ㅈ', 'ㅊ', 'ㅋ', 'ㅌ', 'ㅍ', 'ㅎ' };

        private static readonly Dictionary<char, int> L_IDX = BuildIndex(L_LIST);
        private static readonly Dictionary<char, int> V_IDX = BuildIndex(V_LIST);
        private static readonly Dictionary<char, int> T_IDX = BuildIndex(T_LIST);

        // 이중 자음/모음 결합표
        private static readonly Dictionary<(char, char), char> DOUBLE_L = new()
        {
            {('ㄱ','ㄱ'), 'ㄲ'}, {('ㄷ','ㄷ'), 'ㄸ'}, {('ㅂ','ㅂ'), 'ㅃ'}, {('ㅅ','ㅅ'), 'ㅆ'}, {('ㅈ','ㅈ'), 'ㅉ'}
        };

        private static readonly Dictionary<(char, char), char> DOUBLE_V = new()
        {
            {('ㅗ','ㅏ'),'ㅘ'}, {('ㅗ','ㅐ'),'ㅙ'}, {('ㅗ','ㅣ'),'ㅚ'},
            {('ㅜ','ㅓ'),'ㅝ'}, {('ㅜ','ㅔ'),'ㅞ'}, {('ㅜ','ㅣ'),'ㅟ'},
            {('ㅡ','ㅣ'),'ㅢ'}
        };

        private static readonly Dictionary<(char, char), char> DOUBLE_T = new()
        {
            {('ㄱ','ㅅ'),'ㄳ'},
            {('ㄴ','ㅈ'),'ㄵ'}, {('ㄴ','ㅎ'),'ㄶ'},
            {('ㄹ','ㄱ'),'ㄺ'}, {('ㄹ','ㅁ'),'ㄻ'}, {('ㄹ','ㅂ'),'ㄼ'}, {('ㄹ','ㅅ'),'ㄽ'}, {('ㄹ','ㅌ'),'ㄾ'}, {('ㄹ','ㅍ'),'ㄿ'}, {('ㄹ','ㅎ'),'ㅀ'},
            {('ㅂ','ㅅ'),'ㅄ'}
        };

        // 종성 분리(받침 → 다음 글자 초성 이동)
        private static readonly Dictionary<char, (char remain, char moved)> SPLIT_T = new()
        {
            {'ㄳ',('ㄱ','ㅅ')}, {'ㄵ',('ㄴ','ㅈ')}, {'ㄶ',('ㄴ','ㅎ')},
            {'ㄺ',('ㄹ','ㄱ')}, {'ㄻ',('ㄹ','ㅁ')}, {'ㄼ',('ㄹ','ㅂ')}, {'ㄽ',('ㄹ','ㅅ')}, {'ㄾ',('ㄹ','ㅌ')}, {'ㄿ',('ㄹ','ㅍ')}, {'ㅀ',('ㄹ','ㅎ')},
            {'ㅄ',('ㅂ','ㅅ')}
        };

        // 복모음/복받침 Backspace 분해
        private static readonly Dictionary<char, (char a, char b)> DECOMP_V = new()
        {
            {'ㅘ',('ㅗ','ㅏ')}, {'ㅙ',('ㅗ','ㅐ')}, {'ㅚ',('ㅗ','ㅣ')},
            {'ㅝ',('ㅜ','ㅓ')}, {'ㅞ',('ㅜ','ㅔ')}, {'ㅟ',('ㅜ','ㅣ')},
            {'ㅢ',('ㅡ','ㅣ')}
        };

        private static readonly Dictionary<char, (char a, char b)> DECOMP_L = new()
        {
            {'ㄲ',('ㄱ','ㄱ')}, {'ㄸ',('ㄷ','ㄷ')}, {'ㅃ',('ㅂ','ㅂ')}, {'ㅆ',('ㅅ','ㅅ')}, {'ㅉ',('ㅈ','ㅈ')},
        };

        private static readonly Dictionary<char, (char a, char b)> DECOMP_T = new()
        {
            {'ㄳ',('ㄱ','ㅅ')},{'ㄵ',('ㄴ','ㅈ')},{'ㄶ',('ㄴ','ㅎ')},{'ㄺ',('ㄹ','ㄱ')},{'ㄻ',('ㄹ','ㅁ')},{'ㄼ',('ㄹ','ㅂ')},{'ㄽ',('ㄹ','ㅅ')},{'ㄾ',('ㄹ','ㅌ')},{'ㄿ',('ㄹ','ㅍ')},{'ㅀ',('ㄹ','ㅎ')},{'ㅄ',('ㅂ','ㅅ')},
        };

        static Dictionary<char, int> BuildIndex(char[] arr)
        {
            var d = new Dictionary<char, int>(arr.Length);
            for (int i = 0; i < arr.Length; i++) d[arr[i]] = i;
            return d;
        }

        private char L, V, T; // compatibility jamo

        public bool HasBuffer => (L != '\0') || (V != '\0');

        public void Reset() { L = V = T = '\0'; }

        public void Input(char jamo, out string replace, out string append, out bool startNewBlock)
        {
            replace = Render();   // 기본은 현재 렌더를 교체

            append = null;
            startNewBlock = false;

            bool isV = V_IDX.ContainsKey(jamo);
            bool isC = L_IDX.ContainsKey(jamo) || T_IDX.ContainsKey(jamo);

            if (!isV && !isC)
            {
                // 한글 자모가 아니면 현 블록 커밋 후 외부에서 일반 삽입
                startNewBlock = true;

                return;
            }

            // 1) L 비어있음
            if (L == '\0')
            {
                if (isV)
                {
                    // 모음만 입력: ㅇ 자동 추가하지 않음
                    if (V == '\0')
                    {
                        V = jamo;
                        replace = Render();
                    }
                    else
                    {
                        // 복모음 결합 시도 (ㅗ+ㅏ=ㅘ 등)
                        if (DOUBLE_V.TryGetValue((V, jamo), out char dv))
                        {
                            V = dv;
                            replace = Render();
                        }
                        else
                        {
                            // 결합 불가면 이전 모음 확정, 새 모음 블록 시작
                            startNewBlock = true;
                            append = jamo.ToString();

                            L = '\0';
                            V = jamo; // 새 블록 상태
                            T = '\0';
                        }
                    }

                    return;
                }
                else // 자음: 초성으로 시작
                {
                    append = jamo.ToString();
                    startNewBlock = true;
                    L = jamo;
                    V = T = '\0';

                    return;
                }
            }

            // 2) L 있고 V 없음 (초성 단계)
            if (V == '\0')
            {
                if (isV)
                {
                    V = jamo;                 // 초성+중성
                    replace = Render();

                    return;
                }
                else
                {
                    // 자동 쌍자음 합성 금지: 새 블록으로 자음 시작
                    // (앞 글자는 그대로 두고, 다음 글자에 입력)
                    append = jamo.ToString();

                    startNewBlock = true;

                    // 새 블록 상태로 전환
                    L = jamo;
                    V = T = '\0';

                    return;
                }
            }

            // 3) L,V 있고 T 없음 (종성 자리)
            if (T == '\0')
            {
                if (isV)
                {
                    // 모음 연속 → 복모음 시도
                    if (DOUBLE_V.TryGetValue((V, jamo), out char dv))
                    {
                        V = dv;
                        replace = Render();

                        return;
                    }

                    // 이전 음절 확정, 새 음절(초성 없음 → 보류) 대신 ㅇ 자동 추가는 하지 않음
                    // 새 블록을 모음 자모로 시작시키려면 append에 모음 넣고 startNewBlock
                    append = jamo.ToString();
                    startNewBlock = true;
                    L = '\0'; V = jamo; T = '\0';

                    return;
                }
                else
                {
                    // 자음 → 받침으로
                    if (T_IDX.ContainsKey(jamo))
                    {
                        T = jamo;
                        replace = Render();

                        return;
                    }
                }
            }

            // 4) L,V,T 모두 있음 (받침 있음)
            {
                if (isV)
                {
                    // 모음이 들어오면 받침을 다음 초성으로 이동(분리)
                    char remain, moved;

                    if (SPLIT_T.TryGetValue(T, out var pair)) 
                    {
                        remain = pair.remain;
                        moved = pair.moved;
                    }
                    else
                    {
                        remain = '\0';
                        moved = T;
                    }

                    // 이전 음절은 remain 받침으로 교체(치환)
                    T = remain;
                    replace = Render();

                    // 새 블록: (이전 받침 → 초성) + 이번 모음
                    char newL = moved == '\0' ? '\0' : moved; // 초성 없는 모음 시작도 허용

                    if (newL == '\0')
                    {
                        append = jamo.ToString();          // 모음 자모로 시작
                        L = '\0'; V = jamo; T = '\0';
                    }
                    else
                    {
                        var newSyll = ComposeSyllable(newL, jamo, '\0');
                        append = newSyll.ToString();
                        L = newL; V = jamo; T = '\0';
                    }
                    startNewBlock = true;

                    return;
                }
                else
                {
                    // 자음 추가 → 복받침 시도
                    if (DOUBLE_T.TryGetValue((T, jamo), out char dt))
                    {
                        T = dt;
                        replace = Render();

                        return;
                    }

                    // 안되면 이전 음절 확정하고 새 음절 시작(초성=자음)
                    append = jamo.ToString();
                    startNewBlock = true;

                    L = jamo;
                    V = T = '\0';

                    return;
                }
            }
        }

        public void Backspace(out string replace, out bool bufferEmpty)
        {
            bufferEmpty = false;

            if (T != '\0')
            {
                // 복받침이면 한 글자만 줄이기
                if (DECOMP_T.TryGetValue(T, out var parts))
                {
                    T = parts.a; // 마지막 요소 제거
                }
                else
                {
                    T = '\0';
                }
                replace = Render();

                return;
            }

            if (V != '\0')
            {
                // 복모음이면 한 글자만 줄이기
                if (DECOMP_V.TryGetValue(V, out var mp))
                {
                    V = mp.a; // 마지막 요소 제거
                    replace = Render();

                    return;
                }
                else
                {
                    V = '\0';
                    // V가 없어지면 초성만 남으니 자모 표시
                    replace = Render();

                    return;
                }
            }

            if (L != '\0')
            {
                // 쌍자음이면 줄이기
                if (DECOMP_L.TryGetValue(L, out var lp))
                {
                    L = lp.a;
                    replace = Render();

                    return;
                }

                // 초성까지 삭제 → 버퍼 비움
                L = '\0';
                replace = "";
                bufferEmpty = true;

                return;
            }

            replace = "";
        }

        private string Render()
        {
            if (L == '\0')
            {
                if (V != '\0') 
                    return V.ToString();

                return "";
            }

            if (V == '\0') 
                return L.ToString(); // 초성만 있을 때는 자모 그대로
                                     // 초성+중성(+종성) 완성형 조합
            char s = ComposeSyllable(L, V, T);

            return s.ToString();
        }

        private static char ComposeSyllable(char Lc, char Vc, char Tc)
        {
            int Lx = L_IDX.TryGetValue(Lc, out var li) ? li : 11 /*ㅇ*/;
            int Vx = V_IDX.TryGetValue(Vc, out var vi) ? vi : 0;
            int Tx = T_IDX.TryGetValue(Tc, out var ti) ? ti : 0;

            int code = 0xAC00 + ((Lx * 21) + Vx) * 28 + Tx;

            return (char)code;
        }
    }
}
