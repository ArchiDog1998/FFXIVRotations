namespace DefaultRotations.Melee;

[Rotation("Default", CombatType.Both, GameVersion = "6.28")]
[SourceCode(Path = "main/DefaultRotations/Melee/SAM_Default.cs")]
public sealed class SAM_Default : SamuraiRotation
{
    [UI("Use Kenki above.")]
    [Range(0, 85, ConfigUnitType.None, 5)]
    [RotationConfig(CombatType.PvE)]
    public int AddKenki { get; set; } = 50;

    private static bool HaveMeikyoShisui => Player.HasStatus(true, StatusID.MeikyoShisui);

    protected override bool GeneralGCD(out IAction? act)
    {
        #region PvP
        if (KashaPvP.CanUse(out act)) return true;
        if (GekkoPvP.CanUse(out act)) return true;
        if (YukikazePvP.CanUse(out act)) return true;
        #endregion

        if (KaeshiNamikiriPvE.CanUse(out act, skipAoeCheck: true)) return true;

        var IsTargetBoss = HostileTarget?.IsBossFromTTK() ?? false;
        var IsTargetDying = HostileTarget?.IsDying() ?? false;

        if (KaeshiGokenPvE.CanUse(out act, skipAoeCheck: true)) return true;
        if (KaeshiSetsugekkaPvE.CanUse(out act, skipAoeCheck: true)) return true;

        if ((!IsTargetBoss || (HostileTarget?.HasStatus(true, StatusID.Higanbana) ?? false)) && HasMoon && HasFlower
            && OgiNamikiriPvEReplace.CanUse(out act, skipAoeCheck: true)) return true;

        if (SenCount == 1 && IsTargetBoss && !IsTargetDying)
        {
            if (HasMoon && HasFlower && HiganbanaPvE.CanUse(out act)) return true;
        }
        if (SenCount == 2)
        {
            if (TenkaGokenPvE.CanUse(out act, skipAoeCheck: !MidareSetsugekkaPvE.EnoughLevel)) return true;
        }
        if (SenCount == 3)
        {
            if (MidareSetsugekkaPvE.CanUse(out act)) return true;
        }

        if ((!HasMoon || IsMoonTimeLessThanFlower || !OkaPvE.EnoughLevel) && MangetsuPvE.CanUse(out act, skipAoeCheck : HaveMeikyoShisui && !HasGetsu)) return true;
        if ((!HasFlower || !IsMoonTimeLessThanFlower) && OkaPvE.CanUse(out act, skipAoeCheck: HaveMeikyoShisui && !HasKa)) return true;

        if (!HasSetsu && YukikazePvE.CanUse(out act, skipAoeCheck: HaveMeikyoShisui && HasGetsu && HasKa && !HasSetsu)) return true;

        if (GekkoPvE.CanUse(out act, skipComboCheck: HaveMeikyoShisui && !HasGetsu)) return true;
        if (KashaPvE.CanUse(out act, skipComboCheck: HaveMeikyoShisui && !HasKa)) return true;

        if ((!HasMoon || IsMoonTimeLessThanFlower || !ShifuPvE.EnoughLevel) && JinpuPvE.CanUse(out act)) return true;
        if ((!HasFlower || !IsMoonTimeLessThanFlower) && ShifuPvE.CanUse(out act)) return true;

        if (FukoPvE.CanUse(out act)) return true;
        if (!FukoPvE.EnoughLevel && FugaPvEReplace.CanUse(out act)) return true;

        if (!HaveMeikyoShisui)
        {
            if (HakazePvEReplace.CanUse(out act)) return true;

            if (EnpiPvE.CanUse(out act)) return true;
        }

        return base.GeneralGCD(out act);
    }

    protected override bool AttackAbility(out IAction? act)
    {
        var IsTargetBoss = HostileTarget?.IsBossFromTTK() ?? false;
        var IsTargetDying = HostileTarget?.IsDying() ?? false;

        if (Kenki <= 50 && IkishotenPvEReplace.CanUse(out act)) return true;

        if ((HostileTarget?.HasStatus(true, StatusID.Higanbana) ?? false) && (HostileTarget?.WillStatusEnd(32, true, StatusID.Higanbana) ?? false) && !(HostileTarget?.WillStatusEnd(28, true, StatusID.Higanbana) ?? false) && SenCount == 1 && IsLastAction(true, YukikazePvE) && !HaveMeikyoShisui)
        {
            if (HagakurePvE.CanUse(out act)) return true;
        }

        if (HasMoon && HasFlower)
        {
            if (HissatsuGurenPvE.CanUse(out act, skipAoeCheck: !HissatsuSeneiPvE.EnoughLevel)) return true;
            if (HissatsuSeneiPvE.CanUse(out act)) return true;
        }

        //if (ShohaIiPvE.CanUse(out act)) return true;
        if (ShohaPvE.CanUse(out act)) return true;

        if (Kenki >= 50 && IkishotenPvE.CD.WillHaveOneCharge(10) || Kenki >= AddKenki || IsTargetBoss && IsTargetDying)
        {
            if (HissatsuKyutenPvE.CanUse(out act)) return true;
            if (HissatsuShintenPvE.CanUse(out act)) return true;
        }

        return base.AttackAbility(out act);
    }
    protected override bool EmergencyAbility(IAction nextGCD, out IAction? act)
    {
        var IsTargetBoss = HostileTarget?.IsBossFromTTK() ?? false;
        var IsTargetDying = HostileTarget?.IsDying() ?? false;

        if (HasHostilesInRange && IsLastGCD(true, YukikazePvE, MangetsuPvE, OkaPvE) &&
            (!IsTargetBoss || (HostileTarget?.HasStatus(true, StatusID.Higanbana) ?? false) && !(HostileTarget?.WillStatusEnd(40, true, StatusID.Higanbana) ?? false) || !HasMoon && !HasFlower || IsTargetBoss && IsTargetDying))
        {
            if (MeikyoShisuiPvE.CanUse(out act, usedUp: true)) return true;
        }
        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override IAction? CountDownAction(float remainTime)
    {
        if (remainTime <= 5 && MeikyoShisuiPvE.CanUse(out var act)) return act;
        if (remainTime <= 2 && TrueNorthPvE.CanUse(out act)) return act;
        return base.CountDownAction(remainTime);
    }
}