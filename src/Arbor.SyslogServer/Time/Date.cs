﻿using System;
using Newtonsoft.Json;

namespace Arbor.SyslogServer.Time
{
    public struct Date : IEquatable<Date>, IComparable<Date>
    {
        [JsonIgnore]
        public DateTime OriginalValue { get; }

        private readonly DateTime _datePart;

        public Date(DateTime date)
        {
            OriginalValue = date;
            _datePart = date.Date;
        }

        public override string ToString()
        {
            return _datePart.ToString("yyyy-MM-dd");
        }

        public static implicit operator Date(DateTime dateTime)
        {
            return new Date(dateTime);
        }

        public bool Equals(Date other)
        {
            return _datePart.Equals(other._datePart);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is Date && Equals((Date)obj);
        }

        public override int GetHashCode() => _datePart.GetHashCode();

        public static bool operator ==(Date left, Date right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Date left, Date right)
        {
            return !left.Equals(right);
        }

        public int CompareTo(Date other)
        {
            return _datePart.CompareTo(other._datePart);
        }

        public static bool operator <(Date date1, Date date2)
        {
            return date1._datePart < date2._datePart;
        }

        public static bool operator >(Date date1, Date date2)
        {
            return date1._datePart > date2._datePart;
        }

        public static bool operator >=(Date date1, Date date2)
        {
            return date1._datePart >= date2._datePart;
        }

        public static bool operator <=(Date date1, Date date2)
        {
            return date1._datePart <= date2._datePart;
        }
    }
}
