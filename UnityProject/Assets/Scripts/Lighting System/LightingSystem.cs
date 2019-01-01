﻿using System;
using UnityEngine;

/// <summary>
/// Lighting System manager. 
/// Orchestrates Mask Renderers and Post Processor to apply lighting to the game scene.
/// </summary>
[RequireComponent(typeof(Camera))]
public class LightingSystem : MonoBehaviour
{
	/// <summary>
	/// FoV Projector position offset in local space.
	/// Assumption that it may be changed during game. Otherwise it should be moved in to rendering settings.
	/// </summary>
	public Vector3 fovCenterOffset;
	public float fovDistance;
	public RenderSettings renderSettings;
	public MaterialContainer materialContainer;

	private static Func<Vector3, Vector3, Vector2, Vector2> HandlePPPositionRequest;

	private Camera mMainCamera;
	private OcclusionMaskRenderer mOcclusionRenderer;
	private LightMaskRenderer mLightMaskRenderer;
	private BackgroundRenderer mBackgroundRenderer;
	private PostProcessingStack mPostProcessingStack;
	private PixelPerfectRT mGlobalOcclusionMask;
	private PixelPerfectRT mFloorOcclusionMask;
	private PixelPerfectRT mWallFloorOcclusionMask;
	private PixelPerfectRT mObstacleLightMask;
	private PixelPerfectRT mOcclusionPPRT;
	private PixelPerfectRT mlightPPRT;
	private bool mDoubleFrameRendererSwitch;	
	private bool mMatrixRotationMode;
	private float mMatrixRotationModeBlend;

	public bool matrixRotationMode
	{
		get
		{
			return mMatrixRotationMode;
		}

		set
		{
			if (mMatrixRotationMode == value)
				return;

			mMatrixRotationMode = value;
		}
	}

	/// <summary>
	/// Holds the occlusion mask used by object sprites (such as wallmount and objects on the floor) to make them invisible when they are out of view. Not used for any other purpose.
	/// Generated from wallFloorOcclusionMask
	/// </summary>
	private PixelPerfectRT objectOcclusionMask
	{
		get
		{
			return mGlobalOcclusionMask;
		}

		set
		{
			if (mGlobalOcclusionMask == value)
				return;

			if (mGlobalOcclusionMask != null)
			{
				mGlobalOcclusionMask.Release();
			}

			mGlobalOcclusionMask = value;

			Shader.SetGlobalTexture("_ObjectFovMask", value.renderTexture);
		}
	}

	/// <summary>
	/// Holds the occlusion mask which only masks out the floor based on the position of walls / the player. This ignores occluded walls and is used
	/// for the various lighting shaders (which look best when we use this rather than wallFloorOcclusionMask because that's what the lighting system
	/// was designed to use).
	/// </summary>
	private PixelPerfectRT floorOcclusionMask
	{
		get
		{
			return mFloorOcclusionMask;
		}

		set
		{
			if (mFloorOcclusionMask == value)
				return;

			if (mFloorOcclusionMask != null)
			{
				mFloorOcclusionMask.Release();
			}

			mFloorOcclusionMask = value;
		}
	}

	/// <summary>
	/// Holds the occlusion mask which occludes walls and floors, only used to generate objectOcclusionMask so that wallmounts
	/// are occluded along with objects on the floor which are out of view.
	/// </summary>
	private PixelPerfectRT wallFloorOcclusionMask
	{
		get
		{
			return mWallFloorOcclusionMask;
		}

		set
		{
			if (mWallFloorOcclusionMask == value)
				return;

			if (mWallFloorOcclusionMask != null)
			{
				mWallFloorOcclusionMask.Release();
			}

			mWallFloorOcclusionMask = value;
		}
	}

	private PixelPerfectRT obstacleLightMask
	{
		get
		{
			return mObstacleLightMask;
		}

		set
		{
			if (mObstacleLightMask == value)
				return;

			if (mObstacleLightMask != null)
			{
				mObstacleLightMask.Release();
			}

			mObstacleLightMask = value;
		}
	}

	private OperationParameters operationParameters { get; set; }

	public static Vector2 GetPixelPerfectPosition(Vector3 iPosition, Vector3 iPreviousPosition, Vector2 iPreviousFilteredPosition)
	{
		if (HandlePPPositionRequest == null)
		{
			return iPosition;
		}

		return HandlePPPositionRequest(iPosition, iPreviousPosition, iPreviousFilteredPosition);
	}

