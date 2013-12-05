using UnityEngine;
using System.Collections;

using HAPI;

public class HAPI_AssetOTLUndoInfo : ScriptableObject
{
	public bool isGeoVisible;
	public bool showPinnedInstances;
	public bool autoSelectAssetRootNode;
	public bool hideGeometryOnLinking;
	public HAPI_ShaderType materialShaderType;
	public Vector2 renderResolution;
	public bool	showOnlyVertexColours;
	public bool	generateTangents;
	public bool	enableCooking;
	public bool	cookingTriggersDownCooks;
	public bool	playmodePerFrameCooking;
	public bool	pushUnityTransformToHoudini;
	public bool	transformChangeTriggersCooks;
	public bool	importTemplatedGeos;
}
