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

public struct HoudiniGUIParm
{
	public HoudiniGUIParm( string name )
		: this( name, "", 1 )
	{}

	public HoudiniGUIParm( string name, string label )
		: this( name, label, 1 )
	{}
	
	public HoudiniGUIParm( string name, string label, int size )
	{
		id				= -1;

		type			= HAPI_ParmType.HAPI_PARMTYPE_INT;
		typeInfo		= "";

		this.size		= size;
		choiceCount 	= 0;
		width			= -1;
		
		this.name 		= name;
		this.label 		= label;
		labelExtraWidth	= 0;
		
		hasMin 			= false;
		hasMax 			= false;
		hasUIMin 		= false;
		hasUIMax 		= false;
		
		min 			= 0.0f;
		max 			= 0.0f;
		UIMin 			= 0.0f;
		UIMax 			= 0.0f;

		disabled		= false;
		
		joinNext 		= false;
		labelNone 		= false;

		isChildOfMultiParm 	= false;
		isRamp				= false;

		instanceNum 		= -1;
		instanceStartOffset = 1;

		valuesIndex		= 0;
		
		isBold 			= false;
	}
	
	public HoudiniGUIParm( HAPI_ParmInfo info, HAPI_ParmInfoStrings strings )
	{
		id				= info.id;

		type			= info.type;
		typeInfo		= strings.typeInfo;

		size 			= info.size;
		choiceCount 	= info.choiceCount;
		width			= -1;
		
		name 			= strings.name;
		label 			= strings.label;
		labelExtraWidth = 0;
		
		hasMin 			= info.hasMin;
		hasMax 			= info.hasMax;
		hasUIMin 		= info.hasUIMin;
		hasUIMax 		= info.hasUIMax;
		
		min 			= info.min;
		max 			= info.max;
		UIMin 			= info.UIMin;
		UIMax 			= info.UIMax;

		disabled		= info.disabled;
		
		joinNext 		= info.joinNext;
		labelNone 		= info.labelNone;

		isChildOfMultiParm = info.isChildOfMultiParm;
		isRamp = info.rampType != HAPI_RampType.HAPI_RAMPTYPE_MAX;

		instanceNum 	= info.instanceNum;
		instanceStartOffset = info.instanceStartOffset;
		
		if ( info.isInt() )
			valuesIndex = info.intValuesIndex;
		else if ( info.isFloat() )
			valuesIndex = info.floatValuesIndex;
		else if ( info.isString() )
			valuesIndex = info.stringValuesIndex;
		else
			valuesIndex = 0;
		
		isBold 			= false;
	}

	public int id;

	public HAPI_ParmType type;
	public string typeInfo;

	public int size;
	public int choiceCount;
	public int width;
	
	public string name;
	public string label;
	public int labelExtraWidth;
	
	public bool hasMin;
	public bool hasMax;
	public bool hasUIMin;
	public bool hasUIMax;
	
	public float min;
	public float max;
	public float UIMin;
	public float UIMax;

	public bool disabled;
	
	public bool joinNext;
	public bool labelNone;

	public bool isChildOfMultiParm;
	public int instanceNum;
	public int instanceStartOffset;
	
	public bool isRamp;
	
	public int valuesIndex;
	
	public bool isBold;
}

public class HoudiniGUI : Editor 
{
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public

	public static void flexibleSpace( int width, bool join_next, bool join_last )
	{
		initializeConstants();

		// Decide whether to join with the previous parameter on the same 
		// line or not.
		if ( !join_last )
			EditorGUILayout.BeginHorizontal();

		EditorGUILayout.SelectableLabel( "", myLabelStyle, GUILayout.Width( width ), myLineHeightGUI );

		join_last = join_next;
		if ( !join_next )
			EditorGUILayout.EndHorizontal();
	}

	public static void help( string value, MessageType message_type )
	{
		EditorGUILayout.HelpBox( value, message_type );
	}

	public static bool foldout( string label, bool value )
	{
		return foldout( label, value, false, 0 );
	}
	public static bool foldout( string label, bool value, bool is_bold )
	{
		return foldout( label, value, is_bold, 0 );
	}
	public static bool foldout( string label, bool value, bool is_bold, int indent )
	{
		const int indent_multiplier = 10;

		GUIStyle style = new GUIStyle( GUI.skin.FindStyle( "Foldout" ) );
		RectOffset rect_offset	= style.margin;
		rect_offset.left		+= indent * indent_multiplier;
		style.margin			= rect_offset;

		if ( is_bold )
			style.fontStyle = FontStyle.Bold;

		return EditorGUILayout.Foldout( value, 
										new GUIContent( label ),
										style );
	}
	
	public static bool dropdown< T >(
		string name, string label, 
		ref T value,
		string[] dropdown_labels,
		T[] dropdown_values,
	    Object undo_info, 
	    ref T undo_value )
	{
		return dropdown(
			name, label, ref value, false, dropdown_labels, 
			dropdown_values, undo_info, ref undo_value );
	}
	
