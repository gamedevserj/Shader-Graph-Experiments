using UnityEngine;
using UnityEngine.UI;

public class FadeOutBehaviour : MonoBehaviour
{
    private Image rend;

    private float aspect = 1;
    private float offsetX = 0.5f;
    private float offsetY = 0.5f;
    private float tilingX = 1;
    private float tilingY = 1;
    private float hypotenuse = 1;
    
    private float fadeAmount = 0;
    
    private float scrollScale = 0.02f;
    private Vector3 targetPosition = new Vector3(0.5f, 0.5f);

    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        TrackObject();
    }

    void TrackObject()
    {
        fadeAmount += Input.mouseScrollDelta.y * scrollScale;
        fadeAmount = Mathf.Clamp01(fadeAmount);
        Aspect();
        NormalizedPosition();
        Hypotenuse();
        SetMaterialProperties();
    }

    void NormalizedPosition()
    {
        offsetX = Input.mousePosition.x / Screen.width;
        offsetY = Input.mousePosition.y / Screen.height;
        if(Screen.width > Screen.height)
        {
            offsetX *= aspect;
        }
        else
        {
            offsetY *= aspect;
        }
    }

    void Aspect()
    {
        if(Screen.width > Screen.height)
        {
            aspect = (float)Screen.width / Screen.height;
            tilingX = aspect;
            tilingY = 1;
        }
        else
        {
            aspect = (float)Screen.height / Screen.width;
            tilingY = aspect;
            tilingX = 1;
        }
    }

    void Hypotenuse()
    {
        float x = offsetX;
        float y = offsetY;

        float midX = 0.5f;
        float midY = 0.5f;

        if (Screen.width > Screen.height)
            midX *= aspect;
        else
            midY *= aspect;
        
        // determines which quadrant of the screen the point is in
        if (x < midX)
            x = midX*2  - x;
        if (y < midY)
            y = midY*2  - y;

        hypotenuse = Mathf.Sqrt(x * x + y * y);
    }

    void SetMaterialProperties()
    {
        rend.material.SetVector("_Offset", new Vector4(offsetX, offsetY, 0, 0));
        rend.material.SetVector("_Tiling", new Vector4(tilingX, tilingY, 0, 0));
        rend.material.SetFloat("_Hypotenuse", hypotenuse);
        rend.material.SetFloat("_FadeAmount", fadeAmount);
    }

}
