#define PLANAR3_PRO

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if PLANAR3_PRO && UNITY_POST_PROCESSING_STACK_V2
using UnityEngine.Rendering.PostProcessing;
#endif
using UnityEditor;

namespace PlanarReflections3 {

    [CustomEditor( typeof( PlanarReflectionsRenderer ) )]
    public class PlanarReflectionsRenderer_Editor : Editor {

        public GUISkin pidiSkin2;

        public PlanarReflectionsRenderer reflections;

        public Texture2D reflectionsLogo;


        public void OnEnable() {
            reflections = (PlanarReflectionsRenderer)target;
        }

        public override void OnInspectorGUI() {

            Undo.RecordObject( reflections, reflections.name + reflections.GetInstanceID() );

            GUILayout.BeginVertical( pidiSkin2.box );

            AssetLogoAndVersion();

            // GENERAL SETTINGS

            if ( BeginCenteredGroup( "GENERAL SETTINGS", ref reflections.folds[0] ) ) {

                GUILayout.Space( 16 );

                reflections.displaySceneReflector = EnableDisableToggle( new GUIContent( "Scene View Display", "If enabled, the reflections renderer will be displayed as a flat surface in the SceneView" ), reflections.displaySceneReflector );
#if UNITY_POST_PROCESSING_STACK_V2 && PLANAR3_PRO
                reflections.Settings.usePostFX = EnableDisableToggle( new GUIContent( "Post FX Support", "Enables or disables the Post Process FX support for this reflections renderer" ), reflections.Settings.usePostFX );
#elif UNITY_2019_3_OR_NEWER && (PLANAR3_PRO || PLANAR3_LWRP || PLANAR3_URP || PLANAR3_HDRP)
                if ( reflections.isSRP ) {
                    reflections.Settings.usePostFX = EnableDisableToggle( new GUIContent( "Post FX Support", "Enables or disables the Post Process FX support for this reflections renderer" ), reflections.Settings.usePostFX );
                }
#endif
                if ( !reflections.isSRP ) {
                    reflections.Settings.useDepth = EnableDisableToggle( new GUIContent( "Contact Depth Support", "Enables or disables the support for contact depth rendering for this reflections renderer" ), reflections.Settings.useDepth );

                    if ( reflections.Settings.useDepth ) {
                        reflections.internalDepthShader = ObjectField<Shader>( new GUIContent( "Depth Pass Shader", "The shader used to render the depth/custom pass of the reflections" ), reflections.internalDepthShader );
                        GUILayout.Space( 8 );
                    }
                }

                reflections.Settings.useCustomComponents = EnableDisableToggle( new GUIContent( "Track Custom Components", "If enabled, custom components attached to this reflection renderer will be added to the reflections themselves" ), reflections.Settings.useCustomComponents );

                reflections.Settings.trackCamerasWithTag = EnableDisableToggle( new GUIContent( "Track Cameras by Tag", "If enabled, only the cameras with a certain tag will trigger the reflection rendering process" ), reflections.Settings.trackCamerasWithTag );

                if ( reflections.Settings.trackCamerasWithTag ) {
                    reflections.Settings.CamerasTag = TextField( new GUIContent( "Tag to Track" ), reflections.Settings.CamerasTag );
                }

                GUILayout.Space( 16 );

            }
            EndCenteredGroup();

            //REFLECTION SETTINGS

            if ( BeginCenteredGroup( "REFLECTION SETTINGS", ref reflections.folds[1] ) ) {

                GUILayout.Space( 16 );

                reflections.ExternalReflectionTex = ObjectField<RenderTexture>( new GUIContent( "External Reflection Texture", "An external Render Texture asset to which the reflection will be rendered to" ), reflections.ExternalReflectionTex );

                if ( reflections.Settings.useDepth && reflections.ExternalReflectionTex ) {
                    reflections.ExternalReflectionDepth = ObjectField<RenderTexture>( new GUIContent( "External Reflection Depth", "An external Render Texture asset to which the reflection's depth will be rendered to" ), reflections.ExternalReflectionDepth );
                }

                GUILayout.Space( 8 );

                if ( reflections.Settings.reflectionClipMode == ReflectionClipMode.SimpleApproximation )
                    reflections.Settings.renderingPath = (RenderingPath)UpperCaseEnumField( new GUIContent( "Rendering Path", "The rendering path used to render this reflection" ), reflections.Settings.renderingPath );

                reflections.Settings.reflectLayers = LayerMaskField( new GUIContent( "Layers to Reflect", "The layers that will be rendered by this reflcetions renderer" ), reflections.Settings.reflectLayers );

                reflections.Settings.reflectionClipMode = (ReflectionClipMode)UpperCaseEnumField( new GUIContent( "Clipping Mode", "The way in which the clipping planes of this reflection are handled" ), reflections.Settings.reflectionClipMode );

                reflections.Settings.nearClipDistance = Mathf.Max( FloatField( new GUIContent( "Near Clip Plane", "The distance from the camera to the near-clip plane" ), reflections.Settings.nearClipDistance ), 0.01f );
                reflections.Settings.farClipDistance = Mathf.Clamp( FloatField( new GUIContent( "Far Clip Plane", "The distance from the camera to the far-clip plane" ), reflections.Settings.farClipDistance ), reflections.Settings.nearClipDistance, Mathf.Infinity );

                GUILayout.Space( 8 );

                reflections.Settings.useCustomClearFlags = EnableDisableToggle( new GUIContent( "Override Clear Flags" ), reflections.Settings.useCustomClearFlags, true );

                if ( reflections.Settings.useCustomClearFlags ) {
                    reflections.Settings.clearFlags = PopupField( new GUIContent( "Clear Flags" ), reflections.Settings.clearFlags, new string[] { "Skybox", "Color" } );

                    if (reflections.Settings.clearFlags == 1 ) {
                        reflections.Settings.backgroundColor = ColorField( new GUIContent( "Background Color" ), reflections.Settings.backgroundColor );
                    }
                }


                GUILayout.Space( 8 );


                if ( !reflections.isSRP ) {
                    GUILayout.Space( 8 );
                    reflections.Settings.customShadowDistance = EnableDisableToggle( new GUIContent( "Custom Shadow Distance", "If enabled, the shadows rendering distance defined on the Quality Settings will be overriden by this reflection renderer" ), reflections.Settings.customShadowDistance );

                    if ( reflections.Settings.customShadowDistance )
                        reflections.Settings.shadowDistance = SliderField( new GUIContent( "Shadows Distance", "The maximum render distance for the shadows" ), reflections.Settings.shadowDistance, 0, reflections.Settings.farClipDistance );
                }
                else {
                    GUILayout.Space( 8 );
                    reflections.Settings.shadowDistance = EnableDisableToggle( new GUIContent( "Display Shadows", "Whether shadows will be shown in the reflections" ), reflections.Settings.shadowDistance > 0 ) ? 50 : 0;

                }

                GUILayout.Space( 16 );

            }
            EndCenteredGroup();


            //OPTIMIZATION SETTINGS

            if ( BeginCenteredGroup( "OPTIMIZATION SETTINGS", ref reflections.folds[2] ) ) {

                GUILayout.Space( 16 );

                HelpBox( "When using Mip Maps in your reflection you must make sure that it is a power of two texture. Screen based resolution and some downscaling values are not compatible with Mip Maps and will disable this feature. In order to reflect changes to the antialiasing setting, you must force it to refresh by entering playmode or changing its resolution / downscaling.", MessageType.Warning );


                GUILayout.Space( 16 );

                reflections.Settings.useOcclusionCulling = EnableDisableToggle( new GUIContent( "Use Occlusion Culling", "Occlusion culling is, in most cases, incompatible with reflections due to the virtual reflection camera not existing in the same space as the game's camera, thus triggering occlusion on different items and producing unexpected results. In most cases, occlusion culling for reflections should be turned off" ), reflections.Settings.useOcclusionCulling );

                reflections.Settings.updateOnCastOnly = EnableDisableToggle( new GUIContent( "Visibility Optimizations", "If enabled, this reflection will only be rendered when a PlanarReflectionsCaster on the scene casting it becomes visible, greatly improving performance" ), reflections.Settings.updateOnCastOnly );

                if ( !reflections.ExternalReflectionTex ) {

                    if ( !reflections.ExternalReflectionTex ) {
                        reflections.Settings.resolutionMode = (ResolutionMode)UpperCaseEnumField( new GUIContent( "Resolution Mode", "The way the resolution of this reflection is defined, either based on the screen size or on an actual numerical value" ), reflections.Settings.resolutionMode );

                        reflections.Settings.useAntialiasing = EnableDisableToggle( new GUIContent( "Anti-Aliasing" ), reflections.Settings.useAntialiasing );

                        if ( reflections.Settings.resolutionMode != ResolutionMode.ScreenBased ) {
                            reflections.Settings.useMipMaps = EnableDisableToggle( new GUIContent( "Mip Maps" ), reflections.Settings.useMipMaps );
                            reflections.Settings.explicitResolution = Vector2Field( new GUIContent( "Explicit Resolution", "The explicit size of the texture used to render the reflection" ), reflections.Settings.explicitResolution );
                        }
                        else {
                            reflections.Settings.useMipMaps = false;
                        }

                        reflections.Settings.resolutionDownscale = SliderField( new GUIContent( "Reflection's Downscale", "The final value that the resolution of this reflection will be multiplied by." ), reflections.Settings.resolutionDownscale, 0.05f, 2.0f );
                    }

                    GUILayout.Space( 8 );

                    reflections.Settings.targetFramerate = IntSliderField( new GUIContent( "Target Framerate", "The targeted framerate for this reflection. If set to 0 there will be no framerate cap applied to this reflection" ), reflections.Settings.targetFramerate, 0, 60 );
                }

                GUILayout.Space( 16 );

            }
            EndCenteredGroup();



            //POST FX SETTINGS

            if ( reflections.Settings.usePostFX ) {
                if ( BeginCenteredGroup( "POST FX SETTINGS", ref reflections.folds[3] ) ) {

                    GUILayout.Space( 16 );

                    reflections.Settings.forceFloatOutput = EnableDisableToggle( new GUIContent( "Float Texture Output", "If enabled, a float point precision texture format (ARGB32Float) will be used to render this reflection, allowing for better compatibility with Bloom and other HDR dependent effects. It may not be compatible with all devices" ), reflections.Settings.forceFloatOutput );

                    GUILayout.Space( 16 );
#if UNITY_POST_PROCESSING_STACK_V2 && PLANAR3_PRO

                    if ( !reflections.GetComponent<PostProcessLayer>() ) {
                        if ( !reflections.gameObject.GetComponent<Camera>() ) {
                            var c = reflections.gameObject.AddComponent<Camera>();
                            c.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable;
                            c.enabled = false;
                        }
                        reflections.gameObject.AddComponent<PostProcessLayer>();
                    }
                    else {

                        reflections.GetComponent<Camera>().hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable;
                        reflections.GetComponent<Camera>().enabled = false;

                        var postFX = reflections.GetComponent<PostProcessLayer>();

                        postFX.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable;

                        reflections.Settings.PostFXSettingsMode = (PostFXSettingsMode)UpperCaseEnumField( new GUIContent( "Post FX Settings Mode", "The source from which the settings of the Post FX Layer / Renderer will be read from" ), reflections.Settings.PostFXSettingsMode );

                        GUILayout.Space( 8 );

                        if ( reflections.Settings.PostFXSettingsMode == PostFXSettingsMode.CustomSettings ) {
                            postFX.volumeLayer = LayerMaskField( new GUIContent( "Post FX Volume Layers", "The layers in which the Post FX Volumes that this reflection will render are located" ), postFX.volumeLayer );

                            GUILayout.Space( 8 );

                            CenteredLabel( "Anti-Aliasing" );

                            GUILayout.Space( 8 );

                            postFX.antialiasingMode = (PostProcessLayer.Antialiasing)UpperCaseEnumField( new GUIContent( "Anti-Aliasing Mode", "The way the antialiasing will be handled for the reflections through the Post Processing component" ), postFX.antialiasingMode );

                            switch ( postFX.antialiasingMode ) {
                                case PostProcessLayer.Antialiasing.TemporalAntialiasing:
                                    postFX.antialiasingMode = PostProcessLayer.Antialiasing.FastApproximateAntialiasing;
                                    break;

                                case PostProcessLayer.Antialiasing.SubpixelMorphologicalAntialiasing:
                                    postFX.subpixelMorphologicalAntialiasing.quality = (SubpixelMorphologicalAntialiasing.Quality)UpperCaseEnumField( new GUIContent( "Quality" ), postFX.subpixelMorphologicalAntialiasing.quality );
                                    break;

                                case PostProcessLayer.Antialiasing.FastApproximateAntialiasing:
                                    postFX.fastApproximateAntialiasing.fastMode = EnableDisableToggle( new GUIContent( "Fast Mode" ), postFX.fastApproximateAntialiasing.fastMode );
                                    postFX.fastApproximateAntialiasing.keepAlpha = EnableDisableToggle( new GUIContent( "Keep Alpha" ), postFX.fastApproximateAntialiasing.keepAlpha );
                                    break;
                            }
                        }

                    }





#elif UNITY_2019_3_OR_NEWER && PLANAR3_PRO
                    if ( reflections.isSRP ) {
#if PLANAR3_URP
                        if ( !reflections.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>() ) {
#elif PLANAR3_HDRP
                        if ( !reflections.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>() ) {
#endif
                            if ( !reflections.gameObject.GetComponent<Camera>() ) {
                                var c = reflections.gameObject.AddComponent<Camera>();
                                c.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable;
                                c.enabled = false;
                            }
#if PLANAR3_URP
                            reflections.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
#elif PLANAR3_HDRP
                        reflections.gameObject.AddComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
#endif
                        }
                        else {

                            reflections.GetComponent<Camera>().hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable;
                            reflections.GetComponent<Camera>().enabled = false;
#if PLANAR3_URP
                            var postFX = reflections.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
#elif PLANAR3_HDRP
                        var postFX = reflections.GetComponent<UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData>();
#endif

#if PLANAR3_URP || PLANAR3_HDRP
                            postFX.hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable;

#if PLANAR3_URP
                            postFX.renderPostProcessing = true;
#endif
                            reflections.Settings.PostFXSettingsMode = (PostFXSettingsMode)UpperCaseEnumField( new GUIContent( "POST FX SETTINGS MODE", "The source from which the settings of the Post FX Layer / Renderer will be read from" ), reflections.Settings.PostFXSettingsMode );

                            GUILayout.Space( 8 );

                            if ( reflections.Settings.PostFXSettingsMode == PostFXSettingsMode.CustomSettings ) {
                                postFX.volumeLayerMask = LayerMaskField( new GUIContent( "Post FX Volume Layers", "The layers in which the Post FX Volumes that this reflection will render are located" ), postFX.volumeLayerMask );

                                GUILayout.Space( 8 );

                                CenteredLabel( "ANTI-ALIASING" );

                                GUILayout.Space( 8 );

#if PLANAR3_URP
                                postFX.antialiasing = (UnityEngine.Rendering.Universal.AntialiasingMode)UpperCaseEnumField( new GUIContent( "Anti-Aliasing Mode", "The way the antialiasing will be handled for the reflections through the Post Processing component" ), postFX.antialiasing );

                                switch ( postFX.antialiasing ) {

                                    case UnityEngine.Rendering.Universal.AntialiasingMode.SubpixelMorphologicalAntiAliasing:
                                        postFX.antialiasingQuality = (UnityEngine.Rendering.Universal.AntialiasingQuality)UpperCaseEnumField( new GUIContent( "Quality" ), postFX.antialiasingQuality );
                                        break;

                                    case UnityEngine.Rendering.Universal.AntialiasingMode.FastApproximateAntialiasing:
                                        postFX.antialiasingQuality = (UnityEngine.Rendering.Universal.AntialiasingQuality)UpperCaseEnumField( new GUIContent( "Quality" ), postFX.antialiasingQuality );
                                        break;
                                }

#elif PLANAR3_HDRP
                                postFX.antialiasing = (UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData.AntialiasingMode)UpperCaseEnumField( new GUIContent( "Anti-Aliasing Mode", "The way the antialiasing will be handled for the reflections through the Post Processing component" ), postFX.antialiasing );

                                switch ( postFX.antialiasing ) {

                                    case UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData.AntialiasingMode.SubpixelMorphologicalAntiAliasing:
                                        postFX.SMAAQuality = (UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData.SMAAQualityLevel)UpperCaseEnumField( new GUIContent( "Quality" ), postFX.SMAAQuality );
                                        break;

                                }
#endif

                            }

                        }

#endif
                        }
#endif
                    GUILayout.Space( 16 );

                }
                EndCenteredGroup();
            }

            //CUSTOM COMPONENT SETTINGS

            if ( reflections.Settings.useCustomComponents ) {
                if ( BeginCenteredGroup( "CUSTOM COMPONENT SETTINGS", ref reflections.folds[4] ) ) {

                    GUILayout.Space( 16 );
                    HelpBox( "This is a highly experimental feature that may produce undesired results and even crash the application or the Unity Editor. This feature is provided as an experimental extension and as such we will not be responsible nor will provide any fixes for incompatibilities with any third party components that are synched using this feature. If you want to use Post Process FX with your reflections, please use the dedicated settings for a better and more stable result", MessageType.Warning );

                    GUILayout.Space( 16 );

                    reflections.Settings.autoSynchComponents = EnableDisableToggle( new GUIContent( "Auto-Sync Components", "Automatically synch the values for all the fields of the components attached to this reflection renderer" ), reflections.Settings.autoSynchComponents );

                    GUILayout.Space( 8 );
                    CenteredLabel( "CUSTOM COMPONENT NAMES" );
                    GUILayout.Space( 8 );

                    for ( int i = 0; i < reflections.Settings.customComponentNames.Length; i++ ) {
                        GUILayout.Space( 2 );
                        GUILayout.BeginHorizontal();
                        reflections.Settings.customComponentNames[i] = TextField( new GUIContent( "Component Type Name", "The full typename (assemblies included) of the component that will be tracked" ), reflections.Settings.customComponentNames[i] );
                        GUILayout.Space( 8 );
                        if ( StandardButton( "X", 24 ) ) {
                            ArrayUtility.RemoveAt<string>( ref reflections.Settings.customComponentNames, i );
                            GUILayout.EndHorizontal();
                            GUILayout.EndVertical();
                            break;
                        }
                        GUILayout.EndHorizontal();
                        GUILayout.Space( 4 );
                    }

                    GUILayout.Space( 4 );

                    if ( CenteredButton( "NEW TRACKED COMPONENT", 200 ) ) {
                        ArrayUtility.Add<string>( ref reflections.Settings.customComponentNames, "Full Component Type name" );
                    }

                    GUILayout.Space( 16 );

                }
                EndCenteredGroup();
            }


            //HELP AND SUPPORT
            if ( BeginCenteredGroup( "HELP & SUPPORT", ref reflections.folds[6] ) ) {

                GUILayout.Space( 16 );
                CenteredLabel( "SUPPORT AND ASSISTANCE" );
                GUILayout.Space( 10 );

                HelpBox( "Please make sure to include the following information with your request :\n - Invoice number preferably in the PDF format it was provided to you at the time of purchase\n - Screenshots of the PlanarReflectionsRenderer / PlanarReflectionsCaster component and its settings\n - Steps to reproduce the issue.\n - Unity version you are using \n - LWRP/Universal RP version you are using (if any)\n\nOur support service usually takes 1-3 business days to reply, so please be patient. We always reply to all emails.\nPlease remember that our assets do not offer official support to any Experimental feature nor any Beta/Alpha Unity versions.", MessageType.Info );

                GUILayout.Space( 8 );
                GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
                GUILayout.Label( "For support, contact us at : support@irreverent-software.com", pidiSkin2.label );
                GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();

                GUILayout.Space( 8 );

                GUILayout.Space( 16 );
                CenteredLabel( "ONLINE TUTORIALS" );
                GUILayout.Space( 10 );
                if ( CenteredButton( "INITIAL SETUP", 200 ) ) {
                    Help.BrowseURL( "https://pidiwiki.irreverent-software.com/wiki/doku.php?id=pidi_planar_reflections_3#quick_setup_guide" );
                }
                if ( CenteredButton( "BASIC REFLECTIONS", 200 ) ) {
                    Help.BrowseURL( "https://pidiwiki.irreverent-software.com/wiki/doku.php?id=pidi_planar_reflections_3#adding_reflections_to_a_scene" );
                }
                if ( CenteredButton( "BASIC SETTINGS", 200 ) ) {
                    Help.BrowseURL( "https://pidiwiki.irreverent-software.com/wiki/doku.php?id=pidi_planar_reflections_3#planar_reflections_renderer_basic_settings" );
                }
                if ( CenteredButton( "MOVABLE SURFACES", 200 ) ) {
                    Help.BrowseURL( "https://pidiwiki.irreverent-software.com/wiki/doku.php?id=pidi_planar_reflections_3#movable_reflective_surfaces" );
                }
                if ( CenteredButton( "POST FX SUPPORT", 200 ) ) {
                    Help.BrowseURL( "https://pidiwiki.irreverent-software.com/wiki/doku.php?id=pidi_planar_reflections_3#post_fx_support" );
                }
                if ( CenteredButton( "CAMERA DEPTH EFFECTS", 200 ) ) {
                    Help.BrowseURL( "https://pidiwiki.irreverent-software.com/wiki/doku.php?id=pidi_2d_reflections_2#post_process_fx_standard_only" );
                }

                if ( CenteredButton( "LWRP SETUP & LIMITS", 200 ) ) {
                    Help.BrowseURL( "https://pidiwiki.irreverent-software.com/wiki/doku.php?id=pidi_planar_reflections_3#lwrp_vs_standard_pipeline" );
                }

                if ( CenteredButton( "CREATING CUSTOM SHADERS", 200 ) ) {
                    Help.BrowseURL( "https://pidiwiki.irreverent-software.com/wiki/doku.php?id=pidi_planar_reflections_3#create_custom_shaders" );
                }


                GUILayout.Space( 24 );
                CenteredLabel( "ABOUT PIDI : PLANAR REFLECTIONS™ 3" );
                GUILayout.Space( 12 );

                HelpBox( "PIDI : PLANAR REFLECTIONS™ has been integrated in dozens of projects by hundreds of users since 2017.\nYour use and support to this tool is what keeps it growing, evolving and adapting to better suit your needs and keep providing you with the best quality reflections for Unity.\n\nIf this tool has been useful for your project, please consider taking a minute to rate and review it, to help us to continue its development for a long time.", MessageType.Info );

                GUILayout.Space( 8 );
                if ( CenteredButton( "REVIEW PLANAR REFLECTIONS 3", 200 ) ) {
                    Help.BrowseURL( "https://assetstore.unity.com/packages/tools/particles-effects/pidi-planar-reflections-3-standard-edition-153073" );
                }
                GUILayout.Space( 8 );
                if ( CenteredButton( "ABOUT THIS VERSION", 200 ) ) {
                    Help.BrowseURL( "https://assetstore.unity.com/packages/tools/particles-effects/pidi-planar-reflections-3-standard-edition-153073" );
                }
                GUILayout.Space( 8 );

            }
            EndCenteredGroup();

            GUILayout.Space( 16 );

            GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();

            var lStyle = new GUIStyle();
            lStyle.fontStyle = FontStyle.Italic;
            lStyle.normal.textColor = Color.white;
            lStyle.fontSize = 8;

            GUILayout.Label( "Copyright© 2017-2021,   Jorge Pinal N.", lStyle );

            GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();

            GUILayout.Space( 24 );
            GUILayout.EndVertical();

        }



        #region PIDI 2020 EDITOR


        public void HelpBox( string message, MessageType messageType ) {
            GUILayout.Space( 8 );
            GUILayout.BeginHorizontal(); GUILayout.Space( 8 );
            GUILayout.BeginVertical( pidiSkin2.customStyles[5] );

            GUILayout.Space( 4 );
            GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();

            var mType = "INFO";

            switch ( messageType ) {
                case MessageType.Error:
                    mType = "ERROR";
                    break;

                case MessageType.Warning:
                    mType = "WARNING";
                    break;
            }

            var tStyle = new GUIStyle();
            tStyle.fontSize = 11;
            tStyle.fontStyle = FontStyle.Bold;
            tStyle.normal.textColor = Color.black;

            GUILayout.Label( mType, tStyle );

            GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();
            GUILayout.Space( 4 );

            GUILayout.BeginHorizontal(); GUILayout.Space( 8 ); GUILayout.BeginVertical();
            tStyle.fontSize = 9;
            tStyle.fontStyle = FontStyle.Normal;
            tStyle.wordWrap = true;
            GUILayout.TextArea( message, tStyle );

            GUILayout.Space( 8 );
            GUILayout.EndVertical(); GUILayout.Space( 8 ); GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.Space( 8 ); GUILayout.EndHorizontal();
            GUILayout.Space( 8 );
        }


        public Color ColorField( GUIContent label, Color currentValue ) {

            GUILayout.Space( 2 );
            GUILayout.BeginHorizontal();
            GUILayout.Label( label, pidiSkin2.label, GUILayout.Width( EditorGUIUtility.labelWidth ) );
            currentValue = EditorGUILayout.ColorField( currentValue );
            GUILayout.EndHorizontal();
            GUILayout.Space( 2 );

            return currentValue;

        }



        /// <summary>
        /// Draws a standard object field in the PIDI 2020 style
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="label"></param>
        /// <param name="inputObject"></param>
        /// <param name="allowSceneObjects"></param>
        /// <returns></returns>
        public T ObjectField<T>( GUIContent label, T inputObject, bool allowSceneObjects = true ) where T : UnityEngine.Object {

            GUILayout.Space( 2 );
            GUILayout.BeginHorizontal();
            GUILayout.Label( label, pidiSkin2.label, GUILayout.Width( EditorGUIUtility.labelWidth ) );
            GUI.color = Color.gray;
            inputObject = (T)EditorGUILayout.ObjectField( inputObject, typeof( T ), allowSceneObjects );
            GUI.color = Color.white;
            GUILayout.EndHorizontal();
            GUILayout.Space( 2 );
            return inputObject;
        }


        /// <summary>
        /// Draws a centered button in the standard PIDI 2020 editor style
        /// </summary>
        /// <param name="label"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public bool CenteredButton( string label, float width ) {
            GUILayout.Space( 2 );
            GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
            var tempBool = GUILayout.Button( label, pidiSkin2.customStyles[0], GUILayout.MaxWidth( width ) );
            GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();
            GUILayout.Space( 2 );
            return tempBool;
        }

