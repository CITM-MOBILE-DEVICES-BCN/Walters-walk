using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
namespace Artngame.GLAMOR.VolFx
{
    [ExecuteInEditMode]
    public class connectSuntoHazyFogVOL_FX : MonoBehaviour
    {
        public Transform Sun;
        VolumeHazyFogFxVol volFog;
        public Volume _postProcessVolume;

        public Light localLightA;
        public float localLightIntensity=0;
        public float localLightRadius=0;

        //IMPOSTORS
        [Header("------------------------------------------------------")]
        [Header("Impostor Lights Array (List of Spot-Point disabled lights)")]
        [Header("------------------------------------------------------")]
        //v2.0
        public int maxImpostorLights = 32;
        //v1.9.9.1
        [Tooltip("Impostor spot and point Lights list, light component can be disabled and will still produce a volume based on their properties")]
        public List<Light> lightsArray = new List<Light>();


        // Start is called before the first frame update
        void Start()
        {
        }
        public bool useImpostorLights = false;
        public Material _material;

        // Update is called once per frame
        void Update()
        {

            if(_material == null)
            {
                _material = Resources.Load<Material>("VolumeFogFx_URP_RG_FBM");
            }


            if (useImpostorLights)
            {
                //v1.9.9.1
                //Debug.Log(_material.HasProperty("lightsArrayLength"));
                //Debug.Log(_material.HasProperty("controlByColor"));
                if (_material.HasProperty("lightsArrayLength") && lightsArray.Count > 0) //check for other shader versions
                {
                    //pass array
                    _material.SetVectorArray("_LightsArrayPos", new Vector4[maxImpostorLights]);//32
                    _material.SetVectorArray("_LightsArrayDir", new Vector4[maxImpostorLights]);//32
                    int countLights = lightsArray.Count;
                    if (countLights > maxImpostorLights)//32
                    {
                        countLights = maxImpostorLights;//32
                    }
                    _material.SetInt("lightsArrayLength", countLights);
                    //Debug.Log(countLights);
                    // material.SetFloatArray("_Points", new float[10]);
                    //float[] array = new float[] { 1, 2, 3, 4 };
                    Vector4[] posArray = new Vector4[countLights];
                    Vector4[] dirArray = new Vector4[countLights];
                    Vector4[] colArray = new Vector4[countLights];
                    for (int i = 0; i < countLights; i++)
                    {
                        //posArray[i].x = lightsArray(0).
                        posArray[i].x = lightsArray[i].transform.position.x;
                        posArray[i].y = lightsArray[i].transform.position.y;
                        posArray[i].z = lightsArray[i].transform.position.z;
                        posArray[i].w = lightsArray[i].intensity;
                        //Debug.Log(posArray[i].w);
                        colArray[i].x = lightsArray[i].color.r;
                        colArray[i].y = lightsArray[i].color.g;
                        colArray[i].z = lightsArray[i].color.b;

                        //check if point light
                        if (lightsArray[i].type == LightType.Point)
                        {
                            dirArray[i].x = 0;
                            dirArray[i].y = 0;
                            dirArray[i].z = 0;
                        }
                        else
                        {
                            dirArray[i].x = lightsArray[i].transform.forward.x;
                            dirArray[i].y = lightsArray[i].transform.forward.y;
                            dirArray[i].z = lightsArray[i].transform.forward.z;
                        }
                        dirArray[i].w = lightsArray[i].range;
                    }
                    _material.SetVectorArray("_LightsArrayPos", posArray);
                    _material.SetVectorArray("_LightsArrayDir", dirArray);
                    _material.SetVectorArray("_LightsArrayColor", colArray);
                    //material.SetFloatArray(array);
                }
                else
                {
                    _material.SetInt("lightsArrayLength", 0);
                }
            }
            else
            {
                _material.SetInt("lightsArrayLength", 0);
            }







            if (Sun != null && _postProcessVolume !=null)
            {
                if (Application.isPlaying)
                {
                    _postProcessVolume.profile.TryGet(out volFog);
                    if (volFog != null)
                    {
                        //if (localLightA != null)
                        //{
                            //volFog.sunTransform.value = sun.transform.position;
                        //}
                        Camera cam = Camera.current;
                        if (cam == null)
                        {
                            cam = Camera.main;
                        }
                        volFog._cameraRoll.value = cam.transform.eulerAngles.z;

                        volFog._cameraDiff.value = cam.transform.eulerAngles;// - prevRot;

                        if (cam.transform.eulerAngles.y > 360)
                        {
                            //volFog._cameraDiff.value.y = cam.transform.eulerAngles.y % 360;
                            volFog._cameraDiff.value = new Vector4(volFog._cameraDiff.value.x, cam.transform.eulerAngles.y % 360, volFog._cameraDiff.value.z, volFog._cameraDiff.value.w);
                        }
                        if (cam.transform.eulerAngles.y > 180)
                        {
                            //volFog._cameraDiff.value.y = -(360 - volFog._cameraDiff.value.y);
                            volFog._cameraDiff.value = new Vector4(volFog._cameraDiff.value.x, -(360 - volFog._cameraDiff.value.y), volFog._cameraDiff.value.z, volFog._cameraDiff.value.w);
                        }

                        //slipt in 90 degs, 90 to 180 mapped to 90 to zero
                        //volFog._cameraDiff.value.w = 1;
                        if (volFog._cameraDiff.value.y > 90 && volFog._cameraDiff.value.y < 180)
                        {
                            //volFog._cameraDiff.value.y = 180 - volFog._cameraDiff.value.y;
                            //volFog._cameraDiff.value.w = -1;
                            volFog._cameraDiff.value = new Vector4(volFog._cameraDiff.value.x, 180 - volFog._cameraDiff.value.y, volFog._cameraDiff.value.z, -1);
                            //volFog._cameraDiff.value.w = Mathf.Lerp(volFog._cameraDiff.value.w ,- 1, Time.deltaTime * 20);
                        }
                        else if (volFog._cameraDiff.value.y < -90 && volFog._cameraDiff.value.y > -180)
                        {
                           // volFog._cameraDiff.value.y = -180 - volFog._cameraDiff.value.y;
                            //volFog._cameraDiff.value.w = -1;
                            volFog._cameraDiff.value = new Vector4(volFog._cameraDiff.value.x, -180 - volFog._cameraDiff.value.y, volFog._cameraDiff.value.z, -1);
                            //volFog._cameraDiff.value.w = Mathf.Lerp(volFog._cameraDiff.value.w, -1, Time.deltaTime * 20);
                            //Debug.Log("dde");
                        }
                        else
                        {
                            //volFog._cameraDiff.value.w = Mathf.Lerp(volFog._cameraDiff.value.w, 1, Time.deltaTime * 20);
                            volFog._cameraDiff.value = new Vector4(volFog._cameraDiff.value.x, volFog._cameraDiff.value.y, volFog._cameraDiff.value.z, -1);
                            //volFog._cameraDiff.value.w = 1;
                        }

                        //vertical fix
                        if (cam.transform.eulerAngles.x > 360)
                        {
                            //volFog._cameraDiff.value.x = cam.transform.eulerAngles.x % 360;
                            volFog._cameraDiff.value = new Vector4(cam.transform.eulerAngles.x % 360, volFog._cameraDiff.value.y, volFog._cameraDiff.value.z, volFog._cameraDiff.value.w);
                        }
                        if (cam.transform.eulerAngles.x > 180)
                        {
                            //volFog._cameraDiff.value.x = 360 - volFog._cameraDiff.value.x;
                            volFog._cameraDiff.value = new Vector4(360 - volFog._cameraDiff.value.x, volFog._cameraDiff.value.y, volFog._cameraDiff.value.z, volFog._cameraDiff.value.w);
                        }
                        //Debug.Log(cam.transform.eulerAngles.x);
                        if (cam.transform.eulerAngles.x > 0 && cam.transform.eulerAngles.x < 180)
                        {
                            volFog._cameraTiltSign.value = 1;
                        }
                        else
                        {
                            // Debug.Log(cam.transform.eulerAngles.x);
                            volFog._cameraTiltSign.value = -1;
                        }
                        if (Sun != null)
                        {
                            Vector3 sunDir = Sun.transform.forward;
                            sunDir = Quaternion.AngleAxis(-cam.transform.eulerAngles.y, Vector3.up) * -sunDir;
                            sunDir = Quaternion.AngleAxis(cam.transform.eulerAngles.x, Vector3.left) * sunDir;
                            sunDir = Quaternion.AngleAxis(-cam.transform.eulerAngles.z, Vector3.forward) * sunDir;
                            // volFog.Sun.value = -new Vector4(sunDir.x, sunDir.y, sunDir.z, 1);
                            volFog.Sun.value = new Vector4(sunDir.x, sunDir.y, sunDir.z, 1);
                        }
                        if (localLightA != null)
                        {
                            volFog.PointL.value = new Vector4(localLightA.transform.position.x, localLightA.transform.position.y, localLightA.transform.position.z, localLightIntensity);
                            volFog.PointLParams.value = new Vector4(localLightA.color.r, localLightA.color.g, localLightA.color.b, localLightRadius);
                        }
                        //postProfile.sunTransform.value = Sun.transform.position;
                    }
                }
                else
                {
                    //IF IN EDITOR dont instantiate
                    _postProcessVolume.sharedProfile.TryGet(out volFog);
                    if (volFog != null)
                    {
                        Camera cam = Camera.current;
                        if (cam == null)
                        {
                            cam = Camera.main;
                        }
                        Vector3 sunDir = Sun.transform.forward;
                        sunDir = Quaternion.AngleAxis(-cam.transform.eulerAngles.y, Vector3.up) * -sunDir;
                        sunDir = Quaternion.AngleAxis(cam.transform.eulerAngles.x, Vector3.left) * sunDir;
                        sunDir = Quaternion.AngleAxis(-cam.transform.eulerAngles.z, Vector3.forward) * sunDir;
                        // volFog.Sun.value = -new Vector4(sunDir.x, sunDir.y, sunDir.z, 1);
                        volFog.Sun.value = new Vector4(sunDir.x, sunDir.y, sunDir.z, 1);

                        //postProfile.sunTransform.value = sun.transform.position;
                    }
                }
            }
        }
    }
}