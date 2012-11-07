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
 * 		Contains the main script attached to a Unity game object that corresponds to a Houdini asset instance on the 
 *		Houdini side. 
 * 
 */


using UnityEngine;
using UnityEditor;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using HAPI;
using Utility = HAPI_AssetUtility;

/// <summary>
/// 	Main script attached to an Unity game object that corresponds to a Houdini asset instance on the 
/// 	Houdini side.
/// </summary>
[ ExecuteInEditMode ]
public partial class HAPI_Asset : MonoBehaviour 
{	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	
	public bool 					prAssetPathChanged { get; set; }
	public string 					prAssetPath { get { return myAssetPath; } set { myAssetPath = value; } }
	public byte[]					prPreset { get { return myPreset; } set { myPreset = value; } }
	public int 						prAssetId { get; set; }
	public HAPI_AssetType			prAssetType { get; set; }
	public int 						prMinInputCount { get; set; }
	public int 						prMaxInputCount { get; set; }
	public int 						prMinGeoInputCount { get; set; }
	public int						prMaxGeoInputCount { get; set; }
	public List< string >			prFileInputs { get; set; }
	
	public List< HAPI_Asset >		prDownStreamTransformAssets { get; set; }
	public List< HAPI_Asset >		prUpStreamTransformAssets { get; set; }
	public List< GameObject >		prUpStreamTransformObjects { get; set; }
	
	public List< HAPI_Asset >		prDownStreamGeoAssets { get; set; }
	public List< HAPI_Asset >		prUpStreamGeoAssets { get; set; }
	public List< GameObject >		prUpStreamGeoObjects { get; set; }
	public List< bool >				prUpStreamGeoAdded { get; set; }
	
	public int 						prParmCount { get; set; }
	public int						prParmIntValueCount { get; set; }
	public int						prParmFloatValueCount { get; set; }
	public int						prParmStringValueCount { get; set; }
	public int						prParmChoiceCount { get; set; }
	
	public int 						prObjectCount { get; set; }
	public int						prHandleCount { get; set; }
	public int						prMaterialCount { get; set; }
	
	public HAPI_ParmInfo[] 			prParms { get; set; }
	public int[]					prParmIntValues { get; set; }
	public float[]					prParmFloatValues { get; set; }
	public int[]					prParmStringValues { get; set; } // string handles (SH)
	public HAPI_ParmChoiceInfo[]	prParmChoiceLists { get; set; }
	
	public HAPI_AssetInfo 			prAssetInfo { get; set; }
	public HAPI_ObjectInfo[] 		prObjects { get; set; }
	public HAPI_MaterialInfo[]		prMaterials { get; set; }
	
	public GameObject[]				prGameObjects {	get; set; }
	
	public HAPI_Transform[] 		prObjectTransforms { get; set; }
	public HAPI_HandleInfo[]		prHandleInfos { get; set; }	
	public List< HAPI_HandleBindingInfo[] > prHandleBindingInfos { get; set; }
	
	public bool 					prShowObjectControls { get; set; }
	public bool 					prShowAssetControls { get; set; }
	public bool						prShowInputControls { get; set; }
	public bool						prAutoSelectAssetNode { get; set; }
	public bool						prEnableLogging { get; set; }
	
	public int						prLastChangedParmId { get; set; }
	
	/// <summary>
	/// 	Indices of the currently selected folders in the Inspector.
	/// 	A 1:1 mapping with myFolderListSelectionIds.
	/// </summary>
	public List< int > 				prFolderListSelections { get; set; }
	
	/// <summary>
	/// 	Parameter ids of the currently selected folders in the Inspector. 
	/// 	A 1:1 mapping with myFolderListSelections.
	/// </summary>
	public List< int > 				prFolderListSelectionIds { get; set; }
	
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods
	
