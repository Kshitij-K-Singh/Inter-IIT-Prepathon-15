using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public static class PlatformerSetup
{
    const int WorldLayer = 6;
    const int UiLayer = 5;
    const string Village = "Assets/Cainos/Pixel Art Platformer - Village Props/Prefab/PF Village Props - ";

    [MenuItem("Game/Rebuild Platformer Scene")]
    public static void Rebuild()
    {
        if (!EditorUtility.DisplayDialog(
                "Rebuild Platformer Scene",
                "This replaces Assets/Scenes/Game.unity with Theo, Cainos village, and the opened book. Continue?",
                "Rebuild",
                "Cancel"))
            return;
        RebuildImmediate();
    }

    public static void RebuildImmediate()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var ch1 = Load<ChapterData>("Assets/ScriptableObjects/Chapters/Chapter_01.asset");
        var ch2 = Load<ChapterData>("Assets/ScriptableObjects/Chapters/Chapter_02.asset");
        var ch3 = Load<ChapterData>("Assets/ScriptableObjects/Chapters/Chapter_03.asset");
        var ch4 = Load<ChapterData>("Assets/ScriptableObjects/Chapters/Chapter_04.asset");

        var camGo = new GameObject("Main Camera");
        camGo.tag = "MainCamera";
        var cam = camGo.AddComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.23f, 0.23f, 0.24f);
        cam.transform.position = new Vector3(4f, 2.5f, -10f);
        camGo.AddComponent<AudioListener>();
        camGo.AddComponent<UniversalAdditionalCameraData>();
        var follow = camGo.AddComponent<TheoCamera>();

        var lightGo = new GameObject("Global Light 2D");
        var light = lightGo.AddComponent<Light2D>();
        light.lightType = Light2D.LightType.Global;
        light.intensity = 1f;

        var worldsRoot = new GameObject("Chapters");
        var w1 = BuildRivia(worldsRoot.transform);
        var w2 = BuildWildwood(worldsRoot.transform);
        var w3 = BuildAshRoad(worldsRoot.transform);
        var w4 = BuildEmbergate(worldsRoot.transform);
        w2.SetActive(false);
        w3.SetActive(false);
        w4.SetActive(false);

        TryOpenedBookBackdrop();
        var theo = BuildTheo();
        follow.Bind(theo.transform);

        var gmGo = new GameObject("GameManager");
        var sfx = gmGo.AddComponent<AudioSource>();
        sfx.playOnAwake = false;
        var ambGo = new GameObject("Ambient");
        ambGo.transform.SetParent(gmGo.transform, false);
        var amb = ambGo.AddComponent<AudioSource>();
        amb.playOnAwake = false;
        var audio = gmGo.AddComponent<GameAudio>();
        audio.Bind(sfx, amb);
        var audioSo = new SerializedObject(audio);
        audioSo.FindProperty("cardHover").objectReferenceValue = Clip("Assets/Audio/card_hover.wav");
        audioSo.FindProperty("cardSelect").objectReferenceValue = Clip("Assets/Audio/card_select.wav");
        audioSo.FindProperty("cardConfirm").objectReferenceValue = Clip("Assets/Audio/card_confirm.wav");
        audioSo.FindProperty("step").objectReferenceValue = Clip("Assets/Audio/Walk.wav");
        audioSo.FindProperty("jump").objectReferenceValue = Clip("Assets/Audio/Jump.wav");
        audioSo.FindProperty("land").objectReferenceValue = Clip("Assets/Audio/Land.wav");
        audioSo.FindProperty("dash").objectReferenceValue = Clip("Assets/Audio/BodyRoll.wav");
        audioSo.FindProperty("death").objectReferenceValue = Clip("Assets/Audio/death.wav");
        audioSo.FindProperty("pillar").objectReferenceValue = Clip("Assets/Audio/pillar.wav");
        audioSo.FindProperty("pillarHurt").objectReferenceValue = Clip("Assets/Audio/PillarHurt.mp3");
        audioSo.FindProperty("page").objectReferenceValue = Clip("Assets/Audio/page.wav");
        audioSo.FindProperty("wind").objectReferenceValue = Clip("Assets/Audio/Ambience.mp3");
        audioSo.FindProperty("sliceA").objectReferenceValue = Clip("Assets/Audio/SwordSlice1.wav");
        audioSo.FindProperty("sliceB").objectReferenceValue = Clip("Assets/Audio/SwordSlice2.wav");
        audioSo.ApplyModifiedPropertiesWithoutUndo();

        var gm = gmGo.AddComponent<GameManager>();
        var chapters = gmGo.AddComponent<ChapterController>();
        var death = theo.GetComponent<PlayerDeath>();

        var canvasGo = new GameObject("Canvas");
        canvasGo.layer = UiLayer;
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        var eventGo = new GameObject("EventSystem");
        eventGo.AddComponent<EventSystem>();
        eventGo.AddComponent<InputSystemUIInputModule>();

        var barRoot = Ui("ConstitutionBar", canvasGo.transform);
        Stretch(barRoot, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(16f, -56f), new Vector2(-16f, -12f));
        var barLayout = barRoot.AddComponent<HorizontalLayoutGroup>();
        barLayout.spacing = 8f;
        barLayout.childAlignment = TextAnchor.MiddleLeft;
        barLayout.childForceExpandWidth = false;
        barLayout.childForceExpandHeight = true;
        var constitution = barRoot.AddComponent<ConstitutionBar>();
        constitution.Bind(barRoot.transform, UiFont());

        var storyGo = Ui("StoryPage", canvasGo.transform);
        Stretch(storyGo, new Vector2(0f, 0.55f), new Vector2(0.34f, 1f), new Vector2(16f, 8f), new Vector2(-8f, -64f));
        var storyBg = storyGo.AddComponent<Image>();
        storyBg.color = new Color(0.93f, 0.88f, 0.74f, 0.92f);
        storyBg.raycastTarget = false;
        var storyTextGo = Ui("Text", storyGo.transform);
        Stretch(storyTextGo, Vector2.zero, Vector2.one, new Vector2(14f, 12f), new Vector2(-14f, -12f));
        var storyText = AddText(storyTextGo, "", 16, TextAnchor.UpperLeft, new Color(0.18f, 0.12f, 0.08f));
        var story = storyGo.AddComponent<StoryLog>();
        story.Bind(storyText);

        var tutorialGo = Ui("TutorialText", canvasGo.transform);
        Stretch(tutorialGo, new Vector2(0.34f, 0f), new Vector2(1f, 0f), new Vector2(16f, 12f), new Vector2(-16f, 48f));
        var tutorial = AddText(tutorialGo, "", 20, TextAnchor.LowerLeft, new Color(0.9f, 0.86f, 0.72f));

        var delayGo = Ui("DeathDelay", canvasGo.transform);
        Stretch(delayGo, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-40f, -120f), new Vector2(40f, -70f));
        var delayLabel = AddText(delayGo, "", 42, TextAnchor.MiddleCenter, new Color(0.95f, 0.25f, 0.2f));
        delayLabel.enabled = false;

        var flashGo = Ui("DeathFlash", canvasGo.transform);
        Stretch(flashGo, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var flashImg = flashGo.AddComponent<Image>();
        flashImg.color = new Color(0.85f, 0.1f, 0.1f, 0.5f);
        flashImg.raycastTarget = false;
        flashGo.SetActive(false);

        var dimGo = Ui("VisionDim", canvasGo.transform);
        Stretch(dimGo, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var dimImg = dimGo.AddComponent<Image>();
        dimImg.color = new Color(0.02f, 0.02f, 0.05f, 0.72f);
        dimImg.raycastTarget = false;
        dimGo.SetActive(false);
        var vision = dimGo.AddComponent<VisionDimController>();
        vision.Bind(dimImg);

        var overlayRoot = Ui("ThresholdOverlay", canvasGo.transform);
        Stretch(overlayRoot, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var dimmer = overlayRoot.AddComponent<Image>();
        dimmer.color = new Color(0f, 0f, 0f, 0.62f);

        var cardsRow = Ui("Cards", overlayRoot.transform);
        Stretch(cardsRow, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-520f, -40f), new Vector2(520f, 280f));
        var cardsLayout = cardsRow.AddComponent<HorizontalLayoutGroup>();
        cardsLayout.spacing = 24f;
        cardsLayout.childAlignment = TextAnchor.MiddleCenter;
        cardsLayout.childForceExpandHeight = true;
        cardsLayout.childForceExpandWidth = true;

        var cardA = MakeCardWidget(cardsRow.transform, "CardA");
        var cardB = MakeCardWidget(cardsRow.transform, "CardB");
        var cardC = MakeCardWidget(cardsRow.transform, "CardC");

        var footerGo = Ui("Footer", overlayRoot.transform);
        Stretch(footerGo, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-500f, 110f), new Vector2(500f, 160f));
        var footer = AddText(footerGo, "Choose a sentence.", 24, TextAnchor.MiddleCenter, Color.white);

        var confirmGo = Ui("Confirm", overlayRoot.transform);
        Stretch(confirmGo, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-120f, 40f), new Vector2(120f, 92f));
        confirmGo.AddComponent<Image>().color = new Color(0.92f, 0.84f, 0.55f);
        var confirmTextGo = Ui("Label", confirmGo.transform);
        Stretch(confirmTextGo, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        AddText(confirmTextGo, "Seal the ink", 24, TextAnchor.MiddleCenter, Color.black).raycastTarget = false;
        var confirm = confirmGo.AddComponent<Button>();
        confirm.targetGraphic = confirmGo.GetComponent<Image>();

        var thresholdUI = overlayRoot.AddComponent<ThresholdUI>();
        thresholdUI.Bind(overlayRoot, cardA, cardB, cardC, footer, confirm);
        overlayRoot.SetActive(false);

        var title = MakePanel(canvasGo.transform, "TitlePanel", "THE CODEX OF THEO", "Open the Book", out var startButton);
        var ending = MakePanel(canvasGo.transform, "EndingPanel", "To be continued.", "Begin again", out var restartButton);
        ending.SetActive(false);

        if (death != null) death.Bind(delayLabel, flashGo);
        chapters.Bind(new[] { ch1, ch2, ch3, ch4 }, new[] { w1, w2, w3, w4 }, theo, tutorial);
        gm.Bind(chapters, thresholdUI, constitution, story, title, ending, startButton, restartButton, dimGo, death);

        EditorUtility.SetDirty(gm);
        EditorUtility.SetDirty(chapters);
        EditorUtility.SetDirty(thresholdUI);
        EditorUtility.SetDirty(audio);

        const string path = "Assets/Scenes/Game.unity";
        if (!EditorSceneManager.SaveScene(scene, path))
            throw new System.Exception("SaveScene failed for " + path);

        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/Boot.unity", true),
            new EditorBuildSettingsScene(path, true)
        };

        EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        Debug.Log("Platformer scene rebuilt. Hierarchy should show Theo, Chapters, OpenedBook — not Grid.");
    }

    static T Load<T>(string path) where T : Object => AssetDatabase.LoadAssetAtPath<T>(path);

    static AudioClip Clip(string path) => AssetDatabase.LoadAssetAtPath<AudioClip>(path);

    static TheoController BuildTheo()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Hero Knight - Pixel Art/Demo/HeroKnight.prefab");
        GameObject go;
        if (prefab != null)
        {
            go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            go.name = "Theo";
            var stock = go.GetComponent<HeroKnight>();
            if (stock != null) stock.enabled = false;
        }
        else
        {
            go = new GameObject("Theo");
            var rb = go.AddComponent<Rigidbody2D>();
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.gravityScale = 2f;
            var col = go.AddComponent<CapsuleCollider2D>();
            col.size = new Vector2(0.55f, 1.05f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Placeholder("theo", new Color(0.85f, 0.55f, 0.28f), 24, 40);
            sr.sortingOrder = 10;
        }

        go.tag = "Player";
        go.layer = 0;
        go.transform.position = new Vector3(2f, 1.4f, 0f);
        var theo = go.GetComponent<TheoController>();
        if (theo == null) theo = go.AddComponent<TheoController>();
        if (go.GetComponent<PlayerDeath>() == null) go.AddComponent<PlayerDeath>();
        return theo;
    }

    [MenuItem("Game/Place Opened Book Backdrop")]
    public static void PlaceOpenedBookMenu()
    {
        if (TryOpenedBookBackdrop() == null)
            EditorUtility.DisplayDialog("Opened book", "Could not load Assets/Book/OpenedBook.png as a Sprite. Select the PNG, set Texture Type to Sprite (2D and UI), Apply, then run this again.", "OK");
        else
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    static GameObject TryOpenedBookBackdrop()
    {
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Book/OpenedBook.png");
        if (sprite == null)
            sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Book/FlippingPages.png");
        if (sprite == null) return null;

        var existing = GameObject.Find("OpenedBook");
        if (existing != null) Object.DestroyImmediate(existing);

        var go = new GameObject("OpenedBook");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = sprite;
        sr.sortingOrder = -40;
        var book = go.AddComponent<BookWorld>();
        book.BindAndFit(sr, Camera.main);

        var cam = Camera.main;
        if (cam != null)
        {
            cam.backgroundColor = new Color(0.23f, 0.23f, 0.24f);
            cam.clearFlags = CameraClearFlags.SolidColor;
        }

        Undo.RegisterCreatedObjectUndo(go, "Place Opened Book");
        Selection.activeGameObject = go;
        return go;
    }

    static GameObject BuildRivia(Transform parent)
    {
        var root = new GameObject("Ch1_Rivia");
        root.transform.SetParent(parent, false);
        GroundRun(root.transform, 0f, 0f, 3);
        VillageProp("Jump Platform 01", root.transform, new Vector3(13.5f, 1.4f, 0f));
        GroundRun(root.transform, 16f, 0f, 5);
        VillageProp("Tree 01", root.transform, new Vector3(2.5f, 0f, 0.1f));
        VillageProp("Well", root.transform, new Vector3(8f, 0f, 0.1f));
        VillageProp("Barrel", root.transform, new Vector3(10.2f, 0f, 0.1f));
        VillageProp("Grass 01", root.transform, new Vector3(4.5f, 0f, 0.1f));
        VillageProp("Grass 01", root.transform, new Vector3(20f, 0f, 0.1f));
        VillageProp("Tree 02", root.transform, new Vector3(24f, 0f, 0.1f));
        KillPlane(root.transform, new Vector3(18f, -5f, 0f), new Vector3(80f, 2f, 1f), HazardKind.Pit);
        Pillar(root.transform, new Vector3(34f, 1.15f, 0f), new Color(0.35f, 0.08f, 0.1f));
        return root;
    }

    static GameObject BuildWildwood(Transform parent)
    {
        var root = new GameObject("Ch2_Wildwood");
        root.transform.SetParent(parent, false);
        GroundRun(root.transform, 0f, 0f, 2);
        VillageProp("Bounding Platform 01", root.transform, new Vector3(12f, 1.6f, 0f));
        VillageProp("Jump Platform 01", root.transform, new Vector3(18f, 2.8f, 0f));
        VillageProp("Bounding Platform 01", root.transform, new Vector3(24f, 1.8f, 0f));
        GroundRun(root.transform, 30f, 0f, 4);
        VillageProp("Tree 01", root.transform, new Vector3(1.5f, 0f, 0.1f));
        VillageProp("Tree 02", root.transform, new Vector3(32f, 0f, 0.1f));
        VillageProp("Tree 01", root.transform, new Vector3(38f, 0f, 0.1f));
        VillageProp("Bush 01", root.transform, new Vector3(6f, 0f, 0.1f));
        KillPlane(root.transform, new Vector3(20f, -5f, 0f), new Vector3(90f, 2f, 1f), HazardKind.Pit);
        Pillar(root.transform, new Vector3(42f, 1.15f, 0f), new Color(0.28f, 0.08f, 0.14f));
        return root;
    }

    static GameObject BuildAshRoad(Transform parent)
    {
        var root = new GameObject("Ch3_AshRoad");
        root.transform.SetParent(parent, false);
        GroundRun(root.transform, 0f, 0f, 3);
        GroundRun(root.transform, 16f, 0f, 2);
        GroundRun(root.transform, 28f, 0f, 4);
        SpikeRun(root.transform, 16.5f, 0.55f, 5);
        VillageProp("Stump", root.transform, new Vector3(4f, 0f, 0.1f));
        KillPlane(root.transform, new Vector3(20f, -5f, 0f), new Vector3(90f, 2f, 1f), HazardKind.Pit);
        Pillar(root.transform, new Vector3(42f, 1.15f, 0f), new Color(0.4f, 0.06f, 0.06f));
        return root;
    }

    static GameObject BuildEmbergate(Transform parent)
    {
        var root = new GameObject("Ch4_Embergate");
        root.transform.SetParent(parent, false);
        GroundRun(root.transform, 0f, 0f, 2);
        VillageProp("Jump Platform 01", root.transform, new Vector3(10f, 1.8f, 0f));
        VillageProp("Bounding Platform 01", root.transform, new Vector3(16f, 3.2f, 0f));
        VillageProp("Jump Platform 01", root.transform, new Vector3(22f, 4.6f, 0f));
        VillageProp("Bounding Platform 01", root.transform, new Vector3(30f, 5.6f, 0f));
        SpikeRun(root.transform, 21.5f, 5.05f, 2);
        KillPlane(root.transform, new Vector3(20f, -6f, 0f), new Vector3(90f, 2f, 1f), HazardKind.Pit);
        Pillar(root.transform, new Vector3(32f, 6.7f, 0f), new Color(0.45f, 0.08f, 0.05f));
        return root;
    }

    static void GroundRun(Transform parent, float startX, float y, int count)
    {
        for (int i = 0; i < count; i++)
            VillageProp("Platform 02 X4", parent, new Vector3(startX + i * 4f, y, 0f));
    }

    static void SpikeRun(Transform parent, float startX, float y, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var spike = VillageProp("Spike", parent, new Vector3(startX + i * 1.05f, y, 0f));
            if (spike != null && spike.GetComponent<DeathZone>() == null)
            {
                var box = spike.GetComponent<BoxCollider2D>();
                if (box != null) box.isTrigger = true;
                spike.AddComponent<DeathZone>().SetKind(HazardKind.Red);
                SetLayer(spike, 0);
            }
        }
    }

    static GameObject VillageProp(string shortName, Transform parent, Vector3 pos)
    {
        var prefab = Load<GameObject>(Village + shortName + ".prefab");
        if (prefab == null)
            return SolidPlatform(shortName, parent, pos, Vector3.one, new Color(0.4f, 0.35f, 0.3f));
        var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        SetLayer(go, WorldLayer);
        return go;
    }

    static void SetLayer(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
            SetLayer(child.gameObject, layer);
    }

    static GameObject SolidPlatform(string name, Transform parent, Vector3 pos, Vector3 scale, Color color)
    {
        var go = new GameObject(name);
        go.layer = WorldLayer;
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.localScale = scale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Placeholder("block-" + ColorUtility.ToHtmlStringRGB(color), color, 16, 16);
        sr.drawMode = SpriteDrawMode.Simple;
        sr.sortingOrder = 1;
        var col = go.AddComponent<BoxCollider2D>();
        col.size = Vector2.one;
        return go;
    }

    static void KillPlane(Transform parent, Vector3 pos, Vector3 scale, HazardKind kind)
    {
        Hazard("KillPlane", parent, pos, scale, new Color(0.05f, 0.05f, 0.08f, 0.0f), kind);
    }

    static GameObject Hazard(string name, Transform parent, Vector3 pos, Vector3 scale, Color color, HazardKind kind)
    {
        var go = new GameObject(name);
        go.layer = 0;
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.localScale = scale;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Placeholder("hazard-" + ColorUtility.ToHtmlStringRGB(color), new Color(color.r, color.g, color.b, 1f), 16, 16);
        sr.sortingOrder = 2;
        if (color.a <= 0.01f) sr.enabled = false;
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = Vector2.one;
        go.AddComponent<DeathZone>().SetKind(kind);
        return go;
    }

    static void Pillar(Transform parent, Vector3 pos, Color color)
    {
        var go = new GameObject("DemonPillar");
        go.layer = 0;
        go.transform.SetParent(parent, false);
        go.transform.position = pos;
        go.transform.localScale = Vector3.one;
        var sr = go.AddComponent<SpriteRenderer>();
        var art = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/demonPillar/DemonPillar.png");
        sr.sprite = art != null ? art : Placeholder("pillar", color, 16, 32);
        sr.sortingOrder = 1;
        var col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        if (sr.sprite != null)
        {
            col.size = sr.sprite.bounds.size;
            col.offset = sr.sprite.bounds.center;
        }
        go.AddComponent<DemonPillar>();
    }

    static Sprite Placeholder(string name, Color color, int w, int h)
    {
        string folder = "Assets/Art/Placeholders";
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/Art", "Placeholders");
        string path = folder + "/" + name + ".png";
        if (!System.IO.File.Exists(path))
        {
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            var pixels = new Color[w * h];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
            tex.SetPixels(pixels);
            tex.Apply();
            System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
            Object.DestroyImmediate(tex);
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 16;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static CardView MakeCardWidget(Transform parent, string name)
    {
        var go = Ui(name, parent);
        var le = go.AddComponent<LayoutElement>();
        le.preferredWidth = 300f;
        le.preferredHeight = 320f;
        var image = go.AddComponent<Image>();
        image.color = new Color(0.92f, 0.84f, 0.55f);
        image.raycastTarget = true;
        var titleGo = Ui("Title", go.transform);
        Stretch(titleGo, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(12f, -58f), new Vector2(-12f, -12f));
        var title = AddText(titleGo, "Card", 22, TextAnchor.MiddleCenter, Color.black);
        var bodyGo = Ui("Body", go.transform);
        Stretch(bodyGo, Vector2.zero, Vector2.one, new Vector2(16f, 16f), new Vector2(-16f, -64f));
        var body = AddText(bodyGo, "", 16, TextAnchor.UpperCenter, new Color(0.15f, 0.12f, 0.10f));
        var view = go.AddComponent<CardView>();
        view.Bind(title, body, image);
        return view;
    }

    static GameObject MakePanel(Transform canvas, string name, string heading, string buttonLabel, out Button button)
    {
        var root = Ui(name, canvas);
        Stretch(root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        var bg = root.AddComponent<Image>();
        bg.color = new Color(0.07f, 0.05f, 0.04f, 0.96f);
        var titleGo = Ui("Heading", root.transform);
        Stretch(titleGo, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-420f, 40f), new Vector2(420f, 150f));
        AddText(titleGo, heading, 44, TextAnchor.MiddleCenter, new Color(0.92f, 0.84f, 0.55f));
        var subGo = Ui("Sub", root.transform);
        Stretch(subGo, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-400f, -10f), new Vector2(400f, 40f));
        AddText(subGo, "A living book. One gift. Two wounds.", 20, TextAnchor.MiddleCenter, new Color(0.75f, 0.7f, 0.58f));
        var btnGo = Ui("Button", root.transform);
        Stretch(btnGo, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-140f, -70f), new Vector2(140f, -10f));
        var img = btnGo.AddComponent<Image>();
        img.color = new Color(0.92f, 0.84f, 0.55f);
        var labelGo = Ui("Label", btnGo.transform);
        Stretch(labelGo, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        AddText(labelGo, buttonLabel, 24, TextAnchor.MiddleCenter, Color.black).raycastTarget = false;
        button = btnGo.AddComponent<Button>();
        button.targetGraphic = img;
        return root;
    }

    static GameObject Ui(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = UiLayer;
        go.transform.SetParent(parent, false);
        return go;
    }

    static RectTransform Stretch(GameObject go, Vector2 min, Vector2 max, Vector2 offMin, Vector2 offMax)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = offMin;
        rt.offsetMax = offMax;
        return rt;
    }

    static Text AddText(GameObject go, string content, int size, TextAnchor align, Color color)
    {
        var text = go.AddComponent<Text>();
        text.text = content;
        text.font = UiFont();
        text.fontSize = size;
        text.alignment = align;
        text.color = color;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    static Font UiFont()
    {
        var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        return font;
    }
}
