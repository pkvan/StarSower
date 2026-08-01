using System.Collections.Generic;
using StarSower.Level;

namespace StarSower.Constellations
{
    // Tra ra chom sao ung voi level dang choi (S2-009).
    //
    // Anh xa theo CHI SO: level thu i trong LevelDatabase ung voi chom sao thu i trong ChapterData.
    // Khong hardcode ten scene hay id chom sao — them khu vuc moi vao hai danh sach la tu khop.
    //
    // Tach thanh ham tinh vi gio co HAI ben can biet: ConstellationScreen (de dien canh khoi phuc)
    // va CollectibleManager (de biet man nay phai co bao nhieu sao).
    public static class ConstellationLookup
    {
        public static ConstellationData ForLevel(ChapterData chapter, LevelManager levelManager)
        {
            if (chapter == null || levelManager == null || levelManager.Database == null)
                return null;

            IReadOnlyList<LevelDefinition> levels = levelManager.Database.Levels;
            int index = -1;
            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i].levelId != levelManager.CurrentLevelId)
                    continue;

                index = i;
                break;
            }

            if (index < 0 || index >= chapter.Constellations.Count)
                return null;

            return chapter.Constellations[index];
        }
    }
}
