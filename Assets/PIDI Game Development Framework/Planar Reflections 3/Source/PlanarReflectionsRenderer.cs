#define PLANAR3_PRO
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


#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
#if PLANAR3_PRO && UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif

using System.IO;
using System.Collections.Generic;
#if PLANAR3_URP
using UnityEngine.Rendering.Universal;
#endif
#if PLANAR3_HDRP
using UnityEngine.Rendering.HighDefinition;
#endif


namespace PlanarReflections3 {

    public enum ResolutionMode { ScreenBased, ExplicitValue }

    public enum ReflectionClipMode { AccurateClipping, SimpleApproximation }

    public enum PostFXSettingsMode { CopyFromCamera, CustomSettings }


    [System.Serializable]
    public class InternalReflectionRenderer {

        /// <summary> The internal camera that actually renders the reflection </summary>
        public Camera refCamera;

#if PLANAR3_URP
        public UnityEngine.Rendering.Universal.UniversalAdditionalCameraData camData;
#endif

#if PLANAR3_HDRP
        public UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData camData;
#endif

#if PLANAR3_PRO || PLANAR3_LWRP || PLANAR3_URP || PLANAR3_HDRP

        public RenderTexture assignedTexture;

        /// <summary> In SRP mode, the camera that renders the reflection's depth </summary>
        public Camera depthCamera;

#endif

    }

    [System.Serializable]
    public class ReflectionSettings {

        /// <summary> The way in which the renderer's resolution is handled, either based on the screen resolution or a manual value </summary>
        public ResolutionMode resolutionMode = ResolutionMode.ScreenBased;

        /// <summary> The way the reflection's projection and clipping will be handled, either with an accurate clipping limited to the surface of the plane or with a simplified approximation </summary>
        public ReflectionClipMode reflectionClipMode = ReflectionClipMode.AccurateClipping;

        /// <summary> The explicit resolution to be assigned to this reflection </summary>
        public Vector2 explicitResolution = new Vector2( 1024, 1024 );

        /// <summary> The final scale for the resolution to be multiplied by </summary>
        public float resolutionDownscale = 0.5f;

        /// <summary> The amount to frames to wait before the reflection is re-drawn and updated </summary>
        public int targetFramerate = 0;

        ///<summary> If enabled, additional components attached to this reflection Renderer will be added to the reflection itself </summary>
        public bool useCustomComponents;

        ///<summary> The list of components, as strings, that will be tracked and synchronized </summary>
        public string[] customComponentNames = new string[0];

        /// <summary> If enabled, a specified array of custom components added to this reflective instance will be synched automatically to all the rendered reflections </summary>
        public bool autoSynchComponents = false;

        /// <summary> The near clip distance of the reflection's renderer </summary>
        public float nearClipDistance = 0.05f;

        /// <summary> The far clip distance of the reflection's renderer </summary>
        public float farClipDistance = 100.0f;

        /// <summary> Whether the shadow distance from the Quality Settings should be overriden by this reflection </summary>
        public bool customShadowDistance = false;

        /// <summary> Whether this reflection will be updated only while a Reflection Caster on the scene is using its output texture </summary>
        public bool updateOnCastOnly = false;

        /// <summary> The distance in which shadows are rendered for this reflection </summary>
        public float shadowDistance = 25.0f;

        /// <summary> The rendering path used by the reflection renderer </summary>
        public RenderingPath renderingPath = RenderingPath.Forward;

        ///<summary> Sets the amount of pixel lights that this reflection will render. If set to -1, the number of pixel lights set on the Quality Settings will be used </summary>
        public int pixelLights = -1;

        public bool useMipMaps = true;

        public bool useAntialiasing = false;

        public bool useOcclusionCulling = false;

        public bool useCustomClearFlags = false;

        public int clearFlags = 0;

        public Color backgroundColor = Color.blue;

#if PLANAR3_PRO

        ///<summary> Whether this reflection will support post-process effects </summary>
        public bool usePostFX = false;

        public PostFXSettingsMode PostFXSettingsMode;

        public bool forceFloatOutput = false;

        ///<summary> Whether this reflection should render the depth pass or not </summary>
        public bool useDepth = false;

#endif

        ///<summary> The layers that this reflection will render </summary>
        public LayerMask reflectLayers = 1;

        /// <summary> If enabled, only cameras with a certain tag will trigger the rendering process of this reflection </summary>
        public bool trackCamerasWithTag = false;

        /// <summary> The tag to look for if the "trackCamerasWithTag" setting is enabled </summary>
        public string CamerasTag = "MainCamera";


#if UNITY_2019_3_OR_NEWER && (PLANAR3_PRO && (PLANAR3_URP || PLANAR3_HDRP))
        //UNIVERSAL RP & HDRP DEFINITIONS GO HERE
#elif UNITY_2019_1_OR_NEWER && (PLANAR3_PRO || PLANAR3_LWRP || PLANAR3_URP)
        //LWRP DEFINITIONS GO HERE
#endif

    }

#if UNITY_2018_3_OR_NEWER
    [ExecuteAlways]
#else
    [ExecuteInEditMode]
#endif
    public class PlanarReflectionsRenderer : MonoBehaviour {

#if UNITY_EDITOR

        public Texture2D sceneIcon;

        public bool displaySceneReflector = true;

        public Mesh defaultReflectorMesh;

        public Material defaultReflectorMaterial;
#if UNITY_2019_1_OR_NEWER && (PLANAR3_PRO || PLANAR3_LWRP || PLANAR3_URP || PLANAR3_HDRP)
        public Material defaultSRPReflectorMaterial;
#endif

        public bool[] folds = new bool[16];

#endif

        private string version = "3.8.0";

        private float frameTime;

        ///<summary> The current version of the PlanarReflectionsRenderer component </summary>
        public string Version { get { return version; } }

        /// <summary> The static reflection settings shared across all reflection renderers </summary>
        protected static ReflectionSettings globalReflectionSettings = new ReflectionSettings();

        /// <summary> The static reflection settings shared across all reflection renderers </summary>
        public ReflectionSettings GlobalSettings { get { return globalReflectionSettings; } }

        /// <summary> The specific settings of this reflection renderer </summary>
        [SerializeField] protected ReflectionSettings settings = new ReflectionSettings();

        /// <summary> The specific settings of this reflection renderer </summary>
        public ReflectionSettings Settings { get { return settings; } }

        /// <summary> An external RenderTextureAsset that will store the rendered reflection's texture </summary>
        [SerializeField] protected RenderTexture externalReflectionTex;

#if UNITY_EDITOR
        /// <summary> An external RenderTextureAsset that will store the rendered reflection's texture </summary>
        public RenderTexture ExternalReflectionTex { set { externalReflectionTex = value; } get { return externalReflectionTex; } }
#endif

        /// <summary> An external RenderTextureAsset that will store the rendered reflection's depth </summary>
        [SerializeField] protected RenderTexture externalReflectionDepth;

#if UNITY_EDITOR
        /// <summary> An external RenderTextureAsset that will store the rendered reflection's depth </summary>
        public RenderTexture ExternalReflectionDepth { set { externalReflectionDepth = value; } get { return externalReflectionDepth; } }
#endif

        /// <summary> An internal RenderTexture generated to store the rendered reflection's texture </summary>
        protected RenderTexture reflectionTex;

        /// <summary> The output reflection texture that can be used by the Planar Reflection Casters  </summary>
        public RenderTexture ReflectionTex { get { return externalReflectionTex ? externalReflectionTex : reflectionTex; } }

        /// <summary> An internal RenderTexture generated to store the rendered reflection's depth </summary>
        protected RenderTexture reflectionDepth;

        /// <summary> the output reflection depth texture that can be used by the Planar Reflection Casters </summary>
        public RenderTexture ReflectionDepth { get { return externalReflectionDepth ? externalReflectionDepth : reflectionDepth; } }

        /// <summary> Whether this reflection renderer will work in SRP mode or not </summary>
        [SerializeField] protected bool SRPMode;

        public Shader internalDepthShader;

        public bool isSRP { get { return SRPMode; } }

        public bool castersActive;

        public Dictionary<Camera, InternalReflectionRenderer> gameReflectors = new Dictionary<Camera, InternalReflectionRenderer>();

