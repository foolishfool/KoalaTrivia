#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;



public class PlanarReflections_SRPDetector {

#if UNITY_2017_1_OR_NEWER
    [UnityEditor.Callbacks.DidReloadScripts]
    public static void UpdateSRPDefinitions() {
        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
        if ( GraphicsSettings.renderPipelineAsset != null ) {
            // SRP

#if UNITY_2019_3_OR_NEWER
            var srpType = GraphicsSettings.renderPipelineAsset.GetType().ToString();
            if ( srpType.Contains( "Universal" ) ) {
                if ( !defines.Contains( "PLANAR3_URP" ) ) {
                    PlayerSettings.SetScriptingDefineSymbolsForGroup( EditorUserBuildSettings.selectedBuildTargetGroup, defines + ";PLANAR3_URP" );
                }
            }
            else if (srpType.Contains("HD")){
                if (!defines.Contains("PLANAR3_HDRP") ){
                    PlayerSettings.SetScriptingDefineSymbolsForGroup( EditorUserBuildSettings.selectedBuildTargetGroup, defines + ";PLANAR3_HDRP" );
                }
            }
#else
            if ( !defines.Contains("PLANAR3_LWRP")) {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defines + ";PLANAR3_LWRP");
            }
#endif


        }
    
}
#endif


}
#endif