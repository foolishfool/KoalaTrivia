/*
* PIDI - Planar Reflections™ 3 - Copyright© 2017-2020
* PIDI - Planar Reflections is a trademark and copyrighted property of Jorge Pinal Negrete.

* You cannot sell, redistribute, share nor make public this code, modified or not, in part nor in whole, through any
* means on any platform except with the purpose of contacting the developers to request support and only when taking
* all pertinent measures to avoid its release to the public and / or any unrelated third parties.
* Modifications are allowed only for internal use within the limits of your Unity based projects and cannot be shared,
* published, redistributed nor made available to any third parties unrelated to Irreverent Software by any means.
*
* For more information, contact us at support@irreverent-software.com
*
*/
#define PLANAR3_PRO

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PlanarReflections3 {
#if UNITY_2018_3_OR_NEWER
    [ExecuteAlways]
#else
    [ExecuteInEditMode]
#endif
    public class PlanarReflectionsCaster : MonoBehaviour {


        public static readonly int _reflectionTex = Shader.PropertyToID( "_ReflectionTex" );
        public static readonly int _reflectionDepth = Shader.PropertyToID( "_ReflectionDepth" );
        public static readonly int _blurReflectionTex = Shader.PropertyToID( "_BlurReflectionTex" );


        [System.Serializable]
        public struct BlurSettings {

            [System.NonSerialized] public RenderTexture blurredMap;
            public bool useBlur;
            public int blurPassMode;
            public float blurRadius;
            public int blurDownscale;
        }


        public PlanarReflectionsRenderer castFromRenderer;
        public Material BlurMaterial;
        public bool[] castDepth = new bool[0];
        public bool[] castReflection = new bool[0];
        public BlurSettings[] blurSettings = new BlurSettings[0];

        /// <summary> Whether this reflection renderer will work in SRP mode or not </summary>
        [SerializeField] protected bool SRPMode;

        public bool isSRP { get { return SRPMode; } }

#if UNITY_EDITOR
        public bool[] folds = new bool[16];
        private string version = "3.8.0";
        public string Version { get { return version; } }
#endif

        private Renderer rend;
        private Material[] sharedMats = new Material[0];

        private MaterialPropertyBlock mBlock;


        public void OnEnable() {

#if UNITY_EDITOR
            BlurMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>( UnityEditor.AssetDatabase.GUIDToAssetPath( UnityEditor.AssetDatabase.FindAssets( "PlanarReflections3_InternalBlur" )[0] ) );
#endif

            rend = GetComponent<Renderer>();

            if ( castDepth.Length != rend.sharedMaterials.Length )
                castDepth = new bool[rend.sharedMaterials.Length];

            if ( castReflection.Length != castDepth.Length )
                castReflection = new bool[castDepth.Length];

            if ( blurSettings.Length != castDepth.Length ) {
                blurSettings = new BlurSettings[castDepth.Length];
            }

            for ( int i = 0; i < blurSettings.Length; i++ ) {
                blurSettings[i].blurDownscale = Mathf.Clamp( blurSettings[i].blurDownscale, 1, 4 );
            }

#if UNITY_2019_1_OR_NEWER
            if ( UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null ) {
                SRPMode = true;
            }
            else {
                SRPMode = false;
            }
#endif
            if ( SRPMode ) {
#if UNITY_2019_1_OR_NEWER
                UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= CheckSRPVisibility;
                UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering += CheckSRPVisibility;

                UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= ApplySRPReflections;
                UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering += ApplySRPReflections;

                UnityEngine.Rendering.RenderPipelineManager.endCameraRendering -= BlackSRPMaterial;
                UnityEngine.Rendering.RenderPipelineManager.endCameraRendering += BlackSRPMaterial;
                //this.gameObject.layer = 4;
#endif
            }
            else {
                Camera.onPreCull -= CheckVisibility;
                Camera.onPreCull += CheckVisibility;
                Camera.onPreRender -= ApplyReflections;
                Camera.onPreRender += ApplyReflections;
            }
        }



        public IEnumerator Start() {



            if ( SRPMode ) {
#if UNITY_2019_1_OR_NEWER
                UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= CheckSRPVisibility;
                UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering += CheckSRPVisibility;

                UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= ApplySRPReflections;
                UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering += ApplySRPReflections;

                UnityEngine.Rendering.RenderPipelineManager.endCameraRendering -= BlackSRPMaterial;
                UnityEngine.Rendering.RenderPipelineManager.endCameraRendering += BlackSRPMaterial;



#endif
            }
            else {
                Camera.onPreCull -= CheckVisibility;
                Camera.onPreCull += CheckVisibility;
                Camera.onPreRender -= ApplyReflections;
                Camera.onPreRender += ApplyReflections;
            }

            yield break;

        }


#if UNITY_2019_1_OR_NEWER
        public void CheckSRPVisibility( UnityEngine.Rendering.ScriptableRenderContext context, Camera srcCamera ) {

            CheckVisibility( srcCamera );

        }

        public void ApplySRPReflections( UnityEngine.Rendering.ScriptableRenderContext context, Camera srcCamera ) {
            //foreach ( Camera cam in srcCamera )
            ApplyReflections( srcCamera );
        }


        public void BlackSRPMaterial( UnityEngine.Rendering.ScriptableRenderContext context, Camera srcCamera ) {
            if ( mBlock == null ) {
                mBlock = new MaterialPropertyBlock();
            }

            for ( int i = 0; i < castReflection.Length; i++ ) {
                var m = mBlock;
                m.SetTexture( "_BlurReflectionTex", (Texture)Texture2D.blackTexture );
                m.SetTexture( "_ReflectionTex", (Texture)Texture2D.blackTexture );
                SetPropertyBlock( m, i );
            }
        }
#endif


        public bool IsVisibleFrom( Camera camera ) {

            if ( !rend ) {
                return false;
            }

            Plane[] planes = GeometryUtility.CalculateFrustumPlanes( camera );
            return GeometryUtility.TestPlanesAABB( planes, rend.bounds );
        }


        public void CheckVisibility( Camera cam ) {


            if ( castFromRenderer && IsVisibleFrom( cam ) ) {
                castFromRenderer.castersActive = true;

            }
        }


        public void ApplyReflections( Camera cam ) {

            if ( mBlock == null ) {
                mBlock = new MaterialPropertyBlock();
            }

            var voidCam = false;

            if ( cam.cameraType == CameraType.Reflection || ( cam.cameraType == CameraType.Game && cam.gameObject.hideFlags != HideFlags.None ) ) {
                voidCam = true;
            }

            if ( !castFromRenderer ) {
                return;
            }

            for ( int i = 0; i < castReflection.Length; i++ ) {
                var m = mBlock;

                GetPropertyBlock( ref m, i );

                Texture rTex = null;

                if ( SRPMode ) {

                    if ( cam.cameraType == CameraType.Game && castFromRenderer.gameReflectors.ContainsKey( cam ) ) {
                        if ( castFromRenderer.gameReflectors[cam].refCamera )
                            rTex = castFromRenderer.gameReflectors[cam].refCamera.targetTexture;
                    }
                    else if ( cam.cameraType == CameraType.SceneView && castFromRenderer.sceneViewReflector.refCamera ) {
                        rTex = castFromRenderer.sceneViewReflector.refCamera.targetTexture;
                    }
                    else {
                        m.SetTexture( _blurReflectionTex, (Texture)Texture2D.blackTexture );
                        m.SetTexture( _reflectionTex, (Texture)Texture2D.blackTexture );
                        SetPropertyBlock( m, i );
                        return;
                    }
                }
                else {
                    rTex = castFromRenderer.ReflectionTex;
                }

                if ( blurSettings[i].useBlur && castReflection[i] && rTex ) {

                    if ( cam.cameraType != CameraType.Reflection ) {

                        var rd = new RenderTextureDescriptor( rTex.width / blurSettings[i].blurDownscale, rTex.height / blurSettings[i].blurDownscale );
                        rd.sRGB = false;

                        if ( !blurSettings[i].blurredMap ) {
                            blurSettings[i].blurredMap = RenderTexture.GetTemporary( rd );
                        }
                        else if ( blurSettings[i].blurredMap.width != rTex.width / blurSettings[i].blurDownscale || blurSettings[i].blurredMap.height != rTex.height / blurSettings[i].blurDownscale ) {
#if UNITY_2018_3_OR_NEWER
                            RenderTexture.ReleaseTemporary( blurSettings[i].blurredMap );
#else
                                if ( blurSettings[i].blurredMap )
                                    DestroyImmediate( blurSettings[i].blurredMap );
#endif
                            blurSettings[i].blurredMap = RenderTexture.GetTemporary( rd );
                        }

                        BlurMaterial.SetFloat( "_Radius", ( blurSettings[i].blurRadius + 0.01f ) * 8 );
                        var tempRT = RenderTexture.GetTemporary( rd );
                        Graphics.Blit( rTex, blurSettings[i].blurredMap, BlurMaterial );
                        Graphics.Blit( blurSettings[i].blurredMap, tempRT, BlurMaterial );
                        Graphics.Blit( tempRT, blurSettings[i].blurredMap, BlurMaterial );
                        RenderTexture.ReleaseTemporary( tempRT );

                    }

                    if ( blurSettings[i].blurPassMode == 0 ) {
                        m.SetTexture(_reflectionTex, !voidCam ? blurSettings[i].blurredMap : (Texture)Texture2D.blackTexture );
                    }
                    else {
                        m.SetTexture( _blurReflectionTex, !voidCam ? blurSettings[i].blurredMap : (Texture)Texture2D.blackTexture );
                        m.SetTexture( _reflectionTex, rTex && castReflection[i] && ( !voidCam ) ? rTex : (Texture)Texture2D.blackTexture );
                    }
                }
                else {
                    m.SetTexture( _blurReflectionTex, (Texture)Texture2D.blackTexture );
                    m.SetTexture( _reflectionTex, rTex && castReflection[i] && ( !voidCam ) ? rTex : (Texture)Texture2D.blackTexture );
                }

                m.SetTexture( _reflectionDepth, castFromRenderer.ReflectionDepth && castDepth[i] && ( !voidCam ) ? castFromRenderer.ReflectionDepth : (Texture)Texture2D.whiteTexture );


                SetPropertyBlock( m, i );

            }
        }


        public void OnDisable() {

            for ( int i = 0; i < blurSettings.Length; i++ ) {
#if UNITY_2018_3_OR_NEWER
                RenderTexture.ReleaseTemporary( blurSettings[i].blurredMap );
#else
                if ( blurSettings[i].blurredMap )
                    DestroyImmediate( blurSettings[i].blurredMap );
#endif
            }

            if ( SRPMode ) {
#if UNITY_2019_1_OR_NEWER
                UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= CheckSRPVisibility;
                UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= ApplySRPReflections;
                UnityEngine.Rendering.RenderPipelineManager.endCameraRendering -= BlackSRPMaterial;
#endif
            }
            else {
                Camera.onPreCull -= CheckVisibility;
                Camera.onPreRender -= ApplyReflections;
            }
        }


        void GetPropertyBlock( ref MaterialPropertyBlock block, int index ) {
            if ( !rend ) {
                rend = GetComponent<Renderer>();
            }

            if ( rend && sharedMats.Length < 1 ) {
                sharedMats = rend.sharedMaterials;
            }

            if ( block == null || index < 0 || !rend || sharedMats.Length <= index ) {
                return;
            }
            else {
#if UNITY_2018_1_OR_NEWER
                rend.GetPropertyBlock( block, index );
#else
                sharedMats = rend.sharedMaterials;
                var t = sharedMats[0];
                sharedMats[0] = sharedMats[index];
                rend.sharedMaterials = sharedMats;
                rend.GetPropertyBlock( block );
                sharedMats[0] = t;
                rend.sharedMaterials = sharedMats;
#endif
            }
        }


        void SetPropertyBlock( MaterialPropertyBlock block, int index ) {

            if ( !rend ) {
                rend = GetComponent<Renderer>();
            }

            if ( rend && sharedMats.Length < 1 ) {
                sharedMats = rend.sharedMaterials;
            }

            if ( block == null || index < 0 || !rend || sharedMats.Length <= index ) {
                return;
            }
            else {

#if UNITY_2018_1_OR_NEWER
                rend.SetPropertyBlock( block, index );
#else
                sharedMats = rend.sharedMaterials;
                var t = sharedMats[0];
                sharedMats[0] = sharedMats[index];
                rend.sharedMaterials = sharedMats;
                rend.SetPropertyBlock( block );
                sharedMats[0] = t;
                rend.sharedMaterials = sharedMats;
#endif

            }
        }


    }

}