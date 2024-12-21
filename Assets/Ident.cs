using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ReactiveInk
{
    public readonly struct Ident : IEquatable<Ident>
    {
        private readonly long _id;

        public Ident(long id)
        {
            _id = Interlocked.Increment(ref _lastId);
        }

        private static long _lastId;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Ident other)
        {
            return _id == other._id;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            return obj is Ident other && Equals(other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Ident left, Ident right)
        {
            return left.Equals(right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Ident left, Ident right)
        {
            return !left.Equals(right);
        }
    }
}