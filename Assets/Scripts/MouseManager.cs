﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
[DisallowMultipleComponent]
public class MouseManager : MonoBehaviour
{
    private PlayerInput m_PlayerInput;
    private InputAction m_LocationAction;
    private InputAction m_HoldAction;

    private ClickType click;
    private GameObject lastHit;

    public Vector2 MouseLocation => 
        Camera.main.ScreenToWorldPoint(m_LocationAction.ReadValue<Vector2>());

    private int layerMask;

    private void Start()
    {
        click = ClickType.Clear;
        lastHit = null;
        layerMask = 1 << 9;
    }

    private void FixedUpdate()
    {
        if (m_PlayerInput == null) 
        {
            m_PlayerInput = GetComponent<PlayerInput>();
            m_LocationAction = m_PlayerInput.actions["Location"];
            m_HoldAction = m_PlayerInput.actions["Hold"];
        }

        Vector2 screenLocation = m_LocationAction.ReadValue<Vector2>();
        var hold = m_HoldAction.ReadValue<float>();

        if (click == ClickType.Unclick)
        {
            click = ClickType.Clear;
        }
        if (click == ClickType.Click)
        {
            click = ClickType.Hold;
        }

        if (click == ClickType.Clear && hold > 0) 
        {
            click = ClickType.Click;
        }
        if (click == ClickType.Hold && hold == 0)
        {
            click = ClickType.Unclick;
        }

        notifyObject(click, screenLocation);

        //Debug.Log("Click: " + click + ", Hold: " + hold + ", Location: (" + screenLocation.x + ", " + screenLocation.y + ")");
    }

    private void notifyObject(ClickType clickEvent, Vector2 screenLocation)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenLocation);
        RaycastHit hit;
        GameObject obj;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
        {
            obj = hit.transform.gameObject;
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.yellow);

            if (obj != null)
            {
                if (lastHit != obj) 
                {
                    if (lastHit != null)
                    {
                        foreach(IIntractable lsht in lastHit.GetComponents<IIntractable>())
                            lsht.OnLeave(clickEvent);
                    }
                    foreach (IIntractable obb in obj.GetComponents<IIntractable>())
                        obb.OnEnter(clickEvent);
                    lastHit = obj;
                }
                foreach (IIntractable obb in obj.GetComponents<IIntractable>())
                    obb.UpdateMouseState(clickEvent, Camera.main.ScreenToWorldPoint(screenLocation));
            }
        }
        else 
        {
            Debug.DrawRay(ray.origin, ray.direction * 50, Color.yellow);
            if (lastHit != null) 
            {
                foreach (IIntractable lsht in lastHit.GetComponents<IIntractable>())
                    lsht.OnLeave(clickEvent);
                lastHit = null;
            }
        }
    } 
}

public enum ClickType 
{
    Click,
    Hold,
    Unclick,
    Clear
}
