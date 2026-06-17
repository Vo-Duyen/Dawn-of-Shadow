namespace DawnOfShadow.MVP.PauseMenu
{
    public class PauseMenuModel
    {
        public bool IsPaused { get; private set; }

        public void SetPaused(bool isPaused)
        {
            IsPaused = isPaused;
        }
    }
}
