using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    private Image _crosshairImage;

    private void Awake()
    {
        _crosshairImage = GetComponent<Image>();
    }

    private void Update()
    {
        if (_crosshairImage != null)
        {
            Vector3 mousePos = Input.mousePosition;
            _crosshairImage.rectTransform.position = mousePos;
        }
    }

    public void SetCrosshairSprite(Sprite crosshairSprite)
    {
        if (_crosshairImage != null)
        {
            _crosshairImage.sprite = crosshairSprite;
        }
    }
}
