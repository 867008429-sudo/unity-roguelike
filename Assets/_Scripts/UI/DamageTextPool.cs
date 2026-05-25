using System.Collections.Generic;
using UnityEngine;

public class DamageTextPool : MonoBehaviour
{
    public static DamageTextPool Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject obj = new GameObject("DamageTextPool");
                instance = obj.AddComponent<DamageTextPool>();
                instance.Initialize();
            }

            return instance;
        }
    }

    public int prewarmCount = 32;

    private static DamageTextPool instance;
    private readonly Queue<DamageTextPopup> pool = new Queue<DamageTextPopup>();
    private bool initialized;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        Initialize();
    }

    public void Initialize()
    {
        if (initialized)
        {
            return;
        }

        initialized = true;
        for (int i = 0; i < prewarmCount; i++)
        {
            pool.Enqueue(CreatePopup());
        }
    }

    public void ShowDamage(Vector3 position, int amount, bool critical = false, Color? color = null)
    {
        DamageTextPopup popup = pool.Count > 0 ? pool.Dequeue() : CreatePopup();
        popup.Play(position, amount, critical, color);
    }

    public void ShowText(Vector3 position, string text, Color color, bool emphasized = false)
    {
        DamageTextPopup popup = pool.Count > 0 ? pool.Dequeue() : CreatePopup();
        popup.PlayText(position, text, color, emphasized);
    }

    public void Release(DamageTextPopup popup)
    {
        if (popup == null)
        {
            return;
        }

        popup.gameObject.SetActive(false);
        pool.Enqueue(popup);
    }

    private DamageTextPopup CreatePopup()
    {
        GameObject obj = new GameObject("DamageTextPopup");
        obj.transform.SetParent(transform, false);
        DamageTextPopup popup = obj.AddComponent<DamageTextPopup>();
        popup.Initialize(this);
        return popup;
    }
}
