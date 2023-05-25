﻿namespace DefaultRotations.Magical;

[SourceCode("https://github.com/ArchiDog1998/FFXIVRotations/blob/main/DefaultRotations/Magical/BLU_Default.cs")]
public sealed class BLU_Default : BLU_Base
{
    public override string GameVersion => "6.18";

    public override string RotationName => "Default";

    protected override bool CanHealAreaSpell => base.CanHealAreaSpell && BlueId == BLUID.Healer;
    protected override bool CanHealSingleSpell => base.CanHealSingleSpell && BlueId == BLUID.Healer;

    protected override IRotationConfigSet CreateConfiguration()
    {
        return base.CreateConfiguration()
            .SetBool("MoonFluteBreak", false, "Use Moon Flute")
            .SetBool("SingleAOE", true, "Use high-damage AoE skills on single target")
            .SetBool("GamblerKill", false, "Use skills with a chance to fail")
            .SetBool("UseFinalSting", false, "Use Final Sting")
            .SetFloat("FinalStingHP", 0, "Target HPP for Final Sting");
    }

    private bool MoonFluteBreak => Configs.GetBool("MoonFluteBreak");
    private bool UseFinalSting => Configs.GetBool("UseFinalSting");
    private float FinalStingHP => Configs.GetFloat("FinalStingHP");
    /// <summary>
    /// 0-70练级,快速练级,滑舌拉怪
    /// </summary>
    private bool QuickLevel => false;
    /// <summary>
    /// 赌几率秒杀
    /// </summary>
    private bool GamblerKill => Configs.GetBool("GamblerKill");
    /// <summary>
    /// 单体时是否释放高伤害AOE
    /// </summary>
    private bool SingleAOE => Configs.GetBool("SingleAOE");

