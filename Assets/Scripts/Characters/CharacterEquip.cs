using System;
using UnityEngine;

public class CharacterEquip : CharacterBehaviour
{
    [SerializeField] protected EquipmentSlot m_leftHand;
    public ref EquipmentSlot leftHand => ref m_leftHand;

    [SerializeField] protected EquipmentSlot m_rightHand;
    public ref EquipmentSlot rightHand => ref m_rightHand;

    public CharacterEquip()
    {
        m_leftHand = new EquipmentSlot(-1);
        m_rightHand = new EquipmentSlot(1);
    }

    public override void OnUpdateCharacter()
    {
        m_leftHand.Update();
        m_rightHand.Update();
    }
}

[Serializable]
public struct EquipmentSlot
{
    [SerializeField, ReadOnly] private int m_id;
    [SerializeField] private GameObject m_source;
    [SerializeField] public bool locked;
    [SerializeField] private EquipData m_current;
    [SerializeField] private EquipData m_next;

    public EquipmentSlot(int id)
    {
        m_id = id;
        m_source = null;
        m_current = default;
        m_next = default;
        locked = false;
    }

    public void Update()
    {
        float targetStatusValue = 0;
        if (m_current.status == EquipStatus.Equip)
        {
            targetStatusValue = 1;
        }
        else if (m_current.status == EquipStatus.Unequip)
        {
            targetStatusValue = 0;
        }
        else
        {
            return;
        }

        if (targetStatusValue == 1)
        {
            m_current.statusValue = Mathf.MoveTowards(m_current.statusValue,
                targetStatusValue, m_current.equipSpeed * Time.deltaTime);

            if (m_current.statusValue == targetStatusValue)
            {
                m_current.status = EquipStatus.Equipped;
            }
        }
        else if (targetStatusValue == 0)
        {
            m_current.statusValue = Mathf.MoveTowards(m_current.statusValue,
                targetStatusValue, m_current.unequipSpeed * Time.deltaTime);

            if (m_current.statusValue == targetStatusValue)
            {
                m_current.status = EquipStatus.Unequipped;
            }
        }

        // if finalState reached
        if (m_current.status == EquipStatus.Equipped)
        {
            GameObject gameObject = m_current.gameObject;
            if (gameObject != null)
            {
                gameObject.transform.SetParent(m_source.transform, false);
                gameObject.transform.localPosition = m_current.localPositionOnEquip;
                gameObject.transform.localRotation = m_current.localRotationOnEquip;
            }
        }
        else if (m_current.status == EquipStatus.Unequipped)
        {
            GameObject gameObject = m_current.gameObject;
            if (gameObject != null)
            {
                gameObject.transform.SetParent(m_current.parentOnUnequip.transform, false);
                gameObject.transform.localPosition = m_current.localPositionOnUnequip;
                gameObject.transform.localRotation = m_current.localRotationOnUnequip;
            }
        }

        // update the events
        if (m_current.OnUpdate != null)
        {
            m_current.OnUpdate(m_current.equipable, m_current.status, m_current.statusValue);
        }

        // check if there is next to equip
        if (m_current.status == EquipStatus.Unequipped)
        {
            if (m_next.equipable != null)
            {
                m_current = m_next;
                m_current.status = EquipStatus.Equip;

                m_next.Clear();

                // // update the events to proveide the fresh values
                // if (m_current.OnUpdate != null)
                // {
                //     m_current.OnUpdate(m_current.equipable, m_current.equipStatus, m_current.equipMeter);
                // }
            }
        }
    }

    public bool CanEquip(IEquipable equipable)
    {
        return true;
    }

    public bool Equip(EquipData equipData)
    {
        m_next = equipData;
        m_current.status = EquipStatus.Unequip;

        return true;
    }

    public bool Unequip()
    {
        m_current.status = EquipStatus.Unequip;
        return true;
    }
}

[Serializable]
public struct EquipData
{
    public Action<IEquipable, EquipStatus, float> OnUpdate;

    public IEquipable equipable;
    public GameObject equipableObject;
    public EquipStatus status;
    public float statusValue;
    public float equipSpeed;
    public float unequipSpeed;
    public Vector3 localPositionOnEquip;
    public Quaternion localRotationOnEquip;
    public Vector3 localPositionOnUnequip;
    public Quaternion localRotationOnUnequip;
    public GameObject parentOnUnequip;

    public GameObject gameObject => equipable == null ? null : equipable.gameObject;

    public void Clear()
    {
        this = default;
    }

    public bool isNull
    {
        get
        {
            if (equipable == null)
                return true;

            return false;
        }
    }
}