        public InternalReflectionRenderer sceneViewReflector = new InternalReflectionRenderer();

        [SerializeField] protected List<RenderTexture> releasables = new List<RenderTexture>();

        private List<RenderTexture> rTex = new List<RenderTexture>();

#if UNITY_EDITOR
        [MenuItem( "GameObject/Effects/Planar Reflections 3/Create Reflections Renderer", priority = -99 )]
        public static void CreateReflectionsRendererObject() {

            var reflector = new GameObject( "Reflection Renderer", typeof( PlanarReflectionsRenderer ) );
            reflector.transform.position = Vector3.zero;
            reflector.transform.rotation = Quaternion.identity;

        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded() {
            var cams = Resources.FindObjectsOfTypeAll<Camera>();

            foreach ( Camera cam in cams ) {
                if ( cam.name.Contains( "PLANAR3_" ) && cam.cameraType == CameraType.Reflection ) {
#if !UNITY_2018_3_OR_NEWER
                    DestroyImmediate( cam.targetTexture );
#else
                    RenderTexture.ReleaseTemporary( cam.targetTexture );
#endif
                    cam.targetTexture = null;
                    DestroyImmediate( cam.gameObject );
                }
            }
        }

#endif




        public void OnEnable() {

#if UNITY_EDITOR

#if UNITY_2018_1_OR_NEWER
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
            UnityEditor.SceneView.RepaintAll();
#endif

            if ( AssetDatabase.FindAssets( "Planar3Logo_Gizmos" ).Length < 1 ) {

                if ( !sceneIcon ) {
                    sceneIcon = AssetDatabase.LoadAssetAtPath<Texture2D>( AssetDatabase.GUIDToAssetPath( AssetDatabase.FindAssets("l: Pidi_PlanarGizmos")[0] ) );
                }

                if ( !AssetDatabase.IsValidFolder( "Assets/Gizmos" ) )
                    AssetDatabase.CreateFolder( "Assets", "Gizmos" );
                var t = new Texture2D( sceneIcon.width, sceneIcon.height );
                t.SetPixels( sceneIcon.GetPixels() );
                File.WriteAllBytes( Application.dataPath + "/Gizmos/Planar3Logo_Gizmos.png", t.EncodeToPNG() );
                AssetDatabase.Refresh();
                var importer = (TextureImporter)AssetImporter.GetAtPath( "Assets/Gizmos/Planar3Logo_Gizmos.png" );
                importer.isReadable = true;
                importer.textureType = TextureImporterType.GUI;
                importer.SaveAndReimport();
                AssetDatabase.Refresh();
            }
#endif



            var cams = Resources.FindObjectsOfTypeAll<Camera>();

            foreach ( Camera cam in cams ) {
                if ( cam.name.Contains( "PLANAR3_" ) && !SRPMode ) {

                    if ( cam.targetTexture != externalReflectionTex ) {
#if !UNITY_2018_3_OR_NEWER
                        DestroyImmediate( cam.targetTexture );
#else
                        RenderTexture.ReleaseTemporary( cam.targetTexture );
#endif
                    }
                    cam.targetTexture = null;

                }
            }

#if UNITY_2019_1_OR_NEWER && ( PLANAR3_PRO || PLANAR3_LWRP || PLANAR3_URP || PLANAR3_HDRP )
            if ( UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null ) {
                SRPMode = true;
            }
            else {
                SRPMode = false;
            }
#endif



            if ( SRPMode ) {
#if UNITY_2019_1_OR_NEWER && ( PLANAR3_PRO || PLANAR3_LWRP || PLANAR3_URP || PLANAR3_HDRP )



                foreach ( Camera cam in cams ) {
                    if ( cam.name.Contains( "PLANAR3_" ) ) {
                        cam.targetTexture = null;
                        if ( !Application.isPlaying )
                            DestroyImmediate( cam.gameObject );
                        else
                            Destroy( cam.gameObject );
                    }
                }

                UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= SRPRenderReflection;
                UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering += SRPRenderReflection;

                UnityEngine.Rendering.RenderPipelineManager.endFrameRendering -= VisibilityDisable;
                UnityEngine.Rendering.RenderPipelineManager.endFrameRendering += VisibilityDisable;

                UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= CleanupTextures;
                UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering += CleanupTextures;

                this.gameObject.layer = 4;
#endif
            }
            else {
                Camera.onPreCull -= RenderReflection;
                Camera.onPreCull += RenderReflection;
                this.gameObject.layer = 4;
            }


        }


        System.Collections.IEnumerator Start() {
            var cams = Resources.FindObjectsOfTypeAll<Camera>();

            foreach ( Camera cam in cams ) {
                if ( cam.name.Contains( "PLANAR3_" ) && !SRPMode ) {
#if !UNITY_2018_3_OR_NEWER
                    DestroyImmediate( cam.targetTexture );
#else
                    RenderTexture.ReleaseTemporary( cam.targetTexture );
#endif
                    cam.targetTexture = null;
                    DestroyImmediate( cam.gameObject );
                }
            }

#if UNITY_2019_1_OR_NEWER && ( PLANAR3_PRO || PLANAR3_LWRP || PLANAR3_URP || PLANAR3_HDRP )
            if ( UnityEngine.Rendering.GraphicsSettings.renderPipelineAsset != null ) {
                SRPMode = true;
            }
            else {
                SRPMode = false;
            }
#endif



            if ( SRPMode ) {
#if UNITY_2019_1_OR_NEWER && ( PLANAR3_PRO || PLANAR3_LWRP || PLANAR3_URP || PLANAR3_HDRP )



                foreach ( Camera cam in cams ) {
                    if ( cam.name.Contains( "PLANAR3_" ) ) {
                        cam.targetTexture = null;
                        DestroyImmediate( cam.gameObject );
                    }
                }

                UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= SRPRenderReflection;
                UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering += SRPRenderReflection;

                UnityEngine.Rendering.RenderPipelineManager.endFrameRendering -= VisibilityDisable;
                UnityEngine.Rendering.RenderPipelineManager.endFrameRendering += VisibilityDisable;

                UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= CleanupTextures;
                UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering += CleanupTextures;


                this.gameObject.layer = 4;

               


#endif
            }
            else {
                Camera.onPreCull -= RenderReflection;
                Camera.onPreCull += RenderReflection;
            }

            yield break;

        }




#if UNITY_EDITOR
        private void DrawReflectorMesh( Camera sceneCam ) {

            if ( !displaySceneReflector )
                return;

            var matrix = new Matrix4x4();
            matrix.SetTRS( transform.position, transform.rotation, Vector3.one * 10 );

            if ( SRPMode ) {
#if UNITY_2019_1_OR_NEWER && (PLANAR3_PRO || PLANAR3_LWRP || PLANAR3_URP || PLANAR3_HDRP)
                if ( !defaultSRPReflectorMaterial ) {
                    defaultSRPReflectorMaterial = AssetDatabase.LoadAssetAtPath<Material>( AssetDatabase.GUIDToAssetPath( AssetDatabase.FindAssets( "Planar3_DefaultSRPReflectorMaterial" )[0] ) );
                }
                var mBlock = new MaterialPropertyBlock();

                mBlock.SetTexture( "_ReflectionTex", ReflectionTex );
                Graphics.DrawMesh( defaultReflectorMesh, matrix, defaultSRPReflectorMaterial, 0, sceneCam, 0, mBlock );
#endif
            }
            else {
                if ( !defaultReflectorMaterial ) {
                    defaultReflectorMaterial = AssetDatabase.LoadAssetAtPath<Material>( AssetDatabase.GUIDToAssetPath( AssetDatabase.FindAssets( "Planar3_DefaultReflectorMaterial" )[0] ) );
                }
                var mBlock = new MaterialPropertyBlock();

                mBlock.SetTexture( "_ReflectionTex", ReflectionTex );
                Graphics.DrawMesh( defaultReflectorMesh, matrix, defaultReflectorMaterial, 0, sceneCam, 0, mBlock );
            }
        }
#endif

#if UNITY_2019_1_OR_NEWER && ( PLANAR3_PRO || PLANAR3_LWRP || PLANAR3_URP || PLANAR3_HDRP )

        public void VisibilityDisable( UnityEngine.Rendering.ScriptableRenderContext cotnext, Camera[] cameras ) {
            castersActive = false;
        }


        public void SRPRenderReflection( UnityEngine.Rendering.ScriptableRenderContext context, Camera srcCamera ) {

            if ( Settings.updateOnCastOnly && !castersActive && Application.isPlaying ) {
                return;
            }

            if ( Settings.trackCamerasWithTag ) {
                if ( srcCamera.tag != Settings.CamerasTag && srcCamera.cameraType != CameraType.SceneView ) {
                    return;
                }
            }

            if ( srcCamera.cameraType == CameraType.Reflection || ( srcCamera.cameraType == CameraType.Game && srcCamera.gameObject.hideFlags != HideFlags.None ) ) {
                return;
            }

            InternalReflectionRenderer currentReflector = null;

            if ( srcCamera.cameraType == CameraType.SceneView ) {

                if ( Application.isPlaying && SRPMode ) {
                    if ( sceneViewReflector.refCamera )
                        sceneViewReflector.refCamera.enabled = false;
                    return;
                }

                if ( !sceneViewReflector.refCamera ) {
                    sceneViewReflector.refCamera = new GameObject( "PLANAR3_SCENEVIEW", typeof( Camera )
#if PLANAR3_URP
                    ,typeof(UniversalAdditionalCameraData)
#elif PLANAR3_HDRP
                    ,typeof(UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData)
#endif
                        ).GetComponent<Camera>();
                    sceneViewReflector.refCamera.gameObject.hideFlags = sceneViewReflector.refCamera.hideFlags = HideFlags.HideAndDontSave;
                }

                currentReflector = sceneViewReflector;


            }
            else if ( srcCamera.cameraType == CameraType.Game ) {

                if ( !gameReflectors.ContainsKey( srcCamera ) ) {
                    gameReflectors.Add( srcCamera, new InternalReflectionRenderer() );
                }
                else if ( gameReflectors[srcCamera] == null ) {
                    gameReflectors[srcCamera] = new InternalReflectionRenderer();
                }



                if ( !gameReflectors[srcCamera].refCamera ) {
                    gameReflectors[srcCamera].refCamera = new GameObject( "PLANAR3_GAMECAM_" + srcCamera.GetInstanceID(), typeof( Camera )
#if PLANAR3_URP
                    , typeof( UniversalAdditionalCameraData )
#elif PLANAR3_HDRP
                    ,typeof(UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData)
#endif
                        ).GetComponent<Camera>();
                    gameReflectors[srcCamera].refCamera.gameObject.hideFlags = gameReflectors[srcCamera].refCamera.hideFlags = HideFlags.HideAndDontSave;
                }
                else {
                    gameReflectors[srcCamera].refCamera.gameObject.hideFlags = gameReflectors[srcCamera].refCamera.hideFlags = HideFlags.HideAndDontSave;
                }

                currentReflector = gameReflectors[srcCamera];



            }

            if ( currentReflector == null ) {
                return;
            }

            currentReflector.refCamera.CopyFrom( srcCamera );

            if ( Settings.useCustomClearFlags ) {
                currentReflector.refCamera.clearFlags = Settings.clearFlags==0?CameraClearFlags.Skybox:CameraClearFlags.Color;
                currentReflector.refCamera.backgroundColor = Settings.backgroundColor;

#if PLANAR3_HDRP
                if ( currentReflector.camData ) {
                    currentReflector.camData.clearColorMode = Settings.clearFlags == 0 ? HDAdditionalCameraData.ClearColorMode.Sky : HDAdditionalCameraData.ClearColorMode.Color;
                    currentReflector.camData.backgroundColorHDR = Settings.backgroundColor;
                }
#endif

            }

            if (!SRPMode)
                currentReflector.refCamera.cameraType = CameraType.Reflection;
            else {
                currentReflector.refCamera.cameraType = CameraType.Game;
            }


            currentReflector.refCamera.useOcclusionCulling = Settings.useOcclusionCulling;


            if ( currentReflector.refCamera.clearFlags == CameraClearFlags.Nothing || currentReflector.refCamera.clearFlags == CameraClearFlags.Depth ) {
                currentReflector.refCamera.clearFlags = CameraClearFlags.Color;
            }


            if ( !currentReflector.assignedTexture ) {
                int width = (int)( ( Settings.resolutionMode == ResolutionMode.ExplicitValue ? Settings.explicitResolution.x : srcCamera.pixelWidth ) * Settings.resolutionDownscale );
                int height = (int)( ( Settings.resolutionMode == ResolutionMode.ExplicitValue ? Settings.explicitResolution.y : srcCamera.pixelHeight ) * Settings.resolutionDownscale );

#if PLANAR3_PRO
                if ( settings.forceFloatOutput && settings.usePostFX )
                    reflectionTex = RenderTexture.GetTemporary( new RenderTextureDescriptor( width, height, RenderTextureFormat.ARGBFloat, 24 ) );
                else
                    reflectionTex = RenderTexture.GetTemporary( new RenderTextureDescriptor( width, height, RenderTextureFormat.Default, 24 ) );
#else
                        reflectionTex = RenderTexture.GetTemporary( new RenderTextureDescriptor( width, height, RenderTextureFormat.Default, 24 ) );
#endif

                reflectionTex.name = "PIDI_REFTEX" + ( srcCamera.cameraType == CameraType.SceneView ? "SCENE" : "GAME" );
                if ( !rTex.Contains( reflectionTex ) ) {
                    rTex.Add( reflectionTex );
                }

                reflectionTex.useMipMap = Settings.useMipMaps;

                if ( !reflectionTex.IsCreated() )
                    reflectionTex.antiAliasing = Settings.useAntialiasing ? 4 : 1;

                reflectionTex.Create();
                currentReflector.assignedTexture = reflectionTex;
            }
            else {



                int width = (int)( ( Settings.resolutionMode == ResolutionMode.ExplicitValue ? Settings.explicitResolution.x : srcCamera.pixelWidth ) * Settings.resolutionDownscale );
                int height = (int)( ( Settings.resolutionMode == ResolutionMode.ExplicitValue ? Settings.explicitResolution.y : srcCamera.pixelHeight ) * Settings.resolutionDownscale );

                if ( !currentReflector.assignedTexture || currentReflector.assignedTexture.width != width || currentReflector.assignedTexture.height != height ) {

                    if ( currentReflector.assignedTexture != null && !releasables.Contains( currentReflector.assignedTexture ) ) {
                        releasables.Add( currentReflector.assignedTexture );
                    }

                    currentReflector.refCamera.targetTexture = null;
#if PLANAR3_PRO
                    if ( settings.forceFloatOutput && settings.usePostFX ) {
                        reflectionTex = RenderTexture.GetTemporary( new RenderTextureDescriptor( width, height, RenderTextureFormat.ARGBFloat, 24 ) );

                        reflectionTex.name = "PIDI_REFTEX" + ( srcCamera.cameraType == CameraType.SceneView ? "SCENE" : "GAME" );
                        if ( !rTex.Contains( reflectionTex ) ) {
                            rTex.Add( reflectionTex );
                        }

                        if ( !reflectionTex.IsCreated() ) {
                            reflectionTex.useMipMap = Settings.useMipMaps;
                            reflectionTex.antiAliasing = Settings.useAntialiasing ? 4 : 1;
                            reflectionTex.Create();
                        }
                        currentReflector.assignedTexture = reflectionTex;
                    }
                    else {
                        reflectionTex = RenderTexture.GetTemporary( new RenderTextureDescriptor( width, height, RenderTextureFormat.Default, 24 ) );

#else
                                reflectionTex = RenderTexture.GetTemporary( new RenderTextureDescriptor( width, height, RenderTextureFormat.Default, 24 ) );
#endif
                        reflectionTex.name = "PIDI_REFTEX" + ( srcCamera.cameraType == CameraType.SceneView ? "SCENE" : "GAME" );
                        if ( !rTex.Contains( reflectionTex ) ) {
                            rTex.Add( reflectionTex );
                        }

                        if ( !reflectionTex.IsCreated() ) {
                            reflectionTex.useMipMap = Settings.useMipMaps;
                            reflectionTex.antiAliasing = Settings.useAntialiasing ? 4 : 1;
                            reflectionTex.Create();
                        }
                        currentReflector.assignedTexture = reflectionTex;
                    }
                }

                reflectionTex = currentReflector.assignedTexture;

            }


            reflectionTex.filterMode = FilterMode.Bilinear;

            var tempShadowDistance = QualitySettings.shadowDistance;

            if ( !SRPMode && Settings.customShadowDistance ) {
                QualitySettings.shadowDistance = Settings.shadowDistance;
            }
#if PLANAR3_URP
            else if ( currentReflector.camData ) {
                currentReflector.camData.renderShadows = Settings.shadowDistance > 0 ? srcCamera.GetUniversalAdditionalCameraData().renderShadows : false;
            }
#endif
            currentReflector.refCamera.enabled = false;

            currentReflector.refCamera.targetTexture = ReflectionTex;

            currentReflector.refCamera.aspect = srcCamera.aspect;

            currentReflector.refCamera.rect = new Rect( 0, 0, 1, 1 ); 

            Vector3 worldSpaceViewDir = srcCamera.transform.forward;
            Vector3 worldSpaceViewUp = srcCamera.transform.up;
            Vector3 worldSpaceCamPos = srcCamera.transform.position;

            Vector3 planeSpaceViewDir = transform.InverseTransformDirection( worldSpaceViewDir );
            Vector3 planeSpaceViewUp = transform.InverseTransformDirection( worldSpaceViewUp );
            Vector3 planeSpaceCamPos = transform.InverseTransformPoint( worldSpaceCamPos );

            planeSpaceViewDir.y *= -1.0f;
            planeSpaceViewUp.y *= -1.0f;
            planeSpaceCamPos.y *= -1.0f;

            worldSpaceViewDir = transform.TransformDirection( planeSpaceViewDir );
            worldSpaceViewUp = transform.TransformDirection( planeSpaceViewUp );
            worldSpaceCamPos = transform.TransformPoint( planeSpaceCamPos );

            currentReflector.refCamera.transform.position = worldSpaceCamPos;
            currentReflector.refCamera.transform.LookAt( worldSpaceCamPos + worldSpaceViewDir, worldSpaceViewUp );

            currentReflector.refCamera.nearClipPlane = Settings.nearClipDistance;
            currentReflector.refCamera.farClipPlane = Settings.farClipDistance;




            currentReflector.refCamera.renderingPath = Settings.renderingPath;

            currentReflector.refCamera.cullingMask = Settings.reflectLayers;

#if PLANAR3_PRO && UNITY_POST_PROCESSING_STACK_V2

            if ( Settings.usePostFX ) {
            
                currentReflector.refCamera.allowMSAA = false;

                PostProcessLayer refPostFX = Settings.PostFXSettingsMode == PostFXSettingsMode.CustomSettings ? GetComponent<PostProcessLayer>() : srcCamera.GetComponent<PostProcessLayer>();

                if ( refPostFX ) {
                    PostProcessLayer targetPostFX = currentReflector.refCamera.GetComponent<PostProcessLayer>();

                    if ( targetPostFX ) {
                        targetPostFX.volumeTrigger = currentReflector.refCamera.transform;
                        targetPostFX.volumeLayer = refPostFX.volumeLayer;
                        targetPostFX.antialiasingMode = refPostFX.antialiasingMode == PostProcessLayer.Antialiasing.None ? PostProcessLayer.Antialiasing.None : PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
                        targetPostFX.fastApproximateAntialiasing = refPostFX.fastApproximateAntialiasing;
                    }
                    else {
                        targetPostFX = currentReflector.refCamera.gameObject.AddComponent<PostProcessLayer>();
                        System.Reflection.BindingFlags bindFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;
                        System.Reflection.FieldInfo field = typeof( PostProcessLayer ).GetField( "m_Resources", bindFlags );
                        targetPostFX.Init( (PostProcessResources)field.GetValue( refPostFX ) );

                        targetPostFX.volumeTrigger = currentReflector.refCamera.transform;
                        targetPostFX.volumeLayer = refPostFX.volumeLayer;
                        targetPostFX.antialiasingMode = refPostFX.antialiasingMode == PostProcessLayer.Antialiasing.None ? PostProcessLayer.Antialiasing.None : PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
                        targetPostFX.fastApproximateAntialiasing = refPostFX.fastApproximateAntialiasing;

                    }
                }

            }
            else {
                if ( GetComponent<PostProcessLayer>() ) {
                    DestroyImmediate( GetComponent<PostProcessLayer>() );
                    if ( GetComponent<Camera>() ) {
                        DestroyImmediate( GetComponent<Camera>() );
                    }
                }

                if ( currentReflector.refCamera.GetComponent<PostProcessLayer>() ) {
                    DestroyImmediate( currentReflector.refCamera.GetComponent<PostProcessLayer>() );
                }
            }

#elif PLANAR3_PRO && UNITY_2019_3_OR_NEWER && ( PLANAR3_URP || PLANAR3_HDRP )

            if ( Settings.usePostFX ) {
#if PLANAR3_URP
                UnityEngine.Rendering.Universal.UniversalAdditionalCameraData refPostFX = Settings.PostFXSettingsMode == PostFXSettingsMode.CustomSettings ? GetComponent<UniversalAdditionalCameraData>() : srcCamera.GetComponent<UniversalAdditionalCameraData>();
#elif PLANAR3_HDRP
                    UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData refPostFX = Settings.PostFXSettingsMode == PostFXSettingsMode.CustomSettings ? GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>() : srcCamera.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
#endif
                if ( refPostFX ) {
#if PLANAR3_URP
                    UniversalAdditionalCameraData targetPostFX = currentReflector.camData = currentReflector.refCamera.GetUniversalAdditionalCameraData();
#elif PLANAR3_HDRP
                        UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData targetPostFX = currentReflector.camData = currentReflector.refCamera.GetComponent<HDAdditionalCameraData>();
#endif
                    if ( targetPostFX ) {
#if PLANAR3_URP
                        targetPostFX.renderPostProcessing = refPostFX.renderPostProcessing;
                        targetPostFX.volumeTrigger = srcCamera.transform;
#elif PLANAR3_HDRP
                        targetPostFX.volumeAnchorOverride = currentReflector.refCamera.transform;
#endif
                        targetPostFX.volumeLayerMask = refPostFX.volumeLayerMask;
#if PLANAR3_URP
                        targetPostFX.antialiasing = refPostFX.antialiasing == UnityEngine.Rendering.Universal.AntialiasingMode.None ? UnityEngine.Rendering.Universal.AntialiasingMode.None : UnityEngine.Rendering.Universal.AntialiasingMode.FastApproximateAntialiasing;
                    }
                    else {
                        targetPostFX = currentReflector.refCamera.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                        currentReflector.camData = targetPostFX;
                        targetPostFX.renderPostProcessing = refPostFX.renderPostProcessing;

                        targetPostFX.volumeTrigger = srcCamera.transform;
                        targetPostFX.volumeLayerMask = refPostFX.volumeLayerMask;
                        targetPostFX.antialiasing = refPostFX.antialiasing == UnityEngine.Rendering.Universal.AntialiasingMode.None ? UnityEngine.Rendering.Universal.AntialiasingMode.None : UnityEngine.Rendering.Universal.AntialiasingMode.FastApproximateAntialiasing;

                    }

                }

            }
            else {

                if ( currentReflector.camData ) {
                    currentReflector.camData.renderPostProcessing = false;
                }

            }


#elif PLANAR3_HDRP
                            targetPostFX.antialiasing = refPostFX.antialiasing == UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData.AntialiasingMode.None ? UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData.AntialiasingMode.None : UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData.AntialiasingMode.FastApproximateAntialiasing;
                        }
                        else {
                            targetPostFX = currentReflector.refCamera.gameObject.AddComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
                            currentReflector.camData = targetPostFX;

                            targetPostFX.volumeAnchorOverride = currentReflector.refCamera.transform;
                            targetPostFX.volumeLayerMask = refPostFX.volumeLayerMask;
                            targetPostFX.antialiasing = refPostFX.antialiasing == UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData.AntialiasingMode.None ? UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData.AntialiasingMode.None : UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData.AntialiasingMode.FastApproximateAntialiasing;

                        }

                    }

                }
            
#endif


#endif

