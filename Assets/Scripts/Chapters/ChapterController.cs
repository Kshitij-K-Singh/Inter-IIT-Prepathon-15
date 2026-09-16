using UnityEngine;
using UnityEngine.UI;

public class ChapterController : MonoBehaviour
{
    [SerializeField] ChapterData[] chapters;
    [SerializeField] GameObject[] chapterWorlds;
    [SerializeField] TheoController theo;
    [SerializeField] Text tutorialText;

    public ChapterData Current { get; private set; }
    public int Count => chapters != null ? chapters.Length : 0;

    public void Bind(ChapterData[] list, GameObject[] worlds, TheoController player, Text tutorial)
    {
        chapters = list;
        chapterWorlds = worlds;
        theo = player;
        tutorialText = tutorial;
    }

    public void Bind(
        ChapterData[] list,
        GridWorld world,
        GridMover mover,
        PlayerDeath death,
        Transform parent,
        Camera cam,
        Text tutorial)
    {
        Bind(list, System.Array.Empty<GameObject>(), null, tutorial);
    }

    public void Load(int index)
    {
        if (chapters == null || index < 0 || index >= chapters.Length)
        {
            Current = null;
            return;
        }

        Current = chapters[index];
        if (chapterWorlds != null)
        {
            for (int i = 0; i < chapterWorlds.Length; i++)
            {
                if (chapterWorlds[i] != null)
                    chapterWorlds[i].SetActive(i == index);
            }
        }

        if (chapterWorlds != null)
        {
            foreach (var world in chapterWorlds)
            {
                if (world == null || !world.activeSelf) continue;
                foreach (var pillar in world.GetComponentsInChildren<DemonPillar>(true))
                    pillar.ResetPillar();
                ResetPhysics(world);
            }
        }

        theo?.Respawn(Current.spawnPosition);
        if (tutorialText != null)
            tutorialText.text = Current.tutorialText ?? string.Empty;
    }

    static void ResetPhysics(GameObject world)
    {
        var bodies = world.GetComponentsInChildren<Rigidbody2D>(true);
        for (int i = 0; i < bodies.Length; i++)
        {
            var rb = bodies[i];
            if (rb == null) continue;
            var rest = rb.GetComponent<PhysicsRestorer>();
            if (rest == null)
                rb.gameObject.AddComponent<PhysicsRestorer>();
            else
                rest.Restore();
        }
    }
}
