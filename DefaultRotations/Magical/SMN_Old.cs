﻿namespace DefaultRotations.Magical;

[RotationDesc(ActionID.SearingLight)]
[SourceCode("https://github.com/ArchiDog1998/FFXIVRotations/blob/main/DefaultRotations/Magical/SMN_Old.cs")]

public sealed class SMN_Old : SMN_Base
{
    public override string GameVersion => "6.28";

    public override string RotationName => "Old";

    protected override IRotationConfigSet CreateConfiguration()
    {
        return base.CreateConfiguration()
            .SetCombo("addSwiftcast", 0, "Use Swiftcast", "No", "Emerald", "Ruby", "All")
            .SetCombo("SummonOrder", 0, "Order", "Topaz-Emerald-Ruby", "Topaz-Ruby-Emerald", "Emerald-Topaz-Ruby")
            .SetBool("addCrimsonCyclone", true, "Use Crimson Cyclone");
    }

    protected override bool CanHealSingleSpell => false;

    [RotationDesc(ActionID.CrimsonCyclone)]
    protected override bool MoveForwardGCD(out IAction act)
    {
        //火神突进
        if (CrimsonCyclone.CanUse(out act, CanUseOption.MustUse)) return true;
        return base.MoveForwardGCD(out act);
    }

    protected override bool GeneralGCD(out IAction act)
    {
        //宝石兽召唤
        if (SummonCarbuncle.CanUse(out act)) return true;

        //风神读条
        if (Slipstream.CanUse(out act, CanUseOption.MustUse)) return true;
        //火神冲锋
        if (CrimsonStrike.CanUse(out act, CanUseOption.MustUse)) return true;

        //AOE
        if (PreciousBrilliance.CanUse(out act)) return true;
        //单体
        if (Gemshine.CanUse(out act)) return true;

        if (!IsMoving && Configs.GetBool("addCrimsonCyclone") && CrimsonCyclone.CanUse(out act, CanUseOption.MustUse)) return true;

        //龙神不死鸟
        if ((Player.HasStatus(false, StatusID.SearingLight) || SearingLight.IsCoolingDown) && SummonBahamut.CanUse(out act)) return true;
        if (!SummonBahamut.EnoughLevel && HasHostilesInRange && AetherCharge.CanUse(out act)) return true;

        //毁4
        if (IsMoving && (Player.HasStatus(true, StatusID.GarudasFavor) || InIfrit)
            && !Player.HasStatus(true, StatusID.SwiftCast) && !InBahamut && !InPhoenix
            && RuinIV.CanUse(out act, CanUseOption.MustUse)) return true;

        //召唤蛮神
        switch (Configs.GetCombo("SummonOrder"))
        {
            default:
                //土
                if (SummonTopaz.CanUse(out act)) return true;
                //风
                if (SummonEmerald.CanUse(out act)) return true;
                //火
                if (SummonRuby.CanUse(out act)) return true;
                break;

            case 1:
                //土
                if (SummonTopaz.CanUse(out act)) return true;
                //火
                if (SummonRuby.CanUse(out act)) return true;
                //风
                if (SummonEmerald.CanUse(out act)) return true;
                break;

            case 2:
                //风
                if (SummonEmerald.CanUse(out act)) return true;
                //土
                if (SummonTopaz.CanUse(out act)) return true;
                //火
                if (SummonRuby.CanUse(out act)) return true;
                break;
        }

        if (SummonTimeEndAfterGCD() && AttunmentTimeEndAfterGCD() &&
            !Player.HasStatus(true, StatusID.SwiftCast) && !InBahamut && !InPhoenix &&
            RuinIV.CanUse(out act, CanUseOption.MustUse)) return true;
        //迸裂三灾
        if (Outburst.CanUse(out act)) return true;

        //毁123
        if (Ruin.CanUse(out act)) return true;
        return false;
    }

    protected override bool AttackAbility(out IAction act)
    {
        if (InBurst && !Player.HasStatus(false, StatusID.SearingLight))
        {
            //灼热之光
            if (SearingLight.CanUse(out act)) return true;
        }

        //龙神不死鸟迸发
        if ((InBahamut && SummonBahamut.ElapsedOneChargeAfterGCD(3) || InPhoenix || IsTargetBoss && IsTargetDying) && EnkindleBahamut.CanUse(out act, CanUseOption.MustUse)) return true;
        //死星核爆
        if ((SummonBahamut.ElapsedOneChargeAfterGCD(3) || IsTargetBoss && IsTargetDying) && DeathFlare.CanUse(out act, CanUseOption.MustUse)) return true;
        //苏生之炎
        if (Rekindle.CanUse(out act, CanUseOption.MustUse)) return true;
        //山崩
        if (MountainBuster.CanUse(out act, CanUseOption.MustUse)) return true;

        //痛苦核爆
        if ((Player.HasStatus(false, StatusID.SearingLight) && InBahamut && (SummonBahamut.ElapsedOneChargeAfterGCD(3) || !EnergyDrain.IsCoolingDown) ||
            !SearingLight.EnoughLevel || IsTargetBoss && IsTargetDying) && PainFlare.CanUse(out act)) return true;
        //溃烂爆发
        if ((Player.HasStatus(false, StatusID.SearingLight) && InBahamut && (SummonBahamut.ElapsedOneChargeAfterGCD(3) || !EnergyDrain.IsCoolingDown) ||
            !SearingLight.EnoughLevel || IsTargetBoss && IsTargetDying) && Fester.CanUse(out act)) return true;

        //能量抽取
        if (EnergySiphon.CanUse(out act)) return true;
        //能量吸收
        if (EnergyDrain.CanUse(out act)) return true;

        return false;
    }
    protected override bool EmergencyAbility(IAction nextGCD, out IAction act)
    {
        //即刻进循环
        switch (Configs.GetCombo("addSwiftcast"))
        {
            default:
                break;
            case 1:
                if (nextGCD.IsTheSameTo(true, Slipstream) || Attunement == 0 && Player.HasStatus(true, StatusID.GarudasFavor))
                {
                    if (Swiftcast.CanUse(out act, CanUseOption.MustUse)) return true;
                }
                break;
            case 2:
                if (InIfrit && (nextGCD.IsTheSameTo(true, Gemshine, PreciousBrilliance) || IsMoving))
                {
                    if (Swiftcast.CanUse(out act, CanUseOption.MustUse)) return true;
                }
                break;

            case 3:
                if (nextGCD.IsTheSameTo(true, Slipstream) || Attunement == 0 && Player.HasStatus(true, StatusID.GarudasFavor) ||
                   InIfrit && (nextGCD.IsTheSameTo(true, Gemshine, PreciousBrilliance) || IsMoving))
                {
                    if (Swiftcast.CanUse(out act, CanUseOption.MustUse)) return true;
                }
                break;
        }
        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override IAction CountDownAction(float remainTime)
    {
        if (remainTime <= 30 && SummonCarbuncle.CanUse(out _)) return SummonCarbuncle;
        //1.5s预读毁3
        if (remainTime <= Ruin.CastTime + Service.Config.CountDownAhead
            && Ruin.CanUse(out _)) return Ruin;
        return base.CountDownAction(remainTime);
    }
}
