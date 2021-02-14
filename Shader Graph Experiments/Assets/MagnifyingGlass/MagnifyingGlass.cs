using UnityEngine;

public class MagnifyingGlass : MonoBehaviour
{

    public string offsetX = "_OffsetX";
    public string offsetY = "_OffsetY";

    public Camera cam;
    
    
    [Range(0, 0.9f)]
    public float zoom = 0.1f;

    Transform cameraTrans;
    Transform trans;
    Renderer rend;
    float screenAspect;

    float xOffset;
    float yOffset;
    float zoomOrthoAdjusted; // zoom adjusted to orthographic size
    float zoomOrhtoAspectAdjusted;// zoom adjusetd to orthographic size and screen aspect ratio

    void Start()
    {
        screenAspect = (float)Screen.width / Screen.height;
        rend = GetComponent<Renderer>();
        trans = GetComponent<Transform>();
        cameraTrans = cam.GetComponent<Transform>();
    }

    void Update()
    {
        //moving the object with mouse
        Vector3 pos = cam.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0));
        trans.position = new Vector3(pos.x, pos.y, 0);
        //zoom with scroll wheel
        zoom = Mathf.Clamp(zoom += Input.mouseScrollDelta.y * 0.1f, 0, 0.9f);

        Calcualte();
    }

    /*
     * zoom value is subtracted from tiling property
     * to center image back we take half of zoom value and assign to offset property
     * zoomOrthoAdjusted and zoomOrhtoAspectAdjusted adjusts offset values to account for aspect ratio and camera's orthographic size
     * */
    void Calcualte()
    {
        float halfZoom = zoom * 0.5f; 

        zoomOrhtoAspectAdjusted = (halfZoom / cam.orthographicSize) / screenAspect;
        zoomOrthoAdjusted = (halfZoom / cam.orthographicSize);
        // calculate offset on X axis based on object and camera positions, orthographic camera size, and aspect ratio
        xOffset = halfZoom + zoomOrhtoAspectAdjusted * trans.position.x - zoomOrhtoAspectAdjusted * cameraTrans.position.x;
        // calculate offset on Y axis based on object and camera positions, and orthographic camera size
        yOffset = halfZoom + zoomOrthoAdjusted * trans.position.y - zoomOrthoAdjusted * cameraTrans.position.y;

        rend.material.SetVector("_Tiling", new Vector4(1 - zoom, 1 - zoom, 0, 0));
        rend.material.SetVector("_Offset", new Vector4(xOffset, yOffset, 0, 0));
    }
}
