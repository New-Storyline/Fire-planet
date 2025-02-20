using TMPro;
using UnityEngine;

/// <summary>
/// Class is responsible for additional rendering of units in game
/// </summary>
public class UnitObject : MonoBehaviour
{
    [SerializeField]
    private Renderer bodyRenderer;
    [SerializeField]
    private TextMeshPro TMP;

    public void SetMaterial(Material material)
    {
        bodyRenderer.material = material;
    }

    public void SetHP(float hp,float maxHP)
    {
        TMP.text = $"{Mathf.Round(hp * 10) / 10} / {maxHP}";
    }

    public void Update()
    {
        TMP.transform.rotation = Camera.main.transform.rotation;
    }
}