            currentReflector.refCamera.transform.Rotate( 0.01f, 0.01f, 0.01f );
            currentReflector.refCamera.transform.Translate( 0.002f, 0.002f, 0.002f );

            if ( settings.reflectionClipMode == ReflectionClipMode.AccurateClipping ) {
                currentReflector.refCamera.projectionMatrix = currentReflector.refCamera.CalculateObliqueMatrix( CameraSpacePlane( currentReflector.refCamera, transform.position, transform.up ) );
            }




            if ( !Application.isPlaying || Settings.targetFramerate == 0 ) {

#if PLANAR3_URP || PLANAR3_HDRP

#if PLANAR3_URP
                if ( Settings.usePostFX && srcCamera.cameraType != CameraType.SceneView ) {
                    currentReflector.refCamera.enabled = true;
                }
                else {
                    UniversalRenderPipeline.RenderSingleCamera( context, currentReflector.refCamera );
                }

#elif PLANAR3_HDRP
                currentReflector.refCamera.enabled = true;
                //UnityEngine.Rendering.HighDefinition.HDRenderPipeline.R
                //currentReflector.refCamera.Render();
#endif


                if ( !currentReflector.camData ) {
#if PLANAR3_URP
                    if ( currentReflector.refCamera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>() ) {
                        currentReflector.camData = currentReflector.refCamera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                    }
                    else {
                        currentReflector.camData = currentReflector.refCamera.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                    }
#elif PLANAR3_HDRP
                        if ( currentReflector.refCamera.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>() ) {
                            currentReflector.camData = currentReflector.refCamera.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
                        }
                        else {
                            currentReflector.camData = currentReflector.refCamera.gameObject.AddComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
                        }
#endif
                }
#endif
            }
            else {

                if ( Time.realtimeSinceStartup > frameTime ) {

#if PLANAR3_URP
                    if ( Settings.usePostFX && Settings.PostFXSettingsMode == PostFXSettingsMode.CustomSettings && srcCamera.cameraType != CameraType.SceneView )
                        currentReflector.refCamera.enabled = true;
                    else {
                        UniversalRenderPipeline.RenderSingleCamera( context, currentReflector.refCamera );
                    }

#elif PLANAR3_HDRP
                    currentReflector.refCamera.enabled = true;
#endif


                    frameTime = Time.realtimeSinceStartup + ( 1.0f / Settings.targetFramerate );
#if PLANAR3_URP || PLANAR3_HDRP
                    if ( !currentReflector.camData ) {
#if PLANAR3_URP
                        currentReflector.camData = currentReflector.refCamera.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
#elif PLANAR3_HDRP
                            currentReflector.camData = currentReflector.refCamera.gameObject.AddComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
#endif
                    }
#if PLANAR3_URP
                    currentReflector.camData.requiresDepthOption = CameraOverrideOption.Off;
                    currentReflector.camData.requiresColorOption = CameraOverrideOption.Off;
                    currentReflector.camData.renderShadows = Settings.shadowDistance > 0?srcCamera.GetUniversalAdditionalCameraData().renderShadows:false;
#endif
                }
                else {
                    currentReflector.refCamera.enabled = false;
                    currentReflector.refCamera.allowMSAA = false;
                    if ( !currentReflector.camData ) {
#if PLANAR3_URP
                        currentReflector.camData = currentReflector.refCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();
#endif
                    }

#if PLANAR3_URP
                    currentReflector.camData.requiresDepthOption = CameraOverrideOption.Off;
                    currentReflector.camData.requiresColorOption = CameraOverrideOption.Off;
                    currentReflector.camData.renderShadows = Settings.shadowDistance > 0 ? srcCamera.GetUniversalAdditionalCameraData().renderShadows : false;
#endif

#endif
                }

            }

