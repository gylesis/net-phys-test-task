using Cysharp.Threading.Tasks;

namespace Scripts.Runtime.Core.States
{
    public class LoadMainMenuState : GameState
    {
        private SceneLoader _sceneLoader;

        public LoadMainMenuState(SceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }

        public override void Enter()
        {
            LoadScene().Forget();
        }

        private async UniTask LoadScene()
        {
            await _sceneLoader.LoadScene("MainMenu");
            Machine.Enter<MainMenuState>();
        }

        public override void Tick() { }

        public override void Exit() { }
    }
}