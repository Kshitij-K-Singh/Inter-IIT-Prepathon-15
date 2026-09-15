using System.Collections.Generic;
using UnityEngine;

public static class RuleBook
{
    static List<RuleInstance> rules;
    static HashSet<Vector3Int> remembered;
    static bool persistRememberedWalls;

    public static void Bind(List<RuleInstance> activeRules, HashSet<Vector3Int> rememberedCells, bool persistWalls)
    {
        rules = activeRules;
        remembered = rememberedCells;
        persistRememberedWalls = persistWalls;
    }

    public static bool Has(RuleKind kind)
    {
        if (rules == null) return false;
        for (int i = 0; i < rules.Count; i++)
        {
            if (rules[i].kind == kind) return true;
        }
        return false;
    }

    public static bool CanFly() => Has(RuleKind.Fly);
    public static bool IsPushPull() => Has(RuleKind.PushPull);
    public static bool CanPush() => !Has(RuleKind.NoPush);
    public static bool CanDash() => IsPushPull() && CanPush();
    public static bool WinOnRed() => Has(RuleKind.WinRed);
    public static bool RedKills() => Has(RuleKind.RedDeadly);
    public static bool GlyphWins() => Has(RuleKind.WinGlyph);
    public static int DeathDelayMoves() => Has(RuleKind.DeathDelay) ? 3 : 0;
    public static float DeathDelaySeconds() => Has(RuleKind.DeathDelay) ? 2f : 0f;
    public static bool VisionDim() => Has(RuleKind.VisionDim);
    public static bool WallsRemember() => Has(RuleKind.WallsRemember);
    public static bool WallsResent() => Has(RuleKind.WallsResent);
    public static bool IsPitWalkable() => CanFly();
    public static bool DeathHidden() => Has(RuleKind.DeathHidden);
    public static bool WinLocked() => Has(RuleKind.WinLocked);
    public static bool LightFoot() => Has(RuleKind.LightFoot);
    public static bool Brittle() => Has(RuleKind.Brittle);

    public static bool IsRememberedWall(Vector3Int cell)
    {
        if (remembered == null || !persistRememberedWalls) return false;
        return remembered.Contains(cell);
    }
}
