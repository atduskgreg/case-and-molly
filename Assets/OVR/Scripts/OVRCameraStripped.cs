/************************************************************************************

Filename    :   OVRCameraStripped.cs
Content     :   Interface to camera class
Created     :   August 5th, 2013
Authors     :   Peter Giokaris

Copyright   :   Copyright 2013 Oculus VR, Inc. All Rights reserved.

Use of this software is subject to the terms of the Oculus LLC license
agreement provided at the time of installation or download, or which
otherwise accompanies this software in either electronic or hard copy form.

************************************************************************************/
using UnityEngine;
using System.Runtime.InteropServices;

[RequireComponent(typeof(Camera))]

//-------------------------------------------------------------------------------------
// ***** OVRCameraStripped
//
public class OVRCameraStripped : OVRComponent
{	
	private RenderTexture	CameraTexture	  	= null;
 	private float			CameraTextureScale 	= 1.0f;

	// Start
	new void Start()
	{
		base.Start ();

		// If CameraTextureScale is not 1.0f, create a new texture and assign to target texture
		// Otherwise, fall back to normal camera rendering
		if((CameraTexture == null) && (CameraTextureScale != 1.0f))
		{
			int w = (int)(Screen.width / 2.0f * CameraTextureScale);
			int h = (int)(Screen.height * CameraTextureScale);
			CameraTexture = new RenderTexture(  w, h, 24);
			
			// Use MSAA settings in QualitySettings for new RenderTexture
			CameraTexture.antiAliasing = QualitySettings.antiAliasing;
		}
	}
	
	// OnPreRender
	void OnPreRender()
	{
		// Set new buffers and clear color and depth
		if(CameraTexture != null)
		{
			//CameraTexture.DiscardContents();
			Graphics.SetRenderTarget(CameraTexture);
			//GL.Clear (true, true, gameObject.camera.backgroundColor);
		}
	}
	
	// OnRenderImage
	void  OnRenderImage (RenderTexture source, RenderTexture destination)
	{			
		// Use either source input or CameraTexutre, if it exists
		RenderTexture SourceTexture = source;
	
		if (CameraTexture != null)
			SourceTexture = CameraTexture;
		
		Graphics.Blit(SourceTexture, destination);			
	}	
}
