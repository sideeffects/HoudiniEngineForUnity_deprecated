using UnityEngine;
using System.Collections;

using HAPI;

public class HAPI_AssetOTLUndoInfo : ScriptableObject
{
	// view settings
	public bool isGeoVisible;
	public bool showPinnedInstances;
	public bool autoSelectAssetRootNode;
	public bool hideGeometryOnLinking;

	// material settings
	public HAPI_ShaderType materialShaderType;
	public Vector2 renderResolution;
	public bool	showOnlyVertexColours;
	public bool	generateTangents;

	// cook settings
	public bool	enableCooking;
	public bool	cookingTriggersDownCooks;
	public bool	playmodePerFrameCooking;
	public bool	pushUnityTransformToHoudini;
	public bool	transformChangeTriggersCooks;
	public bool	importTemplatedGeos;

	// bake settings
	public float bakeStartTime;
	public float bakeEndTime;
	public int bakeSamplesPerSecond;

	// Initialization
	public void initialize( HAPI_AssetOTL asset_otl )
	{
		isGeoVisible 					= asset_otl.prIsGeoVisible;
		showPinnedInstances 			= asset_otl.prShowPinnedInstances;
		autoSelectAssetRootNode 		= asset_otl.prAutoSelectAssetRootNode;
		hideGeometryOnLinking 			= asset_otl.prHideGeometryOnLinking;
		materialShaderType 				= asset_otl.prMaterialShaderType;
		renderResolution 				= asset_otl.prRenderResolution;
		showOnlyVertexColours 			= asset_otl.prShowOnlyVertexColours;
		generateTangents 				= asset_otl.prGenerateTangents;
		enableCooking 					= asset_otl.prEnableCooking;
		cookingTriggersDownCooks 		= asset_otl.prCookingTriggersDownCooks;
		playmodePerFrameCooking 		= asset_otl.prPlaymodePerFrameCooking;
		pushUnityTransformToHoudini		= asset_otl.prPushUnityTransformToHoudini;
		transformChangeTriggersCooks 	= asset_otl.prTransformChangeTriggersCooks;
		importTemplatedGeos 			= asset_otl.prImportTemplatedGeos;
		bakeStartTime					= asset_otl.prBakeStartTime;
		bakeEndTime						= asset_otl.prBakeEndTime;
		bakeSamplesPerSecond			= asset_otl.prBakeSamplesPerSecond;
	}
}
