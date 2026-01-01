using UnityEngine;
using UnityEngine.UI;
using WheelGame.Config;
using WheelGame.Domain;
using WheelGame.Adapters;
using WheelGame.UI;
using System.Collections.Generic;

public class WheelGameController : MonoBehaviour
{
    [Header("Configs")]
    [SerializeField] private WheelLayoutConfig normalZoneConfig;
    [SerializeField] private WheelLayoutConfig safeZoneConfig;
    [SerializeField] private WheelLayoutConfig superZoneConfig;
    [SerializeField] private RewardProgressionConfig progressionConfig;

    [Header("UI References")]
    [SerializeField] private WheelView wheelView;
    [SerializeField] private HudView hudView;
    [SerializeField] private BombPopupView bombPopupView;
    [SerializeField] private LeaveSummaryPopupView leavePopupView;

    [SerializeField] private Button ui_button_spin;
    [SerializeField] private Button ui_button_leave;

    [SerializeField] private RewardSummaryView rewardSummaryView;
    [SerializeField] private RectTransform ui_image_reward_fly_anchor;
    private int _lastSpinSliceIndex = -1;

    private readonly Dictionary<RewardType, int> _rewardTotals = new();


    // domain + adapters
    private IZoneTypeResolver _zoneResolver;
    private ScriptableWheelDefinitionProvider _wheelProvider;  //Interface type - easy to swap implementations
    private IRewardProgressionStrategy _progression;
    private IRandomProvider _random;
    private WheelGameSession _session;

    private bool _isSpinning;

    private void OnValidate()
    {
        if (wheelView == null)
            wheelView = GetComponentInChildren<WheelView>(true);

        if (hudView == null)
            hudView = GetComponentInChildren<HudView>(true);

        if (bombPopupView == null)
            bombPopupView = GetComponentInChildren<BombPopupView>(true);

        if (ui_button_spin == null)
            ui_button_spin = UIAutoBinder.FindComponentInChildren<Button>(transform, "ui_button_spin");

        if (ui_button_leave == null)
            ui_button_leave = UIAutoBinder.FindComponentInChildren<Button>(transform, "ui_button_leave");
            
        if (rewardSummaryView == null)
            rewardSummaryView = GetComponentInChildren<RewardSummaryView>(true);

        if (ui_image_reward_fly_anchor == null)
            ui_image_reward_fly_anchor =
                UIAutoBinder.FindComponentInChildren<RectTransform>(transform, "ui_image_reward_fly_anchor");
        if (leavePopupView == null)
            leavePopupView = GetComponentInChildren<LeaveSummaryPopupView>(true);
    }

    private void Awake()
    {
        if (ui_button_spin == null)
        {
            ui_button_spin = UIAutoBinder.FindComponentInChildren<Button>(transform, "ui_button_spin");
            //Debug.Log(ui_button_spin);
        }
           

        if (ui_button_leave == null)
            ui_button_leave = UIAutoBinder.FindComponentInChildren<Button>(transform, "ui_button_leave");

        if (wheelView == null)
            wheelView = GetComponentInChildren<WheelView>(true);

        if (hudView == null)
            hudView = GetComponentInChildren<HudView>(true);

        if (bombPopupView == null)
            bombPopupView = GetComponentInChildren<BombPopupView>(true);
        
        if (rewardSummaryView == null)
            rewardSummaryView = GetComponentInChildren<RewardSummaryView>(true);

        if (ui_image_reward_fly_anchor == null)
            ui_image_reward_fly_anchor =
                UIAutoBinder.FindComponentInChildren<RectTransform>(transform, "ui_image_reward_fly_anchor");
        if (leavePopupView == null)
            leavePopupView = GetComponentInChildren<LeaveSummaryPopupView>(true);

        _zoneResolver = new ScriptableZoneTypeResolver();
        _progression = new LinearRewardProgressionStrategy(progressionConfig);
        _random = new UnityRandomProvider();
        _wheelProvider = new ScriptableWheelDefinitionProvider(
            new List<WheelLayoutConfig> { normalZoneConfig, safeZoneConfig, superZoneConfig }
        );

        StartNewSession();
    }

    private void OnEnable()
    {
        if (ui_button_spin != null)
            ui_button_spin.onClick.AddListener(OnSpinClicked);

        if (ui_button_leave != null)
            ui_button_leave.onClick.AddListener(OnLeaveClicked);

        if (bombPopupView != null)
            bombPopupView.SetCallbacks(OnGiveUpClicked, null, null);
        
        if (leavePopupView != null)
            leavePopupView.SetPlayAgainCallback(OnPlayAgainClicked);

    }

    private void OnDisable()
    {
        if (ui_button_spin != null)
            ui_button_spin.onClick.RemoveListener(OnSpinClicked);

        if (ui_button_leave != null)
            ui_button_leave.onClick.RemoveListener(OnLeaveClicked);

        if (bombPopupView != null)
            bombPopupView.ClearCallbacks();
    }

    private void StartNewSession()
    {
        _session = new WheelGameSession(
            _zoneResolver,
            _wheelProvider,
            _progression,
            _random
        );

        _rewardTotals.Clear();
        _isSpinning = false;
        bombPopupView?.Hide();
        RefreshAllUI();
        rewardSummaryView?.SetTotals(_rewardTotals);
    }

    private void RefreshAllUI()
    {
        UpdateHud();
        BuildWheelForCurrentZone();
        UpdateButtons();
    }

