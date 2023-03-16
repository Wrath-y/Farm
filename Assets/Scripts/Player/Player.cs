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

    private Vector2 _movementInput;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        PlayerInput();
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
            _inputX = _inputX * 0.6f;
            _inputY = _inputY * 0.6f;
        }
        _movementInput = new Vector2(_inputX, _inputY);
    }

    private void Movement()
    {
        _rb.MovePosition(_rb.position + _movementInput*(speed*Time.deltaTime));
    }
}
