using UnityEngine;
using UnityEditor;
using System.Collections;

public struct HAPI_GUIParm
{
	public HAPI_GUIParm( string name )
		: this( name, "", 1 )
	{}

	public HAPI_GUIParm( string name, string label )
		: this( name, label, 1 )
	{}
	
	public HAPI_GUIParm( string name, string label, int size )
	{
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
		
		joinNext 		= false;
		labelNone 		= false;
		
		valuesIndex		= 0;
	}
	
	public HAPI_GUIParm( HAPI.HAPI_ParmInfo info )
	{
		size 			= info.size;
		choiceCount 	= info.choiceCount;
		width			= -1;
		
		name 			= info.name;
		label 			= info.label;
		labelExtraWidth = 0;
		
		hasMin 			= info.hasMin;
		hasMax 			= info.hasMax;
		hasUIMin 		= info.hasUIMin;
		hasUIMax 		= info.hasUIMax;
		
		min 			= info.min;
		max 			= info.max;
		UIMin 			= info.UIMin;
		UIMax 			= info.UIMax;
		
		joinNext 		= info.joinNext;
		labelNone 		= info.labelNone;
		
		if ( info.isInt() )
			valuesIndex = info.intValuesIndex;
		else if ( info.isFloat() )
			valuesIndex = info.floatValuesIndex;
		else if ( info.isString() )
			valuesIndex = info.stringValuesIndex;
		else
			valuesIndex = 0;
	}
	
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
	
	public bool joinNext;
	public bool labelNone;
	
	public int valuesIndex;
}

