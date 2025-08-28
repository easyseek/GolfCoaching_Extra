using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SensorProcess : MonoBehaviour
{
    [SerializeField] webcamclient client;
    public float visibilityFront;
    public float visibilitySide;

    // front
    KalmanFilter _iGetHandDir = new KalmanFilter();
    [HideInInspector] public int iGetHandDir; //���� 0~360
    [HideInInspector] public int iGetHandDirNF; //���� 0~360 (���;���)

    KalmanFilter _iGetHandDistance = new KalmanFilter();
    [HideInInspector] public int iGetHandDistance;

    KalmanFilter _iGetShoulderDistance = new KalmanFilter();
    [HideInInspector] public int iGetShoulderDistance;

    KalmanFilter _iGetSpineDir = new KalmanFilter();
    [HideInInspector] public int iGetSpineDir;

    KalmanFilter _iGetShoulderAngle = new KalmanFilter();
    [HideInInspector] public int iGetShoulderAngle;

    KalmanFilter _iGetWeight = new KalmanFilter();
    [HideInInspector] public int iGetWeight;
    [HideInInspector] public float fGetPelvisDir;

    KalmanFilter _iGetFootDisRate = new KalmanFilter();
    [HideInInspector] public int iGetFootDisRate;

    KalmanFilter _iGetForearmAngle = new KalmanFilter();
    [HideInInspector] public int iGetForearmAngle; //����

    KalmanFilter _iGetElbowFrontDir = new KalmanFilter();
    [HideInInspector] public int iGetElbowFrontDir;

    KalmanFilter _iGetElbowRightFrontDir = new KalmanFilter();
    [HideInInspector] public int iGetElbowRightFrontDir;

    Vector2 handVector;


    //Side
    KalmanFilter _iGetWaistSideDir = new KalmanFilter();
    [HideInInspector] public int iGetWaistSideDir;

    KalmanFilter _iGetHandSideDir = new KalmanFilter();
    [HideInInspector] public int iGetHandSideDir;

    KalmanFilter _iGetKneeSideDir = new KalmanFilter();
    [HideInInspector] public int iGetKneeSideDir;

    KalmanFilter _iGetElbowSideDir = new KalmanFilter();
    [HideInInspector] public int iGetElbowSideDir;

    KalmanFilter _iGetArmpitDir = new KalmanFilter();
    [HideInInspector] public int iGetArmpitDir;

    //Combine
    KalmanFilter _iGetShoulderDir = new KalmanFilter();
    [HideInInspector] public int iGetShoulderDir;

    KalmanFilter _iGetPelvisDir = new KalmanFilter();
    [HideInInspector] public int iGetPelvisDir;

    KalmanFilter[] _vRightElbowDir = new KalmanFilter[3];
    [HideInInspector] public Vector3 vRightElbowDir;

    //options
    [HideInInspector] public float fLeftElbowSideVis;
    [HideInInspector] public float fRightElbowFrontVis;
    bool bNormal = false;
    public bool Normal { get { return bNormal; } }
    Vector2 CheckAdressCenterShoulder = Vector2.zero;
    [HideInInspector] public float DistanceAdressCenterShoulder = 0;

    [SerializeField] float frontLenth = 1.25f;  //����ī�޶�� �Ÿ� m
    [SerializeField] float sideLenth = 1.4f;    //����ī�޶�� �Ÿ� m
    float cmbScale = 1f;

    [SerializeField] TextMeshProUGUI txtDebug;

    public bool handVectorIsLeft = true;

    private void Awake()
    {
        cmbScale = sideLenth / frontLenth;
        for (int i = 0; i < 3; i++) _vRightElbowDir[i] = new KalmanFilter();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            client.StopPipeClient();
            Application.Quit();
        }

        visibilityFront = client.visibilityFront;
        visibilitySide = client.visibilitySide;

        /*  Front  */
        GetHandDistance();  //�� �ո� ���� �Ÿ� (ī�޶�� �Ÿ��� ���� �����)
        GetShoulderDistance();  //�� ��� ���� �Ÿ� (ī�޶�� �Ÿ��� ���� �����)

        GetHandPosition();  //�������� ����� ���� ���� �� ��ǥ
        GetHandDir();   //�������� 0~360��, �齺������ ������ �پ��� �ȷο����� �����Ѵ�. ��巹�� �� 180��
        GetSpineDir();  //�㸮 ���� ����. ����0��~������180�� �㸮�� �����϶� 90��
        GetShoulderAngle(); //���ʱ��� ������ ��� ���� �Ʒ��� �ִ� 0��~ ���� �ִ� 180��. ��� ������ 90��
        GetWeight();    //���� ġ��ħ ���� �� -30 ~ 30��
        GetFootDisRate();   //��� ���� ��� �ٸ� ���� �����
        GetForearmAngle();  //���� ������� �����Ȳ�ġ���� ����
        GetElbowFrontDir();  //�� ���� �Ȳ�ġ ���� ����
        GetElbowRightFrontDir();  //�������� �Ȳ�ġ ���� ����(����)

        /*  Side  */
        GetWaistSideDir();  //�㸮 ���� ���� �ٷ� ������ 90��
        GetHandSideDir();   //���� ������� �����Ȳ�ġ ����
        GetKneeSideDir();   //������ ���� ���� ����
        GetElbowSideDir();  //������ ���� �Ȳ�ġ ���� ����(����)
        GetArmpitDir(); //������ �㸮������ �������� �Ȳ�ġ ����

        /*  Combine  */
        GetShoulderDir();   //��� ȸ������ (��/���� ������ �� �Ÿ��� ���� ���� ġ)
        GetPelvisDir(); //��� ȸ������ (��/���� ������ �� �Ÿ��� ���� ���� ġ)

        GetRightElbowDir();

        /*  Option */
        try {
            fLeftElbowSideVis = client.poseData2["landmark_13"].visibility;
            fRightElbowFrontVis = client.poseData1["landmark_14"].visibility;

            CheckNormal();

        } catch { fLeftElbowSideVis = 0; }        

    }





    //=========================================================
    // ���� poseData1 �� ���
    //=========================================================

    //�� �ո� ���� �Ÿ� (ī�޶�� �Ÿ��� ���� �����)
    void GetHandDistance()
    {
        try
        {
            iGetHandDistance = (int)(_iGetHandDistance.Update(Vector2.Distance(client.poseData1["landmark_15"].Position,
                client.poseData1["landmark_16"].Position)));

            iGetHandDistance = (int)(iGetHandDistance * Utillity.Instance.frontPixelDistanceRate);
        }
        catch { iGetHandDistance = -1; }
    }

    public bool IsAddressHand()
    {
        return iGetHandDistance < Utillity.Instance.addresssHandDis ? true : false;
    }

    //�� ��� ���� �Ÿ� (ī�޶�� �Ÿ��� ���� �����)
    void GetShoulderDistance()
    {
        try
        {
            iGetShoulderDistance = (int)(_iGetShoulderDistance.Update(Vector2.Distance(client.poseData1["landmark_11"].Position,
                client.poseData1["landmark_12"].Position)));
            
            iGetShoulderDistance = (int)(iGetShoulderDistance * Utillity.Instance.frontPixelDistanceRate);
        }
        catch { iGetShoulderDistance = -1; }
    }

    //�������� ����� ���� ���� �� ��ǥ
    void GetHandPosition()
    {
        try
        {
            //if (client.poseData1["landmark_12"].Position.y > client.poseData1["landmark_11"].Position.y)            
            //if (client.poseData2["landmark_12"].Position.x > client.poseData2["landmark_11"].Position.x)
            //���� ī�޶� �������� ���ʿ� �ִ� �Ȳ�ġ�� �ִ� ������ ���� ����
            if (client.poseData2["landmark_14"].Position.x > client.poseData2["landmark_13"].Position.x)
            {
                handVectorIsLeft = false;
                handVector = client.poseData1["landmark_16"].Position;
            }
            else
            {
                handVectorIsLeft = true;
                handVector = client.poseData1["landmark_15"].Position;
            }

            /*handVector = Vector2.Lerp(client.poseData1["landmark_15"].Position, client.poseData1["landmark_16"].Position
                    , client.poseData1["landmark_16"].visibility);
            */
        }
        catch { handVector = Vector2.zero; }
    }

    //�������� 0~360��, �齺������ ������ �پ��� �ȷο����� �����Ѵ�. ��巹�� �� 180��
    void GetHandDir()
    {
        try
        {
            // ����߽ɰ� ���߽��� ����
            Vector2 shoulderVector = (client.poseData1["landmark_11"].Position + client.poseData1["landmark_12"].Position) / 2;
            Vector2 dir = handVector - shoulderVector;

            float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
            angle += 180f;
            iGetHandDirNF = (int)angle;
            iGetHandDir = (int)_iGetHandDir.Update(angle);
        }
        catch { iGetHandDir = -1; }
    }

    //�㸮 ���� ����. ����0��~������180�� �㸮�� �����϶� 90��
    void GetSpineDir()
    {
        try
        {
            // ����߽ɰ� ����߽�
            Vector2 pelvisVector = (client.poseData1["landmark_23"].Position + client.poseData1["landmark_24"].Position) / 2;
            Vector2 shoulderVector = (client.poseData1["landmark_11"].Position + client.poseData1["landmark_12"].Position) / 2;
            Vector2 dir = shoulderVector - pelvisVector;

            float angle = -Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            //angle += 180f;
            iGetSpineDir = (int)_iGetSpineDir.Update(angle);
        }
        catch { iGetSpineDir = -1; }
    }

    //���ʱ��� ������ ��� ���� �Ʒ��� �ִ� 0��~ ���� �ִ� 180��. ��� ������ 90��
    void GetShoulderAngle()
    {
        try
        {
            // ���� ��ƿ��� �����ʾ���� ����
            Vector2 dir = client.poseData1["landmark_12"].Position - client.poseData1["landmark_11"].Position;

            float angle = -Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
            //angle += 180f;
            iGetShoulderAngle = (int)_iGetShoulderAngle.Update(angle);
        }
        catch { iGetShoulderAngle = -1; }
    }

    //���� ġ��ħ ���� �� -30 ~ 30��
    void GetWeight()
    {
        try
        {
            Vector2 footCenter = (client.poseData1["landmark_27"].Position + client.poseData1["landmark_28"].Position) / 2;
            Vector2 pelvisCenter = (client.poseData1["landmark_23"].Position + client.poseData1["landmark_24"].Position) / 2;

            Vector2 dir = footCenter - pelvisCenter;

            dir.Normalize();

            fGetPelvisDir = _iGetWeight.Update(dir.x);
            iGetWeight = (int)(fGetPelvisDir * 100f);
        }
        catch { iGetWeight = -1; }
    }


    //��� ���� ��� �ٸ� ���� �����
    void GetFootDisRate()
    {
        try
        {
            float footDis = Vector2.Distance(client.poseData1["landmark_27"].Position, client.poseData1["landmark_28"].Position);
            footDis = footDis * Utillity.Instance.frontPixelDistanceRate;
            iGetFootDisRate = (int)_iGetFootDisRate.Update((footDis / iGetShoulderDistance) * 100f);
        }
        catch { iGetFootDisRate = -1; }
    }

    //���� ������� �����Ȳ�ġ���� ����
    void GetForearmAngle()
    {
        try
        {
            Vector2 dir = client.poseData1["landmark_14"].Position - client.poseData1["landmark_12"].Position;
            float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
            angle += 180f;
            iGetForearmAngle = (int)_iGetForearmAngle.Update(angle);
        }
        catch { iGetForearmAngle = -1; }
    }

    //���� ���� �Ȳ�ġ ���� ����
    void GetElbowFrontDir()
    {
        try
        {
            float angle = CalculateVectorAngle(client.poseData1["landmark_11"].Position, client.poseData1["landmark_13"].Position, client.poseData1["landmark_15"].Position);
            iGetElbowFrontDir = (int)_iGetElbowFrontDir.Update(angle);
        }
        catch { iGetElbowFrontDir = -1; }
    }

    //�������� �Ȳ�ġ ���� ����
    void GetElbowRightFrontDir()
    {
        try
        {
            float angle = CalculateVectorAngle(client.poseData1["landmark_12"].Position, client.poseData1["landmark_14"].Position, client.poseData1["landmark_16"].Position);
            iGetElbowRightFrontDir = (int)_iGetElbowRightFrontDir.Update(angle);
        }
        catch { iGetElbowRightFrontDir = -1; }
    }







    //=========================================================
    // ���� poseData2 �� ���
    //=========================================================

    //�㸮 ���� ���� �ٷ� ������ 90��
    void GetWaistSideDir()
    {
        try
        {
            //���� ��ݿ��� ���� ��Ʒ� ���� ����
            Vector2 dir = client.poseData2["landmark_12"].Position - client.poseData2["landmark_24"].Position;

            float angle = -Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            //angle += 180f;
            iGetWaistSideDir = (int)_iGetWaistSideDir.Update(angle);
        }
        catch { iGetWaistSideDir = -1; }
    }

    //���� ������� �����Ȳ�ġ ����
    void GetHandSideDir()
    {
        try
        {
            Vector2 dir = client.poseData2["landmark_14"].Position - client.poseData2["landmark_12"].Position;
            float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
            angle += 180f;
            iGetHandSideDir = (int)_iGetHandSideDir.Update(angle);
        }
        catch { iGetHandSideDir = -1; }
    }

    //������ ���� ���� ����
    void GetKneeSideDir()
    {
        try
        {
            float angle = CalculateVectorAngle(client.poseData2["landmark_24"].Position, client.poseData2["landmark_26"].Position, client.poseData2["landmark_28"].Position);
            iGetKneeSideDir = (int)_iGetKneeSideDir.Update(angle);
        }
        catch { iGetKneeSideDir = -1; }
    }

    //������ ���� �Ȳ�ġ ���� ����
    void GetElbowSideDir()
    {
        try
        { 
            float angle = CalculateVectorAngle(client.poseData2["landmark_12"].Position, client.poseData2["landmark_14"].Position, client.poseData2["landmark_16"].Position);
            iGetElbowSideDir = (int)_iGetElbowSideDir.Update(angle);
        }
        catch { iGetElbowSideDir = -1; }
    }

    //������ �㸮������ �������� �Ȳ�ġ ����
    void GetArmpitDir()
    {
        try
        { 
            float angle = CalculateVectorAngle(client.poseData2["landmark_24"].Position, client.poseData2["landmark_12"].Position, client.poseData2["landmark_14"].Position);
            iGetArmpitDir = (int)_iGetArmpitDir.Update(angle);
        }
        catch { iGetArmpitDir = -1; }
    }

    




    //=========================================================
    // ����, ���� poseData1, poseData2 ���� ���
    //=========================================================

    //��� ȸ������ (��/���� ������ �� �Ÿ��� ���� ���� ġ)
    void GetShoulderDir()
    {
        try
        {
            Vector2 dirf = client.poseData1["landmark_11"].Position - client.poseData1["landmark_12"].Position;
            Vector2 dirs = client.poseData2["landmark_11"].Position - client.poseData2["landmark_12"].Position;
            dirs *= cmbScale;
            
            dirf.y = dirs.x;

            float angle = Mathf.Atan2(dirf.y, dirf.x ) * Mathf.Rad2Deg;
            angle += 180f + Utillity.Instance.sideAngleOffset;
            iGetShoulderDir = (int)_iGetShoulderDir.Update(angle);
        }
        catch { iGetShoulderDir = -1; }
    }

    //��� ȸ������ (��/���� ������ �� �Ÿ��� ���� ���� ġ)
    void GetPelvisDir()
    {
        try
        {
            Vector2 dirf = client.poseData1["landmark_23"].Position - client.poseData1["landmark_24"].Position;
            Vector2 dirs = client.poseData2["landmark_23"].Position - client.poseData2["landmark_24"].Position;
            dirs *= cmbScale;

            dirf.y = dirs.x;

            float angle = Mathf.Atan2(dirf.y, dirf.x) * Mathf.Rad2Deg;
            angle += 180f + Utillity.Instance.sideAngleOffset;
            iGetPelvisDir = (int)_iGetPelvisDir.Update(angle);
        }
        catch { iGetPelvisDir = -1; }
    }


    void GetRightElbowDir()
    {
        try
        {
            Vector2 dirF = client.poseData1["landmark_14"].Position - client.poseData1["landmark_12"].Position;
            Vector2 dirS = client.poseData2["landmark_14"].Position - client.poseData2["landmark_12"].Position;
            dirF.Normalize();
            dirS.Normalize();
            //txtDebug.text = $"{dirF}\r\n{dirS}";

            vRightElbowDir = new Vector3(
                _vRightElbowDir[0].Update(dirF.x+0.05f),
                _vRightElbowDir[1].Update(-dirS.y),
                _vRightElbowDir[2].Update(-dirS.x));
        }
        catch { vRightElbowDir = Vector3.zero; }
    }




    //=========================================================
    // Util �Լ�
    //=========================================================
    float CalculateVectorAngle(Vector2 v1, Vector2 v2, Vector2 v3)
    {
        try
        {
            Vector2 vec1 = v1 - v2;
            Vector3 vec2 = v3 - v2;

            return Vector2.Angle(vec1.normalized, vec2.normalized);
        }
        catch { return -1f; }
    }

    void CheckNormal()
    {
        /*
        if(client.poseData1[$"landmark_12"].Position.x < 120f || client.poseData1[$"landmark_11"].Position.x > 600f)
            bNormal = false;
        else if (client.poseData1[$"landmark_12"].Position.y < 400f || client.poseData1[$"landmark_12"].Position.y > 600f)
            bNormal = false;
        else if (client.poseData1[$"landmark_11"].Position.y < 400f || client.poseData1[$"landmark_11"].Position.y > 600f)
            bNormal = false;
        else
            bNormal = true;
        */

        if (CheckAdressCenterShoulder == Vector2.zero)
        { 
            bNormal = false;
            DistanceAdressCenterShoulder = -1f;
            return;
        }
        else
        {
            Vector2 center = (client.poseData1["landmark_11"].Position + client.poseData1["landmark_12"].Position) / 2;
            DistanceAdressCenterShoulder = Vector2.Distance(CheckAdressCenterShoulder, center);            
        }
    }

    public void SetAdressCenterShoulder(bool reset = false)
    {
        if(reset)
        {
            CheckAdressCenterShoulder = Vector2.zero;
            return;
        }

        CheckAdressCenterShoulder = (client.poseData1["landmark_11"].Position + client.poseData1["landmark_12"].Position) / 2;
    }

    public Vector2 GetLandmarkPosition(bool isFront, int markNum)
    {
        try
        {
            if (isFront)
                return client.poseData1[$"landmark_{markNum}"].Position;
            else
                return client.poseData2[$"landmark_{markNum}"].Position;
        }
        catch
        {
            return Vector2.zero;
        }
    }
}
