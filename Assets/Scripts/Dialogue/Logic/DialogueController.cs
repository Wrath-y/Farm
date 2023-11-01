using System;
using System.Collections;
using System.Collections.Generic;
using Cursor;
using UnityEngine;
using UnityEngine.Events;

namespace Farm.Dialogue
{
    [RequireComponent(typeof(NPCMovement))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class DialogueController : MonoBehaviour
    {
        private NPCMovement npc => GetComponent<NPCMovement>();
        public UnityEvent OnFinishEvent;
        public List<DialoguePiece> dialogueList = new List<DialoguePiece>();

        private Stack<DialoguePiece> _dailogueStack;

        private bool _canTalk;
        private bool _isTalking;
        private GameObject _uiSign;
        
        private void Awake()
        {
            _uiSign = transform.GetChild(1).gameObject;
            FillDialogueStack();
        }

        private void Update()
        {
            _uiSign.SetActive(_canTalk);

            if (_canTalk && !_isTalking && Input.touchCount > 0 && (Input.GetKeyDown(KeyCode.Space) || CursorClickEvent.Instance.IsShowNpcDialogue(CursorManager.Instance.GetMouseWorldPos())) )
            {
                StartCoroutine(DialogueRoutine());
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                _canTalk = !npc.isMoving && npc.interactable;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                _canTalk = false;
            }
        }

        // 构建对话堆栈
        private void FillDialogueStack()
        {
            _dailogueStack = new Stack<DialoguePiece>();
            for (int i = dialogueList.Count - 1; i > -1; i--)
            {
                dialogueList[i].isDone = false;
                _dailogueStack.Push(dialogueList[i]);
            }
        }
        
        private IEnumerator DialogueRoutine()
        {
            _isTalking = true;
            if (_dailogueStack.TryPop(out DialoguePiece result))
            {
                //传到UI显示对话
                EventHandler.CallShowDialogueEvent(result);
                EventHandler.CallUpdateGameStateEvent(GameState.Pause);
                yield return new WaitUntil(() => result.isDone);
                _isTalking = false;
            }
            else
            {
                EventHandler.CallUpdateGameStateEvent(GameState.Gameplay);
                EventHandler.CallShowDialogueEvent(null);
                FillDialogueStack();
                _isTalking = false;

                if (OnFinishEvent != null)
                {
                    OnFinishEvent.Invoke();
                    _canTalk = false;
                }
            }
        }
    }
}