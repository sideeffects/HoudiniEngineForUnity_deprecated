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
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR
using System.Runtime.Serialization;
using System.Collections;

[ExecuteInEditMode]
public class ExampleScript : MonoBehaviour 
{
	public int parmIndex;
	public string[] parmNames;

	public bool hasAsset()
	{
		return gameObject.GetComponent< HoudiniAsset >() != null;
	}

	[SerializeField] private HoudiniAssetAccessor myAsset;
	[SerializeField] private string parmName;
	[SerializeField] private int parmSize;
	[SerializeField] private HoudiniAssetAccessor.ParmType parmType;
	[SerializeField] private int[] parmIntValue;
	[SerializeField] private float[] parmFloatValue;
	[SerializeField] private string[] parmStringValue;
	[SerializeField] private float seed;
	
	ExampleScript()
	{
		//Debug.Log( "ExampleScript: CONSTRUCTOR" );
#if UNITY_EDITOR
		EditorApplication.playmodeStateChanged += playmodeStateChanged;
#endif // UNITY_EDITOR
	}
	
	~ExampleScript()
	{
		//Debug.Log( "ExampleScript: DESTRUCTOR" );
#if UNITY_EDITOR
		EditorApplication.playmodeStateChanged -= playmodeStateChanged;
#endif // UNITY_EDITOR
	}
	
	public void playmodeStateChanged()
	{
		//Debug.Log( "ExampleScript: playmodeStateChanged - " + EditorApplication.isPlayingOrWillChangePlaymode );
	}

	public void OnApplicationQuit()
	{
		//Debug.Log( "ExampleScript: OnApplicationQuit" );
	}

	public void Reset()
	{
		//Debug.Log( "ExampleScript: Reset" );
	}

	public void Awake()
	{
		//Debug.Log( "ExampleScript: Awake" );
	}
	
	public void Start() 
	{
		//Debug.Log( "ExampleScript: Start" );

		parmIndex = 0;
		parmNames = null;
		myAsset = null;
		parmName = "";
		parmSize = 0;
		parmType = HoudiniAssetAccessor.ParmType.INVALID;
		parmIntValue = null;
		parmFloatValue = null;
		parmStringValue = null;

		// If the game object has a HAPI_Asset component then get
		// the parameters for this asset and set the selected
		// parameter to be the asset's first parameter
		myAsset = HoudiniAssetAccessor.getAssetAccessor( gameObject );
		if ( myAsset != null )
		{
			Debug.Log( "Asset name: " + myAsset.prName );
			parmNames = myAsset.getParameters();
			setSelectedParameter();
		}
	}
	
	public void Update() 
	{
		//Debug.Log( "ExampleScript: Update" );

		// Example that works with the Sidefx Spaceship and will change the
		// random seed that randomizes the spaceship parameters by using
		// the up and down arrow keys.
		try
		{
			if ( !hasAsset() )
				return;

			if ( Input.GetKey( "up" ) )
			{
				seed += 0.01f;
				myAsset.setParmFloatValue( "seed", 0, seed );
			}

			if ( Input.GetKey( "down" ) )
			{
				seed -= 0.01f;
				myAsset.setParmFloatValue( "seed", 0, seed );
			}
		}
		catch {}
	}
	
	public void OnEnable()
	{
		//Debug.Log( "ExampleScript: OnEnable" );
	}
	
	public void OnDisable()
	{
		//Debug.Log( "ExampleScript: OnDisable" );
	}
	
	public void OnScene()
	{
		//Debug.Log( "ExampleScript: OnScene" );
	}
	
	public void OnGUI()
	{
		//Debug.Log( "ExampleScript: OnGUI" );
	}

	// Set the currently selected parameter and retrieve its values
	public void setSelectedParameter()
	{
		try
		{
			if ( !hasAsset() )
				return;

			parmName = parmNames[ parmIndex ];
			parmSize = myAsset.getParmSize( parmName );
			parmType = myAsset.getParmType( parmName );
			parmIntValue = null;
			parmFloatValue = null;
			parmStringValue = null;

			if ( parmType == HoudiniAssetAccessor.ParmType.INT )
			{
				parmIntValue = new int[ parmSize ];

				for ( int i = 0; i < parmSize; i++ )
				{
					parmIntValue[ i ] = myAsset.getParmIntValue( parmName, i );
				}
			}
			else if ( parmType == HoudiniAssetAccessor.ParmType.FLOAT )
			{
				parmFloatValue = new float[ parmSize ];
				
				for ( int i = 0; i < parmSize; i++ )
				{
					parmFloatValue[ i ] = myAsset.getParmFloatValue( parmName, i );
				}
			}
			else if ( parmType == HoudiniAssetAccessor.ParmType.STRING )
			{
				parmStringValue = new string[ parmSize ];
				
				for ( int i = 0; i < parmSize; i++ )
				{
					parmStringValue[ i ] = myAsset.getParmStringValue( parmName, i );
				}
			}
		}
		catch ( HoudiniError err )
		{
			Debug.LogError( err.ToString() );
		}
	}

	// Set the value of the currently selected parameter
	public void setParameterValue()
	{
		try
		{
			if ( !hasAsset() )
				return;

			for ( int i = 0; i < parmSize; i++ )
			{
				if ( parmType == HoudiniAssetAccessor.ParmType.INT )
					myAsset.setParmIntValue( parmName, i, parmIntValue[ i ] );
				else if ( parmType == HoudiniAssetAccessor.ParmType.FLOAT )
					myAsset.setParmFloatValue( parmName, i, parmFloatValue[ i ] );
				else if ( parmType == HoudiniAssetAccessor.ParmType.STRING )
					myAsset.setParmStringValue( parmName, i, parmStringValue[ i ] );
			}
		}
		catch ( HoudiniError err )
		{
			Debug.LogError( err.ToString() );
		}
	}

	// Set up the UI for the currently selected parameter
	public void getParameterGUI()
	{
		// TODO: Move this to the companion Editor class!

#if UNITY_EDITOR
		try
		{
			if ( !hasAsset() )
				return;

			for ( int i = 0; i < parmSize; i++ )
			{
				if ( parmType == HoudiniAssetAccessor.ParmType.INT )
					parmIntValue[ i ] = EditorGUILayout.IntField( parmIntValue[ i ] );
				else if ( parmType == HoudiniAssetAccessor.ParmType.FLOAT )
					parmFloatValue[ i ] = EditorGUILayout.FloatField( parmFloatValue[ i ] );
				else if ( parmType == HoudiniAssetAccessor.ParmType.STRING )
					parmStringValue[ i ] = EditorGUILayout.TextField( parmStringValue[ i ] );
			}
		}
		catch ( HoudiniError err )
		{
			Debug.LogError( err.ToString() );
		}
#endif // UNITY_EDITOR
	}
}
