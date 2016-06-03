using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

[ InitializeOnLoad ]
public class HoudiniGUIUtility : Editor 
{
	static HoudiniGUIUtility()
	{
		
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public

	public static string promptForOTLPath()
	{
		return promptForOTLPath( "" );
	}
	public static string promptForOTLPath( string old_path )
	{
		string ext = "";
#if UNITY_STANDALONE_WIN || ( UNITY_METRO && UNITY_EDITOR )
		ext = "otl;*.hda;*.otllc;*.hdalc;*.otlnc;*.hdanc";
#endif // UNITY_STANDALONE_WIN || ( UNITY_METRO && UNITY_EDITOR )

		string new_path = EditorUtility.OpenFilePanel( "Load Houdini Asset", old_path, ext );
		return new_path;
	}
	
	public static string promptForHIPPath()
	{
		return promptForHIPPath( "" );
	}
	public static string promptForHIPPath( string old_path )
	{
		string ext = "";
#if UNITY_STANDALONE_WIN || ( UNITY_METRO && UNITY_EDITOR )
		ext = "hip;*.hiplc;*.hipnc";
#endif // UNITY_STANDALONE_WIN || ( UNITY_METRO && UNITY_EDITOR )

		string new_path = EditorUtility.OpenFilePanel( "Open Houdini HIP", old_path, ext );
		return new_path;
	}
	
	public static string promptForPrefabPath( string old_path )
	{		
		// Prompt for the absolute path to the .otl file to use.
		string new_path = EditorUtility.OpenFilePanel( "Choose Prefab", old_path, "prefab" );
		return new_path;
	}
	
	public static string promptForFileInputPath( string old_path )
	{		
		// Prompt for the absolute path to the .otl file to use.
		string new_path = EditorUtility.OpenFilePanel( "Open File", old_path, "*.*" );
		return new_path;
	}

	public static void saveHoudiniScene( bool lock_nodes )
	{
		HAPI_License license = HoudiniHost.getCurrentLicense();
		string ext = "hip";

		if ( license == HAPI_License.HAPI_LICENSE_HOUDINI_ENGINE_INDIE
			|| license == HAPI_License.HAPI_LICENSE_HOUDINI_INDIE )
		{
			ext = "hiplc";
		}

		string hip_file_path = EditorUtility.SaveFilePanel( "Save HIP File", "", "hscene", ext );
		if ( hip_file_path != "" )
			HoudiniHost.saveScene( hip_file_path, lock_nodes );
	}

	public static float closestDistanceBetweenLineAndLineSegment( Vector3 line_p0, Vector3 line_p1, Ray ray, out Vector3 out_point )
	{
		float SMALL_NUM = 0.0000001f;

		// Line 1 = line_p0, line_p1
		// Line 2 = ray

		Vector3 p1 = line_p0;
		Vector3 v1 = line_p1 - line_p0;
		Vector3 p2 = ray.origin;
		Vector3 v2 = ray.direction;

		float t1, t2;

		// We use double precision here,
		// because denom can be quite small.
		Vector3 vc = p2 - p1;
		float v1v1   = Vector3.Dot(v1, v1);
		float v1v2   = Vector3.Dot(v1, v2);
		float v2v2   = Vector3.Dot(v2, v2);
		float v1vc   = Vector3.Dot(v1, vc);
		float v2vc   = Vector3.Dot(v2, vc);
		float denom  = (v2v2 * v1v1) - (v1v2 * v1v2);

		t1 = ( v2v2*v1vc - v1v2*v2vc ) / denom;
		t2 = ( v1v2*v1vc - v1v1*v2vc ) / denom;

		if ( denom < SMALL_NUM )
		{
			out_point = new Vector3();
			return 100000000f; // TODO: Be less random.
		}

		t1 = Mathf.Clamp( t1, 0.0f, 1.0f );

		out_point = p2 + ( v2 * t2 );

		Vector3 other_point = p1 + ( v1 * t1 );

		return ( out_point - other_point ).magnitude;
	}

	public static Vector3 getCameraNearPlanePoint( Vector3 original_point, Camera camera )
	{
		float near_plane_distance	= camera.nearClipPlane;
		Vector3 screen_space_point	= camera.WorldToScreenPoint( original_point );

		// Just slightly beyond the near plane.
		screen_space_point.z		= near_plane_distance + myScreenSpaceNearPlanePadding;

		Vector3 near_plane_point	= camera.ScreenToWorldPoint( screen_space_point );

		return near_plane_point;
	}
	
	public const string mySaveHoudiniSceneLabel		= "Save Current Houdini Scene...";
	public const string myLoadAssetLabel			= "Load Houdini Asset...";
	public const string myLoadHipLabel				= "Load Houdini Scene...";
	public const string mySaveHipLabel				= "Save Houdini Scene...";
	public const string myLaunchOrboltPage			= "Get Assets";
	public const string myRevertAllSettingsLabel	= "Revert All Settings";
	public const string myConvertToInputAssetLabel	= "Convert to Input Asset";
	public const string myCreateMergerAsset			= "Create Merger Asset";
	public const string myCreateCurveLabel			= "Create Curve";
	public const string myDefaultPrefabLabel 		= "Houdini Asset";
	public const string myDebugLabel				= "Debug";
	public const string mySettingsLabel 			= "Settings";

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private

	private const float myScreenSpaceNearPlanePadding = 10.0f;
}
