namespace DeepDreams.Interactions
{
    public interface IInteractable
    {
        float HoldDuration { get; }
        bool HoldInteract { get; }
        float MultipleUse { get; }
        bool IsInteractable { get; }

        void OnStartHover();
        void OnInteract();
        void OnEndInteract();
        void OnEndHover();
    }
}