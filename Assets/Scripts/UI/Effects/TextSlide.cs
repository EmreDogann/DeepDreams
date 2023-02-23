using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DeepDreams.UI.Effects
{
    public class TextSlide : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
            // if (_textMesh.transform.localPosition.sqrMagnitude - _targetPos.sqrMagnitude < 0.0001f) return;
            _textMesh.transform.localPosition =
                Vector3.SmoothDamp(_textMesh.transform.localPosition, _targetPos, ref _posVelocity, slideTime, Mathf.Infinity,
                    Time.unscaledDeltaTime);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _targetPos = _startPos + new Vector2(20.0f, 0.0f);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _targetPos = _startPos;
        }
    }
}