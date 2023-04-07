using System;
using System.Collections;
using System.Collections.Generic;
using Farm.Save;
using UnityEngine;

public class Player : MonoBehaviour, ISaveable
{
    private Rigidbody2D _rb;

    public float speed;
    private float _inputX;
    private float _inputY;
    private bool _isMoving;
    private float _mouseX;
    private float _mouseY;

    private Vector2 _movementInput;
    private Animator[] _animators;
    private bool _inputDisable;
    private bool _useTool;
    
    public string GUID => GetComponent<DataGUID>().guid;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animators = GetComponentsInChildren<Animator>();
        _inputDisable = true;
    }
    
    private void Start()
    {
        ISaveable saveable = this;
        saveable.RegisterSaveable();
    }

    private void OnEnable()
    {
        EventHandler.BeforeUnloadSceneEvent += OnBeforeUnloadSceneEvent;
        EventHandler.AfterLoadedSceneEvent += OnAfterLoadedSceneEvent;
        EventHandler.MoveToPos += OnMoveToPos;
        EventHandler.MouseClickedEvent += OnMouseClickedEvent;
        EventHandler.UpdateGameStateEvent += OnUpdateGameStateEvent;
        EventHandler.StartNewGameEvent += OnStartNewGameEvent;
        EventHandler.EndGameEvent += OnEndGameEvent;
    }

    private void OnDisable()
    {
        EventHandler.BeforeUnloadSceneEvent -= OnBeforeUnloadSceneEvent;
        EventHandler.AfterLoadedSceneEvent -= OnAfterLoadedSceneEvent;
        EventHandler.MoveToPos -= OnMoveToPos;
        EventHandler.MouseClickedEvent -= OnMouseClickedEvent;
        EventHandler.UpdateGameStateEvent -= OnUpdateGameStateEvent;
        EventHandler.StartNewGameEvent -= OnStartNewGameEvent;
        EventHandler.EndGameEvent -= OnEndGameEvent;
    }

    private void Update()
    {
        if (!_inputDisable)
            PlayerInput();
        else
            _isMoving = false;
        SwitchAnimation();
    }

    private void FixedUpdate()
    {
        if (_inputDisable)
        {
            return;
        }
        Movement();
    }
    
    private void OnBeforeUnloadSceneEvent()
    {
        _inputDisable = true;
    }

    private void OnAfterLoadedSceneEvent()
    {
        _inputDisable = false;
    }

    private void OnMoveToPos(Vector3 targetPos)
    {
        transform.position = targetPos;
    }

    private void OnMouseClickedEvent(Vector3 mouseWorldPos, ItemDetails itemDetails)
    {
        if (_useTool)
            return;
        
        if (itemDetails.itemType != ItemType.Seed && itemDetails.itemType != ItemType.Commodity && itemDetails.itemType != ItemType.Furniture)
        {
            _mouseX = mouseWorldPos.x - transform.position.x;
            _mouseY = mouseWorldPos.y - (transform.position.y + 0.85f);
            if (Mathf.Abs(_mouseX) > MathF.Abs(_mouseY))
            {
                _mouseY = 0;
            }
            else
            {
                _mouseX = 0;
            }

            StartCoroutine(UseToolRoutine(mouseWorldPos, itemDetails));
            return;
        }
        EventHandler.CallExecuteActionAfterAnimation(mouseWorldPos, itemDetails);
    }
    
    private void OnUpdateGameStateEvent(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.Gameplay:
                _inputDisable = false;
                break;
            case GameState.Pause:
                _inputDisable = true;
                break;
        }
    }
    
    private void OnStartNewGameEvent(int obj)
    {
        _inputDisable = false;
        transform.position = Settings.playerStartPos;
    }

    private void OnEndGameEvent()
    {
        _inputDisable = true;
    }

    private void PlayerInput()
    {
        _inputX = Input.GetAxisRaw("Horizontal");
        _inputY = Input.GetAxisRaw("Vertical");

        if (_inputX != 0 && _inputY != 0)
        {
            _inputX *= 0.6f;
            _inputY *= 0.6f;
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            _inputX *= 0.5f;
            _inputY *= 0.5f;
        }
        _movementInput = new Vector2(_inputX, _inputY);
        _isMoving = _movementInput != Vector2.zero;
    }

    private void Movement()
    {
        _rb.MovePosition(_rb.position + _movementInput*(speed*Time.deltaTime));
    }

    private void SwitchAnimation()
    {
        foreach (var animator in _animators)
        {
            animator.SetBool("IsMoving", _isMoving);
            animator.SetFloat("MouseX", _mouseX);
            animator.SetFloat("MouseY", _mouseY);
            if (_isMoving)
            {
                animator.SetFloat("InputX", _inputX);
                animator.SetFloat("InputY", _inputY);
            }
        }
    }

    private IEnumerator UseToolRoutine(Vector3 mouseWorldPos, ItemDetails itemDetails)
    {
        _useTool = true;
        _inputDisable = true;
        yield return null;
        foreach (var animator in _animators)
        {
            animator.SetTrigger("UseTool");
            // 人物面朝方向
            animator.SetFloat("InputX", _mouseX);
            animator.SetFloat("InputY", _mouseY);
        }

        yield return new WaitForSeconds(0.45f);
        EventHandler.CallExecuteActionAfterAnimation(mouseWorldPos, itemDetails);
        yield return new WaitForSeconds(0.25f);

        _useTool = false;
        _inputDisable = false;
    }
    
    public GameSaveData GenerateSaveData()
    {
        GameSaveData saveData = new GameSaveData();
        saveData.characterPosDict = new Dictionary<string, SerializableVector3>();
        saveData.characterPosDict.Add(name, new SerializableVector3(transform.position));

        return saveData;
    }

    public void RestoreData(GameSaveData saveData)
    {
        var targetPosition = saveData.characterPosDict[name].ToVector3();

        transform.position = targetPosition;
    }
}
