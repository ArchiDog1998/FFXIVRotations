﻿namespace DefaultRotations.Melee;

[SourceCode("https://github.com/ArchiDog1998/FFXIVRotations/blob/main/DefaultRotations/Melee/RPR_Old.cs")]
public sealed class RPR_Old : RPR_Base
{
    public override string GameVersion => "6.28";

    public override string RotationName => "Old";

    protected override IRotationConfigSet CreateConfiguration()
    {
        return base.CreateConfiguration().SetBool("EnshroudPooling", false, "Enshroud Pooling");
    }

    protected override IAction CountDownAction(float remainTime)
    {
        //倒数收获月
        if (remainTime <= 30 && SoulSow.CanUse(out _)) return SoulSow;
        //提前2s勾刃
        if (remainTime <= Harpe.CastTime + Service.Config.CountDownAhead
            && Harpe.CanUse(out _)) return Harpe;
        return base.CountDownAction(remainTime);
    }

    protected override bool GeneralGCD(out IAction act)
    {
        //非战斗收获月
        if (SoulSow.CanUse(out act)) return true;

        //上Debuff
        if (WhorlOfDeath.CanUse(out act)) return true;
        if (ShadowOfDeath.CanUse(out act)) return true;

        //补蓝
        if (HasSoulReaver)
        {
            if (Guillotine.CanUse(out act)) return true;
            if (Player.HasStatus(true, StatusID.EnhancedGibbet))
            {
                if (Gibbet.CanUse(out act)) return true;
            }
            else
            {
                if (Gallows.CanUse(out act)) return true;
            }
        }

        //夜游魂变身状态
        if (HasEnshrouded)
        {
            //补DoT
            if (ShadowOfDeath.CanUse(out act)) return true;

            if (LemureShroud > 1)
            {
                if (Configs.GetBool("EnshroudPooling") && PlentifulHarvest.EnoughLevel && ArcaneCircle.WillHaveOneCharge(9) &&
                   (LemureShroud == 4 && Target.WillStatusEnd(30, true, StatusID.DeathsDesign) || LemureShroud == 3 && Target.WillStatusEnd(50, true, StatusID.DeathsDesign))) //双附体窗口期 
                {
                    if (ShadowOfDeath.CanUse(out act, CanUseOption.MustUse)) return true;
                }

                //夜游魂衣-虚无/交错收割 阴冷收割
                if (GrimReaping.CanUse(out act)) return true;
                if (Player.HasStatus(true, StatusID.EnhancedCrossReaping) || !Player.HasStatus(true, StatusID.EnhancedVoidReaping))
                {
                    if (CrossReaping.CanUse(out act)) return true;
                }
                else
                {
                    if (VoidReaping.CanUse(out act)) return true;
                }
            }
            if (LemureShroud == 1)
            {
                if (Communio.EnoughLevel)
                {
                    if (!IsMoving && Communio.CanUse(out act, CanUseOption.MustUse))
                    {
                        return true;
                    }
                    //跑机制来不及读条？补个buff混一下
                    else
                    {
                        if (ShadowOfDeath.CanUse(out act, IsMoving ? CanUseOption.MustUse : CanUseOption.None)) return true;
                    }
                }
                else
                {
                    //夜游魂衣-虚无/交错收割 阴冷收割
                    if (GrimReaping.CanUse(out act)) return true;
                    if (Player.HasStatus(true, StatusID.EnhancedCrossReaping) || !Player.HasStatus(true, StatusID.EnhancedVoidReaping))
                    {
                        if (CrossReaping.CanUse(out act)) return true;
                    }
                    else
                    {
                        if (VoidReaping.CanUse(out act)) return true;
                    }
                }
            }
        }

        //大丰收
        if (PlentifulHarvest.CanUse(out act, CanUseOption.MustUse)) return true;

        //灵魂钐割
        if (SoulScythe.CanUse(out act, CanUseOption.EmptyOrSkipCombo)) return true;
        //灵魂切割
        if (SoulSlice.CanUse(out act, CanUseOption.EmptyOrSkipCombo)) return true;

        //群体二连
        if (NightmareScythe.CanUse(out act)) return true;
        if (SpinningScythe.CanUse(out act)) return true;

        //单体三连
        if (InfernalSlice.CanUse(out act)) return true;
        if (WaxingSlice.CanUse(out act)) return true;
        if (Slice.CanUse(out act)) return true;

        //摸不到怪 先花掉收获月
        if (InCombat && HarvestMoon.CanUse(out act, CanUseOption.MustUse)) return true;
        if (Harpe.CanUse(out act)) return true;

        return false;
    }

    protected override bool AttackAbility(out IAction act)
    {
        if (InBurst)
        {
            //神秘环
            if (Target.HasStatus(true, StatusID.DeathsDesign) && ArcaneCircle.CanUse(out act)) return true;

            if (IsTargetBoss && IsTargetDying || //资源倾泻
               !Configs.GetBool("EnshroudPooling") && Shroud >= 50 ||//未开启双附体
               Configs.GetBool("EnshroudPooling") && Shroud >= 50 &&
               (!PlentifulHarvest.EnoughLevel || //等级不足以双附体
               Player.HasStatus(true, StatusID.ArcaneCircle) || //在神秘环期间附体
               ArcaneCircle.WillHaveOneCharge(8) || //双附体起手
               !Player.HasStatus(true, StatusID.ArcaneCircle) && ArcaneCircle.WillHaveOneCharge(65) && !ArcaneCircle.WillHaveOneCharge(50) || //奇数分钟不用攒附体
               !Player.HasStatus(true, StatusID.ArcaneCircle) && Shroud >= 90)) //攒蓝条为双附体
            {
                //夜游魂衣
                if (Enshroud.CanUse(out act)) return true;
            }
        }
        if (HasEnshrouded)
        {
            //夜游魂衣-夜游魂切割 夜游魂钐割
            if (LemuresScythe.CanUse(out act, CanUseOption.EmptyOrSkipCombo)) return true;
            if (LemuresSlice.CanUse(out act, CanUseOption.EmptyOrSkipCombo)) return true;
        }

        //暴食
        //大丰收期间延后暴食
        if (PlentifulHarvest.EnoughLevel && !Player.HasStatus(true, StatusID.ImmortalSacrifice) && !Player.HasStatus(true, StatusID.BloodSownCircle) || !PlentifulHarvest.EnoughLevel)
        {
            if (Gluttony.CanUse(out act, CanUseOption.MustUse)) return true;
        }

        if (!Player.HasStatus(true, StatusID.BloodSownCircle) && !Player.HasStatus(true, StatusID.ImmortalSacrifice) && (Gluttony.EnoughLevel && !Gluttony.WillHaveOneChargeGCD(4) || !Gluttony.EnoughLevel || Soul == 100))
        {
            //AOE
            if (GrimSwathe.CanUse(out act)) return true;
            //单体
            if (BloodStalk.CanUse(out act)) return true;
        }

        act = null;
        return false;
    }
}