    protected override bool AttackAbility(out IAction act)
    {
        act = null;
        return false;
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction act)
    {
        if (nextGCD.IsTheSameTo(false, SelfDestruct, FinalSting))
        {
            if (Swiftcast.CanUse(out act)) return true;
        }
        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool GeneralGCD(out IAction act)
    {
        act = null;
        //狂战士副作用期间
        if (Player.HasStatus(true, StatusID.WaningNocturne)) return false;
        //鬼宿脚
        if (PhantomFlurry.IsCoolingDown && !PhantomFlurry.ElapsedOneChargeAfter(1) || Player.HasStatus(true, StatusID.PhantomFlurry))
        {
            if (!Player.WillStatusEnd(0.1f, true, StatusID.PhantomFlurry) && Player.WillStatusEnd(1, true, StatusID.PhantomFlurry) && PhantomFlurry2.CanUse(out act, CanUseOption.MustUse)) return true;
            return false;
        }
        //穿甲散弹
        if (Player.HasStatus(true, StatusID.SurpanakhaFury))
        {
            if (Surpanakha.CanUse(out act, CanUseOption.MustUse | CanUseOption.EmptyOrSkipCombo)) return true;
        }

        //终极针组合
        if (UseFinalSting && CanUseFinalSting(out act)) return true;

        //爆发
        if (MoonFluteBreak && DBlueBreak(out act)) return true;

        //高伤害
        if (PrimalSpell(out act)) return true;
        //群体
        if (AreaGCD(out act)) return true;
        //单体填充
        if (SingleGCD(out act)) return true;


        act = null;
        return false;
    }

    protected override bool HealSingleGCD(out IAction act)
    {
        if (BlueId == BLUID.Healer)
        {
            //有某些非常危险的状态。
            if (SpecialType == SpecialCommandType.EsunaStanceNorth && WeakenPeople.Any() || DyingPeople.Any())
            {
                if (Exuviation.CanUse(out act, CanUseOption.MustUse)) return true;
            }
            if (AngelsSnack.CanUse(out act)) return true;
            if (Stotram.CanUse(out act)) return true;
            if (PomCure.CanUse(out act)) return true;
        }
        else
        {
            if (WhiteWind.CanUse(out act, CanUseOption.MustUse)) return true;
        }

        return base.HealSingleGCD(out act);
    }

    /// <summary>
    /// D青爆发
    /// </summary>
    /// <param name="act"></param>
    /// <returns></returns>
    private bool DBlueBreak(out IAction act)
    {
        if (TripleTrident.OnSlot && TripleTrident.WillHaveOneChargeGCD(OnSlotCount(Whistle, Tingle), 0))
        {
            //口笛
            if (Whistle.CanUse(out act)) return true;
            //哔哩哔哩
            if (!Player.HasStatus(true, StatusID.Tingling)
                && Tingle.CanUse(out act, CanUseOption.MustUse)) return true;
            if (OffGuard.CanUse(out act)) return true;
            //鱼叉
            if (TripleTrident.CanUse(out act, CanUseOption.MustUse)) return true;
        }

        if (AllOnSlot(Whistle, FinalSting, BasicInstinct) && UseFinalSting)
        {
            if (Whistle.CanUse(out act)) return true;
            //破防
            if (OffGuard.CanUse(out act)) return true;
            //哔哩哔哩
            if (Tingle.CanUse(out act)) return true;
        }

        //月笛
        if (CanUseMoonFlute(out act)) return true;

        if (!Player.HasStatus(true, StatusID.WaxingNocturne)) return false;

        //月下彼岸花
        if (NightBloom.CanUse(out act, CanUseOption.MustUse)) return true;
        //地火喷发
        if (Eruption.CanUse(out act, CanUseOption.MustUse)) return true;
        //马特拉魔术
        if (MatraMagic.CanUse(out act, CanUseOption.MustUse)) return true;
        //正义飞踢
        if (JKick.CanUse(out act, CanUseOption.MustUse)) return true;
        //捕食
        if (Devour.CanUse(out act, CanUseOption.MustUse)) return true;
        //轰雷
        if (ShockStrike.CanUse(out act, CanUseOption.MustUse)) return true;
        //冰雪乱舞
        if (GlassDance.CanUse(out act, CanUseOption.MustUse)) return true;
        //魔法锤
        if (MagicHammer.CanUse(out act, CanUseOption.MustUse)) return true;
        //穿甲散弹
        if (Surpanakha.CurrentCharges >= 3 && Surpanakha.CanUse(out act, CanUseOption.MustUse | CanUseOption.EmptyOrSkipCombo)) return true;
        //鬼宿脚
        if (PhantomFlurry.CanUse(out act, CanUseOption.MustUse)) return true;

        //冰雾
        if (WhiteDeath.CanUse(out act)) return true;
        //如意大旋风
        if (InBurst && !MoonFluteBreak && BothEnds.CanUse(out act, CanUseOption.MustUse)) return true;
        //类星体
        if (Quasar.CanUse(out act, CanUseOption.MustUse)) return true;
        //飞翎雨
        if (FeatherRain.CanUse(out act, CanUseOption.MustUse)) return true;
        //山崩
        if (MountainBuster.CanUse(out act, CanUseOption.MustUse)) return true;
        //冰雪乱舞
        if (MountainBuster.CanUse(out act, CanUseOption.MustUse)) return true;

        //音爆
        if (SonicBoom.CanUse(out act)) return true;

        return false;
    }


    /// <summary>
    /// 月笛条件
    /// </summary>
    /// <param name="act"></param>
    /// <returns></returns>
    private bool CanUseMoonFlute(out IAction act)
    {
        if (!MoonFlute.CanUse(out act) && !HasHostilesInRange) return false;

        if (Player.HasStatus(true, StatusID.WaxingNocturne)) return false;

        if (Player.HasStatus(true, StatusID.Harmonized)) return true;

        return false;
    }

    /// <summary>
    /// 终极针组合
    /// </summary>
    /// <param name="act"></param>
    /// <returns></returns>
    private bool CanUseFinalSting(out IAction act)
    {
        act = null;
        if (!UseFinalSting) return false;
        if (!FinalSting.CanUse(out _)) return false;

        var useFinalSting = Player.HasStatus(true, StatusID.WaxingNocturne, StatusID.Harmonized);

        if (AllOnSlot(Whistle, MoonFlute, FinalSting) && !AllOnSlot(BasicInstinct))
        {
            if ((float)Target.CurrentHp / Target.MaxHp > FinalStingHP) return false;

            if (Whistle.CanUse(out act)) return true;
            if (MoonFlute.CanUse(out act)) return true;
            if (useFinalSting && FinalSting.CanUse(out act)) return true;
        }

        if (AllOnSlot(Whistle, MoonFlute, FinalSting, BasicInstinct))
        {
            //破防
            if (Player.HasStatus(true, StatusID.WaxingNocturne) && OffGuard.CanUse(out act)) return true;

            if ((float)Target.CurrentHp / Target.MaxHp > FinalStingHP) return false;
            if (Whistle.CanUse(out act)) return true;
            if (MoonFlute.CanUse(out act)) return true;
            if (useFinalSting && FinalSting.CanUse(out act)) return true;
        }

        return false;
    }

    /// <summary>
    /// 单体GCD填充
    /// </summary>
    /// <param name="act"></param>
    /// <returns></returns>
    private bool SingleGCD(out IAction act)
    {
        act = null;
        if (Player.HasStatus(true, StatusID.WaxingNocturne)) return false;

        //滑舌 +眩晕 0-70练级用
        if (QuickLevel && StickyTongue.CanUse(out act)) return true;

        //苦闷之歌
        if (AllOnSlot(Bristle, SongOfTorment) && SongOfTorment.CanUse(out _))
        {
            //怒发冲冠
            if (Bristle.CanUse(out act)) return true;
            if (SongOfTorment.CanUse(out act)) return true;
        }
        if (SongOfTorment.CanUse(out act)) return true;

        //复仇冲击
        if (RevengeBlast.CanUse(out act)) return true;
        //赌徒行为
        if (GamblerKill)
        {
            //导弹
            if (Missile.CanUse(out act)) return true;
            //螺旋尾
            if (TailScrew.CanUse(out act)) return true;
            //死亡宣告
            if (Doom.CanUse(out act)) return true;
        }

        //锋利菜刀 近战 眩晕增伤
        if (SharpenedKnife.CanUse(out act)) return true;

        //吸血 回蓝
        if (CurrentMp < 1000 && BloodDrain.CanUse(out act)) return true;
        //音爆
        if (SonicBoom.CanUse(out act)) return true;
        if (DrillCannons.CanUse(out act, CanUseOption.MustUse)) return true;
        //永恒射线 无法 +眩晕1s
        if (PerpetualRay.CanUse(out act)) return true;
        //深渊贯穿 无物 +麻痹
        if (AbyssalTransfixion.CanUse(out act)) return true;
        //逆流 雷法 +加重
        if (Reflux.CanUse(out act)) return true;
        //水炮
        if (WaterCannon.CanUse(out act)) return true;

        //小侦测
        if (CondensedLibra.CanUse(out act)) return true;

        //滑舌 +眩晕
        if (StickyTongue.CanUse(out act)) return true;

        //投掷沙丁鱼(打断)
        if (FlyingSardine.CanUse(out act)) return true;

        return false;
    }

    /// <summary>
    /// 范围GCD填充
    /// </summary>
    /// <param name="act"></param>
    /// <returns></returns>
    private bool AreaGCD(out IAction act)
    {
        act = null;
        if (Player.HasStatus(true, StatusID.WaxingNocturne)) return false;

        //赌徒行为
        if (GamblerKill)
        {
            //火箭炮
            if (Launcher.CanUse(out act, CanUseOption.MustUse)) return true;
            //5级即死
            if (Level5Death.CanUse(out act, CanUseOption.MustUse)) return true;
        }

        if (false)
        {
            if (AcornBomb.CanUse(out act, CanUseOption.MustUse))
            {
                return true;
            }

            if (Faze.CanUse(out act, CanUseOption.MustUse)) return true;
            if (Snort.CanUse(out act, CanUseOption.MustUse)) return true;
            if (BadBreath.CanUse(out act, CanUseOption.MustUse)) return true;
            if (Chirp.CanUse(out act, CanUseOption.MustUse)) return true;
            if (Level5Petrify.CanUse(out act, CanUseOption.MustUse)) return true;
        }

        //陆行鸟陨石
        if (HasCompanion && ChocoMeteor.CanUse(out act, CanUseOption.MustUse)) return true;

        if (HostileTargets.GetObjectInRadius(6).Count() < 3)
        {
            //水力吸引
            if (HydroPull.CanUse(out act)) return true;
        }

        //寒冰咆哮
        if (TheRamVoice.CanUse(out act)) return true;

        //超振动
        if (!IsMoving && Target.HasStatus(false, StatusID.DeepFreeze) && TheRamVoice.CanUse(out act)) return true;

        //雷电咆哮
        if (TheDragonVoice.CanUse(out act)) return true;

        //冰焰
        if (Blaze.CanUse(out act)) return true;
        if (FeculentFlood.CanUse(out act)) return true;
        //火炎放射
        if (FlameThrower.CanUse(out act)) return true;
        //水流吐息
        if (AquaBreath.CanUse(out act)) return true;
        //高压电流
        if (HighVoltage.CanUse(out act)) return true;
        //怒视
        if (Glower.CanUse(out act)) return true;
        //平原震裂
        if (PlainCracker.CanUse(out act)) return true;
        //诡异视线
        if (TheLook.CanUse(out act)) return true;
        //喷墨
        if (InkJet.CanUse(out act)) return true;
        if (FireAngon.CanUse(out act)) return true;
        if (MindBlast.CanUse(out act)) return true;
        if (AlpineDraft.CanUse(out act)) return true;
        if (ProteanWave.CanUse(out act)) return true;
        if (Northerlies.CanUse(out act)) return true;
        if (Electrogenesis.CanUse(out act)) return true;
        if (WhiteKnightsTour.CanUse(out act)) return true;
        if (BlackKnightsTour.CanUse(out act)) return true;
        if (Tatamigaeshi.CanUse(out act)) return true;

        if (MustardBomb.CanUse(out act)) return true;
        if (AetherialSpark.CanUse(out act)) return true;
        if (MaledictionOfWater.CanUse(out act)) return true;
        if (FlyingFrenzy.CanUse(out act)) return true;
        if (DrillCannons.CanUse(out act)) return true;
        if (Weight4Tonze.CanUse(out act)) return true;
        if (Needles1000.CanUse(out act)) return true;
        if (Kaltstrahl.CanUse(out act)) return true;
        if (PeripheralSynthesis.CanUse(out act)) return true;
        if (FlameThrower.CanUse(out act)) return true;
        if (FlameThrower.CanUse(out act)) return true;
        if (SaintlyBeam.CanUse(out act)) return true;

        return false;
    }

    /// <summary>
    /// 有CD的技能
    /// </summary>
    /// <param name="act"></param>
    /// <returns></returns>
    private bool PrimalSpell(out IAction act)
    {
        act = null;
        if (Player.HasStatus(true, StatusID.WaxingNocturne)) return false;

        //冰雾
        if (WhiteDeath.CanUse(out act)) return true;
        //玄天武水壁
        if (DivineCataract.CanUse(out act)) return true;

        //斗灵弹
        if (TheRoseOfDestruction.CanUse(out act)) return true;

        //渔叉三段
        if (InBurst && !MoonFluteBreak && TripleTrident.CanUse(out act)) return true;
        //马特拉魔术
        if (InBurst && !MoonFluteBreak && MatraMagic.CanUse(out act)) return true;

        //捕食
        if (Devour.CanUse(out act)) return true;
        //魔法锤
        //if (MagicHammer.ShouldUse(out act)) return true;

        var option = SingleAOE ? CanUseOption.MustUse : CanUseOption.None;
        //月下彼岸花
        if (InBurst && !MoonFluteBreak && NightBloom.CanUse(out act, option)) return true;
        //如意大旋风
        if (InBurst && !MoonFluteBreak && BothEnds.CanUse(out act, option)) return true;

        //穿甲散弹
        if (InBurst && !MoonFluteBreak && Surpanakha.CurrentCharges >= 3 && Surpanakha.CanUse(out act, option | CanUseOption.EmptyOrSkipCombo)) return true;

        //类星体
        if (Quasar.CanUse(out act, option)) return true;
        //正义飞踢
        if (!IsMoving && JKick.CanUse(out act, option)) return true;

        //地火喷发
        if (Eruption.CanUse(out act, option)) return true;
        //飞翎雨
        if (FeatherRain.CanUse(out act, option)) return true;

        //轰雷
        if (ShockStrike.CanUse(out act, option)) return true;
        //山崩
        if (MountainBuster.CanUse(out act, option)) return true;

        //冰雪乱舞
        if (MountainBuster.CanUse(out act, option)) return true;

        //if (MountainBuster.ShouldUse(out act, option)) return true;


        return false;
    }
}
