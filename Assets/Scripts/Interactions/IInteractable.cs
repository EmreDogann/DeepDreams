namespace DeepDreams.Interactions
{
    public interface IInteractable
    {
        float HoldDuration { get; }
        bool HoldInteract { get; }
        float MultipleUse { get; }
        bool IsInteractable { get; }

        void OnStartHover();

        void OnStartInteract(InteractionData interactionData);
        void OnInteract(InteractionData interactionData);
        void OnEndInteract(InteractionData interactionData);
        void OnEndHover();
    }
}