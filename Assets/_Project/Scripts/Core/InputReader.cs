using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Jediný vstup hry: W A S D + Space (nový Input System, akcie sú definované v kóde).
/// Eventy sa spúšťajú pri stlačení; súvislé ovládanie sa číta cez Move / ActionHeld.
/// </summary>
public class InputReader : MonoBehaviour
{
    public event Action<Vector2> OnMove;
    public event Action<int> OnHorizontal;   // -1 = A, +1 = D (pri stlačení)
    public event Action<int> OnVertical;     // -1 = S, +1 = W (pri stlačení)
    public event Action OnAction;            // Space stlačený
    public event Action<bool> OnActionHold;  // Space stlačený (true) / uvoľnený (false)

    public Vector2 Move { get; private set; }
    public bool ActionHeld { get; private set; }

    InputAction move, left, right, up, down, action;

    void Awake()
    {
        move = new InputAction("Move", InputActionType.Value);
        move.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");
        left = new InputAction("Left", InputActionType.Button, "<Keyboard>/a");
        right = new InputAction("Right", InputActionType.Button, "<Keyboard>/d");
        up = new InputAction("Up", InputActionType.Button, "<Keyboard>/w");
        down = new InputAction("Down", InputActionType.Button, "<Keyboard>/s");
        action = new InputAction("Action", InputActionType.Button, "<Keyboard>/space");
    }

    void OnEnable() { foreach (var a in All()) a.Enable(); }
    void OnDisable() { foreach (var a in All()) a.Disable(); }
    void OnDestroy() { foreach (var a in All()) a.Dispose(); }

    InputAction[] All() => new[] { move, left, right, up, down, action };

    void Update()
    {
        Move = move.ReadValue<Vector2>();
        if (Move != Vector2.zero) OnMove?.Invoke(Move);

        if (left.WasPressedThisFrame()) OnHorizontal?.Invoke(-1);
        if (right.WasPressedThisFrame()) OnHorizontal?.Invoke(1);
        if (down.WasPressedThisFrame()) OnVertical?.Invoke(-1);
        if (up.WasPressedThisFrame()) OnVertical?.Invoke(1);

        if (action.WasPressedThisFrame())
        {
            ActionHeld = true;
            OnAction?.Invoke();
            OnActionHold?.Invoke(true);
        }
        if (action.WasReleasedThisFrame())
        {
            ActionHeld = false;
            OnActionHold?.Invoke(false);
        }
    }

    /// <summary>Odpojí všetkých poslucháčov (volá GameManager medzi fázami).</summary>
    public void ClearListeners()
    {
        OnMove = null; OnHorizontal = null; OnVertical = null; OnAction = null; OnActionHold = null;
    }
}
