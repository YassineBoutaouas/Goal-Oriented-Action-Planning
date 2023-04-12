using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System;
using Unity.Collections;

namespace Extensions
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Linear search that returns a bool and an index
        /// </summary>
        public static bool Contains<T>(this NativeList<T> collection, T item, out int index) where T : unmanaged, IEquatable<T>
        {
            index = -1;

            for (int i = 0; i < collection.Length; i++)
            {
                if (!collection[i].Equals(item)) continue;

                index = i;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Calculates the flat index of an item within a flattened array
        /// </summary>
        public static int CalculateFlatIndex(this int itemIndex, int listIndex, int itemCount) { return itemIndex + listIndex * itemCount; }

        #region Coroutines
        public static void RestartCoroutine(this MonoBehaviour behaviour, ref IEnumerator enumerator, IEnumerator newRoutine)
        {
            if (enumerator != null)
                behaviour.StopCoroutine(enumerator);

            enumerator = newRoutine;
            behaviour.StartCoroutine(enumerator);
        }

        public static void StopCoroutine(this MonoBehaviour behaviour, ref IEnumerator enumerator)
        {
            if (enumerator == null) return;

            behaviour.StopCoroutine(enumerator);
            enumerator = null;
        }
        #endregion

        #region NavmeshAgent methods
        public static void StopAgent(this NavMeshAgent agent)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        public static void ResumeAgent(this NavMeshAgent agent)
        {
            agent.isStopped = false;
            agent.ResetPath();
        }

        public static float GetPathDistance(this NavMeshPath path, Vector3 startingPos)
        {
            if (path.corners.Length == 0) return 0;

            float distance = Vector3.Distance(startingPos, path.corners[0]);

            for (int i = 1; i < path.corners.Length; ++i)
                distance += Vector3.Distance(path.corners[i - 1], path.corners[i]);

            return distance;
        }

        public static void SetAgentDestination(this NavMeshAgent agent, Vector3 destination)
        {
            if (!agent.enabled) return;
            agent.SetDestination(destination);
        }

        public static void SetAgentPath(this NavMeshAgent agent, NavMeshPath path, float stoppingDistance, float speed, float acceleration)
        {
            agent.SetAgentValues(stoppingDistance, speed, acceleration);
            agent.SetPath(path);
        }

        public static void SetAgentDestination(this NavMeshAgent agent, Vector3 destination, float stoppingDistance, float speed, float acceleration)
        {
            agent.SetAgentValues(stoppingDistance, speed, acceleration);
            agent.SetDestination(destination);
        }

        public static void SetAgentValues(this NavMeshAgent agent, float stoppingDistance, float speed, float acceleration)
        {
            agent.stoppingDistance = stoppingDistance;
            agent.speed = speed;
            agent.acceleration = acceleration;
        }
        #endregion
    }
}