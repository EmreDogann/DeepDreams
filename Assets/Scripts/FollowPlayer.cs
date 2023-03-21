using UnityEngine;

namespace DeepDreams
{
    public class FollowPlayer : MonoBehaviour
    {
        [SerializeField] private Transform playerTransform;

        // Start is called before the first frame update
        private void Start() {}

        // Update is called once per frame
        private void Update()
        {
            transform.position = new Vector3(playerTransform.position.x, transform.position.y, transform.position.z);
        }
    }
}