    private void UpdateHud()
    {
        if (hudView == null || _session == null) return;

        int zoneIndex = _session.CurrentZoneIndex;
        ZoneType zoneType = _session.CurrentZoneType;
        int totalReward = _session.TotalReward;

        int nextSafe = GetNextMultiple(zoneIndex, 5);
        int nextSuper = GetNextMultiple(zoneIndex, 30);

        hudView.SetZone(zoneIndex, zoneType);
        hudView.SetTotalReward(totalReward);
        hudView.SetNextSafeZone(nextSafe);
        hudView.SetNextSuperZone(nextSuper);

        float growth = progressionConfig.growthPerZone;
        float factor = 1f + growth * (zoneIndex - 1);
        wheelView.SetMultiplier(factor);
    }

    private void BuildWheelForCurrentZone()
    {
        if (wheelView == null || _session == null) return;

        var layout = _wheelProvider.GetLayoutConfigFor(_session.CurrentZoneType);
        
        // Calculate amounts in controller (not in UI layer)
        var calculatedAmounts = new List<int>();
        foreach (var sliceConfig in layout.slices)
        {
            if (sliceConfig.sliceType == SliceType.Bomb)
            {
                calculatedAmounts.Add(0); // Bombs have no amount
            }
            else
            {
                int scaledAmount = _progression.GetAmount(sliceConfig.baseAmount, _session.CurrentZoneIndex);
                calculatedAmounts.Add(scaledAmount);
            }
        }
        
        // Pass pre-calculated amounts to UI (no domain interface dependency)
        wheelView.BuildFromLayout(layout, calculatedAmounts);
    }

    private void UpdateButtons()
    {
        if (_session == null) return;

        bool canSpin = !_isSpinning && _session.CanSpin;
        bool canLeave = !_isSpinning && _session.CanLeave;

        if (ui_button_spin != null)
            ui_button_spin.interactable = canSpin;

        if (ui_button_leave != null)
            ui_button_leave.interactable = canLeave;
    }

    private void OnSpinClicked()
    {
        if (_session == null || !_session.CanSpin || _isSpinning)
            return;

        _isSpinning = true;
        UpdateButtons();

        int sliceIndex = _session.ChooseSliceIndex();
        _lastSpinSliceIndex = sliceIndex;

        wheelView.SpinToSlice(sliceIndex, () =>
        {
            SpinResult result = _session.ResolveSpin(sliceIndex);
            HandleSpinResult(result);
        });
    }

    private void HandleSpinResult(SpinResult result)
    {
        //Debug.Log($"[SPIN] Zone={result.ZoneIndex}, SliceIndex={result.LandedSlice}, " +
          //    $"Type={result.LandedSlice.Reward?.Type}, Delta={result.RewardDelta}");

        if (result.HitBomb)
        {
            _rewardTotals.Clear();
            bombPopupView?.Show();

            UpdateHud();
            rewardSummaryView?.ClearAll(); 
            // rewardSummaryView?.SetTotals(_rewardTotals);

            _isSpinning = false;
            UpdateButtons();
            return;
        }

        RectTransform targetAnchor = null;

        if (result.LandedSlice.Reward != null && result.RewardDelta > 0)
        {
            var type = result.LandedSlice.Reward.Type;
            if (_rewardTotals.TryGetValue(type, out var old))
                _rewardTotals[type] = old + result.RewardDelta;
            else
                _rewardTotals[type] = result.RewardDelta;
        }

        UpdateHud();
        rewardSummaryView?.SetTotals(_rewardTotals);

        // get row anchor for this reward type
        if (result.LandedSlice.Reward != null && rewardSummaryView != null)
        {
            var type = result.LandedSlice.Reward.Type;
            targetAnchor = rewardSummaryView.GetAnchorFor(type);
        }

        // fallback
        if (targetAnchor == null && ui_image_reward_fly_anchor != null)
            targetAnchor = ui_image_reward_fly_anchor;

        if (wheelView != null && targetAnchor != null && _lastSpinSliceIndex >= 0)
        {
            wheelView.PlayCollectAnimation(_lastSpinSliceIndex, targetAnchor, 0.5f, () =>
            {
                if (!_session.IsFinished && !_session.IsBombHit)
                {
                    BuildWheelForCurrentZone();
                }

                _isSpinning = false;
                UpdateButtons();
            });
        }
        else
        {
            if (!_session.IsFinished && !_session.IsBombHit)
            {
                BuildWheelForCurrentZone();
            }

            _isSpinning = false;
            UpdateButtons();
        }
    }

    private void OnLeaveClicked()
    {
        if (_session == null || !_session.CanLeave || _isSpinning)
        return;

        LeaveResult result = _session.Leave();

        UpdateHud();
        UpdateButtons();

        if (leavePopupView != null)
        {
            leavePopupView.Show(_rewardTotals, result.TotalReward);
        }
    }

    private void OnGiveUpClicked()
    {
        // after hitting bomb and giving up -> restart the game again as instructed
        StartNewSession();
    }

    private void OnPlayAgainClicked()
    {
        // close popup then start fresh
        leavePopupView?.Hide();
        StartNewSession();
    }


    private static int GetNextMultiple(int current, int step)
    {
        if (step <= 0) return current;
        int remainder = current % step;
        return remainder == 0 ? current : current + (step - remainder);
    }
}
