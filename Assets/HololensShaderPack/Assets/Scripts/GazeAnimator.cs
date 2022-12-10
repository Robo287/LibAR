using UnityEngine;
#if HAVE_MRTK
using Microsoft.MixedReality.Toolkit;
#elif HAVE_HTK
using HoloToolkit.Unity.InputModule;
#endif

/// <summary>
/// Updates the _LookAtPoint shader parameter so it can visualize gaze location.
/// </summary>
public class GazeAnimator : MonoBehaviour
{
    [Tooltip("The Material with Transition to animate")]
    public Material SurfaceMaterial;


    void Start()
    {
#if HAVE_MRTK
        if (CoreServices.InputSystem == null)
        {
            Debug.LogError($"Unable to start GazeAnimator. An Input System is required for this feature.");
            Destroy(this);
        }
#elif HAVE_HTK
        if (!GazeManager.Instance)
        {
            Debug.LogError("This script expects that you have a GazeManager component in your scene.");
            Destroy(this);
        }
#endif
    }

    // Update is called once per frame
    void Update()
    {
        if (SurfaceMaterial)
        {
#if HAVE_MRTK
            SurfaceMaterial.SetVector("_LookAtPoint", CoreServices.InputSystem.GazeProvider.HitPosition);
#elif HAVE_HTK
            SurfaceMaterial.SetVector("_LookAtPoint", GazeManager.Instance.HitPosition);
#endif
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