	public HAPI_Asset() 
	{
		if ( prEnableLogging )
			Debug.Log( "HAPI_Asset created!" );
		
		HAPI.HAPI_SetPath.setPath();
		
		prAssetPath 				= "";
		prAssetPathChanged 			= true;
		prAssetId 					= -1;
		prAssetType					= HAPI_AssetType.HAPI_ASSETTYPE_INVALID;
		
		prParms						= null;
		prParmCount 				= 0;
		prParmIntValueCount			= 0;
		prParmFloatValueCount		= 0;
		prParmStringValueCount		= 0;
		prParmChoiceCount			= 0;
		
		prObjectCount 				= 0;
		prHandleCount 				= 0;
		prMaterialCount				= 0;
		
		prMinInputCount				= 0;
		prMaxInputCount				= 0;
		prMinGeoInputCount			= 0;
		prMaxGeoInputCount			= 0;
		prFileInputs				= new List<string>();
		prShowInputControls			= true;
		
		prShowAssetControls 		= true;
		prShowObjectControls 		= true;
		prAutoSelectAssetNode		= true;
		prEnableLogging				= false;
		
		prLastChangedParmId			= -1;
				
		prFolderListSelections 		= new List< int >();
		prFolderListSelectionIds 	= new List< int >();
		

		prDownStreamTransformAssets = new List< HAPI_Asset >();
		prUpStreamTransformAssets 	= new List< HAPI_Asset >();
		prUpStreamTransformObjects 	= new List< GameObject >();
		
		prDownStreamGeoAssets 		= new List< HAPI_Asset >();
		prUpStreamGeoAssets 		= new List< HAPI_Asset >();
		prUpStreamGeoObjects 		= new List< GameObject >();
		prUpStreamGeoAdded 			= new List< bool >();
		
		prFolderListSelections.Add( 0 );
		prFolderListSelectionIds.Add( -1 );
		
		myProgressBarJustUsed 		= false;
		myProgressBarCurrent		= 0;
		myProgressBarTitle			= "Building Houdini Asset";
		myProgressBarMsg			= "";
		
		myPreset 					= null;
	}
	
	~HAPI_Asset() 
	{
		if ( prEnableLogging )
			Debug.Log( "HAPI_Asset destroyed!" );
	}
	
	
	// Transform related connection methods -------------------------------------------------------
	
	public void addAssetAsTransformInput( HAPI_Asset asset, int index )
	{		
		
		if ( prUpStreamTransformAssets[ index ] == asset )
			return;
		
		prUpStreamTransformAssets[ index ] = asset;
		HAPI_Host.connectAssetTransform( asset.prAssetId, prAssetId, index );
		asset.addDownstreamTransformAsset( this );
		build();
		return;
	}
	
	public void removeTransformInput( int index )
	{
		if ( prUpStreamTransformAssets[ index ] != null )
		{
			prUpStreamTransformAssets[ index ].removeDownstreamTransformAsset( this );
			HAPI_Host.disconnectAssetTransform( prAssetId, index );
			prUpStreamTransformAssets[ index ] = null;
			build();
		}
		
	}
	
	public void removeAssetAsTransformInput( HAPI_Asset asset )
	{
		for ( int ii = 0; ii < prUpStreamTransformAssets.Count; ++ii )
		{
			if ( prUpStreamTransformAssets[ii] == asset )
			{
				prUpStreamTransformAssets[ ii ] = null;
				HAPI_Host.disconnectAssetTransform( prAssetId, ii );
				
				asset.removeDownstreamTransformAsset( this );
				build();
				return;
			}
		}
	}
	
	public int getAssetTransformConnectionIndex( HAPI_Asset asset )
	{
		for ( int ii = 0; ii < prUpStreamTransformAssets.Count; ii++ )
			if ( prUpStreamTransformAssets[ii] == asset )
				return ii;
		
		return -1;
	}
	
	public void addDownstreamTransformAsset( HAPI_Asset asset )
	{		
		foreach ( HAPI_Asset downstream_asset in prDownStreamTransformAssets )
			if ( downstream_asset == asset )
				return;
		
		prDownStreamTransformAssets.Add( asset );
	}
	
