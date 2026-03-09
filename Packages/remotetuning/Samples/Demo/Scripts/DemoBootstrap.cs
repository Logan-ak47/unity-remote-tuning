using UnityEngine;
using UnityEngine.UIElements;

namespace Ashutosh.RemoteTuning.Demo
{
    public sealed class DemoBootstrap : MonoBehaviour
    {
        [Header("Endpoint")]
        [Tooltip("Point this to your JSON endpoint. For now you can keep a placeholder.")]
        public string endpointUrl = "https://example.com/config.json";

        [Header("UI")]
        public UIDocument uiDocument;

        private RemoteTuningClient _client;
        private DemoUIController _ui;

        private void Awake()
        {
            if (uiDocument == null)
                uiDocument = FindObjectOfType<UIDocument>();

            var options = new RemoteTuningOptions(endpointUrl)
            {
                TimeoutSeconds = 10,
                TtlSeconds = 300,
                StaleSeconds = 1800
            };

            _client = RemoteTuningFactory.CreateDefault(options);
            _ui = new DemoUIController(uiDocument.rootVisualElement, _client);

            _ui.Render();
        }
    }
}