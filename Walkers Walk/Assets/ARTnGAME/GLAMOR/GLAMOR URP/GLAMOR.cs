using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using Artngame.SKYMASTER;
namespace Artngame.GLAMOR.VolFx
{
    [ExecuteInEditMode]
    public class GLAMOR : MonoBehaviour
    {
        public Volume _postProcessVolume;

        public GameObject layerChangeGroup;
        public GameObject layerChangeGroupA;
        public GameObject layerChangeparticleGroup;

        void Start()
        {
        }

        public bool controlInEditor = false;
        public bool controlInRunTime = false;
        public bool controlGUI = false;        

        [HideInInspector]
        public bool controlSunShafts = false;
        [HideInInspector]
        public float sunShaftsBlend = 1;

        // Update is called once per frame
        void Update()
        {
            if (_postProcessVolume != null)
            {
                if (Application.isPlaying)
                {
                    if (controlSunShafts) ///////// SUN SHAFTS /////////
                    {
                        _postProcessVolume.profile.TryGet(out sunShaftsVolume); //IF IN PLAY instantiate
                        if (sunShaftsVolume != null)
                        {
                            //sunShaftsVolume.sunTransform.value = sun.transform.position;
                        }
                    }
                }
                else
                {
                    if (controlSunShafts) ///////// SUN SHAFTS /////////
                    {
                        
                        _postProcessVolume.sharedProfile.TryGet(out sunShaftsVolume); //IF IN EDITOR dont instantiate
                        if (sunShaftsVolume != null)
                        {
                            //sunShaftsVolume.sunTransform.value = sun.transform.position;
                        }
                    }
                }
            }
        }

        public int offsetYGap = 35;
        public int buttonsXGap = 100;
        int currentDirtTexture = 0;
        public List<Texture2D> dirtTextures = new List<Texture2D>();

        float red = 0;
        float green = 0;
        float blue = 0;
        Gradient gradient;
        GradientValue gradVal;// = new GradientValue(gradient);
        GradientColorKey[] colorKeys = new GradientColorKey[2];

        public ScreenSpaceRainLITE_SM_URP rainController;
        public int sliderWidth = 90;

        private void SetGameLayerRecursive(GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform child in gameObject.transform)
            {
                SetGameLayerRecursive(child.gameObject, layer);
            }
        }

        void OnGUI()
        {
            if (controlGUI && Application.isPlaying)
            {
                if (GUI.Button(new Rect(10 + buttonsXGap * 11, 10 + 0, 200, 30), "Toggle GLAMOR GUI"))
                {
                    controlInRunTime = controlInRunTime ? false : true;
                }
            }
            if (controlInRunTime && controlGUI && Application.isPlaying)
            {

                if (layerChangeparticleGroup != null)
                {
                    if (GUI.Button(new Rect(10 + buttonsXGap * 9, 10 + 0, 200, 30), "Add particle to FX"))
                    {
                        SetGameLayerRecursive(layerChangeparticleGroup, 8);
                    }
                    if (GUI.Button(new Rect(10 + buttonsXGap * 9, 10 + 32, 200, 30), "Remove particle to FX"))
                    {
                        SetGameLayerRecursive(layerChangeparticleGroup, 0);                        
                    }
                }
                if (layerChangeGroupA != null)
                {
                    if (GUI.Button(new Rect(10 + buttonsXGap * 7, 10 + 32, 200, 30), "Remove all from FX"))
                    {
                        SetGameLayerRecursive(layerChangeGroup, 0);
                        SetGameLayerRecursive(layerChangeGroupA, 0);
                        //foreach (Transform child in layerChangeGroup.transform)
                        //{
                        //    child.gameObject.layer = 0;
                        //}
                    }
                }
                if (layerChangeGroup != null)
                {
                    if (GUI.Button(new Rect(10 + buttonsXGap * 5, 10, 200, 30), "Add to FX Group"))
                    {
                        //SetGameLayerRecursive(layerChangeGroup, 8);
                        foreach (Transform child in layerChangeGroup.transform)
                        {
                            child.gameObject.layer = 8;
                        }
                    }
                    if (GUI.Button(new Rect(10 + buttonsXGap * 7, 10, 200, 30), "Add more to FX Group"))
                    {
                        SetGameLayerRecursive(layerChangeGroup, 8);
                        SetGameLayerRecursive(layerChangeGroupA, 8);
                        //foreach (Transform child in layerChangeGroup.transform)
                        //{
                        //    child.gameObject.layer = 8;
                        //}
                    }
                    if (GUI.Button(new Rect(10 + buttonsXGap * 5, 10 + 32, 200, 30), "Remove from FX Group"))
                    {
                        SetGameLayerRecursive(layerChangeGroup, 0);
                        //foreach (Transform child in layerChangeGroup.transform)
                        //{
                        //    child.gameObject.layer = 0;
                        //}
                    }
                }

                //SUN SHAFTS
                int offsetY = 0;
                if (GUI.Button(new Rect(10 + buttonsXGap, 10, 100, 30), "Sun Shafts"))
                {
                    enableSunShaftsControls = enableSunShaftsControls ? false : true;
                }
                _postProcessVolume.profile.TryGet(out sunShaftsVolume); //IF IN PLAY instantiate
                if (sunShaftsVolume != null)
                {
                    sunShaftsVolume.m_Intencity.value = GUI.HorizontalSlider(new Rect(10, 10 + offsetY, sliderWidth, 30), sunShaftsVolume.m_Intencity.value, 0, 1);
                    if (enableSunShaftsControls)
                    {
                        if (GUI.Button(new Rect(10 + buttonsXGap * 2, 10 + offsetY, sliderWidth, 30), "Toggle Mode"))
                        {
                            if (sunShaftsVolume.blendChoice.value == 0)
                            {
                                sunShaftsVolume.blendChoice.value = 1;
                            }
                            else
                            {
                                sunShaftsVolume.blendChoice.value = 0;
                            }
                        }
                        if (GUI.Button(new Rect(10 + buttonsXGap * 3, 10 + offsetY, sliderWidth, 30), "Toggle Noise"))
                        {
                            if (sunShaftsVolume.useNoise.value)
                            {
                                sunShaftsVolume.useNoise.value = false;
                            }
                            else
                            {
                                sunShaftsVolume.useNoise.value = true;
                            }
                        }
                        sunShaftsVolume.sunShaftBlurRadius.value =
                            GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 4, 10 + offsetY, sliderWidth, 30), sunShaftsVolume.sunShaftBlurRadius.value, 0, 5);
                    }
                }