	public void removeDownstreamTransformAsset( HAPI_Asset asset )
	{			
		prDownStreamTransformAssets.Remove( asset );		
		
	}
	
	// Geometry related connection methods -------------------------------------------------------
	
	public void addAssetAsGeoInput( HAPI_Asset asset, int index )
	{		
		if( prUpStreamGeoAssets[ index ] == asset )
			return;
		
		prUpStreamGeoAssets[ index ] = asset;
		HAPI_Host.connectAssetGeometry( asset.prAssetId, 0, 0, prAssetId, index );
		asset.addDownstreamGeoAsset( this );
		build();
	}
	
	public void addGeoAsGeoInput( GameObject asset, int index )
	{
		prUpStreamGeoAdded[ index ] = true;
		
		int object_id;
		int geo_id;
		HAPI_Host.createGeoInput( prAssetId, index, out object_id, out geo_id );
		
		MeshFilter asset_mesh_filter 	= asset.GetComponent< MeshFilter >();
		Mesh mesh 						= asset_mesh_filter.sharedMesh;
		
		Utility.setMesh( prAssetId, object_id, geo_id, ref mesh );
		build();
	}
	
	public void removeGeoInput( int index )
	{
		if ( prUpStreamGeoAssets[ index ] )
		{
			prUpStreamGeoAssets[ index ].removeDownstreamGeoAsset( this );
			HAPI_Host.disconnectAssetGeometry( prAssetId, index );
			prUpStreamGeoAssets[ index ] = null;
			build();
		}
		
		if ( prUpStreamGeoAdded[ index ] )
		{
			HAPI_Host.disconnectAssetGeometry( prAssetId, index );
			prUpStreamGeoAdded[ index ] = false;
			build();
		}
	}
	
	public void removeDownstreamGeoAsset( HAPI_Asset asset )
	{
		prDownStreamGeoAssets.Remove( asset );
	}
	
	public void addDownstreamGeoAsset( HAPI_Asset asset )
	{		
		foreach ( HAPI_Asset downstream_asset in prDownStreamGeoAssets )
			if ( downstream_asset == asset )
				return;
		
		prDownStreamGeoAssets.Add( asset );
	}
	
	public void OnDestroy()
	{
		if ( prAssetId >= 0 )
		{
			foreach ( HAPI_Asset upstream_asset in prUpStreamTransformAssets )
				if ( upstream_asset != null )
					upstream_asset.removeDownstreamTransformAsset( this );
			
			List< HAPI_Asset > downstream_asset_list = new List< HAPI_Asset >();
			
			foreach ( HAPI_Asset downstream_asset in prDownStreamTransformAssets )
				downstream_asset_list.Add( downstream_asset );
			
			foreach ( HAPI_Asset downstream_asset in downstream_asset_list )
				downstream_asset.removeAssetAsTransformInput( this );
			
			prUpStreamTransformAssets.Clear();
			prDownStreamTransformAssets.Clear();
			
			HAPI_Host.unloadOTL( prAssetId );
			prAssetId = -1;
		}
	}
	
	/// <summary>
	/// 	Sets the asset path.
	/// </summary>
	/// <param name="path">
	/// 	New asset path (absolute path to the .otl file).
	/// </param>
	/// <returns>
	/// 	Whether the path was changed or not. If the new path is the same as the old path, no change will
	/// 	be made and <c>false</c> will be returned.
	/// </returns>
	public bool setAssetPath( string path ) 
	{
		if ( path != prAssetPath ) 
		{
			prAssetPath = path;
			prAssetPathChanged = true;
		}
		return prAssetPathChanged;
	}
	
	/// <summary>
	/// 	Gets the asset .otl path.
	/// </summary>
	/// <returns>
	/// 	The asset .otl path.
	/// </returns>
	public string getAssetPath() 
	{
		return prAssetPath;	
	}
	
