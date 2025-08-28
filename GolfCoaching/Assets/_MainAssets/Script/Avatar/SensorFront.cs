using UnityEngine;

public class SensorFront : MonoBehaviour
{
    [SerializeField] webcamclient client;

    // front
    KalmanFilter _iGetHandDir = new KalmanFilter();
    public int iGetHandDir; //���� 0~360

    KalmanFilter _iGetHandDistance = new KalmanFilter();
    public int iGetHandDistance;

    KalmanFilter _iGetShoulderDistance = new KalmanFilter();
    public int iGetShoulderDistance;    

    KalmanFilter _iGetSpineDir = new KalmanFilter();
    public int iGetSpineDir;

    KalmanFilter _iGetShoulderAngle = new KalmanFilter();
    public int iGetShoulderAngle;

    KalmanFilter _iGetWeight = new KalmanFilter();
    public int iGetWeight;

    KalmanFilter _iGetFootDisRate = new KalmanFilter();
    public int iGetFootDisRate;

    KalmanFilter _iGetForearmAngle = new KalmanFilter();
    public int iGetForearmAngle; //����

    public int _iGetShoulderDir_Other; // >> ���̵忡�� üũ?
    public int _iGetPelvisDir_Other;   // >> ���̵忡�� üũ?

    Vector2 handVector;

    
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            client.StopPipeClient();
            Application.Quit();
        }

        GetHandDistance();
        GetShoulderDistance();

        GetHandPosition();

        GetHandDir();

        GetSpineDir();

        GetShoulderAngle();

        GetWeight();

        GetFootDisRate();

        GetForearmAngle();
    }


    //�� �ո� ���� �Ÿ� (ī�޶�� �Ÿ��� ���� �����)
    void GetHandDistance()
    {
        try
        {
            iGetHandDistance = (int)(_iGetHandDistance.Update(Vector2.Distance(client.poseData1["landmark_15"].Position,
                client.poseData1["landmark_16"].Position)));
        }
        catch { iGetHandDistance = -1; }
    }

    //�� ��� ���� �Ÿ� (ī�޶�� �Ÿ��� ���� �����)
    void GetShoulderDistance()
    {
        try
        {
            iGetShoulderDistance = (int)(_iGetShoulderDistance.Update(Vector2.Distance(client.poseData1["landmark_11"].Position,
                client.poseData1["landmark_12"].Position)));
        }
        catch { iGetShoulderDistance = -1; }
    }

    //�������� ����� ���� ���� �� ��ǥ
    void GetHandPosition()
    {
        try
        {
            handVector = Vector2.Lerp(client.poseData1["landmark_15"].Position, client.poseData1["landmark_16"].Position
                , client.poseData1["landmark_16"].visibility);
        }
        catch { handVector = Vector2.zero;  }
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

            iGetWeight = (int)(_iGetWeight.Update( dir.x * 100f));
        }
        catch { iGetWeight = -1; }
    }


    //��� ���� ��� �ٸ� ���� �����
    void GetFootDisRate()
    {
        try
        {
            float footDis = Vector2.Distance(client.poseData1["landmark_27"].Position, client.poseData1["landmark_28"].Position);

            iGetFootDisRate = (int)_iGetFootDisRate.Update((footDis / iGetShoulderDistance) * 100f);
        }
        catch { iGetFootDisRate = -1; }
    }

    //���� ������� �����Ȳ�ġ ����
    void GetForearmAngle()
    {
        try { 
            Vector2 dir = client.poseData1["landmark_14"].Position - client.poseData1["landmark_12"].Position;
            float angle = Mathf.Atan2(dir.x, dir.y) * Mathf.Rad2Deg;
            angle += 180f;
            iGetForearmAngle = (int)_iGetForearmAngle.Update(angle);
        }
        catch { iGetForearmAngle = -1; }
    }
}