            currentReflector.refCamera.depth = -999;


            if ( Settings.useDepth && internalDepthShader && !SRPMode ) {
                if ( !externalReflectionDepth ) {


                    int width = (int)( ( Settings.resolutionMode == ResolutionMode.ExplicitValue ? Settings.explicitResolution.x : srcCamera.pixelWidth ) * Settings.resolutionDownscale );
                    int height = (int)( ( Settings.resolutionMode == ResolutionMode.ExplicitValue ? Settings.explicitResolution.y : srcCamera.pixelHeight ) * Settings.resolutionDownscale );


                    if ( reflectionDepth && ( reflectionDepth.width != width || reflectionDepth.height != height ) ) {
                        RenderTexture.ReleaseTemporary( reflectionDepth );
                        reflectionDepth = RenderTexture.GetTemporary( width, height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear );
                        reflectionDepth.name = "PIDI_REFDEPTH" + ( srcCamera.cameraType == CameraType.SceneView ? "SCENE" : "GAME" );
                        if ( !rTex.Contains( reflectionDepth ) ) {
                            rTex.Add( reflectionDepth );
                        }
                    }
                    else if ( !reflectionDepth ) {
                        reflectionDepth = RenderTexture.GetTemporary( width, height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear );
                        reflectionDepth.name = "PIDI_REFDEPTH" + ( srcCamera.cameraType == CameraType.SceneView ? "SCENE" : "GAME" );
                        if ( !rTex.Contains( reflectionDepth ) ) {
                            rTex.Add( reflectionDepth );
                        }
                    }


                }

                currentReflector.refCamera.targetTexture = ReflectionDepth;
                currentReflector.refCamera.backgroundColor = Color.green;
                currentReflector.refCamera.clearFlags = CameraClearFlags.Color;
                Shader.SetGlobalVector( "_Planar3DepthPlaneOrigin", new Vector4( transform.position.x, transform.position.y, transform.position.z ) );
                Shader.SetGlobalVector( "_Planar3DepthPlaneNormal", -new Vector4( transform.up.x, transform.up.y, transform.up.z ) );
                currentReflector.refCamera.renderingPath = RenderingPath.Forward;
                currentReflector.refCamera.RenderWithShader( internalDepthShader, "" );

            }


