namespace DeepDreams.UI.Views
{
    public class PauseMenuView : View
    {
        public override void Open()
        {
            transform.parent.gameObject.SetActive(true);
            base.Open();
        }
    }
}