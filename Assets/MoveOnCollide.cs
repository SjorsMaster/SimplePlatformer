using Platformer.Mechanics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoveOnCollide : MonoBehaviour
{
    public RectTransform m_Rect;
    private void Awake() {
    }
    public Slider slider;

    void OnTriggerStay2D(Collider2D other) {

        if (other.tag == "Player") {
            slider.value = Mathf.Abs(returnWidth().x - other.transform.position.x) / Mathf.Abs(returnWidth().x -returnWidth().y);
            slider.Invoke("onSliderChanged",1);
        }
    }

    Vector2 returnWidth() {
        Vector3[] v = new Vector3[4];
        m_Rect.GetWorldCorners(v);
        return new Vector2(v[0].x, v[3].x);
    }
}