            QualitySettings.shadowDistance = tempShadowDistance;

#if UNITY_EDITOR
            if ( srcCamera.cameraType == CameraType.SceneView )
                DrawReflectorMesh( srcCamera );
#endif

        }


        public void CleanupTextures( UnityEngine.Rendering.ScriptableRenderContext context, Camera srcCamera ) {

            for ( int i = 0; i < releasables.Count; i++ ) {
                if ( releasables[i] != null ) {
                    RenderTexture.ReleaseTemporary( releasables[i] );
                    releasables.RemoveAt( i );
                    if ( i > 0 ) {
                        i--;
                    }
                }
            }
        }



#endif



        public void RenderReflection( Camera srcCamera ) {


            if ( Settings.updateOnCastOnly && !castersActive && Application.isPlaying ) {
                return;
            }

            if ( srcCamera.cameraType != CameraType.SceneView && srcCamera.cameraType != CameraType.Game ) {
                return;
            }

            if ( srcCamera.cameraType == CameraType.Game && srcCamera.hideFlags != HideFlags.None ) {
                return;
            }


            if ( Settings.trackCamerasWithTag ) {
                if ( srcCamera.tag != Settings.CamerasTag ) {
                    return;
                }
            }

            InternalReflectionRenderer currentReflector = null;


            if ( srcCamera.depthTextureMode == DepthTextureMode.None ) {
                srcCamera.depthTextureMode = DepthTextureMode.Depth;
            }


            if ( srcCamera.cameraType == CameraType.SceneView ) {

                if ( Application.isPlaying && SRPMode ) {
                    if ( sceneViewReflector.refCamera )
                        sceneViewReflector.refCamera.enabled = false;
                    return;
                }

                if ( !sceneViewReflector.refCamera ) {
                    sceneViewReflector.refCamera = new GameObject( "PLANAR3_SCENEVIEW", typeof( Camera ) ).GetComponent<Camera>();
                    sceneViewReflector.refCamera.gameObject.hideFlags = sceneViewReflector.refCamera.hideFlags = HideFlags.HideAndDontSave;
                }

                currentReflector = sceneViewReflector;


            }
            else if ( srcCamera.cameraType == CameraType.Game ) {

                if ( !gameReflectors.ContainsKey( srcCamera ) || gameReflectors[srcCamera] == null ) {
                    gameReflectors.Add( srcCamera, new InternalReflectionRenderer() );
                }


                if ( !gameReflectors[srcCamera].refCamera ) {
                    gameReflectors[srcCamera].refCamera = new GameObject( "PLANAR3_GAMECAM_" + srcCamera.GetInstanceID(), typeof( Camera ) ).GetComponent<Camera>();
                    gameReflectors[srcCamera].refCamera.gameObject.hideFlags = gameReflectors[srcCamera].refCamera.hideFlags = HideFlags.HideAndDontSave;
                }
                else {
                    gameReflectors[srcCamera].refCamera.gameObject.hideFlags = gameReflectors[srcCamera].refCamera.hideFlags = HideFlags.HideAndDontSave;
                }

                currentReflector = gameReflectors[srcCamera];



            }

            if ( currentReflector == null ) {
                return;
            }

            currentReflector.refCamera.CopyFrom( srcCamera );

            currentReflector.refCamera.enabled = false;

            currentReflector.refCamera.useOcclusionCulling = Settings.useOcclusionCulling;


            if ( Settings.useCustomClearFlags ) {
                currentReflector.refCamera.clearFlags = Settings.clearFlags == 0 ? CameraClearFlags.Skybox : CameraClearFlags.Color;
                currentReflector.refCamera.backgroundColor = Settings.backgroundColor;
            }


            if ( !SRPMode )
                currentReflector.refCamera.cameraType = CameraType.Reflection;
            else
                currentReflector.refCamera.cameraType = CameraType.Game;

            if ( currentReflector.refCamera.clearFlags == CameraClearFlags.Nothing || currentReflector.refCamera.clearFlags == CameraClearFlags.Depth ) {
                currentReflector.refCamera.clearFlags = CameraClearFlags.Color;
            }


            if ( !externalReflectionTex ) {

                if ( !Application.isPlaying || Settings.targetFramerate == 0 || Time.realtimeSinceStartup > frameTime ) {

                    int width = (int)( ( Settings.resolutionMode == ResolutionMode.ExplicitValue ? Settings.explicitResolution.x : srcCamera.pixelWidth ) * Settings.resolutionDownscale );
                    int height = (int)( ( Settings.resolutionMode == ResolutionMode.ExplicitValue ? Settings.explicitResolution.y : srcCamera.pixelHeight ) * Settings.resolutionDownscale );

                    if ( reflectionTex && ( reflectionTex.width != width || reflectionTex.height != height ) ) {
                        currentReflector.assignedTexture = null;
                        currentReflector.refCamera.targetTexture = null;
#if !UNITY_2018_1_OR_NEWER
                            DestroyImmediate( reflectionTex );
#else
                        RenderTexture.ReleaseTemporary( reflectionTex );
#endif


#if PLANAR3_PRO
                        if ( settings.forceFloatOutput && settings.usePostFX )
                            reflectionTex = RenderTexture.GetTemporary( new RenderTextureDescriptor( width, height, RenderTextureFormat.ARGBFloat, 24 ) );
                        else
                            reflectionTex = RenderTexture.GetTemporary( new RenderTextureDescriptor( width, height, RenderTextureFormat.Default, 24 ) );
#else
                    reflectionTex = RenderTexture.GetTemporary( new RenderTextureDescriptor( width, height, RenderTextureFormat.Default, 24, 3 ) );
#endif

                        reflectionTex.name = "PIDI_REFTEX" + ( srcCamera.cameraType == CameraType.SceneView ? "SCENE" : "GAME" );
                        if ( !rTex.Contains( reflectionTex ) ) {
                            rTex.Add( reflectionTex );
                        }


                        if ( !reflectionTex.IsCreated() ) {
                            reflectionTex.useMipMap = Settings.useMipMaps;
                            reflectionTex.antiAliasing = Settings.useAntialiasing ? 4 : 1;
                            reflectionTex.Create();
                        }

                    }
                    else if ( !reflectionTex ) {
#if PLANAR3_PRO
                        if ( settings.forceFloatOutput && settings.usePostFX )
                            reflectionTex = RenderTexture.GetTemporary( new RenderTextureDescriptor( width, height, RenderTextureFormat.ARGBFloat, 24 ) );
                        else
                            reflectionTex = RenderTexture.GetTemporary( new RenderTextureDescriptor( width, height, RenderTextureFormat.Default, 24 ) );
#else
                    reflectionTex = RenderTexture.GetTemporary( new RenderTextureDescriptor( width, height, RenderTextureFormat.Default, 24, 3 ) );
#endif

                        reflectionTex.name = "PIDI_REFTEX" + ( srcCamera.cameraType == CameraType.SceneView ? "SCENE" : "GAME" );
                        if ( !rTex.Contains( reflectionTex ) ) {
                            rTex.Add( reflectionTex );
                        }


                        if ( !reflectionTex.IsCreated() ) {
                            reflectionTex.useMipMap = Settings.useMipMaps;
                            reflectionTex.antiAliasing = Settings.useAntialiasing ? 4 : 1;
                            reflectionTex.Create();
                        }

                    }

                    currentReflector.assignedTexture = reflectionTex;
                }

                reflectionTex.filterMode = FilterMode.Bilinear;
            }


            var tempShadowDistance = QualitySettings.shadowDistance;

            if ( !SRPMode && Settings.customShadowDistance ) {
                QualitySettings.shadowDistance = Settings.shadowDistance;
            }
#if PLANAR3_URP
            else if ( Settings.shadowDistance < 1 && currentReflector.camData ) {
                currentReflector.camData.renderShadows = Settings.shadowDistance > 0 ? srcCamera.GetUniversalAdditionalCameraData().renderShadows : false;
            }
#endif

            currentReflector.refCamera.targetTexture = ReflectionTex;

            currentReflector.refCamera.aspect = srcCamera.aspect;

            currentReflector.refCamera.rect = new Rect( 0, 0, 1, 1 );

            Vector3 worldSpaceViewDir = srcCamera.transform.forward;
            Vector3 worldSpaceViewUp = srcCamera.transform.up;
            Vector3 worldSpaceCamPos = srcCamera.transform.position;

            Vector3 planeSpaceViewDir = transform.InverseTransformDirection( worldSpaceViewDir );
            Vector3 planeSpaceViewUp = transform.InverseTransformDirection( worldSpaceViewUp );
            Vector3 planeSpaceCamPos = transform.InverseTransformPoint( worldSpaceCamPos );

            planeSpaceViewDir.y *= -1.0f;
            planeSpaceViewUp.y *= -1.0f;
            planeSpaceCamPos.y *= -1.0f;

            worldSpaceViewDir = transform.TransformDirection( planeSpaceViewDir );
            worldSpaceViewUp = transform.TransformDirection( planeSpaceViewUp );
            worldSpaceCamPos = transform.TransformPoint( planeSpaceCamPos );

            currentReflector.refCamera.transform.position = worldSpaceCamPos;
            currentReflector.refCamera.transform.LookAt( worldSpaceCamPos + worldSpaceViewDir, worldSpaceViewUp );

            currentReflector.refCamera.nearClipPlane = Settings.nearClipDistance;
            currentReflector.refCamera.farClipPlane = Settings.farClipDistance;




            currentReflector.refCamera.renderingPath = Settings.renderingPath;

            currentReflector.refCamera.cullingMask = Settings.reflectLayers;

#if PLANAR3_PRO && UNITY_POST_PROCESSING_STACK_V2

            if ( Settings.usePostFX ) {

                currentReflector.refCamera.allowMSAA = false;

                PostProcessLayer refPostFX = Settings.PostFXSettingsMode == PostFXSettingsMode.CustomSettings ? GetComponent<PostProcessLayer>() : srcCamera.GetComponent<PostProcessLayer>();

                if ( refPostFX ) {
                    PostProcessLayer targetPostFX = currentReflector.refCamera.GetComponent<PostProcessLayer>();

                    if ( targetPostFX ) {
                        targetPostFX.volumeTrigger = currentReflector.refCamera.transform;
                        targetPostFX.volumeLayer = refPostFX.volumeLayer;
                        targetPostFX.antialiasingMode = refPostFX.antialiasingMode == PostProcessLayer.Antialiasing.None ? PostProcessLayer.Antialiasing.None : PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
                        targetPostFX.fastApproximateAntialiasing = refPostFX.fastApproximateAntialiasing;
                    }
                    else {
                        targetPostFX = currentReflector.refCamera.gameObject.AddComponent<PostProcessLayer>();
                        System.Reflection.BindingFlags bindFlags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;
                        System.Reflection.FieldInfo field = typeof( PostProcessLayer ).GetField( "m_Resources", bindFlags );
                        targetPostFX.Init( (PostProcessResources)field.GetValue( refPostFX ) );

                        targetPostFX.volumeTrigger = currentReflector.refCamera.transform;
                        targetPostFX.volumeLayer = refPostFX.volumeLayer;
                        targetPostFX.antialiasingMode = refPostFX.antialiasingMode == PostProcessLayer.Antialiasing.None ? PostProcessLayer.Antialiasing.None : PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
                        targetPostFX.fastApproximateAntialiasing = refPostFX.fastApproximateAntialiasing;

                    }
                }

            }
            else {
                if ( GetComponent<PostProcessLayer>() ) {
                    DestroyImmediate( GetComponent<PostProcessLayer>() );
                    if ( GetComponent<Camera>() ) {
                        DestroyImmediate( GetComponent<Camera>() );
                    }
                }

                if ( currentReflector.refCamera.GetComponent<PostProcessLayer>() ) {
                    DestroyImmediate( currentReflector.refCamera.GetComponent<PostProcessLayer>() );
                }
            }
#elif PLANAR3_PRO && UNITY_2019_3_OR_NEWER && (PLANAR3_URP || PLANAR3_HDRP)

            if ( SRPMode ) {
                if ( Settings.usePostFX ) {
#if PLANAR3_URP
                    UniversalAdditionalCameraData refPostFX = Settings.PostFXSettingsMode == PostFXSettingsMode.CustomSettings ? GetComponent<UniversalAdditionalCameraData>() : srcCamera.GetComponent<UniversalAdditionalCameraData>();
#elif PLANAR3_HDRP
                    UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData refPostFX = Settings.PostFXSettingsMode == PostFXSettingsMode.CustomSettings ? GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>() : srcCamera.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
#endif
                    if ( refPostFX ) {
#if PLANAR3_URP
                        UniversalAdditionalCameraData targetPostFX = currentReflector.camData = currentReflector.refCamera.GetUniversalAdditionalCameraData();
#elif PLANAR3_HDRP
                        UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData targetPostFX = currentReflector.camData;
#endif
                        if ( targetPostFX ) {
#if PLANAR3_URP
                            targetPostFX.renderPostProcessing = refPostFX.renderPostProcessing;
                            targetPostFX.volumeTrigger = srcCamera.transform;
#elif PLANAR3_HDRP
                            targetPostFX.volumeAnchorOverride = currentReflector.refCamera.transform;
#endif
                            targetPostFX.volumeLayerMask = refPostFX.volumeLayerMask;
#if PLANAR3_URP
                            targetPostFX.antialiasing = refPostFX.antialiasing == UnityEngine.Rendering.Universal.AntialiasingMode.None ? UnityEngine.Rendering.Universal.AntialiasingMode.None : UnityEngine.Rendering.Universal.AntialiasingMode.FastApproximateAntialiasing;
                        }
                        else {
                            targetPostFX = currentReflector.refCamera.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                            currentReflector.camData = targetPostFX;
                            targetPostFX.renderPostProcessing = refPostFX.renderPostProcessing;

                            targetPostFX.volumeTrigger = currentReflector.refCamera.transform;
                            targetPostFX.volumeLayerMask = refPostFX.volumeLayerMask;
                            targetPostFX.antialiasing = refPostFX.antialiasing == UnityEngine.Rendering.Universal.AntialiasingMode.None ? UnityEngine.Rendering.Universal.AntialiasingMode.None : UnityEngine.Rendering.Universal.AntialiasingMode.FastApproximateAntialiasing;

                        }

                    }

                }
                else {

                    if ( currentReflector.camData ) {
                        currentReflector.camData.renderPostProcessing = false;
                    }

                }

            }

#elif PLANAR3_HDRP
                            targetPostFX.antialiasing = refPostFX.antialiasing == UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData.AntialiasingMode.None ? UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData.AntialiasingMode.None : UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData.AntialiasingMode.FastApproximateAntialiasing;
                        }
                        else {
                            targetPostFX = currentReflector.refCamera.gameObject.AddComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
                            currentReflector.camData = targetPostFX;

                            targetPostFX.volumeAnchorOverride = currentReflector.refCamera.transform;
                            targetPostFX.volumeLayerMask = refPostFX.volumeLayerMask;
                            targetPostFX.antialiasing = refPostFX.antialiasing == UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData.AntialiasingMode.None ? UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData.AntialiasingMode.None : UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData.AntialiasingMode.FastApproximateAntialiasing;

                        }

                    }

                }
            }
