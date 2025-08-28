using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Linq;
using Enums;

public class ProCardController : MonoBehaviour
{
    ProInfoData proInfoData;

    [SerializeField] FilterItemController[] filters;

    [SerializeField] private GameObject prolayoutbutton;
    [SerializeField] private GameObject badgeObj;
    
    [SerializeField] Image imgProImage;
    [SerializeField] Image badgeImage;

    [SerializeField] TextMeshProUGUI txtProName;
    [SerializeField] TextMeshProUGUI txtProInfo;

    [SerializeField] private Button m_InfoBtn;
    public Button InfoBtn 
        {
        get { return m_InfoBtn; }
    }

    private Transform proLayoutTransform;
    public Transform ProLayoutTransform { get { return proLayoutTransform; } }

    [SerializeField] Sprite[] badgeSprites;

    public int ProUid;

    Action<int> actProInfo;

    public void SetProCard(ProInfoData data, ProImageData imageData, Action<int> act)
    {
        //Debug.Log($"[SetProCard] {uid}, {data.name}");
        ProUid = data.uid;

        proInfoData = data;

        StartCoroutine(GameManager.Instance.LoadImageCoroutine($"{INI.proImagePath}{ProUid}/{imageData.path}", (Sprite sprite) => {
            if (sprite != null)
                imgProImage.sprite = sprite;
            else
                Debug.Log("������ Sprite �ε� ����");
        }));

        int rank = Utillity.Instance.GetPopularityRank(ProUid);

        if (rank == 1)
        {
            badgeObj.SetActive(true);
            badgeImage.sprite = badgeSprites[0];
        }
        else if (rank == 2)
        {
            badgeObj.SetActive(true);
            badgeImage.sprite = badgeSprites[1];
        }
        else if (rank == 3)
        {
            badgeObj.SetActive(true);
            badgeImage.sprite = badgeSprites[2];
        }
        else if (rank > 0)
        {
            badgeObj.SetActive(false);
        }
        else
        {
            badgeObj.SetActive(false);
            Debug.Log($"�ش� uid ����");
        }

        txtProName.text = data.name;
        txtProInfo.text = data.info;
        actProInfo = act;

        for (int i = 0; i < filters.Length; i++)
        {
            filters[i].UpdateStringAndSize(Utillity.Instance.ConvertEnumToString(data.filters[i]));
        }
    }

    // ��ư�� Ŭ������ �� ȣ��Ǵ� �Լ�
    public void OnClick_ProSelect()
    {
        if (actProInfo != null)
        {
            actProInfo.Invoke(ProUid);
        }
    }
}
