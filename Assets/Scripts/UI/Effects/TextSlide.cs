using TMPro;
using UnityEngine;

namespace DeepDreams.UI.Effects
{
    public class TextSlide : MonoBehaviour, IButtonEffect
    {
        [Range(0.0f, 3.0f)] [SerializeField] private float slideTime = 0.05f;
        private Vector3 _posVelocity;
        private Vector2 _startPos;
        private Vector2 _targetPos;

        private TMP_Text _textMesh;

        // Start is called before the first frame update
        private void Awake()
        {
            _textMesh = GetComponentInChildren<TMP_Text>();
            _startPos = _textMesh.transform.localPosition;
            _targetPos = _startPos;
        }

        // Update is called once per frame
        private void Update()
        {
            _textMesh.transform.localPosition =
                Vector3.SmoothDamp(_textMesh.transform.localPosition, _targetPos, ref _posVelocity, slideTime, Mathf.Infinity,
                    Time.unscaledDeltaTime);
        }

        private void Activate()
        {
            _targetPos = _startPos + new Vector2(20.0f, 0.0f);
        }

        private void Deactivate()
        {
            _targetPos = _startPos;
        }

        public void OnHoverEnter()
        {
            Activate();
        }

        public void OnHoverExit()
        {
            Deactivate();
        }

        public void OnToggle(bool isSelected)
        {
            if (isSelected) Activate();
            else Deactivate();
        }
    }
}