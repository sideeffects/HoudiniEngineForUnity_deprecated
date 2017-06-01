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
#if !( HAPI_ENABLE_RUNTIME )
		is_editable = false;
		HoudiniGUI.help( HoudiniConstants.HAPI_UNSUPPORTED_PLATFORM_MSG, MessageType.Info );
#else
		if ( !is_editable )
			HoudiniGUI.help( "This mesh is not editable.", MessageType.Info );
#endif // !( HAPI_ENABLE_RUNTIME )

		bool gui_enable = GUI.enabled;
		GUI.enabled = is_editable;

		myAssetMerger.prShowInputSelection =
			HoudiniGUI.foldout( "Houdini Input Selection", myAssetMerger.prShowInputSelection, true );
		if ( myAssetMerger.prShowInputSelection ) 
		{
			bool changed = false;

			Object input_object = myAssetMerger.prInputObject as Object;
			GameObject temp_obj = null;
			HoudiniGUI.objectField(
				"input_object", "Input Object", ref input_object,
				typeof( GameObject ), null, ref temp_obj );
			myAssetMerger.prInputObject = input_object as GameObject;

			bool input_layer_enable = myAssetMerger.prUseLayerMask;
			HoudiniGUIParm input_layer_enable_parm = new HoudiniGUIParm( "input_layer_enable", "" );
			input_layer_enable_parm.joinNext = true;
			input_layer_enable_parm.labelNone = true;
			changed = HoudiniGUI.toggle( ref input_layer_enable_parm, ref input_layer_enable );
			if ( changed )
				myAssetMerger.prUseLayerMask = input_layer_enable;

			LayerMask input_layer = myAssetMerger.prLayerMask;
			HoudiniGUIParm input_layer_parm = new HoudiniGUIParm( "input_layer", "Layer" );
			input_layer_parm.disabled = !input_layer_enable;
			bool join_last = true;
			bool no_label_toggle_last = true;
			changed = HoudiniGUI.layerField( ref input_layer_parm, ref input_layer, ref join_last, ref no_label_toggle_last );
			if ( changed )
				myAssetMerger.prLayerMask = input_layer;

			bool input_tag_enable = myAssetMerger.prUseTag;
			HoudiniGUIParm input_tag_enable_parm = new HoudiniGUIParm( "input_tag_enable", "" );
			input_tag_enable_parm.joinNext = true;
			input_tag_enable_parm.labelNone = true;
			changed = HoudiniGUI.toggle( ref input_layer_enable_parm, ref input_tag_enable );
			if ( changed )
				myAssetMerger.prUseTag = input_tag_enable;

			string input_tag = myAssetMerger.prTag;
			HoudiniGUIParm input_tag_parm = new HoudiniGUIParm( "input_tag", "Tag" );
			input_tag_parm.disabled = !input_tag_enable;
			join_last = true;
			no_label_toggle_last = true;
			changed = HoudiniGUI.tagField( ref input_tag_parm, ref input_tag, ref join_last, ref no_label_toggle_last );
			if ( changed )
				myAssetMerger.prTag = input_tag;
		}

		myAssetMerger.prShowHoudiniControls =
			HoudiniGUI.foldout( "Houdini Controls", myAssetMerger.prShowHoudiniControls, true );
		if ( myAssetMerger.prShowHoudiniControls ) 
		{
			if ( GUILayout.Button( "Recook" ) )
				myAssetMerger.buildClientSide();
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
