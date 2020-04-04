using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustWaterMaterials : MonoBehaviour
{

    public Camera cam;
    public Renderer[] waterRenderers;

    Transform[] transforms;
    // Start is called before the first frame update
    void Start()
    {
        transforms = new Transform[waterRenderers.Length];
        for (int i = 0; i < transforms.Length; i++)
        {
            transforms[i] = waterRenderers[i].GetComponent<Transform>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < waterRenderers.Length; i++)
        {
            waterRenderers[i].material.SetFloat("_ObjectPositionY", transforms[i].position.y);
            waterRenderers[i].material.SetFloat("_CameraOrthoSize", cam.orthographicSize);
        }
    }
}
