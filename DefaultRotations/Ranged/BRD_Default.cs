namespace DefaultRotations.Ranged;

[SourceCode("https://github.com/ArchiDog1998/FFXIVRotations/blob/main/DefaultRotations/Ranged/BRD_Default.cs")]
public sealed class BRD_Default : BRD_Base
{
    public override string GameVersion => "6.28";

    public override string RotationName => "Default";

    protected override IRotationConfigSet CreateConfiguration() => base.CreateConfiguration()
            .SetBool("BindWAND", false, @"Use Raging Strikes on ""Wanderer's Minuet""")
            .SetCombo("FirstSong", 0, "First Song", "Wanderer's Minuet", "Mage's Ballad", "Army's Paeon")
            .SetFloat("WANDTime", 43, "Wanderer's Minuet Uptime", min: 0, max: 45, speed: 1)
            .SetFloat("MAGETime", 34, "Mage's Ballad Uptime", min: 0, max: 45, speed: 1)
            .SetFloat("ARMYTime", 43, "Army's Paeon Uptime", min: 0, max: 45, speed: 1);

    public override string Description => "Please make sure that the three song times add up to 120 seconds!";

    private bool BindWAND => Configs.GetBool("BindWAND") && WanderersMinuet.EnoughLevel;
    private int FirstSong => Configs.GetCombo("FirstSong");
    private float WANDRemainTime => 45 - Configs.GetFloat("WANDTime");
    private float MAGERemainTime => 45 - Configs.GetFloat("MAGETime");
    private float ARMYRemainTime => 45 - Configs.GetFloat("ARMYTime");

    protected override bool GeneralGCD(out IAction act)
    {
        //伶牙俐齿
        if (IronJaws.CanUse(out act)) return true;
        if (IronJaws.CanUse(out act, CanUseOption.MustUse) && IronJaws.Target.WillStatusEnd(30, true, IronJaws.TargetStatus))
        {
            if (Player.HasStatus(true, StatusID.RagingStrikes) && Player.WillStatusEndGCD(1, 0, true, StatusID.RagingStrikes)) return true;
        }

        //放大招！
        if (CanUseApexArrow(out act)) return true;
        //爆破箭
        if (BlastArrow.CanUse(out act, CanUseOption.MustUse))
        {
            if (!Player.HasStatus(true, StatusID.RagingStrikes)) return true;
            if (Player.HasStatus(true, StatusID.RagingStrikes) && Barrage.IsCoolingDown) return true;
        }

        //群体GCD
        if (ShadowBite.CanUse(out act)) return true;
        if (QuickNock.CanUse(out act)) return true;

        //上毒
        if (WindBite.CanUse(out act)) return true;
        if (VenomousBite.CanUse(out act)) return true;

        //直线射击
        if (StraitShoot.CanUse(out act)) return true;

        //强力射击
        if (HeavyShoot.CanUse(out act)) return true;

        return false;
    }

    protected override bool EmergencyAbility(IAction nextGCD, out IAction act)
    {
        //如果接下来要上毒或者要直线射击，那算了。
        if (nextGCD.IsTheSameTo(true, StraitShoot, VenomousBite, WindBite, IronJaws))
        {
            return base.EmergencyAbility(nextGCD, out act);
        }
        else if ((!RagingStrikes.EnoughLevel || Player.HasStatus(true, StatusID.RagingStrikes)) && (!BattleVoice.EnoughLevel || Player.HasStatus(true, StatusID.BattleVoice)))
        {
            if ((EmpyrealArrow.IsCoolingDown && !EmpyrealArrow.WillHaveOneChargeGCD(1) || !EmpyrealArrow.EnoughLevel) && Repertoire != 3)
            {
                //纷乱箭
                if (!Player.HasStatus(true, StatusID.StraightShotReady) && Barrage.CanUse(out act)) return true;
            }
        }

        return base.EmergencyAbility(nextGCD, out act);
    }

