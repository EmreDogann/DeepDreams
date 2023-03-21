using MyBox;
using UnityEngine;

namespace DeepDreams
{
#if UNITY_EDITOR
    public class BanisterPlacer : MonoBehaviour
    {
        private enum SpacingType
        {
            Even,
            Custom
        }

        [SerializeField] private GameObject banisterLegPrefab;
        [SerializeField] private GameObject[] markers;
        [SerializeField] private SpacingType spacingMode;

        [ConditionalField(nameof(spacingMode), false, SpacingType.Even)]
        [OverrideLabel("Spacing (Between each Leg)")] [SerializeField] private float spacing;
        [ConditionalField(nameof(spacingMode), false, SpacingType.Custom)]
        [OverrideLabel("Number of Banister Legs")] [SerializeField] private float numberOfLegs;

        [ButtonMethod]
        private void PlaceBanisterLegs()
        {
            if (spacingMode == SpacingType.Even)
            {
                for (int i = 0; i < markers.Length - 1; i++)
                {
                    Vector2 currentMarkerPos = new Vector2(markers[i].transform.parent.position.x, markers[i].transform.parent.position.z);
                    Vector2 nextMarkerPos = new Vector2(markers[i + 1].transform.parent.position.x,
                        markers[i + 1].transform.parent.position.z);

                    float distance = Vector2.Distance(currentMarkerPos, nextMarkerPos);
                    float legSpacing = distance * spacing;
                    Vector2 direction = (nextMarkerPos - currentMarkerPos).normalized;

                    float remainingDistance = distance;
                    int legIndex = 1;

                    while (remainingDistance > 0.0f)
                    {
                        Instantiate(banisterLegPrefab,
                            new Vector3(currentMarkerPos.x + spacing * legIndex * direction.x, markers[i].transform.parent.position.y,
                                currentMarkerPos.y + spacing * legIndex * direction.y), Quaternion.identity);

                        remainingDistance -= spacing;
                        legIndex++;
                    }
                }
            }
            else
            {
                for (int i = 0; i < markers.Length - 1; i++)
                {
                    Vector2 currentMarkerPos = new Vector2(markers[i].transform.parent.position.x, markers[i].transform.parent.position.z);
                    Vector2 nextMarkerPos = new Vector2(markers[i + 1].transform.parent.position.x,
                        markers[i + 1].transform.parent.position.z);

                    float distance = Vector2.Distance(currentMarkerPos, nextMarkerPos);
                    float legSpacing = distance / (numberOfLegs + 1);
                    Vector2 direction = (nextMarkerPos - currentMarkerPos).normalized;

                    for (int j = 0; j < numberOfLegs; j++)
                    {
                        Instantiate(banisterLegPrefab,
                            new Vector3(currentMarkerPos.x + legSpacing * (j + 1) * direction.x, markers[i].transform.parent.position.y,
                                currentMarkerPos.y + legSpacing * (j + 1) * direction.y), Quaternion.identity);
                    }
                }
            }
        }
    }
#endif
}