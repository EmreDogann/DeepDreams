using System.Collections;
using DeepDreams.Audio;
using DeepDreams.Interactions.Surface;
using DeepDreams.Player.StateMachine.Simple;
using UnityEngine;

namespace DeepDreams.Player
{
    [RequireComponent(typeof(PlayerBlackboard))]
    public class PlayerFootsteps : MonoBehaviour
    {
        [SerializeField] private GameObject footstepMesh;
        [SerializeField] private float meshLifetime;

        [SerializeField] private LayerMask collisionDetectionLayerMask;
        [SerializeField] private float collisionRadius;

        private PlayerBlackboard _blackboard;

        private float _prevPlayerStride;

        private Vector3 _prevTransformPos = Vector3.zero;
        private Vector3 _prevStridePos = Vector3.zero;

        private Coroutine _coroutine;
        private RaycastHit _hit;

        private Material footstepMaterial;

        // Start is called before the first frame update
        private void Awake()
        {
            _blackboard = GetComponent<PlayerBlackboard>();
            _blackboard.OnStrideChange += OnStrideChange;
            _blackboard.OnStride += InteractSurface;

            footstepMesh.SetActive(false);
            footstepMaterial = footstepMesh.GetComponent<MeshRenderer>().sharedMaterial;
            footstepMaterial.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        }

        private void OnDestroy()
        {
            _blackboard.OnStrideChange -= OnStrideChange;
            _blackboard.OnStride -= InteractSurface;
        }

        private void LateUpdate()
        {
            if (_blackboard.IsGrounded) HandleStride();
            _prevTransformPos = transform.position;
        }

        private void HandleStride()
        {
            _blackboard.StrideDistance += Vector3.Magnitude(transform.position - _prevTransformPos);

            if (!_blackboard.IsMoving)
            {
                _prevStridePos = transform.position;
                _blackboard.StrideDistance = _blackboard.PlayerStride - 0.2f;
            }

            if (_blackboard.StrideDistance < _blackboard.PlayerStride) return;

            _prevStridePos = transform.position;
            _blackboard.StrideDistance = 0;
            _blackboard.OnStride?.Invoke();
            // InteractSurface();

            if (_coroutine != null) StopCoroutine(_coroutine);
            _coroutine = StartCoroutine(ToggleFootstep());
        }

        private void InteractSurface()
        {
            if (Physics.SphereCast(transform.position + Vector3.up * (collisionRadius + 0.01f), collisionRadius, Vector3.down, out _hit,
                    collisionRadius,
                    collisionDetectionLayerMask))
            {
                if (_hit.transform.TryGetComponent(out ISteppable component))
                {
                    if (_blackboard.currentPlayerState == PlayerState.Walking || _blackboard.currentPlayerState == PlayerState.Crouching)
                        AudioManager.instance.PlayOneShot(component.GetSurfaceData().walkSound);
                    else if (_blackboard.currentPlayerState == PlayerState.Running)
                        AudioManager.instance.PlayOneShot(component.GetSurfaceData().runSound);
                }
            }
        }

        private void OnStrideChange(float newStride)
        {
            // Keep the same percentage distance to the target from the previous stride to the new stride.
            float amountOfChange = newStride / _prevPlayerStride;
            _prevPlayerStride = newStride;
            _blackboard.StrideDistance *= amountOfChange;
        }

        private IEnumerator ToggleFootstep()
        {
            if (_blackboard.currentPlayerState == PlayerState.Running) footstepMaterial.color = new Color(0.15f, 0.0f, 0.0f, 1.0f);
            else footstepMaterial.color = new Color(0.05f, 0.0f, 0.0f, 1.0f);

            footstepMesh.SetActive(true);
            yield return meshLifetime == 0 ? new WaitForEndOfFrame() : new WaitForSeconds(meshLifetime);
            footstepMesh.SetActive(false);
            footstepMaterial.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        }

        private void OnDrawGizmos()
        {
            // if (Application.isPlaying)
            // {
            //     Gizmos.color = Color.red;
            //     Gizmos.DrawSphere(_prevStridePos + transform.forward * _blackboard.PlayerStride, 0.1f);
            //     Gizmos.color = Color.green;
            //     Gizmos.DrawSphere(_prevStridePos + transform.forward * _blackboard.PlayerStride * _blackboard.StrideDistance, 0.1f);
            // }
        }
    }
}