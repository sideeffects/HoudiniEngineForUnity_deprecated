/*
* Copyright (c) <2017> Side Effects Software Inc.
*
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all
* copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*
* Produced by:
*      Side Effects Software Inc
*      123 Front Street West, Suite 1401
*      Toronto, Ontario
*      Canada   M5J 2M2
*      416-504-9876
*
*/

// Master control for enabling runtime.
#if ( UNITY_EDITOR_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_LINUX )
	#define HAPI_ENABLE_RUNTIME
#endif

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR
using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.IO;

[ SelectionBase ]
[ ExecuteInEditMode ]
public abstract class HoudiniAsset : HoudiniObjectControl
{
	public enum AssetType
	{
		TYPE_OTL = 0,
		TYPE_HIP,
		TYPE_CURVE,
		TYPE_INPUT,
		TYPE_INVALID
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	
	// Please keep these in the same order and grouping as their initializations in HAPI_Asset.reset().
	
	// Assets -------------------------------------------------------------------------------------------------------
	
	public HAPI_AssetInfo 			prAssetInfo {					get { return myAssetInfo; } 
																	set { myAssetInfo = value; } }
	public HAPI_NodeInfo 			prNodeInfo {					get { return myNodeInfo; } 
																	set { myNodeInfo = value; } }
	public byte[]					prPreset {						get { return myPreset; } 
																	set { myPreset = value; } }
	public int						prAssetValidationId {			get { return myAssetValidationId; }
																	set { myAssetValidationId = value; } }
 	public string					prAssetName {					get { return myAssetName; }
																	set { myAssetName = value; } }
	public string					prAssetOpName {					get { return myAssetOpName; }
																	set { myAssetOpName = value; } }
	public string					prAssetHelp {					get { return myAssetHelp; }
																	set { myAssetHelp = value; } }
	public AssetType				prAssetType {					get { return myAssetType; }
																	set { myAssetType = value; } }

	public HoudiniAssetUndoInfo prAssetUndoInfo 
	{
		get
		{
			if ( myAssetOTLUndoInfo == null )
			{
				myAssetOTLUndoInfo = ScriptableObject.CreateInstance< HoudiniAssetUndoInfo >();
				myAssetOTLUndoInfo.name = prAssetName + "_UndoInfo";
				myAssetOTLUndoInfo.initialize( this );
			}
			return myAssetOTLUndoInfo;
		}

		private set {}
	}

	// Parameters ---------------------------------------------------------------------------------------------------

	public HoudiniPresetMap prPresetsMap 
	{
		get 
		{ 
			if ( myPresetsMap == null )
			{
				myPresetsMap = ScriptableObject.CreateInstance< HoudiniPresetMap >();
				myPresetsMap.name = prAssetName + "_PresetsMap";
			}
			return myPresetsMap;
		}
		private set {}
	}

	// Inputs -------------------------------------------------------------------------------------------------------
    public int 						prTransformInputCount {			get { return myTransformInputCount; } 
																	set { myTransformInputCount = value; } }
	public int						prGeoInputCount {				get { return myGeoInputCount; } 
																	set { myGeoInputCount = value; } }
	public List< int >				prGeoInputsTransformTypes {		get { return myGeoInputsTransformTypes; }
																	set { myGeoInputsTransformTypes = value; } }
	
	public List< HoudiniAsset >		prDownStreamTransformAssets {	get { return myDownStreamTransformAssets; } 
																	set { myDownStreamTransformAssets = value; } }
	public List< HoudiniAsset >		prUpStreamTransformAssets {		get { return myUpStreamTransformAssets; } 
																	set { myUpStreamTransformAssets = value; } }
	public List< GameObject >		prUpStreamTransformObjects {	get { return myUpStreamTransformObjects; } 
																	set { myUpStreamTransformObjects = value; } }
	
	public List< HoudiniAsset >		prUpStreamGeoAssets {			get { return myUpStreamGeoAssets; } 
																	set { myUpStreamGeoAssets = value; } }
	public List< GameObject >		prUpStreamGeoObjects {			get { return myUpStreamGeoObjects; } 
																	set { myUpStreamGeoObjects = value; } }
	public List< int >				prUpStreamGeoInputAssetIds {	get { return myUpStreamGeoInputAssetIds; } 
																	set { myUpStreamGeoInputAssetIds = value; } }
	public List< HoudiniAsset >		prDownStreamAssets {			get { return myDownStreamAssets; } 
																	set { myDownStreamAssets = value; } }

	// Objects ------------------------------------------------------------------------------------------------------
	
	public int 						prObjectCount {					get { return myObjectCount; } 
																	set { myObjectCount = value; } }
	public int						prHandleCount {					get { return myHandleCount; } 
																	set { myHandleCount = value; } }
	
	public HAPI_ObjectInfo[] 		prObjects {						get { return myObjects; } 
																	set { myObjects = value; } }
	
	public GameObject[]				prGameObjects {					get { return myGameObjects; } 
																	set { myGameObjects = value; } }
	public HAPI_Transform[] 		prObjectTransforms {			get { return myObjectTransforms; } 
																	set { myObjectTransforms = value; } }

	// Geos --------------------------------------------------------------------------------------------------------

	public HoudiniGeoAttributeManagerMap prGeoAttributeManagerMap
	{
		get 
		{ 
			if ( myGeoAttributeManagerMap == null )
			{
				myGeoAttributeManagerMap = ScriptableObject.CreateInstance< HoudiniGeoAttributeManagerMap >();
				myGeoAttributeManagerMap.name = prAssetName + "_GeoAttributeManagerMap";
			}
			return myGeoAttributeManagerMap;
		}
		private set {}
	}

	// Baking ------------------------------------------------------------------------------------------------------

	public float					prBakeStartTime {				get { return myBakeStartTime; }
																	set { myBakeStartTime = value; } }

	public float					prBakeEndTime {					get { return myBakeEndTime; }
																	set { myBakeEndTime = value; } }

	public int						prBakeSamplesPerSecond {		get { return myBakeSamplesPerSecond; }
																	set { myBakeSamplesPerSecond = value; } }

	// GUI ----------------------------------------------------------------------------------------------------------
	
	public bool 					prShowHoudiniControls {			get { return myShowHoudiniControls; } 
																	set { myShowHoudiniControls = value; } }
	public bool						prShowCookLog {					get { return myShowCookLog; }
																	set { myShowCookLog = value; } }
	public bool 					prShowHelp {					get { return myShowHelp; } 
																	set { myShowHelp = value; } }
	public bool 					prShowAssetSettings {			get { return myShowAssetSettings; } 
																	set { myShowAssetSettings = value; } }
	public bool 					prShowBakeOptions {				get { return myShowBakeOptions; } 
																	set { myShowBakeOptions = value; } }
	public bool						prShowPaintTools {				get { return myShowPaintTools; }
																	set { myShowPaintTools = value; } }
	
	public bool						prShowInputControls {			get { return myShowInputControls; } 
																	set { myShowInputControls = value; } }
	public int						prAssetSettingsCategory {		get { return myAssetOptionsCategory; }
																	set { myAssetOptionsCategory = value; } }

	public bool						prIsGeoVisible {				get { return myIsGeoVisible; }
																	set { myIsGeoVisible = value; } }
	public bool						prShowPinnedInstances {			get { return myShowPinnedInstances; }
																	set { myShowPinnedInstances = value; } }

	public bool						prShowOnlyVertexColours{get { return myShowOnlyVertexColours; }
															set { myShowOnlyVertexColours = value; } }
	public bool						prGenerateUVs {			get { return myGenerateUVs; }
															set { myGenerateUVs = value; } }
	public bool						prGenerateLightmapUV2s {get { return myGenerateLightmapUV2s; }
															set { myGenerateLightmapUV2s = value; } }
	public bool						prGenerateTangents {	get { return myGenerateTangents; } 
															set { myGenerateTangents = value; } }

	public bool	prEnableCooking {				get { return (	myEnableCooking && 
																HoudiniHost.isEnableCookingDefault() )
														  || (	HoudiniHost.prEnableCooking &&
																!HoudiniHost.isEnableCookingDefault() ); } 
												set { myEnableCooking = value; } }
	public bool	prCookingTriggersDownCooks {	get { return (	myCookingTriggersDownCooks && 
																HoudiniHost.isCookingTriggersDownCooksDefault() )
														  || (	HoudiniHost.prCookingTriggersDownCooks &&
																!HoudiniHost.isCookingTriggersDownCooksDefault() ); } 
												set { myCookingTriggersDownCooks = value; } }
	public bool	prPlaymodePerFrameCooking {		get { return (	myPlaymodePerFrameCooking && 
																HoudiniHost.isPlaymodePerFrameCookingDefault() )
														  || (	HoudiniHost.prPlaymodePerFrameCooking &&
																!HoudiniHost.isPlaymodePerFrameCookingDefault() ); } 
												set { myPlaymodePerFrameCooking = value; } }
	public bool	prPushUnityTransformToHoudini {	get { return (	myPushUnityTransformToHoudini && 
																HoudiniHost.isPushUnityTransformToHoudiniDefault() )
														  || (	HoudiniHost.prPushUnityTransformToHoudini &&
																!HoudiniHost.isPushUnityTransformToHoudiniDefault() ); } 
												set { myPushUnityTransformToHoudini = value; } }
	public bool	prTransformChangeTriggersCooks{ get { return (	myTransformChangeTriggersCooks && 
																HoudiniHost.isTransformChangeTriggersCooksDefault() )
														  || (	HoudiniHost.prTransformChangeTriggersCooks &&
																!HoudiniHost.isTransformChangeTriggersCooksDefault() ); } 
												set { myTransformChangeTriggersCooks = value; } }