                //FOG VOLUME
                offsetY += offsetYGap;
                if (GUI.Button(new Rect(10 + buttonsXGap, 10 + offsetY, sliderWidth, 30), "Fog Volume"))
                {
                    enablefogVolumeControls = enablefogVolumeControls ? false : true;
                }
                _postProcessVolume.profile.TryGet(out fogVolume); //IF IN PLAY instantiate
                if (fogVolume != null)
                {
                    fogVolume.m_Intencity.value = GUI.HorizontalSlider(new Rect(10, 10 + offsetY, sliderWidth, 30), fogVolume.m_Intencity.value, 0, 1);
                    if (enablefogVolumeControls)
                    {
                        fogVolume.heightDensity.value =
                          GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 2, 10 + offsetY, sliderWidth, 30), fogVolume.heightDensity.value, 100, 550);
                        fogVolume.noiseThickness.value =
                          GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 3, 10 + offsetY, sliderWidth, 30), fogVolume.noiseThickness.value, 0, 5);
                    }
                }

                //LENS EFFECTS
                offsetY += offsetYGap;
                if (GUI.Button(new Rect(10 + buttonsXGap, 10 + offsetY, sliderWidth, 30), "Lens Effects"))
                {
                    enablelensEffectsControls = enablelensEffectsControls ? false : true;
                }
                _postProcessVolume.profile.TryGet(out lensEffectsVolume); //IF IN PLAY instantiate
                if (lensEffectsVolume != null)
                {
                    lensEffectsVolume.m_Intencity.value = GUI.HorizontalSlider(new Rect(10, 10 + offsetY, sliderWidth, 30), lensEffectsVolume.m_Intencity.value, 0, 1);
                    if (enablelensEffectsControls)
                    {
                        //FIRST Element
                        if (GUI.Button(new Rect(10 + buttonsXGap * 2, 10 + offsetY, sliderWidth, 30), "Toggle Streaks"))
                        {
                            if (lensEffectsVolume.verticalStreaks.value && lensEffectsVolume.horizontalStreaks.value)
                            {
                                lensEffectsVolume.verticalStreaks.value = false;
                            }
                            else if (lensEffectsVolume.horizontalStreaks.value)
                            {
                                lensEffectsVolume.verticalStreaks.value = true;
                                lensEffectsVolume.horizontalStreaks.value = false;
                            }
                            else
                            {
                                lensEffectsVolume.verticalStreaks.value = true;
                                lensEffectsVolume.horizontalStreaks.value = true;
                            }
                        }
                        if (GUI.Button(new Rect(10 + buttonsXGap * 3, 10 + offsetY, sliderWidth, 30), "Toggle Bloom"))
                        {
                            if (lensEffectsVolume.addBloom.value)
                            {
                                lensEffectsVolume.addBloom.value = false;
                            }
                            else
                            {
                                lensEffectsVolume.addBloom.value = true;
                            }
                        }
                        //dirt power
                        if (GUI.Button(new Rect(10 + buttonsXGap * 4, 10 + offsetY, sliderWidth, 30), "Toggle Dirt"))
                        {
                            if (currentDirtTexture >= dirtTextures.Count)
                            {
                                currentDirtTexture = 0;
                            }
                            lensEffectsVolume.lensDirtTexture.value = dirtTextures[currentDirtTexture];
                            currentDirtTexture++;
                        }
                        //powers
                        lensEffectsVolume.m_dirtBloomPower.value =
                            GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 5, 10 + offsetY, sliderWidth, 30), lensEffectsVolume.m_dirtBloomPower.value, 0, 10);
                        lensEffectsVolume.bloomIntensity.value =
                           GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 6, 10 + offsetY, sliderWidth, 30), lensEffectsVolume.bloomIntensity.value, 0, 10);
                        lensEffectsVolume.stretch.value =
                          GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 7, 10 + offsetY, sliderWidth, 30), lensEffectsVolume.stretch.value, 0, 1);
                        lensEffectsVolume.stretchV.value =
                          GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 8, 10 + offsetY, sliderWidth, 30), lensEffectsVolume.stretchV.value, 0, 1);
                        if (GUI.Button(new Rect(10 + buttonsXGap * 9, 10 + offsetY, sliderWidth, 30), "Remove Dirt"))
                        {
                            lensEffectsVolume.lensDirtTexture.value = null;
                        }
                    }
                }

                //PAINTERLY KAWAHARA
                offsetY += offsetYGap;
                if (GUI.Button(new Rect(10 + buttonsXGap, 10 + offsetY, sliderWidth, 30), "Painterly"))
                {
                    enablepainterlyControls = enablepainterlyControls ? false : true;
                }
                _postProcessVolume.profile.TryGet(out painterlyVolume); //IF IN PLAY instantiate
                if (painterlyVolume != null)
                {
                    painterlyVolume.m_Intencity.value = GUI.HorizontalSlider(new Rect(10, 10 + offsetY, sliderWidth, 30), painterlyVolume.m_Intencity.value, 0, 1);
                    if (enablepainterlyControls)
                    {
                        if (GUI.Button(new Rect(10 + buttonsXGap * 2, 10 + offsetY, sliderWidth, 30), "Toggle Type"))
                        {
                            if (painterlyVolume.KawaharaType.value == 0)
                            {
                                painterlyVolume.KawaharaType.value = 1;
                            }
                            else if (painterlyVolume.KawaharaType.value == 1)
                            {
                                painterlyVolume.KawaharaType.value = 2;
                            }
                            else if (painterlyVolume.KawaharaType.value == 2)
                            {
                                painterlyVolume.KawaharaType.value = 0;
                            }
                        }
                        painterlyVolume.kernelSizeKAWAHARA.value =
                            (int)GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 3, 10 + offsetY, sliderWidth, 30), painterlyVolume.kernelSizeKAWAHARA.value, 2, 9);
                        painterlyVolume.sharpness.value =
                            GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 4, 10 + offsetY, sliderWidth, 30), painterlyVolume.sharpness.value, 1, 20);
                        painterlyVolume.zeta.value =
                            GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 5, 10 + offsetY, sliderWidth, 30), painterlyVolume.zeta.value, 0.05f, 2);
                        painterlyVolume.alpha.value =
                            GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 6, 10 + offsetY, sliderWidth, 30), painterlyVolume.alpha.value, 0.01f, 0.2f);
                    }
                }

                //BLOOM
                offsetY += offsetYGap;
                if (GUI.Button(new Rect(10 + buttonsXGap, 10 + offsetY, sliderWidth, 30), "Bloom -"))
                {
                    enablebloomControls = enablebloomControls ? false : true;
                }
                _postProcessVolume.profile.TryGet(out bloom); //IF IN PLAY instantiate
                if (bloom != null)
                {
                    bloom.m_Intencity.value = GUI.HorizontalSlider(new Rect(10, 10 + offsetY, sliderWidth, 30), bloom.m_Intencity.value, 0, 4);
                    if (enablebloomControls)
                    {

                        /*
                        red = 
                            GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 2, 10 + offsetY, sliderWidth, 30), bloom.m_Color.value._grad.colorKeys[0].color.r, 0, 1);
                        green =
                            GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 3, 10 + offsetY, sliderWidth, 30), bloom.m_Color.value._grad.colorKeys[0].color.g, 0, 1);
                        blue =
                            GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 4, 10 + offsetY, sliderWidth, 30), bloom.m_Color.value._grad.colorKeys[0].color.b, 0, 1);

                        
                        
                        if (bloom.m_Color.value._grad.colorKeys[0].color.r != red ||
                            bloom.m_Color.value._grad.colorKeys[0].color.g != green ||
                            bloom.m_Color.value._grad.colorKeys[0].color.b != blue) {

                            Debug.Log("RED 0 = " + bloom.m_Color.value._grad.colorKeys[0].color.r);
                            Debug.Log("RED 1 = " + red);
                            if (gradient == null)
                            {
                                gradient = new Gradient();
                            }
                            gradient.colorKeys = bloom.m_Color.value._grad.colorKeys;
                            gradient.SetKeys(bloom.m_Color.value._grad.colorKeys, bloom.m_Color.value._grad.alphaKeys);
                            gradient.alphaKeys = bloom.m_Color.value._grad.alphaKeys;
                            //gradient.SetKeys(
                            //    new GradientColorKey[] { new GradientColorKey(Color.green, 0.0f), new GradientColorKey(Color.red, 1.0f) },
                            //    new GradientAlphaKey[] { new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f) }
                            //);
                            gradient.colorKeys[0].color = new Color(red,green,blue);
                            gradient.colorKeys[0].color.r = red;
                            gradient.colorKeys[0].color.g = green;
                            gradient.colorKeys[0].color.b = blue;
                            if (gradVal == null)
                            {
                                gradVal = new GradientValue(gradient);
                            }
                            //GradientColorKey[] colorKeys = new GradientColorKey[2];
                            colorKeys[0].color = new Color(red, green, blue);
                            colorKeys[0].time = 0;
                            colorKeys[1].color = bloom.m_Color.value._grad.colorKeys[1].color;
                            colorKeys[1].time = 1;
                            gradVal._grad.SetKeys(colorKeys, gradient.alphaKeys);
                            bloom.m_Color.value = gradVal;
                            bloom.m_Color.overrideState = true;
                            bloom.m_Color = new GradientParameter(gradVal, true);
                            Debug.Log("RED 2 = " + gradVal._grad.colorKeys[0].color.r);
                            //bloom.
                            //bloom.m_Color.value._grad.SetKeys(gradient.colorKeys, gradient.alphaKeys);
                        }
                        */
                    }
                }

                //COLOR ADJUSTEMENTS
                offsetY += offsetYGap;
                if (GUI.Button(new Rect(10 + buttonsXGap, 10 + offsetY, sliderWidth, 30), "Color Adjust"))
                {
                    enableadjustemetsColorControls = enableadjustemetsColorControls ? false : true;
                }
                _postProcessVolume.profile.TryGet(out adjustemetsColor); //IF IN PLAY instantiate
                if (adjustemetsColor != null)
                {
                    adjustemetsColor.m_Alpha.value = GUI.HorizontalSlider(new Rect(10, 10 + offsetY, sliderWidth, 30), adjustemetsColor.m_Alpha.value, -1, 1);
                    if (enableadjustemetsColorControls)
                    {
                        adjustemetsColor.m_Hue.value =
                          GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 2, 10 + offsetY, sliderWidth, 30), adjustemetsColor.m_Hue.value, -1, 1);
                        adjustemetsColor.m_Saturation.value =
                          GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 3, 10 + offsetY, sliderWidth, 30), adjustemetsColor.m_Saturation.value, -1, 1);
                        adjustemetsColor.m_Brightness.value =
                          GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 4, 10 + offsetY, sliderWidth, 30), adjustemetsColor.m_Brightness.value, -1, 1);
                        adjustemetsColor.m_Alpha.value =
                          GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 5, 10 + offsetY, sliderWidth, 30), adjustemetsColor.m_Alpha.value, -1, 1);
                    }
                }

                //GRAYSCALE 
                offsetY += offsetYGap;
                if (GUI.Button(new Rect(10 + buttonsXGap, 10 + offsetY, sliderWidth, 30), "Gray Scale - "))
                {
                    enablegrayScaleControls = enablegrayScaleControls ? false : true;
                }
                _postProcessVolume.profile.TryGet(out grayScale); //IF IN PLAY instantiate
                if (grayScale != null)
                {
                    grayScale.m_Weight.value = GUI.HorizontalSlider(new Rect(10, 10 + offsetY, sliderWidth, 30), grayScale.m_Weight.value, 0, 1);
                    if (enablegrayScaleControls)
                    {

                    }
                }

                //BLUR
                offsetY += offsetYGap;
                if (GUI.Button(new Rect(10 + buttonsXGap, 10 + offsetY, sliderWidth, 30), "Blur"))
                {
                    enableblurControls = enableblurControls ? false : true;
                }
                _postProcessVolume.profile.TryGet(out blur); //IF IN PLAY instantiate
                if (blur != null)
                {
                    blur.m_Radial.value = GUI.HorizontalSlider(new Rect(10, 10 + offsetY, sliderWidth, 30), blur.m_Radial.value, 0, 1);

                    if (enableblurControls)
                    {
                        blur.m_Radius.value = GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 2, 10 + offsetY, sliderWidth, 30), blur.m_Radius.value, 0, 1);
                        //+ buttonsXGap*2
                    }
                }

                //Pixelator
                offsetY += offsetYGap;
                if (GUI.Button(new Rect(10 + buttonsXGap, 10 + offsetY, sliderWidth, 30), "Pixelate"))
                {
                    enablepixelatorControls = enablepixelatorControls ? false : true;
                }
                _postProcessVolume.profile.TryGet(out pixelator); //IF IN PLAY instantiate
                if (pixelator != null)
                {
                    pixelator.m_Scale.value = GUI.HorizontalSlider(new Rect(10, 10 + offsetY, sliderWidth, 30), pixelator.m_Scale.value, 0, 1);
                    if (enablepixelatorControls)
                    {
                        pixelator.m_Grid.value =
                         GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 2, 10 + offsetY, sliderWidth, 30), pixelator.m_Grid.value, 0, 1);
                        pixelator.m_Roundness.value =
                         GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 3, 10 + offsetY, sliderWidth, 30), pixelator.m_Roundness.value, 0, 1);
                    }
                }

                //VHS Video Effect
                offsetY += offsetYGap;
                if (GUI.Button(new Rect(10 + buttonsXGap, 10 + offsetY, sliderWidth, 30), "VHS Video FX"))
                {
                    enablevhsVideoEffectControls = enablevhsVideoEffectControls ? false : true;
                }
                _postProcessVolume.profile.TryGet(out vhsVideoEffect); //IF IN PLAY instantiate
                if (vhsVideoEffect != null)
                {
                    vhsVideoEffect._weight.value = GUI.HorizontalSlider(new Rect(10, 10 + offsetY, sliderWidth, 30), vhsVideoEffect._weight.value, 0, 1);
                    if (enablevhsVideoEffectControls)
                    {
                        vhsVideoEffect._bleed.value =
                         GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 2, 10 + offsetY, sliderWidth, 30), vhsVideoEffect._bleed.value, 0, 10);
                        vhsVideoEffect._rocking.value =
                         GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 3, 10 + offsetY, sliderWidth, 30), vhsVideoEffect._rocking.value, 0, 0.1f);
                        vhsVideoEffect._tape.value =
                         GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 4, 10 + offsetY, sliderWidth, 30), vhsVideoEffect._tape.value, 0, 1);
                        vhsVideoEffect._noise.value =
                         GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 5, 10 + offsetY, sliderWidth, 30), vhsVideoEffect._noise.value, 0, 0.95f);
                    }
                }

                //Ascii Effect
                offsetY += offsetYGap;
                if (GUI.Button(new Rect(10 + buttonsXGap, 10 + offsetY, sliderWidth, 30), "Ascii Effect"))
                {
                    enableasciiEffectControls = enableasciiEffectControls ? false : true;
                }
                _postProcessVolume.profile.TryGet(out asciiEffect); //IF IN PLAY instantiate
                if (asciiEffect != null)
                {
                    asciiEffect.m_Scale.value = GUI.HorizontalSlider(new Rect(10, 10 + offsetY, sliderWidth, 30), asciiEffect.m_Scale.value, 0, 1);
                    if (enableasciiEffectControls)
                    {
                        asciiEffect.m_Depth.value =
                          (int)GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 2, 10 + offsetY, sliderWidth, 30), asciiEffect.m_Depth.value, 0, 7);

                        // Color getCol = asciiEffect.m_Image.value;

                        float red1 = GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 3, 10 + offsetY, sliderWidth, 30), asciiEffect.m_Image.value.r, 0, 1);
                        float green1 = GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 4, 10 + offsetY, sliderWidth, 30), asciiEffect.m_Image.value.g, 0, 1);
                        float blue1 = GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 5, 10 + offsetY, sliderWidth, 30), asciiEffect.m_Image.value.b, 0, 1);
                        asciiEffect.m_Image.value = new Color(red1, green1, blue1);
                    }
                }

                //Ouline Effect
                offsetY += offsetYGap;
                if (GUI.Button(new Rect(10 + buttonsXGap, 10 + offsetY, sliderWidth, 30), "Ouline"))
                {
                    enableoutlineControls = enableoutlineControls ? false : true;
                }
                _postProcessVolume.profile.TryGet(out outline); //IF IN PLAY instantiate
                if (outline != null)
                {
                    outline.m_Sensitive.value = GUI.HorizontalSlider(new Rect(10, 10 + offsetY, sliderWidth, 30), outline.m_Sensitive.value, 0, 0.2f);
                    if (enableoutlineControls)
                    {
                        if (GUI.Button(new Rect(10 + buttonsXGap * 2, 10 + offsetY, sliderWidth, 30), "Outline Mode"))
                        {
                            if (outline.m_Mode.value == OutlinePass.Mode.Depth)
                            {
                                outline.m_Mode.value = OutlinePass.Mode.Alpha;
                            }
                            else if (outline.m_Mode.value == OutlinePass.Mode.Alpha)
                            {
                                outline.m_Mode.value = OutlinePass.Mode.Chroma;
                            }
                            else if (outline.m_Mode.value == OutlinePass.Mode.Chroma)
                            {
                                outline.m_Mode.value = OutlinePass.Mode.Luma;
                            }
                            else if (outline.m_Mode.value == OutlinePass.Mode.Luma)
                            {
                                outline.m_Mode.value = OutlinePass.Mode.Depth;
                            }
                        }
                        outline.m_Thickness.value = GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 3, 10 + offsetY, sliderWidth, 30), outline.m_Thickness.value, 0, 1);
                    }
                }

                //Flow Effect
                offsetY += offsetYGap;
                if (GUI.Button(new Rect(10 + buttonsXGap, 10 + offsetY, sliderWidth, 30), "Flow Effect"))
                {
                    enableflowEffectControls = enableflowEffectControls ? false : true;
                }
                _postProcessVolume.profile.TryGet(out flowEffect); //IF IN PLAY instantiate
                if (flowEffect != null)
                {

                    if (enableflowEffectControls)
                    {
                        Vector3 flow = flowEffect.m_Flow.value;
                        float flowY = GUI.HorizontalSlider(new Rect(10, 10 + offsetY, sliderWidth, 30), flowEffect.m_Flow.value.y, -2, 2);
                        float flowZ = GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 2, 10 + offsetY, sliderWidth, 30), flowEffect.m_Flow.value.z, -2, 2);
                        flowEffect.m_Flow.value = new Vector3(0, flowY, flowZ);
                    }
                    else
                    {
                        Vector3 flow = flowEffect.m_Flow.value;
                        float flowY = GUI.HorizontalSlider(new Rect(10, 10 + offsetY, sliderWidth, 30), flowEffect.m_Flow.value.y, -2, 2);
                        //float flowZ = GUI.HorizontalSlider(new Rect(10 + 100, 10 + offsetY, sliderWidth, 30), flowEffect.m_Flow.value.z, -2, 2);
                        flowEffect.m_Flow.value = new Vector3(0, flowY, 0);
                    }
                }

                //Gradient Color
                offsetY += offsetYGap;
                if (GUI.Button(new Rect(10 + buttonsXGap, 10 + offsetY, sliderWidth, 30), "Gradient Color -"))
                {
                    enablegradientVolControls = enablegradientVolControls ? false : true;
                }
                _postProcessVolume.profile.TryGet(out gradientVol); //IF IN PLAY instantiate
                if (gradientVol != null)
                {
                    gradientVol.m_Weight.value = GUI.HorizontalSlider(new Rect(10, 10 + offsetY, sliderWidth, 30), gradientVol.m_Weight.value, 0, 1);
                    if (enablegradientVolControls)
                    {

                    }
                }

                //Old Movie Effect
                offsetY += offsetYGap;
                if (GUI.Button(new Rect(10 + buttonsXGap, 10 + offsetY, sliderWidth, 30), "Old Movie"))
                {
                    enableoldMovieEffectControls = enableoldMovieEffectControls ? false : true;
                }
                _postProcessVolume.profile.TryGet(out oldMovieEffect); //IF IN PLAY instantiate
                if (oldMovieEffect != null)
                {
                    if (GUI.Button(new Rect(10, 10 + offsetY, sliderWidth, 30), "Old Movie"))
                    {
                        oldMovieEffect.active = oldMovieEffect.active ? false : true;
                    }
                    //oldMovieEffect.active = GUI.HorizontalSlider(new Rect(10, 10 + offsetY, sliderWidth, 30), oldMovieEffect.m_Weight.value, 0, 1);
                    if (enableoldMovieEffectControls)
                    {
                        oldMovieEffect.m_Fps.value =
                            GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 2, 10 + offsetY, sliderWidth, 30), oldMovieEffect.m_Fps.value, 2, 15);
                        oldMovieEffect.m_Jolt.value =
                            GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 3, 10 + offsetY, sliderWidth, 30), oldMovieEffect.m_Jolt.value, 0, 0.3f);
                    }
                }

                //Sharpen Effect
                offsetY += offsetYGap;
                if (GUI.Button(new Rect(10 + buttonsXGap, 10 + offsetY, sliderWidth, 30), "Sharpen Effect"))
                {
                    enablesharpenerControls = enablesharpenerControls ? false : true;
                }
                _postProcessVolume.profile.TryGet(out sharpener); //IF IN PLAY instantiate
                if (sharpener != null)
                {
                    sharpener.m_Samples.value = GUI.HorizontalSlider(new Rect(10, 10 + offsetY, sliderWidth, 30), sharpener.m_Samples.value, 0, 100);
                    if (enablesharpenerControls)
                    {
                        sharpener.m_Radius.value = GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 2, 10 + offsetY, sliderWidth, 30), sharpener.m_Radius.value, 0, 0.4f);
                    }
                }

                //RAIN Effect
                offsetY += offsetYGap;
                if (GUI.Button(new Rect(10 + buttonsXGap, 10 + offsetY, sliderWidth, 30), "Rain Effect"))
                {
                    enablerainEffectControls = enablerainEffectControls ? false : true;
                }
                _postProcessVolume.profile.TryGet(out rainEffect); //IF IN PLAY instantiate
                if (rainEffect != null)
                {
                    rainEffect.m_Weight.value = GUI.HorizontalSlider(new Rect(10, 10 + offsetY, sliderWidth, 30), rainEffect.m_Weight.value, 0, 1);
                    //rainEffect.m_Radius.value = GUI.HorizontalSlider(new Rect(10 + 100, 10 + offsetY, sliderWidth, 30), rainEffect.m_Radius.value, 0, 0.4f);
                    if (enablerainEffectControls)
                    {
                        if (rainController != null)
                        {
                            rainController.screenRainPower =
                                GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 2, 10 + offsetY, sliderWidth, 30), rainController.screenRainPower, 0, 5);
                            rainController.rainContrast =
                                GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 3, 10 + offsetY, sliderWidth, 30), rainController.rainContrast, 0, 5);
                            rainController.rainPower =
                                GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 4, 10 + offsetY, sliderWidth, 30), rainController.rainPower, 0, 5);
                            rainController.RainIntensity =
                                GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 5, 10 + offsetY, sliderWidth, 30), rainController.RainIntensity, 0, 8);
                        }
                    }
                }

                //Lens Flare Effects
                offsetY += offsetYGap;
                if (GUI.Button(new Rect(10 + buttonsXGap, 10 + offsetY, sliderWidth, 30), "Lens Flares"))
                {
                    enablelensFlaresControls = enablelensFlaresControls ? false : true;
                }
                _postProcessVolume.profile.TryGet(out lensFlares); //IF IN PLAY instantiate
                if (lensFlares != null)
                {
                    lensFlares.m_Intencity.value = GUI.HorizontalSlider(new Rect(10, 10 + offsetY, sliderWidth, 30), lensFlares.m_Intencity.value, 0, 1);
                    if (enablelensFlaresControls)
                    {
                        lensFlares.BlurSize.value =
                            (int)GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 2, 10 + offsetY, sliderWidth, 30), lensFlares.BlurSize.value, 0, 10);
                        lensFlares.Subtract.value =
                            GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 3, 10 + offsetY, sliderWidth, 30), lensFlares.Subtract.value, -1, 35);
                        lensFlares.Multiply.value =
                            GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 4, 10 + offsetY, sliderWidth, 30), lensFlares.Multiply.value, 0, 6);
                        lensFlares.Downsample.value =
                            (int)GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 5, 10 + offsetY, sliderWidth, 30), lensFlares.Downsample.value, 1, 6);
                        lensFlares.NumberOfGhosts.value =
                            (int)GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 6, 10 + offsetY, sliderWidth, 30), lensFlares.NumberOfGhosts.value, 1, 85);
                        lensFlares.Falloff.value =
                            GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 7, 10 + offsetY, sliderWidth, 30), lensFlares.Falloff.value, -5, 35);
                        lensFlares.HaloWidth.value =
                            GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 8, 10 + offsetY, sliderWidth, 30), lensFlares.HaloWidth.value, -1, 1.5f);
                        lensFlares.HaloFalloff.value =
                            GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 9, 10 + offsetY, sliderWidth, 30), lensFlares.HaloFalloff.value, -4, 2.5f);
                        lensFlares.HaloSubtract.value =
                            GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 10, 10 + offsetY, sliderWidth, 30), lensFlares.HaloSubtract.value, -0.2f, 50f);
                        lensFlares.Sigma.value =
                            GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 11, 10 + offsetY, sliderWidth, 30), lensFlares.Sigma.value, 0.01f, 100f);
                        lensFlares.chromaticAberration.value =
                            GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 12, 10 + offsetY, sliderWidth, 30), lensFlares.chromaticAberration.value, -1, 10f);
                    }
                }

                //Hazy bloom Effect
                offsetY += offsetYGap;
                if (GUI.Button(new Rect(10 + buttonsXGap, 10 + offsetY, sliderWidth, 30), "Hazy bloom"))
                {
                    enablehazyBloomControls = enablehazyBloomControls ? false : true;
                }
                _postProcessVolume.profile.TryGet(out hazyBloom); //IF IN PLAY instantiate
                if (hazyBloom != null)
                {
                    hazyBloom.m_Intencity.value = GUI.HorizontalSlider(new Rect(10, 10 + offsetY, sliderWidth, 30), hazyBloom.m_Intencity.value, 0, 1);
                    if (enablehazyBloomControls)
                    {
                        hazyBloom.distanceMultiplier.value =
                            GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 2, 10 + offsetY, sliderWidth, 30), hazyBloom.distanceMultiplier.value, 0, 100);
                    }
                }

                //Cloud shadows Effect
                offsetY += offsetYGap;
                if (GUI.Button(new Rect(10 + buttonsXGap, 10 + offsetY, sliderWidth, 30), "Cloud shadows"))
                {
                    enablecloudShadowsControls = enablecloudShadowsControls ? false : true;
                }
                _postProcessVolume.profile.TryGet(out cloudShadows); //IF IN PLAY instantiate
                if (cloudShadows != null)
                {
                    cloudShadows.m_Weight.value = GUI.HorizontalSlider(new Rect(10, 10 + offsetY, sliderWidth, 30), cloudShadows.m_Weight.value, 0, 1);
                    if (enablecloudShadowsControls)
                    {
                        if (GUI.Button(new Rect(10 + buttonsXGap * 2, 10 + offsetY, sliderWidth, 30), "Caustic Toggle"))
                        {
                            if (cloudShadows.rainMode.value == 0)
                            {
                                cloudShadows.rainMode.value = 1;
                            }
                            else
                            {
                                cloudShadows.rainMode.value = 0;
                            }
                        }
                        float scaleX = GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 3, 10 + offsetY, sliderWidth, 30), cloudShadows.cloudShadowScale.value.x, 0.5f, 3);
                        cloudShadows.cloudShadowScale.value = new Vector3(scaleX, cloudShadows.cloudShadowScale.value.y, cloudShadows.cloudShadowScale.value.z);
                    }
                }

                //Sketch Effect
                offsetY += offsetYGap;
                if (GUI.Button(new Rect(10 + buttonsXGap, 10 + offsetY, sliderWidth, 30), "Sketch"))
                {
                    enablesketchEffectsControls = enablesketchEffectsControls ? false : true;
                }
                _postProcessVolume.profile.TryGet(out sketchEffects); //IF IN PLAY instantiate
                if (sketchEffects != null)
                {
                    sketchEffects.m_Intencity.value = GUI.HorizontalSlider(new Rect(10, 10 + offsetY, sliderWidth, 30), sketchEffects.m_Intencity.value, 0, 1);
                    if (enablesketchEffectsControls)
                    {
                        sketchEffects.luminance.value =
                           GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 2, 10 + offsetY, sliderWidth, 30), sketchEffects.luminance.value, 0.5f, 5);
                        sketchEffects.contrast.value =
                           GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 3, 10 + offsetY, sliderWidth, 30), sketchEffects.contrast.value, 1, 2.5f);
                    }
                }

                //Unity color adjust Effect
                offsetY += offsetYGap;
                if (GUI.Button(new Rect(10 + buttonsXGap, 10 + offsetY, sliderWidth, 30), "Color Adjust"))
                {
                    enableunityColorControls = enableunityColorControls ? false : true;
                }
                _postProcessVolume.profile.TryGet(out colorAdjust); //IF IN PLAY instantiate
                if (colorAdjust != null)
                {
                    colorAdjust.postExposure.value = GUI.HorizontalSlider(new Rect(10, 10 + offsetY, sliderWidth, 30), colorAdjust.postExposure.value, 0.2f, 3.2f);
                    if (enableunityColorControls)
                    {
                        colorAdjust.saturation.value =
                           GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 2, 10 + offsetY, sliderWidth, 30), colorAdjust.saturation.value, -30f, 30);
                        colorAdjust.contrast.value =
                           GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 3, 10 + offsetY, sliderWidth, 30), colorAdjust.contrast.value, -25, 35f);
                    }
                }

                //Unity Tone Mapping Effect
                offsetY += offsetYGap;
                _postProcessVolume.profile.TryGet(out unityToneMap); //IF IN PLAY instantiate
                if (unityToneMap != null)
                {                   
                    if (GUI.Button(new Rect(10 + buttonsXGap, 10 + offsetY, sliderWidth, 30), "Tone Map"))
                    {
                        if (unityToneMap.mode.value == TonemappingMode.Neutral)
                        {
                            unityToneMap.mode.value = TonemappingMode.ACES;
                            currentMap = "ACES";
                        }
                        else if (unityToneMap.mode.value == TonemappingMode.ACES)
                        {
                            unityToneMap.mode.value = TonemappingMode.None;
                            currentMap = "None";
                        }
                        else if (unityToneMap.mode.value == TonemappingMode.None)
                        {
                            unityToneMap.mode.value = TonemappingMode.Neutral;
                            currentMap = "Neutral";
                        }
                        //enableunityColorControls = enableunityColorControls ? false : true;
                    }
                    GUI.Label(new Rect(10, 10 + offsetY, sliderWidth, 30), currentMap);
                }

                //Unity bloom Effect
                offsetY += offsetYGap;
                if (GUI.Button(new Rect(10 + buttonsXGap, 10 + offsetY, sliderWidth, 30), "Unity Bloom"))
                {
                    enableunityBloomControls = enableunityBloomControls ? false : true;
                }
                _postProcessVolume.profile.TryGet(out unityBloom); //IF IN PLAY instantiate
                if (unityBloom != null)
                {
                    unityBloom.intensity.value = GUI.HorizontalSlider(new Rect(10, 10 + offsetY, sliderWidth, 30), unityBloom.intensity.value, 0.01f, 0.65f);
                    if (enableunityBloomControls)
                    {
                        unityBloom.threshold.value =
                           GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 2, 10 + offsetY, sliderWidth, 30), unityBloom.threshold.value, 0f, 2);
                        unityBloom.scatter.value =
                           GUI.HorizontalSlider(new Rect(10 + buttonsXGap * 3, 10 + offsetY, sliderWidth, 30), unityBloom.scatter.value, 0.4f, 1);
                    }
                }


            }//END PLAYING CHECK
        }//END ON GUI

        string currentMap = "Neutral";

        ScreenSpaceSunShaftsVol sunShaftsVolume;
        VolumeFogFxVol fogVolume;
        LensEffectsVolFxVol lensEffectsVolume;
        PainterlyVolFxVol painterlyVolume;
        BloomVol bloom;
        AdjustmentsVol adjustemetsColor;
        GrayscaleVol grayScale;
        BlurVol blur;
        PixelationVol pixelator;
        VhsVol vhsVideoEffect;
        AsciiVol asciiEffect;
        OutlineVol outline;
        FlowVol flowEffect;
        GradientMapVol gradientVol;
        OldMovieVol oldMovieEffect;
        SharpVol sharpener;
        RainLiteVol rainEffect;
        PencilVolFxVol pencilEffect;
        WaterColorVolFxVol waterEffect;
        StreaksVolFxVol streak;
        LensFlareVolFxVol lensFlares;
        HazyBloomVolFxVol hazyBloom;
        CloudShadowsVol cloudShadows;
        SketchVolFxVol sketchEffects;

        UnityEngine.Rendering.Universal.Bloom unityBloom;
        DepthOfField unityDOF;
        FilmGrain unityGrain;
        Tonemapping unityToneMap;
        ColorAdjustments colorAdjust;

        bool enableSunShaftsControls = false;
        bool enablefogVolumeControls = false;
        bool enablelensEffectsControls = false;
        bool enablepainterlyControls = false;

        bool enablebloomControls = false;
        bool enableadjustemetsColorControls = false;
        bool enablegrayScaleControls = false;
        bool enableblurControls = false;
        bool enablepixelatorControls = false;
        bool enablevhsVideoEffectControls = false;
        bool enableasciiEffectControls = false;
        bool enableoutlineControls = false;
        bool enableflowEffectControls = false;
        bool enablegradientVolControls = false;
        bool enableoldMovieEffectControls = false;
        bool enablesharpenerControls = false;
        bool enablerainEffectControls = false;
        bool enablepencilEffectControls = false;
        bool enablewaterEffectControls = false;
        bool enablestreakControls = false;
        bool enablelensFlaresControls = false;
        bool enablehazyBloomControls = false;
        bool enablecloudShadowsControls = false;
        bool enablesketchEffectsControls = false;

        bool enableunityBloomControls = false;
        bool enableunityDOFControls = false;
        bool enableunityGrainControls = false;
        bool enableunityToneMapControls = false;
        bool enableunityColorControls = false;

    }
}