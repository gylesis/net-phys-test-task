using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

namespace Scripts.Runtime.Core.Net
{
    // Debug overlay (OnGUI) for the host: tweaks the UnityTransport network simulation
    // (packet delay, jitter, loss) at runtime to test gameplay under bad network conditions.
    //
    // UnityTransport only accepts simulator parameters before its driver is created, so the values set
    // here are kept in static fields and ConnectionManager applies them right before StartHost/StartClient.
    public class NetworkSimDebugGui : MonoBehaviour
    {
        private const float WindowWidth = 340f;
        private const float WindowHeight = 190f;

        private static int _desiredPacketDelayMs;
        private static int _desiredPacketJitterMs;
        private static int _desiredDropRatePercent;
        private static bool _wasApplied;

        private GUIStyle _labelStyle;

        // Values ConnectionManager picks up before creating a new connection.
        public static bool HasDesiredParameters =>
            _desiredPacketDelayMs > 0 || _desiredPacketJitterMs > 0 || _desiredDropRatePercent > 0;

        // True if a previous connection was started with simulator parameters - they must be explicitly cleared.
        public static bool NeedsReset => _wasApplied && !HasDesiredParameters;

        public static void GetDesiredParameters(out int packetDelayMs, out int packetJitterMs, out int dropRatePercent)
        {
            packetDelayMs = _desiredPacketDelayMs;
            packetJitterMs = _desiredPacketJitterMs;
            dropRatePercent = _desiredDropRatePercent;
        }

        public static void MarkApplied()
        {
            _wasApplied = true;
        }

        public static void MarkReset()
        {
            _wasApplied = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetAndSpawn()
        {
            _desiredPacketDelayMs = 0;
            _desiredPacketJitterMs = 0;
            _desiredDropRatePercent = 0;
            _wasApplied = false;

            var go = new GameObject(nameof(NetworkSimDebugGui));
            go.AddComponent<NetworkSimDebugGui>();
            DontDestroyOnLoad(go);
        }

        private void OnGUI()
        {
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
                return;

            if (NetworkManager.Singleton.NetworkConfig.NetworkTransport is not UnityTransport)
                return;

            if (_labelStyle == null)
                _labelStyle = new GUIStyle(GUI.skin.label) { wordWrap = true };

            Rect rect = new Rect(10f, 10f, WindowWidth, WindowHeight);
            GUILayout.BeginArea(rect, GUI.skin.box);

            GUILayout.Label("Network simulation (runtime, not saved)", _labelStyle);

            DrawSlider("Packet delay", ref _desiredPacketDelayMs, 0, 500, " ms");
            DrawSlider("Packet jitter", ref _desiredPacketJitterMs, 0, 200, " ms");
            DrawSlider("Packet loss", ref _desiredDropRatePercent, 0, 100, " %");

            GUILayout.Label("Applies on the next Host/Join (UnityTransport limitation).", _labelStyle);

            GUILayout.EndArea();
        }

        private void DrawSlider(string label, ref int value, int min, int max, string suffix)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label($"{label}: {value}{suffix}", _labelStyle, GUILayout.Width(160f));
            float newValue = GUILayout.HorizontalSlider(value, min, max);
            value = Mathf.RoundToInt(newValue);
            GUILayout.EndHorizontal();
        }
    }
}
