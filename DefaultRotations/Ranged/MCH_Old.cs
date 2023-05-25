namespace DefaultRotations.Ranged;

[SourceCode("https://github.com/ArchiDog1998/FFXIVRotations/blob/main/DefaultRotations/Ranged/MCH_Old.cs")]
public sealed class MCH_Old : MCH_Base
{
    public override string GameVersion => "6.28";

    public override string RotationName => "Old";


    /// <summary>
    /// 4人本小怪快死了
    /// </summary>
    private static bool isDyingNotBoss => !Target.IsBoss() && IsTargetDying && PartyMembers.Count() is > 1 and <= 4;

    protected override IRotationConfigSet CreateConfiguration()
    {
        return base.CreateConfiguration()
            .SetBool("MCH_Opener", true, "Basic Opener")
            .SetBool("MCH_Automaton", true, "Care for Automation")
            .SetBool("MCH_Reassemble", true, "Ressamble for ChainSaw")
            .SetBool("DelayHypercharge", false, "Use Hypercharge late");
    }

    protected override bool GeneralGCD(out IAction act)
    {
        //不在战斗中时重置起手
        if (!InCombat)
        {
            //开场前整备,空气锚和钻头必须冷却好
            if (AirAnchor.EnoughLevel && (!AirAnchor.IsCoolingDown || !Drill.IsCoolingDown) && Reassemble.CanUse(out act, CanUseOption.EmptyOrSkipCombo)) return true;
        }

        //群体常规GCD
        //AOE,毒菌冲击
        if (BioBlaster.CanUse(out act)) return true;
        if (ChainSaw.CanUse(out act)) return true;
        if (IsOverheated && AutoCrossbow.CanUse(out act)) return true;
        if (SpreadShot.CanUse(out act)) return true;

        if (!IsOverheated || IsOverheated && OverheatedEndAfterGCD())
        {
            //单体,四个牛逼的技能。先空气锚再钻头
            if (AirAnchor.CanUse(out act)) return true;
            else if (!AirAnchor.EnoughLevel && HotShot.CanUse(out act)) return true;
            if (Drill.CanUse(out act)) return true;
            if (ChainSaw.CanUse(out act, CanUseOption.MustUse))
            {
                if (Player.HasStatus(true, StatusID.Reassemble)) return true;
                if (!Configs.GetBool("MCH_Opener") || Wildfire.IsCoolingDown) return true;
                if (AirAnchor.IsCoolingDown && AirAnchor.ElapsedOneChargeAfterGCD(4) && Drill.IsCoolingDown && Drill.ElapsedOneChargeAfterGCD(3)) return true;
                if (AirAnchor.IsCoolingDown && AirAnchor.ElapsedOneChargeAfterGCD(3) && Drill.IsCoolingDown && Drill.ElapsedOneChargeAfterGCD(4)) return true;
            }
        }

        //过热状态
        if (IsOverheated && HeatBlast.CanUse(out act)) return true;

        //单体常规GCD
        if (CleanShot.CanUse(out act)) return true;
        if (SlugShot.CanUse(out act)) return true;
        if (SplitShot.CanUse(out act)) return true;

        return false;
    }