        /// <summary>
        /// Draws a button in the standard PIDI 2020 editor style
        /// </summary>
        /// <param name="label"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        public bool StandardButton( string label, float width ) {
            var tempBool = GUILayout.Button( label, pidiSkin2.customStyles[0], GUILayout.MaxWidth( width ) );
            return tempBool;
        }


        /// <summary>
        /// Draws the asset's logo and its current version
        /// </summary>
        public void AssetLogoAndVersion() {

            GUILayout.BeginVertical( reflectionsLogo, pidiSkin2 ? pidiSkin2.customStyles[1] : null );
            GUILayout.Space( 45 );
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label( reflections.Version, pidiSkin2.customStyles[2] );
            GUILayout.Space( 6 );
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        /// <summary>
        /// Draws a label centered in the Editor window
        /// </summary>
        /// <param name="label"></param>
        public void CenteredLabel( string label ) {

            GUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
            GUILayout.Label( label, pidiSkin2.label );
            GUILayout.FlexibleSpace(); GUILayout.EndHorizontal();

        }

        /// <summary>
        /// Begins a custom centered group similar to a foldout that can be expanded with a button
        /// </summary>
        /// <param name="label"></param>
        /// <param name="groupFoldState"></param>
        /// <returns></returns>
        public bool BeginCenteredGroup( string label, ref bool groupFoldState ) {

            if ( GUILayout.Button( label, pidiSkin2.customStyles[0] ) ) {
                groupFoldState = !groupFoldState;
            }
            GUILayout.BeginHorizontal(); GUILayout.Space( 12 );
            GUILayout.BeginVertical();
            return groupFoldState;
        }


        /// <summary>
        /// Finishes a centered group
        /// </summary>
        public void EndCenteredGroup() {
            GUILayout.EndVertical();
            GUILayout.Space( 12 );
            GUILayout.EndHorizontal();
        }



        /// <summary>
        /// Custom integer field following the PIDI 2020 editor skin
        /// </summary>
        /// <param name="label"></param>
        /// <param name="currentValue"></param>
        /// <returns></returns>
        public int IntField( GUIContent label, int currentValue ) {

            GUILayout.Space( 2 );
            GUILayout.BeginHorizontal();
            GUILayout.Label( label, pidiSkin2.label, GUILayout.Width( EditorGUIUtility.labelWidth ) );
            currentValue = EditorGUILayout.IntField( currentValue, pidiSkin2.customStyles[4] );
            GUILayout.EndHorizontal();
            GUILayout.Space( 2 );

            return currentValue;
        }

        /// <summary>
        /// Custom float field following the PIDI 2020 editor skin
        /// </summary>
        /// <param name="label"></param>
        /// <param name="currentValue"></param>
        /// <returns></returns>
        public float FloatField( GUIContent label, float currentValue ) {

            GUILayout.Space( 2 );
            GUILayout.BeginHorizontal();
            GUILayout.Label( label, pidiSkin2.label, GUILayout.Width( EditorGUIUtility.labelWidth ) );
            currentValue = EditorGUILayout.FloatField( currentValue, pidiSkin2.customStyles[4] );
            GUILayout.EndHorizontal();
            GUILayout.Space( 2 );

            return currentValue;
        }


        /// <summary>
        /// Custom text field following the PIDI 2020 editor skin
        /// </summary>
        /// <param name="label"></param>
        /// <param name="currentValue"></param>
        /// <returns></returns>
        public string TextField( GUIContent label, string currentValue ) {

            GUILayout.Space( 2 );
            GUILayout.BeginHorizontal();
            GUILayout.Label( label, pidiSkin2.label, GUILayout.Width( EditorGUIUtility.labelWidth ) );
            currentValue = EditorGUILayout.TextField( currentValue, pidiSkin2.customStyles[4] );
            GUILayout.EndHorizontal();
            GUILayout.Space( 2 );

            return currentValue;
        }


        public Vector2 Vector2Field( GUIContent label, Vector2 currentValue ) {

            GUILayout.Space( 2 );
            GUILayout.BeginHorizontal();
            GUILayout.Label( label, pidiSkin2.label, GUILayout.Width( EditorGUIUtility.labelWidth ) );
            currentValue.x = EditorGUILayout.FloatField( currentValue.x, pidiSkin2.customStyles[4] );
            GUILayout.Space( 8 );
            currentValue.y = EditorGUILayout.FloatField( currentValue.y, pidiSkin2.customStyles[4] );
            GUILayout.EndHorizontal();
            GUILayout.Space( 2 );

            return currentValue;

        }


        /// <summary>
        /// Custom slider using the PIDI 2020 editor skin and adding a custom suffix to the float display
        /// </summary>
        /// <param name="label"></param>
        /// <param name="currentValue"></param>
        /// <param name="minSlider"></param>
        /// <param name="maxSlider"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public float SliderField( GUIContent label, float currentValue, float minSlider = 0.0f, float maxSlider = 1.0f, string suffix = "" ) {

            GUILayout.Space( 4 );
            GUILayout.BeginHorizontal();
            GUILayout.Label( label, pidiSkin2.label, GUILayout.Width( EditorGUIUtility.labelWidth ) );
            GUI.color = Color.gray;
            currentValue = GUILayout.HorizontalSlider( currentValue, minSlider, maxSlider, GUI.skin.horizontalSlider, GUI.skin.horizontalSliderThumb );
            GUI.color = Color.white;
            GUILayout.Space( 12 );
            currentValue = Mathf.Clamp( EditorGUILayout.FloatField( float.Parse( currentValue.ToString( "n2" ) ), pidiSkin2.customStyles[4], GUILayout.MaxWidth( 40 ) ), minSlider, maxSlider );
            GUILayout.EndHorizontal();
            GUILayout.Space( 4 );

            return currentValue;
        }