	public bool	prImportTemplatedGeos { get { return (			myImportTemplatedGeos && 
																HoudiniHost.isImportTemplatedGeosDefault() )
														  || (	HoudiniHost.prImportTemplatedGeos &&
																!HoudiniHost.isImportTemplatedGeosDefault() ); } 
												set { myImportTemplatedGeos = value; } }

	public bool prSplitGeosByGroupOverride {	get { return mySplitGeosByGroupOverride; }
												set { mySplitGeosByGroupOverride = value; } }
	public bool prSplitGeosByGroup { get { return prSplitGeosByGroupOverride
														? mySplitGeosByGroup
														: HoudiniHost.prSplitGeosByGroup; } 
												set { mySplitGeosByGroup = value; } }
	public bool prSplitPointsByVertexAttributeOverride {	get { return mySplitPointsByVertexAttributesOverride; }
															set { mySplitPointsByVertexAttributesOverride = value; } }
	public bool prSplitPointsByVertexAttribute { get { return mySplitPointsByVertexAttributesOverride
																? mySplitPointsByVertexAttributes
																: HoudiniHost.prSplitPointsByVertexAttributes; }
												set { mySplitPointsByVertexAttributes = value; } }

	public bool prOmitPartNameEnumeration {		get { return myOmitPartNameEnumeration; }
												set { myOmitPartNameEnumeration = value; } }

	public bool						prEnableLogging {				get { return myEnableLogging; } 
																	set { myEnableLogging = value; } }

	public List< string >			prTransInputNames {				get { return myTransInputNames; }
																	set { myTransInputNames = value; } }
	public List< string >			prGeoInputNames {				get { return myGeoInputNames; }
																	set { myGeoInputNames = value; } }

	// Hooks --------------------------------------------------------------------------------------------------------

	public HoudiniApiAssetHook[] prAssetHooks { get { return GetComponents< HoudiniApiAssetHook >(); } }
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods
	
	public HoudiniAsset() 
	{
		if ( prEnableLogging )
			Debug.Log( "HoudiniAsset created - Instance Id: " + GetInstanceID() );
		
		HoudiniSetPath.setPath();

		reset();
	}
	
	~HoudiniAsset() 
	{
		if ( prEnableLogging )
			Debug.Log( "HoudiniAsset destroyed - Instance Id:" + GetInstanceID() );
	}

	public int findObjectByName( string object_name )
	{
		for ( int object_index = 0; object_index < prObjectCount; ++object_index )
		{
			HAPI_ObjectInfo object_info = prObjects[ object_index ];
			if ( object_info.name == object_name )
				return object_index;
		}
		return -1;
	}
	
	public int findObjectByNodeId( int object_id )
	{
		for ( int object_index = 0; object_index < prObjectCount; ++object_index )
		{
			HAPI_ObjectInfo object_info = prObjects[ object_index ];
			if ( object_info.nodeId == object_id )
				return object_index;
		}
		return -1;
	}
	
	public GameObject findPartByName( string part_name, bool allow_partial_match )
	{
		for ( int object_index = 0; object_index < prObjectCount; ++object_index )
		{
			GameObject top_level_obj = prGameObjects[ object_index ];
			for( int geo_index = 0; geo_index < top_level_obj.transform.childCount; geo_index++ )
			{
				GameObject geo = top_level_obj.transform.GetChild( geo_index ).gameObject;
				for( int part_index = 0; part_index < geo.transform.childCount; part_index++ )
				{
					GameObject part = geo.transform.GetChild( part_index ).gameObject;
					if( allow_partial_match )
					{
						if( part.name.StartsWith( part_name ) )
							return part;
					}
					else
					{
						if( part.name == part_name )
							return part;
					}
				}
			}
			
		}
		return null;
	}

    // Transform related connection methods -------------------------------------------------------

	public void addAssetAsTransformInput( HoudiniAsset asset, int index )
	{
		if ( prUpStreamTransformAssets[ index ] == asset )
			return;
		
		prUpStreamTransformAssets[ index ] = asset;
        HoudiniHost.connectNodeInput(prNodeId, index, asset.prNodeId);
		//HoudiniHost.connectAssetTransform( asset.prAssetId, prAssetId, index );
		asset.addDownstreamTransformAsset( this );
	}
	
	public void removeTransformInput( int index )
	{
		if ( prUpStreamTransformAssets[ index ] != null )
		{
			prUpStreamTransformAssets[ index ].removeDownstreamTransformAsset( this );
            HoudiniHost.disconnectNodeInput( prNodeId, index );
			// HoudiniHost.disconnectAssetTransform( prAssetId, index );
			prUpStreamTransformAssets[ index ] = null;
		}
	}
	
	public void removeAssetAsTransformInput( HoudiniAsset asset )
	{
		for ( int ii = 0; ii < prUpStreamTransformAssets.Count; ++ii )
		{
			if ( prUpStreamTransformAssets[ii] == asset )
			{
				prUpStreamTransformAssets[ ii ] = null;
                HoudiniHost.disconnectNodeInput( prNodeId, ii );
				//HoudiniHost.disconnectAssetTransform( prAssetId, ii );
				
				asset.removeDownstreamTransformAsset( this );
			}
		}
	}
	
	public int getAssetTransformConnectionIndex( HoudiniAsset asset )
	{
		for ( int ii = 0; ii < prUpStreamTransformAssets.Count; ii++ )
			if ( prUpStreamTransformAssets[ii] == asset )
				return ii;
		
		return -1;
	}
	
	public void addDownstreamTransformAsset( HoudiniAsset asset )
	{
		foreach ( HoudiniAsset downstream_asset in prDownStreamTransformAssets )
			if ( downstream_asset == asset )
				return;
		
		prDownStreamTransformAssets.Add( asset );
	}
	
	public void removeDownstreamTransformAsset( HoudiniAsset asset )
	{
		prDownStreamTransformAssets.Remove( asset );
	}
	
	// Geometry related connection methods -------------------------------------------------------
	
	public void addAssetAsGeoInput( HoudiniAsset asset, int object_index, int index )
	{
		prUpStreamGeoAssets[ index ] = asset;

        HoudiniHost.connectNodeInput( prNodeId, index, asset.prNodeId );
        //HoudiniHost.connectAssetGeometry( asset.prAssetId, object_index, prAssetId, index );
        asset.addDownstreamAsset( this );

		// We have to save the presets here because this connection might change a parm
		// and we want to save it.
#if UNITY_EDITOR
		if ( !EditorApplication.isPlaying )
			savePreset();
#endif // UNITY_EDITOR
	}

	public void addGeoAsGeoInput( GameObject obj, int index )
	{
		addGeoAsGeoInput( obj, index, false );
	}
	public void addGeoAsGeoInput( GameObject obj, int index, bool is_duplication )
	{
		if ( is_duplication ||
			prUpStreamGeoInputAssetIds[ index ] < 0 || 
			!HoudiniHost.isNodeValid( prUpStreamGeoInputAssetIds[ index ], myUpStreamGeoInputAssetValidationIds[ index ] ) )
		{
			// Remove spaces in the node name
			string inputName = prAssetName + "_GeoInput_" + index;
			inputName = inputName.Replace(' ', '_');

			prUpStreamGeoInputAssetIds[index] = HoudiniHost.createInputNode( inputName );

			HAPI_NodeInfo node_info = HoudiniHost.getNodeInfo(prUpStreamGeoInputAssetIds[index]);
			myUpStreamGeoInputAssetValidationIds[index] = node_info.uniqueHoudiniNodeId;
		}

		// Add the geo info onto the asset.
		if ( !obj.GetComponent< MeshFilter >() )
		{
			Debug.LogWarning( "No mesh filter found on input geo object: " + obj.name );
			return;
		}
		Mesh mesh = obj.GetComponent< MeshFilter >().sharedMesh;
		HoudiniAssetUtility.setMesh(
			prUpStreamGeoInputAssetIds[ index ], 0, prUpStreamGeoInputAssetIds[index], ref mesh, null, null );

        // Set the asset transform from the source GameObject transform.
        HAPI_TransformEuler trans = HoudiniAssetUtility.getHapiTransform(obj.transform.localToWorldMatrix);
        HAPI_NodeInfo input_node_info = HoudiniHost.getNodeInfo( prUpStreamGeoInputAssetIds[index] );
        HoudiniHost.setObjectTransform(input_node_info.parentId, ref trans);

        HoudiniHost.connectNodeInput(prNodeId, index, prUpStreamGeoInputAssetIds[index]);
        //HoudiniHost.connectAssetGeometry( prUpStreamGeoInputAssetIds[ index ], 0, prAssetId, index );

        //HAPI_NodeInfo node_info = HoudiniHost.getNodeInfo(prUpStreamGeoInputAssetIds[index]);
        //myUpStreamGeoInputAssetValidationIds[index] = node_info.uniqueHoudiniNodeId;

        // We have to save the presets here because this connection might change a parm
        // and we want to save it.
#if UNITY_EDITOR
        if ( !EditorApplication.isPlaying )
			savePreset();
#endif // UNITY_EDITOR
	}