public class HAPI_GUI : Editor 
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

	public static void label( string value, int width, bool join_next, ref bool join_last )
	{
		initializeConstants();

		// Decide whether to join with the previous parameter on the same line or not.
		if ( !join_last )
			EditorGUILayout.BeginHorizontal();

		EditorGUILayout.SelectableLabel( value, myLabelStyle, GUILayout.Width( width ), myLineHeightGUI );

		join_last = join_next;
		if ( !join_next )
			EditorGUILayout.EndHorizontal();
	}
	
	public static bool dropdown< T >( string name, string label, 
									  ref T value,
									  string[] dropdown_labels,
									  T[] dropdown_values )
	{
		HAPI_GUIParm parm = new HAPI_GUIParm( name, label, 1 );
		T[] values = new T[ 1 ];
		values[ 0 ] = value;
		bool changed = dropdown( ref parm, ref values, dropdown_labels, dropdown_values );
		value = values[ 0 ];
		return changed;
	}
	public static bool dropdown< T >( ref HAPI_GUIParm parm,
									  ref T[] values,
									  string[] dropdown_labels,
									  T[] dropdown_values )
	{
		bool join_last = false; bool no_label_toggle_last = false;
		return dropdown( ref parm, ref values, dropdown_labels, dropdown_values, 
						 ref join_last, ref no_label_toggle_last );
	}
	public static bool dropdown< T >( ref HAPI_GUIParm parm,
									  ref T[] values,
									  string[] dropdown_labels,
									  T[] dropdown_values,
									  ref bool join_last, ref bool no_label_toggle_last )
	{
		initializeConstants();
		
		bool changed = false;
		int parm_size = parm.size;
		
		// Decide whether to join with the previous parameter on the same line or not.
		if ( !join_last || parm_size > 1 )
			EditorGUILayout.BeginHorizontal();
		
		label( ref parm, ref join_last, ref no_label_toggle_last );

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
		if ( old_mapped_value < 0 )
		{
			// Current value not possible!
			EditorGUILayout.TextField( "Dropdown value outside range of possible values!" );
			return false;
		}
		
		// Draw popup.
		int new_mapped_value = 0;
		if ( parm.width >= 0 )
			new_mapped_value = EditorGUILayout.IntPopup( old_mapped_value, dropdown_labels, mapped_values,
														 GUILayout.Width( parm.width ) );
		else
			new_mapped_value = EditorGUILayout.IntPopup( old_mapped_value, dropdown_labels, mapped_values );
		
		T new_value = dropdown_values[ new_mapped_value ];

		// Determine if value changed and update parameter value.
		if ( !new_value.Equals( old_value ) )
		{
			values[ parm.valuesIndex ] = new_value;
			changed |= true;
		}
		
		// Decide whether to join with the next parameter on the same line or not
		// but also save our status for the next parameter.
		join_last = ( parm.joinNext && parm_size <= 1 );
		if ( !parm.joinNext || parm_size > 1 )
			EditorGUILayout.EndHorizontal();
		
		return changed;
	}
	
	public static bool intField( ref HAPI_GUIParm parm,
								 ref bool delay_build,
								 ref int[] values )
	{
		bool join_last = false; bool no_label_toggle_last = false;
		return intField( ref parm, ref delay_build, ref values, ref join_last, ref no_label_toggle_last );
	}
	public static bool intField( ref HAPI_GUIParm parm,
								 ref bool delay_build,
								 ref int[] values,
								 ref bool join_last, ref bool no_label_toggle_last )
	{
		initializeConstants();
		
		bool changed = false;
		int parm_size = parm.size;
		
		// Decide whether to join with the previous parameter on the same line or not.
		if ( !join_last || parm_size > 1 )
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
				EditorGUILayout.LabelField( "", myLabelWidthGUI );
				per_line = 0;
			}
			
			// Get old value.
			int old_value = values[ parm.valuesIndex + p ];
			
			// Draw field.
			int new_value = EditorGUILayout.IntField( old_value );
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
				changed |= true;
			} // if
		} // for
		
		// Decide whether to join with the next parameter on the same line or not
		// but also save our status for the next parameter.
		join_last = ( parm.joinNext && parm_size <= 1 );
		if ( !parm.joinNext || parm_size > 1 )
			EditorGUILayout.EndHorizontal();
		
		return changed;
	}

	public static bool floatField( string name, string label, ref float value )
	{
		HAPI_GUIParm gui_parm = new HAPI_GUIParm( name, label );
		float[] values = new float[ 1 ];
		values[ 0 ] = value;
		bool delay_build = false;
		bool changed = floatField( ref gui_parm, ref delay_build, ref values );
		value = values[ 0 ];
		return changed;
	}
	public static bool floatField( ref HAPI_GUIParm parm,
								   ref bool delay_build,
								   ref float[] values )
	{
		bool join_last = false; bool no_label_toggle_last = false;
		return floatField( ref parm, ref delay_build, ref values, ref join_last, ref no_label_toggle_last );
	}
	public static bool floatField( ref HAPI_GUIParm parm,
								   ref bool delay_build,
								   ref float[] values,
								   ref bool join_last, ref bool no_label_toggle_last )
	{
		initializeConstants();
		
		bool changed = false;
		int parm_size = parm.size;
		
		// Decide whether to join with the previous parameter on the same line or not.
		if ( !join_last || parm_size > 1 )
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
				EditorGUILayout.LabelField( "", myLabelWidthGUI );
				per_line = 0;
			}
			
			// Get old value.
			float old_value = values[ parm.valuesIndex + p ];
			
			// Draw field.
			float new_value = EditorGUILayout.FloatField( old_value );
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
				new_value = GUI.HorizontalSlider( lastDoubleRect, new_value, ui_min, ui_max, 
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
				changed |= true;
			} // if
		} // for
		
		// Decide whether to join with the next parameter on the same line or not
		// but also save our status for the next parameter.
		join_last = ( parm.joinNext && parm_size <= 1 );
		if ( !parm.joinNext || parm_size > 1 )
			EditorGUILayout.EndHorizontal();
		
		return changed;
	}
	
	public static bool stringField( string name, string label, ref string value )
	{
		HAPI_GUIParm gui_parm = new HAPI_GUIParm( name, label );
		string[] values = new string[ 1 ];
		values[ 0 ] = value;
		bool delay_build = false;
		bool changed = stringField( ref gui_parm, ref delay_build, ref values );
		value = values[ 0 ];
		return changed;
	}
	public static bool stringField( ref HAPI_GUIParm parm,
									ref bool delay_build,
									ref string[] values )
	{
		bool join_last = false; bool no_label_toggle_last = false;
		return stringField( ref parm, ref delay_build, ref values, ref join_last, ref no_label_toggle_last );
	}
	public static bool stringField( ref HAPI_GUIParm parm,
									ref bool delay_build,
									ref string[] values,
									ref bool join_last, ref bool no_label_toggle_last )
	{
		initializeConstants();
		
		bool changed = false;
		int parm_size = parm.size;
		
		// Decide whether to join with the previous parameter on the same line or not.
		if ( !join_last || parm_size > 1 )
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
				EditorGUILayout.LabelField( "", myLabelWidthGUI );
				per_line = 0;
			}
			
			// Get old value.
			string old_value = values[ parm.valuesIndex + p ];
			
			// Draw field.
			string new_value = EditorGUILayout.TextField( old_value );
			if ( new_value != old_value ) // Check if the field is being used instead of the slider.
				delay_build = true;
			
			// Determine if value changed and update parameter value. 
			if ( new_value != old_value )
			{
				values[ parm.valuesIndex + p ] = new_value;
				changed |= true;
			}
		}
		
		// Decide whether to join with the next parameter on the same line or not
		// but also save our status for the next parameter.
		join_last = ( parm.joinNext && parm_size <= 1 );
		if ( !parm.joinNext || parm_size > 1 )
			EditorGUILayout.EndHorizontal();
		
		return changed;
	}
	
	public static bool objectField( string name, string label, ref Object obj, System.Type type )
	{
		HAPI_GUIParm gui_parm = new HAPI_GUIParm( name, label );
		return objectField( ref gui_parm, ref obj, type );
	}
	public static bool objectField( ref HAPI_GUIParm parm,
									ref Object obj, System.Type type )
	{
		bool join_last = false; bool no_label_toggle_last = false;
		return objectField( ref parm, ref obj, type, ref join_last, ref no_label_toggle_last );
	}
	public static bool objectField( ref HAPI_GUIParm parm,
									ref Object obj, System.Type type,
									ref bool join_last, ref bool no_label_toggle_last )
	{
		initializeConstants();
		
		bool changed = false;
		int parm_size = parm.size;
		
		// Decide whether to join with the previous parameter on the same line or not.
		if ( !join_last || parm_size > 1 )
			EditorGUILayout.BeginHorizontal();
		
		label( ref parm, ref join_last, ref no_label_toggle_last );
		
		Object old_obj = obj;
		Object new_obj = EditorGUILayout.ObjectField( old_obj, type, true );
		
		if ( new_obj != old_obj )
		{
			obj = new_obj;
			changed |= true;
		}
		
		// Decide whether to join with the next parameter on the same line or not
		// but also save our status for the next parameter.
		join_last = ( parm.joinNext && parm_size <= 1 );
		if ( !parm.joinNext || parm_size > 1 )
			EditorGUILayout.EndHorizontal();
		
		return changed;
	}
	
	public static bool fileField( string name, string label,
								  ref bool delay_build,
								  ref string path )
	{
		HAPI_GUIParm parm = new HAPI_GUIParm( name, label );
		return fileField( ref parm, ref delay_build, ref path );
	}
	public static bool fileField( ref HAPI_GUIParm parm,
								  ref bool delay_build,
								  ref string path )
	{
		bool join_last = false; bool no_label_toggle_last = false;
		return fileField( ref parm, ref delay_build, ref path, ref join_last, ref no_label_toggle_last );
	}
	public static bool fileField( ref HAPI_GUIParm parm,
								  ref bool delay_build,
								  ref string path,
								  ref bool join_last, ref bool no_label_toggle_last )
	{
		initializeConstants();
		
		bool changed = false;
		int parm_size = parm.size;
		
		// Decide whether to join with the previous parameter on the same line or not.
		if ( !join_last || parm_size > 1 )
			EditorGUILayout.BeginHorizontal();
		
		label( ref parm, ref join_last, ref no_label_toggle_last );
		
		string old_path = path;
		string new_path = EditorGUILayout.TextField( old_path );
		if ( new_path != old_path ) // Check if the field is being used instead of the slider.
			delay_build = true;
		
		if ( GUILayout.Button( "...", GUILayout.Width( myFileChooserButtonWidth ), myLineHeightGUI ) ) 
		{
			string prompt_path = EditorUtility.OpenFilePanel( "Select File", old_path, "*" );;
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
		
		return changed;
	}
	
	public static bool toggle( string name, string label, ref bool value )
	{
		HAPI_GUIParm gui_parm = new HAPI_GUIParm( name, label );
		int[] values = new int[ 1 ];
		values[ 0 ] = ( value ? 1 : 0 );
		bool result = toggle( ref gui_parm, ref values );
		value = ( values[ 0 ] == 1 ?  true : false );
		return result;
	}
	public static bool toggle( ref HAPI_GUIParm parm,
							   ref int[] values )
	{
		bool join_last = false; bool no_label_toggle_last = false;
		return toggle( ref parm, ref values, ref join_last, ref no_label_toggle_last );
	}
	public static bool toggle( ref HAPI_GUIParm parm,
							   ref int[] values,
							   ref bool join_last, ref bool no_label_toggle_last )
	{
		initializeConstants();
		
		bool changed = false;
		int parm_size = parm.size;
		
		// Decide whether to join with the previous parameter on the same line or not.
		if ( !join_last || parm_size > 1 )
			EditorGUILayout.BeginHorizontal();
		
		if ( !parm.joinNext )
		{
			// Add padding for the toggle column.
			EditorGUILayout.LabelField( myNullContent, myToggleWidthGUI );
			// Add empty space to align with fields.
			EditorGUILayout.LabelField( myNullContent, myLabelWidthGUI );
		}
		
		// Get old value.
		int old_value = values[ parm.valuesIndex ];
		
		// Draw toggle with its label.
		bool toggle_result = EditorGUILayout.Toggle( old_value != 0, myToggleWidthGUI );
		int new_value = ( toggle_result ? 1 : 0 );
		if ( !parm.labelNone )
			EditorGUILayout.SelectableLabel( parm.label, myLineHeightGUI );
		else
			no_label_toggle_last = true;
		
		// Determine if value changed and update parameter value.
		if ( new_value != old_value )
		{
			values[ parm.valuesIndex ] = new_value;
			changed |= true;
		}
		
		// Decide whether to join with the next parameter on the same line or not
		// but also save our status for the next parameter.
		join_last = ( parm.joinNext && parm_size <= 1 );
		if ( !parm.joinNext || parm_size > 1 )
			EditorGUILayout.EndHorizontal();
		
		return changed;
	}
	
	public static bool colourField( string name, string label, ref Color value )
	{
		HAPI_GUIParm gui_parm = new HAPI_GUIParm( name, label );
		gui_parm.size = 4;
		bool delay_build = false;
		float[] values = new float[ 4 ];
		values[ 0 ] = value.r;
		values[ 1 ] = value.g;
		values[ 2 ] = value.b;
		values[ 3 ] = value.a;
		bool result = colourField( ref gui_parm, ref delay_build, ref values );
		value.r = values[ 0 ];
		value.g = values[ 1 ];
		value.b = values[ 2 ];
		value.a = values[ 3 ];
		return result;
	}
	public static bool colourField( ref HAPI_GUIParm parm,
									ref bool delay_build,
									ref float[] values )
	{
		bool join_last = false; bool no_label_toggle_last = false;
		return colourField( ref parm, ref delay_build, ref values, ref join_last, ref no_label_toggle_last );
	}
	public static bool colourField( ref HAPI_GUIParm parm,
									ref bool delay_build,
									ref float[] values,
									ref bool join_last, ref bool no_label_toggle_last )
	{
		initializeConstants();
		
		bool changed = false;
		int parm_size = parm.size;
		
		// Decide whether to join with the previous parameter on the same line or not.
		if ( !join_last || parm_size > 1 )
			EditorGUILayout.BeginHorizontal();
		
		label( ref parm, ref join_last, ref no_label_toggle_last );
		
		Color old_color = new Color( values[ parm.valuesIndex + 0 ], 
								 	 values[ parm.valuesIndex + 1 ], 
								 	 values[ parm.valuesIndex + 2 ] );
		if ( parm_size > 3 )
			old_color.a = values[ parm.valuesIndex + 3 ];
		
		// Draw control.
		Color new_color = EditorGUILayout.ColorField( old_color );

		// Determine if value changed and update parameter value.
		if ( new_color != old_color )
		{
			values[ parm.valuesIndex + 0 ] = new_color.r;
			values[ parm.valuesIndex + 1 ] = new_color.g;
			values[ parm.valuesIndex + 2 ] = new_color.b;
			
			if ( parm_size > 3 )
				values[ parm.valuesIndex + 3 ] = new_color.a;
		
			changed |= true;
		}
		
		// Decide whether to join with the next parameter on the same line or not
		// but also save our status for the next parameter.
		join_last = ( parm.joinNext && parm_size <= 1 );
		if ( !parm.joinNext || parm_size > 1 )
			EditorGUILayout.EndHorizontal();
		
		return changed;
	}
	
	public static bool button( string name, string label )
	{
		HAPI_GUIParm gui_parm = new HAPI_GUIParm( name, label );
		bool result = button( ref gui_parm );
		return result;
	}
	public static bool button( ref HAPI_GUIParm parm )
	{
		bool join_last = false; bool no_label_toggle_last = false;
		return button( ref parm, ref join_last, ref no_label_toggle_last );
	}
	public static bool button( ref HAPI_GUIParm parm,
							   ref bool join_last, ref bool no_label_toggle_last )
	{
		initializeConstants();
		
		bool changed = false;
		int parm_size = parm.size;
		
		// Decide whether to join with the previous parameter on the same line or not.
		if ( !join_last || parm_size > 1 )
			EditorGUILayout.BeginHorizontal();
		
		if ( !parm.joinNext )
		{
			// Add padding for the toggle column.
			EditorGUILayout.LabelField( myNullContent, myToggleWidthGUI );
			// Add empty space to align with fields.
			EditorGUILayout.LabelField( myNullContent, myLabelWidthGUI );
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
		
		return changed;
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
	public const float myDummyLabelMinWidth			= 20;
	
	public const float myDefaultUIMin 				= 0.0f;
	public const float myDefaultUIMax 				= 10.0f;
	
	public static GUILayoutOption 	myLineHeightGUI 			= GUILayout.Height( myLineHeight );
	public static GUILayoutOption 	myLabelWidthGUI 			= GUILayout.Width( myLabelWidth );
	public static GUILayoutOption 	myToggleWidthGUI 			= GUILayout.Width( myToggleWidth );
	public static GUILayoutOption	myDummyLabelMinWidthGUI		= GUILayout.MinWidth( myDummyLabelMinWidth );
	
	public static GUIContent 		myNullContent 				= new GUIContent( "" );
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private
	
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
	
	private static void label( ref HAPI_GUIParm parm,
							   ref bool join_last, ref bool no_label_toggle_last )
	{
		if ( !parm.labelNone )
		{
			float label_final_width = myLabelWidth + (float) parm.labelExtraWidth;
			if ( join_last && !no_label_toggle_last )
			{
				float min_width;
				float max_width;
				myLabelStyle.CalcMinMaxWidth( new GUIContent( parm.label ), out min_width, out max_width );
				label_final_width = min_width + (float) parm.labelExtraWidth;
			}
			else if ( !join_last )
			{
				// Add padding for the toggle column.
				EditorGUILayout.LabelField( "", myToggleWidthGUI );
			}

			GUILayoutOption label_final_width_gui = GUILayout.Width( label_final_width );

			EditorGUILayout.SelectableLabel( parm.label, myLabelStyle, label_final_width_gui, myLineHeightGUI );
			no_label_toggle_last = false;
		}
	}
	
	private static void initializeConstants()
	{
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
	
	private static GUIStyle			myLabelStyle;
	private static GUIStyle			mySliderStyle;
	private static GUIStyle			mySliderThumbStyle;
}