	public static bool dropdown< T >(
		string name, string label, 
		ref T value,
		bool is_bold,
		string[] dropdown_labels,
		T[] dropdown_values,
		Object undo_info,
		ref T undo_value )
	{
		HoudiniGUIParm parm = new HoudiniGUIParm( name, label, 1 );
		parm.isBold = is_bold;
		T[] values = new T[ 1 ];
		values[ 0 ] = value;
		T[] undo_values = new T[ 1 ];
		undo_values[ 0 ] = undo_value;
		bool changed = dropdown(
			ref parm, ref values, dropdown_labels, 
			dropdown_values, undo_info, ref undo_values );
		value = values[ 0 ];
		undo_value = undo_values[ 0 ];
		return changed;
	}
	public static bool dropdown< T >(
		ref HoudiniGUIParm parm,
		ref T[] values,
		string[] dropdown_labels,
		T[] dropdown_values,
		Object undo_info,
		ref T[] undo_values )
	{
		bool join_last = false; bool no_label_toggle_last = false;
		return dropdown(
			ref parm, ref values, dropdown_labels, dropdown_values, 
			ref join_last, ref no_label_toggle_last, undo_info, 
		    ref undo_values );
	}
	public static bool dropdown< T >(
		ref HoudiniGUIParm parm,
		ref T[] values,
		string[] dropdown_labels,
		T[] dropdown_values,
		ref bool join_last, 
		ref bool no_label_toggle_last,
		Object undo_info,
		ref T[] undo_values )
	{
		initializeConstants();
		disableGUI( parm.disabled );
		
		bool changed = false;
		bool is_undo_event = Event.current.type == EventType.ValidateCommand && 
							 Event.current.commandName == "UndoRedoPerformed";
		int parm_size = parm.size;
		
		// Decide whether to join with the previous parameter on the same line or not.
		if ( !join_last )
			EditorGUILayout.BeginHorizontal();

		label( ref parm, ref join_last, ref no_label_toggle_last );

		// Check if value is being changed from an undo/redo event
		if ( is_undo_event && undo_info != null )
		{ 
			T undo_value = undo_values[ parm.valuesIndex ];
			if ( !values[ parm.valuesIndex ].Equals ( undo_value ) )
			{
				values[ parm.valuesIndex ] = undo_value;
				changed |= true;
			}
		}
		
		// Get old value.
		T old_value = values[ parm.valuesIndex ];

		// Create map integer array.
		int[] mapped_values = new int[ dropdown_values.Length ];
		for ( int i = 0; i < dropdown_values.Length; ++i )
			mapped_values[ i ] = i;

		int old_mapped_value = -1;
		for ( int i = 0; i < dropdown_values.Length; ++i )
			if ( old_value.Equals( dropdown_values[ i ] ) )
			{
				old_mapped_value = i;
				break;
			}

		// If the old value is blank just assume the first choice is selected.
		if ( old_mapped_value < 0 )
		{
			old_mapped_value = 0;
			changed |= true;
		}

		// Get Style
		GUIStyle popup_style = new GUIStyle( EditorStyles.popup );
		if ( parm.isBold )
			popup_style.fontStyle = FontStyle.Bold;

		// Sanitize Slashes
		for ( int i = 0; i < dropdown_labels.Length; ++i )
			dropdown_labels[ i ] = dropdown_labels[ i ].Replace( "/", "\u2044" );

		// Draw popup.
		GUI.SetNextControlName(
			parm.name + " " + parm.instanceNum + " dropdown_field" );
		int new_mapped_value = 0;
		if ( parm.width >= 0 )
			new_mapped_value = EditorGUILayout.IntPopup(
				old_mapped_value, dropdown_labels, mapped_values,
				popup_style, GUILayout.Width( parm.width ) );
		else
			new_mapped_value = EditorGUILayout.IntPopup(
				old_mapped_value, dropdown_labels, mapped_values, popup_style );

		T new_value = dropdown_values[ new_mapped_value ];

		// Determine if value changed and update parameter value.
		if ( !new_value.Equals( old_value ) )
		{
			values[ parm.valuesIndex ] = new_value;

			// record undo info
			if ( undo_info != null )
			{
				Undo.RecordObject( undo_info, parm.label );
				undo_values[ parm.valuesIndex ] = new_value;
			}

			changed |= true;
		}

		// Decide whether to join with the next parameter on the same line or not
		// but also save our status for the next parameter.
		join_last = ( parm.joinNext && parm_size <= 1 );
		if ( !parm.joinNext || parm_size > 1 )
			EditorGUILayout.EndHorizontal();
		
		restoreGUIEnable();

		return changed;
	}
	
	public static bool intField(
		string name, string label, ref int value,
		Object undo_info, ref int undo_value )
	{
		HoudiniGUIParm gui_parm = new HoudiniGUIParm( name, label );
		int[] values = new int[ 1 ];
		values[ 0 ] = value;
		int[] undo_values = new int[ 1 ];
		undo_values[ 0 ] = undo_value;
		bool delay_build = false;
		bool changed = intField( ref gui_parm, ref delay_build, ref values,
		                         undo_info, ref undo_values );
		value = values[ 0 ];
		undo_value = undo_values[ 0 ];
		return changed;
	}
	public static bool intField(
		string name, string label, ref int value, int ui_min, 
		int ui_max, Object undo_info, ref int undo_value )
	{
		HoudiniGUIParm gui_parm = new HoudiniGUIParm( name, label );
		gui_parm.hasUIMin = true;
		gui_parm.hasUIMax = true;
		gui_parm.UIMin = ui_min;
		gui_parm.UIMax = ui_max;
		int[] values = new int[ 1 ];
		values[ 0 ] = value;
		int[] undo_values = new int[ 1 ];
		undo_values[ 0 ] = undo_value;
		bool delay_build = false;
		bool changed = intField( ref gui_parm, ref delay_build, ref values, undo_info, ref undo_values );
		value = values[ 0 ];
		undo_value = undo_values[ 0 ];
		return changed;
	}
	public static bool intField(
		ref HoudiniGUIParm parm,
		ref bool delay_build,
		ref int[] values,
		Object undo_info,
		ref int[] undo_values )
	{
		bool join_last = false; bool no_label_toggle_last = false;
		return intField(
			ref parm, ref delay_build, ref values, ref join_last, 
			ref no_label_toggle_last, undo_info, ref undo_values );
	}
	public static bool intField(
		ref HoudiniGUIParm parm,
		ref bool delay_build,
		ref int[] values,
		ref bool join_last, 
		ref bool no_label_toggle_last,
		Object undo_info,
		ref int[] undo_values )
	{
		initializeConstants();
		disableGUI( parm.disabled );
		
		bool changed = false;
		bool is_undo_event = Event.current.type == EventType.ValidateCommand && 
							 Event.current.commandName == "UndoRedoPerformed";
		int parm_size = parm.size;
		
		// Decide whether to join with the previous parameter on the same line or not.
		if ( !join_last )
			EditorGUILayout.BeginHorizontal();
		
		label( ref parm, ref join_last, ref no_label_toggle_last );
		
		int per_line = 0;
		for ( int p = 0; p < parm_size; ++p, ++per_line )
		{
			if ( per_line >= myMaxFieldCountPerLine )
			{
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField( "", myToggleWidthGUI );
				EditorGUILayout.LabelField( "", getLabelWidth( parm ) );
				per_line = 0;
			}

			// Check if value is being changed from an undo/redo event
			if ( is_undo_event && undo_info != null )
			{ 
				int undo_value = undo_values[ parm.valuesIndex + p ];
				if ( values[ parm.valuesIndex + p ] != undo_value )
				{
					values[ parm.valuesIndex + p ] = undo_value;
					changed |= true;
				}
			}
			
			// Get old value.
			int old_value = values[ parm.valuesIndex + p ];
			
			// Get style
			GUIStyle int_field_style = new GUIStyle( EditorStyles.numberField );
			if ( parm.isBold )
			{
				int_field_style.fontStyle	= FontStyle.Bold;
			}
			
			// Draw field.
			GUI.SetNextControlName( parm.name +
									" " + parm.instanceNum +
									" " + p + " int_field" );
			int new_value = EditorGUILayout.IntField( old_value, int_field_style );
			if ( new_value != old_value ) // Check if the field is being used instead of the slider.
				delay_build = true;
			
			// Draw the slider.
			if ( parm_size == 1 && !join_last && !parm.joinNext )
			{
				float ui_min = ( parm.hasUIMin ? parm.UIMin : 0.0f );
				float ui_max = ( parm.hasUIMax ? parm.UIMax : 10.0f );
				Rect lastDoubleRect = getLastDoubleRect();
				mySliderStyle.stretchWidth = false;
				mySliderStyle.fixedWidth = lastDoubleRect.width;

				// Set the name of the UI that we generate to approximately the paramter name
				GUI.SetNextControlName( parm.name +
										" " + parm.instanceNum +
										" " + p + " int_slider" );

				new_value = (int) GUI.HorizontalSlider( lastDoubleRect, new_value, ui_min, ui_max, 
														mySliderStyle, mySliderThumbStyle );
			}
			
			// Enforce min/max bounds.
			if ( parm.hasMin && new_value < (int) parm.min )
				new_value = (int) parm.min;
			if ( parm.hasMax && new_value > (int) parm.max )
				new_value = (int) parm.max;
			
			// Determine if value changed and update parameter value.
			if ( new_value != old_value )
			{
				values[ parm.valuesIndex + p ] = new_value;

				// record undo info
				if ( undo_info != null )
				{
					Undo.RecordObject( undo_info, parm.label );
					undo_values[ parm.valuesIndex + p ] = new_value;
				}

				changed |= true;
			} // if
		} // for
		
		// Decide whether to join with the next parameter on the same line or not
		// but also save our status for the next parameter.
		join_last = ( parm.joinNext && parm_size <= 1 );
		if ( !parm.joinNext || parm_size > 1 )
			EditorGUILayout.EndHorizontal();

		restoreGUIEnable();

		return changed;
	}

