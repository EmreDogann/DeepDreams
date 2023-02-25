using TMPro;
using UnityEngine;

namespace DeepDreams.UI.Effects
{
    public class TextBold : MonoBehaviour, IButtonEffect
    {
        [Range(0.0f, 3.0f)] [SerializeField] private float boldTime = 0.05f;
        [Range(0.0f, 3.0f)] [SerializeField] private float boldAmount = 0.3f;
        private float _dilateVelocity;
        private float _startDilate;
        private float _targetDilate;

        private TMP_Text _textMesh;
        [SerializeField] private Material boldMaterial;
        private Material _textMaterialInstance;

        // Start is called before the first frame update
        private void Awake()
        {
            if (_textMesh == null) _textMesh = GetComponentInChildren<TMP_Text>();
            _textMesh.fontMaterial = boldMaterial;
            _textMaterialInstance = _textMesh.fontMaterial;

            _startDilate = _textMaterialInstance.GetFloat(ShaderUtilities.ID_FaceDilate);
            _targetDilate = _startDilate;
        }

        // Update is called once per frame
        private void Update()
        {
            _textMaterialInstance.SetFloat(ShaderUtilities.ID_FaceDilate, Mathf.SmoothDamp(
                _textMaterialInstance.GetFloat(ShaderUtilities.ID_FaceDilate), _targetDilate, ref _dilateVelocity, boldTime,
                Mathf.Infinity,
                Time.unscaledDeltaTime));
        }

        private void Activate()
        {
            _targetDilate = boldAmount;
        }

        private void Deactivate()
        {
            _targetDilate = _startDilate;
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