	private static void ValidateMainCamera(Camera iMainCamera, RenderSettings iRenderSettings)
	{
		if (iMainCamera.backgroundColor.a > 0)
		{
			UnityEngine.Debug.Log("FovSystem Camera Validation: Camera backgroundColor.a must be 0. This is required to create background mask. Adjusted...");

			iMainCamera.backgroundColor = new Color(iMainCamera.backgroundColor.r, iMainCamera.backgroundColor.g, iMainCamera.backgroundColor.b, 0);
		}

		if (((LayerMask)iMainCamera.cullingMask).HasAny(iRenderSettings.lightSourceLayers))
		{
			UnityEngine.Debug.Log("FovSystem Camera Validation: Camera does not cull one of Light Source Layers! Light System may not work currently.");
		}

		if (((LayerMask)iMainCamera.cullingMask).HasAny(iRenderSettings.backgroundLayers))
		{
			UnityEngine.Debug.Log("FovSystem Camera Validation: Camera does not cull one of Background Layers! Light System wound be able to mask background and would not work correctly.");
		}
	}

	private void OnEnable()
	{
		HandlePPPositionRequest += ProviderPPPosition;

		// Initialize members.
		mMainCamera = gameObject.GetComponent<Camera>();

		if (mMainCamera == null)
			throw new Exception("FovSystemManager require Camera component to operate.");

		ValidateMainCamera(mMainCamera, renderSettings);

		if (mOcclusionRenderer == null)
		{
			mOcclusionRenderer = OcclusionMaskRenderer.InitializeMaskRenderer(gameObject, renderSettings.occlusionLayers, materialContainer.OcclusionMaskShader);
		}

		if (mLightMaskRenderer == null)
		{
			mLightMaskRenderer = LightMaskRenderer.InitializeMaskRenderer(gameObject);
		}

		if (mBackgroundRenderer == null)
		{
			mBackgroundRenderer = BackgroundRenderer.InitializeMaskRenderer(gameObject);
		}

		if (mPostProcessingStack == null)
		{
			mPostProcessingStack = new PostProcessingStack(materialContainer);
		}
	}

	private Vector2 ProviderPPPosition(Vector3 iPosition, Vector3 iPreviousPosition, Vector2 iPreviousFilteredPosition)
	{
		return operationParameters.occlusionPPRTParameter.GetFilteredRendererPosition(iPosition, iPreviousPosition, iPreviousFilteredPosition);
	}

	private void OnDisable()
	{
		// Set object occlusion white, so occlusion dependent shaders will show appropriately while system is off.
		Shader.SetGlobalTexture("_ObjectFovMask", Texture2D.whiteTexture);

		// Default parameters to force parameters update on enable.
		operationParameters = default(OperationParameters);

		HandlePPPositionRequest -= ProviderPPPosition;
	}

	private void Update()
	{
		// Monitor state to detect when we should trigger reinitialization of rendering textures.
		var _newParameters = new OperationParameters(mMainCamera, renderSettings, matrixRotationMode);

		bool _shouldReinitializeTextures = _newParameters != operationParameters;

		if (_shouldReinitializeTextures)
		{
			operationParameters = _newParameters;

			ResolveRenderingTextures(operationParameters);
		}

		// Blend switch for matrix rotation effects.
		// Used to smooth effects in and out.
		if (mMatrixRotationMode == true)
		{
			mMatrixRotationModeBlend = Mathf.MoveTowards(mMatrixRotationModeBlend, 1, Time.unscaledDeltaTime * 5);
		}
		else
		{
			mMatrixRotationModeBlend = 0;
		}
	}	

	private void ResolveRenderingTextures(OperationParameters iParameters)
	{
		// Prepare render textures.
		floorOcclusionMask = new PixelPerfectRT(operationParameters.fovPPRTParameter);
		wallFloorOcclusionMask = new PixelPerfectRT(operationParameters.fovPPRTParameter);

		objectOcclusionMask = new PixelPerfectRT(operationParameters.lightPPRTParameter);

		obstacleLightMask = new PixelPerfectRT(operationParameters.obstacleLightPPRTParameter);

		// Let members handle their own textures.
		// Possibly move to container?
		mPostProcessingStack.ResetRenderingTextures(iParameters);
		mBackgroundRenderer.ResetRenderingTextures(iParameters);
	}

