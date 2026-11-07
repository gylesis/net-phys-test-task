using Scripts.Runtime.Core.CameraLogic;

namespace Scripts.Runtime.Core
{
    public class GameplaySceneContext
    {
        public GameplaySceneRefs SceneRefs { get; private set; }
        public CameraController Camera { get; private set; }

        public void Setup(GameplaySceneRefs sceneRefs, CameraController camera)
        {
            SceneRefs = sceneRefs;
            Camera = camera;
        }
    }
}
