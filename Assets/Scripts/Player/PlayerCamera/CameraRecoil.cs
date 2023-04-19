using UnityEngine;

namespace DeepDreams.Player.PlayerCamera
{
    public class CameraRecoil : MonoBehaviour
    {
        private Vector3 _currentRotation;
        private Vector3 _targetRotation;

        private float _snappiness;
        private float _returnSpeed;

        // Update is called once per frame
        private void FixedUpdate()
        {
            _targetRotation = Vector3.Lerp(_targetRotation, Vector3.zero, _returnSpeed * Time.deltaTime);
            _currentRotation = Vector3.Slerp(_currentRotation, _targetRotation, _snappiness * Time.fixedDeltaTime);

            transform.localRotation = Quaternion.Euler(_currentRotation);
        }

        public void RecoilFire(Vector3 recoil, float newSnappiness, float newReturnSpeed)
        {
            _snappiness = newSnappiness;
            _returnSpeed = newReturnSpeed;

            _targetRotation += new Vector3(
                recoil.x,
                Random.Range(-recoil.y, recoil.y),
                Random.Range(-recoil.z, recoil.z)
            );
        }
    }
}