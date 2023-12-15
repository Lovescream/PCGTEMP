using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerInputController : MonoBehaviour {

    private Player player;
    private CharacterController2D CC2D;

    private GraphicRaycaster graphicRaycaster;

    void Awake() {
        player = this.GetComponent<Player>();
        CC2D = this.GetComponent<CharacterController2D>();
        graphicRaycaster = FindObjectOfType<GraphicRaycaster>();
    }

    void Update() {
        if (EventSystem.current.IsPointerOverGameObject()) {
            var ped = new PointerEventData(null);
            ped.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            graphicRaycaster.Raycast(ped, results);

            if (results.Count > 0) {
                if (!FindInfoUIManager(results[0].gameObject.transform)) return;
            }
        }

        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        CC2D.SetInputVelocity(inputX, inputY);

        if (Input.GetKeyDown(KeyCode.Space)) {
            if (inputY < 0) CC2D.DownJump();
            else CC2D.Jump();
        }
        if (Input.GetKeyDown(KeyCode.Tab)) {
            player.SwapWeapon();
        }
        if (Input.GetMouseButtonDown(0)) {
            player.CurrentMainHand.OrderAttack();
        }
        if (Input.GetMouseButtonDown(1)) {
            CC2D.Dash((mousePosition - (Vector2)this.transform.position).normalized);
        }

        float angle = Utility.GetAngle(this.transform.position, mousePosition);
        player.CurrentMainHand.SetAngle(angle);
        CC2D.Flip(mousePosition.x > this.transform.position.x);
    }

    private bool FindInfoUIManager(Transform t) {
        for(int i = 0; i < 100; i++) {
            if (t.name == "InfoUIManager") return true;
            else {
                if (t.parent != null) t = t.parent;
                else return false;
            }
        }
        return false;
    }

}