using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class CarDriver : MonoBehaviour
{
    [SerializeField] CarConfig config;

    Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        ApplyConfig();
    }

    void OnValidate()
    {
        if (_rb == null) _rb = GetComponent<Rigidbody>();
        ApplyConfig();
    }

    void ApplyConfig()
    {
        if (config == null || _rb == null) return;
        _rb.centerOfMass = config.centerOfMass;
        _rb.linearDamping = config.groundLinearDamping;
        _rb.angularDamping = config.groundAngularDamping;
    }

    void FixedUpdate()
    {
        if (config == null) return;

        Vector2 input = ReadInput();

        float throttle = Mathf.Clamp(input.y, -1f, 1f);
        float steer = Mathf.Clamp(input.x, -1f, 1f);

        if (Mathf.Abs(throttle) > 0.01f && _rb.linearVelocity.magnitude < config.maxSpeed)
        {
            _rb.AddForce(transform.forward * (config.motorForce * throttle), ForceMode.Force);
        }

        if (Mathf.Abs(steer) > 0.01f)
        {
            _rb.AddTorque(Vector3.up * (config.steerTorque * steer), ForceMode.Force);
        }
    }

    static Vector2 ReadInput()
    {
        Vector2 v = Vector2.zero;

        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.wKey.isPressed || kb.upArrowKey.isPressed) v.y += 1f;
            if (kb.sKey.isPressed || kb.downArrowKey.isPressed) v.y -= 1f;
            if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) v.x += 1f;
            if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) v.x -= 1f;
        }

        var ts = Touchscreen.current;
        if (ts != null)
        {
            var screen = new Vector2(Screen.width, Screen.height);
            foreach (var t in ts.touches)
            {
                if (!t.press.isPressed) continue;
                Vector2 p = t.position.ReadValue();
                if (p.x < screen.x * 0.5f)
                {
                    float nx = (p.x / (screen.x * 0.5f) - 0.5f) * 2f;
                    v.x += Mathf.Clamp(nx, -1f, 1f);
                }
                else
                {
                    float ny = (p.y / screen.y - 0.5f) * 2f;
                    v.y += Mathf.Clamp(ny, -1f, 1f);
                }
            }
        }

        return new Vector2(Mathf.Clamp(v.x, -1f, 1f), Mathf.Clamp(v.y, -1f, 1f));
    }
}
