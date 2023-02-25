namespace DeepDreams.UI.Effects
{
    public interface IButtonEffect
    {
        public void OnHoverEnter();
        public void OnHoverExit();

        public void OnToggle(bool isSelected);
    }
}