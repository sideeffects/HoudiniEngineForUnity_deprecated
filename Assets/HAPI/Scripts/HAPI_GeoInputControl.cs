using UnityEngine;
using System.Collections;

using HAPI;

[ ExecuteInEditMode ]
public class HAPI_GeoInputControl : HAPI_Control 
{
	public bool						prEnableCooking {				get { return myEnableCooking; }
																	set { myEnableCooking = value; } }
	
	public bool						prSyncAssetTransform {			get { return mySyncAssetTransform; } 
																	set { mySyncAssetTransform = value; } }
	
	public bool						prLiveTransformPropagation {	get { return myLiveTransformPropagation; } 
																	set { myLiveTransformPropagation = value; } }

	public int						prInputObjectId {				get { return myInputObjectId; }
																	set { myInputObjectId = value; } }
	
	
	public void Awake()
	{
		myLastLocalToWorld = transform.localToWorldMatrix;
	}
	
	
	public override void reset()
	{
		base.reset();

		// Please keep these in the same order and grouping as their declarations at the top.
		
		prEnableCooking = true;
		prSyncAssetTransform = true;
		prLiveTransformPropagation = false;		
		prInputObjectId = -1;
		
	}
	
	// Use this for initialization
	void Start () 
	{
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		Matrix4x4 local_to_world = transform.localToWorldMatrix;
		
		if ( local_to_world == myLastLocalToWorld )
			return;
						
		myLastLocalToWorld = local_to_world;	
		
		if( prEnableCooking )
		{
			//TODO: detect geometry changes...
			
			if( prSyncAssetTransform )
			{
				HAPI_TransformEuler trans = HAPI_AssetUtility.getHapiTransform( transform.localToWorldMatrix );
				HAPI_Host.setObjectTransform( 0, prInputObjectId, trans );
				
				if( prLiveTransformPropagation )
				{
					prAsset.buildClientSide();
				}
			}
		}
	
	}
	
	
	
	
	[SerializeField] private bool					mySyncAssetTransform;
	[SerializeField] private bool					myLiveTransformPropagation;
	[SerializeField] private bool					myEnableCooking;		
	[SerializeField] private Matrix4x4				myLastLocalToWorld;
	[SerializeField] private int					myInputObjectId;
}