	public static bool multiparmField(
		ref HoudiniGUIParm parm,
		ref bool delay_build,
		ref int[] values,
		ref bool join_last, ref bool no_label_toggle_last )
	{
		initializeConstants();
		disableGUI( parm.disabled );
		
		// Decide whether to join with the previous parameter on the same line or not.
		GUILayout.BeginHorizontal();
		label( ref parm, ref join_last, ref no_label_toggle_last );
		
		// Get old value.
		int old_value = values[ parm.valuesIndex ];
		
		// Get style
		GUIStyle int_field_style = new GUIStyle( EditorStyles.numberField );
		if ( parm.isBold )
		{
			int_field_style.fontStyle = FontStyle.Bold;
		}

		// Draw field.
		GUI.SetNextControlName( parm.name + " multiparm_field" );
		int new_value = EditorGUILayout.IntField( old_value, int_field_style );
		if ( new_value != old_value ) 
			delay_build = true;

		if ( GUILayout.Button( "+" ) )
			new_value++;

		if ( GUILayout.Button( "-" ) && new_value > 0 )
			new_value--;

		if ( GUILayout.Button( "Clear" ) )
			new_value = 0;

		GUILayout.EndHorizontal();

		// Determine if value changed and update parameter value.
		if ( new_value != old_value )
			values[ parm.valuesIndex ] = new_value;

		restoreGUIEnable();
		
		return new_value != old_value;
	}

	public static bool floatField(
		string name, string label, ref float value, Object undo_info, ref float undo_value )
	{
		HoudiniGUIParm gui_parm = new HoudiniGUIParm( name, label );
		float[] values = new float[ 1 ];
		values[ 0 ] = value;
		float[] undo_values = new float[ 1 ];
		undo_values[ 0 ] = undo_value;
		bool delay_build = false;
		bool changed = floatField( ref gui_parm, ref delay_build, ref values, undo_info, ref undo_values );
		value = values[ 0 ];
		undo_value = undo_values[ 0 ];
		return changed;
	}

	public static bool floatField(
		string name, string label, ref Vector3 value, Object undo_info, ref Vector3 undo_value )
	{
		HoudiniGUIParm gui_parm = new HoudiniGUIParm( name, label );
		gui_parm.size = 3;
		float[] values = new float[ 3 ];
		values[ 0 ] = value.x;
		values[ 1 ] = value.y;
		values[ 2 ] = value.z;
		float[] undo_values = new float[ 3 ];
		undo_values[ 0 ] = undo_value.x;
		undo_values[ 1 ] = undo_value.y;
		undo_values[ 2 ] = undo_value.z;
		bool delay_build = false;
		bool changed = floatField( ref gui_parm, ref delay_build, ref values, undo_info, ref undo_values );
		value.x = values[ 0 ];
		value.y = values[ 1 ];
		value.z = values[ 2 ];
		undo_value.x = undo_values[ 0 ];
		undo_value.y = undo_values[ 1 ];
		undo_value.z = undo_values[ 2 ];
		return changed;
	}