	private void OnPreRender()
	{
		if (renderSettings.doubleFrameRenderingMode && mDoubleFrameRendererSwitch == false)
		{
			Shader.SetGlobalVector("_ObjectFovMaskTransformation", objectOcclusionMask.GetTransformation(mMainCamera));
			return;
		}

		using (new DisposableProfiler("1. Occlusion Mask Render (No Gfx Time)"))
		{
			mOcclusionPPRT = mOcclusionRenderer.Render(mMainCamera, operationParameters.occlusionPPRTParameter, matrixRotationMode);

			if (mMatrixRotationModeBlend > 0.001f)
			{
				mPostProcessingStack.BlurOcclusionMaskRotation(mOcclusionPPRT.renderTexture, renderSettings, operationParameters.cameraOrthographicSize, mMatrixRotationModeBlend);
			}
			
		}

		using (new DisposableProfiler("2. Generate FoV"))
		{
			if (wallFloorOcclusionMask == null)
			{
				floorOcclusionMask = new PixelPerfectRT(operationParameters.fovPPRTParameter);
				wallFloorOcclusionMask = new PixelPerfectRT(operationParameters.fovPPRTParameter);
			}
			else
			{
				floorOcclusionMask.Update(operationParameters.fovPPRTParameter);
				wallFloorOcclusionMask.Update(operationParameters.fovPPRTParameter);
			}

			// This step will result in two masks: floorOcclusionMask used later in light mixing, and floorWallOcclusionMask which is only used for calculating
			// objects / wallmounts sprite visibility.
			Vector3 _fovCenterInWorldSpace = transform.TransformPoint(fovCenterOffset);
			Vector3 _fovCenterOffsetInViewSpace = mMainCamera.WorldToViewportPoint(_fovCenterInWorldSpace) - new Vector3(0.5f, 0.5f, 0);
			Vector3 _fovCenterOffsetInExtendedViewSpace = _fovCenterOffsetInViewSpace * (float)operationParameters.cameraOrthographicSize / mOcclusionPPRT.orthographicSize;
			
			mPostProcessingStack.GenerateFovMask(mOcclusionPPRT, floorOcclusionMask, wallFloorOcclusionMask, renderSettings, _fovCenterOffsetInExtendedViewSpace, fovDistance, operationParameters);
		}

		using (new DisposableProfiler("3. Object Occlusion Mask"))
		{
			// These step calculates the objectOcclusionMask using the wallFloorOcclusionMask, so that objects / wallmounts will be hidden
			// if they are not in view
			objectOcclusionMask.Update(operationParameters.lightPPRTParameter);
			PixelPerfectRT.Transform(wallFloorOcclusionMask, objectOcclusionMask, materialContainer.PPRTTransformMaterial);

			// Update shader global transformation for occlusionMaskExtended so sprites can transform mask correctly.
			Shader.SetGlobalVector("_ObjectFovMaskTransformation", objectOcclusionMask.GetTransformation(mMainCamera));
		}

		using (new DisposableProfiler("4. Blur Object Occlusion Mask"))
		{
			// Note: This blur is used only with shaders during scene render, so 1 pass should be enough.
			mPostProcessingStack.BlurOcclusionMask(objectOcclusionMask.renderTexture, renderSettings, operationParameters.cameraOrthographicSize);

			objectOcclusionMask.renderTexture.filterMode = FilterMode.Point;
		}

		// Note: After execution of this method, MainCamera.Render will be executed and scene will be drawn.
	}

