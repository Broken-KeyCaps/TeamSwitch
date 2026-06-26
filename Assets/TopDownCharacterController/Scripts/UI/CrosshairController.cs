using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    private SpriteRenderer _spriteRenderer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (_spriteRenderer != null)
        {
            
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f; 

            
            transform.position = mousePos;
        }
    }

    public void SetCrosshairSprite(Sprite crosshairSprite)
    {
        
        if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_spriteRenderer != null)
        {
            _spriteRenderer.sprite = crosshairSprite;
        }
    }
}