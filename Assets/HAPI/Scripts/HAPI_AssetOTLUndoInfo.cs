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
}
