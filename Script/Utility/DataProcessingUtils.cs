using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GeneralGameDevKit.Utils
{
    public enum ComparisonType
    {
        Equal,
        NotEqual,
        Less,
        LessOrEqual,
        Greater,
        GreaterOrEqual
    }

    /// <summary>
    /// A library class with implemented frequently used methods.
    /// </summary>
    public static class DataProcessingUtils
    {
        /// <summary>
        /// Obtains the result of a probability evaluation.<br/>
        /// It is recommended to specify probability values in the range of 0 to 1. If the value is less than or equal to 0, it always returns false, and if the value is greater than or equal to 1, it always returns true.
        /// </summary>
        /// <param name="probability">probability value</param>
        /// <returns>true if passed or false.</returns>
        public static bool EvaluateProbability(float probability)
        {
            return Random.Range(0f, 1f) <= probability;
        }

        /// <summary>
        /// Obtains the result of a probability evaluation.
        /// This method is for evaluating weighted probabilities and takes an array of probabilities as a parameter.<br/>
        /// Each member of the array contains a probability weight, and the user is free to set the range.<br/>
        /// The evaluation result is provided as the index of the passed weight.
        /// </summary>
        /// <param name="probabilities">weighted probability array or params</param>
        /// <returns></returns>
        public static int EvaluateWeightedProbability(params float[] probabilities)
        {
            var len = probabilities.Length;
            var sum = probabilities.Sum();

            if (sum == 0.0f)
            {
                return Random.Range(0, len);
            }

            var randomEval = Random.Range(0f, 1f);
            var probabilitySum = 0f;

            for (var i = 0; i < len; i++)
            {
                var rebalanced = probabilities[i] / sum;
                probabilitySum += rebalanced;
                if (randomEval <= probabilitySum)
                {
                    return i;
                }
            }

            return 0;
        }
        
        /// <summary>
        /// Returns the value that transforms a given value 'v' from the range of 'fMin' to 'fMax' to the range of 'tMin' to 'tMax'.
        /// </summary>
        /// <param name="v">value v v</param>
        /// <param name="fMin">from range min</param>
        /// <param name="fMax">from range max</param>
        /// <param name="tMin">to range min</param>
        /// <param name="tMax">to range max</param>
        /// <returns></returns>
        public static float Remap(float v, float fMin, float fMax, float tMin, float tMax)
        {
            if (fMin - fMax == 0 || tMin - tMax == 0)
                return fMin;

            var nPos = (v - fMin) / (fMax - fMin);
            var remappedVal = tMin + (nPos * (tMax - tMin));
            return remappedVal;
        }

        /// <summary>
        /// Returns the result of testing the two values with the given comparison type.
        /// </summary>
        /// <param name="valueA">VALUE A</param>
        /// <param name="valueB">VALUE B</param>
        /// <param name="comparisonType">Comparison Type</param>
        /// <returns>If the test is passed return true or false.</returns>
        /// <exception cref="ArgumentOutOfRangeException">wrong comparison type value</exception>
        public static bool EvaluateComparision(int valueA, int valueB, ComparisonType comparisonType)
        {
            return comparisonType switch
            {
                ComparisonType.Equal => valueA == valueB,
                ComparisonType.NotEqual => valueA != valueB,
                ComparisonType.Less => valueA < valueB,
                ComparisonType.LessOrEqual => valueA <= valueB,
                ComparisonType.Greater => valueA > valueB,
                ComparisonType.GreaterOrEqual => valueA >= valueB,
                _ => throw new ArgumentOutOfRangeException(nameof(comparisonType), comparisonType, null)
            };
        }

        /// <summary>
        /// Returns the result of testing the two values with the given comparison type.
        /// </summary>
        /// <param name="valueA">VALUE A</param>
        /// <param name="valueB">VALUE B</param>
        /// <param name="comparisonType">Comparison Type</param>
        /// <param name="equalThreshold">Threshold value to determine if two float values are practically equal.</param>
        /// <returns>If the test is passed return true or false.</returns>
        /// <exception cref="ArgumentOutOfRangeException">wrong comparison type value</exception>
        public static bool EvaluateComparision(float valueA, float valueB, ComparisonType comparisonType, float equalThreshold = 0.0001f)
        {
            return comparisonType switch
            {
                ComparisonType.Equal => Math.Abs(valueA - valueB) <= equalThreshold,
                ComparisonType.NotEqual => Math.Abs(valueA - valueB) > equalThreshold,
                ComparisonType.Less => valueA < valueB,
                ComparisonType.LessOrEqual => valueA <= valueB,
                ComparisonType.Greater => valueA > valueB,
                ComparisonType.GreaterOrEqual => valueA >= valueB,
                _ => throw new ArgumentOutOfRangeException(nameof(comparisonType), comparisonType, null)
            };
        }

        /// <summary>
        /// Shuffles the given list randomly.
        /// </summary>
        /// <param name="listToShuffle">List to shuffle</param>
        /// <typeparam name="T">List Member Type</typeparam>
        public static void ShuffleList<T>(ref List<T> listToShuffle)
        {
            for (var i = 0; i < listToShuffle.Count; i++)
            {
                var rand = Random.Range(i, listToShuffle.Count);
                (listToShuffle[i], listToShuffle[rand]) = (listToShuffle[rand], listToShuffle[i]);
            }
        }

        /// <summary>
        /// Returns a random member from the given list.
        /// </summary>
        /// <param name="listToExtract">List source to extract member</param>
        /// <typeparam name="T">List Member Type</typeparam>
        /// <returns>Randomly extracted list member.</returns>
        public static T GetRandomMemberOfList<T>(List<T> listToExtract)
        {
            return listToExtract[Random.Range(0, listToExtract.Count)];
        }
    }
}