    protected override bool AttackAbility(out IAction act)
    {
        act = null;

        if (Song == Song.NONE)
        {
            if (FirstSong == 0 && WanderersMinuet.CanUse(out act)) return true;
            if (FirstSong == 1 && MagesBallad.CanUse(out act)) return true;
            if (FirstSong == 2 && ArmysPaeon.CanUse(out act)) return true;
            if (WanderersMinuet.CanUse(out act)) return true;
            if (MagesBallad.CanUse(out act)) return true;
            if (ArmysPaeon.CanUse(out act)) return true;
        }

        if (InBurst && Song != Song.NONE && MagesBallad.EnoughLevel)
        {
            //猛者强击
            if (RagingStrikes.CanUse(out act))
            {
                if (BindWAND && Song == Song.WANDERER && WanderersMinuet.EnoughLevel) return true;
                if (!BindWAND) return true;
            }

            //光明神的最终乐章
            if (RadiantFinale.CanUse(out act, CanUseOption.MustUse))
            {
                if (Player.HasStatus(true, StatusID.RagingStrikes) && RagingStrikes.ElapsedOneChargeAfterGCD(1)) return true;
            }

            //战斗之声
            if (BattleVoice.CanUse(out act, CanUseOption.MustUse))
            {
                if (IsLastAction(true, RadiantFinale)) return true;

                if (Player.HasStatus(true, StatusID.RagingStrikes) && RagingStrikes.ElapsedOneChargeAfterGCD(1)) return true;
            }
        }

        if (RadiantFinale.EnoughLevel && RadiantFinale.IsCoolingDown && BattleVoice.EnoughLevel && !BattleVoice.IsCoolingDown) return false;

        //放浪神的小步舞曲
        if (WanderersMinuet.CanUse(out act, CanUseOption.OnLastAbility))
        {
            if (SongEndAfter(ARMYRemainTime) && (Song != Song.NONE || Player.HasStatus(true, StatusID.ArmyEthos))) return true;
        }

        //九天连箭
        if (Song != Song.NONE && EmpyrealArrow.CanUse(out act)) return true;

        //完美音调
        if (PitchPerfect.CanUse(out act))
        {
            if (SongEndAfter(3) && Repertoire > 0) return true;

            if (Repertoire == 3) return true;

            if (Repertoire == 2 && EmpyrealArrow.WillHaveOneChargeGCD(1) && NextAbilityToNextGCD < PitchPerfect.AnimationLockTime + Ping) return true;

            if (Repertoire == 2 && EmpyrealArrow.WillHaveOneChargeGCD() && NextAbilityToNextGCD > PitchPerfect.AnimationLockTime + Ping) return true;
        }

        //贤者的叙事谣
        if (MagesBallad.CanUse(out act))
        {
            if (Song == Song.WANDERER && SongEndAfter(WANDRemainTime) && Repertoire == 0) return true;
            if (Song == Song.ARMY && SongEndAfterGCD(2) && WanderersMinuet.IsCoolingDown) return true;
        }


        //军神的赞美歌
        if (ArmysPaeon.CanUse(out act))
        {
            if (WanderersMinuet.EnoughLevel && SongEndAfter(MAGERemainTime) && Song == Song.MAGE) return true;
            if (WanderersMinuet.EnoughLevel && SongEndAfter(2) && MagesBallad.IsCoolingDown && Song == Song.WANDERER) return true;
            if (!WanderersMinuet.EnoughLevel && SongEndAfter(2)) return true;
        }

        //测风诱导箭
        if (Sidewinder.CanUse(out act))
        {
            if (Player.HasStatus(true, StatusID.BattleVoice) && (Player.HasStatus(true, StatusID.RadiantFinale) || !RadiantFinale.EnoughLevel)) return true;

            if (!BattleVoice.WillHaveOneCharge(10) && !RadiantFinale.WillHaveOneCharge(10)) return true;

            if (RagingStrikes.IsCoolingDown && !Player.HasStatus(true, StatusID.RagingStrikes)) return true;
        }

        //看看现在有没有开猛者强击和战斗之声
        bool empty = Player.HasStatus(true, StatusID.RagingStrikes) && (Player.HasStatus(true, StatusID.BattleVoice) || !BattleVoice.EnoughLevel) || Song == Song.MAGE;

        if (EmpyrealArrow.IsCoolingDown || !EmpyrealArrow.WillHaveOneChargeGCD() || Repertoire != 3 || !EmpyrealArrow.EnoughLevel)
        {
            //死亡剑雨
            if (RainOfDeath.CanUse(out act, CanUseOption.EmptyOrSkipCombo)) return true;

            //失血箭
            if (Bloodletter.CanUse(out act, CanUseOption.EmptyOrSkipCombo)) return true;
        }

        return false;
    }

    private bool CanUseApexArrow(out IAction act)
    {
        //放大招！
        if (!ApexArrow.CanUse(out act, CanUseOption.MustUse)) return false;

        //aoe期间
        if (QuickNock.CanUse(out _) && SoulVoice == 100) return true;

        //快爆发了,攒着等爆发
        if (SoulVoice == 100 && BattleVoice.WillHaveOneCharge(25)) return false;

        //爆发快过了,如果手里还有绝峰箭,就把绝峰箭打出去
        if (SoulVoice >= 80 && Player.HasStatus(true, StatusID.RagingStrikes) && Player.WillStatusEnd(10, false, StatusID.RagingStrikes)) return true;

        //爆发期绝峰箭
        if (SoulVoice == 100 && Player.HasStatus(true, StatusID.RagingStrikes) && Player.HasStatus(true, StatusID.BattleVoice)) return true;

        //贤者歌期间
        if (Song == Song.MAGE && SoulVoice >= 80 && SongEndAfter(22) && SongEndAfter(18)) return true;

        //能量之声等于100
        if (!Player.HasStatus(true, StatusID.RagingStrikes) && SoulVoice == 100) return true;

        return false;
    }
}
