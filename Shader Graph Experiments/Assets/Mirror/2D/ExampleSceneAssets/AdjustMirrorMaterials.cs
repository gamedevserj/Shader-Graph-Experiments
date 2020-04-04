using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustMirrorMaterials : MonoBehaviour
{

    public Camera cam;
    public Renderer[] mirrorRenderers;

    Transform[] transforms;
    // Start is called before the first frame update
    void Start()
    {
        transforms = new Transform[mirrorRenderers.Length];
        for (int i = 0; i < transforms.Length; i++)
        {
            transforms[i] = mirrorRenderers[i].GetComponent<Transform>();
        }
    }

    void LateUpdate()
    {
        for (int i = 0; i < mirrorRenderers.Length; i++)
        {
            mirrorRenderers[i].material.SetFloat("_ObjectPositionX", transforms[i].position.x);
            mirrorRenderers[i].material.SetFloat("_CameraOrthoSize", cam.orthographicSize);
        }
    }
}
