namespace DefaultRotations.Healer;

[RotationDesc(ActionID.Divination)]
[SourceCode("https://github.com/ArchiDog1998/FFXIVRotations/blob/main/DefaultRotations/Healer/AST_Default.cs")]
public sealed class AST_Default : AST_Base
{
    public override string GameVersion => "6.28";

    public override string RotationName => "Default";

    protected override IRotationConfigSet CreateConfiguration()
        => base.CreateConfiguration()
            .SetFloat("UseEarthlyStarTime", 15, "Use Earthly Star during countdown timer.", 4, 20);

    static IBaseAction AspectedBeneficDefense { get; } = new BaseAction(ActionID.AspectedBenefic, ActionOption.Hot)
    {
        ChoiceTarget = TargetFilter.FindAttackedTarget,
        ActionCheck = (b, m) => b.IsJobCategory(JobRole.Tank),
        TargetStatus = new StatusID[] { StatusID.AspectedBenefic },
    };

    protected override IAction CountDownAction(float remainTime)
    {
        if (remainTime < Malefic.CastTime + Service.Config.CountDownAhead
            && Malefic.CanUse(out var act, CanUseOption.IgnoreClippingCheck)) return act;
        if (remainTime < 3 && UseBurstMedicine(out act)) return act;
        if (remainTime < 4 && AspectedBeneficDefense.CanUse(out act, CanUseOption.IgnoreClippingCheck)) return act;
        if (remainTime < Configs.GetFloat("UseEarthlyStarTime")
            && EarthlyStar.CanUse(out act, CanUseOption.IgnoreClippingCheck)) return act;
        if (remainTime < 30 && Draw.CanUse(out act, CanUseOption.IgnoreClippingCheck)) return act;

        return base.CountDownAction(remainTime);
    }

    [RotationDesc(ActionID.CelestialIntersection, ActionID.Exaltation)]
    protected override bool DefenseSingleAbility(out IAction act)
    {
        //天星交错
        if (CelestialIntersection.CanUse(out act, CanUseOption.EmptyOrSkipCombo)) return true;

        //给T减伤，这个很重要。
        if (Exaltation.CanUse(out act)) return true;
        return false;
    }

    [RotationDesc(ActionID.CollectiveUnconscious)]
    protected override bool DefenseAreaAbility(out IAction act)
    {
        //来个命运之轮
        if (CollectiveUnconscious.CanUse(out act)) return true;

        return base.DefenseAreaAbility(out act);
    }

    protected override bool GeneralGCD(out IAction act)
    {
        //Add AspectedBeneficwhen not in combat.
        if (NotInCombatDelay && AspectedBeneficDefense.CanUse(out act)) return true;

        //群体输出
        if (Gravity.CanUse(out act)) return true;

        //单体输出
        if (Combust.CanUse(out act)) return true;
        if (Malefic.CanUse(out act)) return true;
        if (Combust.CanUse(out act, CanUseOption.MustUse)) return true;

        act = null!;
        return false;
    }

    [RotationDesc(ActionID.AspectedHelios, ActionID.Helios)]
    protected override bool HealAreaGCD(out IAction act)
    {
        //阳星相位
        if (AspectedHelios.CanUse(out act)) return true;

        //阳星
        if (Helios.CanUse(out act)) return true;

        act = null!;
        return false;
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction act)
    {
        if (base.EmergencyAbility(nextGCD, out act)) return true;

        if (PartyHealers.Count() == 1 && Player.HasStatus(false, StatusID.Silence)
            && HasHostilesInRange && EchoDrops.CanUse(out act)) return true;

        if (!InCombat) return false;

        //如果要群奶了，先上个天宫图！
        if (nextGCD.IsTheSameTo(true, AspectedHelios, Helios))
        {
            if (Horoscope.CanUse(out act)) return true;

            //中间学派
            if (NeutralSect.CanUse(out act)) return true;
        }

        //如果要单奶了，先上星位合图！
        if (nextGCD.IsTheSameTo(true, Benefic, Benefic2, AspectedBenefic))
        {
            if (Synastry.CanUse(out act)) return true;
        }
        return false;
    }

    protected override bool GeneralAbility(out IAction act)
    {
        //如果当前还没有卡牌，那就抽一张
        if (Draw.CanUse(out act)) return true;

        //如果当前卡牌已经拥有了，就重抽
        if (Redraw.CanUse(out act)) return true;

        act = null;
        return false;
    }

