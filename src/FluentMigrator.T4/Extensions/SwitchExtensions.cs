using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System
{


    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class Switch<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Switch{T}" /> class.
        /// </summary>
        /// <param name="o">The o.</param>
        public Switch(T o)
        {
            Object = o;
        }

        /// <summary>
        /// Gets the object.
        /// </summary>
        /// <value>
        /// The object.
        /// </value>
        public T Object { get; private set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="R"></typeparam>
    class Switch<T, R> : Switch<T>
    {
        public Switch(T o) :
            base(o)
        {
        }
        /// <summary>
        /// Gets a value indicating whether this instance has value.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance has value; otherwise, <c>false</c>.
        /// </value>
        public bool HasValue { get; private set; }
        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public R Value { get; private set; }

        /// <summary>
        /// Sets the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        public void Set(R value)
        {
            Value = value;
            HasValue = true;
        }

        //public static operator implicit R (Switch<T, R> @switch)
        //{
        //    return @switch.Value;
        //}

        internal object Case(string p, Func<string, Data.Rule> func)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    static class SwitchExtensions
    {
        /// <summary>
        /// Cases the specified switch.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="switch">The switch.</param>
        /// <param name="case">The case.</param>
        /// <param name="do">The do.</param>
        /// <returns></returns>
        public static Switch<T> Case<T>(this Switch<T> @switch, T @case, Action<T> @do)
        {
            return Case(@switch, @case, @do, false);
        }

        /// <summary>
        /// Cases the specified switch.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="switch">The switch.</param>
        /// <param name="case">The case.</param>
        /// <param name="do">The do.</param>
        /// <param name="fallThrough">if set to <c>true</c> [fall through].</param>
        /// <returns></returns>
        public static Switch<T> Case<T>(this Switch<T> @switch, T @case, Action<T> @do, bool fallThrough)
        {
            return Case(@switch, x => EqualityComparer<T>.Default.Equals(x, @case), @do, fallThrough);
        }

        /// <summary>
        /// Cases the specified switch.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="switch">The switch.</param>
        /// <param name="case">The case.</param>
        /// <param name="do">The do.</param>
        /// <returns></returns>
        public static Switch<T> Case<T>(this Switch<T> @switch, Func<T, bool> @case, Action<T> @do)
        {
            return Case(@switch, @case, @do, false);
        }

        /// <summary>
        /// Cases the specified switch.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="switch">The switch.</param>
        /// <param name="case">The case.</param>
        /// <param name="do">The do.</param>
        /// <param name="fallThrough">if set to <c>true</c> [fall through].</param>
        /// <returns></returns>
        public static Switch<T> Case<T>(this Switch<T> @switch, Func<T, bool> @case, Action<T> @do, bool fallThrough)
        {
            if (@switch == null)
            {
                return null;
            }
            else if (@case(@switch.Object))
            {
                @do(@switch.Object);
                return fallThrough ? @switch : null;
            }

            return @switch;
        }

        /// <summary>
        /// Defaults the specified switch.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="switch">The switch.</param>
        /// <param name="do">The do.</param>
        public static void Default<T>(this Switch<T> @switch, Action<T> @do)
        {
            if (@switch != null)
            {
                @do(@switch.Object);
            }
        }

        /// <summary>
        /// Cases the specified switch.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="switch">The switch.</param>
        /// <param name="case">The case.</param>
        /// <param name="do">The do.</param>
        /// <returns></returns>
        public static Switch<T, R> Case<T, R>(this Switch<T, R> @switch, T @case, R result)
        {
            return Case<T, R>(@switch, x => EqualityComparer<T>.Default.Equals(x, @case), r => result);
        }


        /// <summary>
        /// Cases the specified switch.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="switch">The switch.</param>
        /// <param name="case">The case.</param>
        /// <param name="do">The do.</param>
        /// <returns></returns>
        public static Switch<T, R> Case<T, R>(this Switch<T, R> @switch, T @case, Func<T, R> @do)
        {
            return Case<T, R>(@switch, x => EqualityComparer<T>.Default.Equals(x, @case), @do);
        }

        /// <summary>
        /// Cases the specified switch.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="switch">The switch.</param>
        /// <param name="case">The case.</param>
        /// <param name="do">The do.</param>
        /// <returns></returns>
        public static Switch<T, R> Case<T, R>(this Switch<T, R> @switch, Func<T, bool> @case, Func<T, R> @do)
        {
            if (!@switch.HasValue && @case(@switch.Object))
            {
                @switch.Set(@do(@switch.Object));
            }

            return @switch;
        }

        /// <summary>
        /// Defaults the specified switch.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="switch">The switch.</param>
        /// <param name="do">The do.</param>
        /// <returns></returns>
        public static R Default<T, R>(this Switch<T, R> @switch, Func<T, R> @do)
        {
            if (!@switch.HasValue)
            {
                @switch.Set(@do(@switch.Object));
            }

            return @switch.Value;
        }


        /// <summary>
        /// Defaults the specified switch.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="switch">The switch.</param>
        /// <param name="do">The do.</param>
        /// <returns></returns>
        public static R Default<T, R>(this Switch<T, R> @switch, Func<R> @do)
        {
            if (!@switch.HasValue)            
                @switch.Set(@do());            
            return @switch.Value;
        }

        /// <summary>
        /// Defaults the specified switch.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="R"></typeparam>
        /// <param name="switch">The switch.</param>
        /// <param name="result">The result.</param>
        /// <returns></returns>
        public static R Default<T, R>(this Switch<T, R> @switch, R result)
        {
            if (!@switch.HasValue)
                @switch.Set(result);
            return @switch.Value;
        }
    }
}
