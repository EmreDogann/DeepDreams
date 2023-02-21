using UnityEngine;
using UnityEngine.InputSystem;

namespace DeepDreams.Interactions
{
    public class CameraRaycast : MonoBehaviour
    {
        [SerializeField] private float range;

        private IInteractable _currentTarget;
        private Ray _cameraRay;
        private RaycastHit _hit;
        private Camera _mainCamera;

        private bool _isInteractPressed;

        private void Awake() {
            _mainCamera = Camera.main;
        }

        private void Start() {
            // Send ray out from center of the screen.
            _cameraRay = _mainCamera.ScreenPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
        }

        private void Update() {
            RaycastForInteractable();

            if (_isInteractPressed && _currentTarget != null) _currentTarget.OnInteract();
        }

        private void RaycastForInteractable() {
            bool isObjHit = Physics.Raycast(_cameraRay, out _hit, range);
            IInteractable newTarget = null;

            if (isObjHit) newTarget = _hit.collider.GetComponent<IInteractable>();
            if (newTarget == _currentTarget) return;

            _currentTarget?.OnEndHover();
            newTarget?.OnStartHover();

            _currentTarget = newTarget;
        }

        private void OnInteract(InputValue value) {
            _isInteractPressed = value.isPressed;
        }
    }
}