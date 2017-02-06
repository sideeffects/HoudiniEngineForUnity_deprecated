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
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR
using System.Runtime.Serialization;
using System.Collections;

[ExecuteInEditMode]
public class HoudiniApiAssetAccessorExample : MonoBehaviour 
{
	public int parmIndex;
	public string[] parmNames;

	public bool hasAsset()
	{
		return gameObject.GetComponent< HoudiniAsset >() != null;
	}

	[SerializeField] private HoudiniApiAssetAccessor myAsset;
	[SerializeField] private string parmName;
	[SerializeField] private int parmSize;
	[SerializeField] private HoudiniApiAssetAccessor.ParmType parmType;
	[SerializeField] private int[] parmIntValue;
	[SerializeField] private float[] parmFloatValue;
	[SerializeField] private string[] parmStringValue;
	[SerializeField] private float seed;
	
	HoudiniApiAssetAccessorExample()
	{
		//Debug.Log( "HoudiniApiAssetAccessorExample: CONSTRUCTOR" );
#if UNITY_EDITOR
		EditorApplication.playmodeStateChanged += playmodeStateChanged;
#endif // UNITY_EDITOR
	}
	
	~HoudiniApiAssetAccessorExample()
	{
		//Debug.Log( "HoudiniApiAssetAccessorExample: DESTRUCTOR" );
#if UNITY_EDITOR
		EditorApplication.playmodeStateChanged -= playmodeStateChanged;
#endif // UNITY_EDITOR
	}
	
	public void playmodeStateChanged()
	{
		//Debug.Log( "HoudiniApiAssetAccessorExample: playmodeStateChanged - " + EditorApplication.isPlayingOrWillChangePlaymode );
	}

	public void OnApplicationQuit()
	{
		//Debug.Log( "HoudiniApiAssetAccessorExample: OnApplicationQuit" );
	}

	public void Reset()
	{
		//Debug.Log( "HoudiniApiAssetAccessorExample: Reset" );
	}

	public void Awake()
	{
		//Debug.Log( "HoudiniApiAssetAccessorExample: Awake" );
	}
	
	public void Start() 
	{
		//Debug.Log( "HoudiniApiAssetAccessorExample: Start" );

		parmIndex = 0;
		parmNames = null;
		myAsset = null;
		parmName = "";
		parmSize = 0;
		parmType = HoudiniApiAssetAccessor.ParmType.INVALID;
		parmIntValue = null;
		parmFloatValue = null;
		parmStringValue = null;

		// If the game object has a HAPI_Asset component then get
		// the parameters for this asset and set the selected
		// parameter to be the asset's first parameter
		myAsset = HoudiniApiAssetAccessor.getAssetAccessor( gameObject );
		if ( myAsset != null )
		{
			Debug.Log( "Asset name: " + myAsset.prName );
			parmNames = myAsset.getParameters();
			setSelectedParameter();
		}
	}
	
	public void Update() 
	{
		//Debug.Log( "HoudiniApiAssetAccessorExample: Update" );

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
		//Debug.Log( "HoudiniApiAssetAccessorExample: OnEnable" );
	}
	
	public void OnDisable()
	{
		//Debug.Log( "HoudiniApiAssetAccessorExample: OnDisable" );
	}
	
	public void OnScene()
	{
		//Debug.Log( "HoudiniApiAssetAccessorExample: OnScene" );
	}
	
	public void OnGUI()
	{
		//Debug.Log( "HoudiniApiAssetAccessorExample: OnGUI" );
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

			if ( parmType == HoudiniApiAssetAccessor.ParmType.INT )
			{
				parmIntValue = new int[ parmSize ];

				for ( int i = 0; i < parmSize; i++ )
				{
					parmIntValue[ i ] = myAsset.getParmIntValue( parmName, i );
				}
			}
			else if ( parmType == HoudiniApiAssetAccessor.ParmType.FLOAT )
			{
				parmFloatValue = new float[ parmSize ];
				
				for ( int i = 0; i < parmSize; i++ )
				{
					parmFloatValue[ i ] = myAsset.getParmFloatValue( parmName, i );
				}
			}
			else if ( parmType == HoudiniApiAssetAccessor.ParmType.STRING )
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
				if ( parmType == HoudiniApiAssetAccessor.ParmType.INT )
					myAsset.setParmIntValue( parmName, i, parmIntValue[ i ] );
				else if ( parmType == HoudiniApiAssetAccessor.ParmType.FLOAT )
					myAsset.setParmFloatValue( parmName, i, parmFloatValue[ i ] );
				else if ( parmType == HoudiniApiAssetAccessor.ParmType.STRING )
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
				if ( parmType == HoudiniApiAssetAccessor.ParmType.INT )
					parmIntValue[ i ] = EditorGUILayout.IntField( parmIntValue[ i ] );
				else if ( parmType == HoudiniApiAssetAccessor.ParmType.FLOAT )
					parmFloatValue[ i ] = EditorGUILayout.FloatField( parmFloatValue[ i ] );
				else if ( parmType == HoudiniApiAssetAccessor.ParmType.STRING )
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
