using System;
using System.Collections;
using System.Collections.Generic;
using DeepDreams.Audio;
using DeepDreams.SaveLoad;
using DeepDreams.SaveLoad.Data;
using DeepDreams.ScriptableObjects.Audio;
using DeepDreams.Services;
using DeepDreams.UI.Components;
using DeepDreams.UI.Components.Buttons;
using UnityEngine;

namespace DeepDreams.UI.Views
{
    public class OptionsMenuView : View
    {
        [Serializable]
        private class OptionsViewLink
        {
            public OptionsCategoryButton From;
            public GameObject To;
            [HideInInspector] public bool visible;
            private bool active;

            public void SetVisible(bool isVisible)
            {
                To.SetActive(isVisible);
                visible = isVisible;
            }

            public void SetActive(bool isActive)
            {
                active = isActive;
                From.Toggle(isActive);
            }
        }

        [SerializeField] private CanvasGroupBlocker categoryButtonBlocker;
        [SerializeField] private CanvasGroupBlocker optionButtonsBlocker;
        [SerializeField] private List<OptionsViewLink> optionsViewLinks;

        [SerializeField] private AudioReference activationSound;
        [SerializeField] private AudioReference deactivationSound;

        [SerializeField] private ScrollRectNoDrag scrollRect;

        private OptionsViewLink _currentActiveCategory;

        private ISaveManager _saveManager;
        private IAudioManager _audioManager;

        private bool _onEnableFirstTime = true;

        public override void Initialize()
        {
            base.Initialize();

            _saveManager = ServiceLocator.Instance.GetService<ISaveManager>();
            _audioManager = ServiceLocator.Instance.GetService<IAudioManager>();

            categoryButtonBlocker.OnClicked += OnCategoryBlockerClicked;
            optionButtonsBlocker.OnClicked += OnOptionsBlockerClicked;

            for (int i = 0; i < optionsViewLinks.Count; i++)
            {
                optionsViewLinks[i].From.OnCategoryChanged += CategoryChange;
                optionsViewLinks[i].From.OnCategoryClicked += CategoryClicked;
            }

            // Briefly activate and deactivate the options in order to call their Awake() methods.
            transform.parent.gameObject.SetActive(true);
            gameObject.SetActive(true);
            StartCoroutine(ActivateObjectsAtAwake());
        }

        private IEnumerator ActivateObjectsAtAwake()
        {
            var originalActiveStates = new Dictionary<int, bool>();

            for (int i = 0; i < optionsViewLinks.Count; i++)
            {
                originalActiveStates.Add(optionsViewLinks[i].To.GetInstanceID(), optionsViewLinks[i].To.activeSelf);
                optionsViewLinks[i].To.SetActive(true);
            }

            yield return new WaitForEndOfFrame();

            for (int i = 0; i < optionsViewLinks.Count; i++)
            {
                optionsViewLinks[i].To.SetActive(originalActiveStates[optionsViewLinks[i].To.GetInstanceID()]);
            }

            gameObject.SetActive(false);
            transform.parent.gameObject.SetActive(false);
        }

        public override void Open()
        {
            transform.parent.gameObject.SetActive(true);
            base.Open();
        }

        private void OnEnable()
        {
            // OnEnable() is not guaranteed to run after different game objects' Awake(). So menu button's effects might not load in time.
            if (_onEnableFirstTime)
            {
                _onEnableFirstTime = false;
                StartCoroutine(DelayedActivateOptionsMenu());
                return;
            }

            for (int i = 0; i < optionsViewLinks.Count; i++)
            {
                optionsViewLinks[i].SetActive(false);
                optionsViewLinks[i].SetVisible(false);
            }

            categoryButtonBlocker.OnToggle(false);
            optionButtonsBlocker.OnToggle(true);
            optionsViewLinks[0].SetVisible(true);
            optionsViewLinks[0].SetActive(true);
            _currentActiveCategory = optionsViewLinks[0];
        }

        private IEnumerator DelayedActivateOptionsMenu()
        {
            yield return new WaitForEndOfFrame();

            for (int i = 0; i < optionsViewLinks.Count; i++)
            {
                optionsViewLinks[i].SetActive(false);
                optionsViewLinks[i].SetVisible(false);
            }

            categoryButtonBlocker.OnToggle(false);
            optionButtonsBlocker.OnToggle(true);
            optionsViewLinks[0].SetVisible(true);
            optionsViewLinks[0].SetActive(true);
            _currentActiveCategory = optionsViewLinks[0];
        }

        private void OnDestroy()
        {
            categoryButtonBlocker.OnClicked -= OnCategoryBlockerClicked;
            optionButtonsBlocker.OnClicked -= OnOptionsBlockerClicked;

            for (int i = 0; i < optionsViewLinks.Count; i++)
            {
                optionsViewLinks[i].From.OnCategoryChanged -= CategoryChange;
                optionsViewLinks[i].From.OnCategoryClicked -= CategoryClicked;
            }
        }

        public void CategoryChange(OptionsCategoryButton categoryButton)
        {
            for (int i = 0; i < optionsViewLinks.Count; i++)
            {
                if (optionsViewLinks[i].From == categoryButton)
                {
                    optionsViewLinks[i].SetVisible(true);
                    _currentActiveCategory = optionsViewLinks[i];
                }
                else
                {
                    optionsViewLinks[i].SetActive(false);
                    optionsViewLinks[i].SetVisible(false);
                }
            }

            scrollRect.OnCategoryChanged();
        }

        public void CategoryClicked(OptionsCategoryButton categoryButton)
        {
            for (int i = 0; i < optionsViewLinks.Count; i++)
            {
                if (optionsViewLinks[i].From == categoryButton)
                {
                    optionsViewLinks[i].SetActive(true);

                    categoryButtonBlocker.OnToggle(true);
                    optionButtonsBlocker.OnToggle(false);
                }
                else
                {
                    optionsViewLinks[i].SetActive(false);
                }
            }
        }

        public void OnCategoryBlockerClicked()
        {
            _audioManager.PlayOneShot(deactivationSound);
            optionButtonsBlocker.OnToggle(true);

            for (int i = 0; i < optionsViewLinks.Count; i++)
            {
                optionsViewLinks[i].SetActive(false);
            }
        }

        public void OnOptionsBlockerClicked()
        {
            _audioManager.PlayOneShot(activationSound);
            CategoryClicked(_currentActiveCategory.From);
        }

        protected override IEnumerator Hide()
        {
            _saveManager.Save<SettingsData>();
            yield return base.Hide();
        }
    }
}