	/// <summary>
	/// 	If the asset path has changed, unload the current asset and re-load from the new file along with
	/// 	all parameter information. Either way, push parameter values back to Houdini and rebuild
	///		all object geometries.
	/// </summary>
	public bool build() 
	{
		if ( !HAPI.HAPI_SetPath.prIsPathSet )
		{
			Debug.LogError( "Cannot build asset as Houdini dlls not found!" );
			return false;
		}
		
		try
		{
			myProgressBarStartTime = System.DateTime.Now;
			
			if ( prAssetPathChanged ) 
			{
				HAPI_Host.unloadOTL( prAssetId );
				
				try
				{
					prAssetInfo = HAPI_Host.loadOTL( prAssetPath );
				}
				catch ( HAPI_Error error )
				{
					Debug.LogError( error.ToString() );
					// Nothing to build since the load failed.
					return false; // false for failed :(
				}
				
				// For convinience we copy some asset info properties locally (since they are constant anyway).
				prAssetId 				= prAssetInfo.id;
				prAssetType				= (HAPI_AssetType) prAssetInfo.assetType;
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
				Utility.getArray1Id( prAssetId, HAPI_Host.getParmStringValues, prParmStringValues, prParmStringValueCount );
				displayProgressBar( prParmStringValueCount );
				
				// Get parameter choice lists.
				prParmChoiceLists = new HAPI_ParmChoiceInfo[ prParmChoiceCount ];
				Utility.getArray1Id( prAssetId, HAPI_Host.getParmChoiceLists, prParmChoiceLists, prParmChoiceCount );
				displayProgressBar( prParmChoiceCount );								
				
				myProgressBarMsg = "Loading handles...";
				
				// Get exposed handle information.
				prHandleInfos = new HAPI_HandleInfo[ prHandleCount ];
				Utility.getArray1Id( prAssetId, HAPI_Host.getHandleInfo, prHandleInfos, prHandleCount );
				
				// Get handles.
				prHandleBindingInfos = new List< HAPI_HandleBindingInfo[] >( prHandleCount );		
				for ( int handle_index = 0; handle_index < prHandleCount; ++handle_index )
				{
					incrementProgressBar();
					HAPI_HandleInfo handle_info = prHandleInfos[ handle_index ];
					
					if ( handle_info.typeName != "xform" )
						Debug.LogWarning( "Handle " + handle_info.name + " of type " 
								   		  + handle_info.typeName + " is unsupported at this time." );
					
					HAPI_HandleBindingInfo[] binding_infos = new HAPI_HandleBindingInfo[ handle_info.bindingsCount ];				
					Utility.getArray2Id( prAssetId, handle_index, HAPI_Host.getHandleBindingInfo, 
								 		 binding_infos, handle_info.bindingsCount );
					
					prHandleBindingInfos.Add( binding_infos );
				}
				
				// Get materials.
				prMaterials = new HAPI_MaterialInfo[ prMaterialCount ];
				Utility.getArray1Id( prAssetId, HAPI_Host.getMaterials, prMaterials, prMaterialCount );
				displayProgressBar( prMaterialCount );
				
				// Add input fields.
				if( prAssetInfo.assetType == (int) HAPI_AssetType.HAPI_ASSETTYPE_OBJ )
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
				if( prAssetInfo.assetType == (int) HAPI_AssetType.HAPI_ASSETTYPE_OBJ )
				{
					int numValidTransformInputs = 0;
					for ( int ii = 0; ii < prMaxInputCount ; ++ii )
						if ( prUpStreamTransformAssets[ ii ] != null )
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
				
				if( prAssetInfo.assetType == (int) HAPI_AssetType.HAPI_ASSETTYPE_OBJ )
				{
					for ( int ii = 0; ii < prMaxInputCount ; ++ii )
					{												
						if ( prUpStreamTransformAssets[ ii ] != null )
						{
							HAPI_Host.connectAssetTransform( prUpStreamTransformAssets[ ii ].prAssetId, prAssetId, ii );						
						}
					}
				}
				
				for ( int ii = 0; ii < prMaxGeoInputCount ; ++ii )
				{
					if ( prFileInputs[ ii ] != "" )
					{
						HAPI_Host.setFileInput( prAssetId, ii, prFileInputs[ ii ] );
					}
					
					//TODO: handle restoring geometry connections
					//if ( prUpStreamGeoAssets[ ii ] != null )
					//{
					//	HAPI_Host.connectAsset( prUpStreamAssets[ ii ].prAssetId, prAssetId, ii );						
					//}
				}
				
				foreach ( HAPI_Asset downstream_asset in prDownStreamTransformAssets )
				{
					int index = downstream_asset.getAssetTransformConnectionIndex( this );
					if( index >=0 )
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
					if( !prObjects[ object_index ].isInstancer && prObjects[ object_index ].isVisible )
						createObject( object_index );
				}
				catch ( HAPI_Error error )
				{
					// Per-object errors are not re-thrown so that the rest of the asset has a chance to load.
					Debug.LogWarning( error.ToString() );
				}
			}
			
			// Processing instancers.
			for ( int object_index = 0; object_index < prObjectCount; ++object_index )
			{			
				HAPI_ObjectInfo object_info = prObjects[ object_index ];
				if ( object_info.isInstancer )
				{
					try
					{
						if ( object_info.objectToInstanceId >= 0 && 
							 prGameObjects[ object_info.objectToInstanceId ] == null )
							createObject( object_info.objectToInstanceId );
						
						instanceObjects( object_index );
					}
					catch ( HAPI_Error error )
					{
						// Per-object errors are not re-thrown so that the rest of the asset has a chance to load.
						Debug.LogWarning( error.ToString() );
					}
				}
			}
			
			// process dependent assets.
			foreach ( HAPI_Asset downstream_asset in prDownStreamTransformAssets )
			{
				downstream_asset.build();
			}
			
			foreach ( HAPI_Asset downstream_asset in prDownStreamGeoAssets )
			{
				downstream_asset.build();
			}
			
			prAssetPathChanged = false;
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
		/*
		int length = 0;
		HAPI_Host.getPreset( prAssetId, new byte[0], ref length );
		byte[] buf = new byte[length];
		HAPI_Host.getPreset( prAssetId, buf, ref length );
		*/
		
		return true;
	}
	
	
	public int findObjectByName( string object_name )
	{
		for ( int object_index = 0; object_index < prObjectCount; ++object_index )
		{			
			HAPI_ObjectInfo object_info = prObjects[ object_index ];
			if( object_info.name == object_name )
			{
				return object_index;
			}
		}
		return -1;
	}
			
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private Methods
	
	/// <summary>
	/// 	Recursively delete all children of this Unity asset.
	/// </summary>
	private void destroyChildren() 
	{
		List< GameObject > children = new List< GameObject >();
		
		foreach ( Transform child in transform )
			children.Add( child.gameObject );
		
		foreach ( GameObject child in children )
			DestroyImmediate( child );
	}
	
	private void instanceObjects( int object_id )
	{
		
		HAPI_ObjectInfo object_info = prObjects[ object_id ];
		
		GameObject main_object = new GameObject( object_info.name );
		main_object.transform.parent = transform;
		
		main_object.AddComponent( "HAPI_Instancer" );		
		HAPI_Instancer instancer = main_object.GetComponent< HAPI_Instancer >();
		
		instancer.prObjectControl = this;
		instancer.prObjectId = object_id;
		
		instancer.instanceObjects();				
		
	}
	
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
			main_child.AddComponent( "MeshFilter" );
			main_child.AddComponent( "MeshRenderer" );
			main_child.AddComponent( "HAPI_ChildSelectionControl" );
			
			// Set Object Control on child selection control so it can read settings from here.
			main_child.GetComponent< HAPI_ChildSelectionControl >().setObjectControl( this );
			
			// Get or create mesh.
			MeshFilter main_child_mesh_filter 	= main_child.GetComponent< MeshFilter >();
			Mesh main_child_mesh 				= main_child_mesh_filter.sharedMesh;
			if ( main_child_mesh == null ) 
			{
				main_child_mesh_filter.mesh 	= new Mesh();
				main_child_mesh 				= main_child_mesh_filter.sharedMesh;
			}
			main_child_mesh.Clear();
			
			// Get mesh.
			Utility.getMesh( prAssetId, object_id, 0, main_child_mesh );
			
			// Add Mesh-to-Prefab component.
			prGameObjects[ object_id ] = main_child;			
			main_child.AddComponent( "HAPI_MeshToPrefab" );		
			HAPI_MeshToPrefab mesh_saver = main_child.GetComponent< HAPI_MeshToPrefab >();			
			mesh_saver.prObjectControl = this;
			mesh_saver.prObjectId = object_id;
			mesh_saver.prMeshName = this.prAssetInfo.name + "_" + main_child.name;
			
			// Get transforms.
			HAPI_Transform trans = prObjectTransforms[ object_id ];
			
			// Get Detail info.
			HAPI_GeoInfo geo_info = new HAPI_GeoInfo();
			HAPI_Host.getGeoInfo( prAssetId, object_id, 0, out geo_info );
			if ( prEnableLogging )
			Debug.Log( "Obj #" + object_id + " (" + object_info.name + "): "
					   + "verts: " + geo_info.vertexCount + " faces: " + geo_info.faceCount );
			
			// Set diffuse material.
			Material diffuse = new Material( Shader.Find( "Specular" ) );		
			main_child.GetComponent< MeshRenderer >().material = diffuse;
			if ( prMaterialCount > 0 && geo_info.materialId >= 0 )
			{
				if ( geo_info.hasMaterialChanged )
				{
					HAPI_MaterialInfo[] material = new HAPI_MaterialInfo[ 1 ];
					HAPI_Host.getMaterials( prAssetId, material, geo_info.materialId, 1 );
					prMaterials[ geo_info.materialId ] = material[ 0 ];
					geo_info.hasMaterialChanged = false;
				}
				Utility.assignTexture( ref diffuse, prMaterials[ geo_info.materialId ] );
			}
			
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
	
	// PROGRESS BAR -------------------------------------------------------------------------------------------------
	
	private void incrementProgressBar()
	{
		displayProgressBar( 1 );
	}
	
	private void displayProgressBar()
	{
		displayProgressBar( 0 );	
	}
	
	private void displayProgressBar( int increment )
	{
		System.DateTime current = System.DateTime.Now;
		System.TimeSpan delta = current - myProgressBarStartTime;
		
		// This delay for displaying the progress bar is so the bar won't flicker for really quick updates
		// (less than a few seconds). Also, when we do show the progress bar the focus of the current 
		// inspector control is lost.
		if ( delta.Seconds < HAPI_Constants.HAPI_SEC_BEFORE_PROGRESS_BAR_SHOW )
		{
			EditorUtility.ClearProgressBar();
			return;
		}
		
		myProgressBarJustUsed = true;
				
		myProgressBarCurrent += increment;
		string message = myProgressBarMsg + " Item " + myProgressBarCurrent + " of " + myProgressBarTotal;
		bool result = !EditorUtility.DisplayCancelableProgressBar( myProgressBarTitle, message, 
												Mathf.InverseLerp( 0, myProgressBarTotal, myProgressBarCurrent ) );
		
		if ( !result )
			throw new HAPI_ErrorProgressCancelled();
	}
	
	public bool hasProgressBarBeenUsed()
	{
		return myProgressBarJustUsed;	
	}
	
	private void clearProgressBar()
	{
		myProgressBarJustUsed = false;
		myProgressBarCurrent = 0;
		EditorUtility.ClearProgressBar();
	}
	
	private bool			myProgressBarJustUsed;
	private	System.DateTime	myProgressBarStartTime;
	private int				myProgressBarTotal; // Used for the progress bar.
	private int				myProgressBarCurrent;
	private string			myProgressBarTitle;
	private string			myProgressBarMsg;
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Serialized Data
	
	[SerializeField]
	private string			myAssetPath;
	[SerializeField]
	private byte[] 			myPreset;
	
	
}
