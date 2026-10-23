using NUnit.Framework;
using UnityEngine;

public class CarConfigTests
{
    [Test]
    public void NewCarConfig_HasSensibleDefaults()
    {
        var cfg = ScriptableObject.CreateInstance<CarConfig>();

        Assert.That(cfg.motorForce, Is.GreaterThan(0f), "motorForce must be positive");
        Assert.That(cfg.steerTorque, Is.GreaterThan(0f), "steerTorque must be positive");
        Assert.That(cfg.maxSpeed, Is.GreaterThan(0f), "maxSpeed must be positive");
        Assert.That(cfg.groundLinearDamping, Is.GreaterThanOrEqualTo(0f));
        Assert.That(cfg.groundAngularDamping, Is.GreaterThanOrEqualTo(0f));
        Assert.That(cfg.centerOfMass.y, Is.LessThanOrEqualTo(0f),
            "centerOfMass should be at or below the car's origin for stability");

        Object.DestroyImmediate(cfg);
    }

    [Test]
    public void ProjectCarConfigAsset_LoadsAndIsSensible()
    {
        var cfg = UnityEditor.AssetDatabase.LoadAssetAtPath<CarConfig>(
            "Assets/Data/CarConfig.asset");

        Assert.That(cfg, Is.Not.Null, "CarConfig asset must exist at Assets/Data/CarConfig.asset");
        Assert.That(cfg.maxSpeed, Is.InRange(5f, 300f), "maxSpeed should be in a plausible m/s range");
    }
}
