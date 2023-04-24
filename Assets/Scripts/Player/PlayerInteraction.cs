using DeepDreams.Interactions;
using DeepDreams.ScriptableObjects.Events.UnityEvents;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DeepDreams.Player
{
    public class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private float interactionForce;

        [SerializeField] private float interactMouseSensitivityMultiplier;
        [SerializeField] private float interactMouseSensitivitySmoothingTime;

        [SerializeField] private float range;
        [SerializeField] private LayerMask mask;

        private InteractableBase _currentTarget;
        private Ray _cameraRay;
        private RaycastHit _hit;
        private Camera _mainCamera;

        private PlayerInput _playerInput;
        private InputAction _interactAction;
        private bool _isInteractPressed;
        private bool _isInteracting;

        private bool _isPaused;
        private BoolEventListener _onGamePausedEvent;

        private Vector3 _startingInteractionPosition;

        private void Awake()
        {
            _mainCamera = Camera.main;
            _onGamePausedEvent = GetComponent<BoolEventListener>();
        }

        private void Start()
        {
            _playerInput = GetComponent<PlayerInput>();
            _interactAction = _playerInput.actions["Interact"];
        }

        private void OnEnable()
        {
            _onGamePausedEvent.Response.AddListener(OnGamePause);
        }

        private void OnDisable()
        {
            _onGamePausedEvent.Response.RemoveListener(OnGamePause);
        }

        private void Update()
        {
            if (_isInteracting && _interactAction.WasReleasedThisFrame()) StopInteraction();

            if (!_isInteractPressed) RaycastForInteractable();
            else
            {
                if (_isInteracting)
                {
                    // Check if out of interaction range.
                    // if (Vector3.Distance(transform.position, _startingInteractionPosition) > range) StopInteraction();
                }
            }

            // Some house-keeping.
            if (_currentTarget != null)
            {
                if (!_currentTarget.IsInteractable)
                {
                    if (_isInteracting) StopInteraction();
                    return;
                }

                if (_interactAction.WasPressedThisFrame())
                {
                    _isInteracting = true;

                    Vector2 mouseDelta = Mouse.current.delta.ReadValue();
                    mouseDelta = _mainCamera.ScreenToViewportPoint(mouseDelta) * interactionForce;

                    _currentTarget?.OnStartInteract(new InteractionData(transform, _hit.point,
                        new Vector3(mouseDelta.x, mouseDelta.y, 0.0f)));

                    PlayerMouseLook.LookSensitivityMultiply.Invoke(interactMouseSensitivityMultiplier,
                        interactMouseSensitivitySmoothingTime);

                    _startingInteractionPosition = transform.position;
                }

                if (_isInteracting)
                {
                    if (!_currentTarget.HoldInteract || _isPaused) StopInteraction();
                    else
                    {
                        if (_currentTarget.IsHoldInteractFinished())
                            StopInteraction();
                    }

                    Vector2 mouseDelta = Mouse.current.delta.ReadValue();
                    mouseDelta = _mainCamera.ScreenToViewportPoint(mouseDelta) * interactionForce;

                    _currentTarget.OnInteract(new InteractionData(transform, _hit.point, new Vector3(mouseDelta.x, mouseDelta.y, 0.0f)));
                }
            }
        }

        private void StopInteraction()
        {
            _isInteracting = false;
            PlayerMouseLook.LookSensitivityMultiply.Invoke(1.0f, interactMouseSensitivitySmoothingTime);

            Vector2 mouseDelta = Mouse.current.delta.ReadValue();
            mouseDelta = _mainCamera.ScreenToViewportPoint(mouseDelta) * interactionForce;

            _currentTarget?.OnEndInteract(new InteractionData(transform, _hit.point, new Vector3(mouseDelta.x, mouseDelta.y, 0.0f)));
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
                // Debug.DrawRay(_cameraRay.origin, _cameraRay.direction * range, Color.green);
            }
            // else Debug.DrawRay(_cameraRay.origin, _cameraRay.direction * range, Color.red);

            if (newTarget == _currentTarget) return;

            _currentTarget?.OnEndHover();
            newTarget?.OnStartHover();

            _currentTarget = newTarget;
        }

        private void OnInteract(InputValue value)
        {
            _isInteractPressed = value.isPressed;
        }

        private void OnGamePause(bool isPaused)
        {
            _isPaused = isPaused;
        }
    }
}