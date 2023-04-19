using System.Collections;
using MyBox;
using UnityEngine;

namespace DeepDreams.Interactions
{
    public class DoorOld : InteractableBase
    {
        private enum DoorState
        {
            Closed,
            Peek,
            Open,
            Locked
        }

        // [Separator("General")]
        // public float min;
        // public float max;

        [Separator("Rotations")]
        [SerializeField] private float duration;
        [SerializeField] private Vector3 closeRotation;
        [SerializeField] private Vector3 peekRotation;
        [SerializeField] private Vector3 openRotation;
        private Vector3 _targetRotation;
        private Vector3 _currentStateRotation;

        private DoorState _doorState;
        private Coroutine _interactDoorCoroutine;
        private bool _isInteractDoorCoroutineFinished;
        private int _doorOpenDirection;

        private float _velocity;

        public DoorOld(float holdDuration, bool holdInteract, float multipleUse, bool isInteractable) : base(holdDuration, holdInteract,
            multipleUse, isInteractable) {}

        private void Awake()
        {
            _doorState = DoorState.Closed;
            _isInteractDoorCoroutineFinished = true;
        }

        public override void OnInteract(InteractionSource interactionSource)
        {
            base.OnInteract();

            if (!_isInteractDoorCoroutineFinished) return;

            if (_doorState == DoorState.Closed || _doorState == DoorState.Peek)
            {
                _currentStateRotation = closeRotation;
                openDoor();
            }
            else if (_doorState == DoorState.Open)
            {
                _currentStateRotation = openRotation;
                closeDoor();
            }

            _doorOpenDirection =
                (int)Mathf.Sign(Vector3.Dot(transform.forward, (interactionSource.position - transform.position).normalized));

            if (_interactDoorCoroutine != null) StopCoroutine(_interactDoorCoroutine);

            _interactDoorCoroutine = StartCoroutine(interactDoor());
        }

        private IEnumerator interactDoor()
        {
            _isInteractDoorCoroutineFinished = false;

            float timeElasped = 0.0f;

            while (timeElasped < duration)
            {
                float t = timeElasped / duration;

                t = t * t * (3.0f - 2.0f * t); // Smoothstep curve

                transform.rotation = Quaternion.Euler(Vector3.Slerp(_currentStateRotation, _targetRotation, t) * _doorOpenDirection);
                timeElasped += Time.deltaTime;

                yield return null; // Wait 1 frame;
            }

            transform.rotation = Quaternion.Euler(_targetRotation * _doorOpenDirection);
            _isInteractDoorCoroutineFinished = true;
        }

        private void openDoor()
        {
            if (_doorState == DoorState.Peek)
            {
                _doorState = DoorState.Open;
                _currentStateRotation = peekRotation;
                _targetRotation = openRotation;
            }
            else
            {
                _doorState = DoorState.Peek;
                _targetRotation = peekRotation;
            }
        }

        private void closeDoor()
        {
            _doorState = DoorState.Closed;
            _targetRotation = closeRotation;
        }
    }
}