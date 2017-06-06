﻿#if UNITY_EDITOR
using System;
using System.Collections;
using UnityEditor.Experimental.EditorVR.Extensions;
using UnityEditor.Experimental.EditorVR.Modules;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UnityEditor.Experimental.EditorVR.Menus
{
	sealed class MainMenuActivator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler, IUsesMenuOrigins, IUsesRayOrigin, IPerformHaptics
	{
		readonly Vector3 m_OriginalActivatorLocalPosition = new Vector3(0f, 0f, -0.075f);
		static readonly float kAlternateLocationOffset = 0.06f;

		public Transform alternateMenuOrigin
		{
			get { return m_AlternateMenuOrigin; }
			set
			{
				m_AlternateMenuOrigin = value;
				transform.SetParent(m_AlternateMenuOrigin);
				transform.localPosition = m_OriginalActivatorLocalPosition;
				transform.localRotation = Quaternion.identity;
				transform.localScale = Vector3.one;

				m_OriginalActivatorIconLocalScale = m_Icon.localScale;
				m_OriginalActivatorIconLocalPosition = m_Icon.localPosition;
				m_HighlightedActivatorIconLocalScale = m_HighlightedPRS.localScale;
				m_HighlightedActivatorIconLocalPosition = m_HighlightedPRS.localPosition;
				m_AlternateActivatorLocalPosition = m_OriginalActivatorLocalPosition + Vector3.back * kAlternateLocationOffset;
			}
		}
		Transform m_AlternateMenuOrigin;

		public bool activatorButtonMoveAway
		{
			get { return m_ActivatorButtonMoveAway; }
			set
			{
				if (m_ActivatorButtonMoveAway == value)
					return;

				m_ActivatorButtonMoveAway = value;

				this.StopCoroutine(ref m_ActivatorMoveCoroutine);

				m_ActivatorMoveCoroutine = StartCoroutine(AnimateMoveActivatorButton(m_ActivatorButtonMoveAway));
			}
		}
		bool m_ActivatorButtonMoveAway;

		[SerializeField]
		Transform m_Icon;
		[SerializeField]
		Transform m_HighlightedPRS;

		Vector3 m_OriginalActivatorIconLocalScale;
		Vector3 m_OriginalActivatorIconLocalPosition;
		Vector3 m_HighlightedActivatorIconLocalScale;
		Vector3 m_HighlightedActivatorIconLocalPosition;
		Coroutine m_HighlightCoroutine;
		Coroutine m_ActivatorMoveCoroutine;
		Vector3 m_AlternateActivatorLocalPosition;

		public Transform rayOrigin { private get; set; }
		public Transform menuOrigin { private get; set; }

		public event Action<Transform> hoverStarted;
		public event Action<Transform> hoverEnded;
		public event Action<Transform, Transform> selected;

		public void OnPointerEnter(PointerEventData eventData)
		{
			// A child may have used the event, but still reflect that is was hovered
			var rayEventData = eventData as RayEventData;
			if (rayEventData != null && hoverStarted != null)
				hoverStarted(rayEventData.rayOrigin);

			if (eventData.used)
				return;

			if (m_HighlightCoroutine != null)
				StopCoroutine(m_HighlightCoroutine);

			m_HighlightCoroutine = null;
			m_HighlightCoroutine = StartCoroutine(Highlight());
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			// A child may have used the event, but still reflect that is was hovered
			var rayEventData = eventData as RayEventData;
			if (rayEventData != null && hoverEnded != null)
				hoverEnded(rayEventData.rayOrigin);

			if (eventData.used)
				return;

			if (m_HighlightCoroutine != null)
				StopCoroutine(m_HighlightCoroutine);

			m_HighlightCoroutine = null;
			m_HighlightCoroutine = StartCoroutine(Highlight(false));
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			var rayEventData = eventData as RayEventData;
			if (selected != null)
			{
				this.Pulse(rayOrigin, 0.5f, 0.06f, true, true);
				selected(rayOrigin, rayEventData != null ? rayEventData.rayOrigin : null);
			}
		}

		IEnumerator Highlight(bool transitionIn = true)
		{
			this.Pulse(rayOrigin, 0.005f, 0.125f);
			var amount = 0f;
			var currentScale = m_Icon.localScale;
			var currentPosition = m_Icon.localPosition;
			var targetScale = transitionIn == true ? m_HighlightedActivatorIconLocalScale : m_OriginalActivatorIconLocalScale;
			var targetLocalPosition = transitionIn == true ? m_HighlightedActivatorIconLocalPosition : m_OriginalActivatorIconLocalPosition;
			var speed = (currentScale.x + 0.5f / targetScale.x) * 4; // perform faster is returning to original position

			while (amount < 1f)
			{
				amount += Time.deltaTime * speed;
				m_Icon.localScale = Vector3.Lerp(currentScale, targetScale,  Mathf.SmoothStep(0f, 1f, amount));
				m_Icon.localPosition = Vector3.Lerp(currentPosition, targetLocalPosition,  Mathf.SmoothStep(0f, 1f, amount));
				yield return null;
			}

			m_Icon.localScale = targetScale;
			m_Icon.localPosition = targetLocalPosition;
		}

		IEnumerator AnimateMoveActivatorButton(bool moveAway = true)
		{
			var amount = 0f;
			var currentPosition = transform.localPosition;
			var targetPosition = moveAway ? m_AlternateActivatorLocalPosition : m_OriginalActivatorLocalPosition;
			var speed = (currentPosition.z / targetPosition.z) * (moveAway ? 10 : 3); // perform faster is returning to original position

			while (amount < 1f)
			{
				amount += Time.deltaTime * speed;
				transform.localPosition = Vector3.Lerp(currentPosition, targetPosition, Mathf.SmoothStep(0f, 1f, amount));
				yield return null;
			}

			transform.localPosition = targetPosition;
			m_ActivatorMoveCoroutine = null;
		}
	}
}
#endif
