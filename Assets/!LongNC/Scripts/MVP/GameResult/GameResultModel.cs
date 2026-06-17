namespace DawnOfShadow.MVP.GameResult
{
    public class GameResultModel
    {
        public bool IsVictory { get; private set; }

        public void SetResult(bool isVictory)
        {
            IsVictory = isVictory;
        }

        public void ProcessRewards()
        {
            if (IsVictory)
            {
                // TODO: Tính toán lượng vàng/kinh nghiệm nhận được và lưu qua SaveManager
                // Ví dụ: SaveManager.Instance.Data.Gold += 100;
                // SaveManager.Instance.SaveData();
            }
        }
    }
}
