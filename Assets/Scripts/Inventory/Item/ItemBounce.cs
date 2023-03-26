using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.Inventory
{
    public class ItemBounce : MonoBehaviour
    {
        public float gravity = -3.5f;
        private Transform _spriteTransform;
        private BoxCollider2D _collider2D;
        private bool _isGround;
        private float _distance;
        private Vector2 _direction;
        private Vector3 _targetPos;

        private void Awake()
        {
            _spriteTransform = transform.GetChild(0);
            _collider2D = GetComponent<BoxCollider2D>();
            _collider2D.enabled = false;
        }

        private void Update()
        {
            Bounce();
        }

        public void InitBounceItem(Vector3 targetPos, Vector2 dir)
        {
            Debug.Log(Vector3.Distance(transform.position, _targetPos));
            _collider2D.enabled = false;
            _direction = dir;
            _targetPos = targetPos;
            _distance = Vector3.Distance(targetPos, transform.position);

            _spriteTransform.position += Vector3.up * 2f; // 人物举着item时，item定义为在Player的y轴为2的地方
        }

        private void Bounce()
        {
            _isGround = _spriteTransform.position.y <= transform.position.y;

            if (Vector3.Distance(transform.position, _targetPos) > 0.1f)
            {
                transform.position += (Vector3)_direction * _distance * -gravity * Time.deltaTime;
            }

            if (!_isGround)
            {
                _spriteTransform.position += Vector3.up * gravity * Time.deltaTime;
            }
            else
            {
                _spriteTransform.position = transform.position;
                _collider2D.enabled = true;
            }
        }
    }
}