	public void updateGeoInputTransformType( int input_index, int newTransformType )
	{
		if ( ( input_index < 0 ) || ( input_index >= prGeoInputCount ) )
			return;

		try
		{
			int inputNodeID = HoudiniHost.queryNodeInput( prNodeId, input_index );
			if ( inputNodeID < 0 )
				return;

			string sXformType = "xformtype";
			HoudiniHost.setParmIntValue( inputNodeID, sXformType, 0, newTransformType );

			prGeoInputsTransformTypes[ input_index ] = newTransformType;
		}
		catch ( HoudiniError )
		{
			// Do nothing.
		}
		catch ( Exception error )
		{
			Debug.LogError( error.ToString() );
		}
	}

#if UNITY_EDITOR
	protected void marshalAnimCurve( int node_id, AnimationCurve curve, HAPI_TransformComponent transform_component )
	{
		HAPI_Keyframe[] keys = new HAPI_Keyframe[ curve.length ];
		for ( int ii = 0; ii < curve.length; ii++ )
		{
			Keyframe unity_key = curve.keys[ ii ];
			keys[ ii ].time = unity_key.time;
			keys[ ii ].value = unity_key.value;
			keys[ ii ].inTangent = unity_key.inTangent;
			keys[ ii ].outTangent = unity_key.outTangent;
			
			if ( transform_component == HAPI_TransformComponent.HAPI_TRANSFORM_TX )
			{
				keys[ ii ].value *= -1;
			}
			
			if ( transform_component == HAPI_TransformComponent.HAPI_TRANSFORM_RY ||
				transform_component == HAPI_TransformComponent.HAPI_TRANSFORM_RZ )
			{
				keys[ ii ].value *= -1;
			}
		}
		
		HoudiniHost.setTransformAnimCurve( node_id, transform_component, keys, curve.length );
	}

	protected void marshalRotation( int node_id, AnimationClipCurveData[] curve_datas )
	{
		AnimationCurve qx = null, qy = null, qz = null, qw = null;

		foreach ( AnimationClipCurveData curve_data in curve_datas )
		{
			if( curve_data.propertyName == "m_LocalRotation.x" )
				qx = curve_data.curve;
			else if( curve_data.propertyName == "m_LocalRotation.y" )
				qy = curve_data.curve;
			else if( curve_data.propertyName == "m_LocalRotation.z" )
				qz = curve_data.curve;
			else if( curve_data.propertyName == "m_LocalRotation.w" )
				qw = curve_data.curve;

			if ( qx != null && qy != null && qz != null && qw != null )
				break;
		}
		
		if ( qx != null && qy != null && qz != null && qw != null )
		{
			AnimationCurve rx = new AnimationCurve();
			AnimationCurve ry = new AnimationCurve();
			AnimationCurve rz = new AnimationCurve();
			for ( int ii = 0; ii < qx.length; ii++ )
			{
				Keyframe key_qx = qx.keys[ ii ];
				Keyframe key_qy = qy.keys[ ii ];
				Keyframe key_qz = qz.keys[ ii ];
				Keyframe key_qw = qw.keys[ ii ];

				Quaternion quat = new Quaternion( key_qx.value, key_qy.value, key_qz.value, key_qw.value );
				Vector3 eulerAngle = quat.eulerAngles;

				HoudiniAssetUtility.addKeyToCurve( key_qx.time, eulerAngle.x, rx );
				HoudiniAssetUtility.addKeyToCurve( key_qx.time, -eulerAngle.y, ry );
				HoudiniAssetUtility.addKeyToCurve( key_qx.time, -eulerAngle.z, rz );
			}
			
			marshalAnimCurve( node_id, rx, HAPI_TransformComponent.HAPI_TRANSFORM_RX );
			marshalAnimCurve( node_id, ry, HAPI_TransformComponent.HAPI_TRANSFORM_RY );
			marshalAnimCurve( node_id, rz, HAPI_TransformComponent.HAPI_TRANSFORM_RZ );
		}
	}
#endif // UNITY_EDITOR

	protected void marshalCurvesFromClip( int node_id, AnimationClip clip )
	{
#if UNITY_EDITOR
		#pragma warning disable 0618
		AnimationClipCurveData[] curve_datas = AnimationUtility.GetAllCurves( clip );
		#pragma warning restore 0618
		foreach ( AnimationClipCurveData curve_data in curve_datas )
		{
			if( curve_data.propertyName == "m_LocalPosition.x" )
				marshalAnimCurve( node_id, curve_data.curve, HAPI_TransformComponent.HAPI_TRANSFORM_TX );
			else if( curve_data.propertyName == "m_LocalPosition.y" )
				marshalAnimCurve( node_id, curve_data.curve, HAPI_TransformComponent.HAPI_TRANSFORM_TY );
			else if( curve_data.propertyName == "m_LocalPosition.z" )
				marshalAnimCurve( node_id, curve_data.curve, HAPI_TransformComponent.HAPI_TRANSFORM_TZ );
			else if( curve_data.propertyName == "m_LocalScale.x" )
				marshalAnimCurve( node_id, curve_data.curve, HAPI_TransformComponent.HAPI_TRANSFORM_SX );
			else if( curve_data.propertyName == "m_LocalScale.y" )
				marshalAnimCurve( node_id, curve_data.curve, HAPI_TransformComponent.HAPI_TRANSFORM_SY );
			else if( curve_data.propertyName == "m_LocalScale.z" )
				marshalAnimCurve( node_id, curve_data.curve, HAPI_TransformComponent.HAPI_TRANSFORM_SZ );
		}

		marshalRotation( node_id, curve_datas );
#endif // UNITY_EDITOR
	}

    public bool isGeoInputValid(int index)
    {
        if (prUpStreamGeoObjects[index] != null)
        {
            return prUpStreamGeoAssets[index] != null ||
                (prUpStreamGeoInputAssetIds[index] >= 0 && HoudiniHost.isNodeValid(
                    prUpStreamGeoInputAssetIds[index], myUpStreamGeoInputAssetValidationIds[index]));
        }
        else
        {
            return prUpStreamGeoAssets[index] == null && prUpStreamGeoInputAssetIds[index] == -1;
        }
    }

    public void removeGeoInput(int index)
    {
        try
        {
        	if ( prUpStreamGeoAssets[ index ] != null )
        	{
        		prUpStreamGeoAssets[ index ].removeDownstreamAsset( this );
                HoudiniHost.disconnectNodeInput( prNodeId, index );
                //HoudiniHost.disconnectAssetGeometry( prAssetId, index );
                prUpStreamGeoAssets[ index ] = null;
        	}
        	else if ( prUpStreamGeoInputAssetIds[ index ] >= 0 )
        	{
                HoudiniHost.disconnectNodeInput( prNodeId, index);
                //HoudiniHost.disconnectAssetGeometry( prAssetId, index );
        		HoudiniHost.destroyAsset( prUpStreamGeoInputAssetIds[ index ] );
        		prUpStreamGeoInputAssetIds[ index ] = -1;
        		myUpStreamGeoInputAssetValidationIds[ index ] = -1;
        	}

        	// Full value reset.
        	prUpStreamGeoObjects[ index ] = null;
        	prUpStreamGeoAssets[ index ] = null;
        	prUpStreamGeoInputAssetIds[ index ] = -1;
        	myUpStreamGeoInputAssetValidationIds[ index ] = -1;
        }
        catch ( HoudiniError )
        {
        	// Do nothing. It's usally that we we already disconnected.
        }
        catch ( Exception error )
        {
        	Debug.LogError( error.ToString() );
        }
    }

    public void removeDownstreamAsset( HoudiniAsset asset )
	{
		prDownStreamAssets.Remove( asset );
	}
	
	public void addDownstreamAsset( HoudiniAsset asset )
	{
		foreach ( HoudiniAsset downstream_asset in prDownStreamAssets )
			if ( downstream_asset == asset )
				return;
		
		prDownStreamAssets.Add( asset );
	}

	public virtual void OnDestroy()
	{
		if ( HoudiniHost.isInstallationOk() && prAssetId >= 0 && HoudiniHost.isRealDestroy()
#if UNITY_EDITOR
			&& !BuildPipeline.isBuildingPlayer
#endif // UNITY_EDITOR
			)

		{
            // Remove this asset as transform output from the upstream assets.
			foreach ( HoudiniAsset upstream_asset in prUpStreamTransformAssets )
				if ( upstream_asset != null )
					upstream_asset.removeDownstreamTransformAsset( this );

			// Remove this asset as geo output from the upstream assets.
			foreach ( HoudiniAsset upstream_asset in prUpStreamGeoAssets )
				if ( upstream_asset != null )
					upstream_asset.removeDownstreamAsset( this );

			// Clean up any geo inputs.
			foreach ( int asset_id in prUpStreamGeoInputAssetIds )
			{
				if ( asset_id >= 0 )
				{
					try
					{
						HoudiniHost.destroyAsset( asset_id );
					}
					catch ( HoudiniError error )
					{
						Debug.LogError( "Asset failed to unload input asset: " + error.ToString() );
					}
				}
			}

			{
				List< HoudiniAsset > downstream_asset_list = new List< HoudiniAsset >();
				foreach ( HoudiniAsset downstream_asset in prDownStreamTransformAssets )
					downstream_asset_list.Add( downstream_asset );
				foreach ( HoudiniAsset downstream_asset in downstream_asset_list )
					downstream_asset.removeAssetAsTransformInput( this );
			}

			prUpStreamTransformAssets.Clear();
			prDownStreamTransformAssets.Clear();
			
			try
			{
				HoudiniHost.destroyAsset( prAssetId );
			}
			catch ( HoudiniError error )
			{
				Debug.LogError( "Asset failed to unload: " + error.ToString() );
			}

			prAssetId = -1;
		}
	}
	
