using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StructureEntranceDetector : MonoBehaviour
{

    public enum EntranceType
    {
        Entrance,
        Exit
    }
    [SerializeField] private EntranceType _entranceType;
    [SerializeField] private GameObject _roofOB;
    [SerializeField] private MeshRenderer[] _materialRoofs;

    private IEnumerator ActivateRoofMaterial(bool active)
    {
        float alphaMat = active ? 1 : 0;

        if(alphaMat==1)
            _roofOB.SetActive(true); //activate when material lerp alpha done

        float lerpTime = 0.2f;
        while (Mathf.Abs(_materialRoofs[0].material.color.a - alphaMat) > 0.01f)
        {
            foreach (MeshRenderer meshRenderer in _materialRoofs)
            {
                Color newColor = meshRenderer.material.color;
                newColor.a = Mathf.Lerp(newColor.a, alphaMat, lerpTime);
                meshRenderer.material.SetColor("_BaseColor", newColor);
                yield return null;
            }
        }
        if (alphaMat == 0)
            _roofOB.SetActive(false);// deactive when material lerp alpha done


    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerManager player))
        {
            if (!player.hasAuthority) { return; }

            StopAllCoroutines();

            switch (_entranceType)
            {
                case EntranceType.Entrance:
                    {
                        StartCoroutine(ActivateRoofMaterial(false));
                        break;
                    }

                case EntranceType.Exit:
                    {
                        StartCoroutine(ActivateRoofMaterial(true));
                        break;
                    }
            }
        }
    }
}
