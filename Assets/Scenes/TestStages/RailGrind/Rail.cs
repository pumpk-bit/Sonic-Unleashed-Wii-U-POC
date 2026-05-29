using UnityEngine;

public class Rail : MonoBehaviour
{
    [Header("Rail Points (in order)")]
    public Transform[] points;

    [SerializeField] bool LazyBuildForMeMode = false;
    [Header("Loop")]
    public bool loop = false;

    private float[] segmentLengths;
    private float totalLength;

    void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player") return;

        var rb = other.GetComponentInParent<Rigidbody>();
        if (rb == null) return;

        var player = rb.GetComponentInChildren<PlayerRailGrinding>();
        SavedGrinding = player;
        if (player == null) return;

        SavedGrinding.StartGrinding(GetComponent<Rail>(), rb.velocity);
    }

    PlayerRailGrinding SavedGrinding;

    void Awake()
    {
        if (points == null || points.Length == 0)
        {
            Debug.LogWarning("No points assigned to the rail! Attempting to auto-assign...");
            LazyBuildForMeMode = true;
        }
        if (LazyBuildForMeMode)
        {
            // Automatically find all Transform components in children
            // (Excluding the parent object itself)
            var childTransforms = GetComponentsInChildren<Transform>();

            // Create a list to temporarily hold the points
            var pointList = new System.Collections.Generic.List<Transform>();

            foreach (var t in childTransforms)
            {
                if (t != this.transform) // Skip the parent
                {
                    pointList.Add(t);
                }
            }

            // Assign to your array
            points = pointList.ToArray();
        }


        BuildLengths();
    }

    void BuildLengths()
    {
        if (points == null || points.Length < 2)
            return;

        segmentLengths = new float[points.Length - 1];
        totalLength = 0f;

        for (int i = 0; i < points.Length - 1; i++)
        {
            float len = Vector3.Distance(points[i].position, points[i + 1].position);
            segmentLengths[i] = len;
            totalLength += len;
        }

        if (loop)
        {
            float len = Vector3.Distance(points[points.Length - 1].position, points[0].position);
            totalLength += len;
        }
    }

    public Vector3 GetPoint(float distance)
    {
        if (points.Length < 2)
            return transform.position;

        if (loop)
        {
            distance = Mathf.Repeat(distance, totalLength);
        }
        else
        {
            distance = Mathf.Clamp(distance, 0f, totalLength);
        }

        for (int i = 0; i < points.Length - 1; i++)
        {
            float segLen = segmentLengths[i];

            if (distance > segLen)
            {
                distance -= segLen;
                continue;
            }

            float t = distance / segLen;

            return Vector3.Lerp(
                points[i].position,
                points[i + 1].position,
                t
            );
        }

        //Unused

        if (loop)
        {
            float len = Vector3.Distance(
                points[points.Length - 1].position,
                points[0].position
            );

            float t = distance / len;

            return Vector3.Lerp(
                points[points.Length - 1].position,
                points[0].position,
                t
            );
        }

        return points[points.Length - 1].position;
    }

    public Vector3 GetDirection(float distance)
    {
        if (points.Length < 2)
            return transform.forward;

        distance = Mathf.Clamp(distance, 0f, totalLength);

        float travelled = 0f;

        for (int i = 0; i < points.Length - 1; i++)
        {
            float segLen = segmentLengths[i];

            if (distance <= travelled + segLen)
            {
                return (points[i + 1].position - points[i].position).normalized;
            }

            travelled += segLen;
        }

        return (points[points.Length - 1].position -
                points[points.Length - 2].position).normalized;
    }


    public float GetClosestDistance(Vector3 worldPos)
    {
        float bestDist = 0f;
        float bestSqr = float.MaxValue;

        float travelled = 0f;

        for (int i = 0; i < points.Length - 1; i++)
        {
            Vector3 a = points[i].position;
            Vector3 b = points[i + 1].position;

            Vector3 ab = b - a;
            float abLen = ab.magnitude;
            Vector3 dir = ab / abLen;

            Vector3 ap = worldPos - a;

            float t = Mathf.Clamp01(Vector3.Dot(ap, dir) / abLen);
            Vector3 closest = a + dir * (t * abLen);

            float sqr = (worldPos - closest).sqrMagnitude;

            if (sqr < bestSqr)
            {
                bestSqr = sqr;
                bestDist = travelled + t * abLen;
            }

            travelled += abLen;
        }

        return bestDist;
    }

    public float Length
    {
        get { return totalLength; }
    }
}