	public static bool floatField(
		string name, string label, ref float value, float ui_min, 
		float ui_max, Object undo_info, ref float undo_value )
	{
		HoudiniGUIParm gui_parm = new HoudiniGUIParm( name, label );
		gui_parm.hasUIMin = true;
		gui_parm.hasUIMax = true;
		gui_parm.UIMin = ui_min;
		gui_parm.UIMax = ui_max;
		float[] values = new float[ 1 ];
		values[ 0 ] = value;
		float[] undo_values = new float[ 1 ];
		undo_values[ 0 ] = undo_value;
		bool delay_build = false;
		bool changed = floatField( ref gui_parm, ref delay_build, ref values, undo_info, ref undo_values );
		value = values[ 0 ];
		undo_value = undo_values[ 0 ];
		return changed;
	}
	public static bool floatField(
		ref HoudiniGUIParm parm,
		ref bool delay_build,
		ref float[] values,
		Object undo_info, 
		ref float[] undo_values )
	{
		bool join_last = false; bool no_label_toggle_last = false;
		return floatField(
			ref parm, ref delay_build, ref values, ref join_last, 
			ref no_label_toggle_last, undo_info, ref undo_values );
	}
	public static bool floatField(
		ref HoudiniGUIParm parm,
		ref bool delay_build,
		ref float[] values,
		ref bool join_last, 
		ref bool no_label_toggle_last,
		Object undo_info,
		ref float[] undo_values )
	{
		initializeConstants();
		disableGUI( parm.disabled );
		
		bool changed = false;
		bool is_undo_event = Event.current.type == EventType.ValidateCommand && 
							 Event.current.commandName == "UndoRedoPerformed";
		int parm_size = parm.size;
		
		// Decide whether to join with the previous parameter on the same line or not.
		if ( !join_last )
			EditorGUILayout.BeginHorizontal();
		
		label( ref parm, ref join_last, ref no_label_toggle_last );
		
		int per_line = 0;
		for ( int p = 0; p < parm_size; ++p, ++per_line )
		{
			if ( per_line >= myMaxFieldCountPerLine )
			{
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField( "", myToggleWidthGUI );
				EditorGUILayout.LabelField( "", getLabelWidth( parm ) );
				per_line = 0;
			}

			// Check if value is being changed from an undo/redo event
			if ( is_undo_event && undo_info != null )
			{ 
				float undo_value = undo_values[ parm.valuesIndex + p ];
				if ( values[ parm.valuesIndex + p ] != undo_value )
				{
					values[ parm.valuesIndex + p ] = undo_value;
					changed |= true;
				}
			}
			
			// Get old value.
			float old_value = values[ parm.valuesIndex + p ];
			
			// Get style
			GUIStyle float_field_style = new GUIStyle( EditorStyles.numberField );
			if ( parm.isBold )
			{
				float_field_style.fontStyle	= FontStyle.Bold;
			}
			
			// Draw field.
			GUI.SetNextControlName( parm.name +
									" " + parm.instanceNum +
									" " + p + " float_field" );
			float new_value = EditorGUILayout.FloatField( old_value, float_field_style );
			if ( new_value != old_value ) // Check if the field is being used instead of the slider.
				delay_build = true;
			
			// Draw the slider.
			if ( parm_size == 1 && !join_last && !parm.joinNext )
			{
				float ui_min = ( parm.hasUIMin ? parm.UIMin : 0.0f );
				float ui_max = ( parm.hasUIMax ? parm.UIMax : 10.0f );
				Rect lastDoubleRect = getLastDoubleRect();
				mySliderStyle.stretchWidth = false;
				mySliderStyle.fixedWidth = lastDoubleRect.width;
				GUI.SetNextControlName(
					parm.name +
					" " + parm.instanceNum +
					" " + p + " float_slider" );
				new_value = GUI.HorizontalSlider(
					lastDoubleRect, new_value, ui_min, ui_max, 
					mySliderStyle, mySliderThumbStyle );
			}
			
			// Enforce min/max bounds.
			if ( parm.hasMin && new_value < parm.min )
				new_value = parm.min;
			if ( parm.hasMax && new_value > parm.max )
				new_value = parm.max;
			
			// Determine if value changed and update parameter value.
			if ( new_value != old_value )
			{
				values[ parm.valuesIndex + p ] = new_value;

				// record undo info
				if ( undo_info != null )
				{
					Undo.RecordObject( undo_info, parm.label );
					undo_values[ parm.valuesIndex + p ] = new_value;
				}
				
				changed |= true;
			} // if
		} // for
		
		// Decide whether to join with the next parameter on the same line or not
		// but also save our status for the next parameter.
		join_last = ( parm.joinNext && parm_size <= 1 );
		if ( !parm.joinNext || parm_size > 1 )
			EditorGUILayout.EndHorizontal();

		restoreGUIEnable();
		
		return changed;
	}
	
	public static bool stringField(
		string name, string label, ref string value,
		Object undo_info, ref string undo_value )
	{
		HoudiniGUIParm gui_parm = new HoudiniGUIParm( name, label );
		string[] values = new string[ 1 ];
		values[ 0 ] = value;
		string[] undo_values = new string[ 1 ];
		undo_values[ 0 ] = undo_value;
		bool delay_build = false;
		bool changed = stringField( ref gui_parm, ref delay_build, ref values,
		                            undo_info, ref undo_values );
		value = values[ 0 ];
		undo_value = undo_values[ 0 ];
		return changed;
	}
	public static bool stringField(
		ref HoudiniGUIParm parm,
		ref bool delay_build,
		ref string[] values,
		Object undo_info,
		ref string[] undo_values )
	{
		bool join_last = false; bool no_label_toggle_last = false;
		return stringField(
			ref parm, ref delay_build, ref values, ref join_last, 
			ref no_label_toggle_last, undo_info, ref undo_values );
	}
	public static bool stringField(
		ref HoudiniGUIParm parm,
		ref bool delay_build,
		ref string[] values,
		ref bool join_last, 
		ref bool no_label_toggle_last,
		Object undo_info,
		ref string[] undo_values )
	{
		initializeConstants();
		disableGUI( parm.disabled );
		
		bool changed = false;
		bool is_undo_event = Event.current.type == EventType.ValidateCommand && 
							 Event.current.commandName == "UndoRedoPerformed";
		int parm_size = parm.size;
		
		// Decide whether to join with the previous parameter on the same line or not.
		if ( !join_last )
			EditorGUILayout.BeginHorizontal();
		
		label( ref parm, ref join_last, ref no_label_toggle_last );
		
		int per_line = 0;
		for ( int p = 0; p < parm_size; ++p, ++per_line )
		{
			if ( per_line >= myMaxFieldCountPerLine )
			{
				EditorGUILayout.EndHorizontal();
				EditorGUILayout.BeginHorizontal();
				EditorGUILayout.LabelField( "", myToggleWidthGUI );
				EditorGUILayout.LabelField( "", getLabelWidth( parm ) );
				per_line = 0;
			}

			// Check if value is being changed from an undo/redo event
			if ( is_undo_event && undo_info != null )
			{ 
				string undo_value = undo_values[ parm.valuesIndex + p ];
				if ( values[ parm.valuesIndex + p ] != undo_value )
				{
					values[ parm.valuesIndex + p ] = undo_value;
					changed |= true;
				}
			}
			
			// Get old value.
			string old_value = values[ parm.valuesIndex + p ];
			
			// Get style
			GUIStyle text_field_style = new GUIStyle( EditorStyles.textField );
			if ( parm.isBold )
			{
				text_field_style.fontStyle	= FontStyle.Bold;
			}
			
			// Draw field.
			GUI.SetNextControlName(
				parm.name +
				" " + parm.instanceNum +
				" " + p + " string_field" );
			string new_value = EditorGUILayout.TextField( old_value, text_field_style );
			if ( new_value != old_value ) // Check if the field is being used instead of the slider.
				delay_build = true;
			
			// Determine if value changed and update parameter value. 
			if ( new_value != old_value )
			{
				values[ parm.valuesIndex + p ] = new_value;

				// record undo info
				if ( undo_info != null )
				{
					Undo.RecordObject( undo_info, parm.label );
					undo_values[ parm.valuesIndex + p ] = new_value;
				}

				changed |= true;
			}
		}
		
		// Decide whether to join with the next parameter on the same line or not
		// but also save our status for the next parameter.
		join_last = ( parm.joinNext && parm_size <= 1 );
		if ( !parm.joinNext || parm_size > 1 )
			EditorGUILayout.EndHorizontal();

		restoreGUIEnable();
		
		return changed;
	}
	