#endif


#endif

            //Global fix for Screen position out of frustum, Camera assertion failed and similar errors.
            currentReflector.refCamera.transform.Rotate( 0.01f, 0.01f, 0.01f );
            currentReflector.refCamera.transform.Translate( 0.002f, 0.002f, 0.002f );

            if ( settings.reflectionClipMode == ReflectionClipMode.AccurateClipping ) {
                currentReflector.refCamera.projectionMatrix = currentReflector.refCamera.CalculateObliqueMatrix( CameraSpacePlane( currentReflector.refCamera, transform.position, transform.up ) );
            }


            if ( !SRPMode ) {
                if ( !Application.isPlaying || Settings.targetFramerate == 0 ) {

                    currentReflector.refCamera.Render();
                }
                else if ( srcCamera.cameraType != CameraType.SceneView && Application.isPlaying ) {

                    if ( Time.realtimeSinceStartup > frameTime ) {
                        frameTime = Time.realtimeSinceStartup + ( 1.0f / Settings.targetFramerate );
                        currentReflector.refCamera.Render();
                    }
                }
            }
            else {

                if ( !Application.isPlaying || Settings.targetFramerate == 0 ) {
                    currentReflector.refCamera.enabled = true;
#if PLANAR3_URP || PLANAR3_HDRP
                    if ( !currentReflector.camData ) {
#if PLANAR3_URP
                        if ( currentReflector.refCamera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>() ) {
                            currentReflector.camData = currentReflector.refCamera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                        }
                        else {
                            currentReflector.camData = currentReflector.refCamera.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
                        }
#elif PLANAR3_HDRP
                        if ( currentReflector.refCamera.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>() ) {
                            currentReflector.camData = currentReflector.refCamera.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
                        }
                        else {
                            currentReflector.camData = currentReflector.refCamera.gameObject.AddComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
                        }
#endif
                    }
#endif
                }
                else {

                    if ( Time.realtimeSinceStartup > frameTime ) {
                        frameTime = Time.realtimeSinceStartup + ( 1.0f / Settings.targetFramerate );
                        currentReflector.refCamera.enabled = true;
#if PLANAR3_URP || PLANAR3_HDRP
                        if ( !currentReflector.camData ) {
#if PLANAR3_URP
                            currentReflector.camData = currentReflector.refCamera.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
#elif PLANAR3_HDRP
                            currentReflector.camData = currentReflector.refCamera.gameObject.AddComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
#endif
                        }
#if PLANAR3_URP
                        currentReflector.camData.requiresDepthOption = CameraOverrideOption.Off;
                        currentReflector.camData.requiresColorOption = CameraOverrideOption.Off;
                        currentReflector.camData.renderShadows = Settings.shadowDistance > 0 ? srcCamera.GetUniversalAdditionalCameraData().renderShadows : false;
#endif
                    }
                    else {
                        currentReflector.refCamera.enabled = false;
                        currentReflector.refCamera.allowMSAA = false;
                        if ( !currentReflector.camData ) {
#if PLANAR3_URP
                            currentReflector.camData = currentReflector.refCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();
#endif
                        }

#if PLANAR3_URP
                        currentReflector.camData.requiresDepthOption = CameraOverrideOption.Off;
                        currentReflector.camData.requiresColorOption = CameraOverrideOption.Off;
                        currentReflector.camData.renderShadows = Settings.shadowDistance > 0?srcCamera.GetUniversalAdditionalCameraData().renderShadows:false;
#endif

#endif
                    }

                }

                currentReflector.refCamera.depth = -999;
            }

            if ( Settings.useDepth && internalDepthShader && !SRPMode ) {
                if ( !externalReflectionDepth ) {


                    int width = (int)( ( Settings.resolutionMode == ResolutionMode.ExplicitValue ? Settings.explicitResolution.x : srcCamera.pixelWidth ) * Settings.resolutionDownscale );
                    int height = (int)( ( Settings.resolutionMode == ResolutionMode.ExplicitValue ? Settings.explicitResolution.y : srcCamera.pixelHeight ) * Settings.resolutionDownscale );


                    if ( reflectionDepth && ( reflectionDepth.width != width || reflectionDepth.height != height ) ) {
                        RenderTexture.ReleaseTemporary( reflectionDepth );
                        reflectionDepth = RenderTexture.GetTemporary( width, height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear );
                        reflectionDepth.name = "PIDI_REFDEPTH" + ( srcCamera.cameraType == CameraType.SceneView ? "SCENE" : "GAME" );
                        if ( !rTex.Contains( reflectionDepth ) ) {
                            rTex.Add( reflectionDepth );
                        }
                    }
                    else if ( !reflectionDepth ) {
                        reflectionDepth = RenderTexture.GetTemporary( width, height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear );
                        reflectionDepth.name = "PIDI_REFDEPTH" + ( srcCamera.cameraType == CameraType.SceneView ? "SCENE" : "GAME" );
                        if ( !rTex.Contains( reflectionDepth ) ) {
                            rTex.Add( reflectionDepth );
                        }
                    }


                }

                currentReflector.refCamera.targetTexture = ReflectionDepth;
                currentReflector.refCamera.backgroundColor = Color.green;
                currentReflector.refCamera.clearFlags = CameraClearFlags.Color;
                Shader.SetGlobalVector( "_Planar3DepthPlaneOrigin", new Vector4( transform.position.x, transform.position.y, transform.position.z ) );
                Shader.SetGlobalVector( "_Planar3DepthPlaneNormal", -new Vector4( transform.up.x, transform.up.y, transform.up.z ) );
                currentReflector.refCamera.renderingPath = RenderingPath.Forward;
                currentReflector.refCamera.RenderWithShader( internalDepthShader, "" );

            }


            QualitySettings.shadowDistance = tempShadowDistance;

