using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

/*
 还需要把顶点的权重计算一下，用于直接影响一些顶点
 
 */
[RequireComponent(typeof(LineRenderer))]
public class BoneSolver : MonoBehaviour
{
    [Header("绳子设置")]
    public GameObject startObject;  // 起点物体（B）
    public GameObject endObject;    // 末端物体（A）
    public int segmentCount = 20;  // 绳子段数
    public float ropeWidth = 0.1f; // 绳子的宽度

    [Header("物理参数")]
    public Vector3 gravity = new Vector3(0, -9.81f, 0); // 重力
    public float damping = 0.98f;                       // 阻尼
    [Range(0, 1)] public float bendingStiffness = 0.5f; // 绳子的硬度
    public int solverIterations = 15;                  // 求解迭代次数

    [Header("拉力设置")]
    public float pullThreshold = 5f;    // 开始施加拉力的距离
    public float pullForce = 50f;       // 拉力大小
    public float stopPullDistance = 1f; // 停止拉力的距离

    private List<RopeSegment> segments = new List<RopeSegment>();
    private LineRenderer lineRenderer;

    private class RopeSegment
    {
        public Vector3 currentPos;   // 当前位置
        public Vector3 previousPos; // 前一帧的位置
        public Vector3 externalForce; // 新增外力属性

        public RopeSegment(Vector3 pos)
        {
            currentPos = pos;
            previousPos = pos;
            externalForce = Vector3.zero;
        }
    }

    void Start()
    {
        InitializeRope();
    }

    void FixedUpdate()
    {
        ApplyGravity();
        ApplyConstraints();
        HandleEndPoints();
        HandlePullForce();
        UpdateRenderer();
    }

    private void InitializeRope()
    {
        segments.Clear();

        Vector3 startPos = startObject.transform.position;
        Vector3 endPos = endObject.transform.position;

        for (int i = 0; i < segmentCount; i++)
        {
            float t = i / (float)(segmentCount - 1);
            Vector3 initialPos = Vector3.Lerp(startPos, endPos, t);
            segments.Add(new RopeSegment(initialPos));
        }

        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = segmentCount;
            lineRenderer.startWidth = ropeWidth;
            lineRenderer.endWidth = ropeWidth;
        }
    }

    public void ApplyForceToSegment(int segmentIndex, Vector3 force)
    {
        if (segmentIndex >= 0 && segmentIndex < segments.Count)
        {
            segments[segmentIndex].externalForce += force;
        }
    }
    private void ApplyGravity()
    {
        for (int i = 1; i < segments.Count - 1; i++)
        {
            RopeSegment segment = segments[i];
            Vector3 velocity = (segment.currentPos - segment.previousPos) * damping;
            segment.previousPos = segment.currentPos;

            // 将外力和重力一起应用到运动中
            segment.currentPos += velocity + gravity * Time.fixedDeltaTime * Time.fixedDeltaTime + segment.externalForce * Time.fixedDeltaTime;

            // 每帧后重置外力，确保动态性
            segment.externalForce = Vector3.zero;
        }
    }

    private void ApplyConstraints()
    {
        for (int iteration = 0; iteration < solverIterations; iteration++)
        {
            for (int i = 0; i < segments.Count - 1; i++)
            {
                RopeSegment segmentA = segments[i];
                RopeSegment segmentB = segments[i + 1];

                Vector3 delta = segmentB.currentPos - segmentA.currentPos;
                float currentDistance = delta.magnitude;
                float targetDistance = Vector3.Distance(startObject.transform.position, endObject.transform.position) / segmentCount;

                // 应用硬度约束
                Vector3 correction = delta.normalized * (currentDistance - targetDistance) * bendingStiffness;

                if (i != 0)
                    segmentA.currentPos += correction * 0.5f;

                if (i != segments.Count - 2)
                    segmentB.currentPos -= correction * 0.5f;
            }
        }
    }

    private void HandleEndPoints()
    {
        // 保持端点位置
        segments[0].currentPos = startObject.transform.position;
        segments[segments.Count - 1].currentPos = endObject.transform.position;
    }

    private void HandlePullForce()
    {
        float distance = Vector3.Distance(startObject.transform.position, endObject.transform.position);

        if (distance > pullThreshold)
        {
            Vector3 direction = (startObject.transform.position - endObject.transform.position).normalized;
            endObject.GetComponent<Rigidbody>().AddForce(direction * pullForce);
        }
        else if (distance < stopPullDistance)
        {
            // 保留缓慢调整运动，而不是直接停止
            Vector3 direction = (startObject.transform.position - endObject.transform.position).normalized;
            endObject.GetComponent<Rigidbody>().velocity = -direction * 0.1f; // 保留微弱的速度
        }
    }

    private void UpdateRenderer()
    {
        if (lineRenderer != null)
        {
            for (int i = 0; i < segments.Count; i++)
            {
                lineRenderer.SetPosition(i, segments[i].currentPos);
            }
        }
    }
}