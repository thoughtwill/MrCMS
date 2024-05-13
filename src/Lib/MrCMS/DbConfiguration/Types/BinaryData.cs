using System;
using System.Data.Common;
using System.Text;
using System.Text.Json;
using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;

namespace MrCMS.DbConfiguration.Types
{
    [Serializable]
    public class BinaryData<T> : BaseImmutableUserType<T> where T : class, new()
    {
        public override SqlType[] SqlTypes
        {
            get { return new[] { NHibernateUtil.BinaryBlob.SqlType }; }
        }

        public override object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
        {
            var o = NHibernateUtil.BinaryBlob.NullSafeGet(rs, names[0], session, owner) as byte[];
            return BinaryData.Deserialize<T>(o);
        }

        public override void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
        {
            cmd.Parameters[index].Size = int.MaxValue;
            NHibernateUtil.BinaryBlob.NullSafeSet(cmd, BinaryData.Serialize(value), index, session);
        }

    }

    public static class BinaryData
    {
        // This method isn't directly applicable since all types that can be serialized to JSON should be supported.
        public static bool CanSerialize(object value)
        {
            // With System.Text.Json, it's generally safe to assume most types can be serialized;
            // however, you might want to implement specific checks for unserializable types if necessary.
            return value != null; // Simplistic check, may need to be more complex depending on requirements
        }

        public static byte[] Serialize(object value)
        {
            if (value == null) return null;
            // Serialize the object to a JSON string, then get the bytes of the string.
            string jsonString = JsonSerializer.Serialize(value);
            return Encoding.UTF8.GetBytes(jsonString);
        }

        public static T Deserialize<T>(byte[] bytes) where T : class, new()
        {
            if (bytes == null || bytes.Length == 0) return new T();
            try
            {
                // Deserialize the JSON bytes to the specified type.
                string jsonString = Encoding.UTF8.GetString(bytes);
                T result = JsonSerializer.Deserialize<T>(jsonString);
                return result ?? new T();
            }
            catch
            {
                // Return a new instance of T if deserialization fails.
                return new T();
            }
        }
    }
}