using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// manages dynamic routing, congestion detection, & rerouting for AGVs.
// monitors path segments for congestion & provides alternative routes

public class RouteManager : MonoBehaviour{
    private static RouteManager instance;
    public static RouteManager Instance => instance;

    [Header("Route Management Settings")]
    [SerializeField] private float congestionCheckInterval = 0.5f;  // How often to check for congestion
    [SerializeField] private float congestionRadius = 2f;             // Radius to check for nearby AGVs
    [SerializeField] private int congestionThreshold = 2;             // Number of AGVs needed to mark as congested
    [SerializeField] private float pathValidationInterval = 1f;  
    [Header("Rerouting Settings")]      // Check for rerouting every N units
    [SerializeField] private bool enableDynamicRerouting = true;
    [SerializeField] private bool enableCongestionAvoidance = true;
    private List<RoviTransporter> registeredAGVs = new List<RoviTransporter>();

    //  track path segments and their congestion levels
    private Dictionary<Vector3, PathSegment> pathSegments = new Dictionary<Vector3, PathSegment>();

    //  track blocked areas (temporary obstacles)
    private HashSet<Vector3> blockedAreas = new HashSet<Vector3>();
    private Dictionary<Vector3, float> blockedAreaTimestamps = new Dictionary<Vector3, float>();

    private int totalReroutes = 0;
    private int congestionDetections = 0;

