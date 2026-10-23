using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CarPhysicsPlayTests
{
    [UnityTest]
    public IEnumerator CarPrefab_Instantiates_AndFallsOntoGround_UnderGravity()
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.transform.localScale = new Vector3(10f, 1f, 10f);

        var prefab = Resources.Load<GameObject>("Car");
        Assert.That(prefab, Is.Not.Null, "Resources/Car.prefab must exist");

        var car = Object.Instantiate(prefab, new Vector3(0f, 5f, 0f), Quaternion.identity);
        Assert.That(car.GetComponent<Rigidbody>(), Is.Not.Null);
        Assert.That(car.GetComponent<CarDriver>(), Is.Not.Null);

        float startY = car.transform.position.y;

        for (int i = 0; i < 240; i++) yield return new WaitForFixedUpdate();

        float endY = car.transform.position.y;
        Assert.That(endY, Is.LessThan(startY),
            "Car should have fallen under gravity");
        Assert.That(endY, Is.LessThan(2f),
            "Car should have landed close to the ground, not still falling");

        Object.Destroy(car);
        Object.Destroy(ground);
    }
}
