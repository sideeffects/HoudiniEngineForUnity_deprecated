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
using System.Collections;
using System.Collections.Generic;
using HAPI;

[ CustomEditor( typeof( HAPI_Asset ) ) ]
public class HAPI_AssetGUI : Editor 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public
	
	public virtual void OnEnable() 
	{
		myAsset 			= target as HAPI_Asset;
		
		myParmChanges		= true;
		myUnbuiltChanges 	= false;
	}
	
	public override void OnInspectorGUI() 
	{
		myDelayBuild = false;
		myParmChanges = false;
		
		///////////////////////////////////////////////////////////////////////
		// Draw Game Object Controls
		
		if ( myAsset.prMaxInputCount > 0 || myAsset.prMaxGeoInputCount > 0 )
		{
			myAsset.prShowInputControls = 
				EditorGUILayout.Foldout( myAsset.prShowInputControls, new GUIContent( "Inputs" ) );
			
			if ( myAsset.prShowInputControls )
			{
				if ( myAsset.prHAPIAssetType == HAPI_AssetType.HAPI_ASSETTYPE_OBJ )
					for ( int ii = 0; ii < myAsset.prMaxInputCount; ++ii )
						myParmChanges |= setTransformInput( ii );
				
				for ( int ii = 0; ii < myAsset.prMaxGeoInputCount; ++ii )
				{
					HAPI_GUIParm geo_input = new HAPI_GUIParm( "geo_input_" + ii, "Geometry Input " + ii );
					Object obj = (Object) myAsset.prUpStreamGeoObjects[ ii ];
					myParmChanges |= HAPI_GUI.objectField( ref geo_input, ref obj, typeof( GameObject ) );
					
					if ( myParmChanges )
					{
						if ( !obj )
						{
							myAsset.prUpStreamGeoObjects[ ii ] = null;
							myAsset.prUpStreamGeoAssets[ ii ] = null;
							myAsset.removeGeoInput( ii );
						}
						else
						{
							GameObject new_obj = (GameObject) obj;
							
							myAsset.prUpStreamGeoObjects[ ii ] = new_obj;
							
							HAPI_Asset asset_component = null;
							HAPI_ChildSelectionControl 
								child_selection_control = new_obj.GetComponent< HAPI_ChildSelectionControl >(); 
							
							int object_index = 0;
							if ( child_selection_control )
							{
								object_index = child_selection_control.prObjectId;
								asset_component = child_selection_control.prAsset;
							}
							else
								asset_component = new_obj.GetComponent< HAPI_Asset >();
							
							if ( asset_component )
								if ( myAsset == asset_component )
									Debug.LogError( "Can't connect an asset to itself!" );
								else
									myAsset.addAssetAsGeoInput( asset_component, object_index, ii );
							else
								myAsset.addGeoAsGeoInput( new_obj, ii );
						}
					}
					
					HAPI_GUIParm file_input = new HAPI_GUIParm( "file_input_" + ii, "File Input " + ii );
					string file_path = myAsset.prFileInputs[ ii ];
					myParmChanges |= HAPI_GUI.fileField( ref file_input, ref myDelayBuild, ref file_path );
					
				} // for
			} // if
		} // if
	}
	
	protected bool setTransformInput( int index )
	{
		bool changed = false;
		
		HAPI_GUIParm trans_input = new HAPI_GUIParm( "trans_input_" + index, "Transform Input " + index );
		Object obj = (Object) myAsset.prUpStreamTransformObjects[ index ];
		changed |= HAPI_GUI.objectField( ref trans_input, ref obj, typeof( GameObject ) );
		
		if ( changed )
		{
			if ( !obj )
			{
				myAsset.prUpStreamTransformObjects[ index ] = null;
				myAsset.removeTransformInput( index );
			}
			else
			{
				GameObject game_obj = (GameObject) obj;
				myAsset.prUpStreamTransformObjects[ index ] = game_obj;
				HAPI_Asset input_asset = game_obj.GetComponent< HAPI_Asset >();
				if ( input_asset )
					myAsset.addAssetAsTransformInput( input_asset, index );
				else
					myAsset.removeTransformInput( index );
			}
		}
		
		return changed;
	}
	
	protected HAPI_Asset 	myAsset;
	protected bool			myDelayBuild;
	protected bool			myParmChanges;
	protected bool			myUnbuiltChanges;
}
