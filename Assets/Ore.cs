using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Collider2D))]
public class ClickableOre : MonoBehaviour
{

    void OnMouseDown()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
             return;
        }

        GlowingOreManager manager = FindFirstObjectByType<GlowingOreManager>();

        if (manager != null)
        {
            manager.TryCollectOre(this.gameObject);
        }
        else
        {
            Debug.LogError("GlowingOreManager not found in the scene!", this);
        }
    }
}