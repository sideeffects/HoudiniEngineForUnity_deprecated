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
using System.Reflection;
using HAPI;

[ CustomEditor( typeof( HAPI_InstancerManager ) ) ]
public partial class HAPI_InstancerManagerGUI: Editor 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public

	public virtual void OnEnable() 
	{
		myInstanceManager = target as HAPI_InstancerManager;

		myAssetOTL 			= myInstanceManager.gameObject.GetComponent< HAPI_AssetOTL >();
	}
	
	public override void OnInspectorGUI() 
	{


		HAPI_Instancer[] instancers = myAssetOTL.gameObject.GetComponentsInChildren< HAPI_Instancer >();
		if( !myAssetOTL.isPrefab() && instancers.Length > 0 )
		{

			generateAssetInstanceControls();
		}
	}
	


	private void generateAssetInstanceControls()
	{
		HAPI_InstancerManager instancer_manager = myAssetOTL.gameObject.GetComponent< HAPI_InstancerManager >();
		if( instancer_manager == null )
			return;

		List< HAPI_InstancerPersistentData > instancer_persistent_data = instancer_manager.prInstancerPersistentData;

		HAPI_Instancer[] instancers = myAssetOTL.gameObject.GetComponentsInChildren< HAPI_Instancer >();

		foreach( HAPI_Instancer instancer in instancers )
		{
			HAPI_InstancerPersistentData persistent_data = null;

			for( int ii = 0; ii < instancer_persistent_data.Count; ii++ )
			{
				HAPI_InstancerPersistentData data = instancer_persistent_data[ ii ];
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

			persistent_data.showInstancerGUI = HAPI_GUI.foldout( persistent_data.instancerName, 
			                                                    persistent_data.showInstancerGUI, true );
			if ( persistent_data.showInstancerGUI )
			{
				bool changed = false;
				
				{
					List< string > unique_names = persistent_data.uniqueNames;
					
					for( int ii = 0; ii < unique_names.Count; ii++ )
					{

						string instanced_name = unique_names[ ii ];
						int base_index = persistent_data.baseIndex( ii );



						for( int jj = 0; jj < persistent_data.numObjsToInstantiate[ ii ]; jj++ )
						{
							Object obj = (Object) persistent_data.objsToInstantiate[ base_index + jj ];

							GUILayout.BeginHorizontal();
							


							string label = "";
							if( jj == 0 )
								label = instanced_name;

							changed |= HAPI_GUI.objectField( "object_to_instantiate", label, 
							                                ref obj, typeof( GameObject ) );

							if( changed )
							{
								persistent_data.objsToInstantiate[ base_index + jj ] = (GameObject) obj;
							}

							if ( GUILayout.Button( "+" ) )
							{
								persistent_data.objsToInstantiate.Insert
									( base_index + jj, null );
								persistent_data.numObjsToInstantiate[ ii ]++;
								changed = true;
								break;
							}
							
							if ( GUILayout.Button( "-" ) )
							{
								if( persistent_data.numObjsToInstantiate[ ii ] == 1 )
								{
									persistent_data.objsToInstantiate[ base_index ] = null;
								}
								else
								{
									persistent_data.objsToInstantiate.RemoveAt( base_index + jj );
									persistent_data.numObjsToInstantiate[ ii ]--;
								}
								changed = true;
								break;
							}

							GUILayout.EndHorizontal();

						}

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
					HAPI_ProgressBar progress_bar = new HAPI_ProgressBar();
					instancer.instanceObjects( progress_bar );
					progress_bar.clearProgressBar();
				}
			}

			EditorGUILayout.Separator();
		}
	}

	private HAPI_AssetOTL myAssetOTL;
	private HAPI_InstancerManager myInstanceManager;

}
