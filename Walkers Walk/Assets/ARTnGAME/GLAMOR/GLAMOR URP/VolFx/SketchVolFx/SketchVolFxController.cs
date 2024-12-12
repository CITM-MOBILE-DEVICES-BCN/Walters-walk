using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.GLAMOR.VolFx
{
    [ExecuteInEditMode]
    public class SketchVolFxController : MonoBehaviour
    {
        public bool enableDepthNormalsTex = true;
        public Camera normalsCam;
        public RenderTexture depthNormals;
        public bool createDepthNormalsTex = true;
        public bool addDepthInNormalsTex = false;

        public UnityEngine.RenderTextureFormat textureFormat = RenderTextureFormat.ARGB32;

        // Start is called before the first frame update
        void Start()
        {

        }
        public Material sketchMaterial;

        // Update is called once per frame
        void Update()
        {
            if (enableDepthNormalsTex && normalsCam != null)
            {
                if (depthNormals != null && (depthNormals.width == Screen.width && depthNormals.height == Screen.height && (!addDepthInNormalsTex || (depthNormals.depth == 16 && addDepthInNormalsTex))))
                {
                    if (normalsCam.targetTexture == null || normalsCam.targetTexture != depthNormals)
                    {
                        normalsCam.targetTexture = depthNormals;
                    }
                    //GL.ClearWithSkybox(true, normalsCam);
                    //normalsCam.Render();
                    Shader.SetGlobalTexture("_CameraDepthNormalsTextureA", depthNormals);//
                    if (sketchMaterial != null)
                    {                                                     // Debug.Log("SEND");
                        sketchMaterial.SetTexture("cameraDepthNormalsTextureA", depthNormals);
                    }
                }
                else
                {
                    if (createDepthNormalsTex)
                    {
                        if (addDepthInNormalsTex)
                        {
                            depthNormals = new RenderTexture(Screen.width, Screen.height, 16, textureFormat);
                        }
                        else
                        {
                            depthNormals = new RenderTexture(Screen.width, Screen.height, 0, textureFormat);
                        }
                        if (normalsCam.targetTexture == null || normalsCam.targetTexture != depthNormals)
                        {
                            normalsCam.targetTexture = depthNormals;
                        }
                        //GL.ClearWithSkybox(true, normalsCam);
                        //normalsCam.Render();
                        Shader.SetGlobalTexture("_CameraDepthNormalsTextureA", depthNormals);//
                        if (sketchMaterial != null)
                        {
                            sketchMaterial.SetTexture("cameraDepthNormalsTextureA", depthNormals);
                        }
                    }
                }
            }
        }
    }
}