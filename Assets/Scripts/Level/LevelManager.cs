using UnityEngine;
using UnityEngine.SceneManagement;
using StarSower.UI;

namespace StarSower.Level
{
    // Điều hướng giữa các level: level hiện tại là ai, load level kế tiếp, quay lại Level Select.
    // Không biết Player/Goal/Collectible — chỉ biết Database + Scene. Level Select hiện là 1 Canvas
    // trong cùng scene (xem LevelSelectController) chứ chưa phải scene riêng — LoadNextLevel() vẫn
    // dùng đúng API SceneManager.LoadScene nên khi tách level thành scene riêng sau này, chỗ khác
    // không cần sửa gì.
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private LevelDatabase levelDatabase;
        [SerializeField] private string currentLevelId;
        [SerializeField] private LevelSelectController levelSelect;

        public string CurrentLevelId => currentLevelId;
        public LevelDatabase Database => levelDatabase;

        public bool HasNextLevel => levelDatabase.GetNext(currentLevelId) != null;

        public void LoadNextLevel()
        {
            LevelDefinition next = levelDatabase.GetNext(currentLevelId);
            if (next == null)
                return;

            SceneManager.LoadScene(next.sceneName);
        }

        public void LoadLevelSelect()
        {
            levelSelect.Show();
        }

        // Gọi bởi LevelSelectController khi Player chọn 1 level đã mở khóa.
        public void LoadLevel(LevelDefinition level)
        {
            if (level.sceneName == SceneManager.GetActiveScene().name)
            {
                levelSelect.Hide();
                return;
            }

            SceneManager.LoadScene(level.sceneName);
        }
    }
}
