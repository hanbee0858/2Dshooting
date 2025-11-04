using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(NewMonoBehaviourScript))]
public class PlayerReplayController : MonoBehaviour
{
    [Header("기록 설정")]
    [Tooltip("최근 몇 초를 리플레이할지")]
    public float maxRecordSeconds = 15f;

    [Tooltip("기록 샘플링 간격(초). 너무 작으면 리스트가 커짐")]
    public float sampleInterval = 0.02f; // 50fps 정도

    [Header("재생 설정")]
    [Tooltip("재생 배속 (1 = 실시간)")]
    public float playbackSpeed = 1f;

    // 내부 상태
    private NewMonoBehaviourScript _move;  // 플레이어 이동 스크립트
    private struct Frame { public float t; public Vector3 pos; }
    private readonly List<Frame> _frames = new();
    private float _recordStartTime;
    private float _lastSampleTime;
    private bool _isReplaying = false;
    private float _replayTime = 0f;
    private int _replayIndex = 0;

    void Awake()
    {
        _move = GetComponent<NewMonoBehaviourScript>();
    }

    void Start()
    {
        _recordStartTime = Time.unscaledTime;
        _lastSampleTime = Time.unscaledTime;
        // 초기 한 샘플
        AddSample();
    }

    void Update()
    {
        if (_isReplaying)
        {
            UpdateReplay();
        }
        else
        {
            UpdateRecord();
        }
    }

    // ----- 기록 -----
    private void UpdateRecord()
    {
        // 일정 간격으로만 샘플링
        if (Time.unscaledTime - _lastSampleTime >= sampleInterval)
        {
            AddSample();
            _lastSampleTime = Time.unscaledTime;
            PruneOldFrames(); // maxRecordSeconds 넘어가면 오래된 프레임 삭제
        }
    }

    private void AddSample()
    {
        float t = Time.unscaledTime - _recordStartTime; // 기록 시작 후 경과 시간(초)
        _frames.Add(new Frame { t = t, pos = transform.position });
    }

    private void PruneOldFrames()
    {
        if (_frames.Count < 2) return;
        // 맨 마지막 프레임 시각 기준으로 maxRecordSeconds만 유지
        float latestT = _frames[_frames.Count - 1].t;
        float minKeep = latestT - maxRecordSeconds;

        // 앞에서부터 오래된 것 제거
        while (_frames.Count > 2 && _frames[1].t < minKeep)
            _frames.RemoveAt(0);
    }

    // ----- 재생 -----
    private void UpdateReplay()
    {
        if (_frames.Count < 2)
        {
            StopReplay();
            return;
        }

        _replayTime += Time.unscaledDeltaTime * playbackSpeed;

        // 현재 재생 시간에 맞는 구간 찾기
        while (_replayIndex < _frames.Count - 2 && _frames[_replayIndex + 1].t < _replayTime)
            _replayIndex++;

        // 마지막 프레임을 지나면 종료
        if (_replayTime >= _frames[_frames.Count - 1].t)
        {
            // 재생 끝 → 마지막 위치에 맞춰두고 종료
            transform.position = _frames[_frames.Count - 1].pos;
            StopReplay();
            return;
        }

        // 보간으로 부드럽게 이동
        var a = _frames[_replayIndex];
        var b = _frames[_replayIndex + 1];

        float segment = Mathf.Max(0.0001f, b.t - a.t);
        float t = Mathf.Clamp01((_replayTime - a.t) / segment);
        Vector3 pos = Vector3.Lerp(a.pos, b.pos, t);

        transform.position = pos;
    }

    // ----- 외부(UI 버튼)에서 호출할 공개 메서드 -----
    public void StartReplay()
    {
        if (_frames.Count < 2) return;

        _isReplaying = true;
        _replayTime = _frames[0].t;   // 가장 오래된 프레임부터
        _replayIndex = 0;

        // 재생 중에는 플레이어 조작 비활성화
        if (_move) _move.enabled = false;
    }

    public void StopReplay()
    {
        _isReplaying = false;
        _replayTime = 0f;
        _replayIndex = 0;

        // 조작 다시 활성화
        if (_move) _move.enabled = true;
    }

    public void ClearRecording()
    {
        _frames.Clear();
        _recordStartTime = Time.unscaledTime;
        _lastSampleTime = Time.unscaledTime;
        AddSample();
    }

    public bool IsReplaying => _isReplaying;
}