using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UISpriteAnimator : MonoBehaviour
{
    Image renderImage;
    [SerializeField] Sprite[] sprites;
    [SerializeField] float interval = 0.1f;
    [SerializeField] bool loop = false;
    [SerializeField] bool AutoStart = true;

    private void Awake()
    {
        renderImage = GetComponent<Image>();
        renderImage.sprite = sprites[0];
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(AutoStart)
            StartCoroutine(CoAnimator());
    }

    public void Play()
    {
        StartCoroutine(CoAnimator());
    }

    public void Stop()
    {
        StopAllCoroutines();
    }

    IEnumerator CoAnimator()
    {
        int index = 0;
        do
        {
            renderImage.sprite = sprites[index];
            index++;
            yield return new WaitForSeconds(interval);

            if(index >= sprites.Length)
            {
                if (loop)
                    index = 0;
                else
                    yield break;
            }

        } while (true);
    }
}
