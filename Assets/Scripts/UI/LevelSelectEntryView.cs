using System;
using UnityEngine;
using UnityEngine.UI;
using StarSower.Level;

namespace StarSower.UI
{
    // 1 dòng trong danh sách Level Select: tên + trạng thái (Locked/Unlocked/Completed) + số sao.
    // Thuần hiển thị — không tự quyết định unlocked hay không, LevelSelectController truyền vào.
    public class LevelSelectEntryView : MonoBehaviour
    {
        [SerializeField] private Text nameLabel;
        [SerializeField] private Text statusLabel;
        [SerializeField] private Button selectButton;

        public void Setup(LevelDefinition level, bool unlocked, int starsEarned, Action<LevelDefinition> onSelected)
        {
            nameLabel.text = level.displayName;
            selectButton.interactable = unlocked;

            if (!unlocked)
                statusLabel.text = "🔒 Locked";
            else if (starsEarned > 0)
                statusLabel.text = new string('★', starsEarned) + new string('☆', 3 - starsEarned);
            else
                statusLabel.text = "Unlocked";

            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() => onSelected(level));
        }
    }
}
