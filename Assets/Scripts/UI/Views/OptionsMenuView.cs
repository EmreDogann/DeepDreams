using System;
using System.Collections;
using System.Collections.Generic;
using DeepDreams.Audio;
using DeepDreams.DataPersistence;
using DeepDreams.DataPersistence.Data;
using DeepDreams.ScriptableObjects.Audio;
using DeepDreams.UI.Components;
using DeepDreams.UI.Components.Buttons;
using UnityEngine;

namespace DeepDreams.UI.Views
{
    public class OptionsMenuView : View
    {
        [Serializable]
        private struct OptionsViewLink
        {
            public OptionsCategoryButton From;
            public GameObject To;
            public bool visible;
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

        private OptionsViewLink? _currentActiveCategory;
        private bool _isFirstTime = true;

        public override void Initialize()
        {
            base.Initialize();
            categoryButtonBlocker.OnClicked += OnCategoryBlockerClicked;
            optionButtonsBlocker.OnClicked += OnOptionsBlockerClicked;

            optionButtonsBlocker.OnToggle(true);
            optionsViewLinks[0].SetVisible(true);
            _currentActiveCategory = optionsViewLinks[0];

            for (int i = 0; i < optionsViewLinks.Count; i++)
            {
                optionsViewLinks[i].From.OnCategoryChanged += CategoryChange;
                optionsViewLinks[i].From.OnCategoryClicked += CategoryClicked;
            }
        }

        private void OnEnable()
        {
            if (_isFirstTime)
            {
                _isFirstTime = false;
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
                else optionsViewLinks[i].SetActive(false);
            }
        }

        public void OnCategoryBlockerClicked()
        {
            AudioManager.instance.PlayOneShot(deactivationSound);
            optionButtonsBlocker.OnToggle(true);

            for (int i = 0; i < optionsViewLinks.Count; i++)
            {
                optionsViewLinks[i].SetActive(false);
            }
        }

        public void OnOptionsBlockerClicked()
        {
            AudioManager.instance.PlayOneShot(activationSound);
            CategoryClicked(_currentActiveCategory.Value.From);
        }

        protected override IEnumerator Hide()
        {
            DataPersistenceManager.instance.SaveData<SettingsData>();
            yield return base.Hide();
        }
    }
}