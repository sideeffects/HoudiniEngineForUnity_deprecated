/*
 * PROPRIETARY INFORMATION.  This software is proprietary to
 * Side Effects Software Inc., and is not to be reproduced,
 * transmitted, or disclosed in any way without written permission.
 *
 * Produced by:
 *      Side Effects Software Inc
 *		123 Front Street West, Suite 1401
 *		Toronto, Ontario
 *		Canada   M5J 2M2
 *		416-504-9876
 *
 * COMMENTS:
 * 
 */

using UnityEngine;
using UnityEditor;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using HAPI;
using Utility = HAPI_AssetUtility;

[ ExecuteInEditMode ]
public class HAPI_AssetCurve : HAPI_Asset 
{	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	
	// Please keep these in the same order and grouping as their initializations in HAPI_Asset.reset().
	
	public List< Vector3 > 	prPoints { get; set; }
	public Vector3[]		prVertices { get; set; }
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods
	
	public HAPI_AssetCurve() 
	{
		if ( prEnableLogging )
			Debug.Log( "HAPI_Asset created!" );
		
		reset();
	}
	
	~HAPI_AssetCurve()
	{
		if ( prEnableLogging )
			Debug.Log( "HAPI_Asset destroyed!" );
	}
	
	public override void OnDestroy()
	{
		if ( prAssetId >= 0 )
		{
			HAPI_Host.unloadOTL( prAssetId );
			prAssetId = -1;
		}
	}
	
	public void addPoint( Vector3 pos )
	{
		prPoints.Add( pos );
		
		updatePoints();
	}
	
	public void updatePoint( int index, Vector3 pos )
	{
		prPoints[ index ] = pos;
		
		updatePoints();
	}
	
	public void updatePoints()
	{
		string parm = "";
		for ( int i = 0; i < prPoints.Count; ++i )
		{
			parm += prPoints[ i ][ 0 ];
			parm += ",";
			parm += prPoints[ i ][ 1 ];
			parm += ",";
			parm += prPoints[ i ][ 2 ];
			parm += " ";
		}
		
		HAPI_Host.setParmStringValue( prAssetId, parm, 2, 0 );
		
		build();
	}
	
	public override void reset()
	{
		base.reset();
		
		// Please keep these in the same order and grouping as their declarations at the top.
		
		prPoints 		= new List< Vector3 >();
		prVertices 		= new Vector3[ 0 ];
	}
	