	// Methods for determining where OnEnable is being called from -------------------------------

	public bool isDuplicatingAsset()
	{
		foreach ( HoudiniAsset asset in FindObjectsOfType( typeof( HoudiniAsset ) ) as HoudiniAsset[] )
		{
			if ( asset.prAssetId == prAssetId &&
				asset.GetInstanceID() != GetInstanceID() )
			{
				return true;
			}
		}

		return false;
	}

	public virtual void OnEnable()
	{
#if ( HAPI_ENABLE_RUNTIME )
		if ( !HoudiniHost.isInstallationOk() )
			return;

#if UNITY_EDITOR
		if ( BuildPipeline.isBuildingPlayer )
			return;
#endif // UNITY_EDITOR

		bool is_duplication = isDuplicatingAsset();

		if ( prAssetId >= 0 )
		{
			bool is_asset_valid =
				HoudiniHost.isNodeValid( prAssetId, prAssetValidationId );

			if ( is_asset_valid && !is_duplication )
			{
				// Reloading asset after mode change or script-reload.
				build(	false,	// reload_asset
						false,	// unload_asset_first
						true,	// serializatin_recovery_only
						false,	// force_reconnect
						false,	// is_duplication
						true,	// cook_downstream_assets
						true	// use_delay_for_progress_bar
					);
			}
			// It's important we don't reset the prAssetId to -1 if there
			// is no installation because if they save the scene and load it
			// in another Unity session where there is an installation of
			// Houdini then the asset will no longer load.
			else if ( is_duplication )
			{
				prAssetId = -1;
				build(
					true,	// reload_asset
					true,	// unload_asset_first
					false,	// serializatin_recovery_only
					true,	// force_reconnect
					is_duplication,
					true,	// cook_downstream_assets
					false	// use_delay_for_progress_bar
				);
			}
			else
			{
				// Loading Scene (no Houdini scene exists yet).
				prAssetId = -1;
				prNodeId = -1;
			}
		}
#endif // ( HAPI_ENABLE_RUNTIME )
	}

	public virtual void OnDisable()
	{
#if UNITY_EDITOR
		// Not matter what, re-enable the transform gizmo on deletion/disable.
		System.Type type = typeof( Tools );
		FieldInfo field = type.GetField( "s_Hidden", BindingFlags.NonPublic | BindingFlags.Static );
		field.SetValue( null, false );
#endif // UNITY_EDITOR
	}

	public override void reset()
	{
		base.reset();

		// Please keep these in the same order and grouping as their declarations at the top.
		
		// Assets ---------------------------------------------------------------------------------------------------
		
		prAsset							= this;
		prAssetInfo 					= new HAPI_AssetInfo();
		prNodeInfo						= new HAPI_NodeInfo();
		prPreset 						= null;
		prAssetValidationId				= -1;
		prAssetName						= "ASSET_NAME";
		prAssetOpName					= "ASSET_OP_NAME";
		prAssetHelp						= "ASSET_HELP";
		prAssetType						= AssetType.TYPE_INVALID;

		// Parameters -----------------------------------------------------------------------------------------------

		prPresetsMap					= null;

		// Inputs ---------------------------------------------------------------------------------------------------
        prTransformInputCount 			= 0;
		prGeoInputCount 				= 0;
		prGeoInputsTransformTypes		= new List< int >();
		
		prDownStreamTransformAssets		= new List< HoudiniAsset >();
		prUpStreamTransformAssets 		= new List< HoudiniAsset >();
		prUpStreamTransformObjects 		= new List< GameObject >();
		
		prUpStreamGeoAssets 			= new List< HoudiniAsset >();
		prUpStreamGeoObjects 			= new List< GameObject >();
		prUpStreamGeoInputAssetIds		= new List< int >();
		myUpStreamGeoInputAssetValidationIds = new List< int >();

        prDownStreamAssets 			    = new List< HoudiniAsset >();

		// Objects --------------------------------------------------------------------------------------------------
		
		prObjectCount 					= 0;
		prHandleCount 					= 0;
		
		prObjects 						= new HAPI_ObjectInfo[ 0 ];
		
		prGameObjects 					= new GameObject[ 0 ];
		prObjectTransforms 				= new HAPI_Transform[ 0 ];

		// Geos -----------------------------------------------------------------------------------------------------

		prGeoAttributeManagerMap		= null;
		
		// Baking ---------------------------------------------------------------------------------------------------
		
		prBakeStartTime					= 0.0f;
		prBakeEndTime					= 1.0f;
		prBakeSamplesPerSecond			= 30;
		
		// GUI ------------------------------------------------------------------------------------------------------
		
		prShowHoudiniControls 			= true;
		prShowCookLog					= false;
		prShowHelp						= false;
		prShowAssetSettings				= true;
		prShowBakeOptions				= false;
		prShowPaintTools				= false;
		prShowInputControls 			= true;
		prAssetSettingsCategory			= 0;

		prIsGeoVisible					= true;
		prShowPinnedInstances			= true;

		prShowOnlyVertexColours			= false;

		prGenerateUVs					= false;
		prGenerateLightmapUV2s			= false;
		prGenerateTangents				= true;

		prEnableCooking					= HoudiniHost.myDefaultEnableCooking;
		prCookingTriggersDownCooks		= HoudiniHost.myDefaultCookingTriggersDownCooks;
		prPlaymodePerFrameCooking		= false;
		prPushUnityTransformToHoudini	= HoudiniHost.myDefaultPushUnityTransformToHoudini;
		prTransformChangeTriggersCooks	= HoudiniHost.myDefaultTransformChangeTriggersCooks;
		prImportTemplatedGeos 			= HoudiniHost.myDefaultImportTemplatedGeos;
		prSplitGeosByGroupOverride		= false;
		prSplitGeosByGroup				= HoudiniHost.myDefaultSplitGeosByGroup;
		prSplitPointsByVertexAttributeOverride = false;
		prSplitPointsByVertexAttribute  = HoudiniHost.myDefaultSplitPointsByVertexAttributes;
		prOmitPartNameEnumeration		= false;

		prEnableLogging					= false;

		prTransInputNames				= new List< string >();
		prGeoInputNames					= new List< string >();
		
		myProgressBarJustUsed 			= false;
	}
	
	public override void onParmChange()
	{
		base.onParmChange();

		buildClientSide();

		// To keep things consistent with Unity workflow, we should not save parameter changes
		// while in Play mode.
#if UNITY_EDITOR
		if ( !EditorApplication.isPlaying )
			savePreset();
#endif // UNITY_EDITOR
	}

	public virtual bool buildAll()
	{
		return build(	true,	// reload_asset
						true,	// unload_asset_first
						false,	// serializatin_recovery_only
						true,	// force_reconnect
						false,  // is_duplication
						prCookingTriggersDownCooks,	// cook_downstream_assets
						true	// use_delay_for_progress_bar
					);
	}

	public virtual bool buildClientSide()
	{
		if ( prAssetId < 0 )
			return build(
				true,	// reload_asset
				true,	// unload_asset_first
				false,	// serializatin_recovery_only
				true,	// force_reconnect
				false,	// is_duplication
				true,	// cook_downstream_assets
				true	// use_delay_for_progress_bar
			);
		else
			return build(
				false,	// reload_asset
				false,	// unload_asset_first
				false,	// serializatin_recovery_only
				false,	// force_reconnect
				false,	// is_duplication
				prCookingTriggersDownCooks,	// cook_downstream_assets
				true	// use_delay_for_progress_bar
			);
	}

