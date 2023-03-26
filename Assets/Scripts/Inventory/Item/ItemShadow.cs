using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.Inventory
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class ItemShadow : MonoBehaviour
    {
        public SpriteRenderer itemSprite;
        private SpriteRenderer _shadowSprite;

        private void Awake()
        {
            _shadowSprite = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            _shadowSprite.sprite = itemSprite.sprite;
            _shadowSprite.color = new Color(0, 0, 0, 0.3f);
        }
    }
}
