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

    // VirtualKeyboard���� Space/Backspace ���� Ư��Ű ó�������� ȣ��
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

            // Return�� VirtualKeyboard���� �̺�Ʈ/�ݱ� ó��
            default:
                break;
        }
    }

    // �Ϲ� ���� �Է�
    public void KeyDown(char ch)
    {
        if (!input)
            return;

        CommitIfComposing();
        InsertPlain(ch.ToString());
        //InsertText(ch.ToString());
    }

    // �ѱ� �Է�(���� ������ �ܺο��� ó���Ѵٸ� ���⼭�� ���Ը�)
    public void KeyDownHangul(char ch)
    {
        if (!input)
            return;

        // ascii�� ������ ���ο��� �� �� �� ���� �õ�
        if (ch <= 127)
        {
            char mapped = AutomateKR.GetHangulSound(ch);
            if (mapped != '\0') ch = mapped;
        }

        // �ڸ� �ƴϸ� �׳� �Ϲ� ����(����)
        if (!IsCompatJamo(ch))
        {
            InsertPlain(ch.ToString());
            return;
        }

        // �� ������ʹ� ���ձ� �帧 (�̹� �־�� HangulComposer ���)
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
                // append�� ��� �־��ٸ� �� ����(�� ���)�� �������� �̵�
                _compStart = CaretEnd() - append.Length;
                _compRendered = append;
            }
            else
            {
                // append�� ���� replace�� ���ŵ� ���
                _compStart = CaretEnd() - replace.Length;
                _compRendered = replace;
            }
        }

        input.ForceLabelUpdate();
    }

    // ȣȯ�ڸ� ����(���� ����)
    private bool IsCompatJamo(char c)
    {
        return (c >= '��' && c <= '��') || (c >= '��' && c <= '��');
    }

    // --- ���� ��ƿ ---

    private void SetCaretAndSelection(int pos)
    {
        pos = Mathf.Max(0, pos);

        // ������ pos�� ��ġ�� ĳ�� ��ġ�� ����
        input.selectionStringAnchorPosition = pos;
        input.selectionStringFocusPosition = pos;

        // �Ϻ� �������� ĳ�� ����
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
        // ���� ����� ������ appendó�� ����
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

        // ���� ���� ���(_compRendered)�� 'replace'�� ��ü
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
        // ���� ���̸� ���� ���¿��� �� �ܰ辿 �����
        if (_ime.HasBuffer)
        {
            string replace;
            bool bufferEmpty;
            _ime.Backspace(out replace, out bufferEmpty);

            if (bufferEmpty)
            {
                // ���� ���� ���� ��ü�� �ؽ�Ʈ���� ����
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
                // ���� ����� 'replace'�� �ٲ�
                ReplaceAtComposition(replace);
                _compRendered = replace;
            }

            input.ForceLabelUpdate();
            return;
        }

        // ����� �齺���̽�
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
        private static readonly char[] L_LIST = { '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��' };
        private static readonly char[] V_LIST = { '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��' };
        private static readonly char[] T_LIST = { '\0', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��', '��' };

        private static readonly Dictionary<char, int> L_IDX = BuildIndex(L_LIST);
        private static readonly Dictionary<char, int> V_IDX = BuildIndex(V_LIST);
        private static readonly Dictionary<char, int> T_IDX = BuildIndex(T_LIST);

        // ���� ����/���� ����ǥ
        private static readonly Dictionary<(char, char), char> DOUBLE_L = new()
        {
            {('��','��'), '��'}, {('��','��'), '��'}, {('��','��'), '��'}, {('��','��'), '��'}, {('��','��'), '��'}
        };

        private static readonly Dictionary<(char, char), char> DOUBLE_V = new()
        {
            {('��','��'),'��'}, {('��','��'),'��'}, {('��','��'),'��'},
            {('��','��'),'��'}, {('��','��'),'��'}, {('��','��'),'��'},
            {('��','��'),'��'}
        };

        private static readonly Dictionary<(char, char), char> DOUBLE_T = new()
        {
            {('��','��'),'��'},
            {('��','��'),'��'}, {('��','��'),'��'},
            {('��','��'),'��'}, {('��','��'),'��'}, {('��','��'),'��'}, {('��','��'),'��'}, {('��','��'),'��'}, {('��','��'),'��'}, {('��','��'),'��'},
            {('��','��'),'��'}
        };

        // ���� �и�(��ħ �� ���� ���� �ʼ� �̵�)
        private static readonly Dictionary<char, (char remain, char moved)> SPLIT_T = new()
        {
            {'��',('��','��')}, {'��',('��','��')}, {'��',('��','��')},
            {'��',('��','��')}, {'��',('��','��')}, {'��',('��','��')}, {'��',('��','��')}, {'��',('��','��')}, {'��',('��','��')}, {'��',('��','��')},
            {'��',('��','��')}
        };

        // ������/����ħ Backspace ����
        private static readonly Dictionary<char, (char a, char b)> DECOMP_V = new()
        {
            {'��',('��','��')}, {'��',('��','��')}, {'��',('��','��')},
            {'��',('��','��')}, {'��',('��','��')}, {'��',('��','��')},
            {'��',('��','��')}
        };

        private static readonly Dictionary<char, (char a, char b)> DECOMP_L = new()
        {
            {'��',('��','��')}, {'��',('��','��')}, {'��',('��','��')}, {'��',('��','��')}, {'��',('��','��')},
        };

        private static readonly Dictionary<char, (char a, char b)> DECOMP_T = new()
        {
            {'��',('��','��')},{'��',('��','��')},{'��',('��','��')},{'��',('��','��')},{'��',('��','��')},{'��',('��','��')},{'��',('��','��')},{'��',('��','��')},{'��',('��','��')},{'��',('��','��')},{'��',('��','��')},
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
            replace = Render();   // �⺻�� ���� ������ ��ü

            append = null;
            startNewBlock = false;

            bool isV = V_IDX.ContainsKey(jamo);
            bool isC = L_IDX.ContainsKey(jamo) || T_IDX.ContainsKey(jamo);

            if (!isV && !isC)
            {
                // �ѱ� �ڸ� �ƴϸ� �� ��� Ŀ�� �� �ܺο��� �Ϲ� ����
                startNewBlock = true;

                return;
            }

            // 1) L �������
            if (L == '\0')
            {
                if (isV)
                {
                    // ������ �Է�: �� �ڵ� �߰����� ����
                    if (V == '\0')
                    {
                        V = jamo;
                        replace = Render();
                    }
                    else
                    {
                        // ������ ���� �õ� (��+��=�� ��)
                        if (DOUBLE_V.TryGetValue((V, jamo), out char dv))
                        {
                            V = dv;
                            replace = Render();
                        }
                        else
                        {
                            // ���� �Ұ��� ���� ���� Ȯ��, �� ���� ��� ����
                            startNewBlock = true;
                            append = jamo.ToString();

                            L = '\0';
                            V = jamo; // �� ��� ����
                            T = '\0';
                        }
                    }

                    return;
                }
                else // ����: �ʼ����� ����
                {
                    append = jamo.ToString();
                    startNewBlock = true;
                    L = jamo;
                    V = T = '\0';

                    return;
                }
            }

            // 2) L �ְ� V ���� (�ʼ� �ܰ�)
            if (V == '\0')
            {
                if (isV)
                {
                    V = jamo;                 // �ʼ�+�߼�
                    replace = Render();

                    return;
                }
                else
                {
                    // �ڵ� ������ �ռ� ����: �� ������� ���� ����
                    // (�� ���ڴ� �״�� �ΰ�, ���� ���ڿ� �Է�)
                    append = jamo.ToString();

                    startNewBlock = true;

                    // �� ��� ���·� ��ȯ
                    L = jamo;
                    V = T = '\0';

                    return;
                }
            }

            // 3) L,V �ְ� T ���� (���� �ڸ�)
            if (T == '\0')
            {
                if (isV)
                {
                    // ���� ���� �� ������ �õ�
                    if (DOUBLE_V.TryGetValue((V, jamo), out char dv))
                    {
                        V = dv;
                        replace = Render();

                        return;
                    }

                    // ���� ���� Ȯ��, �� ����(�ʼ� ���� �� ����) ��� �� �ڵ� �߰��� ���� ����
                    // �� ����� ���� �ڸ�� ���۽�Ű���� append�� ���� �ְ� startNewBlock
                    append = jamo.ToString();
                    startNewBlock = true;
                    L = '\0'; V = jamo; T = '\0';

                    return;
                }
                else
                {
                    // ���� �� ��ħ����
                    if (T_IDX.ContainsKey(jamo))
                    {
                        T = jamo;
                        replace = Render();

                        return;
                    }
                }
            }

            // 4) L,V,T ��� ���� (��ħ ����)
            {
                if (isV)
                {
                    // ������ ������ ��ħ�� ���� �ʼ����� �̵�(�и�)
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

                    // ���� ������ remain ��ħ���� ��ü(ġȯ)
                    T = remain;
                    replace = Render();

                    // �� ���: (���� ��ħ �� �ʼ�) + �̹� ����
                    char newL = moved == '\0' ? '\0' : moved; // �ʼ� ���� ���� ���۵� ���

                    if (newL == '\0')
                    {
                        append = jamo.ToString();          // ���� �ڸ�� ����
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
                    // ���� �߰� �� ����ħ �õ�
                    if (DOUBLE_T.TryGetValue((T, jamo), out char dt))
                    {
                        T = dt;
                        replace = Render();

                        return;
                    }

                    // �ȵǸ� ���� ���� Ȯ���ϰ� �� ���� ����(�ʼ�=����)
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
                // ����ħ�̸� �� ���ڸ� ���̱�
                if (DECOMP_T.TryGetValue(T, out var parts))
                {
                    T = parts.a; // ������ ��� ����
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
                // �������̸� �� ���ڸ� ���̱�
                if (DECOMP_V.TryGetValue(V, out var mp))
                {
                    V = mp.a; // ������ ��� ����
                    replace = Render();

                    return;
                }
                else
                {
                    V = '\0';
                    // V�� �������� �ʼ��� ������ �ڸ� ǥ��
                    replace = Render();

                    return;
                }
            }

            if (L != '\0')
            {
                // �������̸� ���̱�
                if (DECOMP_L.TryGetValue(L, out var lp))
                {
                    L = lp.a;
                    replace = Render();

                    return;
                }

                // �ʼ����� ���� �� ���� ���
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
                return L.ToString(); // �ʼ��� ���� ���� �ڸ� �״��
                                     // �ʼ�+�߼�(+����) �ϼ��� ����
            char s = ComposeSyllable(L, V, T);

            return s.ToString();
        }

        private static char ComposeSyllable(char Lc, char Vc, char Tc)
        {
            int Lx = L_IDX.TryGetValue(Lc, out var li) ? li : 11 /*��*/;
            int Vx = V_IDX.TryGetValue(Vc, out var vi) ? vi : 0;
            int Tx = T_IDX.TryGetValue(Tc, out var ti) ? ti : 0;

            int code = 0xAC00 + ((Lx * 21) + Vx) * 28 + Tx;

            return (char)code;
        }
    }
}