	public override bool build() 
	{
		bool base_built = base.build();
		if ( !base_built )
			return false;
		
		try
		{
			myProgressBarStartTime = System.DateTime.Now;
			
			if ( prFullBuild )
			{
				HAPI_Host.unloadOTL( prAssetId );
					
				try
				{
					prAssetInfo = HAPI_Host.createCurve();
				}
				catch ( HAPI_Error error )
				{
					Debug.LogError( "Asset not loaded: " + error.ToString() );
					// Nothing to build since the load failed.
					
					// Clean up.
					reset();
					
					return false; // false for failed :(
				}
				
				// For convinience we copy some asset info properties locally (since they are constant anyway).
				prAssetId 				= prAssetInfo.id;
				prAssetType				= (HAPI_AssetType) prAssetInfo.type;
				prMinInputCount			= prAssetInfo.minInputCount;
				prMaxInputCount			= prAssetInfo.maxInputCount;
				prMinGeoInputCount 		= prAssetInfo.minGeoInputCount;
				prMaxGeoInputCount		= prAssetInfo.maxGeoInputCount;
				prParmCount 			= prAssetInfo.parmCount;
				prParmIntValueCount		= prAssetInfo.parmIntValueCount;
				prParmFloatValueCount	= prAssetInfo.parmFloatValueCount;
				prParmStringValueCount	= prAssetInfo.parmStringValueCount;
				prParmChoiceCount		= prAssetInfo.parmChoiceCount;
				
				prObjectCount 			= prAssetInfo.objectCount;
				prHandleCount 			= prAssetInfo.handleCount;
				prMaterialCount			= prAssetInfo.materialCount;
				
				myProgressBarCurrent	= 0;
				myProgressBarTotal		= prParmCount
										  + prParmIntValueCount
										  + prParmFloatValueCount
										  + prParmStringValueCount
										  + prParmChoiceCount
										  + prObjectCount
										  + prHandleCount
										  + prMaterialCount;
				
				// Try to load presets.
				try
				{
					if ( myPreset != null && myPreset.Length > 0 )
						HAPI_Host.setPreset( prAssetId, myPreset, myPreset.Length );
				}
				catch ( HAPI_Error error )
				{
					Debug.LogWarning( error.ToString() );	
				}
				catch
				{
					Debug.LogWarning( "Unable to load presets." );	
				}
				
				displayProgressBar();
				
				myProgressBarMsg = "Loading parameter information...";
				
				// Get all parameters.
				prParms = new HAPI_ParmInfo[ prParmCount ];
				Utility.getArray1Id( prAssetId, HAPI_Host.getParameters, prParms, prParmCount );
				displayProgressBar( prParmCount );
				
				// Get parameter int values.
				prParmIntValues = new int[ prParmIntValueCount ];
				Utility.getArray1Id( prAssetId, HAPI_Host.getParmIntValues, prParmIntValues, prParmIntValueCount );
				displayProgressBar( prParmIntValueCount );
				
				// Get parameter float values.
				prParmFloatValues = new float[ prParmFloatValueCount ];
				Utility.getArray1Id( prAssetId, HAPI_Host.getParmFloatValues, prParmFloatValues, prParmFloatValueCount );
				displayProgressBar( prParmFloatValueCount );
				
				// Get parameter string (handle) values.
				prParmStringValues = new int[ prParmStringValueCount ];
				Utility.getArray1Id( prAssetId, HAPI_Host.getParmStringValues, prParmStringValues, 
									 prParmStringValueCount );
				displayProgressBar( prParmStringValueCount );
				
				// Get parameter choice lists.
				prParmChoiceLists = new HAPI_ParmChoiceInfo[ prParmChoiceCount ];
				Utility.getArray1Id( prAssetId, HAPI_Host.getParmChoiceLists, prParmChoiceLists, prParmChoiceCount );
				displayProgressBar( prParmChoiceCount );
				
				myProgressBarMsg = "Loading handles...";
				
				// Add input fields.
				if( prAssetInfo.type == (int) HAPI_AssetType.HAPI_ASSETTYPE_OBJ )
				{
					if ( prMaxInputCount > 0 && prUpStreamTransformAssets.Count <= 0 )
						for ( int ii = 0; ii < prMaxInputCount ; ++ii )
						{
							prUpStreamTransformAssets.Add( null );
							prUpStreamTransformObjects.Add( null );
						}
				}
				
				if ( prMaxGeoInputCount > 0 && prFileInputs.Count <= 0 )
					for ( int ii = 0; ii < prMaxGeoInputCount ; ++ii )
					{
						prFileInputs.Add( "" );
						prUpStreamGeoAssets.Add( null );
						prUpStreamGeoObjects.Add( null );
						prUpStreamGeoAdded.Add( false );
					}
				
				// Check for min input fields set.
				if ( prAssetInfo.type == (int) HAPI_AssetType.HAPI_ASSETTYPE_OBJ )
				{
					int numValidTransformInputs = 0;
					for ( int ii = 0; ii < prMaxInputCount ; ++ii )
						if ( prUpStreamTransformAssets[ ii ] )
							numValidTransformInputs++;
					
					if ( numValidTransformInputs < prMinInputCount )
						Debug.LogWarning( "Insufficent Transform Inputs to Asset. " +
										  "Please provide inputs in the Inputs section." );
				}
				
				int numValidGeoInputs = 0;
				for ( int ii = 0; ii < prMaxGeoInputCount ; ++ii )
					if ( prFileInputs[ ii ] != "" )
						numValidGeoInputs++;
				
				if ( numValidGeoInputs < prMinGeoInputCount )
					Debug.LogWarning( "Insufficent Geo Inputs to Asset. Please provide inputs in the Inputs section." );
				
				if ( prAssetInfo.type == (int) HAPI_AssetType.HAPI_ASSETTYPE_OBJ )
					for ( int ii = 0; ii < prMaxInputCount ; ++ii )
						if ( prUpStreamTransformAssets[ ii ] )
							HAPI_Host.connectAssetTransform( prUpStreamTransformAssets[ ii ].prAssetId, prAssetId, ii );
				
				for ( int ii = 0; ii < prMaxGeoInputCount ; ++ii )
				{
					if ( prFileInputs[ ii ] != "" )
						HAPI_Host.setFileInput( prAssetId, ii, prFileInputs[ ii ] );
					
					//TODO: handle restoring geometry connections
					//if ( prUpStreamGeoAssets[ ii ] != null )
					//{
					//	HAPI_Host.connectAsset( prUpStreamAssets[ ii ].prAssetId, prAssetId, ii );
					//}
				}
				
				foreach ( HAPI_Asset downstream_asset in prDownStreamTransformAssets )
				{
					int index = downstream_asset.getAssetTransformConnectionIndex( this );
					if ( index >= 0 )
						HAPI_Host.connectAssetTransform( prAssetId, downstream_asset.prAssetId, index );
				}
			}
			else
			{
				displayProgressBar();
				
				myProgressBarTotal = prObjectCount;
			}
			
			myProgressBarMsg = "Loading and composing objects...";
			
			// Clean up.
			destroyChildren();
			
			// Create local object info caches (transforms need to be stored in a parallel array).
			prObjects 			= new HAPI_ObjectInfo[ prObjectCount ];
			prGameObjects		= new GameObject[ prObjectCount ];
			prObjectTransforms 	= new HAPI_Transform[ prObjectCount ];
			
			Utility.getArray1Id( prAssetId, HAPI_Host.getObjects, prObjects, prObjectCount );
			Utility.getArray2Id( prAssetId, (int) HAPI_RSTOrder.SRT, HAPI_Host.getObjectTransforms, 
						 		 prObjectTransforms, prObjectCount );
			
			for ( int object_index = 0; object_index < prObjectCount; ++object_index )
			{
				incrementProgressBar();
				try
				{
					prGameObjects[ object_index ] = null;
					if ( !prObjects[ object_index ].isInstancer && prObjects[ object_index ].isVisible )
						createObject( object_index );
				}
				catch ( HAPI_Error )
				{
					// Per-object errors are not re-thrown so that the rest of the asset has a chance to load.
					//Debug.LogWarning( error.ToString() );
				}
			}
			
			// Process dependent assets.
			foreach ( HAPI_Asset downstream_asset in prDownStreamTransformAssets )
				downstream_asset.build();
			
			foreach ( HAPI_Asset downstream_asset in prDownStreamGeoAssets )
				downstream_asset.build();
			
			prFullBuild = false;
		}
		catch ( HAPI_Error error )
		{
			Debug.LogError( error.ToString() );
		}
		catch ( System.Exception error )
		{
			Debug.LogError( error.ToString() );	
		}
		finally
		{
			clearProgressBar();
		}
		
		return true;
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private Methods
	
	/// <summary>
	/// 	Instantiate a game object corresponding to a Houdini object of this asset, get geometry information
	/// 	on the object and re-create the geometry on the Unity side.
	/// </summary>
	/// <param name="object_id">
	/// 	Object_id as returned by <see cref="GetObjects"/>.
	/// </param>
	private void createObject( int object_id )
	{
		HAPI_ObjectInfo object_info = prObjects[ object_id ];
		
		// Create main underling.
		GameObject main_child = new GameObject( object_info.name );
		
		try
		{
			main_child.transform.parent = transform;
			
			// Add required components.
			main_child.AddComponent( "HAPI_ChildSelectionControl" );
			
			// Set Object Control on child selection control so it can read settings from here.
			main_child.GetComponent< HAPI_ChildSelectionControl >().setObjectControl( this );
			main_child.GetComponent< HAPI_ChildSelectionControl >().prObjectId = object_id;
			
			// Get Detail info.
			HAPI_GeoInfo geo_info = new HAPI_GeoInfo();
			HAPI_Host.getGeoInfo( prAssetId, object_id, 0, out geo_info );
			if ( prEnableLogging )
				Debug.Log( "Obj #" + object_id + " (" + object_info.name + "): "
						   + "verts: " + geo_info.vertexCount + " faces: " + geo_info.faceCount );
			
			// Make sure our primitive and vertex numbers are supported by Unity.
			// TODO: add this limit in a more proper place
			if ( geo_info.faceCount > 65000 * 3 )
				throw new HAPI_Error( "Face count (" + geo_info.faceCount 
									  + ") above limit (" + ( 65000 * 3 ) + ")!" );
			if ( geo_info.vertexCount > 65000 )
				throw new HAPI_Error( "Vertex count (" + geo_info.vertexCount + ") above limit (" + 65000 + ")!" );
			
			// Print attribute names.
			//printAllAttributeNames( asset_id, object_id, geo_id, geo_info );
			
			// Get position attributes.
			HAPI_AttributeInfo pos_attr_info = new HAPI_AttributeInfo( "P" );
			float[] pos_attr = new float[ 0 ];
			Utility.getAttribute( prAssetId, object_id, 0, "P", ref pos_attr_info, ref pos_attr, 
								  HAPI_Host.getAttributeFloatData );
			if ( !pos_attr_info.exists )
				throw new HAPI_Error( "No position attribute found." );
			
			prVertices = new Vector3[ pos_attr_info.count ];
			for ( int i = 0; i < pos_attr_info.count; ++i )
				for ( int j = 0; j < 3; ++j )
					prVertices[ i ][ j ] = pos_attr[ i * 3 + j ];
			
			// Get transforms.
			HAPI_Transform trans = prObjectTransforms[ object_id ];
			
			// Apply object transforms.		
			//
			// Axis and Rotation conversions:
			// Note that Houdini's X axis points in the opposite direction that Unity's does.  Also, Houdini's 
			// rotation is right handed, whereas Unity is left handed.  To account for this, we need to invert
			// the x coordinate of the translation, and do the same for the rotations (except for the x rotation,
			// which doesn't need to be flipped because the change in handedness AND direction of the left x axis
			// causes a double negative - yeah, I know).
			
			main_child.transform.localPosition 	= new Vector3( -trans.position[ 0 ], 
																trans.position[ 1 ],
																trans.position[ 2 ] );
			
			Quaternion quat = new Quaternion(	trans.rotationQuaternion[ 0 ],
												trans.rotationQuaternion[ 1 ],
												trans.rotationQuaternion[ 2 ],
												trans.rotationQuaternion[ 3 ] );
			
			Vector3 euler = quat.eulerAngles;
			euler.y = -euler.y;
			euler.z = -euler.z;
			
			main_child.transform.localRotation 	= Quaternion.Euler( euler );
			main_child.transform.localScale = new Vector3( trans.scale[ 0 ], trans.scale[ 1 ], trans.scale[ 2 ] );
			
			AssetDatabase.Refresh();
		}
		catch ( HAPI_Error error )
		{
			DestroyImmediate( main_child );
			error.addMessagePrefix( "Obj(id: " + object_info.id + ", name: " + object_info.name + ")" );
			error.addMessageDetail( "Object Path: " + object_info.objectInstancePath );
			throw;
		}
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Serialized Data
	
	[SerializeField]
	private string			myAssetPath;
	
}
