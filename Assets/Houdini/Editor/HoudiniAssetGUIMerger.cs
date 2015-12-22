using UnityEngine;
using UnityEditor;
using System.Collections;

[ ExecuteInEditMode ]
[ CustomEditor( typeof( HoudiniAssetMerger ) ) ]
public class HoudiniAssetGUIMerger : HoudiniAssetGUI
{

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public

	public override void OnEnable()
	{
		myAssetMerger = target as HoudiniAssetMerger;

		base.OnEnable();
	}

	public override void OnInspectorGUI()
	{
		if ( myAssetMerger == null )
			return;

		bool is_editable = true;

		// We can only build or do anything if we can link to our libraries.
#if !( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )
		is_editable = false;
		HoudiniGUI.help( HoudiniConstants.HAPI_UNSUPPORTED_PLATFORM_MSG, MessageType.Info );
#else
		if ( !is_editable )
			HoudiniGUI.help( "This mesh is not editable.", MessageType.Info );
#endif // !( UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX || ( UNITY_METRO && UNITY_EDITOR ) )

		bool gui_enable = GUI.enabled;
		GUI.enabled = is_editable;

		myAssetMerger.prShowInputSelection =
			HoudiniGUI.foldout( "Houdini Input Selection", myAssetMerger.prShowInputSelection, true );
		if ( myAssetMerger.prShowInputSelection ) 
		{
			Object input_object = myAssetMerger.prInputObject as Object;
			HoudiniGUI.objectField( "input_object", "Input Object", ref input_object, typeof( GameObject ) );
			myAssetMerger.prInputObject = input_object as GameObject;

			myAssetMerger.prLayerMask = EditorGUILayout.LayerField( myAssetMerger.prLayerMask );
			myAssetMerger.prTag = EditorGUILayout.TagField( myAssetMerger.prTag );
		}

		myAssetMerger.prShowHoudiniControls =
			HoudiniGUI.foldout( "Houdini Controls", myAssetMerger.prShowHoudiniControls, true );
		if ( myAssetMerger.prShowHoudiniControls ) 
		{
			if ( !myAssetMerger.isPrefab() )
			{
				if ( GUILayout.Button( "Recook" ) )
					myAssetMerger.buildClientSide();
			}
		}

		GUI.enabled = gui_enable;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private

	private HoudiniAssetMerger myAssetMerger;
}
