using Unity.VisualScripting;
using UnityEngine;

public class SensorSide : MonoBehaviour
{
    [SerializeField] webcamclient client;

    //Side
    KalmanFilter _iGetWaistSideDir = new KalmanFilter();
    public int iGetWaistSideDir;

    KalmanFilter _iGetHandSideDir = new KalmanFilter();
    public int iGetHandSideDir;

    KalmanFilter _iGetKneeSideDir = new KalmanFilter();
    public int iGetKneeSideDir;

    KalmanFilter _iGetElbowSideDir = new KalmanFilter();
    public int iGetElbowSideDir;

    KalmanFilter _iGetArmpitDir = new KalmanFilter();
    public int iGetArmpitDir;


    // Update is called once per frame
    void Update()
    {
        GetWaistSideDir();

        GetForearmAngle();

        GetKneeSideDir();

        GetElbowSideDir();

        GetArmpitDir();
    }

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
    void GetForearmAngle()
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

    void GetKneeSideDir()
    {
         float angle = CalculateVectorAngle(client.poseData2["landmark_24"].Position, client.poseData2["landmark_26"].Position, client.poseData2["landmark_28"].Position);
        iGetKneeSideDir = (int)_iGetKneeSideDir.Update(angle);
    }

    void GetElbowSideDir()
    {
        float angle = CalculateVectorAngle(client.poseData2["landmark_12"].Position, client.poseData2["landmark_14"].Position, client.poseData2["landmark_16"].Position);
        iGetElbowSideDir = (int)_iGetElbowSideDir.Update(angle);
    }

    void GetArmpitDir()
    {
        float angle = CalculateVectorAngle(client.poseData2["landmark_24"].Position, client.poseData2["landmark_12"].Position, client.poseData2["landmark_14"].Position);
        iGetArmpitDir = (int)_iGetArmpitDir.Update(angle);
    }

    public float CalculateVectorAngle(Vector2 v1, Vector2 v2, Vector2 v3)
    {
        Vector2 vec1 = v1 - v2;
        Vector3 vec2 = v3 - v2;

        return Vector2.Angle(vec1.normalized, vec2.normalized);
    }
}
