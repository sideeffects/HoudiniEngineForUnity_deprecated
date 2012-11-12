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
		myAsset = target as HAPI_Asset;
		
		if ( GUI.changed )
			myAsset.build();
	}
	
	public override void OnInspectorGUI() 
	{
		myLabelStyle = new GUIStyle( GUI.skin.label );
		myLabelStyle.alignment = TextAnchor.MiddleRight;
		
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
					GameObject old_object = myAsset.prUpStreamGeoObjects[ ii ];
					GameObject new_object = EditorGUILayout.ObjectField( "Geometry Input", old_object, 
																		 typeof( GameObject ), true ) as GameObject;
					
					if ( new_object != old_object )
					{
						myAsset.prUpStreamGeoObjects[ ii ] = new_object;
						if ( !new_object )
							myAsset.removeGeoInput( ii );
						else
						{
							HAPI_Asset asset_component = null;
							HAPI_ChildSelectionControl 
								child_selection_control = new_object.GetComponent< HAPI_ChildSelectionControl >(); 
							
							int object_index = 0;
							if ( child_selection_control )
							{
								object_index = child_selection_control.prObjectId;
								asset_component = child_selection_control.prAsset;
							}
							else
								asset_component = new_object.GetComponent< HAPI_Asset >();
							
							if ( asset_component )
								myAsset.addAssetAsGeoInput( asset_component, object_index, ii );
							else
								myAsset.addGeoAsGeoInput( new_object, ii );
						}
					}
					
					EditorGUILayout.LabelField( new GUIContent( "File Input " + ii + ":" ) );
					EditorGUILayout.BeginHorizontal(); 
					{
						string old_file_path = myAsset.prFileInputs[ ii ];
						string new_file_path = "";
						new_file_path = EditorGUILayout.TextField( old_file_path );
						
						if ( GUILayout.Button( "...", GUILayout.Width( myFileChooserButtonWidth ) ) ) 
						{
							string prompt_result_path = HAPI_GUIUtility.promptForFileInputPath( old_file_path );
							if ( prompt_result_path.Length > 0 )
								new_file_path = prompt_result_path;
						}
						
						myAsset.prFileInputs[ ii ] = new_file_path;
					} 
					EditorGUILayout.EndHorizontal();
					
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
	
	protected const float		myMaxFieldCountPerLine		= 4;
	
	protected const float 		myFileChooserButtonWidth 	= 30;
	protected const float 		myLineHeight 				= 16;
	protected const float 		myLabelWidth 				= 120;
	protected const float 		myToggleWidth 				= 12;
	protected const float		myDummyLabelMinWidth		= 20;
	
	protected const float 		myDefaultUIMin 				= 0.0f;
	protected const float 		myDefaultUIMax 				= 10.0f;
	
	protected GUILayoutOption 	myLineHeightGUI 			= GUILayout.Height( myLineHeight );
	protected GUILayoutOption 	myLabelWidthGUI 			= GUILayout.Width( myLabelWidth );
	protected GUILayoutOption 	myToggleWidthGUI 			= GUILayout.Width( myToggleWidth );
	protected GUILayoutOption	myDummyLabelMinWidthGUI		= GUILayout.MinWidth( myDummyLabelMinWidth );
	
	protected GUIContent 		myNullContent 				= new GUIContent( "" );
	
	protected GUIStyle			myLabelStyle;
	
	protected HAPI_Asset 		myAsset;
}
