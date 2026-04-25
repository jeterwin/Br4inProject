using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class ButtonTextOutline : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TextMeshProUGUI _btnText;

    [Header("Outline Settings")]
    public Color hoverColor = Color.white;
    public float hoverThickness = 0.5f;

    private Color _baseColor;
    private float _baseThickness;

    void Awake()
    {
        // Automatically find the TMP component in the children
        _btnText = GetComponentInChildren<TextMeshProUGUI>();

        if (_btnText != null)
        {
            _baseColor = _btnText.outlineColor;
            _baseThickness = _btnText.outlineWidth;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_btnText == null) return;
        // This targets the shader property directly
        _btnText.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, hoverColor);
        _btnText.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, hoverThickness);
        _btnText.UpdateMeshPadding();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_btnText == null) return;
        _btnText.fontMaterial.SetColor(ShaderUtilities.ID_OutlineColor, _baseColor);
        _btnText.fontMaterial.SetFloat(ShaderUtilities.ID_OutlineWidth, _baseThickness);
    }
}
