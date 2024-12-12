using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
namespace Artngame.GLAMOR.VolFx
{
    [ExecuteInEditMode]
    public class connectSuntoCloudShadowsVOL_FX : MonoBehaviour
    {

        public Transform sun;
        CloudShadowsVol postProfile;
        public Volume _postProcessVolume;

        public bool controlNoiseVelocity = false;
        public Vector4 cloudNoiseVelocity = new Vector4(1,1,1,1);

        // Start is called before the first frame update
        void Start()
        {
            //Volume volume = gameObject.GetComponent<Volume>();
        }

        // Update is called once per frame
        void Update()
        {
            if (sun != null && _postProcessVolume !=null)
            {


                if (Application.isPlaying)
                {
                    _postProcessVolume.profile.TryGet(out postProfile);
                    if (postProfile != null)
                    {
                        postProfile.sunTransform.value = sun.transform.eulerAngles;

                        if (controlNoiseVelocity)
                        {
                            postProfile.noiseCloudSpeed.value = cloudNoiseVelocity;
                        }
                    }
                }
                else
                {
                    //IF IN EDITOR dont instantiate
                    _postProcessVolume.sharedProfile.TryGet(out postProfile);
                    if (postProfile != null)
                    {
                        postProfile.sunTransform.value = sun.transform.eulerAngles;

                        if (controlNoiseVelocity)
                        {
                            postProfile.noiseCloudSpeed.value = cloudNoiseVelocity;
                        }
                    }
                }


                //var sunShats = postProfile.GetSetting<ScreenSpaceSunShaftsVol>();
                //if (sunShats != null)
                //{
                //    sunShats.sunTransform.value = sun.transform.position;
                //}
            }
        }
    }
}