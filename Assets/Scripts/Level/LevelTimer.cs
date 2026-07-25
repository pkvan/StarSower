using UnityEngine;

namespace StarSower.Level
{
    // Đếm thời gian trôi qua kể từ khi level bắt đầu. Chỉ đo thời gian, không biết gì về
    // Goal/UI — GoalController đọc ElapsedTime khi Player hoàn thành level.
    public class LevelTimer : MonoBehaviour
    {
        private float elapsedTime;
        private bool isRunning = true;

        public float ElapsedTime => elapsedTime;

        private void Update()
        {
            if (isRunning)
                elapsedTime += Time.deltaTime;
        }

        public void StopTimer()
        {
            isRunning = false;
        }
    }
}