#if UNITY_EDITOR
            if ( srcCamera.cameraType == CameraType.SceneView )
                DrawReflectorMesh( srcCamera );
#endif

        }



        public void LateUpdate() {
            castersActive = false;

            Camera[] cams = new Camera[gameReflectors.Keys.Count];

            gameReflectors.Keys.CopyTo( cams, 0 );

            for ( int c = 0; c < cams.Length; c++ ) {
                if ( cams[c] != null ) {

                    Vector3 worldSpaceViewDir = cams[c].transform.forward;
                    Vector3 worldSpaceViewUp = cams[c].transform.up;
                    Vector3 worldSpaceCamPos = cams[c].transform.position;

                    Vector3 planeSpaceViewDir = transform.InverseTransformDirection( worldSpaceViewDir );
                    Vector3 planeSpaceViewUp = transform.InverseTransformDirection( worldSpaceViewUp );
                    Vector3 planeSpaceCamPos = transform.InverseTransformPoint( worldSpaceCamPos );

                    planeSpaceViewDir.y *= -1.0f;
                    planeSpaceViewUp.y *= -1.0f;
                    planeSpaceCamPos.y *= -1.0f;

                    worldSpaceViewDir = transform.TransformDirection( planeSpaceViewDir );
                    worldSpaceViewUp = transform.TransformDirection( planeSpaceViewUp );
                    worldSpaceCamPos = transform.TransformPoint( planeSpaceCamPos );

                    if ( gameReflectors[cams[c]].refCamera ) {
                        gameReflectors[cams[c]].refCamera.transform.position = worldSpaceCamPos;
                        gameReflectors[cams[c]].refCamera.transform.LookAt( worldSpaceCamPos + worldSpaceViewDir, worldSpaceViewUp );


                        if ( Settings.useCustomComponents ) {
                            var comps = new List<string>( Settings.customComponentNames );
                            foreach ( Component comp in gameObject.GetComponents( typeof( Component ) ) ) {
                                if ( comps.Contains( comp.GetType().Name ) ) {
                                    if ( !gameReflectors[cams[c]].refCamera.GetComponent( comp.GetType() ) ) {
                                        var copy = gameReflectors[cams[c]].refCamera.gameObject.AddComponent( comp.GetType() );
                                        System.Reflection.FieldInfo[] fields = comp.GetType().GetFields();
                                        foreach ( System.Reflection.FieldInfo field in fields ) {
                                            field.SetValue( copy, field.GetValue( comp ) );
                                        }
                                    }
                                    else if ( Settings.autoSynchComponents ) {
                                        var copy = gameReflectors[cams[c]].refCamera.gameObject.AddComponent( comp.GetType() );
                                        System.Reflection.FieldInfo[] fields = comp.GetType().GetFields();
                                        foreach ( System.Reflection.FieldInfo field in fields ) {
                                            field.SetValue( copy, field.GetValue( comp ) );
                                        }
                                    }
                                }
                            }
                        }


                    }

                }
                else {
                    gameReflectors.Remove( cams[c] );
                }
            }


        }


        public void OnDisable() {

            foreach ( RenderTexture r in rTex ) {
#if UNITY_2018_3_OR_NEWER
                RenderTexture.ReleaseTemporary( r );
#else
                DestroyImmediate( r );
#endif 
            }

            if ( SRPMode ) {
#if UNITY_2019_1_OR_NEWER && ( PLANAR3_PRO || PLANAR3_LWRP || PLANAR3_URP || PLANAR3_HDRP )
                UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= SRPRenderReflection;
                UnityEngine.Rendering.RenderPipelineManager.beginCameraRendering -= CleanupTextures;
                UnityEngine.Rendering.RenderPipelineManager.endFrameRendering -= VisibilityDisable;
#endif
            }
            else {
                Camera.onPreCull -= RenderReflection;
            }
#if UNITY_2019_1_OR_NEWER && ( PLANAR3_PRO || PLANAR3_LWRP || PLANAR3_URP || PLANAR3_HDRP )

            var cams = Resources.FindObjectsOfTypeAll<Camera>();

            foreach ( Camera cam in cams ) {
                if ( cam.name.Contains( "PLANAR3_" ) ) {
                    if ( cam.targetTexture && rTex.Contains( cam.targetTexture ) ) {
                        var rt = cam.targetTexture;
                        cam.targetTexture = null;
                        RenderTexture.ReleaseTemporary( rt );
                    }
                    DestroyImmediate( cam.gameObject );
                }
            }

#endif
        }


        private Vector4 CameraSpacePlane( Camera forCamera, Vector3 planeCenter, Vector3 planeNormal ) {
            Vector3 offsetPos = planeCenter;
            Matrix4x4 mtx = forCamera.worldToCameraMatrix;
            Vector3 cPos = mtx.MultiplyPoint( offsetPos );
            Vector3 cNormal = mtx.MultiplyVector( planeNormal ).normalized * 1;
            return new Vector4( cNormal.x, cNormal.y, cNormal.z, -Vector3.Dot( cPos, cNormal ) );
        }


#if UNITY_EDITOR

        public void OnDrawGizmos() {

            Gizmos.matrix = Matrix4x4.TRS( transform.position, transform.rotation, Vector3.one );
            Gizmos.DrawIcon( transform.position + transform.rotation * Vector3.up, "Planar3Logo_Gizmos.png" );
            Gizmos.color = Color.clear;
            Gizmos.DrawCube( Vector3.zero, new Vector3( 1, 0.01f, 1 ) * 10 );
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube( Vector3.zero, new Vector3( 1, 0, 1 ) * 10 );
            Gizmos.matrix = Matrix4x4.TRS( Vector3.zero, Quaternion.identity, Vector3.one );

        }


        public void OnDrawGizmosSelected() {

        }

#endif

    }


}