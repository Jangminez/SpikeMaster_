public class PlayerSpiker : BasePlayer
{
    private bool _onSpike = false;

    protected override void OnInit()
    {
        Type = PlayerType.Spiker;
        _onSpike = false;
    }

    public override void ExecuteAction(QualityType type)
    {
        if (!_onSpike)
        {
            PlayAnim(PlayerAnimType.Spike_Start);
            _onSpike = true;
        }

        else
        {
            PlayAnim(PlayerAnimType.Spike_End);
            _onSpike = false;
        }
    }
}