	public virtual bool build( bool reload_asset, bool unload_asset_first,
							   bool serialization_recovery_only,
							   bool force_reconnect,
							   bool is_duplication,
							   bool cook_downstream_assets,
							   bool use_delay_for_progress_bar )
	{
		// We can only build or do anything if we can link to our libraries.
#if !( HAPI_ENABLE_RUNTIME )
		return false;
		#pragma warning disable 0162
#endif // !( HAPI_ENABLE_RUNTIME )

		if ( !HoudiniHost.isInstallationOk() )
			return false;

		if ( !prEnableCooking )
			return false;

		// Run post-cook hook.
		foreach ( var asset_hook in prAssetHooks )
			asset_hook.preCook( this );

		HoudiniProgressBar progress_bar = new HoudiniProgressBar();
		progress_bar.prUseDelay = use_delay_for_progress_bar;
		progress_bar.prAsset = this;

		try
		{
			progress_bar.prStartTime = System.DateTime.Now;

			bool is_first_time_build = false;

			if ( reload_asset ) 
			{	
				if ( unload_asset_first )
				{
					// There's no reason to abort the whole rebuild process because we can't unload
					// the asset first as that would leave the user with no options other than
					// to delete this HAPI asset and create a new one for this OTL.
					try
					{
						HoudiniHost.destroyAsset( prAssetId );
					}
					catch ( HoudiniError ) {}

					// Once an asset is unloaded its id will is obviously no longer valid, so reset it here.
					prAssetId = -1;
					
					// Need to reset the parms as well.
					prParms.reset();
				}

				try
				{
					int asset_id = 0;

					if ( prAssetId < 0 )
						is_first_time_build = true;

					if ( unload_asset_first )
						asset_id = buildCreateAsset( progress_bar );
					else
						asset_id = prAssetId;

					// We need to update the prAssetId in case the cook is aborted/fails 
					// and we need to clean up (unload the asset) in the catch.
					prAssetId = asset_id;

					prAssetInfo = HoudiniHost.getAssetInfo( asset_id );
					prNodeInfo = HoudiniHost.getNodeInfo( asset_id );

					if ( reload_asset )
						Debug.Log( 
							"Houdini Engine: Asset Loaded - ID: " + prAssetInfo.nodeId + "\n" +
							"    Full Name: " + prAssetInfo.fullOpName + "\n" +
							"    Version: " + prAssetInfo.version + "\n" + 
							"    Unique Node Id: " + prNodeInfo.uniqueHoudiniNodeId + "\n" +
							"    Internal Node Path: " + prNodeInfo.internalNodePath + "\n" +
							"    Asset Library File: " + prAssetInfo.filePath + "\n" );
				}
				catch ( HoudiniError error )
				{
					Debug.LogError( "Asset not loaded: " + error.ToString() );
					// Nothing to build since the load failed.

					// Try to unload the asset so it doesn't dangle.
					if ( is_first_time_build )
					{
						try
						{
							HoudiniHost.destroyAsset( prAssetId );
						}
						catch ( HoudiniError ) {}
					}
					
					// Clean up.
					reset();

					// If in play mode, disable live cooks.
#if UNITY_EDITOR
					if ( EditorApplication.isPlaying )
#endif // UNITY_EDITOR
					{
						prPlaymodePerFrameCooking = false;
					}
					
					return false; // false for failed :(
				}
				
			}
			
			prAssetInfo = HoudiniHost.getAssetInfo( prAssetId );
			
			// For convenience we copy some asset info properties locally (since they are constant anyway).
			// More imporantly, structs are not serialized and therefore putting them into their own
			// variables is required in order to maintain state between serialization cycles.
			prAssetId 					= prAssetInfo.nodeId;
			prAssetValidationId			= prNodeInfo.uniqueHoudiniNodeId;
			prNodeId					= prAssetInfo.nodeId;
			prObjectNodeId				= prAssetInfo.objectNodeId;

            prObjectCount 				= HoudiniHost.composeObjectList( prAssetId );
			prHandleCount 				= prAssetInfo.handleCount;

			prAssetName					= prAssetInfo.name;
			prAssetOpName				= prAssetInfo.fullOpName;
			prAssetHelp					= prAssetInfo.helpText;
            prTransformInputCount		= prAssetInfo.transformInputCount;
			prGeoInputCount				= prAssetInfo.geoInputCount;

			// Try to load presets.
			if ( ( reload_asset && unload_asset_first )
#if UNITY_EDITOR
				// Only load presets during serialization recovery if we really need to.
				// The only such case is when we made changes DURING playmode and Unity
				// restores the parameter values from before going into playmode.
				// We only save presets while NOT in playmode which means to restore
				// the Houdini state to before playmode state we need to loadPreset()
				// with the last saved preset. In all other cases, like going INTO
				// playmode, we should avoid this step because loadPreset() WILL
				// cause a cook regardless if the parameters have changed or not which
				// is terrible for large assets.
				|| ( serialization_recovery_only && 
					!EditorApplication.isPlayingOrWillChangePlaymode &&
					!prParms.prValuesEqualToHoudini )
#endif // UNITY_EDITOR
				)
			{
				loadPreset();
				progress_bar.statusCheckLoop();
				
				// Transform may not have been saved as part of the presets so we have to rely 
				// on the serialized value.
				if ( myLastLocalToWorld != Matrix4x4.zero )
				{
					Matrix4x4 world_to_local = Matrix4x4.identity;
					if ( transform.parent )
						world_to_local = transform.parent.worldToLocalMatrix;
					Matrix4x4 local = myLastLocalToWorld * world_to_local;

					transform.localPosition = HoudiniAssetUtility.getPosition( local );
					transform.localRotation = HoudiniAssetUtility.getQuaternion( local );

					Vector3 scale = HoudiniAssetUtility.getScale( local );
					if ( !( Mathf.Approximately( 0.0f, scale.x )
						&& Mathf.Approximately( 0.0f, scale.y )
						&& Mathf.Approximately( 0.0f, scale.z ) ) )
					{
						transform.localScale = HoudiniAssetUtility.getScale( local );
					}
					
					if ( prPushUnityTransformToHoudini )
						pushAssetTransformToHoudini();
				}
			}

			if ( reload_asset )
			{
				progress_bar.prCurrentValue			= 0;
				progress_bar.prTotal				= prObjectCount + prHandleCount;
				
				progress_bar.displayProgressBar();
				myProgressBarJustUsed = true;

                // Add input fields.
                if (is_first_time_build || !force_reconnect)
                    initAssetConnections();

                // Clean up.
                destroyChildren( transform );
				prGameObjects = new GameObject[ prObjectCount ];
			}
				
			if ( reload_asset || serialization_recovery_only )
			{
				// Need to re-acquire all the params for all the child controls that have parms exposed.
				prParms.getParameterValues();
				foreach ( HoudiniParms parms in GetComponentsInChildren< HoudiniParms >() )
					parms.getParameterValues();

				// Custom work during a full build (custom to each subclass).
				buildFullBuildCustomWork( ref progress_bar, is_duplication );
			}
			
			if ( !reload_asset && !serialization_recovery_only )
			{
				progress_bar.displayProgressBar();
				updateParameters( progress_bar );
			}

            // Create local object info caches (transforms need to be stored in a parallel array).
			if ( prObjects == null || prObjects.Length != prObjectCount )
				prObjects = new HAPI_ObjectInfo[ prObjectCount ];
			
			if ( prObjectTransforms == null || prObjectTransforms.Length != prObjectCount )
				prObjectTransforms = new HAPI_Transform[ prObjectCount ];

            // Refresh object info arrays as they are lost after serialization.
            HoudiniHost.composeObjectList( prAssetId );
			HoudiniAssetUtility.getArray1Id(
				prAssetId, HoudiniHost.getComposedObjectList, prObjects, prObjectCount );
			HoudiniAssetUtility.getArray2Id(
				prAssetId, HAPI_RSTOrder.HAPI_SRT, HoudiniHost.getComposedObjectTransforms, 
				prObjectTransforms, prObjectCount );

            bool objects_need_recook = false;
			if ( !serialization_recovery_only )
			{
				// Set asset's transform.
				if ( prPushUnityTransformToHoudini )
					HoudiniAssetUtility.getHoudiniTransformAndApply( prAssetId, prAssetName, transform );
			
				progress_bar.prMessage = "Loading and composing objects...";

				// Destroy/copy non-copiable but still serialized member data.
				if ( is_duplication )
				{
					myAssetOTLUndoInfo = null;
					if ( myGeoAttributeManagerMap != null )
					{
						myGeoAttributeManagerMap = myGeoAttributeManagerMap.copy();
						myGeoAttributeManagerMap.name = prAssetName + "_GeoAttributeManagerMap";
					}
					if ( myPresetsMap != null )
					{
						myPresetsMap = myPresetsMap.clone();
						myPresetsMap.name = prAssetName + "_PresetMap";
					}
				}
			
				// Custom way to load objects (custom to each subclass).
				objects_need_recook = buildCreateObjects( reload_asset, ref progress_bar );
			
				// Process dependent assets.
				if ( cook_downstream_assets )
					processDependentAssets(
						serialization_recovery_only,
						force_reconnect,
						is_duplication,
						use_delay_for_progress_bar );
			}

			// A bit of a hack (but not terrible). If we have presets for other child controls
			// they set their presets by now so we need to rebuild with the new presets.
			if ( objects_need_recook )
			{
				build(
					false,	// reload_asset
					false,	// unload_asset_first
					false,	// serializatin_recovery_only
					false,	// force_reconnect
					false,	// is_duplication
					false,	// cook_downstream_assets
					true	// use_delay_for_progress_bar
				);
			}
		}
		catch ( HoudiniErrorIgnorable ) {}
		catch ( HoudiniErrorProgressCancelled error )
		{
			// If in play mode, disable live cooks.
#if UNITY_EDITOR
			if ( EditorApplication.isPlaying )
#endif // UNITY_EDITOR
			{
				prPlaymodePerFrameCooking = false;
			}

			Debug.LogError( error.ToString() + "\nSource: " + error.Source );
		}
		catch ( HoudiniError error )
		{
			Debug.LogError( error.ToString() + "\nSource: " + error.Source );
		}
		catch ( System.Exception error )
		{
			Debug.LogError( error.ToString() + "\nSource: " + error.Source );
		}
		finally
		{
			progress_bar.clearProgressBar();

			myProgressBarJustUsed = false;

			// Run post-cook hook.
			foreach ( var asset_hook in prAssetHooks )
				asset_hook.postCook( this );
		}

		// We can only build or do anything if we can link to our libraries.
#if !(HAPI_ENABLE_RUNTIME)
#pragma warning restore 0162
#endif // !( HAPI_ENABLE_RUNTIME )

#if UNITY_EDITOR && UNITY_5_3_OR_NEWER
		// Mark the scene dirty after we do a build to make sure the changes get saved
		UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
#endif

		return true;
	}
	
