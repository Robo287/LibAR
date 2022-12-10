#if HAVE_MRTK
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
#elif HAVE_HTK
using HoloToolkit.Unity.SpatialMapping;
using HoloToolkit.Unity.InputModule;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


/// <summary>
/// Uses the tap gesture to trigger the transition animation.
/// Uses Gaze hitpoint as center and Gaze direction for direction of the sweep depending on which transitiontype was selected for the shader.
/// </summary>
public class TapTransitionAnimator : MonoBehaviour
#if HAVE_MRTK
    ,IMixedRealityPointerHandler
#elif HAVE_HTK
    ,IInputClickHandler
#endif
{
    [Tooltip("The Material with Transition to animate")]
    public Material SurfaceMaterial;

    [Tooltip("Animation speed in meters per second")]
    public float Speed = 1.0f;

    [Tooltip("The maximum distance from the gaze hitpoint in meters.")]
    public float Range = 5.0f;

    private float offset = float.MaxValue;

    // Update is called once per frame
    void Update()
    {
        if (SurfaceMaterial && offset < Range)
        {
            offset += Speed * Time.deltaTime;
            SurfaceMaterial.SetFloat("_TransitionOffset", offset - 1);
        }
    }

#if HAVE_MRTK
    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {
        
    }

    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
    }


    public void OnPointerDragged(MixedRealityPointerEventData eventData)
    {
        UpdateMaterial(CoreServices.InputSystem.GazeProvider.HitPosition);
    }

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        UpdateMaterial(CoreServices.InputSystem.GazeProvider.HitPosition);
    }

#elif HAVE_HTK
    public void OnInputClicked(InputClickedEventData eventData)
    {
        UpdateMaterial(GazeManager.Instance.HitPosition);
    }
#endif

    private void UpdateMaterial(Vector4 position)
    {
        offset = 0;
        if (SurfaceMaterial)
        {
            SurfaceMaterial.SetVector("_Center", position);
        }
    }

    void OnEnable()
    {
        SpatialMappingMaterialController.OnNewMaterialSelected += MaterialController_OnNewMaterialSelected;
    }

    void OnDisable()
    {
        SpatialMappingMaterialController.OnNewMaterialSelected -= MaterialController_OnNewMaterialSelected;
    }

    private void MaterialController_OnNewMaterialSelected(Material selectedMaterial)
    {
        SurfaceMaterial = selectedMaterial;
    }
}

