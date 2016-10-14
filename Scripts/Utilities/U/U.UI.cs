namespace UnityEngine.VR.Utilities
{
	using System;
	using UnityEngine;
#if UNITY_EDITOR
	using UnityEditor;
	using System.Reflection;
	using Modules;
#endif

	/// <summary>
	/// EditorVR Utilities
	/// </summary>
	public static partial class U
	{
		/// <summary>
		/// Object related EditorVR utilities
		/// </summary>
		public static class UI
		{
			public const float kDoubleClickIntervalMax = 0.3f;
			private const float kDoubleClickIntervalMin = 0.15f;

			public static bool IsDoubleClick(float timeSinceLastClick)
			{
				return timeSinceLastClick <= kDoubleClickIntervalMax && timeSinceLastClick >= kDoubleClickIntervalMin;
			}

			public static bool IsDirectEvent(RayEventData eventData)
			{
				return eventData.pointerCurrentRaycast.isValid && eventData.pointerCurrentRaycast.distance <= eventData.pointerLength;
			}

			public static bool IsValidEvent(RayEventData eventData, SelectionFlags selectionFlags)
			{
				if ((selectionFlags & SelectionFlags.Direct) != 0 && IsDirectEvent(eventData))
					return true;

				if ((selectionFlags & SelectionFlags.Ray) != 0)
					return true;

				return false;
			}

			/// <summary>
			/// Special version of EditorGUI.MaskField which ensures that only the chosen bits are set. We need this version of the
			/// function to check explicitly whether only a single bit was set.
			/// </summary>
			/// <returns></returns>
			public static int MaskField(Rect position, GUIContent label, int mask, string[] displayedOptions, Type propertyType)
			{
				mask = EditorGUI.MaskField(position, label, mask, displayedOptions);
				return ActualEnumFlags(mask, propertyType);
			}

			public static int ActualEnumFlags(int value, Type t)
			{
				if (value < 0)
				{
					int bits = 0;
					foreach (var enumValue in System.Enum.GetValues(t))
					{
						int checkBit = value & (int)enumValue;
						if (checkBit != 0)
						{
							bits |= (int)enumValue;
						}
					}
					value = bits;
				}
				return value;
			}

			public static Type SerializedPropertyToType(SerializedProperty property)
			{
				var parts = property.propertyPath.Split('.');

				var currentType = property.serializedObject.targetObject.GetType();

				if (parts.Length == 0)
					return null;

				var field = GetFieldInTypeOrParent(currentType, parts[parts.Length - 1]);

				return field != null ? field.FieldType : null;
			}

			public static FieldInfo GetFieldInTypeOrParent(Type type, string fieldName)
			{
				while (true)
				{
					if (type == null)
						return null;
					var field = type.GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance);
					if (field != null) return field;
					type = type.BaseType;
				}
			}
		}
	}
}
