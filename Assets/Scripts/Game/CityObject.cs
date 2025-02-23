using UnityEngine;

/// <summary>
/// Class is responsible for additional rendering of cities in game
/// </summary>
public class CityObject : MonoBehaviour
{
    [SerializeField]
    private Renderer[] roofRenderers;

    public void SetMaterial(Material material)
    {
        foreach (var roofRenderer in roofRenderers)
        {
            roofRenderer.material = material;
        }
    }
}