	public void updateParameters( HoudiniProgressBar progress_bar )
	{
		prParms.setChangedParametersIntoHost();

		HoudiniHost.cookNode( prAssetId, prSplitGeosByGroup, prSplitPointsByVertexAttribute, prImportTemplatedGeos );
		progress_bar.statusCheckLoop();

		myProgressBarJustUsed = true;
		
		progress_bar.prTotal = prObjectCount;

		prParms.getParameterValues();
	}

#if ( HAPI_ENABLE_RUNTIME )
	public virtual void Update()
	{
		// The theory here is that we want to update the last local-to-world
		// all the time, not just when we can actually send it over to
		// Houdini. This prevents jumping when moving an asset while
		// no Houini Engine installation is present, for example, saving the
		// scene and then reloading the scene in a Unity session with a valid
		// Houdini installation.
		Matrix4x4 local_to_world = transform.localToWorldMatrix;
		if ( local_to_world == myLastLocalToWorld )
			return;
		myLastLocalToWorld = local_to_world;

		if ( !prPushUnityTransformToHoudini
			|| !HoudiniHost.isInstallationOk()
			|| prAssetId < 0
#if UNITY_EDITOR
			|| EditorApplication.isPlayingOrWillChangePlaymode
#endif // UNITY_EDITOR
			)
			return;

		try
		{
			pushAssetTransformToHoudini();
			savePreset();

			// Process dependent assets.
			// TODO: These steps here might be too slow for some assets and can grind Unity to
			// a halt. But if we are to support all the different effects of transform changes
			// then we do need to do a full build so I'm not sure how to do this more proper.
			// Do note that the build function is fairly conditional and should only build
			// the bare minimum.
			if ( prTransformChangeTriggersCooks )
				buildClientSide();
		}
		catch ( HoudiniError err )
		{
			Debug.LogError( err.ToString() );
		}
	}
#endif // ( HAPI_ENABLE_RUNTIME )

	public void bakeAsset()
	{
#if UNITY_EDITOR && ( HAPI_ENABLE_RUNTIME )
		// Get/Create directory for the asset being baked.
		string baked_asset_path = HoudiniConstants.HAPI_BAKED_ASSETS_PATH + "/" + prAssetName;

		DirectoryInfo baked_asset_dir = new DirectoryInfo( baked_asset_path );
		int i = 0;
		while ( baked_asset_dir.Exists )
		{
			i++;
			if ( i > 1 )
				baked_asset_path = baked_asset_path.Substring( 0, baked_asset_path.Length - 1 ) + i;
			else
				baked_asset_path = baked_asset_path + "_" + i;

			baked_asset_dir = new DirectoryInfo( baked_asset_path ); 
		}
		baked_asset_dir.Create();

		string rel_baked_asset_path = baked_asset_path.Replace( Application.dataPath, "Assets" );

		// Get/Create directory for textures of the asset being baked.
		string textures_path = baked_asset_path + "/Textures";
		string rel_textures_path = rel_baked_asset_path + "/Textures";
		DirectoryInfo textures_dir = new DirectoryInfo( textures_path );
		if ( !textures_dir.Exists )
			textures_dir.Create();

		// Get/Create directory for meshes of the asset being baked.
		string meshes_path = baked_asset_path + "/Meshes";
		string rel_meshes_path = rel_baked_asset_path + "/Meshes";
		DirectoryInfo meshes_dir = new DirectoryInfo( meshes_path );
		if ( !meshes_dir.Exists )
			meshes_dir.Create();

		// Get/Create directory for materials of the asset being baked.
		string materials_path = baked_asset_path + "/Materials";
		string rel_materials_path = rel_baked_asset_path + "/Materials";
		DirectoryInfo materials_dir = new DirectoryInfo( materials_path );
		if ( !materials_dir.Exists )
			materials_dir.Create();

		// Get/Create directory for shaders of the asset being baked.
		string shaders_path = baked_asset_path + "/Shaders";
		string rel_shaders_path = rel_baked_asset_path + "/Shaders";
		DirectoryInfo shaders_dir = new DirectoryInfo( shaders_path );
		if ( !shaders_dir.Exists )
			shaders_dir.Create();
				
		// Create new game object that is a copy of this asset except with all the HAPI components 
		// removed. This new game object will be used to create a prefab.
		GameObject new_object = Instantiate( gameObject ) as GameObject;

		// Bake all meshes and materials created by HAPI_PartControls.
		foreach ( HoudiniPartControl part_control in new_object.GetComponentsInChildren< HoudiniPartControl >() )
		{
			if ( part_control.prObjectControl.GetComponent< HoudiniInstance >() )
				continue;

			// We don't want to bake any intermediate meshes.
			if ( part_control.prGeoType == HAPI_GeoType.HAPI_GEOTYPE_INTERMEDIATE )
				continue;

			// Bake meshes.
			MeshFilter mesh_filter = part_control.GetComponent< MeshFilter >();
			if ( mesh_filter )
			{
				Mesh mesh = mesh_filter.sharedMesh;

				string mesh_name =
					part_control.prGeoControl.prObjectControl.name + "_" +
					part_control.prGeoControl.name + "_" +
					part_control.name + "_" +
					"mesh";
				string mesh_path = rel_meshes_path + "/" + mesh_name + ".asset";
				Mesh mesh_copy = Mesh.Instantiate( mesh ) as Mesh;
				mesh_copy.name = mesh_name;
				mesh_filter.sharedMesh = mesh_copy;

				// Assigned saved mesh to mesh collider also.
				MeshCollider mesh_collider = part_control.GetComponent< MeshCollider >();
				if ( mesh_collider )
				{
					mesh_collider.sharedMesh = mesh_copy;
					mesh_collider.enabled = false;
					mesh_collider.enabled = true;
				}

				AssetDatabase.CreateAsset( mesh_filter.sharedMesh, mesh_path );
				AssetDatabase.SaveAssets();
			}

			// Bake materials.
			MeshRenderer mesh_renderer = part_control.GetComponent< MeshRenderer >();
			if ( mesh_renderer )
			{
				Material material = mesh_renderer.sharedMaterial;

				// Only bake material if using internal material.
				if ( !AssetDatabase.Contains( material ) )
				{
					Material material_copy = Material.Instantiate( material ) as Material;
					mesh_renderer.sharedMaterial = material_copy;

					// Bake shader needed by material.
					if ( material_copy.shader )
					{
						Shader shader_copy = Shader.Instantiate( material_copy.shader ) as Shader;

						string shader_name =
							shader_copy.name.Substring( Mathf.Max( 0, shader_copy.name.LastIndexOf( '/' ) + 1 ) );
						string shader_path =
							rel_shaders_path + "/" + shader_name + ".asset";

						AssetDatabase.CreateAsset( shader_copy, shader_path );
						AssetDatabase.SaveAssets();

						material_copy.shader = shader_copy;
					}

					// Bake texture needed by material.
					if ( material_copy.mainTexture )
					{
						string texture_name =
							Path.GetFileName( AssetDatabase.GetAssetPath( material_copy.mainTexture ) ); 

						if ( texture_name == "" )
						{
							// This is for when we use in-memory textures. We need to actually
							// bake them out to file now.

							Texture2D orig_texture = material_copy.mainTexture as Texture2D;
							byte[] orig_texture_png = orig_texture.EncodeToPNG();

							string name =
								part_control.prObjectName + "_" +
								part_control.prGeoName + "_" +
								part_control.prPartName + ".png";
							string texture_path = rel_textures_path + "/" + name;

							File.WriteAllBytes( texture_path, orig_texture_png );

							AssetDatabase.ImportAsset( texture_path, ImportAssetOptions.Default );
							Texture2D texture =
								AssetDatabase.LoadAssetAtPath( texture_path, typeof(Texture2D) ) as Texture2D;;
							material_copy.mainTexture = texture;
						}
						else
						{
							string texture_path = rel_textures_path + "/" + texture_name;

							Texture2D texture =
								AssetDatabase.LoadAssetAtPath( texture_path, typeof(Texture2D) ) as Texture2D;
							if ( !texture )
							{
								AssetDatabase.CopyAsset(
									AssetDatabase.GetAssetPath( material_copy.mainTexture ),
									texture_path );
								AssetDatabase.ImportAsset( texture_path, ImportAssetOptions.Default );

								texture =
									AssetDatabase.LoadAssetAtPath( texture_path, typeof(Texture2D) ) as Texture2D;
							}

							material_copy.mainTexture = texture;
						}
					}

					string material_name =
						part_control.prGeoControl.prObjectControl.name + "_" +
						part_control.prGeoControl.name + "_" +
						part_control.name + "_" +
						"mat";
					string mat_path = rel_materials_path + "/" + material_name + ".asset";
					material_copy.name = material_name;

					if ( !AssetDatabase.Contains( material_copy ) )
					{
						AssetDatabase.CreateAsset( material_copy, mat_path );
						AssetDatabase.SaveAssets();
					}
					else
					{
						Debug.LogWarning(
							"Note: " + material_copy.name + " is an external Unity material " +
							"and will not be saved in the Baked Assets folder.\n" +
							"The prefab will depend on this material existing in:\n" +
							AssetDatabase.GetAssetPath( material_copy ) );
					}
				}
			}
		}

		// Re-instance to have the instances use the copied (and baked) meshes from the
		// now-baked instanced objects.
		foreach ( HoudiniInstancer instancer in new_object.GetComponentsInChildren< HoudiniInstancer >() )
		{
			HoudiniProgressBar progress_bar = new HoudiniProgressBar();
			instancer.instanceObjects( progress_bar );
		}

		// Delete all HAPI components from prefab. (Order here matters because of inter-dependencies!)
		// Note: If you move or edit this list please update the wiki reminder about this list
		// on the Runtime wiki page.
		removeComponentsFromChildren< HoudiniCurve >( new_object );
		removeComponentsFromChildren< HoudiniInstancerManager >( new_object );
		removeComponentsFromChildren< HoudiniInstancer >( new_object );
		removeComponentsFromChildren< HoudiniInstance >( new_object );
		removeComponentsFromChildren< HoudiniParms >( new_object );
		removeComponentsFromChildren< HoudiniControl >( new_object );

		// Create prefab.
		string prefab_path = rel_baked_asset_path + "/" + gameObject.name + ".prefab";
		PrefabUtility.CreatePrefab( prefab_path, new_object );
		AssetDatabase.SaveAssets();

		// Destroy object we created because we don't need it anymore.
		DestroyImmediate( new_object );
#endif // UNITY_EDITOR && ( HAPI_ENABLE_RUNTIME )
	}
	
