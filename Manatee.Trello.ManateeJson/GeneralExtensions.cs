﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Manatee.Json;
using Manatee.Json.Serialization;
using Manatee.Trello.Json;

namespace Manatee.Trello.ManateeJson
{
	internal static class GeneralExtensions
	{
		public static string ToLowerString<T>(this T item)
		{
			return item.ToString().ToLower();
		}
		public static bool IsNullOrWhiteSpace(this string value)
		{
#if NET35 || NET35C
			return string.IsNullOrEmpty(value) || string.IsNullOrEmpty(value.Trim());
#elif NET4 || NET4C || NET45
			return string.IsNullOrWhiteSpace(value);
#endif
		}
		public static string Join(this IEnumerable<string> segments, string separator)
		{
#if NET35 || NET35C
			return string.Join(separator, segments.ToArray());
#elif NET4 || NET4C || NET45
			return string.Join(separator, segments);
#endif
		}
		public static T Deserialize<T>(this JsonObject obj, JsonSerializer serializer, string key)
		{
			if (!obj.ContainsKey(key)) return default(T);
#if IOS
			if (typeof (Enum).IsAssignableFrom(typeof (T)))
				return obj.TryGetString(key).ToEnum<T>();
#endif
			return serializer.Deserialize<T>(obj[key]);
		}
		public static void Serialize<T>(this T obj, JsonObject json, JsonSerializer serializer, string key, bool force = false)
		{
			var isDefault = Equals(obj, default(T));
			if (force || !isDefault)
			{
#if IOS
				var enumValue = obj as Enum;
				if (enumValue != null)
					json[key] = enumValue.ToDescription();
				else
#endif
				json[key] = isDefault ? string.Empty : serializer.Serialize(obj);
			}
		}
		public static void SerializeId<T>(this T obj, JsonObject json, string key)
			where T : IJsonCacheable
		{
			if (!Equals(obj, default(T)))
				json[key] = obj.Id;
		}
#if IOS
		// source for these two methods: http://www.kevinwilliampang.com/2008/09/20/mapping-enums-to-strings-and-strings-to-enums-in-net/
		public static string ToDescription(this Enum value)
		{
			try
			{
				var type = value.GetType();
				var field = type.GetField(value.ToString());
				var da = (DescriptionAttribute[]) field.GetCustomAttributes(typeof (DescriptionAttribute), false);
				return da.Length > 0 ? da[0].Description : value.ToString();
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				throw;
			}
		}
		public static T ToEnum<T>(this string stringValue, T defaultValue = default (T))
		{
			foreach (T enumValue in Enum.GetValues(typeof (T)))
			{
				var type = typeof (T);
				var field = type.GetField(enumValue.ToString());
				var da = (DescriptionAttribute[]) field.GetCustomAttributes(typeof (DescriptionAttribute), false);
				if (da.Length > 0 && da[0].Description == stringValue) return enumValue;
			}
			return defaultValue;
		}
#else
		public static T Combine<T>(this IEnumerable<T> values)
			where T : struct
		{
			return (T) Enum.ToObject(typeof (T), values.Select(value => Convert.ToInt32(value))
													   .Aggregate(0, (current, longValue) => current + longValue));
		}
#endif
	}
}
