using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody2D _rb;

    public float speed;
    private float _inputX;
    private float _inputY;
    private bool _isMoving;

    private Vector2 _movementInput;
    private Animator[] _animators;
    private bool _inputDisable;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animators = GetComponentsInChildren<Animator>();
    }

    private void OnEnable()
    {
        EventHandler.BeforeUnloadSceneEvent += OnBeforeUnloadSceneEvent;
        EventHandler.AfterLoadedSceneEvent += OnAfterLoadedSceneEvent;
        EventHandler.MoveToPos += OnMoveToPos;
    }

    private void OnDisable()
    {
        EventHandler.BeforeUnloadSceneEvent -= OnBeforeUnloadSceneEvent;
        EventHandler.AfterLoadedSceneEvent -= OnAfterLoadedSceneEvent;
        EventHandler.MoveToPos -= OnMoveToPos;
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
        Movement();
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
            if (_isMoving)
            {
                animator.SetFloat("InputX", _inputX);
                animator.SetFloat("InputY", _inputY);
            }
        }
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
        Debug.Log(targetPos);
        transform.position = targetPos;
    }
}
