using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayTracedSphere : RayTracedShape
{
	public float radius;

	public SphereScriptableObject sphereScriptableObject;

	protected new void Awake()
	{
		base.Awake();

		shapeClass = ShapeClass.Sphere;

		if (sphereScriptableObject != null) {
			radius = sphereScriptableObject.radius;
			material = sphereScriptableObject.material;
		}
	}

}