	public static bool objectField(
		string name, string label, ref Object obj, System.Type type, Object undo_info,
		ref GameObject undo_value )
	{
		HoudiniGUIParm gui_parm = new HoudiniGUIParm( name, label );
		return objectField( ref gui_parm, ref obj, type, undo_info, ref undo_value );
	}
	public static bool objectField(
		ref HoudiniGUIParm parm, ref Object obj, System.Type type, Object undo_info,
		ref GameObject undo_value )
	{
		bool join_last = false; bool no_label_toggle_last = false;
		return objectField( ref parm, ref obj, type, ref join_last,
			ref no_label_toggle_last, undo_info, ref undo_value );
	}
	public static bool objectField(
		ref HoudiniGUIParm parm, ref Object obj, System.Type type,
		ref bool join_last, ref bool no_label_toggle_last, Object undo_info,
		ref GameObject undo_value )
	{
		initializeConstants();
		disableGUI( parm.disabled );
		
		bool changed = false;
		int parm_size = parm.size;
		
		// Decide whether to join with the previous parameter on the same line or not.
		if ( !join_last )
			EditorGUILayout.BeginHorizontal();
		
		label( ref parm, ref join_last, ref no_label_toggle_last );
		
		Object old_obj = obj;
		Object new_obj = EditorGUILayout.ObjectField( old_obj, type, true );
		
		if ( new_obj != old_obj )
		{
			obj = new_obj;

			// record undo info
			if ( undo_info != null )
			{
				Undo.RecordObject( undo_info, parm.label );
				undo_value = (GameObject) new_obj;
			}

			changed |= true;
		}
		
		// Decide whether to join with the next parameter on the same line or not
		// but also save our status for the next parameter.
		join_last = ( parm.joinNext && parm_size <= 1 );
		if ( !parm.joinNext || parm_size > 1 )
			EditorGUILayout.EndHorizontal();

		restoreGUIEnable();
		
		return changed;
	}

	public static bool layerField(
		string name, string label, ref LayerMask layer )
	{
		HoudiniGUIParm gui_parm = new HoudiniGUIParm( name, label );
		return layerField( ref gui_parm, ref layer );
	}
	public static bool layerField(
		ref HoudiniGUIParm parm, ref LayerMask layer )
	{
		bool join_last = false; bool no_label_toggle_last = false;
		return layerField( ref parm, ref layer, ref join_last, ref no_label_toggle_last );
	}
	public static bool layerField(
		ref HoudiniGUIParm parm, ref LayerMask layer,
		ref bool join_last, ref bool no_label_toggle_last )
	{
		initializeConstants();
		disableGUI( parm.disabled );
		
		bool changed = false;
		int parm_size = parm.size;
		
		// Decide whether to join with the previous parameter on the same line or not.
		if ( !join_last )
			EditorGUILayout.BeginHorizontal();
		
		label( ref parm, ref join_last, ref no_label_toggle_last );
		
		LayerMask old_layer = layer;
		LayerMask new_layer = EditorGUILayout.LayerField( old_layer );
		
		if ( new_layer != old_layer )
		{
			layer = new_layer;
			changed |= true;
		}
		
		// Decide whether to join with the next parameter on the same line or not
		// but also save our status for the next parameter.
		join_last = ( parm.joinNext && parm_size <= 1 );
		if ( !parm.joinNext || parm_size > 1 )
			EditorGUILayout.EndHorizontal();

		restoreGUIEnable();
		
		return changed;
	}

	public static bool tagField(
		string name, string label, ref string tag )
	{
		HoudiniGUIParm gui_parm = new HoudiniGUIParm( name, label );
		return tagField( ref gui_parm, ref tag );
	}
	public static bool tagField(
		ref HoudiniGUIParm parm, ref string tag )
	{
		bool join_last = false; bool no_label_toggle_last = false;
		return tagField( ref parm, ref tag, ref join_last, ref no_label_toggle_last );
	}
	public static bool tagField(
		ref HoudiniGUIParm parm, ref string tag,
		ref bool join_last, ref bool no_label_toggle_last )
	{
		initializeConstants();
		disableGUI( parm.disabled );
		
		bool changed = false;
		int parm_size = parm.size;
		
		// Decide whether to join with the previous parameter on the same line or not.
		if ( !join_last )
			EditorGUILayout.BeginHorizontal();
		
		label( ref parm, ref join_last, ref no_label_toggle_last );
		
		string old_tag = tag;
		string new_tag = EditorGUILayout.TagField( old_tag );
		
		if ( new_tag != old_tag )
		{
			tag = new_tag;
			changed |= true;
		}
		
		// Decide whether to join with the next parameter on the same line or not
		// but also save our status for the next parameter.
		join_last = ( parm.joinNext && parm_size <= 1 );
		if ( !parm.joinNext || parm_size > 1 )
			EditorGUILayout.EndHorizontal();

		restoreGUIEnable();
		
		return changed;
	}
	
	public static bool fileOpenField(
		string name, string label,
		ref bool delay_build,
		ref string path )
	{
		HoudiniGUIParm parm = new HoudiniGUIParm( name, label );
		return fileOpenField( ref parm, ref delay_build, ref path );
	}
	public static bool fileOpenField(
		ref HoudiniGUIParm parm,
		ref bool delay_build,
		ref string path )
	{
		bool join_last = false; bool no_label_toggle_last = false;
		return fileOpenField(
			ref parm, ref delay_build, ref path, ref join_last, ref no_label_toggle_last );
	}
	public static bool fileOpenField(
		ref HoudiniGUIParm parm,
		ref bool delay_build,
		ref string path,
		ref bool join_last, ref bool no_label_toggle_last )
	{
		initializeConstants();
		disableGUI( parm.disabled );
		
		bool changed = false;
		int parm_size = parm.size;
		
		// Decide whether to join with the previous parameter on the same line or not.
		if ( !join_last )
			EditorGUILayout.BeginHorizontal();
		
		label( ref parm, ref join_last, ref no_label_toggle_last );
		
		// Get style
		GUIStyle text_field_style = new GUIStyle( EditorStyles.textField );
		if ( parm.isBold )
		{
			text_field_style.fontStyle	= FontStyle.Bold;
		}
		
		string old_path = path;
		GUI.SetNextControlName( parm.name +
								" " + parm.instanceNum +
								" " + " file_field" );
		string new_path = EditorGUILayout.TextField( old_path, text_field_style );
		if ( new_path != old_path ) // Check if the field is being used instead of the slider.
			delay_build = true;

		if ( GUILayout.Button( "...", GUILayout.Width( myFileChooserButtonWidth ), myLineHeightGUI ) ) 
		{
			string file_pattern = "*";
			if ( parm.typeInfo != "" )
			{
				file_pattern = parm.typeInfo;
				file_pattern = file_pattern.Replace( " ", ";" );
				if ( file_pattern.StartsWith( "*." ) )
					file_pattern = file_pattern.Substring( 2 );
				else if ( file_pattern.StartsWith( "*" ) )
					file_pattern = file_pattern.Substring( 1 );
			}

			string prompt_path = EditorUtility.OpenFilePanel( "Select File", old_path, file_pattern );;
			if ( prompt_path.Length > 0 )
				new_path = prompt_path;
		}
		
		if ( new_path != old_path )
		{
			path = new_path;
			changed |= true;
		}
		
		// Decide whether to join with the next parameter on the same line or not
		// but also save our status for the next parameter.
		join_last = ( parm.joinNext && parm_size <= 1 );
		if ( !parm.joinNext || parm_size > 1 )
			EditorGUILayout.EndHorizontal();

		restoreGUIEnable();
		
		return changed;
	}

