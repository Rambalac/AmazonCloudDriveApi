// <copyright file="Retry.cs" company="Rambalac">
// Copyright (c) Rambalac. All rights reserved.
// </copyright>

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Azi.Tools
{
    /// <summary>
    /// Tool class to retry operations on error
    /// </summary>
    internal static class Retry
    {
        /// <summary>
        /// Does func and retries if it failed
        /// </summary>
        /// <param name="times">Maximum times to retry</param>
        /// <param name="act">Func with action and which returns false if retry required. Throw exception if action fail and can not be retried.</param>
        /// <returns>True if action was successful</returns>
        public static bool Do(int times, Func<bool> act)
        {
            while (times > 0)
            {
                if (act())
                {
                    return true;
                }

                times--;
            }

            return false;
        }

        /// <summary>
        /// Does func and retries if it failed
        /// </summary>
        /// <param name="times">Maximum times to retry</param>
        /// <param name="act">Func with action and which returns false if retry required. Throw exception if action fail and can not be retried.. Throw exception if action fail and can not be retried. Throw exception if action fail and can not be retried.</param>
        /// <returns>True if action was successful</returns>
        public static bool Do(int times, Func<int, bool> act)
        {
            for (var time = 0; time < times - 1; time++)
            {
                if (act(time))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Does async func and retries if it failed
        /// </summary>
        /// <param name="times">Maximum times to retry</param>
        /// <param name="retryDelay">Func that returns time between each retry. First parameter is number of tries before.</param>
        /// <param name="act">Async Func with action and which returns false if retry required. Throw exception if action fail and can not be retried.</param>
        /// <returns>True if action was successful</returns>
        public static async Task<bool> Do(int times, Func<int, TimeSpan> retryDelay, Func<Task<bool>> act)
        {
            return await Do(times, retryDelay, act, DefaultExceptionProcessorAsync);
        }

        /// <summary>
        /// Does async func and retries if it failed
        /// </summary>
        /// <param name="times">Maximum times to retry</param>
        /// <param name="retryDelay">Func that returns time between each retry. First parameter is number of tries before.</param>
        /// <param name="act">Async Func with action and which returns false if retry required. Throw exception if action fail and can not be retried.</param>
        /// <param name="exceptionPocessor">Async Func that checks exception and return true if action can not be retried</param>
        /// <returns>True if action was successful</returns>
        public static async Task<bool> Do(int times, Func<int, TimeSpan> retryDelay, Func<Task<bool>> act, Func<Exception, Task<bool>> exceptionPocessor)
        {
            for (var time = 0; time < times - 1; time++)
            {
                try
                {
                    if (await act().ConfigureAwait(false))
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    if (await exceptionPocessor(ex))
                    {
                        return false;
                    }
                }

                await Task.Delay(retryDelay(time));
            }

            return await act();
        }

        /// <summary>
        /// Does func and retries if it failed
        /// </summary>
        /// <param name="times">Maximum times to retry</param>
        /// <param name="retryDelay">Func that returns time between each retry. First parameter is number of tries before.</param>
        /// <param name="act">Func with action and which returns false if retry required. Throw exception if action fail and can not be retried.</param>
        /// <returns>True if action was successful</returns>
        public static bool Do(int times, Func<int, TimeSpan> retryDelay, Func<bool> act)
        {
            return Do(times, retryDelay, act, DefaultExceptionProcessor);
        }

        /// <summary>
        /// Does func and retries if it failed
        /// </summary>
        /// <param name="times">Maximum times to retry</param>
        /// <param name="retryDelay">Func that returns time between each retry. First parameter is number of tries before.</param>
        /// <param name="act">Func with action and which returns false if retry required. Throw exception if action fail and can not be retried.</param>
        /// <param name="exceptionPocessor">Func that checks exception and return true if action can not be retried</param>
        /// <returns>True if action was successful</returns>
        public static bool Do(int times, Func<int, TimeSpan> retryDelay, Func<bool> act, Func<Exception, bool> exceptionPocessor)
        {
            for (var time = 0; time < times - 1; time++)
            {
                try
                {
                    if (act())
                    {
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    if (exceptionPocessor(ex))
                    {
                        return false;
                    }
                }

                Thread.Sleep(retryDelay(time));
            }

            return act();
        }

        private static bool DefaultExceptionProcessor(Exception ex)
        {
            throw ex;
        }

        private static Task<bool> DefaultExceptionProcessorAsync(Exception ex)
        {
            throw ex;
        }
    }
}
