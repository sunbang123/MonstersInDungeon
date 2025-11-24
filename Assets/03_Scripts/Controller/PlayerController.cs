using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private JoyStickController virtualJoystick;
    [SerializeField] private float moveSpeed = 5f;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        float x = virtualJoystick.Horizontal;
        float y = virtualJoystick.Vertical;

        if (x != 0 || y != 0)
        {
            // 이동
            transform.position += new Vector3(x, y, 0) * moveSpeed * Time.deltaTime;

            // 좌우 반전 (Flip)
            if (x < 0)
                spriteRenderer.flipX = true;
            else if (x > 0)
                spriteRenderer.flipX = false;
        }
    }
}