	public static bool fileSaveField(
		string name, string label,
		ref bool delay_build,
		ref string path )
	{
		HoudiniGUIParm parm = new HoudiniGUIParm( name, label );
		return fileSaveField( ref parm, ref delay_build, ref path );
	}
	public static bool fileSaveField(
		ref HoudiniGUIParm parm,
		ref bool delay_build,
		ref string path )
	{
		bool join_last = false; bool no_label_toggle_last = false;
		return fileSaveField(
			ref parm, ref delay_build, ref path, ref join_last, ref no_label_toggle_last );
	}
	public static bool fileSaveField(
		ref HoudiniGUIParm parm,
		ref bool delay_build,
		ref string path,
		ref bool join_last, ref bool no_label_toggle_last )
	{
		initializeConstants();
		disableGUI( parm.disabled );
		
		bool changed = false;
		int parm_size = parm.size;
		
		// Decide whether to join with the previous parameter on the same line or not.
		if ( !join_last )
			EditorGUILayout.BeginHorizontal();
		
		label( ref parm, ref join_last, ref no_label_toggle_last );
		
		// Get style
		GUIStyle text_field_style = new GUIStyle( EditorStyles.textField );
		if ( parm.isBold )
		{
			text_field_style.fontStyle	= FontStyle.Bold;
		}
		
		string old_path = path;
		GUI.SetNextControlName( parm.name +
								" " + parm.instanceNum +
								" " + " file_field" );
		string new_path = EditorGUILayout.TextField( old_path, text_field_style );
		if ( new_path != old_path ) // Check if the field is being used instead of the slider.
			delay_build = true;

		if ( GUILayout.Button( "...", GUILayout.Width( myFileChooserButtonWidth ), myLineHeightGUI ) ) 
		{
			string file_pattern = "*";
			if ( parm.typeInfo != "" )
			{
				file_pattern = parm.typeInfo;
				file_pattern = file_pattern.Replace( " ", ";" );
				if ( file_pattern.StartsWith( "*." ) )
					file_pattern = file_pattern.Substring( 2 );
				else if ( file_pattern.StartsWith( "*" ) )
					file_pattern = file_pattern.Substring( 1 );
			}

			string prompt_path = EditorUtility.SaveFilePanel( "Select File", old_path, "", file_pattern );;
			if ( prompt_path.Length > 0 )
				new_path = prompt_path;
		}
		
		if ( new_path != old_path )
		{
			path = new_path;
			changed |= true;
		}
		
		// Decide whether to join with the next parameter on the same line or not
		// but also save our status for the next parameter.
		join_last = ( parm.joinNext && parm_size <= 1 );
		if ( !parm.joinNext || parm_size > 1 )
			EditorGUILayout.EndHorizontal();

		restoreGUIEnable();
		
		return changed;
	}
	
	public static bool toggle(
		string name, string label, ref bool value,
		Object undo_info, ref bool undo_value )
	{
		return toggle( name, label, false, ref value, undo_info, ref undo_value );
	}
	public static bool toggle(
		ref HoudiniGUIParm parm, ref bool value )
	{
		int[] values = new int[ 1 ];
		values[ 0 ] = ( value ? 1 : 0 );
		int[] undo_values = new int[ 1 ];
		undo_values[ 0 ] = values[ 0 ];

		bool result = toggle( ref parm, ref values, null, ref undo_values );

		value = ( values[ 0 ] == 1 ?  true : false );
		return result;
	}
	public static bool toggle(
		string name, string label, bool is_bold, ref bool value,
		Object undo_info, ref bool undo_value )
	{
		HoudiniGUIParm gui_parm = new HoudiniGUIParm( name, label );
		gui_parm.isBold = is_bold;
		int[] values = new int[ 1 ];
		values[ 0 ] = ( value ? 1 : 0 );
		int[] undo_values = new int[ 1 ];
		undo_values[ 0 ] = ( undo_value ? 1 : 0 );

		bool result = toggle( ref gui_parm, ref values, undo_info, ref undo_values );

		value = ( values[ 0 ] == 1 ?  true : false );
		undo_value = ( undo_values[ 0 ] == 1 ? true : false );
		return result;
	}
	public static bool toggle(
		ref HoudiniGUIParm parm,
		ref int[] values,
		Object undo_info,
		ref int[] undo_values )
	{
		bool join_last = false; bool no_label_toggle_last = false;
		return toggle( ref parm, ref values, ref join_last, ref no_label_toggle_last,
		               undo_info, ref undo_values );
	}
	public static bool toggle(
		ref HoudiniGUIParm parm,
		ref int[] values,
		ref bool join_last, 
		ref bool no_label_toggle_last,
		Object undo_info,
		ref int[] undo_values )
	{
		initializeConstants();
		disableGUI( parm.disabled );
		
		bool changed = false;
		bool is_undo_event = Event.current.type == EventType.ValidateCommand && 
							 Event.current.commandName == "UndoRedoPerformed";
		int parm_size = parm.size;
		
		// Decide whether to join with the previous parameter on the same line or not.
		if ( !join_last )
			EditorGUILayout.BeginHorizontal();
		
		if ( !parm.joinNext )
		{
			// Add padding for the toggle column.
			EditorGUILayout.LabelField( myNullContent, myToggleWidthGUI );
			// Add empty space to align with fields.
			EditorGUILayout.LabelField( myNullContent, getLabelWidth( parm ) );
		}

		// Check if value is being changed from an undo/redo event
		if ( is_undo_event && undo_info != null )
		{ 
			int undo_value = undo_values[ parm.valuesIndex ];
			if ( values[ parm.valuesIndex ] != undo_value )
			{
				values[ parm.valuesIndex ] = undo_value;
				changed |= true;
			}
		}
		
		// Get old value.
		int old_value = values[ parm.valuesIndex ];
		
		// Get style
		GUIStyle label_style = new GUIStyle( EditorStyles.label );
		if ( parm.isBold )
		{
			label_style.fontStyle = FontStyle.Bold;
		}
		
		// Draw toggle with its label.
		bool toggle_result = EditorGUILayout.Toggle( old_value != 0, myToggleWidthGUI );
		int new_value = ( toggle_result ? 1 : 0 );
		if ( !parm.labelNone )
			EditorGUILayout.SelectableLabel( parm.label, label_style, myLineHeightGUI );
		else
			no_label_toggle_last = true;
		
		// Determine if value changed and update parameter value.
		if ( new_value != old_value )
		{
			values[ parm.valuesIndex ] = new_value;

			// record undo info
			if ( undo_info != null )
			{
				Undo.RecordObject( undo_info, parm.label );
				undo_values[ parm.valuesIndex ] = new_value;
			}

			changed |= true;
		}
		
		// Decide whether to join with the next parameter on the same line or not
		// but also save our status for the next parameter.
		join_last = ( parm.joinNext && parm_size <= 1 );
		if ( !parm.joinNext || parm_size > 1 )
			EditorGUILayout.EndHorizontal();

		restoreGUIEnable();
		
		return changed;
	}
	
