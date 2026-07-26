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
        //
        // So sánh bằng levelId, KHÔNG bằng tên scene: levelId là danh tính thật của một màn, còn
        // sceneName chỉ là chỗ nó đang nằm. Đổi tên file scene, hay để hai levelId cùng trỏ vào một
        // scene (bản dễ/khó, biến thể theo cốt truyện...) đều làm phép so tên trả lời sai — sai theo
        // kiểu im lặng: chọn một màn khác mà game tưởng bạn đang ở đó rồi, nên chỉ đóng Level Select
        // và đứng yên.
        public void LoadLevel(LevelDefinition level)
        {
            if (level.levelId == currentLevelId)
            {
                levelSelect.Hide();
                return;
            }

            SceneManager.LoadScene(level.sceneName);
        }
    }
}
