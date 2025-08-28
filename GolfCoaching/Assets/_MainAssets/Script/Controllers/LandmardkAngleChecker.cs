using UnityEngine;

public class LandmardkAngleChecker : MonoBehaviour
{
    [SerializeField] Transform LeftHand;
    [SerializeField] Transform RightHand;

    [SerializeField] Transform LeftPelvis;
    [SerializeField] Transform RightPelvis;

    [SerializeField] Transform LeftShoulder;
    [SerializeField] Transform RightShoulder;

    [SerializeField] Transform LeftFoot;
    [SerializeField] Transform RightFoot;

    [SerializeField] Transform Head;

    //� ȸ��
    public float GetShoulderDir()
    {
        // ����� �߾��� �������� �ϴ� ���� ���
        Vector3 PelvisVector = RightPelvis.position - LeftPelvis.position;
        Vector3 shoulderVector = RightShoulder.position - LeftShoulder.position;

        // ��� ���Ϳ� ��� ���� ���� ȸ�� ���� ���
        float angle = Vector3.SignedAngle(PelvisVector, shoulderVector, Vector3.forward);
        //calKinetic.ShoulderValue = angle / 180f;
        return angle / 180f;
    }

    //��� ȸ��
    public float GetPelvisDir()
    {
        // ����� �߾��� �������� �ϴ� ���� ���
        Vector3 FootVector = RightFoot.position - LeftFoot.position;
        Vector3 PelvisVector = RightPelvis.position - LeftPelvis.position;

        // ��� ���Ϳ� ��� ���� ���� ȸ�� ���� ���
        float angle = Vector3.SignedAngle(FootVector, PelvisVector, Vector3.forward);
        //calKinetic.PelvisValue = angle / 180f;
        return angle / 180f;
    }

    //���� ȸ����
    public float GetHandDir()
    {
        // ����߽ɰ� ���߽��� ����
        Vector3 shoulderVector = (RightShoulder.position + LeftShoulder.position) / 2;
        Vector3 handVector = (RightHand.position + LeftHand.position) / 2;
        Vector3 dir = handVector - shoulderVector;
        dir.z = 0;

        // ��� ���Ϳ� �� ���� ���� ���� ���
        return Quaternion.FromToRotation(Vector3.up, dir).eulerAngles.z;
    }
}
