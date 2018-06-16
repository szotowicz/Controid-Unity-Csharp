using System.Collections.Generic;
using UnityEngine;

internal class LogoCorner
{
    public Vector3 position;
    public Vector3 velocity;
}

public class LogoScript : MonoBehaviour
{
    private Vector3 center;
    public float limiter;
    public LineRenderer line;
    public LineRenderer line2;
    private readonly List<LogoCorner> points = new List<LogoCorner>();
    public float radius;
    public float strength;

    // Use this for initialization
    private void Start() {
        center = Vector3.zero;
        for (var i = 0; i < 6; ++i) addRandom();
        //points.Add(new LogoCorner() { position = new Vector3(2,0,0), velocity = Vector3.zero });
        //points.Add(new LogoCorner() { position = new Vector3(-2, 0, 0), velocity = Vector3.zero });
    }

    private LogoCorner randomPoint() {
        var result = Vector3.zero;
        result.x = Random.Range(-1.0f, 1.0f);
        result.y = Random.Range(-1.0f, 1.0f);
        var lc = new LogoCorner();
        lc.position = setPointOnCircle(result);
        lc.velocity = Vector3.zero;
        return lc;
    }

    private Vector3 setPointOnCircle(Vector3 v) {
        var result = v;
        result.Normalize();
        result *= radius;
        result += center;
        return result;
    }

    private void setLineOnPoints() {
        points.Sort(
            delegate(LogoCorner p1, LogoCorner p2)
            {
                var angle1 = Mathf.Atan2(p1.position.y, p1.position.x);
                var angle2 = Mathf.Atan2(p2.position.y, p2.position.x);

                var result = 0;
                if (angle2 > angle1)
                    result = 1;
                else if (angle2 < angle1)
                    result = -1;
                return result;
            }
        );
        line.positionCount = points.Count;
        line2.positionCount = points.Count;
        for (var i = 0; i < points.Count; ++i) {
            line.SetPosition(i, points[i].position);
            line2.SetPosition(i, points[i].position);
        }
    }

    private void nextPosition() {
        foreach (var lc1 in points) {
            var force = Vector3.zero;
            foreach (var lc2 in points) {
                if (lc1 == lc2) continue;
                var r = (lc2.position - lc1.position).magnitude;
                var v = (lc2.position - lc1.position).normalized;
                v = v * (strength / (r * r));
                force += v;
            }

            lc1.velocity += force * Time.deltaTime;
        }

        foreach (var lc1 in points) {
            var dot = Vector3.Dot(lc1.position.normalized, lc1.velocity);
            lc1.velocity -= lc1.position.normalized * dot;

            lc1.velocity *= 0.95f;
            lc1.position += lc1.velocity * Time.deltaTime;
            lc1.position = setPointOnCircle(lc1.position);
            if (lc1.velocity.magnitude > limiter) lc1.velocity = lc1.velocity.normalized * limiter;
        }
    }

    private void addRandom() {
        points.Add(randomPoint());
    }

    private void removeRandom() {
        var r = Random.Range(0, points.Count);
        if (points.Count > 1) points.RemoveAt(r);
    }

    // Update is called once per frame
    private void Update() {
        nextPosition();
        setLineOnPoints();
        /*if (Input.GetKeyDown(KeyCode.A)) {
            addRandom();
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            removeRandom();
        }*/
    }

    public void setCornerNumber(int n) {
        if (n < 2 || n > 10) return;
        for (var i = 0; i < 11; ++i) {
            if (points.Count == n) return;
            if (points.Count > n)
                removeRandom();
            if (points.Count < n)
                addRandom();
        }
    }
}