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
		try
		{
			myDelayBuild	= false;
			myParmChanges	= false;
		
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
					
					if ( myAsset.prUpStreamGeoObjects == null || myAsset.prUpStreamGeoAssets == null ||
						 myAsset.prUpStreamGeoObjects.Count <= 0 || myAsset.prUpStreamGeoAssets.Count <= 0 )
						return;

					for ( int ii = 0; ii < myAsset.prMaxGeoInputCount; ++ii )
					{
						bool join_last							= false;
						bool no_label_toggle_last				= true;
						HAPI_GUIParm input_format_dropdown		= new HAPI_GUIParm( "input_format_dropdown_" + ii );
						input_format_dropdown.width				= myInputFormatDropdownWidth;
						input_format_dropdown.size				= 1;
						input_format_dropdown.choiceCount		= 2;
						input_format_dropdown.joinNext			= true;
						input_format_dropdown.labelNone			= true;
						int[] input_format_value				= new int[ 1 ] { 0 };
						int[] input_format_dropdown_values		= new int[ 2 ] { 0, 1 };
						string[] input_format_dropdown_labels	= new string[ 2 ] { "Object", "File" };
						input_format_value[ 0 ]					= (int) myAsset.prGeoInputFormats[ ii ];

						HAPI_GUI.dropdown( ref input_format_dropdown, ref input_format_value, 
										   input_format_dropdown_labels, input_format_dropdown_values, 
										   ref join_last, ref no_label_toggle_last );

						HAPI_GeoInputFormat value				= (HAPI_GeoInputFormat) input_format_value[ 0 ];
						myAsset.prGeoInputFormats[ ii ]			= value;

						if ( value == HAPI_GeoInputFormat.HAPI_GEO_INPUT_FORMAT_OBJECT )
						{
							HAPI_GUIParm geo_input = new HAPI_GUIParm( "geo_input_" + ii, 
																	   myAsset.prGeoInputNames[ ii ] );
							Object obj = (Object) myAsset.prUpStreamGeoObjects[ ii ];
							myParmChanges |= HAPI_GUI.objectField( ref geo_input, ref obj, 
																   typeof( GameObject ), ref join_last,
																   ref no_label_toggle_last );
					
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
						}
						else
						{
							HAPI_GUIParm file_input = new HAPI_GUIParm( "file_input_" + ii, "File Input " + ii );
							string file_path = myAsset.prFileInputs[ ii ];
							myParmChanges |= HAPI_GUI.fileField( ref file_input, ref myDelayBuild, ref file_path,
																 ref join_last, ref no_label_toggle_last );
						} // if
					} // for
				} // if
			} // if
		}
		catch ( HAPI_Error e )
		{
			Debug.LogError( e.ToString() );
		}
	}
	
	protected bool setTransformInput( int index )
	{
		if ( myAsset.prUpStreamTransformObjects == null || myAsset.prUpStreamTransformObjects.Count <= 0 )
			return false;

		bool join_last				= false;
		bool no_label_toggle_last	= true;

		HAPI_GUI.label( "Transform", myInputFormatDropdownWidth, true, ref join_last );

		HAPI_GUIParm trans_input	= new HAPI_GUIParm( "trans_input_" + index, myAsset.prTransInputNames[ index ] );
		Object obj					= (Object) myAsset.prUpStreamTransformObjects[ index ];
		bool changed				= HAPI_GUI.objectField( ref trans_input, ref obj, typeof( GameObject ),
															ref join_last, ref no_label_toggle_last );
		
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

	private const int		myInputFormatDropdownWidth = 62;
}
