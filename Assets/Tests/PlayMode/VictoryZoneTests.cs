using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using Platformer.Core;
using Platformer.Gameplay;
using Platformer.Mechanics;
using Platformer.Model;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;

/// <summary>
/// ЛР №4, индивидуальный вариант 16.
/// Механика: PlayerEnteredVictoryZone (Assets/Scripts/Gameplay/PlayerEnteredVictoryZone.cs)
/// и VictoryZone (Assets/Scripts/Mechanics/VictoryZone.cs).
/// Проверка: вход игрока в VictoryZone планирует событие PlayerEnteredVictoryZone.
/// </summary>
public class VictoryZoneTests
{
    private readonly List<GameObject> created = new List<GameObject>();
    private PlatformerModel model;
    private PlayerController savedModelPlayer;

    [SetUp]
    public void SetUp()
    {
        // Очередь событий статическая — очищаем её, чтобы в ней были
        // только события, вызванные этим тестом
        Simulation.Clear();
        model = Simulation.GetModel<PlatformerModel>();
        savedModelPlayer = model.player;
    }

    [TearDown]
    public void TearDown()
    {
        foreach (var go in created)
            if (go != null) Object.Destroy(go);
        created.Clear();
        model.player = savedModelPlayer;
        Simulation.Clear();
    }

    // PLAYMODE ТЕСТ (вариант 16): игрок входит в зону победы -> планируется PlayerEnteredVictoryZone
    [UnityTest]
    public IEnumerator PlayMode_PlayerEntersVictoryZone_SchedulesPlayerEnteredVictoryZone()
    {
        VictoryZone zone = CreateVictoryZone(Vector2.zero, new Vector2(4f, 4f));
        PlayerController player = CreatePlayer(new Vector2(20f, 0f)); // далеко от зоны

        // 1. Пока игрок вне зоны, событие не должно планироваться
        yield return WaitFixedUpdates(3);
        Assert.IsNull(FindPending<PlayerEnteredVictoryZone>(),
            "Ошибка: событие победы запланировано, хотя игрок ещё не вошёл в VictoryZone!");

        // 2. Перемещаем игрока внутрь триггера зоны (метод игры KinematicObject.Teleport)
        player.Teleport(Vector3.zero);

        PlayerEnteredVictoryZone ev = null;
        for (int i = 0; i < 10 && ev == null; i++)
        {
            yield return new WaitForFixedUpdate();
            ev = FindPending<PlayerEnteredVictoryZone>();
        }

        // 3. Событие должно оказаться в очереди симуляции и ссылаться на нашу зону
        Assert.IsNotNull(ev,
            "Ошибка: вход игрока в VictoryZone не запланировал событие PlayerEnteredVictoryZone!");
        Assert.AreSame(zone, ev.victoryZone,
            "Ошибка: в событии PlayerEnteredVictoryZone указана не та зона победы!");

        // 4. Выполняем запланированное событие так же, как это делает GameController.Update():
        //    после Simulation.Tick() игрок должен потерять управление (уровень пройден)
        model.player = player;
        bool executed = false;
        System.Action<PlayerEnteredVictoryZone> onExecute = e => executed = true;
        PlayerEnteredVictoryZone.OnExecute += onExecute;
        try
        {
            Simulation.Tick();
        }
        finally
        {
            PlayerEnteredVictoryZone.OnExecute -= onExecute;
        }

        Assert.IsTrue(executed, "Ошибка: событие PlayerEnteredVictoryZone не выполнилось после Tick()!");
        Assert.IsFalse(player.controlEnabled,
            "Ошибка: после победы у игрока должно отключаться управление!");
    }

    // PLAYMODE ТЕСТ (вариант 16, негативный сценарий):
    // объект без PlayerController (например, враг или ящик) не вызывает победу
    [UnityTest]
    public IEnumerator PlayMode_NonPlayerEntersVictoryZone_DoesNotScheduleEvent()
    {
        CreateVictoryZone(Vector2.zero, new Vector2(4f, 4f));

        var other = new GameObject("NotAPlayer");
        created.Add(other);
        var body = other.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        other.AddComponent<BoxCollider2D>();

        yield return WaitFixedUpdates(5);

        Assert.IsNull(FindPending<PlayerEnteredVictoryZone>(),
            "Ошибка: VictoryZone запланировала победу для объекта без PlayerController!");
    }

    // ---------- Вспомогательные методы ----------

    private VictoryZone CreateVictoryZone(Vector2 position, Vector2 size)
    {
        var go = new GameObject("TestVictoryZone");
        created.Add(go);
        go.transform.position = position;
        var trigger = go.AddComponent<BoxCollider2D>();
        trigger.isTrigger = true; // как у зоны победы на сцене
        trigger.size = size;
        return go.AddComponent<VictoryZone>();
    }

    private PlayerController CreatePlayer(Vector2 position)
    {
        EnsurePlayerInputActions();

        var go = new GameObject("TestPlayer");
        created.Add(go);
        go.SetActive(false); // Awake сработает после добавления всех компонентов
        go.transform.position = position;

        go.AddComponent<Rigidbody2D>(); // KinematicObject переведёт его в Kinematic
        go.AddComponent<BoxCollider2D>().size = Vector2.one;
        go.AddComponent<SpriteRenderer>();
        go.AddComponent<Animator>();
        go.AddComponent<Health>();
        var player = go.AddComponent<PlayerController>();
        player.gravityModifier = 0f; // чтобы игрок не «уплывал» вниз во время теста

        go.SetActive(true);
        return player;
    }

    // PlayerController.Awake ищет действия Player/Move и Player/Jump в project-wide Input Actions.
    // В проекте они есть (Assets/Settings/InputSystem_Actions.inputactions). Если по какой-то
    // причине ассет не подхватился, создаём минимальную замену, чтобы тест проверял именно VictoryZone.
    private static void EnsurePlayerInputActions()
    {
        if (InputSystem.actions != null &&
            InputSystem.actions.FindAction("Player/Move") != null &&
            InputSystem.actions.FindAction("Player/Jump") != null)
            return;

        var asset = ScriptableObject.CreateInstance<InputActionAsset>();
        var map = asset.AddActionMap("Player");
        map.AddAction("Move", InputActionType.Value);
        map.AddAction("Jump", InputActionType.Button);
        InputSystem.actions = asset;
    }

    private static IEnumerator WaitFixedUpdates(int count)
    {
        for (int i = 0; i < count; i++)
            yield return new WaitForFixedUpdate();
    }

    // Очередь Simulation приватная, поэтому читаем её через отражение.
    // Так тест проверяет именно факт ПЛАНИРОВАНИЯ события, а не только его последствия.
    private static T FindPending<T>() where T : Simulation.Event
    {
        var queueField = typeof(Simulation).GetField("eventQueue", BindingFlags.NonPublic | BindingFlags.Static);
        Assert.IsNotNull(queueField, "Не найдено поле Simulation.eventQueue");
        var queue = queueField.GetValue(null);

        var itemsField = queue.GetType().GetField("items", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(itemsField, "Не найдено поле HeapQueue.items");
        var items = (List<Simulation.Event>)itemsField.GetValue(queue);

        foreach (var e in items)
            if (e is T typed) return typed;
        return null;
    }
}
