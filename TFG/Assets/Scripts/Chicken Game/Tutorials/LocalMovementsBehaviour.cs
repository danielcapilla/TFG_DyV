using DG.Tweening;
using UnityEngine;

public class LocalMovementsBehaviour : MonoBehaviour
{
    [Header("Animación")]
    [SerializeField] private float appearDuration = 0.25f;
    [SerializeField] private float removeDuration = 0.15f;
    [SerializeField] private float idleSwingAngle = 8f;
    [SerializeField] private float idleSwingDuration = 2f;

    [Header("Referencias UI")]
    [SerializeField] private GameObject Movement;
    [SerializeField] private GameObject HorizontalLayout;

    [Header("Referencias")]
    [SerializeField] private TutorialGroupBehaviour tutorialGroupBehaviour;

    private void OnEnable()
    {
        if (tutorialGroupBehaviour != null)
        {
            tutorialGroupBehaviour.OnCommandAdded += HandleCommandAdded;
            tutorialGroupBehaviour.OnTurnExecuted += HandleExecutedTurn;
        }
    }

    private void OnDisable()
    {
        if (tutorialGroupBehaviour != null)
        {
            tutorialGroupBehaviour.OnCommandAdded -= HandleCommandAdded;
            tutorialGroupBehaviour.OnTurnExecuted -= HandleExecutedTurn;
        }
    }

    private void HandleCommandAdded(CommandType commandType)
    {
        SpawnMove(commandType);
    }

    private void HandleExecutedTurn()
    {
        RemoveMoves();
    }

    private void SpawnMove(CommandType commandType)
    {

        GameObject move = Instantiate(Movement, HorizontalLayout.transform);

        int childIndex = commandType switch
        {
            CommandType.MoveUp => 0,
            CommandType.MoveDown => 1,
            CommandType.MoveLeft => 2,
            CommandType.MoveRight => 3,
            CommandType.Wait => 4,
            _ => -1
        };

        move.transform.GetChild(childIndex).gameObject.SetActive(true);

        move.transform.localScale = Vector3.zero;
        move.transform
            .DOScale(Vector3.one, appearDuration)
            .SetEase(Ease.OutBack)
            .OnComplete(() =>
            {
                StartIdleSwing(move.transform);
            });
    }

    private void RemoveMoves()
    {
        foreach (Transform child in HorizontalLayout.transform)
        {
            DOTween.Kill(child);
            child.DOScale(Vector3.zero, removeDuration)
                 .SetEase(Ease.InBack)
                 .OnComplete(() => Destroy(child.gameObject));
        }
    }

    private void StartIdleSwing(Transform move)
    {
        float swingAngle = Random.Range(idleSwingAngle * 0.9f, idleSwingAngle * 1.1f);
        float duration = Random.Range(idleSwingDuration * 1.3f, idleSwingDuration * 1.7f);
        float randomDelay = Random.Range(0f, 0.6f);

        float startAngle = Random.Range(-swingAngle, swingAngle);
        move.localRotation = Quaternion.Euler(0f, 0f, startAngle);

        move.DORotate(new Vector3(0f, 0f, -swingAngle), duration / 2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .From(new Vector3(0f, 0f, swingAngle))
            .SetDelay(randomDelay);
    }
}
