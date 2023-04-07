using System;
using System.Collections;
using System.Collections.Generic;
using Farm.AStar;
using Farm.Save;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class NPCMovement : MonoBehaviour, ISaveable
{
    public ScheduleDataList_SO scheduleData;
    private SortedSet<ScheduleDetails> _scheduleSet;
    private ScheduleDetails _currentSchedule;
    
    //临时存储信息
    [SerializeField] public string currentScene;
    private string _targetScene;
    private Vector3Int _currentGridPosition;
    private Vector3Int _tragetGridPosition;
    private Vector3Int _nextGridPosition;
    private Vector3 _nextWorldPosition;

    public string StartScene
    {
        set => currentScene = value;
    }

    [Header("移动属性")] 
    public float normalSpeed = 2f;
    private float _minSpeed = 1;
    private float _maxSpeed = 3;
    private Vector2 _dir;
    public bool isMoving;
    
    //Components
    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;
    private BoxCollider2D _coll;
    private Animator _anim;
    private Grid _gird;
    private Stack<MovementStep> _movementSteps;
    private Coroutine _npcMoveRoutine;
    
    private bool _isInitialised;
    private bool _npcMove;
    private bool _sceneLoaded;
    public bool interactable;
    public bool isFirstLoad;
    private Season _curSeason;
    
    //动画计时器
    private float _animationBreakTime;
    private bool _canPlayStopAnimaiton;
    private AnimationClip _stopAnimationClip;
    public AnimationClip blankAnimationClip;
    private AnimatorOverrideController _animOverride;
    
    private TimeSpan GameTime => TimeManager.Instance.GameTime;
    
    public string GUID => GetComponent<DataGUID>().guid;
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _coll = GetComponent<BoxCollider2D>();
        _anim = GetComponent<Animator>();
        _movementSteps = new Stack<MovementStep>();
        
        _animOverride = new AnimatorOverrideController(_anim.runtimeAnimatorController);
        _anim.runtimeAnimatorController = _animOverride;
        _scheduleSet = new SortedSet<ScheduleDetails>();

        foreach (var schedule in scheduleData.scheduleList)
        {
            _scheduleSet.Add(schedule);
        }
    }
    
    private void OnEnable()
    {
        EventHandler.BeforeUnloadSceneEvent += OnBeforeSceneUnloadEvent;
        EventHandler.AfterLoadedSceneEvent += OnAfterSceneLoadedEvent;
        EventHandler.GameMinuteEvent += OnGameMinuteEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
    }

    private void OnDisable()
    {
        EventHandler.BeforeUnloadSceneEvent -= OnBeforeSceneUnloadEvent;
        EventHandler.AfterLoadedSceneEvent -= OnAfterSceneLoadedEvent;
        EventHandler.GameMinuteEvent -= OnGameMinuteEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
    }
    
    private void Start()
    {
        ISaveable saveable = this;
        saveable.RegisterSaveable();
    }

    private void Update()
    {
        if (_sceneLoaded)
            SwitchAnimation();

        //计时器
        _animationBreakTime -= Time.deltaTime;
        _canPlayStopAnimaiton = _animationBreakTime <= 0;
    }
    
    private void FixedUpdate()
    {
        if (_sceneLoaded)
            Movement();
    }
    
    private void OnGameMinuteEvent(int minute, int hour, int day, Season season)
    {
        int time = (hour * 100) + minute;

        ScheduleDetails matchSchedule = null;
        foreach (var schedule in _scheduleSet)
        {
            if (schedule.Time == time)
            {
                if (schedule.day != day && schedule.day != 0)
                    continue;
                if (schedule.season != season)
                    continue;
                matchSchedule = schedule;
            }
            else if (schedule.Time > time)
            {
                break;
            }
        }
        if (matchSchedule != null)
            BuildPath(matchSchedule);
    }
    
    private void OnBeforeSceneUnloadEvent()
    {
        _sceneLoaded = false;
    }
    
    private void OnAfterSceneLoadedEvent()
    {
        _gird = FindObjectOfType<Grid>();
        CheckVisiable();
        
        if (!_isInitialised)
        {
            InitNPC();
            _isInitialised = true;
        }
        
        _sceneLoaded = true;
        
        if (!isFirstLoad)
        {
            _currentGridPosition = _gird.WorldToCell(transform.position);
            var schedule = new ScheduleDetails(0, 0, 0, 0, _curSeason, _targetScene, (Vector2Int)_tragetGridPosition, _stopAnimationClip, interactable);
            BuildPath(schedule);
            isFirstLoad = true;
        }
    }
    
    private void OnEndGameEvent()
    {
        _sceneLoaded = false;
        _npcMove = false;
        if (_npcMoveRoutine != null)
            StopCoroutine(_npcMoveRoutine);
    }

    private void OnStartNewGameEvent(int obj)
    {
        _isInitialised = false;
        isFirstLoad = true;
    }
    
    // 主要移动方法
    private void Movement()
    {
        if (!_npcMove)
        {
            if (_movementSteps.Count > 0)
            {
                MovementStep step = _movementSteps.Pop();

                currentScene = step.sceneName;

                CheckVisiable();

                _nextGridPosition = (Vector3Int)step.gridCoordinate;
                TimeSpan stepTime = new TimeSpan(step.hour, step.minute, step.second);

                MoveToGridPosition(_nextGridPosition, stepTime);
            }else if (!isMoving && _canPlayStopAnimaiton)
            {
                StartCoroutine(SetStopAnimation());
            }
        }
    }
    
    private void MoveToGridPosition(Vector3Int gridPos, TimeSpan stepTime)
    {
        _npcMoveRoutine = StartCoroutine(MoveRoutine(gridPos, stepTime));
    }

    private IEnumerator MoveRoutine(Vector3Int gridPos, TimeSpan stepTime)
    {
        _npcMove = true;
        _nextWorldPosition = GetWorldPostion(gridPos);

        //还有时间用来移动
        if (stepTime > GameTime)
        {
            //用来移动的时间差，以秒为单位
            float timeToMove = (float)(stepTime.TotalSeconds - GameTime.TotalSeconds);
            //实际移动距离
            float distance = Vector3.Distance(transform.position, _nextWorldPosition);
            //实际移动速度
            float speed = Mathf.Max(_minSpeed, (distance / timeToMove / Settings.SecondThreshold));

            if (speed <= _maxSpeed)
            {
                while (Vector3.Distance(transform.position, _nextWorldPosition) > Settings.pixelSize)
                {
                    _dir = (_nextWorldPosition - transform.position).normalized;

                    Vector2 posOffset = new Vector2(_dir.x * speed * Time.fixedDeltaTime, _dir.y * speed * Time.fixedDeltaTime);
                    _rb.MovePosition(_rb.position + posOffset);
                    yield return new WaitForFixedUpdate();
                }
            }
        }
        //如果时间已经到了就瞬移
        _rb.position = _nextWorldPosition;
        _currentGridPosition = gridPos;
        _nextGridPosition = _currentGridPosition;

        _npcMove = false;
    }
    
    private void InitNPC()
    {
        _targetScene = currentScene;

        //保持在当前坐标的网格中心点
        _currentGridPosition = _gird.WorldToCell(transform.position);
        transform.position = new Vector3(_currentGridPosition.x + Settings.gridCellSize / 2f, _currentGridPosition.y + Settings.gridCellSize / 2f, 0);

        _tragetGridPosition = _currentGridPosition;
    }

    private void CheckVisiable()
    {
        if (currentScene == SceneManager.GetActiveScene().name)
            SetActiveInScene();
        else
            SetInactiveInScene();
    }

    // 根据Schedule构建路径
    public void BuildPath(ScheduleDetails schedule)
    {
        Debug.Log("BuildPath");
        _movementSteps.Clear();
        _currentSchedule = schedule;
        _targetScene = schedule.targetScene;
        _tragetGridPosition = (Vector3Int)schedule.targetGridPosition;
        _stopAnimationClip = schedule.clipAtStop;
        interactable = schedule.interactable;

        if (schedule.targetScene == currentScene)
        {
            AStar.Instance.BuildPath(schedule.targetScene, (Vector2Int)_currentGridPosition,
                schedule.targetGridPosition, _movementSteps);
        }else if (schedule.targetScene != currentScene)
        {
            SceneRoute sceneRoute = NPCManager.Instance.GetSceneRoute(currentScene, schedule.targetScene);

            if (sceneRoute != null)
            {
                for (int i = 0; i < sceneRoute.scenePathList.Count; i++)
                {
                    Vector2Int fromPos, gotoPos;
                    ScenePath path = sceneRoute.scenePathList[i];

                    if (path.fromGridCell.x >= Settings.maxGridSize)
                    {
                        fromPos = (Vector2Int)_currentGridPosition;
                    }
                    else
                    {
                        fromPos = path.fromGridCell;
                    }

                    if (path.gotoGridCell.x >= Settings.maxGridSize)
                    {
                        gotoPos = schedule.targetGridPosition;
                    }
                    else
                    {
                        gotoPos = path.gotoGridCell;
                    }

                    AStar.Instance.BuildPath(path.sceneName, fromPos, gotoPos, _movementSteps);
                }
            }
        }

        if (_movementSteps.Count > 1)
        {
            //更新每一步对应的时间戳
            UpdateTimeOnPath();
        }
    }
    
    // 更新路径上每一步的时间
    private void UpdateTimeOnPath()
    {
        MovementStep previousSetp = null;

        TimeSpan currentGameTime = GameTime;

        foreach (MovementStep step in _movementSteps)
        {
            if (previousSetp == null)
                previousSetp = step;

            step.hour = currentGameTime.Hours;
            step.minute = currentGameTime.Minutes;
            step.second = currentGameTime.Seconds;

            TimeSpan gridMovementStepTime;

            if (MoveInDiagonal(step, previousSetp))
                gridMovementStepTime = new TimeSpan(0, 0, (int)(Settings.gridCellDiagonalSize / normalSpeed / Settings.SecondThreshold));
            else
                gridMovementStepTime = new TimeSpan(0, 0, (int)(Settings.gridCellSize / normalSpeed / Settings.SecondThreshold));

            //累加获得下一步的时间戳
            currentGameTime = currentGameTime.Add(gridMovementStepTime);
            //循环下一步
            previousSetp = step;
        }
    }

    // 判断是否走斜方向
    private bool MoveInDiagonal(MovementStep currentStep, MovementStep previousStep)
    {
        return (currentStep.gridCoordinate.x != previousStep.gridCoordinate.x) && (currentStep.gridCoordinate.y != previousStep.gridCoordinate.y);
    }
    
    // 网格坐标返回世界坐标中心点
    private Vector3 GetWorldPostion(Vector3Int gridPos)
    {
        Vector3 worldPos = _gird.CellToWorld(gridPos);
        return new Vector3(worldPos.x + Settings.gridCellSize / 2f, worldPos.y + Settings.gridCellSize / 2);
    }
    
    private void SwitchAnimation()
    {
        isMoving = transform.position != GetWorldPostion(_tragetGridPosition);

        _anim.SetBool("IsMoving", isMoving);
        if (isMoving)
        {
            _anim.SetBool("Exit", true);
            _anim.SetFloat("DirX", _dir.x);
            _anim.SetFloat("DirY", _dir.y);
        }
        else
        {
            _anim.SetBool("Exit", false);
        }
    }
    
    private IEnumerator SetStopAnimation()
    {
        //强制面向镜头
        _anim.SetFloat("DirX", 0);
        _anim.SetFloat("DirY", -1);

        _animationBreakTime = Settings.animationBreakTime;
        if (_stopAnimationClip != null)
        {
            _animOverride[blankAnimationClip] = _stopAnimationClip;
            _anim.SetBool("EventAnimation", true);
            yield return null;
            _anim.SetBool("EventAnimation", false);
        }
        else
        {
            _animOverride[_stopAnimationClip] = blankAnimationClip;
            _anim.SetBool("EventAnimation", false);
        }
    }
    
    private void SetActiveInScene()
    {
        _spriteRenderer.enabled = true;
        _coll.enabled = true;

        transform.GetChild(0).gameObject.SetActive(true);
    }

    private void SetInactiveInScene()
    {
        _spriteRenderer.enabled = false;
        _coll.enabled = false;

        transform.GetChild(0).gameObject.SetActive(false);
    }

    public GameSaveData GenerateSaveData()
    {
        GameSaveData saveData = new GameSaveData();
        saveData.characterPosDict = new Dictionary<string, SerializableVector3>();
        saveData.characterPosDict.Add("targetGridPosition", new SerializableVector3(_tragetGridPosition));
        saveData.characterPosDict.Add("currentPosition", new SerializableVector3(transform.position));
        saveData.dataSceneName = currentScene;
        saveData.targetScene = this._targetScene;
        if (_stopAnimationClip != null)
        {
            saveData.animationInstanceID = _stopAnimationClip.GetInstanceID();
        }
        saveData.interactable = this.interactable;
        saveData.timeDict = new Dictionary<string, int>();
        saveData.timeDict.Add("currentSeason", (int)_curSeason);
        return saveData;
    }

    public void RestoreData(GameSaveData saveData)
    {
        _isInitialised = true;
        isFirstLoad = false;

        currentScene = saveData.dataSceneName;
        _targetScene = saveData.targetScene;

        Vector3 pos = saveData.characterPosDict["currentPosition"].ToVector3();
        Vector3Int gridPos = (Vector3Int)saveData.characterPosDict["targetGridPosition"].ToVector2Int();

        transform.position = pos;
        _tragetGridPosition = gridPos;

        if (saveData.animationInstanceID != 0)
        {
            _stopAnimationClip = Resources.InstanceIDToObject(saveData.animationInstanceID) as AnimationClip;
        }
        
        interactable = saveData.interactable;
        _curSeason = (Season)saveData.timeDict["currentSeason"];
    }
}