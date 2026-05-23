using UnityEngine;

public class PlayerHandler
{
    private IPlayer _server, _setter, _libero, _spiker;

    private bool isInitialized = false;

    public PlayerHandler()
    {
        if (!isInitialized)
        {
            _setter = EntityFactory.CreateEntity<IPlayer>(EntityPath.Player_Se);
            _libero = EntityFactory.CreateEntity<IPlayer>(EntityPath.Player_Li);
            _spiker = EntityFactory.CreateEntity<IPlayer>(EntityPath.Player_OH);
            _server = EntityFactory.CreateEntity<IPlayer>(EntityPath.Enemy_Server);
        }

        InitPlayers();

        isInitialized = true;
    }

    private void InitPlayers()
    {
        _setter.Init();
        _libero.Init();
        _spiker.Init();
        _server.Init();
    }

    public void ResetPlayer()
    {
        _setter.ResetPlayer();
        _libero.ResetPlayer();
        _spiker.ResetPlayer();
        _server.ResetPlayer();
    }

    public void ExecuteAction(PlayerType type, QualityType quality)
    {
        switch (type)
        {
            case PlayerType.Spiker:
                _spiker.ExecuteAction(quality);
                break;
            case PlayerType.Setter:
                _setter.ExecuteAction(quality);
                break;
            case PlayerType.Libero:
                _libero.ExecuteAction(quality);
                break;
            case PlayerType.Server:
                _server.ExecuteAction(quality);
                break;
        }
    }

    public Vector2 GetPos(PlayerType type)
    {
        return type switch
        {
            PlayerType.Spiker => _spiker.GetPosition(),
            PlayerType.Setter => _setter.GetPosition(),
            PlayerType.Libero => _libero.GetPosition(),
            PlayerType.Server => _server.GetPosition(),
            _ => _libero.GetPosition()
        };
    }
}
