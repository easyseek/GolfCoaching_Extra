using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;

public class KeyboardManager : MonoBehaviour
{
    public TMP_InputField inputField; // 사용자가 클릭할 InputField
    public GameObject keyboardUI; // 키보드 UI 오브젝트
    private bool isKeyboardActive = false; // 키보드 활성화 상태를 추적

    public Button XBtn;

    private VirtualKeyboard virtualKeyboard;

    void Start()
    {
        if (keyboardUI == null)
        {
            keyboardUI = this.gameObject;
        }

        if (XBtn != null)
        {
            XBtn.onClick.AddListener(DeactivateKeyboard);
        }

        if (inputField != null)
        {
            SubscribeInputEvents(inputField);
        }

        // EventTrigger를 통해 InputField 선택 이벤트 처리
        //EventTrigger trigger = inputField.gameObject.AddComponent<EventTrigger>();

        //EventTrigger.Entry entry = new EventTrigger.Entry();
        //entry.eventID = EventTriggerType.Select;
        //entry.callback.AddListener((eventData) => { ActivateKeyboard(); });

        //trigger.triggers.Add(entry);

        //keyboardUI.GetComponentInChildren<VirtualKeyboard>().SetKeyBoardManager(this); // 가상 키보드에게 키보드 매니저를 알려주기 위함
        //XBtn.onClick.AddListener(DeactivateAllObjects);
    }

    public void BindVirtualKeyboard(VirtualKeyboard vk)
    {
        virtualKeyboard = vk;

        if (virtualKeyboard != null)
        {
            virtualKeyboard.SetKeyBoardManager(this);
        }
    }

    public void AttachTarget(TMP_InputField field)
    {
        if (inputField == field) return;

        // 이전 필드 구독 해제
        if (inputField != null)
        {
            UnsubscribeInputEvents(inputField);
        }

        inputField = field;

        if (inputField != null)
        {
            SubscribeInputEvents(inputField);

            // VirtualTextInputBox를 쓰는 구조라면, 여기서 대상 연동 필요
            // ex) virtualKeyboard.TextInputBox = inputField.GetComponent<VirtualTextInputBox>();
            // (사용 프로젝트 구조에 맞게 연결)
        }

        VirtualTextInputBox box = field.GetComponent<VirtualTextInputBox>();

        if (box == null)
        {
            box = field.gameObject.AddComponent<VirtualTextInputBox>();
        }

        box.Bind(field);

        if (virtualKeyboard != null)
            virtualKeyboard.TextInputBox = box;
    }

    private void SubscribeInputEvents(TMP_InputField field)
    {
        field.onSelect.AddListener(OnTMPSelected);
        field.onDeselect.AddListener(OnTMPDeselected);
        // 포커스 유지/전환 상황에서 가상 키보드 UX를 더 깔끔히 하려면 onEndEdit도 참조 가능
        // field.onEndEdit.AddListener(OnEndEdit);
    }

    private void UnsubscribeInputEvents(TMP_InputField field)
    {
        field.onSelect.RemoveListener(OnTMPSelected);
        field.onDeselect.RemoveListener(OnTMPDeselected);
        // field.onEndEdit.RemoveListener(OnEndEdit);
    }

    private void OnTMPSelected(string _)
    {
        ActivateKeyboard();
    }

    private void OnTMPDeselected(string _)
    {
        // 바깥 클릭 시 자동 닫기를 원하면 주석 해제
        // DeactivateKeyboard();
    }

    public void ActivateKeyboard()
    {
        isKeyboardActive = true;

        if (keyboardUI != null)
        {
            keyboardUI.SetActive(true);
        }

        if (inputField == null)
            return;

        inputField.ActivateInputField();

        StartCoroutine(SetCaretToEndNextFrame());
    }

    private System.Collections.IEnumerator SetCaretToEndNextFrame()
    {
        yield return null;
        //inputField.ActivateInputField();
        int end = inputField.text != null ? inputField.text.Length : 0;
        inputField.selectionStringAnchorPosition = end;
        inputField.selectionStringFocusPosition = end;
        inputField.caretPosition = end;
    }

    public void DeactivateKeyboard()
    {
        isKeyboardActive = false;

        if(virtualKeyboard != null && virtualKeyboard.TextInputBox != null)
        {
            virtualKeyboard.TextInputBox.FinishComposition(true);
        }

        if (inputField != null)
        {
            inputField.DeactivateInputField();
        }

        if (keyboardUI != null)
        {
            keyboardUI.SetActive(false);
        }
    }

    private bool IsPointerOverUIObject()
    {
        // 현재 클릭된 위치가 UI 요소인지 확인
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        // Raycast를 사용해 클릭된 UI 요소가 있는지 체크
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        // Raycast 결과에 UI 요소가 포함되어 있으면 true 반환
        return results.Count > 0;
    }

    public void DeactivateAllObjects()
    {
        DeactivateKeyboard();
    }
}