	public void bakeAnimations( float start_time, 
								float end_time, 
								int samples_per_second,
								GameObject parent_object,
								HoudiniProgressBar progress_bar )
	{
#if UNITY_EDITOR
		try
		{
			//This build is needed in case the user changed a geometry input to the asset.  
			//The geometry to be instanced needs to be re-brought in.
			buildClientSide();
			
			int num_objects = prObjects.Length;
			for ( int ii = 0; ii < num_objects; ii++ )
			{
				GameObject game_object = prGameObjects[ ii ];
				HAPI_ObjectInfo obj_info = prObjects[ ii ];
				
				if ( game_object != null )
				{
					if ( !obj_info.isInstancer )
					{
						HoudiniObjectControl obj_control = game_object.GetComponent< HoudiniObjectControl >();
						obj_control.beginBakeAnimation();
					}
					else
					{
						HoudiniInstancer instancer = game_object.GetComponent< HoudiniInstancer >();
						instancer.beginBakeAnimation();
					}
				}
			}
			
			int num_samples = Mathf.CeilToInt(samples_per_second*( end_time - start_time ));
				
			float total_sim_time = ( end_time - start_time );
			float delta_time = total_sim_time / (float) num_samples;
			
			progress_bar.prTotal = num_samples;
			progress_bar.prCurrentValue = 0;
			for ( float curr_time = start_time; curr_time <= end_time; curr_time += delta_time )
			{
				HoudiniHost.setTime( curr_time );
				
				HoudiniHost.cookNode( prAssetId, prSplitGeosByGroup, prSplitPointsByVertexAttribute, prImportTemplatedGeos );
				
				HAPI_State state = HAPI_State.HAPI_STATE_STARTING_LOAD;
					
				while ( (int) state > (int) HAPI_State.HAPI_STATE_MAX_READY_STATE )
					state = (HAPI_State) HoudiniHost.getStatus( HAPI_StatusType.HAPI_STATUS_COOK_STATE );

				if ( state == HAPI_State.HAPI_STATE_READY_WITH_COOK_ERRORS )
				{
					state = HAPI_State.HAPI_STATE_READY;
					Debug.LogWarning(
						"Cook Errors at time: " + curr_time + "\n" + HoudiniHost.getCookErrorMessage() );
				}
				else if ( state == HAPI_State.HAPI_STATE_READY_WITH_FATAL_ERRORS )
				{
					state = HAPI_State.HAPI_STATE_READY;
					HoudiniHost.throwCookError();
				}

				int object_count = HoudiniHost.composeObjectList( prAssetId );
				HAPI_Transform[] object_transforms = new HAPI_Transform[ object_count ];
				HoudiniAssetUtility.getArray2Id(
					prAssetId, HAPI_RSTOrder.HAPI_SRT, HoudiniHost.getComposedObjectTransforms, 
					object_transforms, object_count );

				for ( int ii = 0; ii < num_objects; ii++ )
				{
					GameObject game_object = prGameObjects[ ii ];
					HAPI_ObjectInfo obj_info = prObjects[ ii ];
					
					if ( game_object != null )
					{
						if ( !obj_info.isInstancer )
						{
							HoudiniObjectControl obj_control = game_object.GetComponent< HoudiniObjectControl >();
							obj_control.bakeAnimation( curr_time, parent_object, object_transforms[ ii ] );
						}
						else
						{
							HoudiniInstancer instancer = game_object.GetComponent< HoudiniInstancer >();
							instancer.bakeAnimation( curr_time, parent_object );
						}
					}
				}			
				
				// Set progress bar information.
				progress_bar.prCurrentValue++;
				progress_bar.prMessage = "Baking: " + progress_bar.prCurrentValue + " of " + num_samples;
				progress_bar.displayProgressBar();
			}
						
			bool found_anim = false;
			for ( int ii = 0; ii < num_objects; ii++ )
			{
				GameObject game_object = prGameObjects[ ii ];
				HAPI_ObjectInfo obj_info = prObjects[ ii ];
				
				if ( game_object != null )
				{
					if ( !obj_info.isInstancer )
					{
						HoudiniObjectControl obj_control = game_object.GetComponent< HoudiniObjectControl >();
						if ( obj_control.endBakeAnimation() )
							found_anim = true;
					}
					else
					{
						HoudiniInstancer instancer = game_object.GetComponent< HoudiniInstancer >();
						if ( instancer.endBakeAnimation( parent_object ) )
							found_anim = true;
					}
				}
			}
			
			if ( !found_anim )
			{
				EditorUtility.DisplayDialog( "Bake Error", "No animation was found to bake", "OK" );
			}
		}
		catch ( HoudiniError error )
		{
			Debug.LogWarning( error.ToString() );
		}
#endif // UNITY_EDITOR
	}

	public void loadPreset()
	{
		try
		{
			if ( myPreset != null && myPreset.Length > 0 )
			{
				HoudiniHost.setPreset( prNodeId, myPreset );
				HoudiniHost.cookNode( prAssetId, prSplitGeosByGroup, prSplitPointsByVertexAttribute, prImportTemplatedGeos );
			}
		}
		catch ( HoudiniError error )
		{
			Debug.LogWarning( error.ToString() );
		}
		catch
		{
			Debug.LogWarning( "Unable to load presets." );
		}
	}

	public void savePreset()
	{
#if UNITY_EDITOR
		try
		{
			myPreset = HoudiniHost.getPreset( prNodeId );
		}
		catch {} // Just catch them here but don't report them because we would just get a huge stream of errors.
#endif // UNITY_EDITOR
	}

	public bool isAssetValid()
	{
		return HoudiniHost.isNodeValid( prAssetId, prNodeInfo.uniqueHoudiniNodeId );
	}

	public void applyGeoVisibilityToParts()
	{
		HoudiniPartControl[] controls = GetComponentsInChildren< HoudiniPartControl >();
		foreach ( HoudiniPartControl control in controls )
		{
			if ( control.prGeoType != HAPI_GeoType.HAPI_GEOTYPE_INTERMEDIATE
				&& control.gameObject.GetComponent< MeshRenderer >() != null )
				control.gameObject.GetComponent< MeshRenderer >().enabled = prIsGeoVisible;
		}
	}

