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
public class TestScript : MonoBehaviour 
{
#if UNITY_EDITOR
	public int value;
	public int[] prArr { get { return arr; } set { arr = value; } }
	[SerializeField] int[] arr;
	
	TestScript()
	{
		//Debug.Log( "TestScript: CONSTRUCTOR(1) - " + value );
		value = 1;
		prArr = null;

		EditorApplication.playmodeStateChanged += playmodeStateChanged;
	}
	
	~TestScript()
	{
		//Debug.Log( "TestScript: DESTRUCTOR - " + value );
		EditorApplication.playmodeStateChanged -= playmodeStateChanged;
	}
	
	public void playmodeStateChanged()
	{
		Debug.Log( "TestScript: playmodeStateChanged - " + EditorApplication.isPlayingOrWillChangePlaymode );
	}

	public void OnApplicationQuit()
	{
		Debug.Log( "TestScript: OnApplicationQuit - " + value );
	}

	public void Reset()
	{
		Debug.Log( "TestScript: Reset - " + value );
	}

	public void Awake()
	{
		Debug.Log( "TestScript: Awake - " + value );
	}
	
	public void Start() 
	{
		Debug.Log( "TestScript: Start - " + value );
	}
	
	public void Update () 
	{
		//Debug.Log( "TestScript: Update - " + value );
	}
	
	public void OnEnable()
	{
		Debug.Log( "TestScript: OnEnable - " + value );
	}
	
	public void OnDisable()
	{
		Debug.Log( "TestScript: OnDisable - " + EditorApplication.isPlayingOrWillChangePlaymode );
	}
	
	public void OnScene()
	{
		//Debug.Log( "TestScript: OnScene - " + value );
	}
	
	public void OnGUI()
	{
		/*
		if (GUI.Button (new Rect (10,10,150,100), "I am a button"))
		{
			Debug.Log ("You clicked the button!");
			value = 5;
		}
		Debug.Log( "TestScript: OnGUI - " + value );
		*/
	}
	
	public void CustomFunc()
	{
		Debug.Log( "TestScript: CustomFunc(3) - " + value );
		value = 3;
	}
#endif // UNITY_EDITOR
}
