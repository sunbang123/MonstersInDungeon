using UnityEngine;
using System;

// 이동 모드 Enum
public enum MovementMode
{
    Stop,
    Walk,
    Run
}

public class PlayerController : MonoBehaviour
{
    [SerializeField] private JoyStickController virtualJoystick;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float runSpeed = 10f;

    private float moveSpeed;
    private SpriteRenderer spriteRenderer;
    private MovementMode currentMode = MovementMode.Walk;

    // 이동 모드 변경 이벤트
    public event Action<MovementMode> OnMovementModeChanged;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        moveSpeed = walkSpeed; // 초기값은 걷기
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

    public void SetMovementMode(MovementMode mode)
    {
        currentMode = mode;

        switch (mode)
        {
            case MovementMode.Stop:
                moveSpeed = 0;
                virtualJoystick.OnPointerUp(null); // 조이스틱 초기화
                break;
            case MovementMode.Walk:
                moveSpeed = walkSpeed;
                break;
            case MovementMode.Run:
                moveSpeed = runSpeed;
                break;
        }

        OnMovementModeChanged?.Invoke(mode);
    }

    public MovementMode GetCurrentMode()
    {
        return currentMode;
    }
}