namespace DeepDreams.Interactions
{
    public interface IInteractable
    {
        float HoldDuration { get; }
        float HoldInteract { get; }
        float MultipleUse { get; }
        float IsInteractable { get; }

        void OnStartHover();
        void OnInteract();
        void OnEndHover();
    }
}