        /// <summary>
        /// Custom slider using the PIDI 2020 editor skin and adding a custom suffix to the float display
        /// </summary>
        /// <param name="label"></param>
        /// <param name="currentValue"></param>
        /// <param name="minSlider"></param>
        /// <param name="maxSlider"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public int IntSliderField( GUIContent label, int currentValue, int minSlider = 0, int maxSlider = 1 ) {

            GUILayout.Space( 4 );
            GUILayout.BeginHorizontal();
            GUILayout.Label( label, pidiSkin2.label, GUILayout.Width( EditorGUIUtility.labelWidth ) );
            GUI.color = Color.gray;
            currentValue = (int)GUILayout.HorizontalSlider( currentValue, minSlider, maxSlider, GUI.skin.horizontalSlider, GUI.skin.horizontalSliderThumb );
            GUI.color = Color.white;
            GUILayout.Space( 12 );
            currentValue = (int)Mathf.Clamp( EditorGUILayout.FloatField( float.Parse( currentValue.ToString( "n2" ) ), pidiSkin2.customStyles[4], GUILayout.MaxWidth( 40 ) ), minSlider, maxSlider );
            GUILayout.EndHorizontal();
            GUILayout.Space( 4 );

            return currentValue;
        }


        /// <summary>
        /// Draw a custom popup field in the PIDI 2020 style
        /// </summary>
        /// <param name="label"></param>
        /// <param name="toggleValue"></param>
        /// <returns></returns>
        public int PopupField( GUIContent label, int selected, string[] options ) {


            GUILayout.Space( 2 );
            GUILayout.BeginHorizontal();
            GUILayout.Label( label, pidiSkin2.label, GUILayout.Width( EditorGUIUtility.labelWidth ) );
            selected = EditorGUILayout.Popup( selected, options, pidiSkin2.customStyles[0] );
            GUILayout.EndHorizontal();
            GUILayout.Space( 2 );
            return selected;
        }



        /// <summary>
        /// Draw a custom toggle that instead of using a check box uses an Enable/Disable drop down menu
        /// </summary>
        /// <param name="label"></param>
        /// <param name="toggleValue"></param>
        /// <returns></returns>
        public bool EnableDisableToggle( GUIContent label, bool toggleValue, bool trueFalseToggle = false ) {

            int option = toggleValue ? 1 : 0;

            GUILayout.Space( 2 );
            GUILayout.BeginHorizontal();
            GUILayout.Label( label, pidiSkin2.label, GUILayout.Width( EditorGUIUtility.labelWidth ) );
            option = EditorGUILayout.Popup( option, new string[] { "DISABLED", "ENABLED" }, pidiSkin2.customStyles[0] );
            GUILayout.EndHorizontal();
            GUILayout.Space( 2 );
            return option == 1;
        }


        /// <summary>
        /// Draw an enum field but changing the labels and names of the enum to Upper Case fields
        /// </summary>
        /// <param name="label"></param>
        /// <param name="userEnum"></param>
        /// <returns></returns>
        public int UpperCaseEnumField( GUIContent label, System.Enum userEnum ) {

            var names = System.Enum.GetNames( userEnum.GetType() );

            for ( int i = 0; i < names.Length; i++ ) {
                names[i] = System.Text.RegularExpressions.Regex.Replace( names[i], "(\\B[A-Z])", " $1" ).ToUpper();
            }

            GUILayout.Space( 2 );
            GUILayout.BeginHorizontal();
            GUILayout.Label( label, pidiSkin2.label, GUILayout.Width( EditorGUIUtility.labelWidth ) );
            var result = EditorGUILayout.Popup( System.Convert.ToInt32( userEnum ), names, pidiSkin2.customStyles[0] );
            GUILayout.EndHorizontal();
            GUILayout.Space( 2 );
            return result;
        }


        /// <summary>
        /// Draw a layer mask field in the PIDI 2020 style
        /// </summary>
        /// <param name="label"></param>
        /// <param name="selected"></param>
        public LayerMask LayerMaskField( GUIContent label, LayerMask selected ) {

            List<string> layers = null;
            string[] layerNames = null;

            if ( layers == null ) {
                layers = new List<string>();
                layerNames = new string[4];
            }
            else {
                layers.Clear();
            }

            int emptyLayers = 0;
            for ( int i = 0; i < 32; i++ ) {
                string layerName = LayerMask.LayerToName( i );

                if ( layerName != "" ) {

                    for ( ; emptyLayers > 0; emptyLayers-- ) layers.Add( "Layer " + (i - emptyLayers) );
                    layers.Add( layerName );
                }
                else {
                    emptyLayers++;
                }
            }

            if ( layerNames.Length != layers.Count ) {
                layerNames = new string[layers.Count];
            }
            for ( int i = 0; i < layerNames.Length; i++ ) layerNames[i] = layers[i];


            GUILayout.Space( 2 );
            GUILayout.BeginHorizontal();
            GUILayout.Label( label, pidiSkin2.label, GUILayout.Width( EditorGUIUtility.labelWidth ) );

            selected.value = EditorGUILayout.MaskField( selected.value, layerNames, pidiSkin2.customStyles[0] );

            GUILayout.EndHorizontal();
            GUILayout.Space( 2 );
            return selected;
        }



        #endregion


    }

}

