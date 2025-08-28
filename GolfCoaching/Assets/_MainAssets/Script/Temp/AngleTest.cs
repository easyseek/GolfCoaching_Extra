using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using static RootMotion.FinalIK.GrounderQuadruped;

public class AngleTest : MonoBehaviour
{
    [SerializeField] Transform eyeL;
    [SerializeField] Transform eyeR;
    [SerializeField] Transform nose;
    //[SerializeField] Transform RootC;    
    //[SerializeField] Transform RootCp;
    //[SerializeField] Transform TestObj;
    [SerializeField] TextMeshProUGUI debug;

    // Update is called once per frame
    void Update()
    {
        GetValue();
    }

    public void GetValue2()
    {
        Vector3 eyeCenter = (eyeL.position + eyeR.position) / 2.0f;
        eyeCenter.y = 0;

        Vector3 projectedNosePosition = new Vector3(nose.position.x, 0, nose.position.z);

        Vector3 horizontalFaceDirection = projectedNosePosition - eyeCenter;

        debug.text = $"{Vector3.SignedAngle(Vector3.forward, horizontalFaceDirection, Vector3.up)}";
    }

    public void GetValue()
    {
        Vector3 eyeCenter = (eyeL.position + eyeR.position) / 2f;
        Vector3 eyeRight = (eyeR.position - eyeL.position).normalized;

        Vector3 headForward = Vector3.Cross(Vector3.up, eyeRight).normalized;

        float yawAngle = Vector3.SignedAngle(Vector3.forward, headForward, Vector3.up);
    }
}
