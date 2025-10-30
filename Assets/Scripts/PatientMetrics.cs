using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SimulationEvents
{
    [System.Serializable]
    public class PatientMetrics : MonoBehaviour
    {
        [Header("Patient Info")]
        public string patientId;
        public string patientName;
        public bool isReceivingCare = false;

        [Header("Metrics")]
        public float totalWaitTime = 0f;
        public float lastCareEndTime = 0f;

        [Header("Associated Tasks")]
        public List<Task> assignedTasks = new List<Task>();

        private void Start()
        {
            // Initialize timing when patient enters system
            lastCareEndTime = Time.time;
        }

        private void Update()
        {
            // Track wait time when not receiving care
            if (!isReceivingCare)
            {
                totalWaitTime += Time.deltaTime;
            }
        }

        public void AssignTask(Task newTask)
        {
            assignedTasks.Add(newTask);
            Debug.Log($"[Patient {patientId}] Task added: {newTask.taskId}");
        }

        public void StartCare()
        {
            isReceivingCare = true;
            Debug.Log($"[Patient {patientId}] Care started.");
        }

        public void EndCare()
        {
            isReceivingCare = false;
            lastCareEndTime = Time.time;
            Debug.Log($"[Patient {patientId}] Care ended at {lastCareEndTime}");
        }

        public void PrintSummary()
        {
            Debug.Log($"=== Patient Metrics ===\n" +
                      $"ID: {patientId}\n" +
                      $"Tasks: {assignedTasks.Count}\n" +
                      $"Total Wait Time: {totalWaitTime:F2} sec\n" +
                      $"Currently in Care: {isReceivingCare}");
        }
    }
}