	public static bool colourField(
		string name, string label, ref Color value,
		Object undo_info, ref Color undo_value )
	{
		HoudiniGUIParm gui_parm = new HoudiniGUIParm( name, label );
		gui_parm.size = 4;
		bool delay_build = false;
		float[] values = new float[ 4 ];
		values[ 0 ] = value.r;
		values[ 1 ] = value.g;
		values[ 2 ] = value.b;
		values[ 3 ] = value.a;
		float[] undo_values = new float[ 4 ];
		undo_values[ 0 ] = undo_value.r;
		undo_values[ 1 ] = undo_value.g;
		undo_values[ 2 ] = undo_value.b;
		undo_values[ 3 ] = undo_value.a;
		bool result = colourField( ref gui_parm, ref delay_build, ref values,
		                           undo_info, ref undo_values );
		value.r = values[ 0 ];
		value.g = values[ 1 ];
		value.b = values[ 2 ];
		value.a = values[ 3 ];
		undo_value.r = undo_values[ 0 ];
		undo_value.g = undo_values[ 1 ];
		undo_value.b = undo_values[ 2 ];
		undo_value.a = undo_values[ 3 ];
		return result;
	}
	public static bool colourField(
		ref HoudiniGUIParm parm,
		ref bool delay_build,
		ref float[] values,
		Object undo_info,
		ref float[] undo_values )
	{
		bool join_last = false; bool no_label_toggle_last = false;
		return colourField(
			ref parm, ref delay_build, ref values, ref join_last, 
			ref no_label_toggle_last, undo_info, ref undo_values );
	}
	public static bool colourField(
		ref HoudiniGUIParm parm,
		ref bool delay_build,
		ref float[] values,
		ref bool join_last, 
		ref bool no_label_toggle_last,
		Object undo_info,
		ref float[] undo_values )
	{
		initializeConstants();
		disableGUI( parm.disabled );
		
		bool changed = false;
		bool is_undo_event = Event.current.type == EventType.ValidateCommand && 
							 Event.current.commandName == "UndoRedoPerformed";
		int parm_size = parm.size;
		
		// Decide whether to join with the previous parameter on the same line or not.
		if ( !join_last )
			EditorGUILayout.BeginHorizontal();
		
		label( ref parm, ref join_last, ref no_label_toggle_last );

		// Check if value is being changed from an undo/redo event
		if ( is_undo_event && undo_info != null )
		{
			if ( values[ parm.valuesIndex + 0 ] != undo_values[ parm.valuesIndex + 0 ] ||
			     values[ parm.valuesIndex + 1 ] != undo_values[ parm.valuesIndex + 1 ] ||
			     values[ parm.valuesIndex + 2 ] != undo_values[ parm.valuesIndex + 2 ] ||
			     ( parm_size > 3 && values[ parm.valuesIndex + 3 ] != undo_values[ parm.valuesIndex + 3 ] ) )
			{
				values[ parm.valuesIndex + 0 ] = undo_values[ parm.valuesIndex + 0 ];
				values[ parm.valuesIndex + 1 ] = undo_values[ parm.valuesIndex + 1 ];
				values[ parm.valuesIndex + 2 ] = undo_values[ parm.valuesIndex + 2 ];

				if ( parm_size > 3 )  
					values[ parm.valuesIndex + 3 ] = undo_values[ parm.valuesIndex + 3 ];

				changed |= true;
			}
		}
		
		Color old_color = new Color(
			values[ parm.valuesIndex + 0 ], 
			values[ parm.valuesIndex + 1 ], 
			values[ parm.valuesIndex + 2 ] );
		if ( parm_size > 3 )
			old_color.a = values[ parm.valuesIndex + 3 ];
		
		// Draw colour control.
		// This control throws a "harmless" exception every time it is clicked so
		// we have to consume that exception before it does real damage.
		Color new_color = old_color;
		try { new_color = EditorGUILayout.ColorField( old_color ); }
		catch {}

		// Determine if value changed and update parameter value.
		if ( new_color != old_color )
		{
			values[ parm.valuesIndex + 0 ] = new_color.r;
			values[ parm.valuesIndex + 1 ] = new_color.g;
			values[ parm.valuesIndex + 2 ] = new_color.b;
			
			if ( parm_size > 3 )
				values[ parm.valuesIndex + 3 ] = new_color.a;

			// record undo info
			if ( undo_info != null )
			{
				Undo.RecordObject( undo_info, parm.label );
				undo_values[ parm.valuesIndex + 0 ] = new_color.r;
				undo_values[ parm.valuesIndex + 1 ] = new_color.g;
				undo_values[ parm.valuesIndex + 2 ] = new_color.b;
				
				if ( parm_size > 3 )
					undo_values[ parm.valuesIndex + 3 ] = new_color.a;
			}
		
			changed |= true;
		}
		
		// Decide whether to join with the next parameter on the same line or not
		// but also save our status for the next parameter.
		join_last = ( parm.joinNext && parm_size <= 1 );
		if ( !parm.joinNext || parm_size > 1 )
			EditorGUILayout.EndHorizontal();
		
		restoreGUIEnable();

		return changed;
	}
	
	public static bool button( string name, string label )
	{
		HoudiniGUIParm gui_parm = new HoudiniGUIParm( name, label );
		bool result = button( ref gui_parm );
		return result;
	}
	public static bool button( ref HoudiniGUIParm parm )
	{
		bool join_last = false; bool no_label_toggle_last = false;
		return button( ref parm, ref join_last, ref no_label_toggle_last );
	}
	public static bool button(
		ref HoudiniGUIParm parm,
		ref bool join_last, ref bool no_label_toggle_last )
	{
		initializeConstants();
		disableGUI( parm.disabled );
		
		bool changed = false;
		int parm_size = parm.size;
		
		// Decide whether to join with the previous parameter on the same line or not.
		if ( !join_last )
			EditorGUILayout.BeginHorizontal();
		
		if ( !parm.joinNext )
		{
			// Add padding for the toggle column.
			EditorGUILayout.LabelField( myNullContent, myToggleWidthGUI );
			// Add empty space to align with fields.
			EditorGUILayout.LabelField( myNullContent, getLabelWidth( parm ) );
		}

		// Buttons should be only as wide as the text in them.
		float min_width, max_width;
		myLabelStyle.CalcMinMaxWidth( new GUIContent( parm.label ), out min_width, out max_width );
		min_width += 20; // Since the min width dosn't take into account button decoration.
		
		// Draw button.
		if ( GUILayout.Button( parm.label, GUILayout.Width( min_width ), myLineHeightGUI ) ) 
		{
			changed = true;
		}
		
		// Decide whether to join with the next parameter on the same line or not
		// but also save our status for the next parameter.
		join_last = ( parm.joinNext && parm_size <= 1 );
		if ( !parm.joinNext || parm_size > 1 )
			EditorGUILayout.EndHorizontal();

		restoreGUIEnable();
		
		return changed;
	}

