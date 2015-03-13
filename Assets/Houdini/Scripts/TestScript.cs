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
