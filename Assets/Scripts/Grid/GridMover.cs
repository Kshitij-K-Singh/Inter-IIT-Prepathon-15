using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GridMover : MonoBehaviour
{
    const int UndoCap = 50;

    [SerializeField] GridWorld world;
    [SerializeField] PlayerDeath death;
    [SerializeField] ThresholdUI thresholdUI;

    public Vector3Int Cell { get; private set; }

    struct UndoRecord
    {
        public Vector3Int playerCell;
        public List<(GridEntity entity, Vector3Int cell)> pushables;
        public int deathQueue;
    }

    readonly List<UndoRecord> undo = new List<UndoRecord>(UndoCap);

    public void Bind(GridWorld gridWorld, PlayerDeath playerDeath, ThresholdUI overlay)
    {
        world = gridWorld;
        death = playerDeath;
        thresholdUI = overlay;
    }

    public void SetCell(Vector3Int cell)
    {
        Cell = cell;
        Snap();
    }

    public void ClearUndo() => undo.Clear();

    void Update()
    {
        if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;
        if (GameManager.Instance.IsPaused) return;
        if (death != null && death.IsResolving) return;

        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.zKey.wasPressedThisFrame)
        {
            TryUndo();
            return;
        }

        Vector3Int dir = Vector3Int.zero;
        if (keyboard.wKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame) dir = Vector3Int.up;
        else if (keyboard.sKey.wasPressedThisFrame || keyboard.downArrowKey.wasPressedThisFrame) dir = Vector3Int.down;
        else if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame) dir = Vector3Int.left;
        else if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame) dir = Vector3Int.right;
        if (dir == Vector3Int.zero) return;

        TryStep(dir);
    }

    void TryStep(Vector3Int dir)
    {
        if (world == null) world = GridWorld.Instance;
        if (world == null) return;

        var target = Cell + dir;
        var occupant = world.GetEntityAt(target);

        if (occupant == null && world.IsPitCell(target) && RuleBook.IsPushPull())
        {
            var across = world.GetEntityAt(target + dir);
            if (across != null && across.IsPushable)
            {
                PushUndo();
                world.TryMoveEntity(across.cell, target);
                AfterTurn();
                return;
            }
        }

        if (occupant != null && occupant.IsPushable && !world.HasPitTerrain(target))
        {
            if (RuleBook.IsPushPull())
            {
                // Frozen Push-is-Pull: do not move the player. Move the pushable one cell
                // toward the player onto the cell behind you (playerCell - dir) if empty.
                var pullDest = Cell - dir;
                if (world.IsBlocked(pullDest)) return;
                if (world.IsPitCell(pullDest) && !RuleBook.IsPitWalkable()) return;
                PushUndo();
                world.TryMoveEntity(occupant.cell, pullDest);
                AfterTurn();
                return;
            }

            if (!RuleBook.CanPush()) return;

            var crateDest = target + dir;
            if (world.IsBlocked(crateDest)) return;
            PushUndo();
            world.TryMoveEntity(occupant.cell, crateDest);
            MovePlayer(target);
            AfterTurn();
            return;
        }

        if (world.IsBlocked(target)) return;

        PushUndo();
        MovePlayer(target);
        AfterTurn();
    }

    void MovePlayer(Vector3Int target)
    {
        Cell = target;
        Snap();
    }

    void Snap()
    {
        if (world == null) world = GridWorld.Instance;
        if (world == null) return;
        transform.position = world.CellToWorld(Cell);
    }

    void AfterTurn()
    {
        if (RuleBook.WallsRemember() && !RuleBook.LightFoot())
            GameManager.Instance?.RememberCell(Cell);

        VisionDimController.Instance?.Apply();

        death?.TickAfterMove();
        if (death != null && death.IsResolving) return;

        var occupant = world.GetEntityAt(Cell);
        if (occupant != null && occupant.IsRed && RuleBook.RedKills())
        {
            death?.Kill();
            return;
        }

        if (world.IsPitCell(Cell) && !RuleBook.IsPitWalkable())
        {
            death?.Kill();
            return;
        }

        if (occupant != null && occupant.IsRed && RuleBook.WinOnRed())
        {
            GameManager.Instance?.OnThresholdReached();
            return;
        }

        if (occupant != null && occupant.IsWin && RuleBook.GlyphWins())
        {
            GameManager.Instance?.OnThresholdReached();
            return;
        }

        if (occupant != null && occupant.IsThreshold)
            GameManager.Instance?.OnThresholdReached();
    }

    void PushUndo()
    {
        var record = new UndoRecord
        {
            playerCell = Cell,
            pushables = new List<(GridEntity, Vector3Int)>(),
            deathQueue = death != null ? death.DeadIn : 0
        };

        if (world != null)
        {
            foreach (var entity in world.AllEntities)
            {
                if (entity != null && entity.IsPushable)
                    record.pushables.Add((entity, entity.cell));
            }
        }

        undo.Add(record);
        if (undo.Count > UndoCap)
            undo.RemoveAt(0);
    }

    void TryUndo()
    {
        if (undo.Count == 0) return;

        var record = undo[undo.Count - 1];
        undo.RemoveAt(undo.Count - 1);

        Cell = record.playerCell;
        Snap();

        if (record.pushables != null && world != null)
        {
            foreach (var pair in record.pushables)
            {
                if (pair.entity == null) continue;
                world.Unregister(pair.entity);
                pair.entity.cell = pair.cell;
                world.Register(pair.entity);
            }
        }

        death?.SetQueue(record.deathQueue);
        VisionDimController.Instance?.Apply();
    }
}
