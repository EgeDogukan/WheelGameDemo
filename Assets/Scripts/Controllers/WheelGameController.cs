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

    [SerializeField] private Button ui_button_spin;
    [SerializeField] private Button ui_button_leave;

    [SerializeField] private RewardSummaryView rewardSummaryView;

    private readonly Dictionary<RewardType, int> _rewardTotals = new();


    // domain + adapters
    private ScriptableZoneTypeResolver _zoneResolver;
    private ScriptableWheelDefinitionProvider _wheelProvider;
    private LinearRewardProgressionStrategy _progression;
    private UnityRandomProvider _random;
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

        _zoneResolver = new ScriptableZoneTypeResolver();
        _progression = new LinearRewardProgressionStrategy(progressionConfig);
        _random = new UnityRandomProvider();
        _wheelProvider = new ScriptableWheelDefinitionProvider(
            normalZoneConfig,
            safeZoneConfig,
            superZoneConfig
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
        wheelView.BuildFromLayout(layout, _progression, _session.CurrentZoneIndex);
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
            // lost everything
            _rewardTotals.Clear();
            bombPopupView?.Show();
        }
        else if (result.LandedSlice.Reward != null && result.RewardDelta > 0)
        {
            var type = result.LandedSlice.Reward.Type;
            if (_rewardTotals.TryGetValue(type, out var old))
                _rewardTotals[type] = old + result.RewardDelta;
            else
                _rewardTotals[type] = result.RewardDelta;
            //Debug.Log("HEYYYYY");
            //Debug.Log(result.LandedSlice.Reward.Type);
        }

        UpdateHud();
        rewardSummaryView?.SetTotals(_rewardTotals);

        _isSpinning = false;
        UpdateButtons();

        if (!_session.IsFinished && !_session.IsBombHit)
        {
            BuildWheelForCurrentZone();
        }
    }

    private void OnLeaveClicked()
    {
        if (_session == null || !_session.CanLeave || _isSpinning)
            return;

        LeaveResult result = _session.Leave();

        // can later show a summary popup here using result.TotalReward
        UpdateHud();
        UpdateButtons();
    }

    private void OnGiveUpClicked()
    {
        // after hitting bomb and giving up -> restart the game again as instructed
        StartNewSession();
    }

    private static int GetNextMultiple(int current, int step)
    {
        if (step <= 0) return current;
        int remainder = current % step;
        return remainder == 0 ? current : current + (step - remainder);
    }
}
