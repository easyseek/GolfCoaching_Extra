using Enums;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SensorViewerSide : MonoBehaviour
{
    [SerializeField] SensorProcess sensor;
    [SerializeField] TextMeshProUGUI txtViewer;
    string snap;
    string get;

    bool isPause = false;

    int iGetWaistSideDir;
    int iGetHandSideDir;
    int iGetKneeSideDir;
    int iGetElbowSideDir;
    int iGetArmpitDir;
    int iGetShoulderDir;
    int iGetPelvisDir;

    [SerializeField] TMP_InputField gapDegree;
    public ProSwingStepData swingStepData;
    
    private void Awake()
    {
        if(txtViewer == null)
            txtViewer = GetComponent<TextMeshProUGUI>();
    }


    // Update is called once per frame
    void Update()
    {
        if (isPause)
            return;

        get = Comapare("iGetWaistSideDir", sensor.iGetWaistSideDir, iGetWaistSideDir);
        get += Comapare("iGetHandSideDir", sensor.iGetHandSideDir, iGetHandSideDir);
        get += Comapare("iGetKneeSideDir", sensor.iGetKneeSideDir, iGetKneeSideDir);
        get += Comapare("iGetElbowSideDir", sensor.iGetElbowSideDir, iGetElbowSideDir);
        get += Comapare("iGetArmpitDir", sensor.iGetArmpitDir, iGetArmpitDir);
        get += $"\r\n";
        get += Comapare("iGetShoulderDir", sensor.iGetShoulderDir, iGetShoulderDir);
        get += Comapare("iGetPelvisDir", sensor.iGetPelvisDir, iGetPelvisDir);        

        if (Input.GetKeyDown(KeyCode.Space))
        {
            iGetWaistSideDir = sensor.iGetWaistSideDir;
            iGetHandSideDir = sensor.iGetHandSideDir;
            iGetKneeSideDir = sensor.iGetKneeSideDir;
            iGetElbowSideDir = sensor.iGetElbowSideDir;
            iGetArmpitDir = sensor.iGetArmpitDir;
            iGetShoulderDir = sensor.iGetShoulderDir;
            iGetPelvisDir = sensor.iGetPelvisDir;
        }
        else if (Input.GetKeyDown(KeyCode.Delete))
        {
            iGetWaistSideDir = -1;
            iGetHandSideDir = -1;
            iGetKneeSideDir = -1;
            iGetElbowSideDir = -1;
            iGetArmpitDir = -1;
            iGetShoulderDir = -1;
            iGetPelvisDir = -1;
        }
        else if (Input.GetKeyDown(KeyCode.P))
        {
            isPause = !isPause;
        }



        txtViewer.text = get;
    }

    public void SetGetProValue(SWINGSTEP step)
    {
        Dictionary<string, int> dicStep = new Dictionary<string, int>();
        if (step == SWINGSTEP.ADDRESS)
            dicStep = swingStepData.dicAddress;
        else if (step == SWINGSTEP.TAKEBACK)
            dicStep = swingStepData.dicTakeback;
        else if (step == SWINGSTEP.BACKSWING)
            dicStep = swingStepData.dicBackswing;
        else if (step == SWINGSTEP.TOP)
            dicStep = swingStepData.dicTop;
        else if (step == SWINGSTEP.DOWNSWING)
            dicStep = swingStepData.dicDownswing;
        else if (step == SWINGSTEP.IMPACT)
            dicStep = swingStepData.dicImpact;
        else if (step == SWINGSTEP.FOLLOW)
            dicStep = swingStepData.dicFollow;
        else if (step == SWINGSTEP.FINISH)
            dicStep = swingStepData.dicFinish;
        else
            dicStep = swingStepData.dicAddress;

        iGetWaistSideDir = dicStep["GetWaistSideDir"];
        iGetHandSideDir = dicStep["GetHandSideDir"];
        iGetKneeSideDir = dicStep["GetKneeSideDir"];
        iGetElbowSideDir = dicStep["GetElbowSideDir"];
        iGetArmpitDir = dicStep["GetArmpitDir"];
        iGetShoulderDir = dicStep["GetShoulderDir"];
        iGetPelvisDir = dicStep["GetPelvisDir"];
    }

    string Comapare(string title, int org, int post)
    {

        try
        {
            int val = int.Parse(gapDegree.text);
            string postCol = "";
            if ((org <= post + val) && (org >= post - val))
                postCol = "green";
            else
                postCol = "red";
            return $"<color={postCol}>{org} / {post} : {title}</color>\r\n";
        }
        catch { return title; }
    }
}
