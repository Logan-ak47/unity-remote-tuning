using UnityEngine;
using UnityEngine.UIElements;

namespace Ashutosh.RemoteTuning.Demo
{
    public sealed class DemoBootstrap : MonoBehaviour
    {
        [Header("Endpoint")]
        public string endpointUrl = "https://example.com/config.json";

        [Header("UI")]
        public UIDocument uiDocument;

        private RemoteTuningClient _client;
        private ToggleableTransport _transportToggle;
        private DemoUIController _ui;

        private void Awake()
        {
            if (uiDocument == null)
                uiDocument = FindObjectOfType<UIDocument>();

            var options = new RemoteTuningOptions(endpointUrl)
            {
                TimeoutSeconds = 10,
                TtlSeconds = 30,
                StaleSeconds = 120
            };

            var built = RemoteTuningFactory.CreateDefault(options);
            _client = built.Client;
            _transportToggle = built.TransportToggle;

            _ui = new DemoUIController(uiDocument.rootVisualElement, _client, _transportToggle);
            _ui.Render();
        }
    }
}