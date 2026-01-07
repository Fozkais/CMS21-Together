using System;
using MelonLoader;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace CMS21Together.ClientSide.Data.Player;

[RegisterTypeInIl2Cpp]
public class InfoBillboard : MonoBehaviour
{
	private Transform cam;
	private Transform textTransform;

	public InfoBillboard(IntPtr ptr) : base(ptr)
	{
	}

	public InfoBillboard() : base(ClassInjector.DerivedConstructorPointer<InfoBillboard>())
	{
		ClassInjector.DerivedConstructorBody(this);
	}

	private void Start()
	{
		cam = Camera.main.transform;

		var textObj = new GameObject("PlayerNameTag");
		textObj.transform.SetParent(transform);
		textObj.transform.localPosition = new Vector3(0, 20, 0);

		var textMesh = textObj.AddComponent<TextMesh>();
		textMesh.font = Font.GetDefault();
		textMesh.text = gameObject.name;
		textMesh.characterSize = 0.07f;
		textMesh.fontSize = 30;
		textMesh.alignment = TextAlignment.Center;
		textMesh.anchor = TextAnchor.MiddleCenter;
		textMesh.color = Color.white;

		var outlineObj = new GameObject("outline");
		outlineObj.transform.SetParent(textObj.transform);
		outlineObj.transform.localPosition = new Vector3(0, 0, 0.05f);

		var outlineMesh = outlineObj.AddComponent<TextMesh>();
		outlineMesh.font = Font.GetDefault();
		outlineMesh.text = gameObject.name;
		outlineMesh.characterSize = 0.071f;
		outlineMesh.fontSize = 30;
		outlineMesh.alignment = TextAlignment.Center;
		outlineMesh.anchor = TextAnchor.MiddleCenter;
		outlineMesh.color = Color.black;

		textTransform = textObj.transform;
	}

	private void Update()
	{
		if (cam != null)
			textTransform.LookAt(textTransform.position + cam.forward);
	}
}