    [RotationDesc(ActionID.AspectedBenefic, ActionID.Benefic2, ActionID.Benefic)]
    protected override bool HealSingleGCD(out IAction act)
    {
        //吉星相位
        if (AspectedBenefic.CanUse(out act)
            && (IsMoving || AspectedBenefic.Target.GetHealthRatio() > 0.4)) return true;

        //福星
        if (Benefic2.CanUse(out act)) return true;

        //吉星
        if (Benefic.CanUse(out act)) return true;

        act = null;
        return false;
    }

    protected override bool AttackAbility(out IAction act)
    {
        if (InBurst && !IsMoving && Divination.CanUse(out act)) return true;

        //如果当前还没有皇冠卡牌，那就抽一张
        if (MinorArcana.CanUse(out act, CanUseOption.EmptyOrSkipCombo)) return true;

        //如果当前还没有卡牌，那就抽一张
        if (Draw.CanUse(out act, InBurst ? CanUseOption.EmptyOrSkipCombo : CanUseOption.None)) return true;

        //光速，创造更多的内插能力技的机会。
        if (IsMoving && Lightspeed.CanUse(out act)) return true;


        if (!IsMoving)
        {
            //如果没有地星也没有巨星，那就试试看能不能放个。
            if (!Player.HasStatus(true, StatusID.EarthlyDominance, StatusID.GiantDominance))
            {
                if (EarthlyStar.CanUse(out act, CanUseOption.MustUse)) return true;
            }
            //加星星的进攻Buff
            if (Astrodyne.CanUse(out act)) return true;
        }

        if (DrawnCrownCard == CardType.LORD || MinorArcana.WillHaveOneChargeGCD(1, 0))
        {
            //进攻牌，随便发。或者CD要转好了，赶紧发掉。
            if (MinorArcana.CanUse(out act, CanUseOption.MustUse)) return true;
        }

        //发牌
        if (Redraw.CanUse(out act)) return true;
        if (PlayCard(out act)) return true;

        return false;
    }

    [RotationDesc(ActionID.EssentialDignity, ActionID.CelestialIntersection, ActionID.CelestialOpposition,
        ActionID.EarthlyStar, ActionID.Horoscope)]
    protected override bool HealSingleAbility(out IAction act)
    {
        //常规奶
        if (EssentialDignity.CanUse(out act)) return true;
        //带盾奶
        if (CelestialIntersection.CanUse(out act, CanUseOption.EmptyOrSkipCombo)) return true;

        //奶量牌，要看情况。
        if (DrawnCrownCard == CardType.LADY && MinorArcana.CanUse(out act, CanUseOption.MustUse)) return true;

        var tank = PartyTanks;
        var isBoss = Malefic.IsTargetBoss;
        if (EssentialDignity.IsCoolingDown && tank.Count() == 1 && tank.Any(t => t.GetHealthRatio() < 0.5) && !isBoss)
        {
            //群Hot
            if (CelestialOpposition.CanUse(out act)) return true;

            //如果有巨星主宰
            if (Player.HasStatus(true, StatusID.GiantDominance))
            {
                //需要回血的时候炸了。
                act = EarthlyStar;
                return true;
            }

            //天宫图
            if (!Player.HasStatus(true, StatusID.HoroscopeHelios, StatusID.Horoscope) && Horoscope.CanUse(out act)) return true;
            //阳星天宫图
            if (Player.HasStatus(true, StatusID.HoroscopeHelios) && Horoscope.CanUse(out act)) return true;
            //超紧急情况天宫图
            if (tank.Any(t => t.GetHealthRatio() < 0.3) && Horoscope.CanUse(out act)) return true;
        }

        return base.HealSingleAbility(out act);
    }

    [RotationDesc(ActionID.CelestialOpposition, ActionID.EarthlyStar, ActionID.Horoscope)]
    protected override bool HealAreaAbility(out IAction act)
    {
        //群Hot
        if (CelestialOpposition.CanUse(out act)) return true;

        //如果有巨星主宰
        if (Player.HasStatus(true, StatusID.GiantDominance))
        {
            //需要回血的时候炸了。
            act = EarthlyStar;
            return true;
        }

        //天宫图
        if (Player.HasStatus(true, StatusID.HoroscopeHelios) && Horoscope.CanUse(out act)) return true;

        //奶量牌，要看情况。
        if (DrawnCrownCard == CardType.LADY && MinorArcana.CanUse(out act, CanUseOption.MustUse)) return true;

        return false;
    }
}
