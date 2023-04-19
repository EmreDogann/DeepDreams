using DeepDreams.Interactions;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DeepDreams.Player
{
    public class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private float interactSensitivityMultiplier;

        [SerializeField] private float range;
        [SerializeField] private LayerMask mask;

        private InteractableBase _currentTarget;
        private Ray _cameraRay;
        private RaycastHit _hit;
        private Camera _mainCamera;

        private PlayerInput _playerInput;
        private InputAction _interactAction;
        private bool _isInteractPressed;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void Start()
        {
            _playerInput = GetComponent<PlayerInput>();
            _interactAction = _playerInput.actions["Interact"];
        }

        private void Update()
        {
            // Some house-keeping.
            if (_interactAction.WasReleasedThisFrame())
            {
                _currentTarget?.OnEndInteract();
                PlayerMouseLook.LookSensitivityMultiply.Invoke(1.0f);
            }

            if (!_isInteractPressed) RaycastForInteractable();

            if (_isInteractPressed && _currentTarget != null)
            {
                if (!_currentTarget.IsInteractable) return;

                if (!_currentTarget.HoldInteract) _isInteractPressed = false;
                else
                {
                    if (_currentTarget.IsHoldInteractFinished()) PlayerMouseLook.LookSensitivityMultiply.Invoke(1.0f);
                    else PlayerMouseLook.LookSensitivityMultiply.Invoke(interactSensitivityMultiplier);
                }

                _currentTarget.OnInteract(new InteractionSource(transform.position, transform.forward));
            }
        }

        private void RaycastForInteractable()
        {
            // Send ray out from center of the screen.
            _cameraRay = _mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
            bool isObjHit = Physics.Raycast(_cameraRay, out _hit, range, mask);
            InteractableBase newTarget = null;

            if (isObjHit)
            {
                newTarget = _hit.collider.GetComponent<InteractableBase>();
                // Debug.DrawRay(_cameraRay.origin, _cameraRay.direction, Color.green);
            }
            // else Debug.DrawRay(_cameraRay.origin, _cameraRay.direction, Color.red);

            if (newTarget == _currentTarget) return;

            _currentTarget?.OnEndHover();
            newTarget?.OnStartHover();

            _currentTarget = newTarget;
        }

        private void OnInteract(InputValue value)
        {
            _isInteractPressed = value.isPressed;
        }
    }
}