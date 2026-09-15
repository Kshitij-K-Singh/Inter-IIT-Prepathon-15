using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[DefaultExecutionOrder(-50)]
public class GridWorld : MonoBehaviour
{
    public static GridWorld Instance { get; private set; }

    [SerializeField] Grid grid;
    [SerializeField] Tilemap collisionTilemap;
    [SerializeField] Tilemap groundTilemap;
    [SerializeField] Tilemap objectsTilemap;
    [SerializeField] Tilemap rememberedTilemap;
    [SerializeField] TileBase pitTile;
    [SerializeField] TileBase rememberedWallTile;

    readonly Dictionary<Vector3Int, GridEntity> entities = new Dictionary<Vector3Int, GridEntity>();
    readonly HashSet<Vector3Int> pitCells = new HashSet<Vector3Int>();

    public Grid Grid => grid;
    public Tilemap GroundTilemap => groundTilemap;
    public Tilemap CollisionTilemap => collisionTilemap;
    public Tilemap ObjectsTilemap => objectsTilemap;
    public Tilemap RememberedTilemap => rememberedTilemap;

    void Awake()
    {
        Instance = this;
    }

    public void Bind(
        Grid gridComponent,
        Tilemap collision,
        Tilemap ground,
        Tilemap objects,
        Tilemap remembered,
        TileBase pit,
        TileBase rememberedWall)
    {
        grid = gridComponent;
        collisionTilemap = collision;
        groundTilemap = ground;
        objectsTilemap = objects;
        rememberedTilemap = remembered;
        pitTile = pit;
        rememberedWallTile = rememberedWall;
    }

    public Vector3Int WorldToCell(Vector3 worldPosition) => grid.WorldToCell(worldPosition);

    public Vector3 CellToWorld(Vector3Int cell) => grid.GetCellCenterWorld(cell);

    public bool HasPitTerrain(Vector3Int cell)
    {
        if (pitCells.Contains(cell)) return true;
        if (pitTile == null) return false;
        if (objectsTilemap != null && objectsTilemap.GetTile(cell) == pitTile) return true;
        if (groundTilemap != null && groundTilemap.GetTile(cell) == pitTile) return true;
        return false;
    }

    public bool IsPitCell(Vector3Int cell)
    {
        if (!HasPitTerrain(cell)) return false;
        var cover = GetEntityAt(cell);
        return cover == null || !cover.IsPushable;
    }

    public bool IsBlocked(Vector3Int cell)
    {
        if (collisionTilemap != null && collisionTilemap.HasTile(cell)) return true;
        if (RuleBook.IsRememberedWall(cell)) return true;

        var entity = GetEntityAt(cell);
        if (entity == null) return false;
        if (entity.IsWall) return true;
        if (entity.IsPushable) return !HasPitTerrain(cell);
        return false;
    }

    public GridEntity GetEntityAt(Vector3Int cell)
    {
        entities.TryGetValue(cell, out var entity);
        return entity;
    }

    public bool TryMoveEntity(Vector3Int from, Vector3Int to)
    {
        if (!entities.TryGetValue(from, out var entity)) return false;
        if (from != to && IsBlocked(to)) return false;

        entities.Remove(from);
        entity.cell = to;
        entities[to] = entity;
        entity.SnapToGrid(grid);
        return true;
    }

    public void Register(GridEntity entity)
    {
        if (entity == null) return;
        entity.SnapToGrid(grid);
        if (entity.IsPit)
        {
            pitCells.Add(entity.cell);
            return;
        }
        entities[entity.cell] = entity;
    }

    public void Unregister(GridEntity entity)
    {
        if (entity == null) return;
        if (entities.TryGetValue(entity.cell, out var current) && current == entity)
            entities.Remove(entity.cell);
    }

    public void ClearEntities()
    {
        entities.Clear();
        pitCells.Clear();
    }

    public IEnumerable<GridEntity> AllEntities => entities.Values;

    public void RefreshRememberedVisuals(IEnumerable<Vector3Int> cells, bool persist)
    {
        if (rememberedTilemap == null) return;
        rememberedTilemap.ClearAllTiles();
        if (!persist || rememberedWallTile == null || cells == null) return;

        foreach (var cell in cells)
            rememberedTilemap.SetTile(cell, rememberedWallTile);
    }
}
