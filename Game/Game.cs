public static class Game
{
    public static MainSceneManager Main => MainSceneManager.Instance;
    public static PlaySceneManager Play => PlaySceneManager.Instance;
    public static EventManager Event => EventManager.Instance;
}
