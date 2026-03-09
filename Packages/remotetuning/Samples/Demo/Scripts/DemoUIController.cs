using System;
using UnityEngine.UIElements;

namespace Ashutosh.RemoteTuning.Demo
{
    public sealed class DemoUIController
    {
        private readonly VisualElement _root;
        private readonly RemoteTuningClient _client;

        private readonly TextField _userIdField;
        private readonly Button _applyUserIdButton;
        private readonly Button _refreshButton;

        private readonly Label _sourceLabel;
        private readonly Label _reasonLabel;
        private readonly Label _cacheLabel;
        private readonly Label _versionLabel;
        private readonly Label _timeLabel;

        private readonly Label _difficultyLabel;
        private readonly Label _adCooldownLabel;
        private readonly Label _iapVisibleLabel;
        private readonly Label _variantLabel;

        public DemoUIController(VisualElement root, RemoteTuningClient client)
        {
            _root = root;
            _client = client;

            _userIdField = _root.Q<TextField>("userIdField");
            _applyUserIdButton = _root.Q<Button>("applyUserIdButton");
            _refreshButton = _root.Q<Button>("refreshButton");

            _sourceLabel = _root.Q<Label>("sourceLabel");
            _reasonLabel = _root.Q<Label>("reasonLabel");
            _cacheLabel = _root.Q<Label>("cacheLabel");
            _versionLabel = _root.Q<Label>("versionLabel");
            _timeLabel = _root.Q<Label>("timeLabel");

            _difficultyLabel = _root.Q<Label>("difficultyLabel");
            _adCooldownLabel = _root.Q<Label>("adCooldownLabel");
            _iapVisibleLabel = _root.Q<Label>("iapVisibleLabel");
            _variantLabel = _root.Q<Label>("variantLabel");

            WireEvents();
        }

        private void WireEvents()
        {
            if (_applyUserIdButton != null)
                _applyUserIdButton.clicked += () =>
                {
                    _client.SetUserId(_userIdField?.value);
                    Render();
                };

            if (_refreshButton != null)
                _refreshButton.clicked += async () =>
                {
                    await _client.RefreshAsync();
                    Render();
                };
        }

        public void Render()
        {
            var s = _client.Current;

            _sourceLabel.text = $"Source: {s.Source}";
            _reasonLabel.text = $"Reason: {s.Reason}";
            _cacheLabel.text = $"Cache: {s.CacheStatus} (IsStale={s.IsStale})";
            _versionLabel.text = $"Version: {s.ConfigVersion}";

            _timeLabel.text =
                $"Fetched: {Fmt(s.FetchedAtUtc)} | FreshUntil: {Fmt(s.FreshUntilUtc)} | StaleUntil: {Fmt(s.StaleUntilUtc)}";

            // Showcase reads (safe fallbacks)
            _difficultyLabel.text = $"Difficulty Scalar: {_client.Values.GetDifficultyScalar(1.0f):0.###}";
            _adCooldownLabel.text = $"Ad Cooldown: {_client.Values.GetAdCooldownSeconds(60)} sec";
            _iapVisibleLabel.text = $"IAP Visible: {_client.Flags.IsEnabled(\"iapVisible\", true)}";
            _variantLabel.text = $"Variant (reward_exp): {_client.Flags.GetVariant(\"reward_exp\", \"control\")}";
        }

        private static string Fmt(DateTime utc)
        {
            if (utc == default) return "-";
            return utc.ToString("HH:mm:ss");
        }
    }
}