	public void pushAssetTransformToHoudini()
	{
		Matrix4x4 local_to_world = transform.localToWorldMatrix;

		HAPI_TransformEuler hapi_transform = HoudiniAssetUtility.getHapiTransform( local_to_world );

		if ( prNodeInfo.type == HAPI_NodeType.HAPI_NODETYPE_OBJ )
			HoudiniHost.setObjectTransform( prAssetId, ref hapi_transform );
		else
			HoudiniHost.setObjectTransform( prNodeInfo.parentId, ref hapi_transform );

		float [] parm_data = new float[ 3 ];

		try
		{
			HAPI_ParmInfo parm_info = prParms.findParm( "t" );
			HoudiniHost.getParmFloatValues( prNodeId, parm_data, parm_info.floatValuesIndex, 3 );
			for ( int i = 0; i < 3; ++i )
				prParms.prParmFloatValues[ parm_info.floatValuesIndex + i ] = parm_data[ i ];

			parm_info = prParms.findParm( "r" );
			HoudiniHost.getParmFloatValues( prNodeId, parm_data, parm_info.floatValuesIndex, 3 );
			for ( int i = 0; i < 3; ++i )
				prParms.prParmFloatValues[ parm_info.floatValuesIndex + i ] = parm_data[ i ];

			parm_info = prParms.findParm( "s" );
			HoudiniHost.getParmFloatValues( prNodeId, parm_data, parm_info.floatValuesIndex, 3 );
			for ( int i = 0; i < 3; ++i )
				prParms.prParmFloatValues[ parm_info.floatValuesIndex + i ] = parm_data[ i ];
		}
		catch {}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Protected Methods

	// Build Custom Work Methods -----------------------------------------------------------------------------------
	
	// Inherited classes should override this with their specific call to the HAPI_Host asset create method.
	// For example: OTLs need to call HAPI_Host.loadOTL( path ), curves need to call HAPI_Host.createCurve().
	protected abstract int buildCreateAsset( HoudiniProgressBar progress_bar );

	// Inherited classes should override this for work they need done during the full build step only. (Optional)
	protected virtual void buildFullBuildCustomWork(
		ref HoudiniProgressBar progress_bar,
		bool is_duplication )
	{}

	// Inherited classes should override this with however they wish to load objects in the prObjects array.
	// Returns whether or not the objects require a recook.
	protected abstract bool buildCreateObjects( bool reload_asset, ref HoudiniProgressBar progress_bar );

	// -------------------------------------------------------------------------------------------------------------
    protected void initAssetConnections()
	{
     	if (prNodeInfo.type == HAPI_NodeType.HAPI_NODETYPE_OBJ )
		{
			if ( prTransformInputCount > 0 && prUpStreamTransformAssets.Count <= 0 )
				for ( int ii = 0; ii < prTransformInputCount ; ++ii )
				{
					prUpStreamTransformAssets.Add( null );
					prUpStreamTransformObjects.Add( null );
				}
		}
	
		if ( prGeoInputCount > 0 && prUpStreamGeoAssets.Count <= 0 )
			for ( int ii = 0; ii < prGeoInputCount ; ++ii )
			{
				prUpStreamGeoAssets.Add( null );
				prUpStreamGeoObjects.Add( null );
				prUpStreamGeoInputAssetIds.Add( -1 );
				myUpStreamGeoInputAssetValidationIds.Add( -1 );
				prGeoInputsTransformTypes.Add(1);
			}

		if (prNodeInfo.type == HAPI_NodeType.HAPI_NODETYPE_OBJ )
			for ( int ii = 0; ii < prTransformInputCount ; ++ii )
				if ( prUpStreamTransformAssets[ ii ] )
					HoudiniHost.connectNodeInput( prNodeId, ii, prUpStreamTransformAssets[ii].prNodeId );
					//HoudiniHost.connectAssetTransform( prUpStreamTransformAssets[ ii ].prAssetId, prAssetId, ii );

		for ( int ii = 0; ii < prGeoInputCount ; ++ii )
		{
			// Reconnect the Geo inputs
			if ( prUpStreamGeoAssets[ ii ] && ( prUpStreamGeoAssets[ ii ].prNodeId >= 0 ) )
			{
				HoudiniHost.connectNodeInput( prNodeId, ii, prUpStreamGeoAssets[ ii ].prNodeId );
			}
			else if ( prUpStreamGeoObjects[ ii ] && ( prUpStreamGeoInputAssetIds[ ii ] >= 0 ) )
			{
				HoudiniHost.connectNodeInput( prNodeId, ii, prUpStreamGeoInputAssetIds[ ii ] );
			}

			// Update their Transform Type
			updateGeoInputTransformType( ii, prGeoInputsTransformTypes[ ii ] );
		}

		foreach ( HoudiniAsset downstream_asset in prDownStreamTransformAssets )
		{
			int index = downstream_asset.getAssetTransformConnectionIndex( this );
			if ( index >= 0 )
                HoudiniHost.connectNodeInput( downstream_asset.prNodeId, index, prNodeId );
                //HoudiniHost.connectAssetTransform( prAssetId, downstream_asset.prAssetId, index );
		}
		
		// Fill input names.
		for ( int i = 0; i < prTransformInputCount; ++i )
		{
			string trans_input_name = HoudiniHost.getNodeInputName( prAssetId, i );
			if ( trans_input_name == "" )
				trans_input_name = "Transform Input #" + ( i + 1 );
			prTransInputNames.Add( trans_input_name );
		}
		for ( int i = 0; i < prGeoInputCount; ++i )
		{
			string geo_input_name = HoudiniHost.getNodeInputName( prAssetId, i );
			if ( geo_input_name == "" )
				geo_input_name = "Geometry Input #" + ( i + 1 );
			prGeoInputNames.Add( geo_input_name );
		}
	}
	protected virtual void processDependentAssets(
		bool serialization_recovery_only,
		bool force_reconnect,
		bool is_duplication,
		bool use_delay_for_progress_bar )
	{
		if ( !serialization_recovery_only )
		{
			foreach ( HoudiniAsset downstream_asset in prDownStreamAssets )
			{
				prEnableCooking = false;
				if ( !downstream_asset.isAssetValid() )
					downstream_asset.onParmChange();
				downstream_asset.build(
					false, // reload_asset
					false, // unload_asset_first
					false, // serialization_recovery_only
					false, // force_reconnect
					false, // is_duplication
					downstream_asset.prCookingTriggersDownCooks,
					use_delay_for_progress_bar );
				prEnableCooking = true;
			}
		}

		if ( !force_reconnect )
			return;

		bool need_rebuild_after_reconnect = false;
		foreach ( var parm in prParms.prParms )
		{
			if ( !parm.isNode() )
				continue;

			need_rebuild_after_reconnect |= prParms.setNodeParameterIntoHost( parm.id );
		}

		// Need to rebuild because now we're connected to other assets.
		if ( need_rebuild_after_reconnect )
			build(
				false, // reload_asset
				false, // unload_asset_first
				false, // serialization_recovery_only
				false, // force_reconnect
				false, // is_duplication
				true,  // cook_downstream_assets
				use_delay_for_progress_bar );
	}
	
	// PROGRESS BAR -------------------------------------------------------------------------------------------------
	
	public bool hasProgressBarBeenUsed()
	{
		return myProgressBarJustUsed;
	}
	
	protected bool				myProgressBarJustUsed;
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Serialized Private Data
	
	// Assets -------------------------------------------------------------------------------------------------------
	
	[SerializeField] private HAPI_AssetInfo			myAssetInfo;
	[SerializeField] private HAPI_NodeInfo			myNodeInfo;
	[SerializeField] private byte[]					myPreset;
	[SerializeField] private int					myAssetValidationId;
	[SerializeField] private string					myAssetName;
	[SerializeField] private string					myAssetOpName;
	[SerializeField] private string					myAssetHelp;
	[SerializeField] private AssetType				myAssetType;

	// Inputs -------------------------------------------------------------------------------------------------------
    	
	[SerializeField] private int 					myTransformInputCount;
	[SerializeField] private int					myGeoInputCount;
	[SerializeField] private List<int>				myGeoInputsTransformTypes;

	[SerializeField] private List< HoudiniAsset >	myDownStreamTransformAssets;
	[SerializeField] private List< HoudiniAsset >	myUpStreamTransformAssets;
	[SerializeField] private List< GameObject >		myUpStreamTransformObjects;
	
	[SerializeField] private List< HoudiniAsset >	myUpStreamGeoAssets;
	[SerializeField] private List< GameObject >		myUpStreamGeoObjects;
	[SerializeField] private List< int >			myUpStreamGeoInputAssetIds;
	[SerializeField] private List< int >			myUpStreamGeoInputAssetValidationIds;

    [SerializeField] private List< HoudiniAsset >	myDownStreamAssets;

	// Parameters ---------------------------------------------------------------------------------------------------

	[SerializeField] private HoudiniPresetMap		myPresetsMap;

	// Objects ------------------------------------------------------------------------------------------------------
	
	[SerializeField] private int 					myObjectCount;
	[SerializeField] private int					myHandleCount;
	
	[SerializeField] private GameObject[]			myGameObjects;

					 private HAPI_ObjectInfo[] 		myObjects;
					 private HAPI_Transform[] 		myObjectTransforms;

	// Geos ---------------------------------------------------------------------------------------------------------

	[SerializeField] private HoudiniGeoAttributeManagerMap myGeoAttributeManagerMap;

	// Baking -------------------------------------------------------------------------------------------------------
	
	[SerializeField] private float 					myBakeStartTime;
	[SerializeField] private float					myBakeEndTime;
	[SerializeField] private int 					myBakeSamplesPerSecond;

	// GUI ----------------------------------------------------------------------------------------------------------
	
	[SerializeField] private bool 					myShowHoudiniControls;
	[SerializeField] private bool					myShowCookLog;
	[SerializeField] private bool					myShowHelp;
	[SerializeField] private bool 					myShowAssetSettings;
	[SerializeField] private bool 					myShowBakeOptions;
	[SerializeField] private bool					myShowPaintTools;
	[SerializeField] private bool					myShowInputControls;
	[SerializeField] private int					myAssetOptionsCategory;

	[SerializeField] private bool					myIsGeoVisible;
	[SerializeField] private bool					myShowPinnedInstances;

	[SerializeField] private bool					myShowOnlyVertexColours;
	[SerializeField] private bool					myGenerateUVs;
	[SerializeField] private bool					myGenerateLightmapUV2s;
	[SerializeField] private bool					myGenerateTangents;

	[SerializeField] private bool					myEnableCooking;
	[SerializeField] private bool					myCookingTriggersDownCooks;
	[SerializeField] private bool					myPlaymodePerFrameCooking;
	[SerializeField] private bool					myPushUnityTransformToHoudini;
	[SerializeField] private bool					myTransformChangeTriggersCooks;
	[SerializeField] private bool					myImportTemplatedGeos;
	[SerializeField] private bool					mySplitGeosByGroupOverride;
	[SerializeField] private bool					mySplitGeosByGroup;
	[SerializeField] private bool					mySplitPointsByVertexAttributesOverride;
	[SerializeField] private bool					mySplitPointsByVertexAttributes;
	[SerializeField] private bool					myOmitPartNameEnumeration;
	
	[SerializeField] private bool					myEnableLogging;

	[SerializeField] private List< string >			myTransInputNames;
	[SerializeField] private List< string >			myGeoInputNames;

	// Private Temporary Data
	[SerializeField] protected Matrix4x4			myLastLocalToWorld;

	[SerializeField] private HoudiniAssetUndoInfo	myAssetOTLUndoInfo;

}
