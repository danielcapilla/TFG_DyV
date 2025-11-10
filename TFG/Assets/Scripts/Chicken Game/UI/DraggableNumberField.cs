using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.Events;

public class DraggableNumberField : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Configuración")]
    [SerializeField] float sensitivity = 0.2f;

    [Header("Eventos")]
    public UnityEvent<int> onValueChanged;

    private TMP_InputField inputField;
    private Vector2 startPos;
    private int startValue;
    private bool dragging;

    public int MinValue { get; set; } = 0;
    public int MaxValue { get; set; } = 100;

    void Awake()
    {
        inputField = GetComponent<TMP_InputField>();

#if UNITY_ANDROID || UNITY_IOS
        inputField.readOnly = true;
#else
        inputField.readOnly = false;
        inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
#endif

        inputField.onEndEdit.AddListener(OnEndEditValidate);
    }

    private void OnDestroy()
    {
        inputField.onEndEdit.RemoveListener(OnEndEditValidate);
    }

    private void OnEndEditValidate(string text)
    {
        // Asegurarse de que el valor este dentro de los limites
        if (!int.TryParse(text, out int value))
            value = MinValue;

        value = Mathf.Clamp(value, MinValue, MaxValue);
        SetValue(value);
        onValueChanged?.Invoke(value);
    }

    public void SetValue(int value)
    {
        value = Mathf.Clamp(value, MinValue, MaxValue);
        inputField.text = value.ToString();
    }

    public int GetValue()
    {
        if (int.TryParse(inputField.text, out int v))
            return Mathf.Clamp(v, MinValue, MaxValue);
        return MinValue;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        dragging = true;
        startPos = eventData.position;
        startValue = GetValue();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!dragging) return;

        float delta = (eventData.position.y - startPos.y) * sensitivity;
        int newValue = Mathf.Clamp(startValue + Mathf.RoundToInt(delta), MinValue, MaxValue);

        if (newValue != GetValue())
        {
            SetValue(newValue);
            onValueChanged?.Invoke(newValue);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        dragging = false;
    }
}
