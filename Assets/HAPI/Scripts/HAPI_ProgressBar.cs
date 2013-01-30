using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

using HAPI;

public class HAPI_ProgressBar  {
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Properties
	public System.DateTime	prProgressBarStartTime { get; set; }
	public int				prProgressBarCurrent { get; set; }
	public int				prProgressBarTotal { get; set; } 
	public string			prProgressBarMsg { get; set; }
	
		
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Public Methods			
	
	public HAPI_ProgressBar() 
	{
		prProgressBarCurrent		= 0;
		prProgressBarTotal			= 0;
		prProgressBarMsg			= "";
		prProgressBarStartTime		= System.DateTime.Now;
						
		myProgressBarTitle			= "Building Houdini Asset";
		myProgressBarLastValue		= -1;
		myProgressBarLastMsg		= "";
		
	}
	
	
	// PROGRESS BAR -------------------------------------------------------------------------------------------------
	
	public void statusCheckLoop()
	{
		HAPI_State state = HAPI_State.HAPI_STATE_STARTING_LOAD;
		prProgressBarCurrent = 0;
		prProgressBarTotal = 100;
		while ( state != HAPI_State.HAPI_STATE_READY && state != HAPI_State.HAPI_STATE_READY_WITH_ERRORS )
		{
			state = (HAPI_State) HAPI_Host.getStatus( HAPI_StatusType.HAPI_STATUS_STATE );

			if ( state == HAPI_State.HAPI_STATE_COOKING )
			{
				prProgressBarCurrent = HAPI_Host.getCookingCurrentCount();
				prProgressBarTotal = HAPI_Host.getCookingTotalCount();
			}
			else
			{
				prProgressBarCurrent = ( System.DateTime.Now - prProgressBarStartTime ).Seconds;
				prProgressBarTotal = 100;
			}

			prProgressBarMsg = HAPI_Host.getStatusString( HAPI_StatusType.HAPI_STATUS_STATE );
			displayProgressBar();
		}

		if ( state == HAPI_State.HAPI_STATE_READY_WITH_ERRORS )
		{
			state = HAPI_State.HAPI_STATE_READY;
			HAPI_Host.throwRuntimeError();
		}
	}
	
	public void displayProgressBar()
	{
		displayProgressBar( prProgressBarCurrent );
	}

	public void incrementProgressBar()
	{
		incrementProgressBar( 1 );
	}

	public void incrementProgressBar( int increment )
	{
		prProgressBarCurrent += increment;
		displayProgressBar( prProgressBarCurrent );
	}
		
	
	public void clearProgressBar()
	{		
		prProgressBarCurrent = 0;
		EditorUtility.ClearProgressBar();
	}
		

	protected void displayProgressBar( int value )
	{
		// If there are no changes to the progress bar value or message don't re-display it again.
		if ( value == myProgressBarLastValue && prProgressBarMsg == myProgressBarLastMsg )
			return;

		System.DateTime current = System.DateTime.Now;
		System.TimeSpan delta = current - prProgressBarStartTime;
		
		// This delay for displaying the progress bar is so the bar won't flicker for really quick updates
		// (less than a few seconds). Also, when we do show the progress bar the focus of the current 
		// inspector control is lost.
		if ( delta.TotalSeconds < HAPI_Constants.HAPI_SEC_BEFORE_PROGRESS_BAR_SHOW )
		{
			EditorUtility.ClearProgressBar();
			return;
		}
				
		
		prProgressBarCurrent = value;
		string message = "";
		if ( delta.Hours > 0 )
			message = delta.Hours + "h " + delta.Minutes + "m " + delta.Seconds + "s - " + prProgressBarMsg;
		else if ( delta.Minutes > 0 )
			message = delta.Minutes + "m " + delta.Seconds + "s - " + prProgressBarMsg;
		else if ( delta.Seconds > 0 )
			message = delta.Seconds + "s - " + prProgressBarMsg;
		else
			message = prProgressBarMsg;

		bool result = 
			!EditorUtility.DisplayCancelableProgressBar( 
				myProgressBarTitle, message, Mathf.InverseLerp( 0, prProgressBarTotal, prProgressBarCurrent ) );
		
		if ( !result )
		{
			myProgressBarLastValue = -1;
			myProgressBarLastMsg = "";
			HAPI_Host.interrupt();
			throw new HAPI_ErrorProgressCancelled();
		}
		else
		{
			myProgressBarLastValue = value;
			myProgressBarLastMsg = prProgressBarMsg;
		}
	}
	
	
		
	
	protected string			myProgressBarTitle;
	
	
	// Used to reduce the update frequency of the progress bar so it doesn't flicker.
	private int					myProgressBarLastValue;
	private string				myProgressBarLastMsg;
	
	
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Overrides
	
	// Use this for initialization
	
	
	
	/////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// Private Members
	
	
	
}
