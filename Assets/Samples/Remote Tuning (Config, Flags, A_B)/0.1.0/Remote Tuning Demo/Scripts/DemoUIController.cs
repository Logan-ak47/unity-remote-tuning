using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Ashutosh.RemoteTuning.Demo
{
    public sealed class DemoUIController
    {
        private readonly VisualElement _root;
        private readonly RemoteTuningClient _client;
        private readonly ToggleableTransport _transport;

        private readonly TextField _userIdField;
        private readonly Button _applyUserIdButton;
        private readonly Button _randomizeUserIdButton;
        private readonly Button _refreshButton;
        private readonly Toggle _offlineToggle;

        private readonly DropdownField _variantDropdown;
        private readonly Button _clearOverrideButton;

        private readonly Label _sourceLabel;
        private readonly Label _reasonLabel;
        private readonly Label _cacheLabel;
        private readonly Label _versionLabel;
        private readonly Label _ttlLabel;
        private readonly Label _timeLabel;

        private readonly Label _difficultyLabel;
        private readonly Label _adCooldownLabel;
        private readonly Button _startCooldownButton;
        private readonly Label _cooldownStateLabel;

        private readonly Label _variantLabel;
        private readonly Label _rewardLabel;
        private readonly VisualElement _iapSection;

        private float _cooldownRemaining;
        private bool _cooldownActive;

        public DemoUIController(VisualElement root, RemoteTuningClient client, ToggleableTransport transport)
        {
            _root = root;
            _client = client;
            _transport = transport;

            _userIdField = _root.Q<TextField>("userIdField");
            _applyUserIdButton = _root.Q<Button>("applyUserIdButton");
            _randomizeUserIdButton = _root.Q<Button>("randomizeUserIdButton");
            _refreshButton = _root.Q<Button>("refreshButton");
            _offlineToggle = _root.Q<Toggle>("offlineToggle");

            _variantDropdown = _root.Q<DropdownField>("variantDropdown");
            _clearOverrideButton = _root.Q<Button>("clearOverrideButton");

            _sourceLabel = _root.Q<Label>("sourceLabel");
            _reasonLabel = _root.Q<Label>("reasonLabel");
            _cacheLabel = _root.Q<Label>("cacheLabel");
            _versionLabel = _root.Q<Label>("versionLabel");
            _ttlLabel = _root.Q<Label>("ttlLabel");
            _timeLabel = _root.Q<Label>("timeLabel");

            _difficultyLabel = _root.Q<Label>("difficultyLabel");
            _adCooldownLabel = _root.Q<Label>("adCooldownLabel");
            _startCooldownButton = _root.Q<Button>("startCooldownButton");
            _cooldownStateLabel = _root.Q<Label>("cooldownStateLabel");

            _variantLabel = _root.Q<Label>("variantLabel");
            _rewardLabel = _root.Q<Label>("rewardLabel");
            _iapSection = _root.Q<VisualElement>("iapSection");

            SetupDropdown();
            WireEvents();

            // Update loop (UI Toolkit needs a driver for timers)
            _root.schedule.Execute(Tick).Every(100); // 10 fps is enough for demo
        }

        private void SetupDropdown()
        {
            _variantDropdown.choices = new List<string> { "None", "A", "B" };
            _variantDropdown.value = "None";
        }

        private void WireEvents()
        {
            _applyUserIdButton.clicked += () =>
            {
                _client.SetUserId(_userIdField?.value);
                Render();
            };

            _randomizeUserIdButton.clicked += () =>
            {
                var id = "user_" + UnityEngine.Random.Range(1000, 9999);
                _userIdField.value = id;
                _client.SetUserId(id);
                Render();
            };

            _refreshButton.clicked += async () =>
            {
                await _client.RefreshAsync();
                Render();
            };

            if (_offlineToggle != null)
            {
                _offlineToggle.RegisterValueChangedCallback(evt =>
                {
                    _transport.SetOffline(evt.newValue);
                    Render();
                });
            }

            if (_variantDropdown != null)
            {
                _variantDropdown.RegisterValueChangedCallback(evt =>
                {
                    var v = evt.newValue;
                    if (v == "None")
                    {
                        _client.Experiments.ClearOverride("reward_exp");
                    }
                    else
                    {
                        _client.Experiments.SetOverride("reward_exp", v);
                    }

                    Render();
                });
            }

            _clearOverrideButton.clicked += () =>
            {
                _variantDropdown.value = "None";
                _client.Experiments.ClearOverride("reward_exp");
                Render();
            };

            _startCooldownButton.clicked += () =>
            {
                var secs = _client.Values.GetAdCooldownSeconds(10);
                _cooldownRemaining = secs;
                _cooldownActive = true;
                Render();
            };
        }

        private void Tick()
        {
            if (!_cooldownActive)
                return;

            _cooldownRemaining -= 0.1f; // because tick every 100ms
            if (_cooldownRemaining <= 0f)
            {
                _cooldownRemaining = 0f;
                _cooldownActive = false;
            }

            _cooldownStateLabel.text = _cooldownActive
                ? $"Cooldown: {_cooldownRemaining:0.0}s"
                : "Ready";
        }

        public void Render()
        {
            var s = _client.Current;

            _sourceLabel.text = $"Source: {s.Source}";
            _reasonLabel.text = $"Reason: {s.Reason}";
            _cacheLabel.text = $"Cache: {s.CacheStatus} (IsStale={s.IsStale})";
            _versionLabel.text = $"Version: {s.ConfigVersion}";
            _timeLabel.text = $"Fetched: {Fmt(s.FetchedAtUtc)} | FreshUntil: {Fmt(s.FreshUntilUtc)} | StaleUntil: {Fmt(s.StaleUntilUtc)}";

            _ttlLabel.text = BuildTtlText(s);

            // Showcase reads
            var diff = _client.Values.GetDifficultyScalar(1.0f);
            var cd = _client.Values.GetAdCooldownSeconds(60);
            var iapVisible = _client.Flags.IsEnabled("iapVisible", true);

            _difficultyLabel.text = $"Difficulty Scalar: {diff:0.###}";
            _adCooldownLabel.text = $"Ad Cooldown: {cd} sec";

            // Experiment affects visible reward multiplier/label
            var variant = _client.Flags.GetVariant("reward_exp", "control");
            _variantLabel.text = $"Variant (reward_exp): {variant}";

            var baseReward = 100;
            var multiplier = _client.Values.GetRewardMultiplier(1.0f);

            // Add a simple visible rule based on variant (demo-only)
            // A: +0%, B: +50% (or control = 1.0)
            var expMult = variant == "B" ? 1.5f : 1.0f;
            var finalReward = (int)(baseReward * multiplier * expMult);

            _rewardLabel.text = $"Reward: {finalReward} (base={baseReward}, cfgMult={multiplier:0.##}, expMult={expMult:0.##})";

            // IAP section gating
            if (_iapSection != null)
                _iapSection.style.display = iapVisible ? DisplayStyle.Flex : DisplayStyle.None;

            // Offline toggle sync (if changed elsewhere)
            if (_offlineToggle != null && _offlineToggle.value != _transport.IsOffline)
                _offlineToggle.value = _transport.IsOffline;

            // Cooldown label update immediately when Render is called
            _cooldownStateLabel.text = _cooldownActive ? $"Cooldown: {_cooldownRemaining:0.0}s" : "Ready";
        }

        private static string BuildTtlText(RemoteTuningSnapshot s)
        {
            // If cache times are default/unset, show status only.
            if (s.FreshUntilUtc == default || s.StaleUntilUtc == default || s.FetchedAtUtc == default)
                return $"TTL: {s.CacheStatus}";

            var now = DateTime.UtcNow;
            if (now < s.FreshUntilUtc)
            {
                var remaining = (s.FreshUntilUtc - now).TotalSeconds;
                return $"TTL: Fresh ({remaining:0}s remaining)";
            }

            if (now < s.StaleUntilUtc)
            {
                var remaining = (s.StaleUntilUtc - now).TotalSeconds;
                return $"TTL: Stale (serving cache, {remaining:0}s until expired)";
            }

            return "TTL: Expired";
        }

        private static string Fmt(DateTime utc)
        {
            if (utc == default) return "-";
            return utc.ToString("HH:mm:ss");
        }
    }
}