    protected override IAction CountDownAction(float remainTime)
    {
        //提前5秒整备
        if (remainTime <= 5 && Reassemble.CanUse(out _, CanUseOption.EmptyOrSkipCombo)) return Reassemble;
        return base.CountDownAction(remainTime);
    }
    protected override bool EmergencyAbility(IAction nextGCD, out IAction act)
    {
        //等级小于钻头时,绑定狙击弹
        if (!Drill.EnoughLevel && nextGCD.IsTheSameTo(true, CleanShot))
        {
            if (Reassemble.CanUse(out act, CanUseOption.EmptyOrSkipCombo)) return true;
        }
        //等级小于90时,整备不再留层数
        if ((!ChainSaw.EnoughLevel || !Configs.GetBool("MCH_Reassemble"))
            && nextGCD.IsTheSameTo(false, AirAnchor, Drill))
        {
            if (Reassemble.CanUse(out act, CanUseOption.EmptyOrSkipCombo)) return true;
        }
        //整备优先链锯
        if (Configs.GetBool("MCH_Reassemble") && nextGCD.IsTheSameTo(true, ChainSaw))
        {
            if (Reassemble.CanUse(out act, CanUseOption.EmptyOrSkipCombo)) return true;
        }
        //如果接下来要搞三大金刚了，整备吧！
        if (ChainSaw.EnoughLevel && nextGCD.IsTheSameTo(true, AirAnchor, Drill))
        {
            if (Reassemble.CanUse(out act)) return true;
        }
        //起手在链锯前释放野火
        if (nextGCD.IsTheSameTo(true, ChainSaw) && !IsLastGCD(true, HeatBlast))
        {
            if (InBurst && Configs.GetBool("MCH_Opener") && Wildfire.CanUse(out act)) return true;
        }
        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(out IAction act)
    {
        //野火
        if (InBurst && CanUseWildfire(out act)) return true;

        //车式浮空炮塔
        if (CanUseRookAutoturret(out act)) return true;

        //起手虹吸弹、弹射
        if (Ricochet.CurrentCharges == Ricochet.MaxCharges && Ricochet.CanUse(out act, CanUseOption.MustUse | CanUseOption.EmptyOrSkipCombo)) return true;
        if (GaussRound.CurrentCharges == GaussRound.MaxCharges && GaussRound.CanUse(out act, CanUseOption.MustUse | CanUseOption.EmptyOrSkipCombo)) return true;

        //枪管加热
        if (BarrelStabilizer.CanUse(out act)) return true;

        //超荷
        if (CanUseHypercharge(out act)) return true;

        if (GaussRound.CurrentCharges <= Ricochet.CurrentCharges)
        {
            //弹射
            if (Ricochet.CanUse(out act, CanUseOption.MustUse | CanUseOption.EmptyOrSkipCombo)) return true;
        }
        //虹吸弹
        if (GaussRound.CanUse(out act, CanUseOption.MustUse | CanUseOption.EmptyOrSkipCombo)) return true;

        act = null!;
        return false;
    }

    /// <summary>
    /// 判断能否使用野火
    /// </summary>
    /// <param name="act"></param>
    /// <returns></returns>
    private bool CanUseWildfire(out IAction act)
    {
        if (!Wildfire.CanUse(out act)) return false;

        if (Heat < 50 && !IsOverheated) return false;

        //小怪和AOE期间不打野火
        if (SpreadShot.CanUse(out _) || PartyMembers.Count() is > 1 and <= 4 && !Target.IsBoss()) return false;

        //在过热时
        if (IsLastAction(true, Hypercharge)) return true;

        if (ChainSaw.EnoughLevel && !ChainSaw.IsCoolingDown) return false;

        if (Hypercharge.IsCoolingDown) return false;

        //当上一个技能是钻头,空气锚,热冲击时不释放野火
        if (IsLastGCD(true, Drill, HeatBlast, AirAnchor)) return false;

        return true;
    }

    /// <summary>
    /// 判断能否使用超荷
    /// </summary>
    /// <param name="act"></param>
    /// <returns></returns>
    private bool CanUseHypercharge(out IAction act)
    {
        if (!Hypercharge.CanUse(out act, CanUseOption.OnLastAbility) || Player.HasStatus(true, StatusID.Reassemble)) return false;

        //有野火buff必须释放超荷
        if (Player.HasStatus(true, StatusID.Wildfire)) return true;

        //4人本小怪快死了不释放
        //if (isDyingNotBoss) return false;

        //在三大金刚还剩8秒冷却好时不释放超荷
        if (Drill.EnoughLevel && Drill.WillHaveOneChargeGCD(3)) return false;
        if (AirAnchor.EnoughLevel && AirAnchor.WillHaveOneCharge(3)) return false;
        if (ChainSaw.EnoughLevel && (ChainSaw.IsCoolingDown && ChainSaw.WillHaveOneCharge(3) || !ChainSaw.IsCoolingDown) && Configs.GetBool("MCH_Opener")) return false;

        //小怪AOE和4人本超荷判断
        if (SpreadShot.CanUse(out _))
        {
            if (!AutoCrossbow.EnoughLevel) return false;
            return true;
        }

        //等级低于野火
        if (!Wildfire.EnoughLevel) return true;

        //野火前攒热量
        if (!Wildfire.WillHaveOneChargeGCD(5) && Wildfire.WillHaveOneChargeGCD(18))
        {
            //如果期间热量溢出超过5,就释放一次超荷
            if (IsLastGCD((ActionID)Drill.ID) && Heat >= 85) return true;
            return false;
        }
        else return true;
    }

    /// <summary>
    /// 判断能否使用机器人
    /// </summary>
    /// <param name="act"></param>
    /// <returns></returns>
    private bool CanUseRookAutoturret(out IAction act)
    {
        if (!RookAutoturret.CanUse(out act, CanUseOption.MustUse)) return false;

        //4人本小怪快死了不释放
        if (isDyingNotBoss) return false;

        //如果上一个技能是野火不释放
        if (IsLastAction((ActionID)Wildfire.ID)) return false;

        //电量等于100,强制释放
        if (Battery == 100 && ChainSaw.EnoughLevel && !ChainSaw.WillHaveOneCharge(13)) return true;

        //小怪,AOE,不吃团辅判断
        if (!Configs.GetBool("MCH_Automaton") || !Target.IsBoss() && !IsMoving || !Wildfire.EnoughLevel) return true;
        if (SpreadShot.CanUse(out _) && !Target.IsBoss() && IsMoving) return false;

        //机器人吃团辅判断
        if (AirAnchor.IsCoolingDown && AirAnchor.WillHaveOneChargeGCD() && Battery > 80) return true;
        if (ChainSaw.WillHaveOneCharge(4) || ChainSaw.IsCoolingDown && !ChainSaw.ElapsedOneChargeAfterGCD(3) && Battery <= 60) return true;

        return false;
    }
}