	private void OnRenderImage(RenderTexture iSource, RenderTexture iDestination)
	{
		if (renderSettings.doubleFrameRenderingMode)
		{
			if (mDoubleFrameRendererSwitch)
			{
				var _blitMaterial = materialContainer.blitMaterial;
				_blitMaterial.SetVector("_LightTransform", mlightPPRT.GetTransformation(mMainCamera));
				_blitMaterial.SetVector("_OcclusionTransform", floorOcclusionMask.GetTransformation(mMainCamera));

				Graphics.Blit(iSource, iDestination, _blitMaterial);

				mDoubleFrameRendererSwitch = false;
				return;
			}

			mDoubleFrameRendererSwitch = true;
		}

		if (materialContainer.blitMaterial == null)
		{
			Debug.Log($"FovSystemManager: Unable to blit Fov mask. {nameof(materialContainer.blitMaterial)} not provided.");
			return;
		}

		if (objectOcclusionMask == null)
		{
			Graphics.Blit(iSource, iDestination);
			return;
		}

		using (new DisposableProfiler("5. Light Mask Render (No Gfx Time)"))
		{
			mlightPPRT = mLightMaskRenderer.Render(
				mMainCamera,
				operationParameters.lightPPRTParameter,
				floorOcclusionMask,
				renderSettings,
				matrixRotationMode);
		}

		using (new DisposableProfiler("6. Generate Obstacle Light Mask"))
		{
			mPostProcessingStack.CreateWallLightMask(
				mlightPPRT,
				obstacleLightMask,
				renderSettings,
				operationParameters.cameraOrthographicSize);
		}
		
		// Debug View Selection.
		if (renderSettings.viewMode == RenderSettings.ViewMode.LightLayer)
		{
			PixelPerfectRT.Transform(mlightPPRT, iDestination, mMainCamera, materialContainer.PPRTTransformMaterial);

			return;
		}
		else if (renderSettings.viewMode == RenderSettings.ViewMode.WallLayer)
		{
			PixelPerfectRT.Transform(obstacleLightMask, iDestination, mMainCamera, materialContainer.PPRTTransformMaterial);

			return;
		}
		else if (renderSettings.viewMode == RenderSettings.ViewMode.FovObjectOcclusion)
		{
			PixelPerfectRT.Transform(objectOcclusionMask, iDestination, mMainCamera, materialContainer.PPRTTransformMaterial);

			return;
		}
		else if (renderSettings.viewMode == RenderSettings.ViewMode.FovFloorOcclusion)
		{
			PixelPerfectRT.Transform(floorOcclusionMask, iDestination, mMainCamera, materialContainer.PPRTTransformMaterial);

			return;
		}
		else if (renderSettings.viewMode == RenderSettings.ViewMode.FovWallAndFloorOcclusion)
		{
			PixelPerfectRT.Transform(wallFloorOcclusionMask, iDestination, mMainCamera, materialContainer.PPRTTransformMaterial);

			return;
		}
		else if (renderSettings.viewMode == RenderSettings.ViewMode.Obstacle)
		{
			PixelPerfectRT.Transform(mOcclusionPPRT, iDestination, mMainCamera, materialContainer.PPRTTransformMaterial);

			return;
		}

		using (new DisposableProfiler("7. Light Mask Blur"))
		{
			mPostProcessingStack.BlurLightMask(mlightPPRT.renderTexture, renderSettings, operationParameters.cameraOrthographicSize, mMatrixRotationModeBlend);
		}

		RenderTexture _backgroundMask = null;

		using (new DisposableProfiler("8. Render Background"))
		{
			_backgroundMask = mBackgroundRenderer.Render(renderSettings);
		}

		// Debug Views Selection.
		if (renderSettings.viewMode == RenderSettings.ViewMode.LightLayerBlurred)
		{
			PixelPerfectRT.Transform(mlightPPRT, iDestination, mMainCamera, materialContainer.PPRTTransformMaterial);

			return;
		}
		else if (renderSettings.viewMode == RenderSettings.ViewMode.Background)
		{
			Graphics.Blit(_backgroundMask, iDestination);

			return;
		}

		using (new DisposableProfiler("9. Blit Scene with Mixed Lights"))
		{
			mlightPPRT.renderTexture.filterMode = FilterMode.Bilinear;
			obstacleLightMask.renderTexture.filterMode = FilterMode.Bilinear;
			floorOcclusionMask.renderTexture.filterMode = matrixRotationMode ? FilterMode.Bilinear : FilterMode.Point;

			var _blitMaterial = materialContainer.blitMaterial;
			_blitMaterial.SetTexture("_LightMask", mlightPPRT.renderTexture);
			_blitMaterial.SetTexture("_OcclusionMask", floorOcclusionMask.renderTexture);
			_blitMaterial.SetTexture("_ObstacleLightMask", obstacleLightMask.renderTexture);
			_blitMaterial.SetVector("_LightTransform", mlightPPRT.GetTransformation(mMainCamera));
			_blitMaterial.SetVector("_OcclusionTransform", floorOcclusionMask.GetTransformation(mMainCamera));

			_blitMaterial.SetTexture("_BackgroundTex", _backgroundMask);
			_blitMaterial.SetVector("_AmbLightBloomSA", new Vector4(renderSettings.ambient, renderSettings.lightMultiplier, renderSettings.bloomSensitivity, renderSettings.bloomAdd));
			_blitMaterial.SetFloat("_BackgroundMultiplier", renderSettings.backgroundMultiplier);

			Graphics.Blit(iSource, iDestination, _blitMaterial);
		}
	}
}