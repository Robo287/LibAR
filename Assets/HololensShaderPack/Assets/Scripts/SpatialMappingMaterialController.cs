#if HAVE_MRTK
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
#elif HAVE_HTK
using HoloToolkit.Unity.SpatialMapping;
#endif
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Steps through the provided materials 
/// </summary>
public class SpatialMappingMaterialController : MonoBehaviour
{
    public List<Material> Materials;
    public Material SelectedMaterial { get; private set; }
    private int MaterialIndex = -1;

    public delegate void NewMaterialSelectedAction(Material selectedMaterial);
    public static event NewMaterialSelectedAction OnNewMaterialSelected;

#if HAVE_MRTK
    IMixedRealitySpatialAwarenessMeshObserver spatialAwarenessMeshObserver;
#endif

    // Use this for initialization
    void Start()
    {
#if HAVE_MRTK
        if (CoreServices.SpatialAwarenessSystem == null)
        {
            Debug.LogError("This script expects that you have a SpatialAwarenessSystem configured.");
            Destroy(this);
        }

        spatialAwarenessMeshObserver = ((IMixedRealityDataProviderAccess)CoreServices.SpatialAwarenessSystem).GetDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();

        if (spatialAwarenessMeshObserver == null)
        {
            Debug.LogError("This script expects that you have a IMixedRealitySpatialAwarenessMeshObserver configured.");
            Destroy(this);
        }
#elif HAVE_HTK
        if (!SpatialMappingManager.Instance)
        {
            Debug.LogError("This script expects that you have a SpatialMappingManager component in your scene.");
            Destroy(this);
        }
#endif
    }

    public void SelectNextMaterial()
    {
        MaterialIndex++;
        if (MaterialIndex >= Materials.Count)
        {
            MaterialIndex = Materials.Count > 0 ? 0 : -1;
        }

        if (MaterialIndex >= 0 && MaterialIndex < Materials.Count)
        {
            SelectedMaterial = Materials[MaterialIndex];

#if HAVE_MRTK
            spatialAwarenessMeshObserver.VisibleMaterial = SelectedMaterial;
#elif HAVE_HTK
            SpatialMappingManager.Instance.SurfaceMaterial = SelectedMaterial;
#endif
            if (OnNewMaterialSelected != null)
            {
                OnNewMaterialSelected(SelectedMaterial);
            }
        }
    }
}