	public static void label(
		ref HoudiniGUIParm parm,
		ref bool join_last, ref bool no_label_toggle_last )
	{
		if ( !parm.labelNone )
		{
			GUIContent label_content = new GUIContent( parm.label );

			float label_final_width =
				( parm.isChildOfMultiParm ? myLabelWidthMulti : myLabelWidth ) +
				(float) parm.labelExtraWidth;
			GUILayoutOption label_final_height = myLineHeightGUI;
			
			if ( parm.isBold )
				myLabelStyle.fontStyle = FontStyle.Bold;
			else
				myLabelStyle.fontStyle = FontStyle.Normal;

			if ( parm.type == HAPI_ParmType.HAPI_PARMTYPE_LABEL )
			{
				myLabelStyle.alignment	= TextAnchor.MiddleLeft;
				myLabelStyle.wordWrap	= true;
			}
			else
			{
				myLabelStyle.alignment	= TextAnchor.MiddleRight;
				myLabelStyle.wordWrap	= false;
			}

			if ( join_last && !no_label_toggle_last )
			{
				float min_width;
				float max_width;
				myLabelStyle.CalcMinMaxWidth( label_content, out min_width, out max_width );
				label_final_width = min_width + (float) parm.labelExtraWidth;
			}
			else if ( !join_last && parm.type != HAPI_ParmType.HAPI_PARMTYPE_LABEL )
			{
				// Add padding for the toggle column.
				EditorGUILayout.LabelField( "", myToggleWidthGUI );
			}

			GUILayoutOption label_final_width_gui = GUILayout.Width( label_final_width );
			if ( parm.type == HAPI_ParmType.HAPI_PARMTYPE_LABEL )
			{
				float height = myLabelStyle.CalcHeight( label_content, label_final_width );
				label_final_height = GUILayout.Height( height );
			}

			EditorGUILayout.SelectableLabel(
				parm.label, myLabelStyle, label_final_width_gui, label_final_height );
			no_label_toggle_last = false;
		}
	}

	public static bool separator()
	{
		EditorGUILayout.Separator();
		return false;
	}
	
	public const float myMaxFieldCountPerLine		= 4;
	
	public const float myFileChooserButtonWidth 	= 30;
	public const float myLineHeight 				= 16;
	public const float myLabelWidth 				= 120;
	public const float myToggleWidth 				= 12;
	public const float myLabelWidthMulti 			= 100;
	public const float myMultiButtonWidth 			= 5;
	public const float myDummyLabelMinWidth			= 20;
	
	public const float myDefaultUIMin 				= 0.0f;
	public const float myDefaultUIMax 				= 10.0f;
	
	public static GUILayoutOption 	myLineHeightGUI 			= GUILayout.Height( myLineHeight );
	public static GUILayoutOption 	myLabelWidthGUI 			= GUILayout.Width( myLabelWidth );
	public static GUILayoutOption 	myLabelWidthMultiGUI		= GUILayout.Width( myLabelWidthMulti );
	public static GUILayoutOption 	myToggleWidthGUI 			= GUILayout.Width( myToggleWidth );
	public static GUILayoutOption	myDummyLabelMinWidthGUI		= GUILayout.MinWidth( myDummyLabelMinWidth );
	
	public static GUIContent 		myNullContent 				= new GUIContent( "" );

	private static GUILayoutOption getLabelWidth( HoudiniGUIParm parm )
	{
		return parm.isChildOfMultiParm ? myLabelWidthMultiGUI : myLabelWidthGUI;
	}
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private

	private static void disableGUI( bool disabled )
	{
		if ( myIsDisablingGUI )
			restoreGUIEnable();

		myPreviousGUIEnableState = GUI.enabled;
		GUI.enabled = !disabled && myPreviousGUIEnableState;
		myIsDisablingGUI = true;
	}

	private static void restoreGUIEnable()
	{
		GUI.enabled = myPreviousGUIEnableState;
		myIsDisablingGUI = false;
	}

	/// <summary>
	/// 	Creates two empty label fields, gets the rectangles from each, and combines it to create
	/// 	the last double rectangle. This is used for <see cref="GUI.HorizontalSlider"/> which
	/// 	uses absolute positioning and needs a rectangle to know it's size and position.
	/// 	This way, we can insert sliders within the relative positioning of the Inspector GUI elements.
	/// </summary>
	/// <returns>
	/// 	The last double rectangle.
	/// </returns>
	private static Rect getLastDoubleRect()
	{
		// Draw first empty label field. 
		EditorGUILayout.LabelField( myNullContent, myDummyLabelMinWidthGUI );
		float xMin 		= GUILayoutUtility.GetLastRect().xMin;
		float yMin 		= GUILayoutUtility.GetLastRect().yMin;
		float width 	= GUILayoutUtility.GetLastRect().width;
		float height 	= GUILayoutUtility.GetLastRect().height;
		
		// Draw second empty label field.
		EditorGUILayout.LabelField( myNullContent, myDummyLabelMinWidthGUI );
		float width2	= GUILayoutUtility.GetLastRect().width;
		
		// Create the double rectangle from the two above.
		Rect last_double_rect = new Rect( xMin, yMin, width + width2, height );
		
		return last_double_rect;
	}

	private static void initializeConstants()
	{
		if ( myIsDisablingGUI )
		{
			restoreGUIEnable();
		}

		myIsDisablingGUI = false;
		myPreviousGUIEnableState = false;

		if ( myLabelStyle == null )
		{
			myLabelStyle			= new GUIStyle( GUI.skin.label );
			myLabelStyle.alignment	= TextAnchor.MiddleRight;
			myLabelStyle.wordWrap	= false;
		}
		
		if ( mySliderStyle == null )
			mySliderStyle = new GUIStyle( GUI.skin.horizontalSlider );
		
		if ( mySliderThumbStyle == null )
			mySliderThumbStyle = new GUIStyle( GUI.skin.horizontalSliderThumb );
	}

	private static bool myIsDisablingGUI;
	private static bool myPreviousGUIEnableState;

	private static GUIStyle myLabelStyle;
	private static GUIStyle mySliderStyle;
	private static GUIStyle mySliderThumbStyle;
}
