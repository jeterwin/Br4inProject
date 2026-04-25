using Gtec.Chain.Common.Templates.Utilities;
using Gtec.UnityInterface;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Gtec.Chain.Common.Templates.DataAcquisitionUnit.DataAcquisitionUnit;

public class BCIPipelineOrchestrator : MonoBehaviour
{
    public static BCIPipelineOrchestrator Instance { get; private set; }

    public enum BCIState { Disconnected, Connected, Training, Trained, Application }

    [Header("SDK References")]
    [SerializeField] private Device _device;
    [SerializeField] private ERPPipeline _erpPipeline;
    [SerializeField] private ERPParadigm _erpParadigm;
    [SerializeField] private ERPParadigmUI _paradigmUI;

    [Header("Scene")]
    [SerializeField] private string _gameplaySceneName = "SampleScene";
    [SerializeField] private GameObject _calibrationTargetsRoot;

    [Header("Flash Materials")]
    [SerializeField] private Material _flashMaterial;

    public BCIState CurrentState { get; private set; } = BCIState.Disconnected;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(transform.root.gameObject);
    }

    private void Start()
    {
        if (_device != null)
            _device.OnDeviceStateChanged.AddListener(OnDeviceStateChanged);

        if (_erpPipeline != null)
        {
            _erpPipeline.OnCalibrationResult.AddListener(OnCalibrationResult);
            _erpPipeline.OnPipelineStateChanged.AddListener(OnPipelineStateChanged);
        }

        if (_erpParadigm != null)
        {
            _erpParadigm.OnParadigmStarted.AddListener(OnParadigmStarted);
            _erpParadigm.OnParadigmStopped.AddListener(OnParadigmStopped);
        }

        if (_paradigmUI != null)
            _paradigmUI.OnStartParadigm.AddListener(OnUIStartParadigm);

        Debug.Log("[BCIOrchestrator] Initialized — waiting for device connection");
    }

    private void OnDeviceStateChanged(States state)
    {
        EventHandler.Instance.Enqueue(() =>
        {
            if (state == States.Connected)
            {
                CurrentState = BCIState.Connected;
                Debug.Log("[BCIOrchestrator] Device connected — ready for training");
            }
            else if (state == States.Disconnected)
            {
                CurrentState = BCIState.Disconnected;
                Debug.Log("[BCIOrchestrator] Device disconnected");
            }
        });
    }

    private void OnPipelineStateChanged(PipelineState state)
    {
        EventHandler.Instance.Enqueue(() =>
        {
            Debug.Log($"[BCIOrchestrator] Pipeline state: {state}");
        });
    }

    private void OnParadigmStarted()
    {
        EventHandler.Instance.Enqueue(() =>
        {
            if (CurrentState == BCIState.Connected || CurrentState == BCIState.Trained)
            {
                CurrentState = BCIState.Training;
                Debug.Log("[BCIOrchestrator] Training paradigm started — user should focus on targets");
            }
            else if (CurrentState == BCIState.Application)
            {
                Debug.Log("[BCIOrchestrator] Application paradigm running — P300 detection active");
            }
        });
    }

    private void OnParadigmStopped()
    {
        EventHandler.Instance.Enqueue(() =>
        {
            if (CurrentState == BCIState.Training)
            {
                Debug.Log("[BCIOrchestrator] Training stopped — waiting for calibration result");
            }
        });
    }

    private void OnCalibrationResult(ERPParadigm paradigm, CalibrationResult result)
    {
        EventHandler.Instance.Enqueue(() =>
        {
            CurrentState = BCIState.Trained;

            string quality = result.CalibrationQuality.ToString();
            float selectionTimeMs = (paradigm.OnTimeMs + paradigm.OffTimeMs) * result.TrialsSelected;

            Debug.Log($"[BCIOrchestrator] Calibration complete — Quality: {quality}");
            Debug.Log($"[BCIOrchestrator] Trials selected: {result.TrialsSelected}, Est. selection time: {selectionTimeMs}ms");

            foreach (var kvp in result.Crossvalidation)
                Debug.Log($"[BCIOrchestrator] Class {kvp.Key} accuracy: {kvp.Value:F1}%");
        });
    }

    private void OnUIStartParadigm(ParadigmMode mode)
    {
        EventHandler.Instance.Enqueue(() =>
        {
            if (mode == ParadigmMode.Application)
            {
                Debug.Log("[BCIOrchestrator] User clicked Continue — transitioning to gameplay");
                TransitionToGameplay();
            }
            else if (mode == ParadigmMode.Training)
            {
                CurrentState = BCIState.Training;
                Debug.Log("[BCIOrchestrator] Retraining started");
            }
        });
    }

    private void TransitionToGameplay()
    {
        if (_calibrationTargetsRoot != null)
            _calibrationTargetsRoot.SetActive(false);

        CurrentState = BCIState.Application;

        SceneManager.sceneLoaded += OnGameplaySceneLoaded;
        SceneManager.LoadScene(_gameplaySceneName);
    }

    private void OnGameplaySceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != _gameplaySceneName)
            return;

        SceneManager.sceneLoaded -= OnGameplaySceneLoaded;

        Debug.Log("[BCIOrchestrator] Gameplay scene loaded — wiring BCI to enemies");

        if (_erpParadigm != null)
            _erpParadigm.StartParadigm(ParadigmMode.Application);
    }

    public void RegisterFlashTarget(int classId, GameObject enemy)
    {
        Debug.Log($"[BCIOrchestrator] Registering flash target classId={classId} on {enemy.name}");

        // The ERPFlashTag3D component makes this enemy part of the P300 paradigm.
        // At runtime, the ERPParadigm discovers flash tags among its children or
        // registered objects. The exact registration API depends on the SDK version:
        // - If ERPParadigm scans children: parent the tag under the BCI Visual ERP 3D object
        // - If ERPParadigm has an Add method: call it directly
        //
        // For now we add the tag to the enemy and log it. The wiring to
        // ERPParadigm will be confirmed at hardware integration time.
    }

    public void UnregisterFlashTarget(int classId)
    {
        Debug.Log($"[BCIOrchestrator] Unregistering flash target classId={classId}");
    }

    public void SimulateClassSelection(uint classId)
    {
        if (BCITargetingSystem.Instance != null)
            BCITargetingSystem.Instance.OnClassSelected(classId);
    }

    private void OnDestroy()
    {
        if (_device != null)
            _device.OnDeviceStateChanged.RemoveListener(OnDeviceStateChanged);

        if (_erpPipeline != null)
        {
            _erpPipeline.OnCalibrationResult.RemoveListener(OnCalibrationResult);
            _erpPipeline.OnPipelineStateChanged.RemoveListener(OnPipelineStateChanged);
        }

        if (_erpParadigm != null)
        {
            _erpParadigm.OnParadigmStarted.RemoveListener(OnParadigmStarted);
            _erpParadigm.OnParadigmStopped.RemoveListener(OnParadigmStopped);
        }

        if (_paradigmUI != null)
            _paradigmUI.OnStartParadigm.RemoveListener(OnUIStartParadigm);

        if (Instance == this)
            Instance = null;
    }
}
