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

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

[ CustomEditor( typeof( HoudiniInstancerManager ) ) ]
public partial class HoudiniInstancerManagerGUI: Editor 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public

	public virtual void OnEnable() 
	{
		myInstanceManager = target as HoudiniInstancerManager;

		myAssetOTL = myInstanceManager.gameObject.GetComponent< HoudiniAssetOTL >();
	}

	public override void OnInspectorGUI()
	{
		HoudiniInstancer[] instancers = myAssetOTL.gameObject.GetComponentsInChildren< HoudiniInstancer >();
		if ( instancers.Length > 0 )
			generateAssetInstanceControls();

		Event current_event = Event.current;

		if (
			current_event.type == EventType.ValidateCommand &&
			current_event.commandName == "UndoRedoPerformed" )
		{
			foreach ( HoudiniInstancer instancer in instancers )
			{
				HoudiniProgressBar progress_bar = new HoudiniProgressBar();
				instancer.instanceObjects( progress_bar );
				progress_bar.clearProgressBar();
			}

			Repaint();
		}
	}

	private void generateAssetInstanceControls()
	{
		HoudiniInstancerManager instancer_manager = myAssetOTL.gameObject.GetComponent< HoudiniInstancerManager >();
		if( instancer_manager == null )
			return;

		List< HoudiniInstancerPersistentData > instancer_persistent_data = instancer_manager.prInstancerPersistentData;

		HoudiniInstancer[] instancers = myAssetOTL.gameObject.GetComponentsInChildren< HoudiniInstancer >();

		foreach( HoudiniInstancer instancer in instancers )
		{
			HoudiniInstancerPersistentData persistent_data = null;

			for( int ii = 0; ii < instancer_persistent_data.Count; ii++ )
			{
				HoudiniInstancerPersistentData data = instancer_persistent_data[ ii ];
				if( data.instancerName == instancer.name )
				{
					persistent_data = data;
					break;
				}
			}

			if( persistent_data == null )
			{
				Debug.LogError("Can't find persistent data for instancer: " + instancer.name );
				continue;
			}

			Undo.RecordObject( persistent_data, "Houdini Instancer Change" );

			persistent_data.showInstancerGUI = HoudiniGUI.foldout(
				persistent_data.instancerName, persistent_data.showInstancerGUI, true );
			if ( persistent_data.showInstancerGUI )
			{
				bool changed = false;
				
				{
					Vector3 dummy = new Vector3();
					changed |= HoudiniGUI.floatField( 
						"RotationOffset", "Rotation Offset", 
						ref persistent_data.rotationalOffset, null, ref dummy );

					changed |= HoudiniGUI.floatField( 
						"ScaleOffset", "Scale Offset", ref persistent_data.scaleOffset, null, ref dummy );
					
					List< string > unique_names = persistent_data.uniqueNames;
					
					for ( int ii = 0; ii < unique_names.Count; ii++ )
					{

						string instanced_name = unique_names[ ii ];
						int base_index = persistent_data.baseIndex( ii );



						for ( int jj = 0; jj < persistent_data.numObjsToInstantiate[ ii ]; jj++ )
						{
							Object obj = (Object) persistent_data.objsToInstantiate[ base_index + jj ];

							GUILayout.BeginHorizontal();
							


							string label = "";
							if ( jj == 0 )
								label = instanced_name;

							GameObject temp_obj = null;
							changed |= HoudiniGUI.objectField(
								"object_to_instantiate", label, ref obj, typeof( GameObject ), null, ref temp_obj );

							if ( changed )
							{
								persistent_data.objsToInstantiate[ base_index + jj ] = (GameObject) obj;
							}

							if ( GUILayout.Button( "+" ) )
							{
								persistent_data.objsToInstantiate.Insert
									( base_index + jj, null );
								persistent_data.numObjsToInstantiate[ ii ]++;
								persistent_data.recalculateVariations[ ii ] = true;
								changed = true;
								break;
							}
							
							if ( GUILayout.Button( "-" ) )
							{
								if ( persistent_data.numObjsToInstantiate[ ii ] == 1 )
								{
									persistent_data.objsToInstantiate[ base_index ] = null;
								}
								else
								{
									persistent_data.objsToInstantiate.RemoveAt( base_index + jj );
									persistent_data.numObjsToInstantiate[ ii ]--;
								}
								persistent_data.recalculateVariations[ ii ] = true;
								changed = true;
								break;
							}

							GUILayout.EndHorizontal();

						}

					}

					if ( GUILayout.Button( "Recalculate Variations" ) )
					{
						for( int ii = 0; ii < unique_names.Count; ii++ )
						{
							persistent_data.recalculateVariations[ ii ] = true;
						}
						changed = true;
					}

				}
				
				
				if ( instancer.hasOverriddenInstances() )
				{
					if ( GUILayout.Button( "UnPin All Instances" ) ) 
					{
						instancer.unPinAllInstances();
						changed = true;
					}
				}
				
				if ( changed )
				{
					HoudiniProgressBar progress_bar = new HoudiniProgressBar();
					instancer.instanceObjects( progress_bar );
					progress_bar.clearProgressBar();

					for( int ii = 0; ii < persistent_data.recalculateVariations.Count; ii++ )
					{
						persistent_data.recalculateVariations[ ii ] = false;
					}
				}
			}

			EditorGUILayout.Separator();
		}
	}

	private HoudiniAssetOTL myAssetOTL;
	private HoudiniInstancerManager myInstanceManager;

}
