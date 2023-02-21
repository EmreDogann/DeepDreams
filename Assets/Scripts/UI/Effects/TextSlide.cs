using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DeepDreams.UI.Effects
{
    public class TextSlide : MonoBehaviour
    {
        [Range(0.0f, 3.0f)] [SerializeField] private float slideTime = 0.05f;

        private TMP_Text _textMesh;
        private Vector2 _startPos;
        private Vector2 _targetPos;
        private Vector3 _posVelocity;

        // private void Reset() {
        //     EventTrigger trigger = GetComponentInParent<EventTrigger>();
        //
        //     if (trigger == null) trigger = gameObject.AddComponent<EventTrigger>();
        //
        //     EventTrigger.Entry entry = new EventTrigger.Entry();
        //     entry.eventID = EventTriggerType.PointerEnter;
        //     entry.callback.AddListener(eventData => OnHoverEnter((PointerEventData)eventData));
        //     trigger.triggers.Add(entry);
        //
        //     // entry = new EventTrigger.Entry();
        //     // entry.eventID = EventTriggerType.PointerExit;
        //     // entry.callback.AddListener(OnHoverExit);
        //     // trigger.triggers.Add(entry);
        // }

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

        public void OnHoverEnter(BaseEventData eventData)
        {
            PointerEventData pointerEventData = (PointerEventData)eventData; // For future reference.
            _targetPos = _startPos + new Vector2(20.0f, 0.0f);
        }

        public void OnHoverExit(BaseEventData eventData)
        {
            PointerEventData pointerEventData = (PointerEventData)eventData;
            _targetPos = _startPos;
        }
    }
}