public class PlayerLibero : BasePlayer
{
    protected override void OnInit()
    {
        Type = PlayerType.Libero;
    }

    public override void ExecuteAction(QualityType type)
    {
        base.ExecuteAction(type);
        PlayAnim(PlayerAnimType.Receive);
    }
}
