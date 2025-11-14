// File: MyFarm/4_Presentation/Scripts/World/WorkerVisualManager.cs
using MyFarm.Domain.Models;
using System.Collections.Generic;
using UnityEngine;
using MyFarm.Presentation; // Thêm
using MyFarm.Presentation.World; // Thêm

namespace MyFarm.Presentation.World
{
    public class WorkerVisualManager : MonoBehaviour
    {
        [SerializeField] private WorkerVisual _workerPrefab; // Kéo Prefab công nhân vào
        [SerializeField] private Transform _spawnPoint; // Nơi công nhân được "đẻ" ra

        // Dùng Dictionary để map Data (Worker) với Visual (WorkerVisual)
        private Dictionary<string, WorkerVisual> _visuals = new Dictionary<string, WorkerVisual>();

        void Start()
        {
            StartCoroutine(InitializeWorkers());
        }

        System.Collections.IEnumerator InitializeWorkers()
        {
            yield return null; // Chờ GameManager
            if (GameManager.Instance == null) yield break;

            var farm = GameManager.Instance.DataRepository.LoadFarm();
            foreach (var workerModel in farm.Workers)
            {
                SpawnWorker(workerModel);
            }
            
            // Lắng nghe xem có thuê thêm công nhân mới không
            GameManager.Instance.EventNotifier.OnWorkerCountChanged += OnWorkerCountChanged;
        }

        private void OnWorkerCountChanged(int totalWorkers)
        {
            var farm = GameManager.Instance.DataRepository.LoadFarm();
            foreach (var workerModel in farm.Workers)
            {
                // Nếu chưa có Visual (3D) cho Model (data) này -> Tạo mới
                if (!_visuals.ContainsKey(workerModel.WorkerId))
                {
                    SpawnWorker(workerModel);
                }
            }
        }

        // --- HÀM ĐÃ SỬA ---
        private void SpawnWorker(Worker workerModel)
        {
            Vector3 spawnPos = _spawnPoint ? _spawnPoint.position : transform.position;
           
            // 1. Instantiate prefab (chứa cả 2 script)
            WorkerVisual newVisual = Instantiate(_workerPrefab, spawnPos, Quaternion.identity, transform);
            
            // 2. Lấy component "Animator Handler" từ prefab đó
            WorkerAnimatorHandler animHandler = newVisual.GetComponent<WorkerAnimatorHandler>();
            
            // 3. Khởi tạo cho script "Visual" (di chuyển)
            newVisual.Initialize(workerModel, spawnPos); 
            
            // 4. Khởi tạo cho script "Animator" (animation)
            if (animHandler != null)
            {
                animHandler.Initialize(workerModel);
            }
            else
            {
                Debug.LogWarning($"[WorkerVisualManager] Prefab Worker '{_workerPrefab.name}' bị thiếu script 'WorkerAnimatorHandler'!", this);
            }
            
            _visuals[workerModel.WorkerId] = newVisual;
        }
        // --- HẾT SỬA ---
        
        private void OnDestroy()
        {
            if (GameManager.Instance != null && GameManager.Instance.EventNotifier != null)
                GameManager.Instance.EventNotifier.OnWorkerCountChanged -= OnWorkerCountChanged;
        }
    }
}