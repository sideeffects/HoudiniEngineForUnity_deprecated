using UnityEngine;
using System.Collections;

[ ExecuteInEditMode ]
public class HAPI_GeoInputControl : HAPI_Control 
{
	public bool						prEnableCooking {				get { return myEnableCooking; }
																	set { myEnableCooking = value; } }
	
	public bool						prSyncAssetTransform {			get { return mySyncAssetTransform; } 
																	set { mySyncAssetTransform = value; } }
	
	public bool						prLiveTransformPropagation {	get { return myLiveTransformPropagation; } 
																	set { myLiveTransformPropagation = value; } }
	
	
	public override void reset()
	{
		base.reset();

		// Please keep these in the same order and grouping as their declarations at the top.
		
		prEnableCooking = true;
		prSyncAssetTransform = true;
		prLiveTransformPropagation = true;
		
	}
	
	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}
	
	
	
	
	[SerializeField] private bool					mySyncAssetTransform;
	[SerializeField] private bool					myLiveTransformPropagation;
	[SerializeField] private bool					myEnableCooking;	
}
