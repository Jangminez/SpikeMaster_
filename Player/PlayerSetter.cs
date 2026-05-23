public class PlayerSetter : BasePlayer
{

    protected override void OnInit()
    {
        Type = PlayerType.Setter;
    }

    public override void ExecuteAction(QualityType type)
    {
        base.ExecuteAction(type);
        PlayAnim(PlayerAnimType.Toss);
    }
}
