using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using StarSower.Constellations;
using StarSower.Level;

namespace StarSower.UI
{
    // Man chon chapter (S2-014). Dung mot the cho moi chapter trong ChapterDatabase.
    //
    // Hien tai moi co Chapter 1, nhung KHONG hardcode con so 1: them chapter vao database la man
    // nay tu moc them the, khong phai sua dong nao.
    public class ChapterSelectController : MonoBehaviour
    {
        [SerializeField] private ChapterDatabase chapterDatabase;
        [SerializeField] private ProgressManager progressManager;
        [SerializeField] private Transform entryContainer;
        [SerializeField] private ChapterSelectEntryView entryPrefab;

        [Tooltip("Router de chuyen sang man chon level sau khi chon chapter.")]
        [SerializeField] private MenuRouter router;

        [Tooltip("Khu vuc MO DAU cua tung chapter, xep cung thu tu voi ChapterDatabase. Lay lop nen " +
                 "dau tien cua no lam anh cho the. De trong thi the khong co anh nen.")]
        [SerializeField] private StarSower.Biome.RegionData[] regions = new StarSower.Biome.RegionData[0];

        private void OnEnable()
        {
            Populate();
        }

        private void Populate()
        {
            if (chapterDatabase == null || entryContainer == null || entryPrefab == null)
                return;

            for (int i = entryContainer.childCount - 1; i >= 0; i--)
                Destroy(entryContainer.GetChild(i).gameObject);

            IReadOnlyList<ChapterData> chapters = chapterDatabase.Chapters;
            for (int i = 0; i < chapters.Count; i++)
            {
                ChapterData data = chapters[i];
                if (data == null)
                    continue;

                // Chapter dau tien luon mo. Cac chapter sau mo khi chapter truoc da xong — dung
                // dung luat "Level N mo sau khi xong N-1" cua spec, chi o cap chapter.
                bool unlocked = i == 0 || IsChapterComplete(chapters[i - 1]);

                ChapterSelectEntryView entry = Instantiate(entryPrefab, entryContainer);
                entry.Setup(data, unlocked, CountRestored(data), CountNodes(data),
                            ResolveBackground(i), OnChapterSelected);
            }
        }

        private Sprite ResolveBackground(int index)
        {
            if (regions == null || index < 0 || index >= regions.Length || regions[index] == null)
                return null;

            IReadOnlyList<StarSower.Biome.BackgroundLayerData> layers = regions[index].BackgroundLayers;
            return layers.Count > 0 ? layers[0].sprite : null;
        }

        private bool IsChapterComplete(ChapterData data)
        {
            if (data == null || progressManager == null)
                return false;

            return CountRestored(data) >= CountNodes(data) && CountNodes(data) > 0;
        }

        private int CountNodes(ChapterData data)
        {
            int total = 0;
            foreach (ConstellationData c in data.Constellations)
            {
                if (c != null)
                    total += c.NodeCount;
            }
            return total;
        }

        private int CountRestored(ChapterData data)
        {
            if (progressManager == null)
                return 0;

            int total = 0;
            foreach (ConstellationData c in data.Constellations)
            {
                if (c != null)
                    total += progressManager.GetConstellationNodes(c.ConstellationId, c.NodeCount);
            }
            return total;
        }

        private void OnChapterSelected(ChapterData data)
        {
            router?.ShowLevelSelect();
        }
    }
}
