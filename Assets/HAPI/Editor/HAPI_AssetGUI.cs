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
		
		myUnbuiltChanges 	= false;
		
		if ( GUI.changed )
			myAsset.build();
	}
	
	public override void OnInspectorGUI() 
	{
		bool changed = false;
		
		///////////////////////////////////////////////////////////////////////
		// Draw Game Object Controls
		
		if ( myAsset.prMaxInputCount > 0 || myAsset.prMaxGeoInputCount > 0 )
		{
			myAsset.prShowInputControls = 
				EditorGUILayout.Foldout( myAsset.prShowInputControls, new GUIContent( "Inputs" ) );
			
			if ( myAsset.prShowInputControls )
			{
				if ( myAsset.prAssetType == HAPI_AssetType.HAPI_ASSETTYPE_OBJ )
					for ( int ii = 0; ii < myAsset.prMaxInputCount; ++ii )
						setTransformInput( ii );
				
				for ( int ii = 0; ii < myAsset.prMaxGeoInputCount; ++ii )
				{
					HAPI_GUIParm obj_input = new HAPI_GUIParm( "obj_input_" + ii, "Object Input " + ii );
					Object obj = (Object) myAsset.prUpStreamGeoObjects[ ii ];
					changed |= HAPI_GUI.objectField( ref obj_input, ref obj );
					
					if ( changed )
					{
						if ( !obj )
						{
							myAsset.prUpStreamGeoObjects[ ii ] = null;
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
								myAsset.addAssetAsGeoInput( asset_component, object_index, ii );
							else
								myAsset.addGeoAsGeoInput( new_obj, ii );
						}
					}
					
					HAPI_GUIParm file_input = new HAPI_GUIParm( "file_input_" + ii, "File Input " + ii );
					string file_path = myAsset.prFileInputs[ ii ];
					changed |= HAPI_GUI.fileField( ref file_input, ref myDelayBuild, ref file_path );
					
				} // for
			} // if
		} // if
	}
	
	protected void setTransformInput( int index )
	{
		myAsset.prUpStreamTransformObjects[ index ] = 
			EditorGUILayout.ObjectField( "Transform Input", myAsset.prUpStreamTransformObjects[ index ], 
										 typeof( GameObject ), true ) as GameObject;
		
		if ( myAsset.prUpStreamTransformObjects[ index ] )
		{
			HAPI_Asset inputAsset = myAsset.prUpStreamTransformObjects[ index ].GetComponent< HAPI_Asset >();
			if ( inputAsset )
				myAsset.addAssetAsTransformInput( inputAsset, index );
			else
				myAsset.removeTransformInput( index );
		}
		else
			myAsset.removeTransformInput( index );
	}
	
	protected HAPI_Asset 	myAsset;
	protected bool			myDelayBuild;
	protected bool			myUnbuiltChanges;
}
