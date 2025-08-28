using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class CaptureRenderTexture : MonoBehaviour
{
    // Inspector���� �������� RawImage
    public RawImage rawImage;

    // ĸó �� ����� ���� �̸� (���Ͻô� ��� ���� ����)
    public string fileName = "CapturedImage.png";

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.P))
        {
            CaptureAndSaveImage();
        }
    }

    /// <summary>
    /// RenderTexture -> Texture2D -> PNG�� ��ȯ�Ͽ� ���Ϸ� �����ϴ� �Լ�
    /// </summary>
    public void CaptureAndSaveImage()
    {
        // RawImage�� �����ְ� �ִ� Texture�� RenderTexture��� ����
        RenderTexture renderTex = rawImage.texture as RenderTexture;
        if (renderTex == null)
        {
            Debug.LogError("RawImage�� ����� Texture�� RenderTexture�� �ƴմϴ�.");
            return;
        }

        // ���� Ȱ��ȭ�Ǿ� �ִ� RenderTexture ���
        RenderTexture currentRT = RenderTexture.active;

        // ĸó�� RenderTexture�� Ȱ��ȭ
        RenderTexture.active = renderTex;

        // RenderTexture�� ũ�⿡ ���� Texture2D ����
        Texture2D captureTexture = new Texture2D(
            renderTex.width,
            renderTex.height,
            TextureFormat.RGB24,
            false
        );

        // RenderTexture�� �ȼ��� �о Texture2D�� ����
        captureTexture.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        captureTexture.Apply();

        // �ٽ� ���� RenderTexture�� ����
        RenderTexture.active = currentRT;

        // Texture2D�� PNG �������� ��ȯ
        byte[] pngData = captureTexture.EncodeToPNG();

        // ���Ϸ� ������ ��� ����
        // ��: Application.persistentDataPath, Application.dataPath �� ���ϴ� ��� ��� ����
        string savePath = Path.Combine(Application.persistentDataPath, fileName);

        // PNG ������ ���Ϸ� ����
        File.WriteAllBytes(savePath, pngData);

        // ����� Ȯ��
        Debug.Log($"RenderTexture ĸó �Ϸ�! ���� ���: {savePath}");
    }
}