    private void Awake(){
        if (instance == null){
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else{
            Destroy(gameObject);
        }
    }

    private void Start(){
        StartCoroutine(CongestionMonitoringCoroutine());
        StartCoroutine(PathValidationCoroutine());
    }

    // register an AGV with the route manager
    public void RegisterAGV(RoviTransporter agv){
        if (!registeredAGVs.Contains(agv)){
            registeredAGVs.Add(agv);
        }
    }

    // unregister an AGV from the route manager
    public void UnregisterAGV(RoviTransporter agv){
        registeredAGVs.Remove(agv);
    }

    // check if path is congested & get alternative route if available
    public bool ShouldReroute(Vector3 currentPosition, Vector3 destination, out Vector3? alternativeRoute){
        alternativeRoute = null;

        if (!enableDynamicRerouting)
            return false;

        if (IsAreaBlocked(destination)){
            alternativeRoute = FindAlternativeDestination(destination);
            return alternativeRoute != null;
        }

        if (enableCongestionAvoidance && IsPathCongested(currentPosition, destination)){
            alternativeRoute = FindAlternativeRoute(currentPosition, destination);
            if (alternativeRoute != null){
                congestionDetections++;
                return true;
            }
        }

        return false;
    }

    // find an alternative route to the destination
    public Vector3? FindAlternativeRoute(Vector3 from, Vector3 to){
        // Create potential intermediate points in corridor spaces
        // These points are offset from the direct path to avoid obstacles
        Vector3 direction = (to - from).normalized;
        float totalDistance = Vector3.Distance(from, to);
        
        // Try points at different distances along the path with perpendicular offsets
        Vector3[] intermediatePoints = new Vector3[] {
            from + direction * (totalDistance / 3) + new Vector3(-2f, 0f, 0f),
            from + direction * (totalDistance / 3) + new Vector3(2f, 0f, 0f),
            from + direction * (totalDistance * 2/3) + new Vector3(-2f, 0f, 0f),
            from + direction * (totalDistance * 2/3) + new Vector3(2f, 0f, 0f),
            // Add diagonal offsets for more options
            from + direction * (totalDistance / 2) + new Vector3(-2f, -2f, 0f),
            from + direction * (totalDistance / 2) + new Vector3(2f, 2f, 0f)
        };
        
        foreach (Vector3 point in intermediatePoints) {
            if (IsPositionReachable(point) && !IsAreaBlocked(point)) {
                // Check if this route avoids the congested/blocked area
                if (!IsPathCongested(from, point) && !IsPathCongested(point, to)) {
                    return point;
                }
            }
        }

        // If no intermediate points work, try simple offsets from destination
        Vector3[] offsets = new Vector3[] {
            new Vector3(-2f, 0f, 0f),
            new Vector3(2f, 0f, 0f),
            new Vector3(0f, -2f, 0f),
            new Vector3(0f, 2f, 0f)
        };

        foreach (Vector3 offset in offsets) {
            Vector3 testPos = to + offset;
            if (IsPositionReachable(testPos) && !IsAreaBlocked(testPos)) {
                return testPos;
            }
        }

        return null;
    }

    /// find an alternative destination near the blocked one
    public Vector3? FindAlternativeDestination(Vector3 blockedDestination){
        Vector3[] searchOffsets = new Vector3[] {
            new Vector3(3f, 0f, 0f), new Vector3(-3f, 0f, 0f),
            new Vector3(0f, 3f, 0f), new Vector3(0f, -3f, 0f),
            new Vector3(3f, 3f, 0f), new Vector3(-3f, -3f, 0f),
            new Vector3(3f, -3f, 0f), new Vector3(-3f, 3f, 0f)
        };

        foreach (Vector3 offset in searchOffsets){
            Vector3 testPos = blockedDestination + offset;
            if (IsPositionReachable(testPos) && !IsAreaBlocked(testPos)){
                return testPos;
            }
        }

        return null;
    }

    // check if a path segment is congested
    private bool IsPathCongested(Vector3 from, Vector3 to){
        Vector3 direction = (to - from).normalized;
        float distance = Vector3.Distance(from, to);
        int steps = Mathf.CeilToInt(distance / congestionRadius);

        for (int i = 0; i <= steps; i++){
            Vector3 checkPoint = from + direction * (distance * i / steps);
            int nearbyAGVs = CountNearbyAGVs(checkPoint, congestionRadius);
            
            if (nearbyAGVs >= congestionThreshold){
                return true;
            }
        }

        return false;
    }

    // count AGVs near a position
    private int CountNearbyAGVs(Vector3 position, float radius){
        int count = 0;
        foreach (RoviTransporter agv in registeredAGVs){
            if (agv == null || !agv.gameObject.activeInHierarchy)
                continue;

            float distance = Vector3.Distance(position, agv.GetCurrentPosition());
            if (distance <= radius && agv.GetCurrentState() == Transporter.MovementState.Moving){
                count++;
            }
        }
        return count;
    }

    public void MarkAreaBlocked(Vector3 position, float duration = 10f){
        Vector3 gridPos = SnapToGrid(position);
        blockedAreas.Add(gridPos);
        blockedAreaTimestamps[gridPos] = Time.time + duration;
    }

    public void UnmarkAreaBlocked(Vector3 position){
        Vector3 gridPos = SnapToGrid(position);
        blockedAreas.Remove(gridPos);
        blockedAreaTimestamps.Remove(gridPos);
    }

    public bool IsAreaBlocked(Vector3 position){
        Vector3 gridPos = SnapToGrid(position);
        if (blockedAreas.Contains(gridPos)){
            // Check if block has expired
            if (blockedAreaTimestamps.ContainsKey(gridPos)){
                if (Time.time > blockedAreaTimestamps[gridPos]){
                    blockedAreas.Remove(gridPos);
                    blockedAreaTimestamps.Remove(gridPos);
                    return false;
                }
            }
            return true;
        }
        return false;
    }

    public bool IsPositionReachable(Vector3 position){
        NavMeshHit hit;
        return NavMesh.SamplePosition(position, out hit, 2f, NavMesh.AllAreas);
    }

    // get path cost multiplier for a position (higher = more expensive/avoid)
    public float GetPathCostMultiplier(Vector3 position){
        float cost = 1f;

        //  increase cost for blocked areas
        if (IsAreaBlocked(position)){
            cost += 10f;
        }

        //  increase cost based on congestion
        int nearbyAGVs = CountNearbyAGVs(position, congestionRadius);
        if (nearbyAGVs > 0){
            cost += nearbyAGVs * 0.5f;
        }

        return cost;
    }

    public bool ValidatePath(Vector3 currentPos, Vector3 destination){
        if (!IsPositionReachable(destination)){
            return false;
        }

        if (IsAreaBlocked(destination)){
            return false;
        }

        return true;
    }

    public void RecordReroute(){
        totalReroutes++;
    }

    public (int reroutes, int congestions) GetStatistics(){
        return (totalReroutes, congestionDetections);
    }

    private IEnumerator CongestionMonitoringCoroutine(){
        while (true){
            yield return new WaitForSeconds(congestionCheckInterval);
            UpdateCongestionData();
        }
    }

    private IEnumerator PathValidationCoroutine(){
        while (true){
            yield return new WaitForSeconds(pathValidationInterval);
            CleanupExpiredBlocks();
        }
    }

    private void UpdateCongestionData(){
        // Update congestion for path segments
        // This can be expanded to maintain a spatial grid of congestion
    }

    private void CleanupExpiredBlocks(){
        List<Vector3> expired = new List<Vector3>();
        foreach (var kvp in blockedAreaTimestamps){
            if (Time.time > kvp.Value){
                expired.Add(kvp.Key);
            }
        }

        foreach (Vector3 pos in expired){
            blockedAreas.Remove(pos);
            blockedAreaTimestamps.Remove(pos);
        }
    }

    private Vector3 SnapToGrid(Vector3 position, float gridSize = 1f){
        return new Vector3(
            Mathf.Round(position.x / gridSize) * gridSize,
            Mathf.Round(position.y / gridSize) * gridSize,
            0f
        );
    }

}

//  helper class for path segment tracking
public class PathSegment {
    public Vector3 start;
    public Vector3 end;
    public float congestionLevel;
    public float lastUpdateTime;
}


